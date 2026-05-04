using DotNetEcuador.API.Common;
using DotNetEcuador.API.Infraestructure.Repositories;
using DotNetEcuador.API.Infraestructure.Services.Telegram;
using DotNetEcuador.API.Models.Mentorias;
using DotNetEcuador.API.Models.Mentorias.DTOs;

namespace DotNetEcuador.API.Infraestructure.Services.Mentorias;

public class MentoriaService : IMentoriaService
{
    private readonly IRepository<Institucion> _institucionRepo;
    private readonly IRepository<SolicitudMentoria> _solicitudRepo;
    private readonly ITelegramBotService _telegramBot;
    private readonly IEmailNotificationService _emailService;
    private readonly ILogger<MentoriaService> _logger;

    public MentoriaService(
        IRepository<Institucion> institucionRepo,
        IRepository<SolicitudMentoria> solicitudRepo,
        ITelegramBotService telegramBot,
        IEmailNotificationService emailService,
        ILogger<MentoriaService> logger)
    {
        _institucionRepo = institucionRepo;
        _solicitudRepo = solicitudRepo;
        _telegramBot = telegramBot;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<List<InstitucionDto>> GetInstitucionesAsync()
    {
        var instituciones = await _institucionRepo.GetAllAsync().ConfigureAwait(false);
        return instituciones
            .Where(i => i.IsActive)
            .OrderBy(i => i.Orden)
            .Select(i => new InstitucionDto { Id = i.Id, Nombre = i.Nombre })
            .ToList();
    }

    public async Task<SolicitudMentoriaResponseDto> CrearSolicitudAsync(SolicitudMentoriaRequestDto dto)
    {
        string institucionNombre;

        if (dto.InstitucionId == "otros")
        {
            institucionNombre = dto.OtraInstitucion?.Trim() ?? string.Empty;
        }
        else
        {
            var institucion = await _institucionRepo.GetByIdAsync(dto.InstitucionId).ConfigureAwait(false)
                ?? throw new KeyNotFoundException($"Institución no encontrada.");
            institucionNombre = institucion.Nombre;
        }

        var solicitud = new SolicitudMentoria
        {
            NombreCompleto = dto.NombreCompleto.Trim(),
            Email          = dto.Email.Trim().ToLowerInvariant(),
            Telefono       = dto.Telefono.Trim(),
            InstitucionId  = dto.InstitucionId,
            InstitucionNombre = institucionNombre,
            OtraInstitucion = dto.InstitucionId == "otros" ? dto.OtraInstitucion?.Trim() : null,
            TemaConsulta   = dto.TemaConsulta.Trim(),
            Estado         = EstadoSolicitud.Pendiente,
            CreadaEn       = TimeZoneInfo.ConvertTimeFromUtc(
                                 DateTime.UtcNow,
                                 TimeZoneInfo.FindSystemTimeZoneById(Constants.TimeZones.Ecuador))
        };

        await _solicitudRepo.CreateAsync(solicitud).ConfigureAwait(false);

        _logger.LogInformation("Solicitud de mentoría creada: {Id} para {Email}", solicitud.Id, solicitud.Email);

        _ = EnviarNotificacionesAsync(solicitud);

        return new SolicitudMentoriaResponseDto
        {
            SolicitudId = solicitud.Id,
            Mensaje     = "¡Tu solicitud fue enviada! Te contactaremos pronto."
        };
    }

    private Task EnviarNotificacionesAsync(SolicitudMentoria solicitud)
    {
        _ = Task.Run(async () =>
        {
            try
            {
                await _telegramBot.NotificarNuevaSolicitudMentoriaAsync(solicitud).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Telegram para solicitud de mentoría {Id}", solicitud.Id);
            }

            try
            {
                await _emailService.NotificarAdminAsync(
                    $"Nueva solicitud de mentoría — {solicitud.NombreCompleto}",
                    BuildAdminEmailHtml(solicitud)).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error email admin para solicitud de mentoría {Id}", solicitud.Id);
            }

            try
            {
                await _emailService.EnviarAsync(
                    solicitud.Email,
                    "Hemos recibido tu solicitud de mentoría — DotNet Ecuador",
                    BuildUserEmailHtml(solicitud)).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error email usuario {Email} para solicitud {Id}", solicitud.Email, solicitud.Id);
            }
        });

        return Task.CompletedTask;
    }

    private static string BuildAdminEmailHtml(SolicitudMentoria s)
    {
        var institucion = !string.IsNullOrEmpty(s.OtraInstitucion) ? s.OtraInstitucion : s.InstitucionNombre;
        return $"""
            <div style="font-family:sans-serif;max-width:600px;margin:0 auto">
              <h2 style="color:#1f085a">🎓 Nueva solicitud de mentoría</h2>
              <table style="width:100%;border-collapse:collapse">
                <tr><td style="padding:8px;color:#666;width:140px">Nombre</td><td style="padding:8px;font-weight:600">{s.NombreCompleto}</td></tr>
                <tr style="background:#f5f5f5"><td style="padding:8px;color:#666">Email</td><td style="padding:8px">{s.Email}</td></tr>
                <tr><td style="padding:8px;color:#666">Teléfono</td><td style="padding:8px">{s.Telefono}</td></tr>
                <tr style="background:#f5f5f5"><td style="padding:8px;color:#666">Institución</td><td style="padding:8px">{institucion}</td></tr>
                <tr><td style="padding:8px;color:#666;vertical-align:top">Tema</td><td style="padding:8px">{s.TemaConsulta}</td></tr>
              </table>
            </div>
            """;
    }

    private static string BuildUserEmailHtml(SolicitudMentoria s)
    {
        return $"""
            <div style="font-family:sans-serif;max-width:600px;margin:0 auto">
              <div style="background:linear-gradient(135deg,#100123,#33057a);padding:32px;border-radius:12px 12px 0 0;text-align:center">
                <h1 style="color:#fff;margin:0;font-size:24px">¡Solicitud recibida! 🎓</h1>
              </div>
              <div style="background:#fff;padding:32px;border:1px solid #e5e7eb;border-top:none;border-radius:0 0 12px 12px">
                <p style="color:#374151;font-size:16px">Hola <strong>{s.NombreCompleto}</strong>,</p>
                <p style="color:#374151">Hemos recibido tu solicitud de mentoría. Uno de nuestros mentores revisará tu caso y se pondrá en contacto contigo pronto.</p>
                <div style="background:#f9fafb;border-left:4px solid #7c3aed;padding:16px;border-radius:4px;margin:16px 0">
                  <p style="color:#6b7280;font-size:14px;margin:0 0 4px">Tu tema de consulta:</p>
                  <p style="color:#100123;font-weight:600;margin:0">{s.TemaConsulta}</p>
                </div>
                <p style="color:#6b7280;font-size:14px">Recuerda que las mentorías de DotNet Ecuador son <strong style="color:#059669">completamente gratuitas</strong>.</p>
                <p style="color:#6b7280;font-size:14px;margin-top:24px">— Equipo DotNet Ecuador</p>
              </div>
            </div>
            """;
    }
}
