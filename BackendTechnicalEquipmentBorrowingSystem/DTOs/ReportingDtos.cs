namespace BackendTechnicalEquipmentBorrowingSystem.DTOs;

public record StockSummary(
    int TotalItems,
    int Available,
    int Reserved,
    int Borrowed,
    int Maintenance,
    int Lost,
    int Retired,
    int ActiveBorrowings,
    int TotalUsers);

public record ImportResult(int Imported, List<string> Errors);
