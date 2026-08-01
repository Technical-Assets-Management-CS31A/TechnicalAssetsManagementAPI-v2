using BackendTechnicalEquipmentBorrowingSystem.Entities;

namespace BackendTechnicalEquipmentBorrowingSystem.IService;

public interface IBorrowingService
{
    Task<List<Borrowing>> GetAllAsync();
    Task<Borrowing?> GetByIdAsync(int id);

    Task<Borrowing> ReserveAsync(int itemId, int borrowerId, int reservationMinutes = 30);
    Task<Borrowing> ApproveAsync(int borrowingId, int approverId);
    Task<Borrowing> DenyAsync(int borrowingId, int approverId);
    Task<Borrowing> BorrowAsync(int borrowingId, DateTime dueAt);
    Task<Borrowing> ReturnAsync(int borrowingId, ItemCondition? returnCondition = null);
    Task<Borrowing> CancelAsync(int borrowingId);

    /// <summary>Auto-expires unclaimed reservations past their ExpiresAt; returns how many expired.</summary>
    Task<int> ExpireReservationsAsync();
}
