using DotNetEcuador.API.Common;
using DotNetEcuador.API.Controllers;
using DotNetEcuador.API.Models.Auth;
using DotNetEcuador.API.Services;
using DotNetEcuador.API.Services.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;

namespace DotNetEcuador.API.Controllers.V1;

[Route("api/v{version:apiVersion}/auth")]
[Produces("application/json")]
public class AuthController : BaseApiController
{
    private readonly IAuthService _authService;

    public AuthController(
        IAuthService authService,
        IMessageService messageService,
        ILogger<AuthController> logger) : base(messageService, logger)
    {
        _authService = authService;
    }

    /// <summary>
    /// Registra un nuevo usuario en el sistema
    /// </summary>
    /// <param name="request">Datos del usuario a registrar</param>
    /// <returns>Información de autenticación con tokens JWT</returns>
    /// <response code="200">Usuario registrado exitosamente</response>
    /// <response code="400">Error de validación o usuario ya existe</response>
    /// <response code="500">Error interno del servidor</response>
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginResponse), 200)]
    [ProducesResponseType(typeof(object), 400)]
    [ProducesResponseType(typeof(object), 500)]
    public async Task<ActionResult<LoginResponse>> Register([FromBody] RegisterRequest request)
    {
        try
        {
            var response = await _authService.RegisterAsync(request);
            Logger.LogInformation("User registered successfully: {Email}", request.Email);
            
            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            Logger.LogWarning("Registration failed: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error during user registration");
            return StatusCode(500, new { message = GetMessage(MessageKeys.ErrorServer) });
        }
    }

    /// <summary>
    /// Inicia sesión de un usuario existente
    /// </summary>
    /// <param name="request">Credenciales de acceso del usuario</param>
    /// <returns>Tokens de acceso y refresh, información del usuario</returns>
    /// <response code="200">Inicio de sesión exitoso</response>
    /// <response code="401">Credenciales inválidas</response>
    /// <response code="500">Error interno del servidor</response>
    [HttpPost("login")]
    [AllowAnonymous]
    [SwaggerOperation(
        Summary = "Inicia sesión de un usuario existente", 
        Description = "Autentica a un usuario con email y contraseña, devolviendo tokens JWT para acceso y renovación."
    )]
    [SwaggerResponse(200, "Inicio de sesión exitoso", typeof(LoginResponse))]
    [SwaggerResponse(401, "Credenciales inválidas")]
    [SwaggerResponse(500, "Error interno del servidor")]
    [ProducesResponseType(typeof(LoginResponse), 200)]
    [ProducesResponseType(typeof(object), 401)]
    [ProducesResponseType(typeof(object), 500)]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        try
        {
            var response = await _authService.LoginAsync(request);
            Logger.LogInformation("User logged in successfully: {Email}", request.Email);
            
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            Logger.LogWarning("Login failed: {Message}", ex.Message);
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error during user login");
            return StatusCode(500, new { message = GetMessage(MessageKeys.ErrorServer) });
        }
    }

    /// <summary>
    /// Renueva el token de acceso usando un refresh token válido
    /// </summary>
    /// <param name="request">Refresh token para generar nuevo access token</param>
    /// <returns>Nuevos tokens de acceso y refresh</returns>
    /// <response code="200">Token renovado exitosamente</response>
    /// <response code="401">Refresh token inválido o expirado</response>
    /// <response code="500">Error interno del servidor</response>
    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginResponse), 200)]
    [ProducesResponseType(typeof(object), 401)]
    [ProducesResponseType(typeof(object), 500)]
    public async Task<ActionResult<LoginResponse>> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        try
        {
            var response = await _authService.RefreshTokenAsync(request.RefreshToken);
            Logger.LogInformation("Token refreshed successfully");
            
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            Logger.LogWarning("Token refresh failed: {Message}", ex.Message);
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error during token refresh");
            return StatusCode(500, new { message = GetMessage(MessageKeys.ErrorServer) });
        }
    }

    /// <summary>
    /// Obtiene el perfil del usuario autenticado actual
    /// </summary>
    /// <returns>Información del perfil del usuario</returns>
    /// <response code="200">Perfil obtenido exitosamente</response>
    /// <response code="401">Usuario no autenticado</response>
    /// <response code="404">Usuario no encontrado</response>
    /// <response code="500">Error interno del servidor</response>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(UserInfo), 200)]
    [ProducesResponseType(typeof(object), 401)]
    [ProducesResponseType(typeof(object), 404)]
    [ProducesResponseType(typeof(object), 500)]
    public async Task<ActionResult<UserInfo>> GetProfile()
    {
        try
        {
            var userId = User.FindFirst("userId")?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "Token inválido" });
            }

            var user = await _authService.GetUserByIdAsync(userId);
            if (user == null)
            {
                return NotFound(new { message = "Usuario no encontrado" });
            }

            var userInfo = new UserInfo
            {
                Id = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                Role = user.Role
            };

            return Ok(userInfo);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error getting user profile");
            return StatusCode(500, new { message = GetMessage(MessageKeys.ErrorServer) });
        }
    }

    /// <summary>
    /// Cambia la contraseña del usuario autenticado
    /// </summary>
    /// <param name="request">Contraseña actual y nueva contraseña</param>
    /// <returns>Confirmación del cambio de contraseña</returns>
    /// <response code="200">Contraseña cambiada exitosamente</response>
    /// <response code="400">Contraseña actual incorrecta</response>
    /// <response code="401">Usuario no autenticado</response>
    /// <response code="500">Error interno del servidor</response>
    [HttpPut("password")]
    [Authorize]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(typeof(object), 400)]
    [ProducesResponseType(typeof(object), 401)]
    [ProducesResponseType(typeof(object), 500)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        try
        {
            var userId = User.FindFirst("userId")?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "Token inválido" });
            }

            await _authService.ChangePasswordAsync(userId, request);
            Logger.LogInformation("Password changed successfully for user: {UserId}", userId);
            
            return Ok(new { message = "Contraseña cambiada exitosamente" });
        }
        catch (UnauthorizedAccessException ex)
        {
            Logger.LogWarning("Password change failed: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error changing password");
            return StatusCode(500, new { message = GetMessage(MessageKeys.ErrorServer) });
        }
    }

    /// <summary>
    /// Cierra la sesión del usuario y revoca todos los refresh tokens
    /// </summary>
    /// <returns>Confirmación del cierre de sesión</returns>
    /// <response code="200">Sesión cerrada exitosamente</response>
    /// <response code="401">Usuario no autenticado</response>
    /// <response code="500">Error interno del servidor</response>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(typeof(object), 401)]
    [ProducesResponseType(typeof(object), 500)]
    public async Task<IActionResult> Logout()
    {
        try
        {
            var userId = User.FindFirst("userId")?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "Token inválido" });
            }

            await _authService.LogoutAsync(userId);
            Logger.LogInformation("User logged out successfully: {UserId}", userId);
            
            return Ok(new { message = "Sesión cerrada exitosamente" });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error during logout");
            return StatusCode(500, new { message = GetMessage(MessageKeys.ErrorServer) });
        }
    }

    /// <summary>
    /// Revoke a specific refresh token
    /// </summary>
    [HttpPost("revoke")]
    [Authorize]
    public async Task<IActionResult> RevokeToken([FromBody] RefreshTokenRequest request)
    {
        try
        {
            var success = await _authService.RevokeTokenAsync(request.RefreshToken);
            
            if (!success)
            {
                return BadRequest(new { message = GetMessage(MessageKeys.ErrorInvalidToken) });
            }

            Logger.LogInformation("Refresh token revoked successfully");
            return Ok(new { message = "Token revocado exitosamente" });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error revoking token");
            return StatusCode(500, new { message = GetMessage(MessageKeys.ErrorServer) });
        }
    }
}