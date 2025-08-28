using DotNetEcuador.API.Models;
using DotNetEcuador.API.Infraestructure.Services;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/v1/area-interest")]
public class AreaOfInterestController : ControllerBase
{
    private readonly IAreaOfInterestService _areaOfInterestService;

    public AreaOfInterestController(IAreaOfInterestService areaOfInterestService)
    {
        _areaOfInterestService = areaOfInterestService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllAreasOfInterest()
    {
        var areas = await _areaOfInterestService.GetAllAreasOfInterestAsync().ConfigureAwait(false);
        return Ok(areas);
    }

    [HttpPost]
    public async Task<IActionResult> CreateAreaOfInterest(
        AreaOfInterest areaOfInterest)
    {
        await _areaOfInterestService.CreateAreaOfInterestAsync(areaOfInterest).ConfigureAwait(false);
        return Ok();
    }
}
