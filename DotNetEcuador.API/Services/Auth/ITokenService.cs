using DotNetEcuador.API.Models.Auth;
using System.Security.Claims;

namespace DotNetEcuador.API.Services.Auth;

public interface ITokenService
{
    string GenerateAccessToken(User user);
    RefreshToken GenerateRefreshToken();
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
    string? GetUserIdFromToken(string token);
}