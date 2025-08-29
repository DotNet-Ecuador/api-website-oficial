using System.Text.RegularExpressions;
using DotNetEcuador.API.Common;
using DotNetEcuador.API.Infraestructure.Repositories;
using DotNetEcuador.API.Models.Auth;
using MongoDB.Bson;
using MongoDB.Driver;

namespace DotNetEcuador.API.Services.Auth;

public class AuthService : IAuthService
{
    private readonly IRepository<User> _userRepository;
    private readonly ITokenService _tokenService;
    private readonly IPasswordService _passwordService;

    public AuthService(
        IRepository<User> userRepository,
        ITokenService tokenService,
        IPasswordService passwordService)
    {
        _userRepository = userRepository;
        _tokenService = tokenService;
        _passwordService = passwordService;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        var user = await GetUserByEmailAsync(request.Email);
        if (user == null || !user.IsActive)
        {
            throw new UnauthorizedAccessException("Credenciales inválidas");
        }

        if (!_passwordService.VerifyPassword(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Credenciales inválidas");
        }

        // Update last login
        user.LastLoginAt = DateTime.UtcNow;

        // Generate tokens
        var accessToken = _tokenService.GenerateAccessToken(user);
        var refreshToken = _tokenService.GenerateRefreshToken();

        // Store refresh token
        user.RefreshTokens.Add(refreshToken);

        // Remove old refresh tokens (keep only last 5)
        user.RefreshTokens = user.RefreshTokens
            .Where(rt => rt.IsActive)
            .OrderByDescending(rt => rt.CreatedAt)
            .Take(5)
            .ToList();

        await _userRepository.UpdateAsync(user.Id, user);

        return new LoginResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken.Token,
            ExpiresAt = DateTime.UtcNow.AddMinutes(15),
            User = new UserInfo
            {
                Id = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                Role = user.Role
            }
        };
    }

    public async Task<LoginResponse> RegisterAsync(RegisterRequest request)
    {
        // Check if user already exists
        var existingUser = await GetUserByEmailAsync(request.Email);
        if (existingUser != null)
        {
            throw new InvalidOperationException("Ya existe un usuario con este email");
        }

        // Create new user
        var user = new User
        {
            Email = request.Email.ToLowerInvariant(),
            FullName = request.FullName,
            PasswordHash = _passwordService.HashPassword(request.Password),
            Role = UserRoles.User,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _userRepository.CreateAsync(user);

        // Generate tokens
        var accessToken = _tokenService.GenerateAccessToken(user);
        var refreshToken = _tokenService.GenerateRefreshToken();

        // Store refresh token
        user.RefreshTokens.Add(refreshToken);
        await _userRepository.UpdateAsync(user.Id, user);

        return new LoginResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken.Token,
            ExpiresAt = DateTime.UtcNow.AddMinutes(15),
            User = new UserInfo
            {
                Id = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                Role = user.Role
            }
        };
    }

    public async Task<LoginResponse> RefreshTokenAsync(string refreshToken)
    {
        var user = await _userRepository.FindAsync(u => 
            u.RefreshTokens.Any(rt => rt.Token == refreshToken && rt.IsActive));

        if (user == null)
        {
            throw new UnauthorizedAccessException("Token de refresco inválido");
        }

        var token = user.RefreshTokens.First(rt => rt.Token == refreshToken);

        if (!token.IsActive)
        {
            throw new UnauthorizedAccessException("Token de refresco inválido");
        }

        // Revoke old refresh token
        token.RevokedAt = DateTime.UtcNow;

        // Generate new tokens
        var newAccessToken = _tokenService.GenerateAccessToken(user);
        var newRefreshToken = _tokenService.GenerateRefreshToken();

        token.ReplacedByToken = newRefreshToken.Token;
        user.RefreshTokens.Add(newRefreshToken);

        // Clean up old tokens
        user.RefreshTokens = user.RefreshTokens
            .Where(rt => rt.IsActive || rt.RevokedAt > DateTime.UtcNow.AddDays(-1))
            .ToList();

        await _userRepository.UpdateAsync(user.Id, user);

        return new LoginResponse
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken.Token,
            ExpiresAt = DateTime.UtcNow.AddMinutes(15),
            User = new UserInfo
            {
                Id = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                Role = user.Role
            }
        };
    }

    public async Task<bool> RevokeTokenAsync(string refreshToken)
    {
        var user = await _userRepository.FindAsync(u => 
            u.RefreshTokens.Any(rt => rt.Token == refreshToken));

        if (user == null) return false;

        var token = user.RefreshTokens.FirstOrDefault(rt => rt.Token == refreshToken);
        if (token == null || !token.IsActive) return false;

        // Revoke token
        token.RevokedAt = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user.Id, user);

        return true;
    }

    public async Task<User?> GetUserByIdAsync(string userId)
    {
        return await _userRepository.GetByIdAsync(userId);
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return null;

        return await _userRepository.FindAsync(u => u.Email == email);
    }

    public async Task<bool> ChangePasswordAsync(string userId, ChangePasswordRequest request)
    {
        var user = await GetUserByIdAsync(userId);
        if (user == null) return false;

        if (!_passwordService.VerifyPassword(request.CurrentPassword, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Contraseña actual incorrecta");
        }

        user.PasswordHash = _passwordService.HashPassword(request.NewPassword);
        
        // Revoke all refresh tokens when password changes
        foreach (var token in user.RefreshTokens.Where(rt => rt.IsActive))
        {
            token.RevokedAt = DateTime.UtcNow;
        }

        await _userRepository.UpdateAsync(user.Id, user);
        return true;
    }

    public async Task LogoutAsync(string userId)
    {
        var user = await GetUserByIdAsync(userId);
        if (user == null) return;

        // Revoke all active refresh tokens
        foreach (var token in user.RefreshTokens.Where(rt => rt.IsActive))
        {
            token.RevokedAt = DateTime.UtcNow;
        }

        await _userRepository.UpdateAsync(user.Id, user);
    }
}