using api.Controllers;
using api.Models;
using DotNetEcuador.API.Infraestructure.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using Moq;

namespace DotNetEcuador.Tests.Controllers;

public class VolunteerApplicationControllerTests
{
    private readonly Mock<IVolunteerApplicationService> _mockService;
    private readonly VolunteerApplicationController _controller;

    public VolunteerApplicationControllerTests()
    {
        _mockService = new Mock<IVolunteerApplicationService>();
        _controller = new VolunteerApplicationController(_mockService.Object);
    }

    [Fact]
    public async Task Apply_ShouldReturnBadRequest_WhenFullNameIsEmpty()
    {
        // Arrange
        var application = new VolunteerApplication
        {
            FullName = "",
            Email = "test@example.com"
        };

        // Act
        var result = await _controller.Apply(application);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result as BadRequestObjectResult;
        badRequestResult!.Value.Should().Be("El nombre completo debe tener al menos 3 caracteres.");
    }

    [Fact]
    public async Task Apply_ShouldReturnBadRequest_WhenFullNameIsTooShort()
    {
        // Arrange
        var application = new VolunteerApplication
        {
            FullName = "Jo",
            Email = "test@example.com"
        };

        // Act
        var result = await _controller.Apply(application);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result as BadRequestObjectResult;
        badRequestResult!.Value.Should().Be("El nombre completo debe tener al menos 3 caracteres.");
    }

    [Fact]
    public async Task Apply_ShouldReturnBadRequest_WhenEmailIsInvalid()
    {
        // Arrange
        var application = new VolunteerApplication
        {
            FullName = "John Doe",
            Email = "invalid-email"
        };

        // Act
        var result = await _controller.Apply(application);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result as BadRequestObjectResult;
        badRequestResult!.Value.Should().Be("El correo electrónico no tiene un formato válido.");
    }

    [Fact]
    public async Task Apply_ShouldReturnBadRequest_WhenAreasOfInterestAreInvalid()
    {
        // Arrange
        var application = new VolunteerApplication
        {
            FullName = "John Doe",
            Email = "john@example.com",
            AreasOfInterest = new Dictionary<string, bool>
            {
                { "InvalidArea", true }
            }
        };

        _mockService
            .Setup(s => s.AreValidAreasOfInterest(It.IsAny<Dictionary<string, bool>>()))
            .Returns(false);

        // Act
        var result = await _controller.Apply(application);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result as BadRequestObjectResult;
        badRequestResult!.Value.Should().Be("Algunas de las áreas de interés seleccionadas no son válidas.");
    }

    [Fact]
    public async Task Apply_ShouldReturnBadRequest_WhenOtherAreasValidationFails()
    {
        // Arrange
        var application = new VolunteerApplication
        {
            FullName = "John Doe",
            Email = "john@example.com",
            AreasOfInterest = new Dictionary<string, bool>
            {
                { "Other", true }
            },
            OtherAreas = "" // Empty when "Other" is selected
        };

        _mockService
            .Setup(s => s.AreValidAreasOfInterest(It.IsAny<Dictionary<string, bool>>()))
            .Returns(true);

        // Act
        var result = await _controller.Apply(application);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result as BadRequestObjectResult;
        badRequestResult!.Value.Should().Be("El campo 'Otras áreas de interés' debe contener un valor si se selecciona.");
    }

    [Fact]
    public async Task Apply_ShouldReturnOk_WhenApplicationIsValid()
    {
        // Arrange
        var application = new VolunteerApplication
        {
            FullName = "John Doe",
            Email = "john@example.com",
            PhoneNumber = "123456789",
            City = "Quito",
            HasVolunteeringExperience = true,
            AreasOfInterest = new Dictionary<string, bool>
            {
                { "EventOrganization", true }
            },
            AvailableTime = "Weekends",
            SkillsOrKnowledge = "Programming",
            WhyVolunteer = "Want to help",
            AdditionalComments = "Available immediately"
        };

        _mockService
            .Setup(s => s.AreValidAreasOfInterest(It.IsAny<Dictionary<string, bool>>()))
            .Returns(true);

        _mockService
            .Setup(s => s.CreateAsync(It.IsAny<VolunteerApplication>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Apply(application);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().Be("Solicitud de voluntariado enviada exitosamente.");
        
        _mockService.Verify(s => s.CreateAsync(application), Times.Once);
    }
}