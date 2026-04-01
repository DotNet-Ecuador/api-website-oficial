using DotNetEcuador.API.Models.Eventos.DTOs;
using DotNetEcuador.API.Validators.Eventos;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace DotNetEcuador.Tests.Validators;

public class RegistroRequestValidatorTests
{
    private readonly RegistroRequestValidator _validator = new();

    private static RegistroRequestDto ValidRequest() => new()
    {
        Nombre = "Juan Pérez",
        Email = "juan@empresa.com",
        Empresa = "Tech Corp",
        Cargo = "Developer",
        Telefono = "+593990000000",
        AceptaMarketing = false,
        EventoSlug = "meetup-mayo-2025"
    };

    [Fact]
    public void Validate_CuandoRequestValido_NoTieneErrores()
    {
        var result = _validator.TestValidate(ValidRequest());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("AB")]
    public void Validate_CuandoNombreInvalido_TieneError(string? nombre)
    {
        var request = ValidRequest();
        request.Nombre = nombre!;
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Nombre);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("noesvalido")]
    [InlineData("@sin-dominio")]
    public void Validate_CuandoEmailInvalido_TieneError(string? email)
    {
        var request = ValidRequest();
        request.Email = email!;
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Validate_CuandoEmailValido_NoTieneError()
    {
        var request = ValidRequest();
        request.Email = "valido@dominio.com";
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.Email);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_CuandoEventoSlugVacio_TieneError(string? slug)
    {
        var request = ValidRequest();
        request.EventoSlug = slug!;
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.EventoSlug);
    }

    [Fact]
    public void Validate_CuandoTelefonoVacio_NoTieneError()
    {
        var request = ValidRequest();
        request.Telefono = string.Empty;
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.Telefono);
    }
}
