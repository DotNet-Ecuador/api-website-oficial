using DotNetEcuador.API.Models.Mentorias.DTOs;
using DotNetEcuador.API.Validators.Mentorias;
using FluentValidation.TestHelper;

namespace DotNetEcuador.Tests.Validators;

public class SolicitudMentoriaRequestValidatorTests
{
    private readonly SolicitudMentoriaRequestValidator _validator = new();

    private static SolicitudMentoriaRequestDto RequestValido() => new()
    {
        NombreCompleto = "Juan Pérez",
        Email = "juan@ejemplo.com",
        Telefono = "+593987654321",
        InstitucionId = "inst-001",
        TemaConsulta = "Quiero aprender sobre APIs en .NET"
    };

    // Solicitud válida

    [Fact]
    public void Validate_CuandoTodosCamposValidos_NoTieneErrores()
    {
        var result = _validator.TestValidate(RequestValido());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_CuandoOtrosConOtraInstitucion_NoTieneErrores()
    {
        var dto = RequestValido();
        dto.InstitucionId = "otros";
        dto.OtraInstitucion = "Universidad del Pacífico";

        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveAnyValidationErrors();
    }

    // NombreCompleto

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_CuandoNombreVacioONulo_TieneError(string? nombre)
    {
        var dto = RequestValido();
        dto.NombreCompleto = nombre!;
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.NombreCompleto);
    }

    [Theory]
    [InlineData("AB")]
    public void Validate_CuandoNombreMenosDe3Caracteres_TieneError(string nombre)
    {
        var dto = RequestValido();
        dto.NombreCompleto = nombre;
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.NombreCompleto);
    }

    [Fact]
    public void Validate_CuandoNombreMasDe100Caracteres_TieneError()
    {
        var dto = RequestValido();
        dto.NombreCompleto = new string('A', 101);
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.NombreCompleto);
    }

    // Email

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_CuandoEmailVacioONulo_TieneError(string? email)
    {
        var dto = RequestValido();
        dto.Email = email!;
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Theory]
    [InlineData("noesvalido")]
    [InlineData("noesvalido@")]
    [InlineData("@dominio.com")]
    public void Validate_CuandoEmailInvalido_TieneError(string email)
    {
        var dto = RequestValido();
        dto.Email = email;
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    // Telefono

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_CuandoTelefonoVacioONulo_TieneError(string? telefono)
    {
        var dto = RequestValido();
        dto.Telefono = telefono!;
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Telefono);
    }

    [Theory]
    [InlineData("+593987654321")]
    [InlineData("0987654321")]
    [InlineData("098 765 4321")]
    public void Validate_CuandoTelefonoValido_NoTieneError(string telefono)
    {
        var dto = RequestValido();
        dto.Telefono = telefono;
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveValidationErrorFor(x => x.Telefono);
    }

    // InstitucionId

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_CuandoInstitucionIdVaciaONula_TieneError(string? id)
    {
        var dto = RequestValido();
        dto.InstitucionId = id!;
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.InstitucionId);
    }

    // OtraInstitucion — sólo requerida cuando InstitucionId == "otros"

    [Fact]
    public void Validate_CuandoOtrosYOtraInstitucionVacia_TieneError()
    {
        var dto = RequestValido();
        dto.InstitucionId = "otros";
        dto.OtraInstitucion = null;
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.OtraInstitucion);
    }

    [Fact]
    public void Validate_CuandoNoOtrosYOtraInstitucionVacia_NoTieneError()
    {
        var dto = RequestValido();
        dto.OtraInstitucion = null;
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveValidationErrorFor(x => x.OtraInstitucion);
    }

    [Fact]
    public void Validate_CuandoOtrosYOtraInstitucionExcede100_TieneError()
    {
        var dto = RequestValido();
        dto.InstitucionId = "otros";
        dto.OtraInstitucion = new string('A', 101);
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.OtraInstitucion);
    }

    // TemaConsulta

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_CuandoTemaVacioONulo_TieneError(string? tema)
    {
        var dto = RequestValido();
        dto.TemaConsulta = tema!;
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.TemaConsulta);
    }

    [Fact]
    public void Validate_CuandoTemaMenosDe10Caracteres_TieneError()
    {
        var dto = RequestValido();
        dto.TemaConsulta = "Corto";
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.TemaConsulta);
    }

    [Fact]
    public void Validate_CuandoTemaMasDe1000Caracteres_TieneError()
    {
        var dto = RequestValido();
        dto.TemaConsulta = new string('X', 1001);
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.TemaConsulta);
    }
}
