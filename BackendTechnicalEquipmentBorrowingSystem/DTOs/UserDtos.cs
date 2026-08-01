using BackendTechnicalEquipmentBorrowingSystem.Entities;

namespace BackendTechnicalEquipmentBorrowingSystem.DTOs;

// Response shape for a user — never exposes PasswordHash or refresh-token state.
// ponytail: base fields only; add StudentDto/FacultyDto when an endpoint needs subtype fields.
public record UserDto(
    int Id,
    string FirstName,
    string LastName,
    string Email,
    Role Role,
    bool IsBlocked,
    bool IsActive,
    string? ImageUrl,
    DateTime CreatedAt);

public record UpdateProfileRequest(string FirstName, string LastName, string? ImageUrl = null);
