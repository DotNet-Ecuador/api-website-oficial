namespace DotNetEcuador.API.Models.Eventos.DTOs;

public class EventoPublicoDto
{
    public string Id { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public DateTime FechaEvento { get; set; }
    public DateTime? FechaFin { get; set; }
    public string Lugar { get; set; } = string.Empty;
    public decimal Precio { get; set; }
    public int CapacidadMaxima { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public string Subtipo { get; set; } = string.Empty;
    public string Formato { get; set; } = string.Empty;
    public bool Networking { get; set; }
    public List<string> Tags { get; set; } = new();
    public List<Speaker> Speakers { get; set; } = new();
    public string CoverImage { get; set; } = string.Empty;
    public string? HostedBy { get; set; }
    public string? PartnerEvento { get; set; }
    public string? RegistroUrl { get; set; }
}
