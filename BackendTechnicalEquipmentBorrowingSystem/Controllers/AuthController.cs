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
    private const string AccessTokenCookie = "accessToken";
    private const string RefreshTokenCookie = "refreshToken";

    // Cross-site (Cloudflare Pages -> VPS) cookies require Secure + SameSite=None,
    // which browsers only honor over HTTPS. Until the API is served over TLS,
    // these cookies won't actually be set by the browser.
    private void SetAuthCookies(AuthResult result)
    {
        Response.Cookies.Append(AccessTokenCookie, result.AccessToken, new CookieOptions
        {
            HttpOnly = false,
            Secure = true,
            SameSite = SameSiteMode.None,
            Expires = result.AccessTokenExpiresAt
        });
        Response.Cookies.Append(RefreshTokenCookie, result.RefreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            Expires = DateTimeOffset.UtcNow.AddDays(7)
        });
    }

    private void ClearAuthCookies()
    {
        Response.Cookies.Delete(AccessTokenCookie);
        Response.Cookies.Delete(RefreshTokenCookie);
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResult>> Register(RegisterRequest request)
    {
        var result = await auth.RegisterAsync(request);
        SetAuthCookies(result);
        return Ok(result);
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResult>> Login(LoginRequest request)
    {
        var result = await auth.LoginAsync(request);
        SetAuthCookies(result);
        return Ok(result);
    }

    [HttpPost("refresh")]
    [HttpPost("refresh-token")]
    public async Task<IActionResult> Refresh()
    {
        var refreshToken = Request.Cookies[RefreshTokenCookie];
        if (string.IsNullOrEmpty(refreshToken))
            return Unauthorized();

        var result = await auth.RefreshAsync(refreshToken);
        SetAuthCookies(result);
        return Ok(new { data = result.AccessToken });
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await auth.LogoutAsync(User.GetUserId());
        ClearAuthCookies();
        return NoContent();
    }
}
