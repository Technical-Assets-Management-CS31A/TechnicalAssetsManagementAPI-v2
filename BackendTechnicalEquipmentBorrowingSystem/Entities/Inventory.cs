namespace BackendTechnicalEquipmentBorrowingSystem.Entities;

// A piece of equipment (HDMI cable, extension, key, AC remote, ...).
public class Item
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    public string SerialNumber { get; set; } = string.Empty;
    public string? RfidUid { get; set; } // optional RFID tag, bound via ItemRfidRegistration

    // ponytail: string for open-ended categories; promote to a table if a fixed list emerges.
    public string Category { get; set; } = string.Empty;
    public ItemCondition Condition { get; set; } = ItemCondition.Good;
    public ItemStatus Status { get; set; } = ItemStatus.Available;

    public string? Location { get; set; }
    public string? ImageUrl { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public ICollection<Borrowing> Borrowings { get; set; } = new List<Borrowing>();
}

// A borrow or reservation record for an item (renamed from LentItems).
public class Borrowing
{
    public int Id { get; set; }

    public int ItemId { get; set; }
    public Item Item { get; set; } = null!;

    public int BorrowerId { get; set; }
    public User Borrower { get; set; } = null!;

    public BorrowingStatus Status { get; set; } = BorrowingStatus.Pending;

    public DateTime? ReservedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public int? ApprovedById { get; set; } // admin who approved/denied

    public DateTime? BorrowedAt { get; set; }
    public DateTime? DueAt { get; set; }
    public DateTime? ReturnedAt { get; set; }

    public DateTime? ExpiresAt { get; set; } // unclaimed reservation auto-expiry
    public ItemCondition? ReturnCondition { get; set; }
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
