using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using DotNetEcuador.API.Common;
using DotNetEcuador.API.Controllers;
using DotNetEcuador.API.Models;
using DotNetEcuador.API.Models.Common;
using DotNetEcuador.API.Infraestructure.Services;
using DotNetEcuador.API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Swashbuckle.AspNetCore.Annotations;

namespace DotNetEcuador.API.Controllers.V1;

[Route("api/v{version:apiVersion}/volunteer-application")]
public class VolunteerApplicationController : BaseApiController
{
    private readonly IVolunteerApplicationService _volunteerApplicationService;

    public VolunteerApplicationController(
        IVolunteerApplicationService volunteerApplicationService,
        IMessageService messageService,
        ILogger<VolunteerApplicationController> logger) : base(messageService, logger)
    {
        _volunteerApplicationService = volunteerApplicationService;
    }

    /// <summary>
    /// Envía una solicitud de voluntariado para unirse a la comunidad
    /// </summary>
    /// <param name="application">Datos completos de la aplicación de voluntariado</param>
    /// <returns>Confirmación del envío de la solicitud</returns>
    /// <response code="200">Solicitud enviada exitosamente</response>
    /// <response code="400">Error de validación en los datos enviados</response>
    /// <response code="500">Error interno del servidor</response>
    /// <remarks>
    /// Ejemplo de solicitud:
    /// 
    ///     POST /api/v1/volunteer-application/apply
    ///     {
    ///         "fullName": "Juan Pérez González",
    ///         "email": "juan@ejemplo.com",
    ///         "phoneNumber": "+593987654321",
    ///         "city": "Quito",
    ///         "hasVolunteeringExperience": true,
    ///         "areasOfInterest": [
    ///             "EventOrganization",
    ///             "TechnicalSupport"
    ///         ],
    ///         "otherAreas": "",
    ///         "availableTime": "Fines de semana y noches entre semana",
    ///         "skillsOrKnowledge": "Programación en C#, .NET, Azure",
    ///         "whyVolunteer": "Quiero contribuir al crecimiento de la comunidad .NET",
    ///         "additionalComments": "Disponible para eventos presenciales en Quito"
    ///     }
    /// 
    /// Valores válidos para areasOfInterest:
    /// - "EventOrganization": Organización de eventos
    /// - "ContentCreation": Creación de contenido
    /// - "TechnicalSupport": Soporte técnico
    /// - "SocialMediaManagement": Gestión de redes sociales
    /// - "Other": Otras áreas (requiere especificar otherAreas)
    /// </remarks>
    [HttpPost("apply")]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(typeof(object), 400)]
    [ProducesResponseType(typeof(object), 500)]
    public async Task<IActionResult> Apply(VolunteerApplication application)
    {
        // Debug logging
        Logger.LogInformation("Received volunteer application: FullName={FullName}, Email={Email}, AreasOfInterest={AreasCount}", 
            application.FullName, application.Email, application.AreasOfInterest?.Count ?? 0);

        var isValid = _volunteerApplicationService.AreValidAreasOfInterest(application.AreasOfInterest);
        var isValidOtherAreas = application.ValidateOtherAreas();

        Logger.LogInformation("Validation results: AreasValid={AreasValid}, OtherAreasValid={OtherAreasValid}", 
            isValid, isValidOtherAreas);

        if (string.IsNullOrWhiteSpace(application.FullName) || application.FullName.Length < 3)
        {
            Logger.LogWarning("Validation failed: FullName is invalid");
            return BadRequest(GetMessage(MessageKeys.ValidationFullName));
        }

        if (string.IsNullOrWhiteSpace(application.Email) || !new EmailAddressAttribute().IsValid(application.Email))
        {
            Logger.LogWarning("Validation failed: Email is invalid");
            return BadRequest(GetMessage(MessageKeys.ValidationEmail));
        }

        if (!isValid)
        {
            Logger.LogWarning("Validation failed: AreasOfInterest are invalid");
            return BadRequest(GetMessage(MessageKeys.ValidationAreas));
        }

        if (!isValidOtherAreas)
        {
            Logger.LogWarning("Validation failed: OtherAreas validation failed");
            return BadRequest(GetMessage(MessageKeys.ValidationOtherAreas));
        }

        try
        {
            Logger.LogInformation("Attempting to save volunteer application to database");
            await _volunteerApplicationService.CreateAsync(application).ConfigureAwait(false);
            Logger.LogInformation("Volunteer application saved successfully");
            return Ok(GetMessage(MessageKeys.SuccessVolunteerSent));
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error saving volunteer application");
            return StatusCode(500, new { message = "Error guardando la solicitud: " + ex.Message });
        }
    }

    /// <summary>
    /// Lista todas las solicitudes de voluntariado con paginación
    /// </summary>
    /// <param name="page">Número de página (por defecto: 1)</param>
    /// <param name="pageSize">Elementos por página (por defecto: 10, máximo: 100)</param>
    /// <param name="search">Término de búsqueda opcional para filtrar por nombre, email o ciudad</param>
    /// <param name="sortBy">Campo por el cual ordenar (createdAt, fullName, email, city)</param>
    /// <param name="sortOrder">Orden de clasificación (asc, desc)</param>
    /// <returns>Lista paginada de solicitudes de voluntariado</returns>
    /// <response code="200">Lista obtenida exitosamente</response>
    /// <response code="400">Parámetros de paginación inválidos</response>
    /// <response code="401">Usuario no autenticado</response>
    /// <response code="403">Sin permisos para acceder a esta información</response>
    /// <response code="500">Error interno del servidor</response>
    /// <remarks>
    /// Este endpoint permite a los administradores y moderadores consultar todas las solicitudes 
    /// de voluntariado enviadas por los usuarios. Incluye funcionalidades de:
    /// 
    /// **Paginación:**
    /// - `page`: Número de página (mínimo 1)
    /// - `pageSize`: Elementos por página (1-100)
    /// 
    /// **Búsqueda:**
    /// - `search`: Busca en nombre completo, email y ciudad
    /// 
    /// **Ordenamiento:**
    /// - `sortBy`: Campo para ordenar
    /// - `sortOrder`: "asc" (ascendente) o "desc" (descendente)
    /// 
    /// Ejemplo de uso:
    /// 
    ///     GET /api/v1/volunteer-application?page=1&amp;pageSize=10&amp;search=juan&amp;sortBy=fullName&amp;sortOrder=asc
    /// 
    /// **Respuesta incluye:**
    /// - `data`: Array de solicitudes
    /// - `totalCount`: Total de registros
    /// - `page`: Página actual
    /// - `pageSize`: Elementos por página
    /// - `totalPages`: Total de páginas
    /// - `hasNextPage`: Indica si hay página siguiente
    /// - `hasPreviousPage`: Indica si hay página anterior
    /// </remarks>
    [HttpGet]
    [Authorize]
    [SwaggerOperation(
        Summary = "Lista solicitudes de voluntariado con paginación",
        Description = "Obtiene una lista paginada de todas las solicitudes de voluntariado. Requiere permisos de administrador o moderador."
    )]
    [SwaggerResponse(200, "Lista paginada obtenida exitosamente", typeof(PagedResponse<VolunteerApplication>))]
    [SwaggerResponse(400, "Parámetros de paginación inválidos")]
    [SwaggerResponse(401, "Usuario no autenticado")]
    [SwaggerResponse(403, "Sin permisos suficientes")]
    [SwaggerResponse(500, "Error interno del servidor")]
    [ProducesResponseType(typeof(PagedResponse<VolunteerApplication>), 200)]
    [ProducesResponseType(typeof(object), 400)]
    [ProducesResponseType(typeof(object), 401)]
    [ProducesResponseType(typeof(object), 403)]
    [ProducesResponseType(typeof(object), 500)]
    public async Task<IActionResult> GetAllApplications([FromQuery] PagedRequest request)
    {
        try
        {
            // Debug: Log user claims
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value ?? 
                          User.FindFirst("role")?.Value ?? 
                          "No role found";
            var userId = User.FindFirst("userId")?.Value ?? "No userId found";
            var allClaims = User.Claims.Select(c => $"{c.Type}={c.Value}");
            Logger.LogInformation("User requesting volunteer applications - UserId: {UserId}, Role: {Role}, All Claims: {Claims}", 
                userId, userRole, string.Join(", ", allClaims));

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            PagedResponse<VolunteerApplication> result;
            
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                result = await _volunteerApplicationService.SearchAsync(request, request.Search);
            }
            else
            {
                result = await _volunteerApplicationService.GetAllAsync(request);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error retrieving volunteer applications: {ErrorMessage}", ex.Message);
            return StatusCode(500, new { 
                message = GetMessage(MessageKeys.ErrorServer),
                details = ex.Message,
                stackTrace = ex.StackTrace?.Split('\n').Take(5).ToArray() // Solo primeras 5 líneas para debug
            });
        }
    }

    /// <summary>
    /// Endpoint temporal para verificar claims del usuario autenticado
    /// </summary>
    [HttpGet("debug/claims")]
    [Authorize]
    public IActionResult GetUserClaims()
    {
        var claims = User.Claims.Select(c => new { Type = c.Type, Value = c.Value }).ToList();
        return Ok(new { 
            IsAuthenticated = User.Identity?.IsAuthenticated,
            Claims = claims,
            Role = User.FindFirst(ClaimTypes.Role)?.Value,
            UserId = User.FindFirst("userId")?.Value
        });
    }

    /// <summary>
    /// Endpoint temporal para crear datos de prueba
    /// </summary>
    [HttpPost("debug/seed")]
    [Authorize(Roles = "Admin,User")]
    public async Task<IActionResult> SeedTestData()
    {
        try
        {
            var testApplications = new List<VolunteerApplication>
            {
                new VolunteerApplication
                {
                    FullName = "María González",
                    Email = "maria@ejemplo.com",
                    PhoneNumber = "+593987654321",
                    City = "Quito",
                    HasVolunteeringExperience = true,
                    AreasOfInterest = new List<string> { "EventOrganization", "ContentCreation" },
                    AvailableTime = "Fines de semana",
                    SkillsOrKnowledge = "Diseño gráfico, redes sociales",
                    WhyVolunteer = "Quiero contribuir a la comunidad",
                    AdditionalComments = "Disponible para eventos presenciales"
                },
                new VolunteerApplication
                {
                    FullName = "Carlos Pérez",
                    Email = "carlos@ejemplo.com",
                    PhoneNumber = "+593987123456",
                    City = "Guayaquil",
                    HasVolunteeringExperience = false,
                    AreasOfInterest = new List<string> { "TechnicalSupport" },
                    AvailableTime = "Noches entre semana",
                    SkillsOrKnowledge = "Programación en C#, .NET",
                    WhyVolunteer = "Desarrollar mis habilidades técnicas",
                    AdditionalComments = "Nuevo en voluntariado pero muy motivado"
                },
                new VolunteerApplication
                {
                    FullName = "Ana Rodríguez",
                    Email = "ana@ejemplo.com",
                    PhoneNumber = "+593981234567",
                    City = "Cuenca",
                    HasVolunteeringExperience = true,
                    AreasOfInterest = new List<string> { "SocialMediaManagement", "Other" },
                    OtherAreas = "Mentoring para mujeres en tech",
                    AvailableTime = "Flexible",
                    SkillsOrKnowledge = "Marketing digital, community management",
                    WhyVolunteer = "Promover la diversidad en tecnología",
                    AdditionalComments = "Experiencia previa en comunidades tech"
                }
            };

            foreach (var app in testApplications)
            {
                await _volunteerApplicationService.CreateAsync(app);
            }

            return Ok(new { message = $"Se crearon {testApplications.Count} solicitudes de prueba exitosamente." });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error creating test data");
            return StatusCode(500, new { message = "Error creando datos de prueba: " + ex.Message });
        }
    }

    /// <summary>
    /// Endpoint temporal para verificar la configuración de MongoDB
    /// </summary>
    [HttpGet("debug/mongodb")]
    [Authorize]
    public async Task<IActionResult> VerifyMongoDBConfiguration()
    {
        try
        {
            // Intentar contar documentos en la colección
            var result = await _volunteerApplicationService.GetAllAsync(new PagedRequest { Page = 1, PageSize = 1 });
            
            return Ok(new { 
                message = "MongoDB connection successful",
                databaseName = "dotnet_ecuador",
                collectionName = "volunteer_applications",
                documentCount = result.TotalCount,
                status = "Connected"
            });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "MongoDB verification failed");
            return Ok(new { 
                message = "MongoDB connection failed",
                databaseName = "dotnet_ecuador",
                collectionName = "volunteer_applications",
                error = ex.Message,
                status = "Failed"
            });
        }
    }
}
