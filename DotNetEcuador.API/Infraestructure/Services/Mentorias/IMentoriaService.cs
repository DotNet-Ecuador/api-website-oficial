using DotNetEcuador.API.Models.Mentorias.DTOs;

namespace DotNetEcuador.API.Infraestructure.Services.Mentorias;

public interface IMentoriaService
{
    Task<List<InstitucionDto>> GetInstitucionesAsync();
    Task<SolicitudMentoriaResponseDto> CrearSolicitudAsync(SolicitudMentoriaRequestDto dto);
}
