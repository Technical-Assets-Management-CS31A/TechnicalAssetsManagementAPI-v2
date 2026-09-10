using BackendTechnicalEquipmentBorrowingSystem.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackendTechnicalEquipmentBorrowingSystem.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "SuperAdmin,Admin")]
public class ImportController(IImportService import) : ControllerBase
{
    [HttpPost("items")]
    public async Task<IActionResult> ImportItems(IFormFile file)
    {
        if (file.Length == 0) return BadRequest("File is empty.");
        await using var stream = file.OpenReadStream();
        return Ok(await import.ImportItemsAsync(stream));
    }
}
