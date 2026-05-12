using DotNetEcuador.API.Models.Eventos;
using DotNetEcuador.API.Models.Eventos.DTOs;
using DotNetEcuador.API.Validators.Eventos;
using FluentValidation.TestHelper;

namespace DotNetEcuador.Tests.Validators;

public class ActualizarEventoRequestValidatorTests
{
    private readonly ActualizarEventoRequestValidator _validator = new();

    private static ActualizarEventoRequestDto ValidRequest() => new()
    {
        Nombre = "Meetup DotNet Ecuador",
        FechaEvento = new DateTime(2025, 9, 1, 18, 0, 0, DateTimeKind.Utc),
        FechaFin = new DateTime(2025, 9, 1, 21, 0, 0, DateTimeKind.Utc),
        Precio = 10m,
        CapacidadMaxima = 50,
        Activo = true,
        Speakers =
        [
            new Speaker { Nombre = "Ana Torres", Rol = "Ponente Principal", Avatar = string.Empty }
        ]
    };

    [Fact]
    public void Validate_CuandoRequestValido_NoTieneErrores()
    {
        var result = _validator.TestValidate(ValidRequest());
        result.ShouldNotHaveAnyValidationErrors();
    }

    // ─── Nombre ───────────────────────────────────────────────────────────────

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_CuandoNombreVacioONull_TieneError(string? nombre)
    {
        var req = ValidRequest();
        req.Nombre = nombre!;
        _validator.TestValidate(req).ShouldHaveValidationErrorFor(x => x.Nombre);
    }

    [Theory]
    [InlineData("AB")]
    [InlineData("X")]
    public void Validate_CuandoNombreMenorDeTresCaracteres_TieneError(string nombre)
    {
        var req = ValidRequest();
        req.Nombre = nombre;
        _validator.TestValidate(req).ShouldHaveValidationErrorFor(x => x.Nombre);
    }

    [Fact]
    public void Validate_CuandoNombreSuperaMaximo_TieneError()
    {
        var req = ValidRequest();
        req.Nombre = new string('A', 121);
        _validator.TestValidate(req).ShouldHaveValidationErrorFor(x => x.Nombre);
    }

    [Theory]
    [InlineData("ABC")]
    [InlineData("Meetup DotNet")]
    public void Validate_CuandoNombreValido_NoTieneError(string nombre)
    {
        var req = ValidRequest();
        req.Nombre = nombre;
        _validator.TestValidate(req).ShouldNotHaveValidationErrorFor(x => x.Nombre);
    }

    // ─── Precio ───────────────────────────────────────────────────────────────

    [Fact]
    public void Validate_CuandoPrecioNegativo_TieneError()
    {
        var req = ValidRequest();
        req.Precio = -1m;
        _validator.TestValidate(req).ShouldHaveValidationErrorFor(x => x.Precio);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(5)]
    [InlineData(100)]
    public void Validate_CuandoPrecioValido_NoTieneError(double precio)
    {
        var req = ValidRequest();
        req.Precio = (decimal)precio;
        _validator.TestValidate(req).ShouldNotHaveValidationErrorFor(x => x.Precio);
    }

    [Fact]
    public void Validate_CuandoPrecioEsNull_NoTieneError()
    {
        var req = ValidRequest();
        req.Precio = null;
        _validator.TestValidate(req).ShouldNotHaveValidationErrorFor(x => x.Precio);
    }

    // ─── CapacidadMaxima ──────────────────────────────────────────────────────

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void Validate_CuandoCapacidadMenorOIgualCero_TieneError(int capacidad)
    {
        var req = ValidRequest();
        req.CapacidadMaxima = capacidad;
        _validator.TestValidate(req).ShouldHaveValidationErrorFor(x => x.CapacidadMaxima);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(500)]
    public void Validate_CuandoCapacidadValida_NoTieneError(int capacidad)
    {
        var req = ValidRequest();
        req.CapacidadMaxima = capacidad;
        _validator.TestValidate(req).ShouldNotHaveValidationErrorFor(x => x.CapacidadMaxima);
    }

    [Fact]
    public void Validate_CuandoCapacidadEsNull_NoTieneError()
    {
        var req = ValidRequest();
        req.CapacidadMaxima = null;
        _validator.TestValidate(req).ShouldNotHaveValidationErrorFor(x => x.CapacidadMaxima);
    }

    // ─── FechaFin ─────────────────────────────────────────────────────────────

    [Fact]
    public void Validate_CuandoFechaFinAntesQueFechaEvento_TieneError()
    {
        var req = ValidRequest();
        req.FechaEvento = new DateTime(2025, 9, 1, 18, 0, 0, DateTimeKind.Utc);
        req.FechaFin = new DateTime(2025, 9, 1, 17, 0, 0, DateTimeKind.Utc);
        _validator.TestValidate(req).ShouldHaveValidationErrorFor(x => x.FechaFin);
    }

    [Fact]
    public void Validate_CuandoFechaFinIgualAFechaEvento_TieneError()
    {
        var fecha = new DateTime(2025, 9, 1, 18, 0, 0, DateTimeKind.Utc);
        var req = ValidRequest();
        req.FechaEvento = fecha;
        req.FechaFin = fecha;
        _validator.TestValidate(req).ShouldHaveValidationErrorFor(x => x.FechaFin);
    }

    [Fact]
    public void Validate_CuandoFechaFinPosteriorAFechaEvento_NoTieneError()
    {
        var req = ValidRequest();
        req.FechaEvento = new DateTime(2025, 9, 1, 18, 0, 0, DateTimeKind.Utc);
        req.FechaFin = new DateTime(2025, 9, 1, 21, 0, 0, DateTimeKind.Utc);
        _validator.TestValidate(req).ShouldNotHaveValidationErrorFor(x => x.FechaFin);
    }

    [Fact]
    public void Validate_CuandoSoloFechaFinSinFechaEvento_NoTieneError()
    {
        var req = ValidRequest();
        req.FechaEvento = null;
        req.FechaFin = new DateTime(2025, 9, 1, 21, 0, 0, DateTimeKind.Utc);
        _validator.TestValidate(req).ShouldNotHaveValidationErrorFor(x => x.FechaFin);
    }

    [Fact]
    public void Validate_CuandoSoloFechaEventoSinFechaFin_NoTieneError()
    {
        var req = ValidRequest();
        req.FechaEvento = new DateTime(2025, 9, 1, 18, 0, 0, DateTimeKind.Utc);
        req.FechaFin = null;
        _validator.TestValidate(req).ShouldNotHaveValidationErrorFor(x => x.FechaFin);
    }

    // ─── Speakers ─────────────────────────────────────────────────────────────

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_CuandoSpeakerNombreVacioONull_TieneError(string? nombre)
    {
        var req = ValidRequest();
        req.Speakers = [new Speaker { Nombre = nombre!, Rol = "Ponente", Avatar = string.Empty }];
        _validator.TestValidate(req).ShouldHaveValidationErrorFor("Speakers[0].Nombre");
    }

    [Fact]
    public void Validate_CuandoSpeakerNombreSuperaMaximo_TieneError()
    {
        var req = ValidRequest();
        req.Speakers = [new Speaker { Nombre = new string('A', 101), Rol = "Ponente", Avatar = string.Empty }];
        _validator.TestValidate(req).ShouldHaveValidationErrorFor("Speakers[0].Nombre");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_CuandoSpeakerRolVacioONull_TieneError(string? rol)
    {
        var req = ValidRequest();
        req.Speakers = [new Speaker { Nombre = "Ana Torres", Rol = rol!, Avatar = string.Empty }];
        _validator.TestValidate(req).ShouldHaveValidationErrorFor("Speakers[0].Rol");
    }

    [Fact]
    public void Validate_CuandoSpeakerRolSuperaMaximo_TieneError()
    {
        var req = ValidRequest();
        req.Speakers = [new Speaker { Nombre = "Ana Torres", Rol = new string('R', 101), Avatar = string.Empty }];
        _validator.TestValidate(req).ShouldHaveValidationErrorFor("Speakers[0].Rol");
    }

    [Fact]
    public void Validate_CuandoSinSpeakers_NoTieneError()
    {
        var req = ValidRequest();
        req.Speakers = [];
        _validator.TestValidate(req).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_CuandoMultiplesSpeakersUnoInvalido_SoloCargaErrorDeEse()
    {
        var req = ValidRequest();
        req.Speakers =
        [
            new Speaker { Nombre = "Ana Torres", Rol = "Ponente", Avatar = string.Empty },
            new Speaker { Nombre = string.Empty, Rol = "Co-Ponente", Avatar = string.Empty }
        ];
        var result = _validator.TestValidate(req);
        result.ShouldHaveValidationErrorFor("Speakers[1].Nombre");
        result.ShouldNotHaveValidationErrorFor("Speakers[0].Nombre");
    }
}
