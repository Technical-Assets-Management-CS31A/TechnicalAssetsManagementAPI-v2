using BackendTechnicalEquipmentBorrowingSystem.Entities;
using BackendTechnicalEquipmentBorrowingSystem.IRepository;
using BackendTechnicalEquipmentBorrowingSystem.IService;

namespace BackendTechnicalEquipmentBorrowingSystem.Services;

public class ActivityLogService : IActivityLogService
{
    private readonly IRepository<ActivityLog> _logs;
    public ActivityLogService(IRepository<ActivityLog> logs) => _logs = logs;

    public async Task LogAsync(string action, int? userId = null, string? entityType = null,
        string? entityId = null, string? details = null)
    {
        await _logs.AddAsync(new ActivityLog
        {
            Action = action,
            UserId = userId,
            EntityType = entityType,
            EntityId = entityId,
            Details = details,
            Timestamp = DateTime.UtcNow
        });
        await _logs.SaveChangesAsync();
    }
}
