using DotNetEcuador.API.Infraestructure.Repositories;
using DotNetEcuador.API.Models.Eventos;
using DotNetEcuador.API.Models.Eventos.DTOs;

namespace DotNetEcuador.API.Infraestructure.Services.Eventos;

public class EventoService : IEventoService
{
    private readonly IRepository<Evento> _eventoRepo;
    private readonly IRepository<Registro> _registroRepo;

    public EventoService(IRepository<Evento> eventoRepo, IRepository<Registro> registroRepo)
    {
        _eventoRepo = eventoRepo;
        _registroRepo = registroRepo;
    }

    public async Task<Evento?> GetBySlugAsync(string slug)
        => await _eventoRepo.FindAsync(e => e.Slug == slug && e.Activo).ConfigureAwait(false);

    public async Task<Evento?> GetBySlugAdminAsync(string slug)
        => await _eventoRepo.FindAsync(e => e.Slug == slug).ConfigureAwait(false);

    public async Task<int> GetCuposDisponiblesAsync(string eventoId)
    {
        var evento = await _eventoRepo.FindAsync(e => e.Id == eventoId).ConfigureAwait(false)
            ?? throw new KeyNotFoundException($"Evento {eventoId} no encontrado.");

        var todos = await _registroRepo.GetAllAsync().ConfigureAwait(false);
        var ocupados = todos.Count(r => r.EventoId == eventoId && r.Estado != EstadoRegistro.Cancelado);
        return evento.CapacidadMaxima - ocupados;
    }

    public async Task<List<Evento>> GetAllAsync()
    {
        var eventos = await _eventoRepo.GetAllAsync().ConfigureAwait(false);
        return eventos.OrderBy(e => e.FechaEvento).ToList();
    }

    public async Task CreateAsync(Evento evento)
        => await _eventoRepo.CreateAsync(evento).ConfigureAwait(false);

    public async Task UpdateAsync(string slug, ActualizarEventoRequestDto dto)
    {
        var evento = await _eventoRepo.FindAsync(e => e.Slug == slug).ConfigureAwait(false)
            ?? throw new KeyNotFoundException($"Evento '{slug}' no encontrado.");

        evento.Nombre = dto.Nombre;
        evento.Descripcion = dto.Descripcion ?? string.Empty;
        if (dto.FechaEvento.HasValue) evento.FechaEvento = dto.FechaEvento.Value;
        evento.FechaFin = dto.FechaFin;
        evento.Lugar = dto.Lugar ?? string.Empty;
        if (dto.Precio.HasValue) evento.Precio = dto.Precio.Value;
        if (dto.CapacidadMaxima.HasValue) evento.CapacidadMaxima = dto.CapacidadMaxima.Value;
        evento.Activo = dto.Activo;
        evento.Tipo = dto.Tipo ?? string.Empty;
        evento.Subtipo = dto.Subtipo ?? string.Empty;
        evento.Formato = dto.Formato ?? string.Empty;
        evento.Networking = dto.Networking;
        evento.Tags = dto.Tags;
        evento.Speakers = dto.Speakers;
        evento.CoverImage = dto.CoverImage ?? string.Empty;
        evento.HostedBy = dto.HostedBy;
        evento.PartnerEvento = dto.PartnerEvento;
        evento.RegistroUrl = dto.RegistroUrl;
        evento.ActualizadoEn = DateTime.UtcNow;

        await _eventoRepo.UpdateAsync(evento.Id, evento).ConfigureAwait(false);
    }

    public async Task DeleteAsync(string slug)
    {
        var evento = await _eventoRepo.FindAsync(e => e.Slug == slug).ConfigureAwait(false)
            ?? throw new KeyNotFoundException($"Evento '{slug}' no encontrado.");

        var todos = await _registroRepo.GetAllAsync().ConfigureAwait(false);
        var activos = todos.Count(r =>
            r.EventoId == evento.Id &&
            r.EliminadoEn == null &&
            (r.Estado == EstadoRegistro.Pendiente || r.Estado == EstadoRegistro.Pagado));

        if (activos > 0)
            throw new InvalidOperationException(
                $"No se puede eliminar el evento porque tiene {activos} registro(s) activo(s).");

        await _eventoRepo.DeleteAsync(evento.Id).ConfigureAwait(false);
    }
}
