using DotNetEcuador.API.Models.Eventos;

namespace DotNetEcuador.API.Infraestructure.Services.Eventos;

public interface IEventoService
{
    Task<Evento?> GetBySlugAsync(string slug);
    Task<int> GetCuposDisponiblesAsync(string eventoId);
    Task<List<Evento>> GetAllAsync();
    Task CreateAsync(Evento evento);
}
