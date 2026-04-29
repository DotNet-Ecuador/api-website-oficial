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

public class PromoCodesControllerTests
{
    private readonly Mock<IPromoCodeService> _mockService;
    private readonly Mock<IMessageService> _mockMessageService;
    private readonly Mock<ILogger<PromoCodesController>> _mockLogger;
    private readonly PromoCodesController _controller;

    public PromoCodesControllerTests()
    {
        _mockService = new Mock<IPromoCodeService>();
        _mockMessageService = new Mock<IMessageService>();
        _mockLogger = new Mock<ILogger<PromoCodesController>>();

        _controller = new PromoCodesController(
            _mockService.Object,
            _mockMessageService.Object,
            _mockLogger.Object);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
    }

    private static PromoCodeValidateRequestDto Request(string code) => new() { Code = code };

    private void SetupService(bool valid, string message = "") =>
        _mockService
            .Setup(s => s.ValidateAsync(It.IsAny<string>()))
            .ReturnsAsync(new PromoCodeValidateResponseDto { Valid = valid, Message = message });

    // Código válido

    [Fact]
    public async Task Validate_CuandoCodigoValido_RetornaOkConDatos()
    {
        var responseDto = new PromoCodeValidateResponseDto
        {
            Valid = true,
            Message = "¡Código aplicado! Tu acceso está confirmado."
        };
        _mockService.Setup(s => s.ValidateAsync("TEST01")).ReturnsAsync(responseDto);

        var result = await _controller.Validate(Request("TEST01"));

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var apiResponse = okResult.Value.Should().BeOfType<ApiResponse<PromoCodeValidateResponseDto>>().Subject;
        apiResponse.Success.Should().BeTrue();
        apiResponse.Data!.Valid.Should().BeTrue();
        apiResponse.Message.Should().Be("¡Código aplicado! Tu acceso está confirmado.");
    }

    [Fact]
    public async Task Validate_CuandoCodigoValido_RetornaStatus200()
    {
        SetupService(valid: true, message: "¡Código aplicado! Tu acceso está confirmado.");

        var result = await _controller.Validate(Request("TEST01"));

        result.Should().BeOfType<OkObjectResult>()
            .Which.StatusCode.Should().Be(200);
    }

    // Código inválido

    [Fact]
    public async Task Validate_CuandoCodigoInvalido_RetornaStatus422()
    {
        SetupService(valid: false, message: "Código no válido o expirado.");

        var result = await _controller.Validate(Request("BADCOD"));

        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(422);
    }

    [Fact]
    public async Task Validate_CuandoCodigoInvalido_CuerpoEsApiError()
    {
        SetupService(valid: false, message: "Código no válido o expirado.");

        var result = await _controller.Validate(Request("BADCOD"));

        var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.Value.Should().BeOfType<ApiError>();
    }

    [Fact]
    public async Task Validate_CuandoCodigoInvalido_ApiErrorContieneMensaje()
    {
        SetupService(valid: false, message: "Código no válido o expirado.");

        var result = await _controller.Validate(Request("BADCOD"));

        var objectResult = (ObjectResult)result;
        var apiError = (ApiError)objectResult.Value!;
        apiError.Detail.Should().Be("Código no válido o expirado.");
    }

    // Delegación al servicio

    [Fact]
    public async Task Validate_PasaElCodigoExactoAlServicio()
    {
        SetupService(valid: true);

        await _controller.Validate(Request("ABCD12"));

        _mockService.Verify(s => s.ValidateAsync("ABCD12"), Times.Once);
    }

    [Fact]
    public async Task Validate_LlamaAlServicioExactamenteUnaVez()
    {
        SetupService(valid: true);

        await _controller.Validate(Request("TEST01"));

        _mockService.Verify(s => s.ValidateAsync(It.IsAny<string>()), Times.Once);
    }

    // Propagación de excepciones

    [Fact]
    public async Task Validate_CuandoServicioLanzaExcepcion_PropagaExcepcion()
    {
        _mockService
            .Setup(s => s.ValidateAsync(It.IsAny<string>()))
            .ThrowsAsync(new Exception("Error de base de datos"));

        await Assert.ThrowsAsync<Exception>(() =>
            _controller.Validate(Request("TEST01")));
    }
}
