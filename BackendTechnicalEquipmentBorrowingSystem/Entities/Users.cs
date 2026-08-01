namespace BackendTechnicalEquipmentBorrowingSystem.Entities;

// Single Users table. Admin/SuperAdmin are plain User rows (no subtype).
// Student and Faculty are EF Core TPT subtypes -> their own tables joined by Id.
public class User
{
    public int Id { get; set; }

    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;

    public Role Role { get; set; }

    public bool IsBlocked { get; set; }
    public bool IsActive { get; set; } = true;

    public string? ImageUrl { get; set; }

    // JWT refresh-token state (handled in AuthService, no middleware).
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiresAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public ICollection<Borrowing> Borrowings { get; set; } = new List<Borrowing>();
}

// TPT subtype: student-specific fields + enrolled RFID card / face reference.
public class Student : User
{
    public string StudentNumber { get; set; } = string.Empty;
    public string? Course { get; set; }
    public string? Section { get; set; }

    // Enrolled via StudentRfidRegistration; FaceImageUrl is the recognition reference (webcam capture).
    public string? RfidCardUid { get; set; }
    public string? FaceImageUrl { get; set; }
}

// TPT subtype: faculty-specific fields; Position categorizes the sub-role.
public class Faculty : User
{
    public FacultyPosition Position { get; set; }
    public string? EmployeeNumber { get; set; }
    public string? Department { get; set; }
}
