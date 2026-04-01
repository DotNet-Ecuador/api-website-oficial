using DotNetEcuador.API.Models.Eventos;
using DotNetEcuador.API.Models.Eventos.DTOs;
using DotNetEcuador.API.Models.Common;

namespace DotNetEcuador.API.Infraestructure.Services.Eventos;

public interface IRegistroService
{
    Task<RegistroResponseDto> CrearRegistroAsync(RegistroRequestDto request);
    Task SubirComprobanteAsync(string registroId, string sessionToken, ComprobanteRequestDto dto);
    Task<EventoEstadoDto> GetEstadoAsync(string registroId);
    Task<PagedResponse<AdminRegistroDto>> GetAdminRegistrosAsync(PagedRequest request, string? eventoId, string? estado);
    Task AprobarAsync(string registroId, string notasAdmin);
    Task RechazarAsync(string registroId, string motivo);
    Task<RecuperarRegistroDto> RecuperarRegistroAsync(string email, string eventoSlug);
}
