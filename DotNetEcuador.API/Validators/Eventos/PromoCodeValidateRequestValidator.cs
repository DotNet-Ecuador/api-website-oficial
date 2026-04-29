using DotNetEcuador.API.Models.Eventos.DTOs;
using FluentValidation;

namespace DotNetEcuador.API.Validators.Eventos;

public class PromoCodeValidateRequestValidator : AbstractValidator<PromoCodeValidateRequestDto>
{
    public PromoCodeValidateRequestValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("El código es requerido.")
            .Length(6).WithMessage("El código debe tener exactamente 6 caracteres.")
            .Matches("^[A-Za-z0-9]{6}$").WithMessage("El código solo puede contener letras y números.");
    }
}
