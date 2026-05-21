namespace DotNetEcuador.API.Models.Eventos.DTOs;

public class ActualizarEventoRequestDto
{
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public DateTime? FechaEvento { get; set; }
    public DateTime? FechaFin { get; set; }
    public string? Lugar { get; set; }
    public decimal? Precio { get; set; }
    public int? CapacidadMaxima { get; set; }
    public bool Activo { get; set; } = true;
    public string? Tipo { get; set; }
    public string? Subtipo { get; set; }
    public string? Formato { get; set; }
    public bool Networking { get; set; }
    public List<string> Tags { get; set; } = new();
    public List<Speaker> Speakers { get; set; } = new();
    public string? CoverImage { get; set; }
    public string? HostedBy { get; set; }
    public string? PartnerEvento { get; set; }
    public string? RegistroUrl { get; set; }
}
