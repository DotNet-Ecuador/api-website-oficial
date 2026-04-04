namespace DotNetEcuador.API.Models.Eventos.DTOs;

public class RegistroRequestDto
{
    public string Nombre { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Empresa { get; set; } = string.Empty;
    public string Cargo { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public bool AceptaMarketing { get; set; }
    public string EventoSlug { get; set; } = string.Empty;
}
