using DotNetEcuador.API.Common;
using DotNetEcuador.API.Infraestructure.Extensions;
using DotNetEcuador.API.Infraestructure.Repositories;
using DotNetEcuador.API.Models;
using DotNetEcuador.API.Models.Common;
using MongoDB.Driver;

namespace DotNetEcuador.API.Infraestructure.Services
{
    public class VolunteerApplicationService : IVolunteerApplicationService
    {
        private readonly IRepository<VolunteerApplication> _repository;
        private readonly IMongoCollection<VolunteerApplication> _collection;
        private readonly ILogger<VolunteerApplicationService> _logger;

        public VolunteerApplicationService(IRepository<VolunteerApplication> repository, IMongoDatabase database, ILogger<VolunteerApplicationService> logger)
        {
            _repository = repository;
            _collection = database.GetCollection<VolunteerApplication>(Constants.MongoCollections.VOLUNTEERAPPLICATION);
            _logger = logger;
        }

        public async Task CreateAsync(VolunteerApplication volunteerApplication)
        {
            try
            {
                _logger.LogInformation("Attempting to save VolunteerApplication to collection: {CollectionName}", Constants.MongoCollections.VOLUNTEERAPPLICATION);
                _logger.LogInformation("Application details: Name={FullName}, Email={Email}, Areas={AreasCount}", 
                    volunteerApplication.FullName, volunteerApplication.Email, volunteerApplication.AreasOfInterest?.Count ?? 0);
                
                await _repository.CreateAsync(volunteerApplication);
                
                _logger.LogInformation("VolunteerApplication saved successfully to database");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save VolunteerApplication to database");
                throw;
            }
        }

        private readonly HashSet<string> _validAreasOfInterest = new HashSet<string>
        {
            "EventOrganization",
            "ContentCreation", 
            "TechnicalSupport",
            "SocialMediaManagement",
            "Other"
        };

        public bool AreValidAreasOfInterest(List<string> selectedAreas)
        {
            if (selectedAreas == null || selectedAreas.Count == 0)
            {
                return false;
            }

            foreach (var area in selectedAreas)
            {
                if (string.IsNullOrWhiteSpace(area) || !_validAreasOfInterest.Contains(area))
                {
                    return false;
                }
            }

            return true;
        }

        public async Task<PagedResponse<VolunteerApplication>> GetAllAsync(PagedRequest request)
        {
            return await _repository.GetPagedAsync(request);
        }

        public async Task<PagedResponse<VolunteerApplication>> SearchAsync(PagedRequest request, string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return await GetAllAsync(request);
            }

            return await _collection.ToPagedResponseAsync(request, app => 
                app.FullName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                app.Email.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                app.City.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
        }
    }
}
