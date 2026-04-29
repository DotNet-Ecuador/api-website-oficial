using Asp.Versioning;
using DotNetEcuador.API.Controllers;
using DotNetEcuador.API.Infraestructure.Services.Eventos;
using DotNetEcuador.API.Models.Eventos.DTOs;
using DotNetEcuador.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace DotNetEcuador.API.Controllers.V1;

[Route("api/v{version:apiVersion}/promo-codes")]
[ApiVersion("1.0")]
public class PromoCodesController : BaseApiController
{
    private readonly IPromoCodeService _promoCodeService;

    public PromoCodesController(
        IPromoCodeService promoCodeService,
        IMessageService messageService,
        ILogger<PromoCodesController> logger) : base(messageService, logger)
    {
        _promoCodeService = promoCodeService;
    }

    /// <summary>
    /// Validates a promo code. Does not increment usage — only validates eligibility.
    /// </summary>
    [HttpPost("validate")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(422)]
    public async Task<IActionResult> Validate([FromBody] PromoCodeValidateRequestDto request)
    {
        var result = await _promoCodeService.ValidateAsync(request.Code).ConfigureAwait(false);

        if (!result.Valid)
            return BusinessError("promo-invalido", result.Message, 422);

        return SuccessResponse(result, result.Message);
    }
}
