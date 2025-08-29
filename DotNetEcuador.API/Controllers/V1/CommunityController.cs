using System.ComponentModel.DataAnnotations;
using DotNetEcuador.API.Common;
using DotNetEcuador.API.Controllers;
using DotNetEcuador.API.Models;
using DotNetEcuador.API.Infraestructure.Services;
using DotNetEcuador.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace DotNetEcuador.API.Controllers.V1;

[Route("api/v{version:apiVersion}/community-member")]
public class CommunityController : BaseApiController
{
    private readonly CommunityService _communityService;

    public CommunityController(
        CommunityService communityService,
        IMessageService messageService,
        ILogger<CommunityController> logger) : base(messageService, logger)
    {
        _communityService = communityService;
    }

    /// <summary>
    /// Registra un nuevo miembro en la comunidad DotNet Ecuador
    /// </summary>
    /// <param name="member">Datos del nuevo miembro de la comunidad</param>
    /// <returns>Información del miembro creado</returns>
    /// <response code="201">Miembro registrado exitosamente</response>
    /// <response code="400">Error de validación en los datos del miembro</response>
    /// <response code="500">Error interno del servidor</response>
    /// <remarks>
    /// Este endpoint permite registrar nuevos miembros en la comunidad oficial.
    /// Solo requiere información básica de contacto.
    /// 
    /// Ejemplo de solicitud:
    /// 
    ///     POST /api/v1/community-member
    ///     {
    ///         "fullName": "María González Pérez",
    ///         "email": "maria.gonzalez@ejemplo.com"
    ///     }
    /// 
    /// Validaciones:
    /// - fullName: Mínimo 3 caracteres
    /// - email: Formato de email válido
    /// </remarks>
    [HttpPost]
    [ProducesResponseType(typeof(CommunityMember), 201)]
    [ProducesResponseType(typeof(object), 400)]
    [ProducesResponseType(typeof(object), 500)]
    public async Task<IActionResult> Create(CommunityMember member)
    {
        if (string.IsNullOrWhiteSpace(member.FullName) || member.FullName.Length < 3)
        {
            return BadRequest(GetMessage(MessageKeys.ValidationFullName));
        }

        if (string.IsNullOrWhiteSpace(member.Email) || !new EmailAddressAttribute().IsValid(member.Email))
        {
            return BadRequest(GetMessage(MessageKeys.ValidationEmail));
        }

        await _communityService.CreateAsync(member).ConfigureAwait(false);
        return CreatedAtAction(nameof(Create), new { id = member.Id }, member);
    }
}
