using DotNetEcuador.API.Models;
using DotNetEcuador.API.Models.Eventos;
using DotNetEcuador.API.Models.Mentorias;

namespace DotNetEcuador.API.Infraestructure.Services.Telegram;

public interface ITelegramBotService
{
    Task NotificarComprobanteAsync(Registro registro, Asistente asistente, Evento evento, string rutaArchivo);
    Task NotificarPromoAplicadoAsync(Registro registro, Asistente asistente, Evento evento);
    Task NotificarNuevoVoluntarioAsync(VolunteerApplication app);
    Task NotificarNuevaSolicitudMentoriaAsync(SolicitudMentoria solicitud);
}
