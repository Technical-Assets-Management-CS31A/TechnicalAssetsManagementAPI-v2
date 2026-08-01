using BackendTechnicalEquipmentBorrowingSystem.Entities;
using BackendTechnicalEquipmentBorrowingSystem.IRepository;
using BackendTechnicalEquipmentBorrowingSystem.IService;
// Disambiguate from Microsoft.AspNetCore.Http.ISession (Web SDK implicit using).
using ISession = BackendTechnicalEquipmentBorrowingSystem.Entities.ISession;

namespace BackendTechnicalEquipmentBorrowingSystem.Services;

public class RfidSessionService : IRfidSessionService
{
    private readonly IRepository<ItemRfidRegistration> _itemRegs;
    private readonly IRepository<StudentRfidRegistration> _studentRegs;
    private readonly IRepository<HardwareBorrowSession> _borrowSessions;
    private readonly IRepository<HardwareReturnSession> _returnSessions;
    private readonly IRepository<GuestScanSession> _guestSessions;
    private readonly IRepository<Item> _items;
    private readonly IRepository<Student> _students;
    private readonly IRepository<Borrowing> _borrowings;
    private readonly IActivityLogService _log;

    public RfidSessionService(
        IRepository<ItemRfidRegistration> itemRegs,
        IRepository<StudentRfidRegistration> studentRegs,
        IRepository<HardwareBorrowSession> borrowSessions,
        IRepository<HardwareReturnSession> returnSessions,
        IRepository<GuestScanSession> guestSessions,
        IRepository<Item> items,
        IRepository<Student> students,
        IRepository<Borrowing> borrowings,
        IActivityLogService log)
    {
        _itemRegs = itemRegs;
        _studentRegs = studentRegs;
        _borrowSessions = borrowSessions;
        _returnSessions = returnSessions;
        _guestSessions = guestSessions;
        _items = items;
        _students = students;
        _borrowings = borrowings;
        _log = log;
    }

    private static DateTime Expiry(int minutes) => DateTime.UtcNow.AddMinutes(minutes);

    // ---- Start ----

    public async Task<ItemRfidRegistration> StartItemRegistrationAsync(int itemId, int initiatedById, int expiryMinutes = 5)
    {
        _ = await _items.GetByIdAsync(itemId) ?? throw new KeyNotFoundException($"Item {itemId} not found.");
        var s = new ItemRfidRegistration { ItemId = itemId, InitiatedById = initiatedById, ExpiresAt = Expiry(expiryMinutes) };
        await _itemRegs.AddAsync(s);
        await _itemRegs.SaveChangesAsync();
        return s;
    }

    public async Task<StudentRfidRegistration> StartStudentRegistrationAsync(int studentId, int initiatedById, int expiryMinutes = 5)
    {
        _ = await _students.GetByIdAsync(studentId) ?? throw new KeyNotFoundException($"Student {studentId} not found.");
        var s = new StudentRfidRegistration { StudentId = studentId, InitiatedById = initiatedById, ExpiresAt = Expiry(expiryMinutes) };
        await _studentRegs.AddAsync(s);
        await _studentRegs.SaveChangesAsync();
        return s;
    }

    public async Task<HardwareBorrowSession> StartBorrowSessionAsync(int itemId, int? borrowerId, int expiryMinutes = 5)
    {
        _ = await _items.GetByIdAsync(itemId) ?? throw new KeyNotFoundException($"Item {itemId} not found.");
        var s = new HardwareBorrowSession { ItemId = itemId, BorrowerId = borrowerId, ExpiresAt = Expiry(expiryMinutes) };
        await _borrowSessions.AddAsync(s);
        await _borrowSessions.SaveChangesAsync();
        return s;
    }

    public async Task<HardwareReturnSession> StartReturnSessionAsync(int borrowingId, int expiryMinutes = 5)
    {
        _ = await _borrowings.GetByIdAsync(borrowingId) ?? throw new KeyNotFoundException($"Borrowing {borrowingId} not found.");
        var s = new HardwareReturnSession { BorrowingId = borrowingId, ExpiresAt = Expiry(expiryMinutes) };
        await _returnSessions.AddAsync(s);
        await _returnSessions.SaveChangesAsync();
        return s;
    }

    public async Task<GuestScanSession> StartGuestScanAsync(string scannedItemUid, string? guestName, int expiryMinutes = 5)
    {
        var item = await _items.FirstOrDefaultAsync(i => i.RfidUid == scannedItemUid);
        var s = new GuestScanSession
        {
            ScannedItemUid = scannedItemUid,
            ItemId = item?.Id,
            GuestName = guestName,
            ExpiresAt = Expiry(expiryMinutes)
        };
        await _guestSessions.AddAsync(s);
        await _guestSessions.SaveChangesAsync();
        return s;
    }

    // ---- Complete ----

    public async Task CompleteItemRegistrationAsync(int sessionId, string scannedUid)
    {
        var s = await _itemRegs.GetByIdAsync(sessionId) ?? throw new KeyNotFoundException($"Session {sessionId} not found.");
        EnsurePending(s);
        var item = await _items.GetByIdAsync(s.ItemId) ?? throw new KeyNotFoundException($"Item {s.ItemId} not found.");
        item.RfidUid = scannedUid;
        s.ScannedUid = scannedUid;
        s.Status = SessionStatus.Completed;
        s.CompletedAt = DateTime.UtcNow;
        _items.Update(item);
        _itemRegs.Update(s);
        await _itemRegs.SaveChangesAsync();
        await _log.LogAsync("ItemRfidRegistered", s.InitiatedById, nameof(Item), item.Id.ToString(), scannedUid);
    }

    public async Task CompleteStudentRegistrationAsync(int sessionId, string scannedCardUid, string? faceImageUrl)
    {
        var s = await _studentRegs.GetByIdAsync(sessionId) ?? throw new KeyNotFoundException($"Session {sessionId} not found.");
        EnsurePending(s);
        var student = await _students.GetByIdAsync(s.StudentId) ?? throw new KeyNotFoundException($"Student {s.StudentId} not found.");
        student.RfidCardUid = scannedCardUid;
        if (faceImageUrl is not null) student.FaceImageUrl = faceImageUrl;
        s.ScannedCardUid = scannedCardUid;
        s.FaceImageUrl = faceImageUrl;
        s.Status = SessionStatus.Completed;
        s.CompletedAt = DateTime.UtcNow;
        _students.Update(student);
        _studentRegs.Update(s);
        await _studentRegs.SaveChangesAsync();
        await _log.LogAsync("StudentRfidRegistered", s.InitiatedById, nameof(Student), student.Id.ToString());
    }

    public async Task<Borrowing> CompleteBorrowSessionAsync(int sessionId, string scannedCardUid, string scannedItemUid, string? faceImageUrl)
    {
        var s = await _borrowSessions.GetByIdAsync(sessionId) ?? throw new KeyNotFoundException($"Session {sessionId} not found.");
        EnsurePending(s);
        var item = await _items.GetByIdAsync(s.ItemId) ?? throw new KeyNotFoundException($"Item {s.ItemId} not found.");
        if (item.RfidUid is not null && item.RfidUid != scannedItemUid)
            throw new InvalidOperationException("Scanned item tag does not match the session item.");

        var borrowerId = s.BorrowerId
            ?? (await _students.FirstOrDefaultAsync(st => st.RfidCardUid == scannedCardUid))?.Id
            ?? throw new InvalidOperationException("No student matches the scanned card.");

        var now = DateTime.UtcNow;
        var borrowing = new Borrowing
        {
            ItemId = item.Id,
            BorrowerId = borrowerId,
            Status = BorrowingStatus.Borrowed,
            BorrowedAt = now,
            CreatedAt = now
        };
        item.Status = ItemStatus.Borrowed;

        s.ScannedCardUid = scannedCardUid;
        s.ScannedItemUid = scannedItemUid;
        s.FaceImageUrl = faceImageUrl;
        s.BorrowerId = borrowerId;
        s.Status = SessionStatus.Completed;
        s.CompletedAt = now;

        await _borrowings.AddAsync(borrowing);
        _items.Update(item);
        await _borrowings.SaveChangesAsync(); // assigns borrowing.Id
        s.ResultBorrowingId = borrowing.Id;
        _borrowSessions.Update(s);
        await _borrowSessions.SaveChangesAsync();
        await _log.LogAsync("HardwareBorrow", borrowerId, nameof(Borrowing), borrowing.Id.ToString());
        return borrowing;
    }

    public async Task<Borrowing> CompleteReturnSessionAsync(int sessionId, string scannedItemUid)
    {
        var s = await _returnSessions.GetByIdAsync(sessionId) ?? throw new KeyNotFoundException($"Session {sessionId} not found.");
        EnsurePending(s);
        var borrowing = await _borrowings.GetByIdAsync(s.BorrowingId) ?? throw new KeyNotFoundException($"Borrowing {s.BorrowingId} not found.");
        if (borrowing.Status is not (BorrowingStatus.Borrowed or BorrowingStatus.Overdue))
            throw new InvalidOperationException($"Borrowing is not returnable (status: {borrowing.Status}).");
        var item = await _items.GetByIdAsync(borrowing.ItemId);

        borrowing.Status = BorrowingStatus.Returned;
        borrowing.ReturnedAt = DateTime.UtcNow;
        if (item is not null) { item.Status = ItemStatus.Available; _items.Update(item); }

        s.ScannedItemUid = scannedItemUid;
        s.Status = SessionStatus.Completed;
        s.CompletedAt = DateTime.UtcNow;

        _borrowings.Update(borrowing);
        _returnSessions.Update(s);
        await _returnSessions.SaveChangesAsync();
        await _log.LogAsync("HardwareReturn", borrowing.BorrowerId, nameof(Borrowing), borrowing.Id.ToString());
        return borrowing;
    }

    // One sweep over every session table via the shared ISession contract.
    public async Task<int> ExpireStaleSessionsAsync()
    {
        var count = 0;
        count += await ExpireAsync(_itemRegs);
        count += await ExpireAsync(_studentRegs);
        count += await ExpireAsync(_borrowSessions);
        count += await ExpireAsync(_returnSessions);
        count += await ExpireAsync(_guestSessions);
        return count;
    }

    // ponytail: loads each session table then filters in memory. Session tables stay small
    // (short-lived, regularly expired). If they ever grow, push this predicate into SQL per type.
    private static async Task<int> ExpireAsync<T>(IRepository<T> repo) where T : class, ISession
    {
        var now = DateTime.UtcNow;
        var stale = (await repo.GetAllAsync())
            .Where(s => s.Status == SessionStatus.Pending && s.ExpiresAt < now)
            .ToList();
        foreach (var s in stale) { s.Status = SessionStatus.Expired; repo.Update(s); }
        if (stale.Count > 0) await repo.SaveChangesAsync();
        return stale.Count;
    }

    private static void EnsurePending(ISession s)
    {
        if (s.Status != SessionStatus.Pending)
            throw new InvalidOperationException($"Session is not pending (status: {s.Status}).");
    }
}
