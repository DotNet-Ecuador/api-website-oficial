using DotNetEcuador.API.Models.Mentorias.DTOs;
using FluentValidation;

namespace DotNetEcuador.API.Validators.Mentorias;

public class SolicitudMentoriaRequestValidator : AbstractValidator<SolicitudMentoriaRequestDto>
{
    public SolicitudMentoriaRequestValidator()
    {
        RuleFor(x => x.NombreCompleto)
            .NotEmpty().WithMessage("El nombre completo es requerido.")
            .Length(3, 100).WithMessage("El nombre debe tener entre 3 y 100 caracteres.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("El correo electrónico es requerido.")
            .EmailAddress().WithMessage("Ingresa un correo electrónico válido.");

        RuleFor(x => x.Telefono)
            .NotEmpty().WithMessage("El número de teléfono es requerido.")
            .Matches(@"^\+?[0-9\s\-]{7,15}$").WithMessage("Ingresa un número de teléfono válido.");

        RuleFor(x => x.InstitucionId)
            .NotEmpty().WithMessage("Selecciona tu institución.");

        RuleFor(x => x.OtraInstitucion)
            .NotEmpty().WithMessage("Especifica el nombre de tu institución.")
            .MaximumLength(100).WithMessage("El nombre de la institución no debe superar 100 caracteres.")
            .When(x => x.InstitucionId == "otros");

        RuleFor(x => x.TemaConsulta)
            .NotEmpty().WithMessage("Describe el tema de tu mentoría.")
            .MinimumLength(10).WithMessage("Describe el tema con al menos 10 caracteres.")
            .MaximumLength(1000).WithMessage("El tema no debe superar 1000 caracteres.");
    }
}
