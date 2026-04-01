using DotNetEcuador.API.Infraestructure.Repositories;
using DotNetEcuador.API.Infraestructure.Services;
using DotNetEcuador.API.Infraestructure.Services.Eventos;
using DotNetEcuador.API.Infraestructure.Services.Telegram;
using DotNetEcuador.API.Models.Eventos;
using DotNetEcuador.API.Models.Eventos.DTOs;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace DotNetEcuador.Tests.Services.Eventos;

public class RegistroServiceTests
{
    private readonly Mock<IRepository<Registro>> _mockRegistroRepo;
    private readonly Mock<IRepository<Asistente>> _mockAsistenteRepo;
    private readonly Mock<IEventoService> _mockEventoService;
    private readonly Mock<IEmailEventoService> _mockEmailService;
    private readonly Mock<IQrService> _mockQrService;
    private readonly Mock<IRepository<EmailLog>> _mockEmailLogRepo;
    private readonly Mock<IFileStorageService> _mockFileStorage;
    private readonly Mock<ITelegramBotService> _mockTelegramBot;
    private readonly Mock<ILogger<RegistroService>> _mockLogger;
    private readonly IRegistroService _service;

    public RegistroServiceTests()
    {
        _mockRegistroRepo = new Mock<IRepository<Registro>>();
        _mockAsistenteRepo = new Mock<IRepository<Asistente>>();
        _mockEventoService = new Mock<IEventoService>();
        _mockEmailService = new Mock<IEmailEventoService>();
        _mockQrService = new Mock<IQrService>();
        _mockEmailLogRepo = new Mock<IRepository<EmailLog>>();
        _mockFileStorage = new Mock<IFileStorageService>();
        _mockTelegramBot = new Mock<ITelegramBotService>();
        _mockLogger = new Mock<ILogger<RegistroService>>();

        _service = new RegistroService(
            _mockRegistroRepo.Object,
            _mockAsistenteRepo.Object,
            _mockEventoService.Object,
            _mockEmailService.Object,
            _mockQrService.Object,
            _mockEmailLogRepo.Object,
            _mockFileStorage.Object,
            _mockTelegramBot.Object,
            _mockLogger.Object);
    }

    private static Evento EventoConCupo(int cupos = 50) => new()
    {
        Id = "507f1f77bcf86cd799439011",
        Slug = "meetup-mayo",
        Nombre = "Meetup Mayo",
        Precio = 5.00m,
        CapacidadMaxima = cupos,
        Activo = true,
        DatosTransferencia = new DatosTransferencia
        {
            Banco = "Banco Pichincha",
            TipoCuenta = "Ahorro",
            NumeroCuenta = "123456789",
            Titular = "DotNet Ecuador"
        }
    };

    [Fact]
    public async Task CrearRegistro_CuandoEventoNoExiste_LanzaKeyNotFoundException()
    {
        _mockEventoService.Setup(s => s.GetBySlugAsync("no-existe")).ReturnsAsync((Evento?)null);
        var request = new RegistroRequestDto { EventoSlug = "no-existe", Nombre = "Juan", Email = "juan@test.com" };

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.CrearRegistroAsync(request));
    }

    [Fact]
    public async Task CrearRegistro_CuandoEventoSinCupo_LanzaInvalidOperationException()
    {
        var evento = EventoConCupo(0);
        _mockEventoService.Setup(s => s.GetBySlugAsync("meetup-mayo")).ReturnsAsync(evento);
        _mockEventoService.Setup(s => s.GetCuposDisponiblesAsync(evento.Id)).ReturnsAsync(0);

        var request = new RegistroRequestDto { EventoSlug = "meetup-mayo", Nombre = "Juan", Email = "juan@test.com" };

        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CrearRegistroAsync(request));
    }

    [Fact]
    public async Task CrearRegistro_CuandoEmailYaRegistradoEnEvento_LanzaInvalidOperationException()
    {
        var evento = EventoConCupo();
        _mockEventoService.Setup(s => s.GetBySlugAsync("meetup-mayo")).ReturnsAsync(evento);
        _mockEventoService.Setup(s => s.GetCuposDisponiblesAsync(evento.Id)).ReturnsAsync(10);

        var asistente = new Asistente { Id = "507f1f77bcf86cd799439012", Email = "juan@test.com" };
        _mockAsistenteRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Asistente, bool>>>()))
            .ReturnsAsync(asistente);

        var registroExistente = new Registro { EventoId = evento.Id, AsistenteId = asistente.Id, Estado = EstadoRegistro.Pendiente };
        _mockRegistroRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Registro, bool>>>()))
            .ReturnsAsync(registroExistente);

        var request = new RegistroRequestDto { EventoSlug = "meetup-mayo", Nombre = "Juan", Email = "juan@test.com" };

        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CrearRegistroAsync(request));
    }

    [Fact]
    public async Task CrearRegistro_CuandoValido_RetornaResponseConIdCortoYSessionToken()
    {
        var evento = EventoConCupo();
        _mockEventoService.Setup(s => s.GetBySlugAsync("meetup-mayo")).ReturnsAsync(evento);
        _mockEventoService.Setup(s => s.GetCuposDisponiblesAsync(evento.Id)).ReturnsAsync(10);

        _mockAsistenteRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Asistente, bool>>>()))
            .ReturnsAsync((Asistente?)null);
        _mockAsistenteRepo.Setup(r => r.CreateAsync(It.IsAny<Asistente>())).Returns(Task.CompletedTask);

        _mockRegistroRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Registro, bool>>>()))
            .ReturnsAsync((Registro?)null);
        _mockRegistroRepo.Setup(r => r.CreateAsync(It.IsAny<Registro>())).Returns(Task.CompletedTask);

        var request = new RegistroRequestDto
        {
            EventoSlug = "meetup-mayo",
            Nombre = "Juan Pérez",
            Email = "juan@test.com",
            Empresa = "Tech Corp",
            Cargo = "Dev"
        };

        var result = await _service.CrearRegistroAsync(request);

        result.Should().NotBeNull();
        result.IdCorto.Should().HaveLength(6);
        result.SessionToken.Should().NotBeNullOrEmpty();
        result.Monto.Should().Be(5.00m);
        result.NombreEvento.Should().Be("Meetup Mayo");
    }

    [Fact]
    public async Task SubirComprobante_CuandoSessionTokenInvalido_LanzaUnauthorizedAccessException()
    {
        var registro = new Registro
        {
            Id = "507f1f77bcf86cd799439011",
            SessionToken = "token-valido",
            RegistradoEn = DateTime.UtcNow,
            Estado = EstadoRegistro.Pendiente
        };
        _mockRegistroRepo.Setup(r => r.GetByIdAsync("507f1f77bcf86cd799439011")).ReturnsAsync(registro);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _service.SubirComprobanteAsync("507f1f77bcf86cd799439011", "token-incorrecto", new ComprobanteRequestDto()));
    }

    [Fact]
    public async Task SubirComprobante_SessionTokenSinExpiracion_PermiteSubir()
    {
        var registroId = "507f1f77bcf86cd799439011";
        var registro = new Registro
        {
            Id = registroId,
            AsistenteId = "507f1f77bcf86cd799439012",
            EventoId = "507f1f77bcf86cd799439013",
            SessionToken = "token-valido",
            RegistradoEn = DateTime.UtcNow.AddDays(-7),
            Estado = EstadoRegistro.Pendiente
        };
        _mockRegistroRepo.Setup(r => r.GetByIdAsync(registroId)).ReturnsAsync(registro);
        _mockRegistroRepo.Setup(r => r.UpdateAsync(registroId, It.IsAny<Registro>())).Returns(Task.CompletedTask);
        _mockAsistenteRepo.Setup(r => r.GetByIdAsync(registro.AsistenteId)).ReturnsAsync((Asistente?)null);
        _mockEventoService.Setup(s => s.GetAllAsync()).ReturnsAsync(new List<Evento>());

        await _service.SubirComprobanteAsync(registroId, "token-valido", new ComprobanteRequestDto());
    }

    [Fact]
    public async Task GetEstado_CuandoRegistroNoExiste_LanzaKeyNotFoundException()
    {
        _mockRegistroRepo.Setup(r => r.GetByIdAsync("no-existe")).ReturnsAsync((Registro?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.GetEstadoAsync("no-existe"));
    }

    [Fact]
    public async Task GetEstado_CuandoExiste_RetornaEstado()
    {
        var registro = new Registro
        {
            Id = "507f1f77bcf86cd799439011",
            Estado = EstadoRegistro.Pendiente,
            IdCorto = "DN1234"
        };
        _mockRegistroRepo.Setup(r => r.GetByIdAsync("507f1f77bcf86cd799439011")).ReturnsAsync(registro);

        var result = await _service.GetEstadoAsync("507f1f77bcf86cd799439011");

        result.Estado.Should().Be(EstadoRegistro.Pendiente);
        result.IdCorto.Should().Be("DN1234");
    }
}
