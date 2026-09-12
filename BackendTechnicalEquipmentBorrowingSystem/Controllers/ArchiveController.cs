using BackendTechnicalEquipmentBorrowingSystem.IService;
using BackendTechnicalEquipmentBorrowingSystem.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackendTechnicalEquipmentBorrowingSystem.Controllers;

[ApiController]
[Route("api/v2/archives")]
[Authorize(Roles = "SuperAdmin,Admin")]
public class ArchiveController(IArchiveService archive) : ControllerBase
{
    [HttpPost("items/{itemId:int}")]
    public async Task<IActionResult> ArchiveItem(int itemId, [FromQuery] string? reason)
    {
        await archive.ArchiveItemAsync(itemId, reason, User.GetUserId());
        return NoContent();
    }

    [HttpPost("users/{userId:int}")]
    public async Task<IActionResult> ArchiveUser(int userId, [FromQuery] string? reason)
    {
        await archive.ArchiveUserAsync(userId, reason, User.GetUserId());
        return NoContent();
    }

    [HttpPost("borrowings/{borrowingId:int}")]
    public async Task<IActionResult> ArchiveBorrowing(int borrowingId, [FromQuery] string? reason)
    {
        await archive.ArchiveBorrowingAsync(borrowingId, reason, User.GetUserId());
        return NoContent();
    }
}
