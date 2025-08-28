using System.ComponentModel.DataAnnotations;
using DotNetEcuador.API.Models;
using DotNetEcuador.API.Infraestructure.Services;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/v1/community-member")]
public class CommunityController : ControllerBase
{
    private readonly CommunityService _communityService;

    public CommunityController(CommunityService communityService)
    {
        _communityService = communityService;
    }

    [HttpPost]
    public async Task<IActionResult> Create(CommunityMember member)
    {
        if (string.IsNullOrWhiteSpace(member.FullName) || member.FullName.Length < 3)
        {
            return BadRequest("El nombre completo debe tener al menos 3 caracteres.");
        }

        if (string.IsNullOrWhiteSpace(member.Email) || !new EmailAddressAttribute().IsValid(member.Email))
        {
            return BadRequest("El correo electrónico no tiene un formato válido.");
        }

        await _communityService.CreateAsync(member).ConfigureAwait(false);
        return CreatedAtAction(nameof(Create), new { id = member.Id }, member);
    }
}
