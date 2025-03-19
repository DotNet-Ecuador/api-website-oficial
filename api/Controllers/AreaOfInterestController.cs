using api.Models;
using api.Services;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class AreaOfInterestController : ControllerBase
    {
        private readonly AreaOfInterestService _areaOfInterestService;

        public AreaOfInterestController(AreaOfInterestService areaOfInterestService)
        {
            _areaOfInterestService = areaOfInterestService;
        }

        [HttpGet()]
        public ActionResult<List<AreaOfInterest>> GetAllAreasOfInterest()
        {
            var areas = _areaOfInterestService.GetAllAreasOfInterest();
            return Ok(areas);
        }
    }
}
