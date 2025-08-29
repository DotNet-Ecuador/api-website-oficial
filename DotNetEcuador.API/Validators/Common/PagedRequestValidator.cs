using DotNetEcuador.API.Models.Common;
using FluentValidation;

namespace DotNetEcuador.API.Validators.Common;

public class PagedRequestValidator : AbstractValidator<PagedRequest>
{
    public PagedRequestValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThan(0)
            .WithMessage("El número de página debe ser mayor a 0");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage("El tamaño de página debe estar entre 1 y 100");

        RuleFor(x => x.SortOrder)
            .Must(order => string.IsNullOrEmpty(order) || order.ToLower() == "asc" || order.ToLower() == "desc")
            .WithMessage("El orden debe ser 'asc' o 'desc'")
            .When(x => !string.IsNullOrEmpty(x.SortOrder));

        RuleFor(x => x.Search)
            .MaximumLength(100)
            .WithMessage("El término de búsqueda no puede exceder los 100 caracteres")
            .When(x => !string.IsNullOrEmpty(x.Search));
    }
}