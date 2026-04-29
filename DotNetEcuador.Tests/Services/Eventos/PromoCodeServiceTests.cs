using DotNetEcuador.API.Infraestructure.Repositories;
using DotNetEcuador.API.Infraestructure.Services.Eventos;
using DotNetEcuador.API.Models.Eventos;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace DotNetEcuador.Tests.Services.Eventos;

public class PromoCodeServiceTests
{
    private readonly Mock<IRepository<PromoCode>> _mockRepo;
    private readonly Mock<ILogger<PromoCodeService>> _mockLogger;
    private readonly IPromoCodeService _service;

    public PromoCodeServiceTests()
    {
        _mockRepo = new Mock<IRepository<PromoCode>>();
        _mockLogger = new Mock<ILogger<PromoCodeService>>();
        _service = new PromoCodeService(_mockRepo.Object, _mockLogger.Object);
    }

    private static PromoCode CodigoActivo(
        string code = "TEST01",
        int? maxUses = null,
        int currentUses = 0,
        DateTime? expiresAt = null) => new()
    {
        Id = "507f1f77bcf86cd799439011",
        Code = code,
        Description = "Código de prueba",
        IsActive = true,
        MaxUses = maxUses,
        CurrentUses = currentUses,
        ExpiresAt = expiresAt,
        CreatedAt = DateTime.UtcNow
    };

    private void SetupRepo(PromoCode? returning) =>
        _mockRepo
            .Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<PromoCode, bool>>>()))
            .ReturnsAsync(returning);

    // Casos inválidos

    [Fact]
    public async Task ValidateAsync_CuandoCodigoNoExiste_RetornaInvalido()
    {
        SetupRepo(null);

        var result = await _service.ValidateAsync("NOEXST");

        result.Valid.Should().BeFalse();
        result.Message.Should().Be("Código no válido o expirado.");
    }

    [Fact]
    public async Task ValidateAsync_CuandoMaxUsesExactamenteAlcanzado_RetornaInvalido()
    {
        SetupRepo(CodigoActivo(maxUses: 10, currentUses: 10));

        var result = await _service.ValidateAsync("TEST01");

        result.Valid.Should().BeFalse();
        result.Message.Should().Be("Código no válido o expirado.");
    }

    [Fact]
    public async Task ValidateAsync_CuandoCurrentUsesSuperaMaxUses_RetornaInvalido()
    {
        SetupRepo(CodigoActivo(maxUses: 5, currentUses: 10));

        var result = await _service.ValidateAsync("TEST01");

        result.Valid.Should().BeFalse();
    }

    [Fact]
    public async Task ValidateAsync_CuandoCodigoVencidoAyer_RetornaInvalido()
    {
        SetupRepo(CodigoActivo(expiresAt: DateTime.UtcNow.AddDays(-1)));

        var result = await _service.ValidateAsync("TEST01");

        result.Valid.Should().BeFalse();
        result.Message.Should().Be("Código no válido o expirado.");
    }

    [Fact]
    public async Task ValidateAsync_CuandoCodigoVencidoHaceUnSegundo_RetornaInvalido()
    {
        SetupRepo(CodigoActivo(expiresAt: DateTime.UtcNow.AddSeconds(-1)));

        var result = await _service.ValidateAsync("TEST01");

        result.Valid.Should().BeFalse();
    }

    // Casos válidos

    [Fact]
    public async Task ValidateAsync_CuandoCodigoActivoSinLimites_RetornaValido()
    {
        SetupRepo(CodigoActivo());

        var result = await _service.ValidateAsync("TEST01");

        result.Valid.Should().BeTrue();
        result.Message.Should().Be("¡Código aplicado! Tu acceso está confirmado.");
    }

    [Fact]
    public async Task ValidateAsync_CuandoMaxUsesNoAlcanzado_RetornaValido()
    {
        SetupRepo(CodigoActivo(maxUses: 100, currentUses: 99));

        var result = await _service.ValidateAsync("TEST01");

        result.Valid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_CuandoExpiracionFutura_RetornaValido()
    {
        SetupRepo(CodigoActivo(expiresAt: DateTime.UtcNow.AddDays(30)));

        var result = await _service.ValidateAsync("TEST01");

        result.Valid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_CuandoMaxUsesNuloYExpiracionNula_RetornaValido()
    {
        SetupRepo(CodigoActivo(maxUses: null, expiresAt: null));

        var result = await _service.ValidateAsync("TEST01");

        result.Valid.Should().BeTrue();
    }

    // Normalización

    [Fact]
    public async Task ValidateAsync_CuandoCodigoEnMinusculas_ConsultaRepositorioUnaVez()
    {
        SetupRepo(CodigoActivo());

        var result = await _service.ValidateAsync("test01");

        result.Valid.Should().BeTrue();
        _mockRepo.Verify(
            r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<PromoCode, bool>>>()), Times.Once);
    }

    [Fact]
    public async Task ValidateAsync_CuandoCodigoMixto_ConsultaRepositorioUnaVez()
    {
        SetupRepo(null);

        await _service.ValidateAsync("tEsT01");

        _mockRepo.Verify(
            r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<PromoCode, bool>>>()), Times.Once);
    }

    // IncrementUsesAsync

    [Fact]
    public async Task IncrementUsesAsync_CuandoCodigoNoExiste_NoLanzaExcepcion()
    {
        SetupRepo(null);

        await _service.IncrementUsesAsync("NOEXST");
    }

    [Fact]
    public async Task IncrementUsesAsync_CuandoCodigoExiste_IncrementaCurrentUses()
    {
        var promoCode = CodigoActivo(currentUses: 3);
        SetupRepo(promoCode);
        _mockRepo.Setup(r => r.UpdateAsync(promoCode.Id, It.IsAny<PromoCode>())).Returns(Task.CompletedTask);

        await _service.IncrementUsesAsync("TEST01");

        _mockRepo.Verify(
            r => r.UpdateAsync(promoCode.Id, It.Is<PromoCode>(p => p.CurrentUses == 4)),
            Times.Once);
    }

    [Fact]
    public async Task IncrementUsesAsync_CuandoCodigoExiste_LlamaUpdateAsync()
    {
        var promoCode = CodigoActivo();
        SetupRepo(promoCode);
        _mockRepo.Setup(r => r.UpdateAsync(It.IsAny<string>(), It.IsAny<PromoCode>())).Returns(Task.CompletedTask);

        await _service.IncrementUsesAsync("TEST01");

        _mockRepo.Verify(
            r => r.UpdateAsync(promoCode.Id, It.IsAny<PromoCode>()),
            Times.Once);
    }

    [Fact]
    public async Task IncrementUsesAsync_NormalizaCodigoAMayusculas()
    {
        var promoCode = CodigoActivo();
        SetupRepo(promoCode);
        _mockRepo.Setup(r => r.UpdateAsync(It.IsAny<string>(), It.IsAny<PromoCode>())).Returns(Task.CompletedTask);

        await _service.IncrementUsesAsync("test01");

        _mockRepo.Verify(
            r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<PromoCode, bool>>>()), Times.Once);
    }
}
