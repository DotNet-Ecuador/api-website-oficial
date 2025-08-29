using DotNetEcuador.API.Exceptions;
using DotNetEcuador.API.Models.Common;
using System.Diagnostics;
using System.Net;
using System.Text.Json;

namespace DotNetEcuador.API.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    private readonly IWebHostEnvironment _environment;

    public GlobalExceptionMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger,
        IWebHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred");
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var traceId = Activity.Current?.Id ?? context.TraceIdentifier;
        var response = context.Response;

        response.ContentType = "application/json";

        ApiError apiError = exception switch
        {
            DuplicateEmailException ex => ApiError.BusinessError(
                "duplicate-email",
                ex.Message,
                context.Request.Path,
                409,
                traceId),

            UnauthorizedAccessException _ => ApiError.BusinessError(
                "unauthorized", 
                exception.Message, 
                context.Request.Path, 
                401, 
                traceId),
            
            ArgumentException _ => ApiError.BusinessError(
                "invalid-argument", 
                exception.Message, 
                context.Request.Path, 
                400, 
                traceId),
                
            InvalidOperationException _ => ApiError.BusinessError(
                "invalid-operation", 
                exception.Message, 
                context.Request.Path, 
                400, 
                traceId),
                
            _ => ApiError.ServerError(
                _environment.IsDevelopment() 
                    ? exception.Message 
                    : "Ha ocurrido un error interno del servidor.",
                context.Request.Path,
                traceId)
        };

        response.StatusCode = apiError.Status ?? 500;

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

        var jsonResponse = JsonSerializer.Serialize(apiError, jsonOptions);
        await response.WriteAsync(jsonResponse);
    }
}