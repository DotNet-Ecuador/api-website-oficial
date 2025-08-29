using DotNetEcuador.API.Models.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Diagnostics;

namespace DotNetEcuador.API.Filters;

public class ValidationActionFilter : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (!context.ModelState.IsValid)
        {
            var errors = context.ModelState
                .Where(x => x.Value?.Errors.Count > 0)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray() ?? Array.Empty<string>()
                );

            var traceId = Activity.Current?.Id ?? context.HttpContext.TraceIdentifier;
            var apiError = ApiError.ValidationError(
                context.HttpContext.Request.Path,
                errors,
                traceId
            );

            context.Result = new BadRequestObjectResult(apiError);
        }
    }
}