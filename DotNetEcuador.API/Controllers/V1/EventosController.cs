using Asp.Versioning;
using DotNetEcuador.API.Controllers;
using DotNetEcuador.API.Infraestructure.Services.Eventos;
using DotNetEcuador.API.Models.Eventos.DTOs;
using DotNetEcuador.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace DotNetEcuador.API.Controllers.V1;

[Route("api/v{version:apiVersion}/eventos")]
[ApiVersion("1.0")]
public class EventosController : BaseApiController
{
    private readonly IEventoService _eventoService;

    public EventosController(
        IEventoService eventoService,
        IMessageService messageService,
        ILogger<EventosController> logger) : base(messageService, logger)
    {
        _eventoService = eventoService;
    }

    /// <summary>
    /// Lista todos los eventos activos
    /// </summary>
    [HttpGet]
    [ProducesResponseType(200)]
    public async Task<IActionResult> GetAll()
    {
        var eventos = await _eventoService.GetAllAsync().ConfigureAwait(false);
        var dtos = eventos.Where(e => e.Activo).Select(ToPublicoDto).ToList();
        return SuccessResponse(dtos);
    }

    /// <summary>
    /// Obtiene los datos públicos de un evento por su slug
    /// </summary>
    [HttpGet("{slug}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetBySlug(string slug)
    {
        var evento = await _eventoService.GetBySlugAsync(slug).ConfigureAwait(false);
        if (evento is null)
            return NotFoundError($"Evento '{slug}' no encontrado.");

        var cupos = await _eventoService.GetCuposDisponiblesAsync(evento.Id).ConfigureAwait(false);
        var dto = ToPublicoDto(evento);

        return SuccessResponse(new
        {
            dto.Id,
            dto.Slug,
            dto.Nombre,
            dto.Descripcion,
            dto.FechaEvento,
            dto.FechaFin,
            dto.Lugar,
            dto.Precio,
            CuposDisponibles = cupos,
            dto.CapacidadMaxima,
            dto.Tipo,
            dto.Subtipo,
            dto.Formato,
            dto.Networking,
            dto.Tags,
            dto.Speakers,
            dto.CoverImage,
            dto.HostedBy,
            dto.PartnerEvento
        });
    }

    private static EventoPublicoDto ToPublicoDto(DotNetEcuador.API.Models.Eventos.Evento e) => new()
    {
        Id = e.Id,
        Slug = e.Slug,
        Nombre = e.Nombre,
        Descripcion = e.Descripcion,
        FechaEvento = e.FechaEvento,
        FechaFin = e.FechaFin,
        Lugar = e.Lugar,
        Precio = e.Precio,
        CapacidadMaxima = e.CapacidadMaxima,
        Tipo = e.Tipo,
        Subtipo = e.Subtipo,
        Formato = e.Formato,
        Networking = e.Networking,
        Tags = e.Tags,
        Speakers = e.Speakers,
        CoverImage = e.CoverImage,
        HostedBy = e.HostedBy,
        PartnerEvento = e.PartnerEvento
    };
}
