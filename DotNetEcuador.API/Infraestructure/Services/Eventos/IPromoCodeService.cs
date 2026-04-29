using DotNetEcuador.API.Models.Eventos.DTOs;

namespace DotNetEcuador.API.Infraestructure.Services.Eventos;

public interface IPromoCodeService
{
    Task<PromoCodeValidateResponseDto> ValidateAsync(string code);
}
