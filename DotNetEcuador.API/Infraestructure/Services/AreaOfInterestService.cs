using DotNetEcuador.API.Models;
using DotNetEcuador.API.Infraestructure.Repositories;

namespace DotNetEcuador.API.Infraestructure.Services;

public class AreaOfInterestService : IAreaOfInterestService
{

    private readonly IRepository<AreaOfInterest> _areaOfInterestRepository;
    public AreaOfInterestService(
        IRepository<AreaOfInterest> areaOfInterestRepository)
    {
        _areaOfInterestRepository = areaOfInterestRepository;
    }

    public async Task<List<AreaOfInterest>> GetAllAreasOfInterestAsync()
    {
        return await _areaOfInterestRepository.GetAllAsync().ConfigureAwait(false);
    }

    public async Task CreateAreaOfInterestAsync(
        AreaOfInterest areaOfInterest)
    {
        await _areaOfInterestRepository.CreateAsync(areaOfInterest).ConfigureAwait(false);
    }
}
