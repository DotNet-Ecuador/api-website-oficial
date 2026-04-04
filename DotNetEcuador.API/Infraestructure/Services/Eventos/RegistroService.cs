using DotNetEcuador.API.Common;
using DotNetEcuador.API.Infraestructure.Repositories;
using DotNetEcuador.API.Infraestructure.Services.Telegram;
using DotNetEcuador.API.Models.Common;
using DotNetEcuador.API.Models.Eventos;
using DotNetEcuador.API.Models.Eventos.DTOs;
using Microsoft.Extensions.Logging;

namespace DotNetEcuador.API.Infraestructure.Services.Eventos;

public class RegistroService : IRegistroService
{
    private readonly IRepository<Registro> _registroRepo;
    private readonly IRepository<Asistente> _asistenteRepo;
    private readonly IEventoService _eventoService;
    private readonly IEmailEventoService _emailService;
    private readonly IQrService _qrService;
    private readonly IRepository<EmailLog> _emailLogRepo;
    private readonly IFileStorageService _fileStorage;
    private readonly ITelegramBotService _telegramBot;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<RegistroService> _logger;
    private readonly string _uploadsPath;

    private static readonly char[] IdCortoChars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789".ToCharArray();

    public RegistroService(
        IRepository<Registro> registroRepo,
        IRepository<Asistente> asistenteRepo,
        IEventoService eventoService,
        IEmailEventoService emailService,
        IQrService qrService,
        IRepository<EmailLog> emailLogRepo,
        IFileStorageService fileStorage,
        ITelegramBotService telegramBot,
        IServiceScopeFactory scopeFactory,
        ILogger<RegistroService> logger)
    {
        _registroRepo = registroRepo;
        _asistenteRepo = asistenteRepo;
        _eventoService = eventoService;
        _emailService = emailService;
        _qrService = qrService;
        _emailLogRepo = emailLogRepo;
        _fileStorage = fileStorage;
        _telegramBot = telegramBot;
        _scopeFactory = scopeFactory;
        _logger = logger;
        _uploadsPath = Environment.GetEnvironmentVariable("UPLOADS_PATH")
            ?? Path.Combine(AppContext.BaseDirectory, "uploads", "comprobantes");
    }

    public async Task<RegistroResponseDto> CrearRegistroAsync(RegistroRequestDto request)
    {
        var evento = await _eventoService.GetBySlugAsync(request.EventoSlug).ConfigureAwait(false)
            ?? throw new KeyNotFoundException($"Evento '{request.EventoSlug}' no encontrado o inactivo.");

        var cupos = await _eventoService.GetCuposDisponiblesAsync(evento.Id).ConfigureAwait(false);
        if (cupos <= 0)
            throw new InvalidOperationException("El evento no tiene cupos disponibles.");

        var asistente = await _asistenteRepo.FindAsync(a => a.Email == request.Email).ConfigureAwait(false);
        if (asistente is null)
        {
            asistente = new Asistente
            {
                Nombre = request.Nombre,
                Email = request.Email,
                Empresa = request.Empresa,
                Cargo = request.Cargo,
                Telefono = request.Telefono,
                AceptaMarketing = request.AceptaMarketing
            };
            await _asistenteRepo.CreateAsync(asistente).ConfigureAwait(false);
        }

        var registroExistente = await _registroRepo
            .FindAsync(r => r.EventoId == evento.Id && r.AsistenteId == asistente.Id && r.Estado != EstadoRegistro.Cancelado)
            .ConfigureAwait(false);

        if (registroExistente is not null)
            throw new InvalidOperationException($"El email '{request.Email}' ya está registrado en este evento.");

        var ecuadorZone = TimeZoneInfo.FindSystemTimeZoneById(Constants.TimeZones.Ecuador);
        var registro = new Registro
        {
            EventoId = evento.Id,
            AsistenteId = asistente.Id,
            Estado = EstadoRegistro.Pendiente,
            IdCorto = GenerarIdCorto(),
            TokenQr = Guid.NewGuid().ToString(),
            SessionToken = Guid.NewGuid().ToString(),
            RegistradoEn = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, ecuadorZone)
        };
        await _registroRepo.CreateAsync(registro).ConfigureAwait(false);

        _logger.LogInformation("Registro creado: {IdCorto} para {Email} en evento {Slug}", registro.IdCorto, request.Email, request.EventoSlug);

        return new RegistroResponseDto
        {
            RegistroId = registro.Id,
            IdCorto = registro.IdCorto,
            SessionToken = registro.SessionToken,
            Monto = evento.Precio,
            NombreEvento = evento.Nombre
        };
    }

    public async Task SubirComprobanteAsync(string registroId, string sessionToken, ComprobanteRequestDto dto)
    {
        var registro = await _registroRepo.GetByIdAsync(registroId).ConfigureAwait(false)
            ?? throw new KeyNotFoundException("Registro no encontrado.");

        if (registro.SessionToken != sessionToken)
            throw new UnauthorizedAccessException("Session token inválido.");

        string? rutaRelativa = null;
        if (dto.Comprobante is not null && dto.Comprobante.Length > 0)
        {
            rutaRelativa = await _fileStorage.GuardarComprobanteAsync(dto.Comprobante, registroId).ConfigureAwait(false);
            registro.ComprobanteUrl = rutaRelativa;
        }

        registro.ReferenciaPago = dto.ReferenciaPago;
        await _registroRepo.UpdateAsync(registroId, registro).ConfigureAwait(false);

        var asistente = await _asistenteRepo.GetByIdAsync(registro.AsistenteId).ConfigureAwait(false);
        var eventos = await _eventoService.GetAllAsync().ConfigureAwait(false);
        var evento = eventos.FirstOrDefault(e => e.Id == registro.EventoId);

        if (asistente is null || evento is null) return;

        _ = EnviarNotificacionesAsync(registro, asistente, evento, rutaRelativa);
    }

    private Task EnviarNotificacionesAsync(Registro registro, Asistente asistente, Evento evento, string? rutaRelativa)
    {
        var rutaFisica = rutaRelativa is not null
            ? Path.Combine(_uploadsPath, rutaRelativa.Replace('/', Path.DirectorySeparatorChar))
            : null;

        _ = Task.Run(async () =>
        {
            try
            {
                await _telegramBot.NotificarComprobanteAsync(registro, asistente, evento, rutaRelativa ?? string.Empty).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enviando notificación Telegram para registro {IdCorto}", registro.IdCorto);
            }

            try
            {
                using var scope = _scopeFactory.CreateScope();
                var emailService = scope.ServiceProvider.GetRequiredService<IEmailEventoService>();
                await emailService.EnviarConfirmacionPendienteAsync(registro, asistente, evento).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enviando email confirmación pendiente a {Email}", asistente.Email);
            }

            try
            {
                using var scope = _scopeFactory.CreateScope();
                var emailService = scope.ServiceProvider.GetRequiredService<IEmailEventoService>();
                await emailService.NotificarAdminAsync(registro, asistente, evento, rutaFisica).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enviando email admin para registro {IdCorto}", registro.IdCorto);
            }
        });

        return Task.CompletedTask;
    }

    public async Task<EventoEstadoDto> GetEstadoAsync(string registroId)
    {
        var registro = await _registroRepo.GetByIdAsync(registroId).ConfigureAwait(false)
            ?? throw new KeyNotFoundException("Registro no encontrado.");

        return new EventoEstadoDto
        {
            RegistroId = registro.Id,
            Estado = registro.Estado,
            IdCorto = registro.IdCorto
        };
    }

    public async Task<PagedResponse<AdminRegistroDto>> GetAdminRegistrosAsync(PagedRequest request, string? eventoId, string? estado)
    {
        var todos = await _registroRepo.GetAllAsync().ConfigureAwait(false);

        var filtrados = todos
            .Where(r => string.IsNullOrEmpty(eventoId) || r.EventoId == eventoId)
            .Where(r => string.IsNullOrEmpty(estado) || r.Estado == estado)
            .ToList();

        var total = filtrados.Count;
        var items = filtrados
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        var dtos = new List<AdminRegistroDto>();
        foreach (var r in items)
        {
            var asistente = await _asistenteRepo.GetByIdAsync(r.AsistenteId).ConfigureAwait(false);
            dtos.Add(new AdminRegistroDto
            {
                Id = r.Id,
                Estado = r.Estado,
                IdCorto = r.IdCorto,
                ReferenciaPago = r.ReferenciaPago,
                ComprobanteUrl = r.ComprobanteUrl,
                NotasAdmin = r.NotasAdmin,
                RegistradoEn = r.RegistradoEn,
                ConfirmadoEn = r.ConfirmadoEn,
                NombreAsistente = asistente?.Nombre ?? string.Empty,
                EmailAsistente = asistente?.Email ?? string.Empty,
                EmpresaAsistente = asistente?.Empresa ?? string.Empty,
                CargoAsistente = asistente?.Cargo ?? string.Empty
            });
        }

        return new PagedResponse<AdminRegistroDto>
        {
            Data = dtos,
            TotalCount = total,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }

    public async Task AprobarAsync(string registroId, string notasAdmin)
    {
        var registro = await _registroRepo.GetByIdAsync(registroId).ConfigureAwait(false)
            ?? throw new KeyNotFoundException("Registro no encontrado.");

        var ecuadorZone = TimeZoneInfo.FindSystemTimeZoneById(Constants.TimeZones.Ecuador);
        registro.Estado = EstadoRegistro.Pagado;
        registro.ConfirmadoEn = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, ecuadorZone);
        registro.NotasAdmin = notasAdmin;
        await _registroRepo.UpdateAsync(registroId, registro).ConfigureAwait(false);

        var asistente = await _asistenteRepo.GetByIdAsync(registro.AsistenteId).ConfigureAwait(false);
        if (asistente is null) return;

        var qrBase64 = _qrService.GenerarQrBase64(registro.TokenQr);

        var eventos = await _eventoService.GetAllAsync().ConfigureAwait(false);
        var evento = eventos.FirstOrDefault(e => e.Id == registro.EventoId);
        if (evento is null) return;

        await EnviarEmailSeguroAsync(async () =>
            await _emailService.EnviarConfirmacionPagadaAsync(registro, asistente, evento, qrBase64).ConfigureAwait(false),
            registroId, TipoEmail.ConfirmacionPagada, asistente.Email).ConfigureAwait(false);

        _logger.LogInformation("Registro {IdCorto} aprobado para {Email}", registro.IdCorto, asistente.Email);
    }

    public async Task RechazarAsync(string registroId, string motivo)
    {
        var registro = await _registroRepo.GetByIdAsync(registroId).ConfigureAwait(false)
            ?? throw new KeyNotFoundException("Registro no encontrado.");

        registro.Estado = EstadoRegistro.Rechazado;
        await _registroRepo.UpdateAsync(registroId, registro).ConfigureAwait(false);

        var asistente = await _asistenteRepo.GetByIdAsync(registro.AsistenteId).ConfigureAwait(false);
        if (asistente is null) return;

        var eventos = await _eventoService.GetAllAsync().ConfigureAwait(false);
        var evento = eventos.FirstOrDefault(e => e.Id == registro.EventoId);
        if (evento is null) return;

        await EnviarEmailSeguroAsync(async () =>
            await _emailService.EnviarRechazoAsync(registro, asistente, evento, motivo).ConfigureAwait(false),
            registroId, TipoEmail.Rechazo, asistente.Email).ConfigureAwait(false);

        _logger.LogInformation("Registro {IdCorto} rechazado", registro.IdCorto);
    }

    public async Task<RecuperarRegistroDto> RecuperarRegistroAsync(string email, string eventoSlug)
    {
        var evento = await _eventoService.GetBySlugAsync(eventoSlug).ConfigureAwait(false)
            ?? throw new KeyNotFoundException("No se encontró un registro con ese correo para este evento.");

        var asistente = await _asistenteRepo.FindAsync(a => a.Email == email).ConfigureAwait(false)
            ?? throw new KeyNotFoundException("No se encontró un registro con ese correo para este evento.");

        var registro = await _registroRepo
            .FindAsync(r => r.EventoId == evento.Id && r.AsistenteId == asistente.Id && r.Estado != EstadoRegistro.Cancelado)
            .ConfigureAwait(false)
            ?? throw new KeyNotFoundException("No se encontró un registro con ese correo para este evento.");

        string? sessionToken = null;

        if (registro.Estado == EstadoRegistro.Pendiente)
            sessionToken = registro.SessionToken;

        return new RecuperarRegistroDto
        {
            RegistroId = registro.Id,
            IdCorto = registro.IdCorto,
            Monto = evento.Precio,
            NombreEvento = evento.Nombre,
            Estado = registro.Estado,
            SessionToken = sessionToken
        };
    }

    private static string GenerarIdCorto()
    {
        var random = new Random();
        return new string(Enumerable.Range(0, 6).Select(_ => IdCortoChars[random.Next(IdCortoChars.Length)]).ToArray());
    }

    private async Task EnviarEmailSeguroAsync(Func<Task> enviar, string registroId, string tipo, string destinatario)
    {
        var log = new EmailLog
        {
            RegistroId = registroId,
            Tipo = tipo,
            Destinatario = destinatario
        };
        try
        {
            await enviar().ConfigureAwait(false);
            log.Exitoso = true;
        }
        catch (Exception ex)
        {
            log.Exitoso = false;
            log.Error = ex.Message;
            _logger.LogError(ex, "Error enviando email tipo {Tipo} a {Destinatario}", tipo, destinatario);
        }
        finally
        {
            await _emailLogRepo.CreateAsync(log).ConfigureAwait(false);
        }
    }
}
