namespace DotNetEcuador.API.Models.Eventos.DTOs;

public class AdminRegistroDto
{
    public string Id { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public string IdCorto { get; set; } = string.Empty;
    public string? ReferenciaPago { get; set; }
    public string? ComprobanteUrl { get; set; }
    public string? NotasAdmin { get; set; }
    public DateTime RegistradoEn { get; set; }
    public DateTime? ConfirmadoEn { get; set; }
    public string NombreAsistente { get; set; } = string.Empty;
    public string EmailAsistente { get; set; } = string.Empty;
    public string EmpresaAsistente { get; set; } = string.Empty;
    public string CargoAsistente { get; set; } = string.Empty;
}

public class RechazarRegistroDto
{
    public string Motivo { get; set; } = string.Empty;
}

public class EventoEstadoDto
{
    public string RegistroId { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public string IdCorto { get; set; } = string.Empty;
}
