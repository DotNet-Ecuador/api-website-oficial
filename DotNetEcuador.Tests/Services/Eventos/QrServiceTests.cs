using DotNetEcuador.API.Infraestructure.Services.Eventos;
using FluentAssertions;

namespace DotNetEcuador.Tests.Services.Eventos;

public class QrServiceTests
{
    private readonly IQrService _service = new QrService();

    [Fact]
    public void GenerarQr_RetornaBytesNoVacios()
    {
        var bytes = _service.GenerarQr("token-test-123");

        bytes.Should().NotBeNullOrEmpty();
        bytes.Length.Should().BeGreaterThan(0);
    }

    [Fact]
    public void GenerarQrBase64_RetornaCadenaBase64Valida()
    {
        var base64 = _service.GenerarQrBase64("token-test-123");

        base64.Should().NotBeNullOrEmpty();
        var bytes = Convert.FromBase64String(base64);
        bytes.Should().NotBeEmpty();
    }

    [Fact]
    public void GenerarQr_ConMismoToken_ProduceBytesConsistentes()
    {
        var bytes1 = _service.GenerarQr("token-fijo");
        var bytes2 = _service.GenerarQr("token-fijo");

        bytes1.Length.Should().Be(bytes2.Length);
    }

    [Fact]
    public void GenerarQr_ConDiferentesTokens_ProduceBytesDistintos()
    {
        var bytes1 = _service.GenerarQr("token-a");
        var bytes2 = _service.GenerarQr("token-b");

        bytes1.Should().NotBeEquivalentTo(bytes2);
    }
}
