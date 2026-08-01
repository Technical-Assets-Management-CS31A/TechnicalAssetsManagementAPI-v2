namespace BackendTechnicalEquipmentBorrowingSystem.Entities;

// All statuses are enums stored as strings in the DB (see AppDbContext) — no raw string statuses.

/// <summary>Top-level account role. Admin/SuperAdmin are plain User rows; Student/Faculty are TPT subtypes.</summary>
public enum Role
{
    SuperAdmin,
    Admin,
    Student,
    Faculty
}

/// <summary>Faculty sub-role — a categorization tag so faculty variants aren't more subtypes.</summary>
public enum FacultyPosition
{
    Teacher,
    StaffUtilities,
    AdmissionStaff,
    Other
}

public enum ItemCondition
{
    New,
    Good,
    Fair,
    Poor,
    Damaged
}

public enum ItemStatus
{
    Available,
    Reserved,
    Borrowed,
    Maintenance,
    Lost,
    Retired
}

/// <summary>Lifecycle of a borrow/reservation record.</summary>
public enum BorrowingStatus
{
    Pending,    // reservation requested, awaiting approval
    Approved,   // approved, awaiting pickup
    Denied,     // approval rejected
    Borrowed,   // item is out
    Returned,   // completed
    Cancelled,  // cancelled before pickup
    Expired,    // unclaimed reservation auto-expired
    Overdue     // past due date, not yet returned
}

/// <summary>Shared lifecycle for every RFID/hardware session (see ISession). Expired is set by the expiry sweep.</summary>
public enum SessionStatus
{
    Pending,
    Completed,
    Failed,
    Cancelled,
    Expired
}
