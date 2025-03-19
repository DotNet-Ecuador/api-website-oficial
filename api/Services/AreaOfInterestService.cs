using api.Models;

namespace api.Services
{
    public class AreaOfInterestService
    {
        private static readonly List<AreaOfInterest> Areas = new List<AreaOfInterest>
        {
            new AreaOfInterest { Name = "EventOrganization", Description = "Ayuda en la organización de eventos comunitarios" },
            new AreaOfInterest { Name = "ContentCreation", Description = "Creación de contenido digital para la comunidad" },
            new AreaOfInterest { Name = "TechnicalSupport", Description = "Brindar soporte técnico a proyectos de la comunidad" },
            new AreaOfInterest { Name = "SocialMediaManagement", Description = "Administración de redes sociales para la comunidad" },
            new AreaOfInterest { Name = "Other", Description = "Otras áreas de interés" }
        };

        public List<AreaOfInterest> GetAllAreasOfInterest()
        {
            return Areas;
        }
    }
}
