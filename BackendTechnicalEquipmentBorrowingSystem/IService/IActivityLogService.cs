namespace BackendTechnicalEquipmentBorrowingSystem.IService;

public interface IActivityLogService
{
    Task LogAsync(string action, int? userId = null, string? entityType = null,
        string? entityId = null, string? details = null);
}
