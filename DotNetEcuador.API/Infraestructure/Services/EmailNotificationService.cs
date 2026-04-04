using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace DotNetEcuador.API.Infraestructure.Services;

public class EmailNotificationService : IEmailNotificationService
{
    private readonly string _smtpHost;
    private readonly int _smtpPort;
    private readonly string _username;
    private readonly string _password;
    private readonly string _fromName;
    private readonly string _fromAddress;
    private readonly string _adminEmail;

    public EmailNotificationService()
    {
        _smtpHost = Environment.GetEnvironmentVariable("EMAIL_SMTP_HOST") ?? string.Empty;
        _smtpPort = int.TryParse(Environment.GetEnvironmentVariable("EMAIL_SMTP_PORT"), out var port) ? port : 587;
        _username = Environment.GetEnvironmentVariable("EMAIL_SMTP_USERNAME") ?? string.Empty;
        _password = Environment.GetEnvironmentVariable("EMAIL_SMTP_PASSWORD") ?? string.Empty;
        _fromName = Environment.GetEnvironmentVariable("EMAIL_FROM_NAME") ?? "DotNet Ecuador";
        _fromAddress = Environment.GetEnvironmentVariable("EMAIL_FROM_ADDRESS") ?? string.Empty;
        _adminEmail = Environment.GetEnvironmentVariable("EMAIL_ADMIN_NOTIFICATION") ?? string.Empty;
    }

    public async Task EnviarAsync(string destinatario, string asunto, string htmlBody)
    {
        if (string.IsNullOrEmpty(_smtpHost) || string.IsNullOrEmpty(_fromAddress))
            return;

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_fromName, _fromAddress));
        message.To.Add(MailboxAddress.Parse(destinatario));
        message.Subject = asunto;
        message.Body = new TextPart("html") { Text = htmlBody };

        using var client = new SmtpClient();
        await client.ConnectAsync(_smtpHost, _smtpPort, SecureSocketOptions.StartTls).ConfigureAwait(false);
        await client.AuthenticateAsync(_username, _password).ConfigureAwait(false);
        await client.SendAsync(message).ConfigureAwait(false);
        await client.DisconnectAsync(true).ConfigureAwait(false);
    }

    public async Task NotificarAdminAsync(string asunto, string htmlBody)
    {
        if (string.IsNullOrEmpty(_adminEmail))
            return;

        await EnviarAsync(_adminEmail, asunto, htmlBody).ConfigureAwait(false);
    }
}
