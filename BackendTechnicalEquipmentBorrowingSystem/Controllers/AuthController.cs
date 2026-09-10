using BackendTechnicalEquipmentBorrowingSystem.DTOs;
using BackendTechnicalEquipmentBorrowingSystem.IService;
using BackendTechnicalEquipmentBorrowingSystem.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackendTechnicalEquipmentBorrowingSystem.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IAuthService auth) : ControllerBase
{
    [HttpPost("register")]
    public async Task<ActionResult<AuthResult>> Register(RegisterRequest request)
        => Ok(await auth.RegisterAsync(request));

    [HttpPost("login")]
    public async Task<ActionResult<AuthResult>> Login(LoginRequest request)
        => Ok(await auth.LoginAsync(request));

    [HttpPost("refresh")]
    public async Task<ActionResult<AuthResult>> Refresh(RefreshRequest request)
        => Ok(await auth.RefreshAsync(request.RefreshToken));

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await auth.LogoutAsync(User.GetUserId());
        return NoContent();
    }
}
