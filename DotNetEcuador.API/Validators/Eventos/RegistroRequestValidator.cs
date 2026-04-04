using DotNetEcuador.API.Models.Eventos.DTOs;
using FluentValidation;

namespace DotNetEcuador.API.Validators.Eventos;

public class RegistroRequestValidator : AbstractValidator<RegistroRequestDto>
{
    public RegistroRequestValidator()
    {
        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El nombre es requerido.")
            .MinimumLength(3).WithMessage("El nombre debe tener al menos 3 caracteres.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("El email es requerido.")
            .EmailAddress().WithMessage("El email no tiene un formato válido.");

        RuleFor(x => x.EventoSlug)
            .NotEmpty().WithMessage("El slug del evento es requerido.");
    }
}
