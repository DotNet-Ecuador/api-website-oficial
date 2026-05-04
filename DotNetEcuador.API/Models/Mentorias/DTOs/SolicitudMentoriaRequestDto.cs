namespace DotNetEcuador.API.Models.Mentorias.DTOs;

public class SolicitudMentoriaRequestDto
{
    public string NombreCompleto { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string InstitucionId { get; set; } = string.Empty;
    public string? OtraInstitucion { get; set; }
    public string TemaConsulta { get; set; } = string.Empty;
}
