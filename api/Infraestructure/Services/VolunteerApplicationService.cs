using api.Models;
using MongoDB.Driver;

namespace DotNetEcuador.API.Infraestructure.Services
{
    public class VolunteerApplicationService
    {
        private readonly IMongoCollection<VolunteerApplication> _volunteerApplicationCollection;

        public VolunteerApplicationService(IMongoDatabase database)
        {
            _volunteerApplicationCollection = database.GetCollection<VolunteerApplication>("volunteer_applications");
        }

        public async Task CreateAsync(VolunteerApplication volunteerApplication)
        {
            await _volunteerApplicationCollection.InsertOneAsync(volunteerApplication);
        }

        private readonly Dictionary<string, string> _areasOfInterest = new Dictionary<string, string>
        {
            { "EventOrganization", "Ayuda en la organización de eventos comunitarios" },
            { "ContentCreation", "Creación de contenido digital para la comunidad" },
            { "TechnicalSupport", "Brindar soporte técnico a proyectos de la comunidad" },
            { "SocialMediaManagement", "Administración de redes sociales para la comunidad" },
            { "Other", "Otras áreas de interés" }
        };

        public bool AreValidAreasOfInterest(Dictionary<string, bool> selectedAreas)
        {
            foreach (var area in selectedAreas)
            {
                if (area.Value && !_areasOfInterest.ContainsKey(area.Key))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
