using Asp.Versioning;
using DotNetEcuador.API.Infraestructure.Repositories;
using DotNetEcuador.API.Models;
using DotNetEcuador.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace DotNetEcuador.API.Controllers.V1;

[Route("api/v{version:apiVersion}/pago")]
[ApiVersion("1.0")]
public class PagoController : BaseApiController
{
    private readonly IRepository<DatosPago> _datosPagoRepo;

    public PagoController(
        IRepository<DatosPago> datosPagoRepo,
        IMessageService messageService,
        ILogger<PagoController> logger) : base(messageService, logger)
    {
        _datosPagoRepo = datosPagoRepo;
    }

    /// <summary>
    /// Obtiene los datos bancarios para realizar el pago
    /// </summary>
    [HttpGet("datos-cuenta")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetDatosCuenta()
    {
        var todos = await _datosPagoRepo.GetAllAsync().ConfigureAwait(false);
        var datos = todos.FirstOrDefault();
        if (datos is null)
            return NotFoundError("Datos de pago no configurados.");

        return SuccessResponse(new
        {
            datos.Banco,
            datos.TipoCuenta,
            datos.NumeroCuenta,
            datos.Titular
        });
    }
}
