using api.Controllers;
using api.Models;
using DotNetEcuador.API.Infraestructure.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace DotNetEcuador.Tests.Controllers;

public class AreaOfInterestControllerTests
{
    private readonly Mock<AreaOfInterestService> _mockService;
    private readonly AreaOfInterestController _controller;

    public AreaOfInterestControllerTests()
    {
        var mockRepository = new Mock<DotNetEcuador.API.Infraestructure.Repositories.IRepository<AreaOfInterest>>();
        _mockService = new Mock<AreaOfInterestService>(mockRepository.Object);
        _controller = new AreaOfInterestController(_mockService.Object);
    }

    [Fact]
    public async Task GetAllAreasOfInterest_ShouldReturnOkWithAreas_WhenAreasExist()
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
        var result = await _controller.GetAllAreasOfInterest();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(expectedAreas);
        
        _mockService.Verify(s => s.GetAllAreasOfInterestAsync(), Times.Once);
    }

    [Fact]
    public async Task GetAllAreasOfInterest_ShouldReturnOkWithEmptyList_WhenNoAreasExist()
    {
        // Arrange
        var emptyList = new List<AreaOfInterest>();

        _mockService
            .Setup(s => s.GetAllAreasOfInterestAsync())
            .ReturnsAsync(emptyList);

        // Act
        var result = await _controller.GetAllAreasOfInterest();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(emptyList);
        
        _mockService.Verify(s => s.GetAllAreasOfInterestAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateAreaOfInterest_ShouldReturnOk_WhenAreaIsValid()
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
        var result = await _controller.CreateAreaOfInterest(newArea);

        // Assert
        result.Should().BeOfType<OkResult>();
        
        _mockService.Verify(s => s.CreateAreaOfInterestAsync(newArea), Times.Once);
    }

    [Fact]
    public async Task CreateAreaOfInterest_ShouldCallService_WithCorrectParameters()
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
        await _controller.CreateAreaOfInterest(newArea);

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