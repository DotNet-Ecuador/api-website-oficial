using api.Models;

namespace DotNetEcuador.API.Infraestructure.Services;

public interface IAreaOfInterestService
{
    Task<List<AreaOfInterest>> GetAllAreasOfInterestAsync();
    Task CreateAreaOfInterestAsync(AreaOfInterest areaOfInterest);
}