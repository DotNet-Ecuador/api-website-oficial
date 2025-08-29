using DotNetEcuador.API.Controllers.V1;
using DotNetEcuador.API.Models;
using DotNetEcuador.API.Models.Common;
using DotNetEcuador.API.Infraestructure.Services;
using DotNetEcuador.API.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace DotNetEcuador.Tests.Controllers;

public class VolunteerApplicationControllerTests
{
    private readonly Mock<IVolunteerApplicationService> _mockService;
    private readonly Mock<IMessageService> _mockMessageService;
    private readonly Mock<ILogger<VolunteerApplicationController>> _mockLogger;
    private readonly VolunteerApplicationController _controller;

    public VolunteerApplicationControllerTests()
    {
        _mockService = new Mock<IVolunteerApplicationService>();
        _mockMessageService = new Mock<IMessageService>();
        _mockLogger = new Mock<ILogger<VolunteerApplicationController>>();
        _controller = new VolunteerApplicationController(_mockService.Object, _mockMessageService.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task ApplyShouldReturnSuccessWhenValidApplication()
    {
        // Arrange
        var application = new VolunteerApplication
        {
            FullName = "Juan Pérez",
            Email = "test@example.com",
            City = "Quito",
            AreasOfInterest = new List<string> { "EventOrganization" },
            AvailableTime = "Weekends",
            WhyVolunteer = "Want to help"
        };

        _mockService
            .Setup(s => s.CreateAsync(It.IsAny<VolunteerApplication>()))
            .Returns(Task.CompletedTask);

        _mockMessageService
            .Setup(s => s.GetMessage(It.IsAny<string>()))
            .Returns("Solicitud enviada exitosamente");

        // Act
        var result = await _controller.Apply(application);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        _mockService.Verify(s => s.CreateAsync(application), Times.Once);
        
        var okResult = result as OkObjectResult;
        var apiResponse = okResult!.Value as ApiResponse;
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Message.Should().Be("Solicitud enviada exitosamente");
    }

    [Fact]
    public async Task ApplyShouldPropagateExceptionWhenServiceFails()
    {
        // Arrange
        var application = new VolunteerApplication
        {
            FullName = "Juan Pérez",
            Email = "test@example.com",
            City = "Quito",
            AreasOfInterest = new List<string> { "EventOrganization" },
            AvailableTime = "Weekends",
            WhyVolunteer = "Want to help"
        };

        _mockService
            .Setup(s => s.CreateAsync(It.IsAny<VolunteerApplication>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _controller.Apply(application));
    }

    [Fact]
    public async Task GetAllApplicationsShouldReturnPagedResponse()
    {
        // Arrange
        var request = new PagedRequest { Page = 1, PageSize = 10 };
        var expectedResponse = new PagedResponse<VolunteerApplication>
        {
            Data = new List<VolunteerApplication>(),
            TotalCount = 0,
            Page = 1,
            PageSize = 10
        };

        _mockService
            .Setup(s => s.GetAllAsync(request))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.GetAllApplications(request);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeOfType<ApiResponse<PagedResponse<VolunteerApplication>>>();
        
        var apiResponse = okResult.Value as ApiResponse<PagedResponse<VolunteerApplication>>;
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data.Should().Be(expectedResponse);
        apiResponse.Message.Should().Be("Lista de solicitudes obtenida exitosamente");
    }

    [Fact]
    public async Task GetAllApplicationsShouldCallSearchWhenSearchTermProvided()
    {
        // Arrange
        var request = new PagedRequest { Page = 1, PageSize = 10, Search = "Juan" };
        var expectedResponse = new PagedResponse<VolunteerApplication>();

        _mockService
            .Setup(s => s.SearchAsync(request, "Juan"))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.GetAllApplications(request);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        _mockService.Verify(s => s.SearchAsync(request, "Juan"), Times.Once);
        _mockService.Verify(s => s.GetAllAsync(It.IsAny<PagedRequest>()), Times.Never);
        
        var okResult = result as OkObjectResult;
        var apiResponse = okResult!.Value as ApiResponse<PagedResponse<VolunteerApplication>>;
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data.Should().Be(expectedResponse);
    }

    [Fact]
    public async Task GetAllApplicationsShouldPropagateExceptionWhenServiceFails()
    {
        // Arrange
        var request = new PagedRequest();

        _mockService
            .Setup(s => s.GetAllAsync(request))
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _controller.GetAllApplications(request));
    }
}