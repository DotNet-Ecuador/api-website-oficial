using Api.Controllers;
using DotNetEcuador.API.Models;
using DotNetEcuador.API.Infraestructure.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace DotNetEcuador.Tests.Controllers;

public class AreaOfInterestControllerTests
{
    private readonly Mock<IAreaOfInterestService> _mockService;
    private readonly AreaOfInterestController _controller;

    public AreaOfInterestControllerTests()
    {
        _mockService = new Mock<IAreaOfInterestService>();
        _controller = new AreaOfInterestController(_mockService.Object);
    }

    [Fact]
    public async Task GetAllAreasOfInterestShouldReturnOkWithAreasWhenAreasExist()
    {
        // Arrange
        var expectedAreas = new List<AreaOfInterest>
        {
            new() { Id = "1", Name = "EventOrganization", Description = "Event organization help" },
            new() { Id = "2", Name = "ContentCreation", Description = "Content creation for community" }
        };

        _mockService
            .Setup(s => s.GetAllAreasOfInterestAsync())
            .ReturnsAsync(expectedAreas);

        // Act
        var result = await _controller.GetAllAreasOfInterest().ConfigureAwait(false);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(expectedAreas);

        _mockService.Verify(s => s.GetAllAreasOfInterestAsync(), Times.Once);
    }

    [Fact]
    public async Task GetAllAreasOfInterestShouldReturnOkWithEmptyListWhenNoAreasExist()
    {
        // Arrange
        var emptyList = new List<AreaOfInterest>();

        _mockService
            .Setup(s => s.GetAllAreasOfInterestAsync())
            .ReturnsAsync(emptyList);

        // Act
        var result = await _controller.GetAllAreasOfInterest().ConfigureAwait(false);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(emptyList);

        _mockService.Verify(s => s.GetAllAreasOfInterestAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateAreaOfInterestShouldReturnOkWhenAreaIsValid()
    {
        // Arrange
        var newArea = new AreaOfInterest
        {
            Name = "NewArea",
            Description = "New area description"
        };

        _mockService
            .Setup(s => s.CreateAreaOfInterestAsync(It.IsAny<AreaOfInterest>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.CreateAreaOfInterest(newArea).ConfigureAwait(false);

        // Assert
        result.Should().BeOfType<OkResult>();

        _mockService.Verify(s => s.CreateAreaOfInterestAsync(newArea), Times.Once);
    }

    [Fact]
    public async Task CreateAreaOfInterestShouldCallServiceWithCorrectParameters()
    {
        // Arrange
        var newArea = new AreaOfInterest
        {
            Id = "123",
            Name = "TechnicalWriting",
            Description = "Help with technical documentation"
        };

        _mockService
            .Setup(s => s.CreateAreaOfInterestAsync(It.IsAny<AreaOfInterest>()))
            .Returns(Task.CompletedTask);

        // Act
        await _controller.CreateAreaOfInterest(newArea).ConfigureAwait(false);

        // Assert
        _mockService.Verify(
            s => s.CreateAreaOfInterestAsync(
                It.Is<AreaOfInterest>(a =>
                    a.Id == newArea.Id &&
                    a.Name == newArea.Name &&
                    a.Description == newArea.Description)),
            Times.Once);
    }
}
