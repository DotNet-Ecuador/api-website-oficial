using DotNetEcuador.API.Models.Eventos;

namespace DotNetEcuador.API.Infraestructure.Services.Eventos;

public interface IEmailEventoService
{
    Task EnviarConfirmacionPendienteAsync(Registro registro, Asistente asistente, Evento evento);
    Task EnviarConfirmacionPagadaAsync(Registro registro, Asistente asistente, Evento evento, string qrBase64);
    Task EnviarRechazoAsync(Registro registro, Asistente asistente, Evento evento, string motivo);
    Task NotificarAdminAsync(Registro registro, Asistente asistente, Evento evento, string? rutaComprobante = null);
}
