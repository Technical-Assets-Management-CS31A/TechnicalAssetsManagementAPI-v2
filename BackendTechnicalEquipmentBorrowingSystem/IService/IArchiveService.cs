namespace BackendTechnicalEquipmentBorrowingSystem.IService;

// Archive-and-restore instead of hard deletes: snapshot a live record into its archive table, then drop it.
public interface IArchiveService
{
    Task ArchiveItemAsync(int itemId, string? reason = null, int? archivedById = null);
    Task ArchiveUserAsync(int userId, string? reason = null, int? archivedById = null);
    Task ArchiveBorrowingAsync(int borrowingId, string? reason = null, int? archivedById = null);
}
