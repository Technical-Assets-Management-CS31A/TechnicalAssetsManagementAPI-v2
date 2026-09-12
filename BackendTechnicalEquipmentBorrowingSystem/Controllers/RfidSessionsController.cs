using BackendTechnicalEquipmentBorrowingSystem.IService;
using BackendTechnicalEquipmentBorrowingSystem.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackendTechnicalEquipmentBorrowingSystem.Controllers;

// Web-initiated session starts require a user; station-side "complete" callbacks are the
// hardware/kiosk talking to the API. Device auth for those is a known gap (see README).
[ApiController]
[Route("api/v2/rfid-sessions")]
public class RfidSessionsController(IRfidSessionService sessions) : ControllerBase
{
    [Authorize(Roles = "SuperAdmin,Admin")]
    [HttpPost("items/{itemId:int}/register")]
    public async Task<IActionResult> StartItemRegistration(int itemId)
        => Ok(await sessions.StartItemRegistrationAsync(itemId, User.GetUserId()));

    [Authorize(Roles = "SuperAdmin,Admin")]
    [HttpPost("students/{studentId:int}/register")]
    public async Task<IActionResult> StartStudentRegistration(int studentId)
        => Ok(await sessions.StartStudentRegistrationAsync(studentId, User.GetUserId()));

    [Authorize]
    [HttpPost("borrow-sessions")]
    public async Task<IActionResult> StartBorrowSession([FromQuery] int itemId, [FromQuery] int? borrowerId)
        => Ok(await sessions.StartBorrowSessionAsync(itemId, borrowerId ?? User.GetUserId()));

    [Authorize(Roles = "SuperAdmin,Admin")]
    [HttpPost("return-sessions")]
    public async Task<IActionResult> StartReturnSession([FromQuery] int borrowingId)
        => Ok(await sessions.StartReturnSessionAsync(borrowingId));

    [HttpPost("guest-scans")]
    public async Task<IActionResult> StartGuestScan([FromQuery] string scannedItemUid, [FromQuery] string? guestName)
        => Ok(await sessions.StartGuestScanAsync(scannedItemUid, guestName));

    // --- Station-side completions ---

    [HttpPost("item-registrations/{sessionId:int}/complete")]
    public async Task<IActionResult> CompleteItemRegistration(int sessionId, [FromQuery] string scannedUid)
    {
        await sessions.CompleteItemRegistrationAsync(sessionId, scannedUid);
        return NoContent();
    }

    [HttpPost("student-registrations/{sessionId:int}/complete")]
    public async Task<IActionResult> CompleteStudentRegistration(
        int sessionId, [FromQuery] string scannedCardUid, [FromQuery] string? faceImageUrl)
    {
        await sessions.CompleteStudentRegistrationAsync(sessionId, scannedCardUid, faceImageUrl);
        return NoContent();
    }

    [HttpPost("borrow-sessions/{sessionId:int}/complete")]
    public async Task<IActionResult> CompleteBorrowSession(
        int sessionId, [FromQuery] string scannedCardUid, [FromQuery] string scannedItemUid,
        [FromQuery] string? faceImageUrl)
        => Ok(await sessions.CompleteBorrowSessionAsync(sessionId, scannedCardUid, scannedItemUid, faceImageUrl));

    [HttpPost("return-sessions/{sessionId:int}/complete")]
    public async Task<IActionResult> CompleteReturnSession(int sessionId, [FromQuery] string scannedItemUid)
        => Ok(await sessions.CompleteReturnSessionAsync(sessionId, scannedItemUid));
}
