using api.Models;
using DotNetEcuador.API.Infraestructure.Repositories;

namespace DotNetEcuador.API.Infraestructure.Services;

public class AreaOfInterestService : IAreaOfInterestService
{
    private static readonly List<AreaOfInterest> Areas = new List<AreaOfInterest>
    {
        new AreaOfInterest { Name = "EventOrganization", Description = "Ayuda en la organización de eventos comunitarios" },
        new AreaOfInterest { Name = "ContentCreation", Description = "Creación de contenido digital para la comunidad" },
        new AreaOfInterest { Name = "TechnicalSupport", Description = "Brindar soporte técnico a proyectos de la comunidad" },
        new AreaOfInterest { Name = "SocialMediaManagement", Description = "Administración de redes sociales para la comunidad" },
        new AreaOfInterest { Name = "Other", Description = "Otras áreas de interés" }
    };

	private readonly IRepository<AreaOfInterest> _areaOfInterestRepository;
    public AreaOfInterestService(
        IRepository<AreaOfInterest> areaOfInterestRepository)
    {
        _areaOfInterestRepository = areaOfInterestRepository;
    }
    public async Task<List<AreaOfInterest>> GetAllAreasOfInterestAsync()
    {
        return await _areaOfInterestRepository.GetAllAsync();
	}

	public async Task CreateAreaOfInterestAsync(
        AreaOfInterest areaOfInterest)
	{
		await _areaOfInterestRepository.CreateAsync(areaOfInterest);
	}
}