using BackendTechnicalEquipmentBorrowingSystem.Entities;
using BackendTechnicalEquipmentBorrowingSystem.IRepository;
using BackendTechnicalEquipmentBorrowingSystem.IService;

namespace BackendTechnicalEquipmentBorrowingSystem.Services;

public class BorrowingService : IBorrowingService
{
    private readonly IRepository<Borrowing> _borrowings;
    private readonly IRepository<Item> _items;
    private readonly IActivityLogService _log;

    public BorrowingService(IRepository<Borrowing> borrowings, IRepository<Item> items, IActivityLogService log)
    {
        _borrowings = borrowings;
        _items = items;
        _log = log;
    }

    public Task<List<Borrowing>> GetAllAsync() => _borrowings.GetAllAsync();
    public Task<Borrowing?> GetByIdAsync(int id) => _borrowings.GetByIdAsync(id);

    public async Task<Borrowing> ReserveAsync(int itemId, int borrowerId, int reservationMinutes = 30)
    {
        var item = await _items.GetByIdAsync(itemId) ?? throw new KeyNotFoundException($"Item {itemId} not found.");
        if (item.Status != ItemStatus.Available)
            throw new InvalidOperationException($"Item is not available (status: {item.Status}).");

        var now = DateTime.UtcNow;
        var borrowing = new Borrowing
        {
            ItemId = itemId,
            BorrowerId = borrowerId,
            Status = BorrowingStatus.Pending,
            ReservedAt = now,
            ExpiresAt = now.AddMinutes(reservationMinutes),
            CreatedAt = now
        };
        item.Status = ItemStatus.Reserved;
        _items.Update(item);
        await _borrowings.AddAsync(borrowing);
        await _borrowings.SaveChangesAsync();
        await _log.LogAsync("Reserve", borrowerId, nameof(Borrowing), borrowing.Id.ToString());
        return borrowing;
    }

    public Task<Borrowing> ApproveAsync(int borrowingId, int approverId) =>
        TransitionAsync(borrowingId, BorrowingStatus.Pending, BorrowingStatus.Approved, "Approve", approverId,
            (b, _) => { b.ApprovedAt = DateTime.UtcNow; b.ApprovedById = approverId; });

    public Task<Borrowing> DenyAsync(int borrowingId, int approverId) =>
        TransitionAsync(borrowingId, BorrowingStatus.Pending, BorrowingStatus.Denied, "Deny", approverId,
            (b, item) =>
            {
                b.ApprovedAt = DateTime.UtcNow;
                b.ApprovedById = approverId;
                if (item is not null && item.Status == ItemStatus.Reserved) item.Status = ItemStatus.Available;
            });

    public async Task<Borrowing> BorrowAsync(int borrowingId, DateTime dueAt)
    {
        var b = await _borrowings.GetByIdAsync(borrowingId) ?? throw new KeyNotFoundException($"Borrowing {borrowingId} not found.");
        if (b.Status != BorrowingStatus.Approved)
            throw new InvalidOperationException($"Borrowing must be Approved to be borrowed (current: {b.Status}).");
        var item = await _items.GetByIdAsync(b.ItemId);
        b.Status = BorrowingStatus.Borrowed;
        b.BorrowedAt = DateTime.UtcNow;
        b.DueAt = dueAt;
        if (item is not null) { item.Status = ItemStatus.Borrowed; _items.Update(item); }
        _borrowings.Update(b);
        await _borrowings.SaveChangesAsync();
        await _log.LogAsync("Borrow", b.BorrowerId, nameof(Borrowing), b.Id.ToString());
        return b;
    }

    public async Task<Borrowing> ReturnAsync(int borrowingId, ItemCondition? returnCondition = null)
    {
        var b = await _borrowings.GetByIdAsync(borrowingId) ?? throw new KeyNotFoundException($"Borrowing {borrowingId} not found.");
        if (b.Status is not (BorrowingStatus.Borrowed or BorrowingStatus.Overdue))
            throw new InvalidOperationException($"Only a borrowed item can be returned (current: {b.Status}).");
        var item = await _items.GetByIdAsync(b.ItemId);
        b.Status = BorrowingStatus.Returned;
        b.ReturnedAt = DateTime.UtcNow;
        b.ReturnCondition = returnCondition;
        if (item is not null)
        {
            item.Status = ItemStatus.Available;
            if (returnCondition is not null) item.Condition = returnCondition.Value;
            _items.Update(item);
        }
        _borrowings.Update(b);
        await _borrowings.SaveChangesAsync();
        await _log.LogAsync("Return", b.BorrowerId, nameof(Borrowing), b.Id.ToString());
        return b;
    }

    public async Task<Borrowing> CancelAsync(int borrowingId)
    {
        var b = await _borrowings.GetByIdAsync(borrowingId) ?? throw new KeyNotFoundException($"Borrowing {borrowingId} not found.");
        if (b.Status is not (BorrowingStatus.Pending or BorrowingStatus.Approved))
            throw new InvalidOperationException($"Only a pending/approved reservation can be cancelled (current: {b.Status}).");
        var item = await _items.GetByIdAsync(b.ItemId);
        b.Status = BorrowingStatus.Cancelled;
        if (item is not null && item.Status == ItemStatus.Reserved) { item.Status = ItemStatus.Available; _items.Update(item); }
        _borrowings.Update(b);
        await _borrowings.SaveChangesAsync();
        await _log.LogAsync("Cancel", b.BorrowerId, nameof(Borrowing), b.Id.ToString());
        return b;
    }

    public async Task<int> ExpireReservationsAsync()
    {
        var now = DateTime.UtcNow;
        var stale = await _borrowings.FindAsync(b =>
            (b.Status == BorrowingStatus.Pending || b.Status == BorrowingStatus.Approved)
            && b.ExpiresAt != null && b.ExpiresAt < now);
        foreach (var b in stale)
        {
            b.Status = BorrowingStatus.Expired;
            var item = await _items.GetByIdAsync(b.ItemId);
            if (item is not null && item.Status == ItemStatus.Reserved) { item.Status = ItemStatus.Available; _items.Update(item); }
            _borrowings.Update(b);
        }
        if (stale.Count > 0) await _borrowings.SaveChangesAsync();
        return stale.Count;
    }

    private async Task<Borrowing> TransitionAsync(int id, BorrowingStatus from, BorrowingStatus to,
        string action, int actorId, Action<Borrowing, Item?> mutate)
    {
        var b = await _borrowings.GetByIdAsync(id) ?? throw new KeyNotFoundException($"Borrowing {id} not found.");
        if (b.Status != from)
            throw new InvalidOperationException($"Borrowing must be {from} for {action} (current: {b.Status}).");
        var item = await _items.GetByIdAsync(b.ItemId);
        b.Status = to;
        mutate(b, item);
        if (item is not null) _items.Update(item);
        _borrowings.Update(b);
        await _borrowings.SaveChangesAsync();
        await _log.LogAsync(action, actorId, nameof(Borrowing), b.Id.ToString());
        return b;
    }
}
