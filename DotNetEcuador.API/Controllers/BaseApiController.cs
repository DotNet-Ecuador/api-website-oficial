using Asp.Versioning;
using DotNetEcuador.API.Services;
using DotNetEcuador.API.Models.Common;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace DotNetEcuador.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
public abstract class BaseApiController : ControllerBase
{
    protected readonly IMessageService MessageService;
    protected readonly ILogger Logger;

    protected BaseApiController(IMessageService messageService, ILogger logger)
    {
        MessageService = messageService;
        Logger = logger;
    }

    protected string GetMessage(string key) => MessageService.GetMessage(key);
    protected string GetMessage(string key, params object[] args) => MessageService.GetMessage(key, args);

    // Standard API Response Methods
    protected IActionResult SuccessResponse<T>(T data, string message = "")
    {
        var response = new ApiResponse<T>
        {
            Success = true,
            Data = data,
            Message = string.IsNullOrEmpty(message) ? "Operación completada exitosamente" : message
        };
        return Ok(response);
    }

    protected IActionResult SuccessResponse(string message = "")
    {
        var response = new ApiResponse
        {
            Success = true,
            Message = string.IsNullOrEmpty(message) ? "Operación completada exitosamente" : message
        };
        return Ok(response);
    }

    protected ActionResult BusinessError(string code, string message, int statusCode = 400)
    {
        var traceId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        var apiError = ApiError.BusinessError(code, message, HttpContext.Request.Path, statusCode, traceId);
        return StatusCode(statusCode, apiError);
    }

    protected ActionResult ValidationErrors()
    {
        if (ModelState.IsValid)
        {
            return BadRequest("No validation errors found");
        }

        var errors = ModelState
            .Where(x => x.Value?.Errors.Count > 0)
            .ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray() ?? Array.Empty<string>()
            );

        var traceId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        var apiError = ApiError.ValidationError(HttpContext.Request.Path, errors, traceId);
        return BadRequest(apiError);
    }

    protected ActionResult NotFoundError(string message = "Recurso no encontrado")
    {
        return BusinessError("not-found", message, 404);
    }

    protected ActionResult UnauthorizedError(string message = "No autorizado")
    {
        return BusinessError("unauthorized", message, 401);
    }

    protected ActionResult ForbiddenError(string message = "Acceso prohibido")
    {
        return BusinessError("forbidden", message, 403);
    }
}