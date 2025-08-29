using DotNetEcuador.API.Models.Auth;
using DotNetEcuador.API.Validators.Auth;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace DotNetEcuador.Tests.Validators.Auth;

public class RegisterRequestValidatorTests
{
    private readonly RegisterRequestValidator _validator;

    public RegisterRequestValidatorTests()
    {
        _validator = new RegisterRequestValidator();
    }

    [Fact]
    public void ShouldHaveErrorWhenFullNameIsEmpty()
    {
        // Arrange
        var model = new RegisterRequest { FullName = string.Empty };

        // Act & Assert
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.FullName)
            .WithErrorMessage("El nombre completo es requerido");
    }

    [Fact]
    public void ShouldHaveErrorWhenFullNameIsTooShort()
    {
        // Arrange
        var model = new RegisterRequest { FullName = "Ab" };

        // Act & Assert
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.FullName)
            .WithErrorMessage("El nombre debe tener al menos 3 caracteres");
    }

    [Fact]
    public void ShouldHaveErrorWhenEmailIsInvalid()
    {
        // Arrange
        var model = new RegisterRequest { Email = "invalid-email" };

        // Act & Assert
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("El email debe tener un formato válido");
    }

    [Fact]
    public void ShouldHaveErrorWhenPasswordIsTooShort()
    {
        // Arrange
        var model = new RegisterRequest { Password = "1234567" };

        // Act & Assert
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("La contraseña debe tener al menos 8 caracteres");
    }

    [Fact]
    public void ShouldHaveErrorWhenPasswordDoesNotMeetComplexity()
    {
        // Arrange
        var model = new RegisterRequest { Password = "password123" };

        // Act & Assert
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("La contraseña debe contener al menos una mayúscula, una minúscula y un número");
    }

    [Fact]
    public void ShouldHaveErrorWhenPasswordsDoNotMatch()
    {
        // Arrange
        var model = new RegisterRequest 
        { 
            Password = "Password123",
            ConfirmPassword = "DifferentPassword123"
        };

        // Act & Assert
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.ConfirmPassword)
            .WithErrorMessage("Las contraseñas no coinciden");
    }

    [Fact]
    public void ShouldNotHaveErrorWhenValidModel()
    {
        // Arrange
        var model = new RegisterRequest 
        { 
            FullName = "Juan Pérez",
            Email = "juan@ejemplo.com",
            Password = "Password123",
            ConfirmPassword = "Password123"
        };

        // Act & Assert
        var result = _validator.TestValidate(model);
        result.ShouldNotHaveAnyValidationErrors();
    }
}