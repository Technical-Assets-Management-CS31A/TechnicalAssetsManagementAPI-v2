using AutoMapper;
using BackendTechnicalEquipmentBorrowingSystem.DTOs;
using BackendTechnicalEquipmentBorrowingSystem.Entities;
using BackendTechnicalEquipmentBorrowingSystem.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackendTechnicalEquipmentBorrowingSystem.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ItemsController(IItemService items, IMapper mapper) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<ItemDto>>> GetAll()
        => Ok(mapper.Map<List<ItemDto>>(await items.GetAllAsync()));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ItemDto>> GetById(int id)
    {
        var item = await items.GetByIdAsync(id);
        return item is null ? NotFound() : Ok(mapper.Map<ItemDto>(item));
    }

    [Authorize(Roles = "SuperAdmin,Admin")]
    [HttpPost]
    public async Task<ActionResult<ItemDto>> Create(CreateItemRequest request)
    {
        var item = await items.CreateAsync(new Item
        {
            Name = request.Name,
            SerialNumber = request.SerialNumber,
            Category = request.Category,
            Description = request.Description,
            RfidUid = request.RfidUid,
            Condition = request.Condition,
            Location = request.Location,
            ImageUrl = request.ImageUrl
        });
        var dto = mapper.Map<ItemDto>(item);
        return CreatedAtAction(nameof(GetById), new { id = item.Id }, dto);
    }

    [Authorize(Roles = "SuperAdmin,Admin")]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<ItemDto>> Update(int id, UpdateItemRequest request)
    {
        var item = await items.UpdateAsync(id, new Item
        {
            Name = request.Name,
            Category = request.Category,
            Condition = request.Condition,
            Description = request.Description,
            RfidUid = request.RfidUid,
            Location = request.Location,
            ImageUrl = request.ImageUrl
        });
        return Ok(mapper.Map<ItemDto>(item));
    }

    [Authorize(Roles = "SuperAdmin,Admin")]
    [HttpPatch("{id:int}/status")]
    public async Task<IActionResult> SetStatus(int id, [FromQuery] ItemStatus status)
    {
        await items.SetStatusAsync(id, status);
        return NoContent();
    }

    [Authorize(Roles = "SuperAdmin,Admin")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await items.DeleteAsync(id);
        return NoContent();
    }
}
