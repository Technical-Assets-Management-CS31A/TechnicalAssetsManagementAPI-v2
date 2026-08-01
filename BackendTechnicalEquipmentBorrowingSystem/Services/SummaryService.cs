using BackendTechnicalEquipmentBorrowingSystem.DTOs;
using BackendTechnicalEquipmentBorrowingSystem.Entities;
using BackendTechnicalEquipmentBorrowingSystem.IRepository;
using BackendTechnicalEquipmentBorrowingSystem.IService;
using Microsoft.EntityFrameworkCore;

namespace BackendTechnicalEquipmentBorrowingSystem.Services;

// Live inventory snapshot: item counts per status + currently-out borrows + user count.
public class SummaryService : ISummaryService
{
    private readonly IRepository<Item> _items;
    private readonly IRepository<Borrowing> _borrowings;
    private readonly IRepository<User> _users;

    public SummaryService(IRepository<Item> items, IRepository<Borrowing> borrowings, IRepository<User> users)
        => (_items, _borrowings, _users) = (items, borrowings, users);

    public async Task<StockSummary> GetStockSummaryAsync()
    {
        var byStatus = await _items.Query()
            .GroupBy(i => i.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Status, x => x.Count);

        int Count(ItemStatus s) => byStatus.GetValueOrDefault(s);

        var activeBorrowings = await _borrowings.Query()
            .CountAsync(b => b.Status == BorrowingStatus.Borrowed || b.Status == BorrowingStatus.Overdue);
        var totalUsers = await _users.Query().CountAsync();

        return new StockSummary(
            TotalItems: byStatus.Values.Sum(),
            Available: Count(ItemStatus.Available),
            Reserved: Count(ItemStatus.Reserved),
            Borrowed: Count(ItemStatus.Borrowed),
            Maintenance: Count(ItemStatus.Maintenance),
            Lost: Count(ItemStatus.Lost),
            Retired: Count(ItemStatus.Retired),
            ActiveBorrowings: activeBorrowings,
            TotalUsers: totalUsers);
    }
}
