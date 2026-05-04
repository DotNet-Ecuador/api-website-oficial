using DotNetEcuador.API.Models;
using DotNetEcuador.API.Models.Eventos;
using DotNetEcuador.API.Models.Mentorias;

namespace DotNetEcuador.API.Infraestructure.Services.Telegram;

public class NullTelegramBotService : ITelegramBotService
{
    public Task NotificarComprobanteAsync(Registro registro, Asistente asistente, Evento evento, string rutaArchivo)
        => Task.CompletedTask;

    public Task NotificarPromoAplicadoAsync(Registro registro, Asistente asistente, Evento evento)
        => Task.CompletedTask;

    public Task NotificarNuevoVoluntarioAsync(VolunteerApplication app)
        => Task.CompletedTask;

    public Task NotificarNuevaSolicitudMentoriaAsync(SolicitudMentoria solicitud)
        => Task.CompletedTask;
}
