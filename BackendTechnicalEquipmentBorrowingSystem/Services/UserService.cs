using BackendTechnicalEquipmentBorrowingSystem.Entities;
using BackendTechnicalEquipmentBorrowingSystem.IRepository;
using BackendTechnicalEquipmentBorrowingSystem.IService;

namespace BackendTechnicalEquipmentBorrowingSystem.Services;

public class UserService : IUserService
{
    private readonly IRepository<User> _users;
    public UserService(IRepository<User> users) => _users = users;

    public Task<List<User>> GetAllAsync() => _users.GetAllAsync();
    public Task<User?> GetByIdAsync(int id) => _users.GetByIdAsync(id);

    public async Task<User> UpdateProfileAsync(int id, string firstName, string lastName, string? imageUrl)
    {
        var user = await _users.GetByIdAsync(id) ?? throw new KeyNotFoundException($"User {id} not found.");
        user.FirstName = firstName;
        user.LastName = lastName;
        user.ImageUrl = imageUrl;
        user.UpdatedAt = DateTime.UtcNow;
        _users.Update(user);
        await _users.SaveChangesAsync();
        return user;
    }

    public async Task SetBlockedAsync(int id, bool blocked)
    {
        var user = await _users.GetByIdAsync(id) ?? throw new KeyNotFoundException($"User {id} not found.");
        user.IsBlocked = blocked;
        _users.Update(user);
        await _users.SaveChangesAsync();
    }
}
