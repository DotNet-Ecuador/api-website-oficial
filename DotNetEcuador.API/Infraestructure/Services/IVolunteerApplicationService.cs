using DotNetEcuador.API.Models;

namespace DotNetEcuador.API.Infraestructure.Services;

public interface IVolunteerApplicationService
{
    Task CreateAsync(VolunteerApplication volunteerApplication);
    bool AreValidAreasOfInterest(Dictionary<string, bool> selectedAreas);
}
