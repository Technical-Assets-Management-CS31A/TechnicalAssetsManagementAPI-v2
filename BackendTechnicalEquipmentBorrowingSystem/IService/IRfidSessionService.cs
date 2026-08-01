using BackendTechnicalEquipmentBorrowingSystem.Entities;

namespace BackendTechnicalEquipmentBorrowingSystem.IService;

public interface IRfidSessionService
{
    // Start (web-initiated) — station completes each by a scan/capture.
    Task<ItemRfidRegistration> StartItemRegistrationAsync(int itemId, int initiatedById, int expiryMinutes = 5);
    Task<StudentRfidRegistration> StartStudentRegistrationAsync(int studentId, int initiatedById, int expiryMinutes = 5);
    Task<HardwareBorrowSession> StartBorrowSessionAsync(int itemId, int? borrowerId, int expiryMinutes = 5);
    Task<HardwareReturnSession> StartReturnSessionAsync(int borrowingId, int expiryMinutes = 5);
    Task<GuestScanSession> StartGuestScanAsync(string scannedItemUid, string? guestName, int expiryMinutes = 5);

    // Complete (station-side scan/capture arrives).
    Task CompleteItemRegistrationAsync(int sessionId, string scannedUid);
    Task CompleteStudentRegistrationAsync(int sessionId, string scannedCardUid, string? faceImageUrl);
    Task<Borrowing> CompleteBorrowSessionAsync(int sessionId, string scannedCardUid, string scannedItemUid, string? faceImageUrl);
    Task<Borrowing> CompleteReturnSessionAsync(int sessionId, string scannedItemUid);

    /// <summary>Single sweep over every session table via the shared ISession contract; returns count expired.</summary>
    Task<int> ExpireStaleSessionsAsync();
}
