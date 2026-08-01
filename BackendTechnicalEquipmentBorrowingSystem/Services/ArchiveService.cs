using BackendTechnicalEquipmentBorrowingSystem.Entities;
using BackendTechnicalEquipmentBorrowingSystem.IRepository;
using BackendTechnicalEquipmentBorrowingSystem.IService;

namespace BackendTechnicalEquipmentBorrowingSystem.Services;

// Snapshot a live record into its archive table, then delete it. One shared DbContext (scoped),
// so the add + remove commit together in a single SaveChanges.
// ponytail: archive only; restore lands when a controller needs it. Item/User with borrowing
// history must have those borrowings archived first (FK is Restrict) — add a cascade if the
// manual ordering becomes a chore.
public class ArchiveService : IArchiveService
{
    private readonly IRepository<Item> _items;
    private readonly IRepository<User> _users;
    private readonly IRepository<Borrowing> _borrowings;
    private readonly IRepository<ArchivedItem> _archivedItems;
    private readonly IRepository<ArchivedUser> _archivedUsers;
    private readonly IRepository<ArchivedBorrowing> _archivedBorrowings;

    public ArchiveService(
        IRepository<Item> items, IRepository<User> users, IRepository<Borrowing> borrowings,
        IRepository<ArchivedItem> archivedItems, IRepository<ArchivedUser> archivedUsers,
        IRepository<ArchivedBorrowing> archivedBorrowings)
    {
        _items = items;
        _users = users;
        _borrowings = borrowings;
        _archivedItems = archivedItems;
        _archivedUsers = archivedUsers;
        _archivedBorrowings = archivedBorrowings;
    }

    public async Task ArchiveItemAsync(int itemId, string? reason = null, int? archivedById = null)
    {
        var item = await _items.GetByIdAsync(itemId)
            ?? throw new KeyNotFoundException($"Item {itemId} not found.");
        if (item.Status is ItemStatus.Borrowed or ItemStatus.Reserved)
            throw new InvalidOperationException("Cannot archive an item that is borrowed or reserved.");
        if (await _borrowings.ExistsAsync(b => b.ItemId == itemId))
            throw new InvalidOperationException("Archive the item's borrowing records first.");

        await _archivedItems.AddAsync(new ArchivedItem
        {
            OriginalItemId = item.Id,
            Name = item.Name,
            SerialNumber = item.SerialNumber,
            RfidUid = item.RfidUid,
            Category = item.Category,
            LastStatus = item.Status,
            Reason = reason,
            ArchivedById = archivedById
        });
        _items.Remove(item);
        await _items.SaveChangesAsync();
    }

    public async Task ArchiveUserAsync(int userId, string? reason = null, int? archivedById = null)
    {
        var user = await _users.GetByIdAsync(userId)
            ?? throw new KeyNotFoundException($"User {userId} not found.");
        if (await _borrowings.ExistsAsync(b => b.BorrowerId == userId))
            throw new InvalidOperationException("Archive the user's borrowing records first.");

        await _archivedUsers.AddAsync(new ArchivedUser
        {
            OriginalUserId = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Role = user.Role,
            Reason = reason,
            ArchivedById = archivedById
        });
        _users.Remove(user); // TPT: removing the base User row cascades to the Student/Faculty subtype row.
        await _users.SaveChangesAsync();
    }

    public async Task ArchiveBorrowingAsync(int borrowingId, string? reason = null, int? archivedById = null)
    {
        var b = await _borrowings.GetByIdAsync(borrowingId)
            ?? throw new KeyNotFoundException($"Borrowing {borrowingId} not found.");
        if (b.Status is BorrowingStatus.Pending or BorrowingStatus.Approved
            or BorrowingStatus.Borrowed or BorrowingStatus.Overdue)
            throw new InvalidOperationException("Only settled borrowings can be archived, not active ones.");

        await _archivedBorrowings.AddAsync(new ArchivedBorrowing
        {
            OriginalBorrowingId = b.Id,
            ItemId = b.ItemId,
            BorrowerId = b.BorrowerId,
            LastStatus = b.Status,
            BorrowedAt = b.BorrowedAt,
            ReturnedAt = b.ReturnedAt,
            Reason = reason,
            ArchivedById = archivedById
        });
        _borrowings.Remove(b);
        await _borrowings.SaveChangesAsync();
    }
}
