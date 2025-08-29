using DotNetEcuador.API.Models;
using DotNetEcuador.API.Infraestructure.Services;
using DotNetEcuador.API.Infraestructure.Repositories;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Moq;

namespace DotNetEcuador.Tests.Services;

public class VolunteerApplicationServiceTests
{
    private readonly Mock<IRepository<VolunteerApplication>> _mockRepository;
    private readonly Mock<IMongoDatabase> _mockDatabase;
    private readonly Mock<IMongoCollection<VolunteerApplication>> _mockCollection;
    private readonly Mock<ILogger<VolunteerApplicationService>> _mockLogger;
    private readonly IVolunteerApplicationService _service;

    public VolunteerApplicationServiceTests()
    {
        _mockRepository = new Mock<IRepository<VolunteerApplication>>();
        _mockDatabase = new Mock<IMongoDatabase>();
        _mockCollection = new Mock<IMongoCollection<VolunteerApplication>>();
        _mockLogger = new Mock<ILogger<VolunteerApplicationService>>();

        _mockDatabase
            .Setup(db => db.GetCollection<VolunteerApplication>("volunteer_applications", null))
            .Returns(_mockCollection.Object);

        _service = new VolunteerApplicationService(_mockRepository.Object, _mockDatabase.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task CreateAsyncShouldCallRepositoryCreateWhenApplicationIsValid()
    {
        // Arrange
        var application = new VolunteerApplication
        {
            FullName = "John Doe",
            Email = "john.doe@example.com",
            PhoneNumber = "123456789",
            City = "Quito",
            HasVolunteeringExperience = true,
            AreasOfInterest = new List<string> { "EventOrganization" },
            AvailableTime = "Weekends",
            SkillsOrKnowledge = "Programming, Marketing",
            WhyVolunteer = "Want to contribute to the community",
            AdditionalComments = "Available immediately"
        };

        // Act
        await _service.CreateAsync(application).ConfigureAwait(false);

        // Assert
        _mockRepository.Verify(
            repo => repo.CreateAsync(application),
            Times.Once);
    }

    [Theory]
    [InlineData("EventOrganization", true)]
    [InlineData("ContentCreation", true)]
    [InlineData("TechnicalSupport", true)]
    [InlineData("SocialMediaManagement", true)]
    [InlineData("Other", true)]
    [InlineData("InvalidArea", false)]
    public void AreValidAreasOfInterestShouldValidateAreaNamesCorrectly(string areaName, bool expectedValid)
    {
        // Arrange
        var areasOfInterest = new List<string> { areaName };

        // Act
        var result = _service.AreValidAreasOfInterest(areasOfInterest);

        // Assert
        result.Should().Be(expectedValid);
    }

    [Fact]
    public void AreValidAreasOfInterestShouldReturnTrueWhenAllAreasAreValid()
    {
        // Arrange
        var areasOfInterest = new List<string> { "EventOrganization", "TechnicalSupport" };

        // Act
        var result = _service.AreValidAreasOfInterest(areasOfInterest);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void AreValidAreasOfInterestShouldReturnFalseWhenAnyAreaIsInvalid()
    {
        // Arrange
        var areasOfInterest = new List<string> { "EventOrganization", "InvalidArea" };

        // Act
        var result = _service.AreValidAreasOfInterest(areasOfInterest);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void AreValidAreasOfInterestShouldReturnFalseWhenListIsEmpty()
    {
        // Arrange
        var areasOfInterest = new List<string>();

        // Act
        var result = _service.AreValidAreasOfInterest(areasOfInterest);

        // Assert
        result.Should().BeFalse();
    }
}
