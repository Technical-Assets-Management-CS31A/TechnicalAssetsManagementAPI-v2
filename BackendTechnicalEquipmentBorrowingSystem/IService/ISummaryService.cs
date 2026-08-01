using BackendTechnicalEquipmentBorrowingSystem.DTOs;

namespace BackendTechnicalEquipmentBorrowingSystem.IService;

public interface ISummaryService
{
    Task<StockSummary> GetStockSummaryAsync();
}
