using DotNetEcuador.API.Infraestructure.Repositories;
using DotNetEcuador.API.Models.Eventos;
using DotNetEcuador.API.Models.Eventos.DTOs;

namespace DotNetEcuador.API.Infraestructure.Services.Eventos;

public class PromoCodeService : IPromoCodeService
{
    private readonly IRepository<PromoCode> _promoCodeRepository;
    private readonly ILogger<PromoCodeService> _logger;

    public PromoCodeService(
        IRepository<PromoCode> promoCodeRepository,
        ILogger<PromoCodeService> logger)
    {
        _promoCodeRepository = promoCodeRepository;
        _logger = logger;
    }

    public async Task<PromoCodeValidateResponseDto> ValidateAsync(string code)
    {
        var upperCode = code.ToUpperInvariant();

        var promoCode = await _promoCodeRepository.FindAsync(p =>
            p.Code == upperCode &&
            p.IsActive).ConfigureAwait(false);

        if (promoCode is null)
            return Invalid();

        if (promoCode.MaxUses.HasValue && promoCode.CurrentUses >= promoCode.MaxUses.Value)
            return Invalid();

        if (promoCode.ExpiresAt.HasValue && promoCode.ExpiresAt.Value < DateTime.UtcNow)
            return Invalid();

        _logger.LogInformation("Promo code {Code} validated successfully", upperCode);

        return new PromoCodeValidateResponseDto
        {
            Valid = true,
            Message = "¡Código aplicado! Tu acceso está confirmado."
        };
    }

    private static PromoCodeValidateResponseDto Invalid() =>
        new() { Valid = false, Message = "Código no válido o expirado." };
}
