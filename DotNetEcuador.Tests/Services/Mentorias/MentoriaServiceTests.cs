using DotNetEcuador.API.Infraestructure.Repositories;
using DotNetEcuador.API.Infraestructure.Services.Mentorias;
using DotNetEcuador.API.Infraestructure.Services.Telegram;
using DotNetEcuador.API.Infraestructure.Services;
using DotNetEcuador.API.Models.Mentorias;
using DotNetEcuador.API.Models.Mentorias.DTOs;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace DotNetEcuador.Tests.Services.Mentorias;

public class MentoriaServiceTests
{
    private readonly Mock<IRepository<Institucion>> _mockInstitucionRepo;
    private readonly Mock<IRepository<SolicitudMentoria>> _mockSolicitudRepo;
    private readonly Mock<ITelegramBotService> _mockTelegram;
    private readonly Mock<IEmailNotificationService> _mockEmail;
    private readonly Mock<ILogger<MentoriaService>> _mockLogger;
    private readonly IMentoriaService _service;

    public MentoriaServiceTests()
    {
        _mockInstitucionRepo = new Mock<IRepository<Institucion>>();
        _mockSolicitudRepo = new Mock<IRepository<SolicitudMentoria>>();
        _mockTelegram = new Mock<ITelegramBotService>();
        _mockEmail = new Mock<IEmailNotificationService>();
        _mockLogger = new Mock<ILogger<MentoriaService>>();

        _service = new MentoriaService(
            _mockInstitucionRepo.Object,
            _mockSolicitudRepo.Object,
            _mockTelegram.Object,
            _mockEmail.Object,
            _mockLogger.Object);
    }

    private static Institucion InstitucionActiva(string id = "inst-001", string nombre = "ESPOL", int orden = 1) => new()
    {
        Id = id,
        Nombre = nombre,
        Orden = orden,
        IsActive = true
    };

    private static SolicitudMentoriaRequestDto DtoValido(string institucionId = "inst-001") => new()
    {
        NombreCompleto = "Ana García",
        Email = "ana@ejemplo.com",
        Telefono = "+593987654321",
        InstitucionId = institucionId,
        TemaConsulta = "Quiero aprender sobre patrones de diseño"
    };

    // GetInstitucionesAsync

    [Fact]
    public async Task GetInstitucionesAsync_CuandoHayInstituciones_RetornaOrdenadas()
    {
        var instituciones = new List<Institucion>
        {
            InstitucionActiva("id-2", "UCE", 2),
            InstitucionActiva("id-1", "ESPOL", 1),
            InstitucionActiva("id-3", "UTE", 3)
        };

        _mockInstitucionRepo
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(instituciones);

        var result = await _service.GetInstitucionesAsync();

        result.Should().HaveCount(3);
        result[0].Nombre.Should().Be("ESPOL");
        result[1].Nombre.Should().Be("UCE");
        result[2].Nombre.Should().Be("UTE");
    }

    [Fact]
    public async Task GetInstitucionesAsync_CuandoHayInactivas_ExcluirInactivas()
    {
        var instituciones = new List<Institucion>
        {
            InstitucionActiva("id-1", "ESPOL", 1),
            new() { Id = "id-2", Nombre = "Inactiva", Orden = 2, IsActive = false }
        };

        _mockInstitucionRepo
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(instituciones);

        var result = await _service.GetInstitucionesAsync();

        result.Should().HaveCount(1);
        result[0].Nombre.Should().Be("ESPOL");
    }

    [Fact]
    public async Task GetInstitucionesAsync_CuandoNoHayInstituciones_RetornaListaVacia()
    {
        _mockInstitucionRepo
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<Institucion>());

        var result = await _service.GetInstitucionesAsync();

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetInstitucionesAsync_RetornaDto_ConIdYNombre()
    {
        _mockInstitucionRepo
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<Institucion> { InstitucionActiva("mi-id", "PUCE") });

        var result = await _service.GetInstitucionesAsync();

        result[0].Id.Should().Be("mi-id");
        result[0].Nombre.Should().Be("PUCE");
    }

    // CrearSolicitudAsync — InstitucionId conocida

    [Fact]
    public async Task CrearSolicitudAsync_CuandoInstitucionExiste_GuardaYRetornaMensaje()
    {
        _mockInstitucionRepo
            .Setup(r => r.GetByIdAsync("inst-001"))
            .ReturnsAsync(InstitucionActiva("inst-001", "ESPOL"));

        _mockSolicitudRepo
            .Setup(r => r.CreateAsync(It.IsAny<SolicitudMentoria>()))
            .Returns(Task.CompletedTask);

        var result = await _service.CrearSolicitudAsync(DtoValido());

        result.Mensaje.Should().NotBeNullOrEmpty();
        _mockSolicitudRepo.Verify(r => r.CreateAsync(It.IsAny<SolicitudMentoria>()), Times.Once);
    }

    [Fact]
    public async Task CrearSolicitudAsync_CuandoInstitucionExiste_GurdaConDatosCorrectos()
    {
        _mockInstitucionRepo
            .Setup(r => r.GetByIdAsync("inst-001"))
            .ReturnsAsync(InstitucionActiva("inst-001", "ESPOL"));

        SolicitudMentoria? guardada = null;
        _mockSolicitudRepo
            .Setup(r => r.CreateAsync(It.IsAny<SolicitudMentoria>()))
            .Callback<SolicitudMentoria>(s => guardada = s)
            .Returns(Task.CompletedTask);

        await _service.CrearSolicitudAsync(DtoValido());

        guardada.Should().NotBeNull();
        guardada!.NombreCompleto.Should().Be("Ana García");
        guardada.Email.Should().Be("ana@ejemplo.com");
        guardada.InstitucionNombre.Should().Be("ESPOL");
        guardada.Estado.Should().Be(EstadoSolicitud.Pendiente);
        guardada.OtraInstitucion.Should().BeNull();
    }

    [Fact]
    public async Task CrearSolicitudAsync_CuandoInstitucionNoExiste_LanzaKeyNotFoundException()
    {
        _mockInstitucionRepo
            .Setup(r => r.GetByIdAsync("no-existe"))
            .ReturnsAsync((Institucion?)null);

        var dto = DtoValido("no-existe");

        await _service.Invoking(s => s.CrearSolicitudAsync(dto))
            .Should().ThrowAsync<KeyNotFoundException>();
    }

    // CrearSolicitudAsync — InstitucionId = "otros"

    [Fact]
    public async Task CrearSolicitudAsync_CuandoOtros_UsaNombreOtraInstitucion()
    {
        _mockSolicitudRepo
            .Setup(r => r.CreateAsync(It.IsAny<SolicitudMentoria>()))
            .Returns(Task.CompletedTask);

        SolicitudMentoria? guardada = null;
        _mockSolicitudRepo
            .Setup(r => r.CreateAsync(It.IsAny<SolicitudMentoria>()))
            .Callback<SolicitudMentoria>(s => guardada = s)
            .Returns(Task.CompletedTask);

        var dto = DtoValido("otros");
        dto.OtraInstitucion = "Instituto Privado";

        await _service.CrearSolicitudAsync(dto);

        guardada!.InstitucionNombre.Should().Be("Instituto Privado");
        guardada.OtraInstitucion.Should().Be("Instituto Privado");
        _mockInstitucionRepo.Verify(r => r.GetByIdAsync(It.IsAny<string>()), Times.Never);
    }

    // CrearSolicitudAsync — email normalizado

    [Fact]
    public async Task CrearSolicitudAsync_NormalizaEmail_AMinusculas()
    {
        _mockInstitucionRepo
            .Setup(r => r.GetByIdAsync("inst-001"))
            .ReturnsAsync(InstitucionActiva());

        SolicitudMentoria? guardada = null;
        _mockSolicitudRepo
            .Setup(r => r.CreateAsync(It.IsAny<SolicitudMentoria>()))
            .Callback<SolicitudMentoria>(s => guardada = s)
            .Returns(Task.CompletedTask);

        var dto = DtoValido();
        dto.Email = "  ANA@EJEMPLO.COM  ";

        await _service.CrearSolicitudAsync(dto);

        guardada!.Email.Should().Be("ana@ejemplo.com");
    }
}
