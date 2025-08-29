using DotNetEcuador.API.Models;
using DotNetEcuador.API.Models.Common;

namespace DotNetEcuador.API.Infraestructure.Services;

public interface IVolunteerApplicationService
{
    Task CreateAsync(VolunteerApplication volunteerApplication);
    bool AreValidAreasOfInterest(List<string> selectedAreas);
    Task<PagedResponse<VolunteerApplication>> GetAllAsync(PagedRequest request);
    Task<PagedResponse<VolunteerApplication>> SearchAsync(PagedRequest request, string searchTerm);
}
