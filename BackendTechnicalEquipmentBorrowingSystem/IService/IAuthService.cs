using BackendTechnicalEquipmentBorrowingSystem.DTOs;

namespace BackendTechnicalEquipmentBorrowingSystem.IService;

public interface IAuthService
{
    Task<AuthResult> RegisterAsync(RegisterRequest request);
    Task<AuthResult> LoginAsync(LoginRequest request);
    Task<AuthResult> RefreshAsync(string refreshToken);
    Task LogoutAsync(int userId);
}
