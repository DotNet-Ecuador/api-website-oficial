using DotNetEcuador.API.Models;
using FluentValidation;

namespace DotNetEcuador.API.Validators;

public class VolunteerApplicationValidator : AbstractValidator<VolunteerApplication>
{
    private readonly HashSet<string> _validAreasOfInterest = new()
    {
        "EventOrganization",
        "ContentCreation",
        "TechnicalSupport", 
        "SocialMediaManagement",
        "Other"
    };

    public VolunteerApplicationValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty()
            .WithMessage("El nombre completo es requerido")
            .MinimumLength(3)
            .WithMessage("El nombre debe tener al menos 3 caracteres")
            .MaximumLength(100)
            .WithMessage("El nombre no puede exceder los 100 caracteres");

        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("El email es requerido")
            .EmailAddress()
            .WithMessage("El email debe tener un formato válido")
            .MaximumLength(255)
            .WithMessage("El email no puede exceder los 255 caracteres");

        RuleFor(x => x.PhoneNumber)
            .MaximumLength(20)
            .WithMessage("El teléfono no puede exceder los 20 caracteres")
            .When(x => !string.IsNullOrEmpty(x.PhoneNumber));

        RuleFor(x => x.City)
            .NotEmpty()
            .WithMessage("La ciudad es requerida")
            .MaximumLength(100)
            .WithMessage("La ciudad no puede exceder los 100 caracteres");

        RuleFor(x => x.Country)
            .NotEmpty()
            .WithMessage("El país es requerido")
            .MaximumLength(100)
            .WithMessage("El país no puede exceder los 100 caracteres");

        RuleFor(x => x.AreasOfInterest)
            .NotEmpty()
            .WithMessage("Debe seleccionar al menos un área de interés")
            .Must(areas => areas.All(area => _validAreasOfInterest.Contains(area)))
            .WithMessage("Una o más áreas de interés seleccionadas no son válidas. Valores válidos: " + 
                        string.Join(", ", _validAreasOfInterest));

        RuleFor(x => x.OtherAreas)
            .NotEmpty()
            .WithMessage("Debe especificar las otras áreas de interés")
            .When(x => x.AreasOfInterest != null && x.AreasOfInterest.Contains("Other"));

        RuleFor(x => x.AvailableTime)
            .NotEmpty()
            .WithMessage("El tiempo disponible es requerido")
            .MaximumLength(500)
            .WithMessage("El tiempo disponible no puede exceder los 500 caracteres");

        RuleFor(x => x.SkillsOrKnowledge)
            .MaximumLength(1000)
            .WithMessage("Las habilidades no pueden exceder los 1000 caracteres")
            .When(x => !string.IsNullOrEmpty(x.SkillsOrKnowledge));

        RuleFor(x => x.WhyVolunteer)
            .NotEmpty()
            .WithMessage("Debe especificar por qué quiere ser voluntario")
            .MaximumLength(1000)
            .WithMessage("La razón no puede exceder los 1000 caracteres");

        RuleFor(x => x.AdditionalComments)
            .MaximumLength(1000)
            .WithMessage("Los comentarios adicionales no pueden exceder los 1000 caracteres")
            .When(x => !string.IsNullOrEmpty(x.AdditionalComments));
    }
}