using DotNetEcuador.API.Controllers.V1;
using DotNetEcuador.API.Infraestructure.Services.Mentorias;
using DotNetEcuador.API.Models.Mentorias.DTOs;
using DotNetEcuador.API.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace DotNetEcuador.Tests.Controllers;

public class MentoriasControllerTests
{
    private readonly Mock<IMentoriaService> _mockService;
    private readonly Mock<IMessageService> _mockMessageService;
    private readonly Mock<ILogger<MentoriasController>> _mockLogger;
    private readonly MentoriasController _controller;

    public MentoriasControllerTests()
    {
        _mockService = new Mock<IMentoriaService>();
        _mockMessageService = new Mock<IMessageService>();
        _mockLogger = new Mock<ILogger<MentoriasController>>();

        _controller = new MentoriasController(
            _mockService.Object,
            _mockMessageService.Object,
            _mockLogger.Object);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
    }

    private static SolicitudMentoriaRequestDto RequestValido() => new()
    {
        NombreCompleto = "Carlos López",
        Email = "carlos@ejemplo.com",
        Telefono = "+593987654321",
        InstitucionId = "inst-001",
        TemaConsulta = "Quiero aprender sobre arquitectura limpia"
    };

    // GetInstituciones

    [Fact]
    public async Task GetInstituciones_CuandoServicioRetornaLista_RetornaStatus200()
    {
        var instituciones = new List<InstitucionDto>
        {
            new() { Id = "id-1", Nombre = "ESPOL" },
            new() { Id = "id-2", Nombre = "UCE" }
        };

        _mockService
            .Setup(s => s.GetInstitucionesAsync())
            .ReturnsAsync(instituciones);

        var result = await _controller.GetInstituciones();

        result.Should().BeOfType<OkObjectResult>()
            .Which.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task GetInstituciones_CuandoServicioRetornaListaVacia_RetornaStatus200()
    {
        _mockService
            .Setup(s => s.GetInstitucionesAsync())
            .ReturnsAsync(new List<InstitucionDto>());

        var result = await _controller.GetInstituciones();

        result.Should().BeOfType<OkObjectResult>()
            .Which.StatusCode.Should().Be(200);
    }

    // CrearSolicitud — éxito

    [Fact]
    public async Task CrearSolicitud_CuandoValido_RetornaStatus200()
    {
        _mockService
            .Setup(s => s.CrearSolicitudAsync(It.IsAny<SolicitudMentoriaRequestDto>()))
            .ReturnsAsync(new SolicitudMentoriaResponseDto
            {
                SolicitudId = "sol-001",
                Mensaje = "¡Tu solicitud fue enviada!"
            });

        var result = await _controller.CrearSolicitud(RequestValido());

        result.Should().BeOfType<OkObjectResult>()
            .Which.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task CrearSolicitud_CuandoValido_LlamaServicioUnaVez()
    {
        _mockService
            .Setup(s => s.CrearSolicitudAsync(It.IsAny<SolicitudMentoriaRequestDto>()))
            .ReturnsAsync(new SolicitudMentoriaResponseDto { SolicitudId = "sol-001", Mensaje = "OK" });

        await _controller.CrearSolicitud(RequestValido());

        _mockService.Verify(s => s.CrearSolicitudAsync(It.IsAny<SolicitudMentoriaRequestDto>()), Times.Once);
    }

    // CrearSolicitud — institución no encontrada

    [Fact]
    public async Task CrearSolicitud_CuandoInstitucionNoExiste_RetornaStatus404()
    {
        _mockService
            .Setup(s => s.CrearSolicitudAsync(It.IsAny<SolicitudMentoriaRequestDto>()))
            .ThrowsAsync(new KeyNotFoundException("Institución no encontrada."));

        var result = await _controller.CrearSolicitud(RequestValido());

        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(404);
    }
}
