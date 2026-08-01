using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using BackendTechnicalEquipmentBorrowingSystem.DTOs;
using BackendTechnicalEquipmentBorrowingSystem.Entities;
using BackendTechnicalEquipmentBorrowingSystem.IRepository;
using BackendTechnicalEquipmentBorrowingSystem.IService;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace BackendTechnicalEquipmentBorrowingSystem.Services;

public class AuthService : IAuthService
{
    private readonly IRepository<User> _users;
    private readonly IActivityLogService _log;
    private readonly IConfiguration _config;

    public AuthService(IRepository<User> users, IActivityLogService log, IConfiguration config)
    {
        _users = users;
        _log = log;
        _config = config;
    }

    public async Task<AuthResult> RegisterAsync(RegisterRequest r)
    {
        if (await _users.ExistsAsync(u => u.Email == r.Email))
            throw new InvalidOperationException($"Email '{r.Email}' is already registered.");

        User user = r.Role switch
        {
            Role.Student => new Student
            {
                StudentNumber = r.StudentNumber
                    ?? throw new InvalidOperationException("StudentNumber is required for students."),
                Course = r.Course,
                Section = r.Section
            },
            Role.Faculty => new Faculty
            {
                Position = r.Position ?? FacultyPosition.Other,
                EmployeeNumber = r.EmployeeNumber,
                Department = r.Department
            },
            _ => new User()
        };

        user.FirstName = r.FirstName;
        user.LastName = r.LastName;
        user.Email = r.Email;
        user.Role = r.Role;
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(r.Password);

        await _users.AddAsync(user);
        await _users.SaveChangesAsync();
        await _log.LogAsync("Register", user.Id, nameof(User), user.Id.ToString());
        return await IssueTokensAsync(user);
    }

    public async Task<AuthResult> LoginAsync(LoginRequest r)
    {
        var user = await _users.FirstOrDefaultAsync(u => u.Email == r.Email);
        if (user is null || !BCrypt.Net.BCrypt.Verify(r.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid email or password.");
        if (user.IsBlocked || !user.IsActive)
            throw new UnauthorizedAccessException("Account is blocked or inactive.");

        await _log.LogAsync("Login", user.Id, nameof(User), user.Id.ToString());
        return await IssueTokensAsync(user);
    }

    public async Task<AuthResult> RefreshAsync(string refreshToken)
    {
        var user = await _users.FirstOrDefaultAsync(u => u.RefreshToken == refreshToken);
        if (user is null || user.RefreshTokenExpiresAt is null || user.RefreshTokenExpiresAt < DateTime.UtcNow)
            throw new UnauthorizedAccessException("Invalid or expired refresh token.");
        return await IssueTokensAsync(user);
    }

    public async Task LogoutAsync(int userId)
    {
        var user = await _users.GetByIdAsync(userId);
        if (user is null) return;
        user.RefreshToken = null;
        user.RefreshTokenExpiresAt = null;
        _users.Update(user);
        await _users.SaveChangesAsync();
    }

    private async Task<AuthResult> IssueTokensAsync(User user)
    {
        var (access, expires) = GenerateAccessToken(user);
        var refresh = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        var refreshDays = int.Parse(_config["Jwt:RefreshTokenDays"] ?? "7");

        user.RefreshToken = refresh;
        user.RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(refreshDays);
        _users.Update(user);
        await _users.SaveChangesAsync();

        return new AuthResult(user.Id, $"{user.FirstName} {user.LastName}", user.Role, access, refresh, expires);
    }

    private (string token, DateTime expires) GenerateAccessToken(User user)
    {
        var key = _config["Jwt:Key"] ?? "";
        var minutes = int.Parse(_config["Jwt:AccessTokenMinutes"] ?? "60");
        var expires = DateTime.UtcNow.AddMinutes(minutes);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };
        var creds = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)), SecurityAlgorithms.HmacSha256);
        var jwt = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: expires,
            signingCredentials: creds);
        return (new JwtSecurityTokenHandler().WriteToken(jwt), expires);
    }
}
