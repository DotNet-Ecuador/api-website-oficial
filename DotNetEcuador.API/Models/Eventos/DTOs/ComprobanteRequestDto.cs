namespace DotNetEcuador.API.Models.Eventos.DTOs;

public class ComprobanteRequestDto
{
    public string? ReferenciaPago { get; set; }
    public IFormFile? Comprobante { get; set; }
}
