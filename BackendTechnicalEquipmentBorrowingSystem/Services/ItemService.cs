using BackendTechnicalEquipmentBorrowingSystem.Entities;
using BackendTechnicalEquipmentBorrowingSystem.IRepository;
using BackendTechnicalEquipmentBorrowingSystem.IService;

namespace BackendTechnicalEquipmentBorrowingSystem.Services;

public class ItemService : IItemService
{
    private readonly IRepository<Item> _items;
    public ItemService(IRepository<Item> items) => _items = items;

    public Task<List<Item>> GetAllAsync() => _items.GetAllAsync();
    public Task<Item?> GetByIdAsync(int id) => _items.GetByIdAsync(id);

    public async Task<Item> CreateAsync(Item item)
    {
        if (await _items.ExistsAsync(i => i.SerialNumber == item.SerialNumber))
            throw new InvalidOperationException($"Serial number '{item.SerialNumber}' already exists.");
        item.CreatedAt = DateTime.UtcNow;
        await _items.AddAsync(item);
        await _items.SaveChangesAsync();
        return item;
    }

    public async Task<Item> UpdateAsync(int id, Item updated)
    {
        var item = await _items.GetByIdAsync(id) ?? throw new KeyNotFoundException($"Item {id} not found.");
        item.Name = updated.Name;
        item.Description = updated.Description;
        item.Category = updated.Category;
        item.Condition = updated.Condition;
        item.Location = updated.Location;
        item.ImageUrl = updated.ImageUrl;
        item.RfidUid = updated.RfidUid;
        item.UpdatedAt = DateTime.UtcNow;
        _items.Update(item);
        await _items.SaveChangesAsync();
        return item;
    }

    public async Task SetStatusAsync(int id, ItemStatus status)
    {
        var item = await _items.GetByIdAsync(id) ?? throw new KeyNotFoundException($"Item {id} not found.");
        item.Status = status;
        item.UpdatedAt = DateTime.UtcNow;
        _items.Update(item);
        await _items.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var item = await _items.GetByIdAsync(id) ?? throw new KeyNotFoundException($"Item {id} not found.");
        if (item.Status is ItemStatus.Borrowed or ItemStatus.Reserved)
            throw new InvalidOperationException("Cannot delete an item that is borrowed or reserved.");
        _items.Remove(item);
        await _items.SaveChangesAsync();
    }
}
