using Asp.Versioning;
using DotNetEcuador.API.Controllers;
using DotNetEcuador.API.Infraestructure.Services.Eventos;
using DotNetEcuador.API.Models.Common;
using DotNetEcuador.API.Models.Eventos;
using DotNetEcuador.API.Models.Eventos.DTOs;
using DotNetEcuador.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace DotNetEcuador.API.Controllers.V1;

[Route("api/v{version:apiVersion}/admin/eventos")]
[ApiVersion("1.0")]
[Authorize]
public class AdminEventosController : BaseApiController
{
    private readonly IEventoService _eventoService;
    private readonly IRegistroService _registroService;
    private readonly IExportService _exportService;

    public AdminEventosController(
        IEventoService eventoService,
        IRegistroService registroService,
        IExportService exportService,
        IMessageService messageService,
        ILogger<AdminEventosController> logger) : base(messageService, logger)
    {
        _eventoService = eventoService;
        _registroService = registroService;
        _exportService = exportService;
    }

    /// <summary>
    /// Lista todos los eventos
    /// </summary>
    [HttpGet]
    [ProducesResponseType(200)]
    public async Task<IActionResult> GetEventos()
    {
        var eventos = await _eventoService.GetAllAsync().ConfigureAwait(false);
        return SuccessResponse(eventos);
    }

    /// <summary>
    /// Crea un nuevo evento
    /// </summary>
    [HttpPost]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> CreateEvento([FromBody] Evento evento)
    {
        await _eventoService.CreateAsync(evento).ConfigureAwait(false);
        return SuccessResponse(evento, "Evento creado exitosamente.");
    }

    /// <summary>
    /// Lista registros con filtros y paginación
    /// </summary>
    [HttpGet("registros")]
    [ProducesResponseType(200)]
    public async Task<IActionResult> GetRegistros(
        [FromQuery] PagedRequest request,
        [FromQuery] string? eventoId = null,
        [FromQuery] string? estado = null)
    {
        var result = await _registroService.GetAdminRegistrosAsync(request, eventoId, estado).ConfigureAwait(false);
        return SuccessResponse(result);
    }

    /// <summary>
    /// Aprueba el pago de un registro y envía email con QR
    /// </summary>
    [HttpPatch("registros/{id}/aprobar")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Aprobar(string id, [FromBody] string? notasAdmin = null)
    {
        try
        {
            await _registroService.AprobarAsync(id, notasAdmin ?? string.Empty).ConfigureAwait(false);
            return SuccessResponse("Registro aprobado. Email con QR enviado al asistente.");
        }
        catch (KeyNotFoundException ex)
        {
            return NotFoundError(ex.Message);
        }
    }

    /// <summary>
    /// Rechaza un registro con motivo y envía email de notificación
    /// </summary>
    [HttpPatch("registros/{id}/rechazar")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Rechazar(string id, [FromBody] RechazarRegistroDto dto)
    {
        try
        {
            await _registroService.RechazarAsync(id, dto.Motivo).ConfigureAwait(false);
            return SuccessResponse("Registro rechazado. Email enviado al asistente.");
        }
        catch (KeyNotFoundException ex)
        {
            return NotFoundError(ex.Message);
        }
    }

    /// <summary>
    /// Exporta CSV de registrados (solo con aceptaMarketing=true) para email marketing
    /// </summary>
    [HttpGet("registros/exportar")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> ExportarCsv([FromQuery] string eventoId)
    {
        if (string.IsNullOrEmpty(eventoId))
            return BusinessError("evento-requerido", "El parámetro eventoId es requerido.");

        var evento = await _eventoService.GetAllAsync().ConfigureAwait(false);
        var evt = evento.FirstOrDefault(e => e.Id == eventoId);
        if (evt is null) return NotFoundError("Evento no encontrado.");

        var csv = await _exportService.ExportarRegistradosCsvAsync(eventoId).ConfigureAwait(false);
        var bytes = Encoding.UTF8.GetBytes(csv);
        var fileName = $"registrados-{evt.Slug}-{DateTime.UtcNow:yyyyMMdd}.csv";
        return File(bytes, "text/csv", fileName);
    }
}
