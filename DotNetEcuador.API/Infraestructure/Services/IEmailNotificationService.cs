namespace DotNetEcuador.API.Infraestructure.Services;

public interface IEmailNotificationService
{
    Task EnviarAsync(string destinatario, string asunto, string htmlBody);
    Task NotificarAdminAsync(string asunto, string htmlBody);
}
