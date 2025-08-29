using DotNetEcuador.API.Models;
using DotNetEcuador.API.Validators;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace DotNetEcuador.Tests.Validators;

public class VolunteerApplicationValidatorTests
{
    private readonly VolunteerApplicationValidator _validator;

    public VolunteerApplicationValidatorTests()
    {
        _validator = new VolunteerApplicationValidator();
    }

    [Fact]
    public void ShouldHaveErrorWhenFullNameIsEmpty()
    {
        // Arrange
        var model = new VolunteerApplication { FullName = string.Empty };

        // Act & Assert
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.FullName)
            .WithErrorMessage("El nombre completo es requerido");
    }

    [Fact]
    public void ShouldHaveErrorWhenFullNameIsTooShort()
    {
        // Arrange
        var model = new VolunteerApplication { FullName = "Ab" };

        // Act & Assert
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.FullName)
            .WithErrorMessage("El nombre debe tener al menos 3 caracteres");
    }

    [Fact]
    public void ShouldHaveErrorWhenEmailIsEmpty()
    {
        // Arrange
        var model = new VolunteerApplication { Email = string.Empty };

        // Act & Assert
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("El email es requerido");
    }

    [Fact]
    public void ShouldHaveErrorWhenEmailIsInvalid()
    {
        // Arrange
        var model = new VolunteerApplication { Email = "invalid-email" };

        // Act & Assert
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("El email debe tener un formato válido");
    }

    [Fact]
    public void ShouldHaveErrorWhenAreasOfInterestIsEmpty()
    {
        // Arrange
        var model = new VolunteerApplication { AreasOfInterest = new List<string>() };

        // Act & Assert
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.AreasOfInterest)
            .WithErrorMessage("Debe seleccionar al menos un área de interés");
    }

    [Fact]
    public void ShouldHaveErrorWhenAreasOfInterestContainsInvalidValues()
    {
        // Arrange
        var model = new VolunteerApplication 
        { 
            AreasOfInterest = new List<string> { "InvalidArea" } 
        };

        // Act & Assert
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.AreasOfInterest)
            .WithErrorMessage("Una o más áreas de interés seleccionadas no son válidas. Valores válidos: EventOrganization, ContentCreation, TechnicalSupport, SocialMediaManagement, Other");
    }

    [Fact]
    public void ShouldHaveErrorWhenOtherAreasIsEmptyButOtherIsSelected()
    {
        // Arrange
        var model = new VolunteerApplication 
        { 
            AreasOfInterest = new List<string> { "Other" },
            OtherAreas = string.Empty
        };

        // Act & Assert
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.OtherAreas)
            .WithErrorMessage("Debe especificar las otras áreas de interés");
    }

    [Fact]
    public void ShouldNotHaveErrorWhenOtherAreasIsFilledAndOtherIsSelected()
    {
        // Arrange
        var model = new VolunteerApplication 
        { 
            FullName = "Juan Pérez",
            Email = "juan@ejemplo.com",
            City = "Quito",
            AreasOfInterest = new List<string> { "Other" },
            OtherAreas = "Mentoring",
            AvailableTime = "Weekends",
            WhyVolunteer = "Want to help"
        };

        // Act & Assert
        var result = _validator.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(x => x.OtherAreas);
    }

    [Fact]
    public void ShouldNotHaveErrorWhenValidModel()
    {
        // Arrange
        var model = new VolunteerApplication 
        { 
            FullName = "Juan Pérez González",
            Email = "juan@ejemplo.com",
            City = "Quito",
            AreasOfInterest = new List<string> { "EventOrganization", "TechnicalSupport" },
            AvailableTime = "Weekends and evenings",
            WhyVolunteer = "I want to contribute to the community"
        };

        // Act & Assert
        var result = _validator.TestValidate(model);
        result.ShouldNotHaveAnyValidationErrors();
    }
}