using DotNetEcuador.API.Models.Auth;

namespace DotNetEcuador.API.Services.Auth;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request);
    Task<LoginResponse> RegisterAsync(RegisterRequest request);
    Task<LoginResponse> RefreshTokenAsync(string refreshToken);
    Task<bool> RevokeTokenAsync(string refreshToken);
    Task<User?> GetUserByIdAsync(string userId);
    Task<User?> GetUserByEmailAsync(string email);
    Task<bool> ChangePasswordAsync(string userId, ChangePasswordRequest request);
    Task LogoutAsync(string userId);
}