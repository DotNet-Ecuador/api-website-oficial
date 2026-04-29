using DotNetEcuador.API.Controllers.V1;
using DotNetEcuador.API.Infraestructure.Services.Eventos;
using DotNetEcuador.API.Models.Common;
using DotNetEcuador.API.Models.Eventos.DTOs;
using DotNetEcuador.API.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace DotNetEcuador.Tests.Controllers;

public class RegistrosControllerTests
{
    private readonly Mock<IRegistroService> _mockService;
    private readonly Mock<IMessageService> _mockMessageService;
    private readonly Mock<ILogger<RegistrosController>> _mockLogger;
    private readonly RegistrosController _controller;

    public RegistrosControllerTests()
    {
        _mockService = new Mock<IRegistroService>();
        _mockMessageService = new Mock<IMessageService>();
        _mockLogger = new Mock<ILogger<RegistrosController>>();

        _controller = new RegistrosController(
            _mockService.Object,
            _mockMessageService.Object,
            _mockLogger.Object);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
    }

    private static AplicarPromoRequestDto PromoRequest(string code = "TEST01") => new() { Code = code };

    // AplicarPromo — sin session token

    [Fact]
    public async Task AplicarPromo_CuandoSessionTokenVacio_RetornaStatus401()
    {
        var result = await _controller.AplicarPromo("reg-id", PromoRequest(), string.Empty);

        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(401);
    }

    [Fact]
    public async Task AplicarPromo_CuandoSessionTokenNulo_RetornaStatus401()
    {
        var result = await _controller.AplicarPromo("reg-id", PromoRequest(), null!);

        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(401);
    }

    // AplicarPromo — servicio exitoso

    [Fact]
    public async Task AplicarPromo_CuandoValido_RetornaStatus200()
    {
        _mockService.Setup(s => s.AplicarPromoAsync("reg-id", "token-ok", "TEST01")).Returns(Task.CompletedTask);

        var result = await _controller.AplicarPromo("reg-id", PromoRequest(), "token-ok");

        result.Should().BeOfType<OkObjectResult>()
            .Which.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task AplicarPromo_CuandoValido_CuerpoEsApiResponse()
    {
        _mockService.Setup(s => s.AplicarPromoAsync("reg-id", "token-ok", "TEST01")).Returns(Task.CompletedTask);

        var result = await _controller.AplicarPromo("reg-id", PromoRequest(), "token-ok");

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeOfType<ApiResponse>();
    }

    [Fact]
    public async Task AplicarPromo_CuandoValido_MensajeConfirmacion()
    {
        _mockService.Setup(s => s.AplicarPromoAsync("reg-id", "token-ok", "TEST01")).Returns(Task.CompletedTask);

        var result = await _controller.AplicarPromo("reg-id", PromoRequest(), "token-ok");

        var okResult = (OkObjectResult)result;
        var response = (ApiResponse)okResult.Value!;
        response.Message.Should().Be("Código promo aplicado. Tu acceso está confirmado.");
    }

    // AplicarPromo — registro no encontrado

    [Fact]
    public async Task AplicarPromo_CuandoRegistroNoExiste_RetornaStatus404()
    {
        _mockService.Setup(s => s.AplicarPromoAsync("no-existe", "token-ok", "TEST01"))
            .ThrowsAsync(new KeyNotFoundException("Registro no encontrado."));

        var result = await _controller.AplicarPromo("no-existe", PromoRequest(), "token-ok");

        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(404);
    }

    // AplicarPromo — token inválido

    [Fact]
    public async Task AplicarPromo_CuandoTokenInvalido_RetornaStatus401()
    {
        _mockService.Setup(s => s.AplicarPromoAsync("reg-id", "token-malo", "TEST01"))
            .ThrowsAsync(new UnauthorizedAccessException("Session token inválido."));

        var result = await _controller.AplicarPromo("reg-id", PromoRequest(), "token-malo");

        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(401);
    }

    // AplicarPromo — delegación al servicio

    [Fact]
    public async Task AplicarPromo_PasaIdTokenYCodigoExactosAlServicio()
    {
        _mockService.Setup(s => s.AplicarPromoAsync("reg-abc", "my-token", "ABCD12")).Returns(Task.CompletedTask);

        await _controller.AplicarPromo("reg-abc", PromoRequest("ABCD12"), "my-token");

        _mockService.Verify(s => s.AplicarPromoAsync("reg-abc", "my-token", "ABCD12"), Times.Once);
    }

    [Fact]
    public async Task AplicarPromo_LlamaAlServicioExactamenteUnaVez()
    {
        _mockService.Setup(s => s.AplicarPromoAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        await _controller.AplicarPromo("reg-id", PromoRequest(), "token-ok");

        _mockService.Verify(s => s.AplicarPromoAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }
}
