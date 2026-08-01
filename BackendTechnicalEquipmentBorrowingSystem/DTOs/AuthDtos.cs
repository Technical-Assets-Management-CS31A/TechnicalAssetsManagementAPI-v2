using BackendTechnicalEquipmentBorrowingSystem.Entities;

namespace BackendTechnicalEquipmentBorrowingSystem.DTOs;

public record RegisterRequest(
    string FirstName,
    string LastName,
    string Email,
    string Password,
    Role Role,
    // Student-only (ignored for other roles)
    string? StudentNumber = null,
    string? Course = null,
    string? Section = null,
    // Faculty-only
    FacultyPosition? Position = null,
    string? EmployeeNumber = null,
    string? Department = null);

public record LoginRequest(string Email, string Password);

public record RefreshRequest(string RefreshToken);

public record AuthResult(
    int UserId,
    string FullName,
    Role Role,
    string AccessToken,
    string RefreshToken,
    DateTime AccessTokenExpiresAt);
