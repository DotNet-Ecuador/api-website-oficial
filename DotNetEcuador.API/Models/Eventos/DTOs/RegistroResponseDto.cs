namespace DotNetEcuador.API.Models.Eventos.DTOs;

public class RegistroResponseDto
{
    public string RegistroId { get; set; } = string.Empty;
    public string IdCorto { get; set; } = string.Empty;
    public string SessionToken { get; set; } = string.Empty;
    public decimal Monto { get; set; }
    public string NombreEvento { get; set; } = string.Empty;
}
