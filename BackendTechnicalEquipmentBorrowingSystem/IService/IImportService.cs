using BackendTechnicalEquipmentBorrowingSystem.DTOs;

namespace BackendTechnicalEquipmentBorrowingSystem.IService;

public interface IImportService
{
    // Ingest items from an .xlsx stream. Good rows are committed; bad rows come back as errors.
    Task<ImportResult> ImportItemsAsync(Stream xlsx);
}
