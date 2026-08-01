namespace BackendTechnicalEquipmentBorrowingSystem.Entities;

// Audit trail. Action is free-form text (audit actions proliferate); status fields elsewhere stay enums.
public class ActivityLog
{
    public int Id { get; set; }
    public int? UserId { get; set; } // null for system actions
    public User? User { get; set; }

    public string Action { get; set; } = string.Empty;
    public string? EntityType { get; set; }
    public string? EntityId { get; set; }
    public string? Details { get; set; }

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

// Archive tables: flat snapshots moved out of the live tables to keep them lean as data grows.
public class ArchivedUser
{
    public int Id { get; set; }
    public int OriginalUserId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public Role Role { get; set; }
    public string? Reason { get; set; }
    public int? ArchivedById { get; set; }
    public DateTime ArchivedAt { get; set; } = DateTime.UtcNow;
}

public class ArchivedItem
{
    public int Id { get; set; }
    public int OriginalItemId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string SerialNumber { get; set; } = string.Empty;
    public string? RfidUid { get; set; }
    public string? Category { get; set; }
    public ItemStatus LastStatus { get; set; }
    public string? Reason { get; set; }
    public int? ArchivedById { get; set; }
    public DateTime ArchivedAt { get; set; } = DateTime.UtcNow;
}

public class ArchivedBorrowing
{
    public int Id { get; set; }
    public int OriginalBorrowingId { get; set; }
    public int ItemId { get; set; }
    public int BorrowerId { get; set; }
    public BorrowingStatus LastStatus { get; set; }
    public DateTime? BorrowedAt { get; set; }
    public DateTime? ReturnedAt { get; set; }
    public string? Reason { get; set; }
    public int? ArchivedById { get; set; }
    public DateTime ArchivedAt { get; set; } = DateTime.UtcNow;
}
