using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;
using DotNetEcuador.API.Models.Eventos;

namespace DotNetEcuador.API.Infraestructure.Services.Eventos;

public class EmailEventoService : IEmailEventoService
{
    private readonly EmailSettings _settings;
    private readonly string _templatesPath;

    public EmailEventoService(IConfiguration configuration)
    {
        _settings = new EmailSettings
        {
            SmtpHost = Environment.GetEnvironmentVariable("EMAIL_SMTP_HOST") ?? string.Empty,
            SmtpPort = int.TryParse(Environment.GetEnvironmentVariable("EMAIL_SMTP_PORT"), out var port) ? port : 587,
            Username = Environment.GetEnvironmentVariable("EMAIL_SMTP_USERNAME") ?? string.Empty,
            Password = Environment.GetEnvironmentVariable("EMAIL_SMTP_PASSWORD") ?? string.Empty,
            FromName = Environment.GetEnvironmentVariable("EMAIL_FROM_NAME") ?? "DotNet Ecuador",
            FromAddress = Environment.GetEnvironmentVariable("EMAIL_FROM_ADDRESS") ?? string.Empty,
            AdminEmail = Environment.GetEnvironmentVariable("EMAIL_ADMIN_NOTIFICATION") ?? string.Empty
        };
        _templatesPath = Path.Combine(AppContext.BaseDirectory, "Templates", "Email");
    }

    public async Task EnviarConfirmacionPendienteAsync(Registro registro, Asistente asistente, Evento evento)
    {
        var html = await LeerTemplateAsync("confirmacion-pendiente.html").ConfigureAwait(false);
        html = html
            .Replace("{{nombre}}", asistente.Nombre)
            .Replace("{{evento}}", evento.Nombre)
            .Replace("{{fecha}}", evento.FechaEvento.ToString("dd/MM/yyyy"))
            .Replace("{{referencia}}", registro.IdCorto);

        await EnviarAsync(asistente.Email, $"Tu registro para {evento.Nombre} está en revisión", html).ConfigureAwait(false);
    }

    public async Task EnviarConfirmacionPagadaAsync(Registro registro, Asistente asistente, Evento evento, string qrBase64)
    {
        var html = await LeerTemplateAsync("confirmacion-pagada.html").ConfigureAwait(false);
        html = html
            .Replace("{{nombre}}", asistente.Nombre)
            .Replace("{{evento}}", evento.Nombre)
            .Replace("{{fecha}}", evento.FechaEvento.ToString("dd/MM/yyyy"))
            .Replace("{{lugar}}", evento.Lugar)
            .Replace("{{qr_image_base64}}", qrBase64);

        await EnviarAsync(asistente.Email, $"¡Confirmado! Tu entrada para {evento.Nombre}", html).ConfigureAwait(false);
    }

    public async Task EnviarRechazoAsync(Registro registro, Asistente asistente, Evento evento, string motivo)
    {
        var html = await LeerTemplateAsync("rechazo.html").ConfigureAwait(false);
        html = html
            .Replace("{{nombre}}", asistente.Nombre)
            .Replace("{{evento}}", evento.Nombre)
            .Replace("{{motivo}}", motivo)
            .Replace("{{contacto}}", _settings.AdminEmail);

        await EnviarAsync(asistente.Email, $"Actualización sobre tu registro para {evento.Nombre}", html).ConfigureAwait(false);
    }

    public async Task NotificarAdminAsync(Registro registro, Asistente asistente, Evento evento, string? rutaComprobante = null)
    {
        if (string.IsNullOrEmpty(_settings.AdminEmail)) return;

        var html = $@"<h3>Nuevo comprobante subido</h3>
<p><strong>Evento:</strong> {evento.Nombre}</p>
<p><strong>Asistente:</strong> {asistente.Nombre} ({asistente.Email})</p>
<p><strong>Referencia:</strong> {registro.ReferenciaPago}</p>
<p><strong>ID Corto:</strong> {registro.IdCorto}</p>";

        await EnviarConAdjuntoAsync(
            _settings.AdminEmail,
            $"[Acción requerida] Comprobante de {asistente.Nombre} — {evento.Nombre}",
            html,
            rutaComprobante).ConfigureAwait(false);
    }

    private Task EnviarAsync(string destinatario, string asunto, string htmlBody)
        => EnviarConAdjuntoAsync(destinatario, asunto, htmlBody, null);

    private async Task EnviarConAdjuntoAsync(string destinatario, string asunto, string htmlBody, string? rutaAdjunto)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_settings.FromName, _settings.FromAddress));
        message.To.Add(MailboxAddress.Parse(destinatario));
        message.Subject = asunto;

        var body = new TextPart("html") { Text = htmlBody };

        if (!string.IsNullOrEmpty(rutaAdjunto) && File.Exists(rutaAdjunto))
        {
            var multipart = new MimeKit.Multipart("mixed");
            multipart.Add(body);

            var adjunto = new MimeKit.MimePart(ObtenerContentType(rutaAdjunto))
            {
                Content = new MimeKit.MimeContent(File.OpenRead(rutaAdjunto)),
                ContentDisposition = new MimeKit.ContentDisposition(MimeKit.ContentDisposition.Attachment),
                ContentTransferEncoding = MimeKit.ContentEncoding.Base64,
                FileName = Path.GetFileName(rutaAdjunto)
            };
            multipart.Add(adjunto);
            message.Body = multipart;
        }
        else
        {
            message.Body = body;
        }

        using var client = new SmtpClient();
        await client.ConnectAsync(_settings.SmtpHost, _settings.SmtpPort, SecureSocketOptions.StartTls).ConfigureAwait(false);
        await client.AuthenticateAsync(_settings.Username, _settings.Password).ConfigureAwait(false);
        await client.SendAsync(message).ConfigureAwait(false);
        await client.DisconnectAsync(true).ConfigureAwait(false);
    }

    private static string ObtenerContentType(string ruta)
    {
        var ext = Path.GetExtension(ruta).ToLowerInvariant();
        return ext switch
        {
            ".pdf" => "application/pdf",
            ".png" => "image/png",
            ".jpg" => "image/jpeg",
            _      => "application/octet-stream"
        };
    }

    private async Task<string> LeerTemplateAsync(string nombreArchivo)
    {
        var ruta = Path.Combine(_templatesPath, nombreArchivo);
        return await File.ReadAllTextAsync(ruta).ConfigureAwait(false);
    }

    private sealed class EmailSettings
    {
        public string SmtpHost { get; set; } = string.Empty;
        public int SmtpPort { get; set; } = 587;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FromName { get; set; } = string.Empty;
        public string FromAddress { get; set; } = string.Empty;
        public string AdminEmail { get; set; } = string.Empty;
    }
}
