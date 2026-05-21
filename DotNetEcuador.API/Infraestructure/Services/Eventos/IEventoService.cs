using DotNetEcuador.API.Models.Eventos;
using DotNetEcuador.API.Models.Eventos.DTOs;

namespace DotNetEcuador.API.Infraestructure.Services.Eventos;

public interface IEventoService
{
    Task<Evento?> GetBySlugAsync(string slug);
    Task<Evento?> GetBySlugAdminAsync(string slug);
    Task<int> GetCuposDisponiblesAsync(string eventoId);
    Task<List<Evento>> GetAllAsync();
    Task CreateAsync(Evento evento);
    Task UpdateAsync(string slug, ActualizarEventoRequestDto dto);
    Task DeleteAsync(string slug);
}
