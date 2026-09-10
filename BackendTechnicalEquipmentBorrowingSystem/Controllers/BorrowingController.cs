using AutoMapper;
using BackendTechnicalEquipmentBorrowingSystem.DTOs;
using BackendTechnicalEquipmentBorrowingSystem.IService;
using BackendTechnicalEquipmentBorrowingSystem.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackendTechnicalEquipmentBorrowingSystem.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BorrowingController(IBorrowingService borrowing, IMapper mapper) : ControllerBase
{
    [Authorize(Roles = "SuperAdmin,Admin")]
    [HttpGet]
    public async Task<ActionResult<List<BorrowingDto>>> GetAll()
        => Ok(mapper.Map<List<BorrowingDto>>(await borrowing.GetAllAsync()));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<BorrowingDto>> GetById(int id)
    {
        var b = await borrowing.GetByIdAsync(id);
        if (b is null) return NotFound();
        if (b.BorrowerId != User.GetUserId() && !User.IsPrivileged()) return Forbid();
        return Ok(mapper.Map<BorrowingDto>(b));
    }

    [HttpPost("reserve")]
    public async Task<ActionResult<BorrowingDto>> Reserve(ReserveRequest request)
    {
        var b = await borrowing.ReserveAsync(request.ItemId, User.GetUserId(),
            request.ReservationMinutes ?? 30);
        return Ok(mapper.Map<BorrowingDto>(await borrowing.GetByIdAsync(b.Id)));
    }

    [Authorize(Roles = "SuperAdmin,Admin")]
    [HttpPost("{id:int}/approve")]
    public async Task<ActionResult<BorrowingDto>> Approve(int id)
    {
        var b = await borrowing.ApproveAsync(id, User.GetUserId());
        return Ok(mapper.Map<BorrowingDto>(await borrowing.GetByIdAsync(b.Id)));
    }

    [Authorize(Roles = "SuperAdmin,Admin")]
    [HttpPost("{id:int}/deny")]
    public async Task<ActionResult<BorrowingDto>> Deny(int id)
    {
        var b = await borrowing.DenyAsync(id, User.GetUserId());
        return Ok(mapper.Map<BorrowingDto>(await borrowing.GetByIdAsync(b.Id)));
    }

    [Authorize(Roles = "SuperAdmin,Admin")]
    [HttpPost("{id:int}/borrow")]
    public async Task<ActionResult<BorrowingDto>> Borrow(int id, BorrowRequest request)
    {
        var b = await borrowing.BorrowAsync(id, request.DueAt);
        return Ok(mapper.Map<BorrowingDto>(await borrowing.GetByIdAsync(b.Id)));
    }

    [Authorize(Roles = "SuperAdmin,Admin")]
    [HttpPost("{id:int}/return")]
    public async Task<ActionResult<BorrowingDto>> Return(int id, ReturnRequest request)
    {
        var b = await borrowing.ReturnAsync(id, request.ReturnCondition);
        return Ok(mapper.Map<BorrowingDto>(await borrowing.GetByIdAsync(b.Id)));
    }

    [HttpPost("{id:int}/cancel")]
    public async Task<ActionResult<BorrowingDto>> Cancel(int id)
    {
        var existing = await borrowing.GetByIdAsync(id);
        if (existing is null) return NotFound();
        if (existing.BorrowerId != User.GetUserId() && !User.IsPrivileged()) return Forbid();

        var b = await borrowing.CancelAsync(id);
        return Ok(mapper.Map<BorrowingDto>(await borrowing.GetByIdAsync(b.Id)));
    }
}
