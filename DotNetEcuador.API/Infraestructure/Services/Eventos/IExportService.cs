namespace DotNetEcuador.API.Infraestructure.Services.Eventos;

public interface IExportService
{
    Task<string> ExportarRegistradosCsvAsync(string eventoId);
}
