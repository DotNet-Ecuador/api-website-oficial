using Asp.Versioning;
using DotNetEcuador.API.Services;
using Microsoft.AspNetCore.Mvc;

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
}