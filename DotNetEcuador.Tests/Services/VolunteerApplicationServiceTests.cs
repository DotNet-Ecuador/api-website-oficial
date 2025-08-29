using DotNetEcuador.API.Models;
using DotNetEcuador.API.Models.Common;
using DotNetEcuador.API.Infraestructure.Services;
using DotNetEcuador.API.Infraestructure.Repositories;
using DotNetEcuador.API.Exceptions;
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

        // Mock that no existing application is found (email is unique)
        _mockRepository
            .Setup(repo => repo.FindAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<VolunteerApplication, bool>>>()))
            .ReturnsAsync((VolunteerApplication?)null);

        // Act
        await _service.CreateAsync(application);

        // Assert
        _mockRepository.Verify(repo => repo.FindAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<VolunteerApplication, bool>>>()), Times.Once);
        _mockRepository.Verify(repo => repo.CreateAsync(application), Times.Once);
    }

    [Fact]
    public async Task GetAllAsyncShouldCallRepositoryGetPagedAsync()
    {
        // Arrange
        var request = new PagedRequest { Page = 1, PageSize = 10 };
        var expectedResponse = new PagedResponse<VolunteerApplication>();

        _mockRepository
            .Setup(repo => repo.GetPagedAsync(request))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _service.GetAllAsync(request);

        // Assert
        result.Should().Be(expectedResponse);
        _mockRepository.Verify(repo => repo.GetPagedAsync(request), Times.Once);
    }

    [Fact]
    public async Task SearchAsyncShouldReturnGetAllAsyncWhenSearchTermIsEmpty()
    {
        // Arrange
        var request = new PagedRequest { Page = 1, PageSize = 10 };
        var expectedResponse = new PagedResponse<VolunteerApplication>();

        _mockRepository
            .Setup(repo => repo.GetPagedAsync(request))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _service.SearchAsync(request, string.Empty);

        // Assert
        result.Should().Be(expectedResponse);
        _mockRepository.Verify(repo => repo.GetPagedAsync(request), Times.Once);
    }

    [Fact]
    public async Task GetByEmailAsyncShouldReturnApplicationWhenEmailExists()
    {
        // Arrange
        var email = "test@example.com";
        var expectedApplication = new VolunteerApplication
        {
            FullName = "John Doe",
            Email = email,
            City = "Quito"
        };

        _mockRepository
            .Setup(repo => repo.FindAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<VolunteerApplication, bool>>>()))
            .ReturnsAsync(expectedApplication);

        // Act
        var result = await _service.GetByEmailAsync(email);

        // Assert
        result.Should().Be(expectedApplication);
        result!.Email.Should().Be(email);
        _mockRepository.Verify(repo => repo.FindAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<VolunteerApplication, bool>>>()), Times.Once);
    }

    [Fact]
    public async Task GetByEmailAsyncShouldReturnNullWhenEmailDoesNotExist()
    {
        // Arrange
        var email = "nonexistent@example.com";

        _mockRepository
            .Setup(repo => repo.FindAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<VolunteerApplication, bool>>>()))
            .ReturnsAsync((VolunteerApplication?)null);

        // Act
        var result = await _service.GetByEmailAsync(email);

        // Assert
        result.Should().BeNull();
        _mockRepository.Verify(repo => repo.FindAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<VolunteerApplication, bool>>>()), Times.Once);
    }

    [Fact]
    public async Task GetByEmailAsyncShouldReturnNullWhenEmailIsEmpty()
    {
        // Act
        var result = await _service.GetByEmailAsync(string.Empty);

        // Assert
        result.Should().BeNull();
        _mockRepository.Verify(repo => repo.FindAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<VolunteerApplication, bool>>>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsyncShouldThrowDuplicateEmailExceptionWhenEmailAlreadyExists()
    {
        // Arrange
        var email = "duplicate@example.com";
        var existingApplication = new VolunteerApplication
        {
            FullName = "Existing User",
            Email = email,
            City = "Guayaquil"
        };

        var newApplication = new VolunteerApplication
        {
            FullName = "New User",
            Email = email,
            City = "Quito"
        };

        _mockRepository
            .Setup(repo => repo.FindAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<VolunteerApplication, bool>>>()))
            .ReturnsAsync(existingApplication);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<DuplicateEmailException>(() => _service.CreateAsync(newApplication));
        
        exception.Email.Should().Be(email);
        exception.Message.Should().Contain(email);
        _mockRepository.Verify(repo => repo.CreateAsync(It.IsAny<VolunteerApplication>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsyncShouldSucceedWhenEmailIsUnique()
    {
        // Arrange
        var application = new VolunteerApplication
        {
            FullName = "John Doe",
            Email = "unique@example.com",
            PhoneNumber = "123456789",
            City = "Quito",
            HasVolunteeringExperience = true,
            AreasOfInterest = new List<string> { "EventOrganization" },
            AvailableTime = "Weekends",
            SkillsOrKnowledge = "Programming, Marketing",
            WhyVolunteer = "Want to contribute to the community",
            AdditionalComments = "Available immediately"
        };

        _mockRepository
            .Setup(repo => repo.FindAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<VolunteerApplication, bool>>>()))
            .ReturnsAsync((VolunteerApplication?)null);

        _mockRepository
            .Setup(repo => repo.CreateAsync(It.IsAny<VolunteerApplication>()))
            .Returns(Task.CompletedTask);

        // Act
        await _service.CreateAsync(application);

        // Assert
        _mockRepository.Verify(repo => repo.FindAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<VolunteerApplication, bool>>>()), Times.Once);
        _mockRepository.Verify(repo => repo.CreateAsync(application), Times.Once);
    }
}