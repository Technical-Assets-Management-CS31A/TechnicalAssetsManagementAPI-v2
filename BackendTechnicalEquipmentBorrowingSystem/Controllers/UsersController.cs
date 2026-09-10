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
public class UsersController(IUserService users, IMapper mapper) : ControllerBase
{
    [Authorize(Roles = "SuperAdmin,Admin")]
    [HttpGet]
    public async Task<ActionResult<List<UserDto>>> GetAll()
        => Ok(mapper.Map<List<UserDto>>(await users.GetAllAsync()));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<UserDto>> GetById(int id)
    {
        if (id != User.GetUserId() && !User.IsPrivileged()) return Forbid();
        var user = await users.GetByIdAsync(id);
        return user is null ? NotFound() : Ok(mapper.Map<UserDto>(user));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<UserDto>> UpdateProfile(int id, UpdateProfileRequest request)
    {
        if (id != User.GetUserId() && !User.IsPrivileged()) return Forbid();
        var user = await users.UpdateProfileAsync(id, request.FirstName, request.LastName, request.ImageUrl);
        return Ok(mapper.Map<UserDto>(user));
    }

    [Authorize(Roles = "SuperAdmin,Admin")]
    [HttpPatch("{id:int}/blocked")]
    public async Task<IActionResult> SetBlocked(int id, [FromQuery] bool blocked)
    {
        await users.SetBlockedAsync(id, blocked);
        return NoContent();
    }
}
