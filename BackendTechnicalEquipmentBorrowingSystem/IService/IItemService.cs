using BackendTechnicalEquipmentBorrowingSystem.Entities;

namespace BackendTechnicalEquipmentBorrowingSystem.IService;

public interface IItemService
{
    Task<List<Item>> GetAllAsync();
    Task<Item?> GetByIdAsync(int id);
    Task<Item> CreateAsync(Item item);
    Task<Item> UpdateAsync(int id, Item updated);
    Task SetStatusAsync(int id, ItemStatus status);
    Task DeleteAsync(int id);
}
