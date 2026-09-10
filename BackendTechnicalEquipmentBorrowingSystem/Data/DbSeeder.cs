using BackendTechnicalEquipmentBorrowingSystem.Entities;
using Microsoft.EntityFrameworkCore;

namespace BackendTechnicalEquipmentBorrowingSystem.Data;

// ponytail: one-shot dev/demo data, not a fixture framework. Skips entirely if Users already has rows.
public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        if (await db.Users.AnyAsync()) return;

        string Hash(string pw) => BCrypt.Net.BCrypt.HashPassword(pw);

        var superAdmin = new User
        {
            FirstName = "Sam", LastName = "Root", Email = "superadmin@school.edu", Username = "superadmin",
            PasswordHash = Hash("Password123!"), Role = Role.SuperAdmin
        };
        var admin = new User
        {
            FirstName = "Ana", LastName = "Admin", Email = "admin@school.edu", Username = "admin",
            PasswordHash = Hash("Password123!"), Role = Role.Admin
        };
        var teacher = new Faculty
        {
            FirstName = "Tom", LastName = "Teacher", Email = "teacher@school.edu", Username = "teacher",
            PasswordHash = Hash("Password123!"), Role = Role.Faculty,
            Position = FacultyPosition.Teacher, EmployeeNumber = "F-0001", Department = "Computer Science"
        };
        var staff = new Faculty
        {
            FirstName = "Stacy", LastName = "Staff", Email = "staff@school.edu", Username = "staff",
            PasswordHash = Hash("Password123!"), Role = Role.Faculty,
            Position = FacultyPosition.StaffUtilities, EmployeeNumber = "F-0002", Department = "Facilities"
        };
        var student1 = new Student
        {
            FirstName = "Sean", LastName = "Student", Email = "student1@school.edu", Username = "student1",
            PasswordHash = Hash("Password123!"), Role = Role.Student,
            StudentNumber = "S-2026-001", Course = "BSIT", Section = "3A"
        };
        var student2 = new Student
        {
            FirstName = "Sofia", LastName = "Santos", Email = "student2@school.edu", Username = "student2",
            PasswordHash = Hash("Password123!"), Role = Role.Student,
            StudentNumber = "S-2026-002", Course = "BSCS", Section = "2B"
        };

        db.Users.AddRange(superAdmin, admin);
        db.Faculties.AddRange(teacher, staff);
        db.Students.AddRange(student1, student2);

        var items = new List<Item>
        {
            new() { Name = "HDMI Cable 2m", SerialNumber = "HDMI-0001", Category = "Cable", Condition = ItemCondition.Good, Status = ItemStatus.Available, Location = "Room 101" },
            new() { Name = "HDMI Cable 5m", SerialNumber = "HDMI-0002", Category = "Cable", Condition = ItemCondition.New, Status = ItemStatus.Available, Location = "Room 101" },
            new() { Name = "Extension Cord 3-outlet", SerialNumber = "EXT-0001", Category = "Extension", Condition = ItemCondition.Fair, Status = ItemStatus.Available, Location = "Storage" },
            new() { Name = "Classroom Key - Rm 204", SerialNumber = "KEY-0204", Category = "Key", Condition = ItemCondition.Good, Status = ItemStatus.Available, Location = "Faculty Office" },
            new() { Name = "AC Remote - Rm 305", SerialNumber = "ACR-0305", Category = "Remote", Condition = ItemCondition.Poor, Status = ItemStatus.Maintenance, Location = "Room 305" },
            new() { Name = "Projector Remote", SerialNumber = "PJR-0001", Category = "Remote", Condition = ItemCondition.Damaged, Status = ItemStatus.Retired, Location = "Storage" },
        };
        db.Items.AddRange(items);

        await db.SaveChangesAsync(); // need generated IDs before wiring a borrowing

        db.Borrowings.Add(new Borrowing
        {
            ItemId = items[0].Id,
            BorrowerId = student1.Id,
            Status = BorrowingStatus.Borrowed,
            ReservedAt = DateTime.UtcNow.AddDays(-1),
            ApprovedAt = DateTime.UtcNow.AddDays(-1),
            ApprovedById = admin.Id,
            BorrowedAt = DateTime.UtcNow.AddDays(-1),
            DueAt = DateTime.UtcNow.AddDays(2)
        });
        items[0].Status = ItemStatus.Borrowed;

        await db.SaveChangesAsync();
    }
}
