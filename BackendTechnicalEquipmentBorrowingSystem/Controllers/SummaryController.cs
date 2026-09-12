using BackendTechnicalEquipmentBorrowingSystem.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackendTechnicalEquipmentBorrowingSystem.Controllers;

[ApiController]
[Route("api/v2/summaries")]
[Authorize(Roles = "SuperAdmin,Admin")]
public class SummaryController(ISummaryService summary) : ControllerBase
{
    [HttpGet("stock")]
    public async Task<IActionResult> GetStock() => Ok(await summary.GetStockSummaryAsync());
}
