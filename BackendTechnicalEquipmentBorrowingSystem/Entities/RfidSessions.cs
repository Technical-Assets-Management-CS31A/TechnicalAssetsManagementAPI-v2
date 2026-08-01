namespace BackendTechnicalEquipmentBorrowingSystem.Entities;

// Shared lifecycle for every RFID/hardware session. One expiry sweep handles all implementers
// (see RfidSessionService): Pending + ExpiresAt < now -> Expired.
public interface ISession
{
    SessionStatus Status { get; set; }
    DateTime ExpiresAt { get; set; }
}

// Bind an RFID tag to an item (web-initiated, completed by a scan at the station).
public class ItemRfidRegistration : ISession
{
    public int Id { get; set; }
    public int ItemId { get; set; }
    public Item Item { get; set; } = null!;
    public string? ScannedUid { get; set; }
    public int InitiatedById { get; set; }
    public SessionStatus Status { get; set; } = SessionStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}

// Enroll a student's RFID card (+ reference face image captured at the webcam).
public class StudentRfidRegistration : ISession
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public Student Student { get; set; } = null!;
    public string? ScannedCardUid { get; set; }
    public string? FaceImageUrl { get; set; }
    public int InitiatedById { get; set; }
    public SessionStatus Status { get; set; } = SessionStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}

// Web-initiated borrow, completed at the station by the three-input capture (face + card + item).
public class HardwareBorrowSession : ISession
{
    public int Id { get; set; }
    public int ItemId { get; set; }
    public Item Item { get; set; } = null!;
    public int? BorrowerId { get; set; }
    public string? ScannedCardUid { get; set; }
    public string? ScannedItemUid { get; set; }
    public string? FaceImageUrl { get; set; }
    public int? ResultBorrowingId { get; set; } // Borrowing created on success
    public SessionStatus Status { get; set; } = SessionStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}

// Web-initiated return, completed by a scan at the station.
public class HardwareReturnSession : ISession
{
    public int Id { get; set; }
    public int BorrowingId { get; set; }
    public Borrowing Borrowing { get; set; } = null!;
    public string? ScannedItemUid { get; set; }
    public SessionStatus Status { get; set; } = SessionStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}

// Guest borrowing driven by an item scan (no card/face).
public class GuestScanSession : ISession
{
    public int Id { get; set; }
    public string? ScannedItemUid { get; set; }
    public int? ItemId { get; set; }
    public string? GuestName { get; set; }
    public SessionStatus Status { get; set; } = SessionStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}

// A physical RFID tag record (not a session).
public class RfidTag
{
    public int Id { get; set; }
    public string Uid { get; set; } = string.Empty;
    public int? ItemId { get; set; }
    public int? UserId { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
