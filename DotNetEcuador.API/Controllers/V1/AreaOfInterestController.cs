using DotNetEcuador.API.Controllers;
using DotNetEcuador.API.Models;
using DotNetEcuador.API.Infraestructure.Services;
using DotNetEcuador.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace DotNetEcuador.API.Controllers.V1;

[Route("api/v{version:apiVersion}/area-interest")]
public class AreaOfInterestController : BaseApiController
{
    private readonly IAreaOfInterestService _areaOfInterestService;

    public AreaOfInterestController(
        IAreaOfInterestService areaOfInterestService,
        IMessageService messageService,
        ILogger<AreaOfInterestController> logger) : base(messageService, logger)
    {
        _areaOfInterestService = areaOfInterestService;
    }

    /// <summary>
    /// Obtiene todas las áreas de interés disponibles para voluntariado
    /// </summary>
    /// <returns>Lista de todas las áreas de interés configuradas</returns>
    /// <response code="200">Lista obtenida exitosamente</response>
    /// <response code="500">Error interno del servidor</response>
    /// <remarks>
    /// Este endpoint devuelve todas las áreas de interés que los voluntarios pueden seleccionar
    /// al enviar su solicitud. Incluye tanto las áreas predefinidas como cualquier área personalizada
    /// que haya sido agregada por los administradores.
    /// </remarks>
    [HttpGet]
    [ProducesResponseType(typeof(List<AreaOfInterest>), 200)]
    [ProducesResponseType(typeof(object), 500)]
    public async Task<IActionResult> GetAllAreasOfInterest()
    {
        var areas = await _areaOfInterestService.GetAllAreasOfInterestAsync().ConfigureAwait(false);
        return Ok(areas);
    }

    /// <summary>
    /// Crea una nueva área de interés para voluntariado
    /// </summary>
    /// <param name="areaOfInterest">Datos de la nueva área de interés</param>
    /// <returns>Confirmación de creación</returns>
    /// <response code="200">Área de interés creada exitosamente</response>
    /// <response code="400">Error de validación en los datos</response>
    /// <response code="500">Error interno del servidor</response>
    /// <remarks>
    /// Solo los administradores pueden crear nuevas áreas de interés.
    /// Las áreas creadas estarán disponibles para que los voluntarios las seleccionen
    /// en sus solicitudes futuras.
    /// 
    /// Ejemplo:
    /// 
    ///     POST /api/v1/area-interest
    ///     {
    ///         "name": "MentorshipProgram",
    ///         "description": "Programa de mentoría para nuevos desarrolladores"
    ///     }
    /// </remarks>
    [HttpPost]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(typeof(object), 400)]
    [ProducesResponseType(typeof(object), 500)]
    public async Task<IActionResult> CreateAreaOfInterest(
        AreaOfInterest areaOfInterest)
    {
        await _areaOfInterestService.CreateAreaOfInterestAsync(areaOfInterest).ConfigureAwait(false);
        return Ok();
    }
}
