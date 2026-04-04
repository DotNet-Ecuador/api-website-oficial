using DotNetEcuador.API.Common;
using DotNetEcuador.API.Exceptions;
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
        private readonly IEmailNotificationService _emailNotification;

        public VolunteerApplicationService(IRepository<VolunteerApplication> repository, IMongoDatabase database, ILogger<VolunteerApplicationService> logger, IEmailNotificationService emailNotification)
        {
            _repository = repository;
            _collection = database.GetCollection<VolunteerApplication>(Constants.MongoCollections.VOLUNTEERAPPLICATION);
            _logger = logger;
            _emailNotification = emailNotification;
        }

        public async Task CreateAsync(VolunteerApplication volunteerApplication)
        {
            try
            {
                _logger.LogInformation("Attempting to save VolunteerApplication to collection: {CollectionName}", Constants.MongoCollections.VOLUNTEERAPPLICATION);
                _logger.LogInformation("Application details: Name={FullName}, Email={Email}, Areas={AreasCount}", 
                    volunteerApplication.FullName, volunteerApplication.Email, volunteerApplication.AreasOfInterest?.Count ?? 0);
                
                // Validar si ya existe un voluntario con este email
                var existingApplication = await GetByEmailAsync(volunteerApplication.Email);
                if (existingApplication != null)
                {
                    _logger.LogWarning("Attempt to create duplicate volunteer application with email: {Email}", volunteerApplication.Email);
                    throw new DuplicateEmailException(volunteerApplication.Email);
                }
                
                // Asignar fecha de creación en hora Ecuador (UTC-5)
                var ecuadorZone = TimeZoneInfo.FindSystemTimeZoneById(Constants.TimeZones.Ecuador);
                volunteerApplication.CreatedAt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, ecuadorZone);
                
                await _repository.CreateAsync(volunteerApplication);

                _logger.LogInformation("VolunteerApplication saved successfully to database");

                // Notificar al admin sin bloquear el flujo
                _ = NotificarAdminNuevoVoluntarioAsync(volunteerApplication);
            }
            catch (DuplicateEmailException)
            {
                // Re-throw la excepción de email duplicado sin logging adicional
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save VolunteerApplication to database");
                throw;
            }
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

        public async Task<VolunteerApplication?> GetByEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return null;
            }

            _logger.LogDebug("Searching for volunteer application with email: {Email}", email);

            var result = await _repository.FindAsync(app => app.Email.ToLower() == email.ToLower());

            _logger.LogDebug("Search result for email {Email}: {Found}", email, result != null ? "Found" : "Not found");

            return result;
        }

        private async Task NotificarAdminNuevoVoluntarioAsync(VolunteerApplication app)
        {
            try
            {
                var areas = app.AreasOfInterest?.Count > 0
                    ? string.Join(", ", app.AreasOfInterest)
                    : "No especificadas";

                var html = $@"<h3>Nueva solicitud de voluntariado</h3>
<p><strong>Nombre:</strong> {app.FullName}</p>
<p><strong>Email:</strong> {app.Email}</p>
<p><strong>Ciudad:</strong> {app.City}</p>
<p><strong>Áreas de interés:</strong> {areas}</p>
<p><strong>Registrado en:</strong> {app.CreatedAt:dd/MM/yyyy HH:mm} (Ecuador)</p>";

                await _emailNotification.NotificarAdminAsync(
                    $"Nuevo voluntario: {app.FullName}",
                    html).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enviando notificación de nuevo voluntario para {Email}", app.Email);
            }
        }
    }
}
