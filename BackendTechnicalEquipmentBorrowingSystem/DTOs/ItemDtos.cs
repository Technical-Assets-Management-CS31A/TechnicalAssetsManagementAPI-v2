using BackendTechnicalEquipmentBorrowingSystem.Entities;

namespace BackendTechnicalEquipmentBorrowingSystem.DTOs;

public record ItemDto(
    int Id,
    string Name,
    string? Description,
    string SerialNumber,
    string? RfidUid,
    string Category,
    ItemCondition Condition,
    ItemStatus Status,
    string? Location,
    string? ImageUrl,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public record CreateItemRequest(
    string Name,
    string SerialNumber,
    string Category,
    string? Description = null,
    string? RfidUid = null,
    ItemCondition Condition = ItemCondition.Good,
    string? Location = null,
    string? ImageUrl = null);

public record UpdateItemRequest(
    string Name,
    string Category,
    ItemCondition Condition,
    string? Description = null,
    string? RfidUid = null,
    string? Location = null,
    string? ImageUrl = null);
