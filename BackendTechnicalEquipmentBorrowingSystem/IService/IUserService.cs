using BackendTechnicalEquipmentBorrowingSystem.Entities;

namespace BackendTechnicalEquipmentBorrowingSystem.IService;

public interface IUserService
{
    Task<List<User>> GetAllAsync();
    Task<User?> GetByIdAsync(int id);
    Task<User> UpdateProfileAsync(int id, string firstName, string lastName, string? imageUrl);
    Task SetBlockedAsync(int id, bool blocked);
}
