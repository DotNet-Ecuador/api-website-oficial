using Microsoft.AspNetCore.Mvc;

namespace DotNetEcuador.API.Models.Common;

public class ApiError : ProblemDetails
{
    public Dictionary<string, string[]> Errors { get; set; } = new();
    public string TraceId { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public ApiError()
    {
        Type = "https://api.dotnetecuador.org/problems/error";
    }

    public static ApiError ValidationError(string instance, Dictionary<string, string[]> errors, string? traceId = null)
    {
        return new ApiError
        {
            Type = "https://api.dotnetecuador.org/problems/validation-error",
            Title = "Error de Validación",
            Status = 400,
            Detail = "Se encontraron uno o más errores de validación.",
            Instance = instance,
            Errors = errors,
            TraceId = traceId ?? string.Empty
        };
    }

    public static ApiError BusinessError(string code, string message, string instance, int status = 400, string? traceId = null)
    {
        return new ApiError
        {
            Type = $"https://api.dotnetecuador.org/problems/{code}",
            Title = "Error de Negocio",
            Status = status,
            Detail = message,
            Instance = instance,
            TraceId = traceId ?? string.Empty
        };
    }

    public static ApiError ServerError(string message, string instance, string? traceId = null)
    {
        return new ApiError
        {
            Type = "https://api.dotnetecuador.org/problems/server-error",
            Title = "Error del Servidor",
            Status = 500,
            Detail = message,
            Instance = instance,
            TraceId = traceId ?? string.Empty
        };
    }
}