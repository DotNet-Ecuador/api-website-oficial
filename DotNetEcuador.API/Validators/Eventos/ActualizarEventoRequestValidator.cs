using DotNetEcuador.API.Models.Eventos.DTOs;
using FluentValidation;

namespace DotNetEcuador.API.Validators.Eventos;

public class ActualizarEventoRequestValidator : AbstractValidator<ActualizarEventoRequestDto>
{
    public ActualizarEventoRequestValidator()
    {
        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El nombre del evento es requerido.")
            .MinimumLength(3).WithMessage("El nombre debe tener al menos 3 caracteres.")
            .MaximumLength(120).WithMessage("El nombre no puede superar los 120 caracteres.");

        RuleFor(x => x.Precio)
            .GreaterThanOrEqualTo(0).When(x => x.Precio.HasValue)
            .WithMessage("El precio no puede ser negativo.");

        RuleFor(x => x.CapacidadMaxima)
            .GreaterThan(0).When(x => x.CapacidadMaxima.HasValue)
            .WithMessage("La capacidad máxima debe ser mayor a 0.");

        RuleFor(x => x.FechaFin)
            .GreaterThan(x => x.FechaEvento)
            .When(x => x.FechaFin.HasValue && x.FechaEvento.HasValue)
            .WithMessage("La fecha de fin debe ser posterior a la fecha de inicio.");

        RuleForEach(x => x.Speakers).ChildRules(speaker =>
        {
            speaker.RuleFor(s => s.Nombre)
                .NotEmpty().WithMessage("El nombre del speaker es requerido.")
                .MaximumLength(100).WithMessage("El nombre del speaker no puede superar los 100 caracteres.");

            speaker.RuleFor(s => s.Rol)
                .NotEmpty().WithMessage("El rol del speaker es requerido.")
                .MaximumLength(100).WithMessage("El rol del speaker no puede superar los 100 caracteres.");
        });
    }
}
