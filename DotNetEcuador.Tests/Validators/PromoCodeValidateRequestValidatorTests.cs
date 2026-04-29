using DotNetEcuador.API.Models.Eventos.DTOs;
using DotNetEcuador.API.Validators.Eventos;
using FluentValidation.TestHelper;

namespace DotNetEcuador.Tests.Validators;

public class PromoCodeValidateRequestValidatorTests
{
    private readonly PromoCodeValidateRequestValidator _validator = new();

    // Request válido

    [Theory]
    [InlineData("ABC123")]
    [InlineData("abc123")]
    [InlineData("AAAAAA")]
    [InlineData("999999")]
    [InlineData("AbCd1E")]
    [InlineData("TEST01")]
    [InlineData("FREE99")]
    public void Validate_CuandoCodigo6CaracteresAlfanumericos_NoTieneErrores(string code)
    {
        var result = _validator.TestValidate(new PromoCodeValidateRequestDto { Code = code });
        result.ShouldNotHaveAnyValidationErrors();
    }

    // Código vacío o nulo

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_CuandoCodigoVacioONulo_TieneError(string? code)
    {
        var result = _validator.TestValidate(new PromoCodeValidateRequestDto { Code = code! });
        result.ShouldHaveValidationErrorFor(x => x.Code);
    }

    // Longitud incorrecta

    [Theory]
    [InlineData("A")]
    [InlineData("AB")]
    [InlineData("ABC")]
    [InlineData("ABCD")]
    [InlineData("ABCDE")]
    public void Validate_CuandoCodigoMenosDe6Caracteres_TieneError(string code)
    {
        var result = _validator.TestValidate(new PromoCodeValidateRequestDto { Code = code });
        result.ShouldHaveValidationErrorFor(x => x.Code);
    }

    [Theory]
    [InlineData("ABCDEFG")]
    [InlineData("12345678")]
    [InlineData("ABCDEFGHIJ")]
    public void Validate_CuandoCodigoMasDe6Caracteres_TieneError(string code)
    {
        var result = _validator.TestValidate(new PromoCodeValidateRequestDto { Code = code });
        result.ShouldHaveValidationErrorFor(x => x.Code);
    }

    // Caracteres no alfanuméricos

    [Theory]
    [InlineData("ABC!23")]
    [InlineData("AB C12")]
    [InlineData("AB-C12")]
    [InlineData("AB.C12")]
    [InlineData("AB_C12")]
    [InlineData("ÁBC123")]
    public void Validate_CuandoCodigoConCaracteresEspeciales_TieneError(string code)
    {
        var result = _validator.TestValidate(new PromoCodeValidateRequestDto { Code = code });
        result.ShouldHaveValidationErrorFor(x => x.Code);
    }
}
