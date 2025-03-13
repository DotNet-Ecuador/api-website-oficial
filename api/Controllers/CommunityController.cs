using api.Models;
using api.Services;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CommunityController : ControllerBase
    {
        private readonly CommunityService _communityService;

        public CommunityController(CommunityService communityService)
        {
            _communityService = communityService;
        }

        [HttpGet]
        public async Task<ActionResult<List<CommunityMember>>> GetAll()
        {
            var members = await _communityService.GetAllAsync();
            return Ok(members);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CommunityMember member)
        {
            if (string.IsNullOrWhiteSpace(member.FullName) || string.IsNullOrWhiteSpace(member.Email))
            {
                return BadRequest("El nombre y el correo son obligatorios.");
            }

            await _communityService.CreateAsync(member);
            return CreatedAtAction(nameof(GetAll), new { id = member.Id }, member);
        }
    }
}
