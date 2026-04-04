namespace DotNetEcuador.API.Infraestructure.Services.Telegram;

public enum TelegramAccion { Aprobar, Rechazar }

public class PendingAction
{
    public TelegramAccion Accion { get; set; }
    public string RegistroId { get; set; } = string.Empty;
    public string IdCorto { get; set; } = string.Empty;
    public string NombreAsistente { get; set; } = string.Empty;
    public bool EsperandoMotivo { get; set; }
}
