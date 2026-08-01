using BackendTechnicalEquipmentBorrowingSystem.Entities;

namespace BackendTechnicalEquipmentBorrowingSystem.DTOs;

public record BorrowingDto(
    int Id,
    int ItemId,
    string ItemName,
    int BorrowerId,
    string BorrowerName,
    BorrowingStatus Status,
    DateTime? ReservedAt,
    DateTime? ApprovedAt,
    DateTime? BorrowedAt,
    DateTime? DueAt,
    DateTime? ReturnedAt,
    DateTime? ExpiresAt,
    ItemCondition? ReturnCondition,
    string? Notes,
    DateTime CreatedAt);

// BorrowerId comes from the authenticated user, not the request body.
public record ReserveRequest(int ItemId, int? ReservationMinutes = null);
public record BorrowRequest(DateTime DueAt);
public record ReturnRequest(ItemCondition? ReturnCondition = null);
