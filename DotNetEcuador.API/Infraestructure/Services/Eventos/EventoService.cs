using DotNetEcuador.API.Infraestructure.Repositories;
using DotNetEcuador.API.Models.Eventos;

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

    public async Task<int> GetCuposDisponiblesAsync(string eventoId)
    {
        var evento = await _eventoRepo.FindAsync(e => e.Id == eventoId).ConfigureAwait(false)
            ?? throw new KeyNotFoundException($"Evento {eventoId} no encontrado.");

        var todos = await _registroRepo.GetAllAsync().ConfigureAwait(false);
        var ocupados = todos.Count(r => r.EventoId == eventoId && r.Estado != EstadoRegistro.Cancelado);
        return evento.CapacidadMaxima - ocupados;
    }

    public async Task<List<Evento>> GetAllAsync()
        => await _eventoRepo.GetAllAsync().ConfigureAwait(false);

    public async Task CreateAsync(Evento evento)
        => await _eventoRepo.CreateAsync(evento).ConfigureAwait(false);
}
