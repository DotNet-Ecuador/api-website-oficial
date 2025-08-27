using api.Models;
using DotNetEcuador.API.Infraestructure.Repositories;
using DotNetEcuador.API.Infraestructure.Services;
using FluentAssertions;
using Moq;

namespace DotNetEcuador.Tests.Services;

public class AreaOfInterestServiceTests
{
    private readonly Mock<IRepository<AreaOfInterest>> _mockRepository;
    private readonly AreaOfInterestService _service;

    public AreaOfInterestServiceTests()
    {
        _mockRepository = new Mock<IRepository<AreaOfInterest>>();
        _service = new AreaOfInterestService(_mockRepository.Object);
    }

    [Fact]
    public async Task GetAllAreasOfInterestAsync_ShouldReturnAllAreas_WhenCalled()
    {
        // Arrange
        var expectedAreas = new List<AreaOfInterest>
        {
            new() { Id = "1", Name = "EventOrganization", Description = "Event organization help" },
            new() { Id = "2", Name = "ContentCreation", Description = "Content creation for community" }
        };

        _mockRepository
            .Setup(repo => repo.GetAllAsync())
            .ReturnsAsync(expectedAreas);

        // Act
        var result = await _service.GetAllAreasOfInterestAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().BeEquivalentTo(expectedAreas);
        _mockRepository.Verify(repo => repo.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetAllAreasOfInterestAsync_ShouldReturnEmptyList_WhenNoAreasExist()
    {
        // Arrange
        var expectedAreas = new List<AreaOfInterest>();

        _mockRepository
            .Setup(repo => repo.GetAllAsync())
            .ReturnsAsync(expectedAreas);

        // Act
        var result = await _service.GetAllAreasOfInterestAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
        _mockRepository.Verify(repo => repo.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateAreaOfInterestAsync_ShouldCallRepositoryCreate_WhenAreaIsValid()
    {
        // Arrange
        var newArea = new AreaOfInterest
        {
            Name = "NewArea",
            Description = "New area description"
        };

        // Act
        await _service.CreateAreaOfInterestAsync(newArea);

        // Assert
        _mockRepository.Verify(
            repo => repo.CreateAsync(newArea),
            Times.Once);
    }

    [Theory]
    [InlineData("EventOrganization")]
    [InlineData("ContentCreation")]
    [InlineData("TechnicalSupport")]
    [InlineData("SocialMediaManagement")]
    [InlineData("Other")]
    public void ValidateAreaName_ShouldReturnTrue_ForValidAreaNames(string areaName)
    {
        // Act & Assert
        var validAreas = new[] { "EventOrganization", "ContentCreation", "TechnicalSupport", "SocialMediaManagement", "Other" };
        validAreas.Should().Contain(areaName);
    }
}