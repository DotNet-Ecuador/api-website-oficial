using Asp.Versioning;
using DotNetEcuador.API.Controllers;
using DotNetEcuador.API.Infraestructure.Services.Eventos;
using DotNetEcuador.API.Models.Eventos.DTOs;
using DotNetEcuador.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace DotNetEcuador.API.Controllers.V1;

[Route("api/v{version:apiVersion}/registros")]
[ApiVersion("1.0")]
public class RegistrosController : BaseApiController
{
    private readonly IRegistroService _registroService;

    public RegistrosController(
        IRegistroService registroService,
        IMessageService messageService,
        ILogger<RegistrosController> logger) : base(messageService, logger)
    {
        _registroService = registroService;
    }

    /// <summary>
    /// Crea un nuevo registro para un evento (estado: pendiente)
    /// </summary>
    [HttpPost]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(409)]
    public async Task<IActionResult> Registrar([FromBody] RegistroRequestDto request)
    {
        try
        {
            var result = await _registroService.CrearRegistroAsync(request).ConfigureAwait(false);
            return SuccessResponse(result, "Registro creado. Realiza la transferencia con los datos indicados.");
        }
        catch (KeyNotFoundException ex)
        {
            return NotFoundError(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BusinessError("registro-invalido", ex.Message, 409);
        }
    }

    /// <summary>
    /// Sube el comprobante de transferencia. Requiere X-Session-Token en el header.
    /// </summary>
    [HttpPost("{id}/comprobante")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> SubirComprobante(
        string id,
        [FromForm] ComprobanteRequestDto dto,
        [FromHeader(Name = "X-Session-Token")] string sessionToken)
    {
        if (string.IsNullOrEmpty(sessionToken))
            return UnauthorizedError("Se requiere el header X-Session-Token.");

        try
        {
            await _registroService.SubirComprobanteAsync(id, sessionToken, dto).ConfigureAwait(false);
            return SuccessResponse("Comprobante recibido. Tu registro está en revisión.");
        }
        catch (KeyNotFoundException ex)
        {
            return NotFoundError(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            return UnauthorizedError(ex.Message);
        }
    }

    /// <summary>
    /// Recupera el registro de un asistente por email y slug del evento
    /// </summary>
    [HttpGet("recuperar")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Recuperar([FromQuery] string? email, [FromQuery] string? eventoSlug)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(eventoSlug))
            return BusinessError("parametros-invalidos", "Los parámetros email y eventoSlug son requeridos.", 400);

        try
        {
            var result = await _registroService.RecuperarRegistroAsync(email, eventoSlug).ConfigureAwait(false);
            return SuccessResponse(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFoundError(ex.Message);
        }
    }

    /// <summary>
    /// Consulta el estado actual de un registro (para polling en frontend)
    /// </summary>
    [HttpGet("{id}/estado")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetEstado(string id)
    {
        try
        {
            var result = await _registroService.GetEstadoAsync(id).ConfigureAwait(false);
            return SuccessResponse(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFoundError(ex.Message);
        }
    }
}
