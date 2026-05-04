using Asp.Versioning;
using DotNetEcuador.API.Controllers;
using DotNetEcuador.API.Infraestructure.Services.Mentorias;
using DotNetEcuador.API.Models.Mentorias.DTOs;
using DotNetEcuador.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace DotNetEcuador.API.Controllers.V1;

[Route("api/v{version:apiVersion}/mentorias")]
[ApiVersion("1.0")]
public class MentoriasController : BaseApiController
{
    private readonly IMentoriaService _mentoriaService;

    public MentoriasController(
        IMentoriaService mentoriaService,
        IMessageService messageService,
        ILogger<MentoriasController> logger) : base(messageService, logger)
    {
        _mentoriaService = mentoriaService;
    }

    /// <summary>
    /// Retorna la lista de instituciones activas para el formulario de mentoría.
    /// </summary>
    [HttpGet("instituciones")]
    [ProducesResponseType(200)]
    public async Task<IActionResult> GetInstituciones()
    {
        var result = await _mentoriaService.GetInstitucionesAsync().ConfigureAwait(false);
        return SuccessResponse(result);
    }

    /// <summary>
    /// Registra una nueva solicitud de mentoría y dispara notificaciones.
    /// </summary>
    [HttpPost("solicitudes")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> CrearSolicitud([FromBody] SolicitudMentoriaRequestDto request)
    {
        try
        {
            var result = await _mentoriaService.CrearSolicitudAsync(request).ConfigureAwait(false);
            return SuccessResponse(result, result.Mensaje);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFoundError(ex.Message);
        }
    }
}
