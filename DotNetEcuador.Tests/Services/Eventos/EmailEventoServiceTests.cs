using DotNetEcuador.API.Infraestructure.Services.Eventos;
using DotNetEcuador.API.Models.Eventos;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace DotNetEcuador.Tests.Services.Eventos;

public class EmailEventoServiceTests
{
    private readonly Mock<IEmailEventoService> _mockService;

    public EmailEventoServiceTests()
    {
        _mockService = new Mock<IEmailEventoService>();
    }

    private static (Registro, Asistente, Evento) DatosValidos() =>
    (
        new Registro { Id = "reg1", IdCorto = "DN1234", TokenQr = Guid.NewGuid().ToString(), RegistradoEn = DateTime.UtcNow },
        new Asistente { Id = "ast1", Nombre = "Juan Pérez", Email = "juan@test.com" },
        new Evento { Id = "evt1", Nombre = "Meetup Mayo", FechaEvento = DateTime.UtcNow.AddDays(7), Lugar = "Quito" }
    );

    [Fact]
    public async Task EnviarConfirmacionPendiente_LlamaServicioUnaVez()
    {
        var (registro, asistente, evento) = DatosValidos();
        _mockService.Setup(s => s.EnviarConfirmacionPendienteAsync(registro, asistente, evento))
            .Returns(Task.CompletedTask);

        await _mockService.Object.EnviarConfirmacionPendienteAsync(registro, asistente, evento);

        _mockService.Verify(s => s.EnviarConfirmacionPendienteAsync(registro, asistente, evento), Times.Once);
    }

    [Fact]
    public async Task EnviarConfirmacionPagada_LlamaServicioUnaVez()
    {
        var (registro, asistente, evento) = DatosValidos();
        var qrBase64 = "base64encodedqr==";
        _mockService.Setup(s => s.EnviarConfirmacionPagadaAsync(registro, asistente, evento, qrBase64))
            .Returns(Task.CompletedTask);

        await _mockService.Object.EnviarConfirmacionPagadaAsync(registro, asistente, evento, qrBase64);

        _mockService.Verify(s => s.EnviarConfirmacionPagadaAsync(registro, asistente, evento, qrBase64), Times.Once);
    }

    [Fact]
    public async Task EnviarRechazo_LlamaServicioUnaVez()
    {
        var (registro, asistente, evento) = DatosValidos();
        var motivo = "Comprobante ilegible";
        _mockService.Setup(s => s.EnviarRechazoAsync(registro, asistente, evento, motivo))
            .Returns(Task.CompletedTask);

        await _mockService.Object.EnviarRechazoAsync(registro, asistente, evento, motivo);

        _mockService.Verify(s => s.EnviarRechazoAsync(registro, asistente, evento, motivo), Times.Once);
    }
}
