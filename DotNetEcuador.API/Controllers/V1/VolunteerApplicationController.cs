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
    ///         "country": "Ecuador",
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
        // FluentValidation will handle validation automatically via ValidationActionFilter
        // If we reach here, the model is valid

        Logger.LogInformation("Received valid volunteer application: FullName={FullName}, Email={Email}, AreasOfInterest={AreasCount}", 
            application.FullName, application.Email, application.AreasOfInterest?.Count ?? 0);

        try
        {
            Logger.LogInformation("Attempting to save volunteer application to database");
            await _volunteerApplicationService.CreateAsync(application).ConfigureAwait(false);
            Logger.LogInformation("Volunteer application saved successfully");
            
            return SuccessResponse(GetMessage(MessageKeys.SuccessVolunteerSent));
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error saving volunteer application");
            throw; // Let GlobalExceptionMiddleware handle it
        }
    }

    /// <summary>
    /// Lista todas las solicitudes de voluntariado con paginación
    /// </summary>
    /// <param name="request">Parámetros de paginación y filtrado</param>
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
        // FluentValidation will handle validation automatically via ValidationActionFilter
        // Debug: Log user claims
        var userRole = User.FindFirst(ClaimTypes.Role)?.Value ?? 
                      User.FindFirst("role")?.Value ?? 
                      "No role found";
        var userId = User.FindFirst("userId")?.Value ?? "No userId found";
        var allClaims = User.Claims.Select(c => $"{c.Type}={c.Value}");
        Logger.LogInformation("User requesting volunteer applications - UserId: {UserId}, Role: {Role}, All Claims: {Claims}", 
            userId, userRole, string.Join(", ", allClaims));

        try
        {
            PagedResponse<VolunteerApplication> result;
            
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                result = await _volunteerApplicationService.SearchAsync(request, request.Search);
            }
            else
            {
                result = await _volunteerApplicationService.GetAllAsync(request);
            }

            return SuccessResponse(result, "Lista de solicitudes obtenida exitosamente");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error retrieving volunteer applications: {ErrorMessage}", ex.Message);
            throw; // Let GlobalExceptionMiddleware handle it
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
        var data = new { 
            IsAuthenticated = User.Identity?.IsAuthenticated,
            Claims = claims,
            Role = User.FindFirst(ClaimTypes.Role)?.Value,
            UserId = User.FindFirst("userId")?.Value
        };
        
        return SuccessResponse(data, "Claims obtenidos exitosamente");
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
                    Country = "Ecuador",
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
                    Country = "Ecuador",
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
                    Country = "Ecuador",
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

            var message = $"Se crearon {testApplications.Count} solicitudes de prueba exitosamente.";
            return SuccessResponse(message);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error creating test data");
            throw; // Let GlobalExceptionMiddleware handle it
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
            
            var data = new { 
                message = "MongoDB connection successful",
                databaseName = "dotnet_ecuador",
                collectionName = "volunteer_applications",
                documentCount = result.TotalCount,
                status = "Connected"
            };
            
            return SuccessResponse(data, "Verificación de MongoDB completada");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "MongoDB verification failed");
            var errorData = new { 
                message = "MongoDB connection failed",
                databaseName = "dotnet_ecuador",
                collectionName = "volunteer_applications",
                error = ex.Message,
                status = "Failed"
            };
            
            return SuccessResponse(errorData, "Verificación de MongoDB con errores");
        }
    }
}
