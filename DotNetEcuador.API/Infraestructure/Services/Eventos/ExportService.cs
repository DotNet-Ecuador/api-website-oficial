using System.Globalization;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using DotNetEcuador.API.Infraestructure.Repositories;
using DotNetEcuador.API.Models.Eventos;

namespace DotNetEcuador.API.Infraestructure.Services.Eventos;

public class ExportService : IExportService
{
    private readonly IRepository<Registro> _registroRepo;
    private readonly IRepository<Asistente> _asistenteRepo;
    private readonly IEventoService _eventoService;

    public ExportService(
        IRepository<Registro> registroRepo,
        IRepository<Asistente> asistenteRepo,
        IEventoService eventoService)
    {
        _registroRepo = registroRepo;
        _asistenteRepo = asistenteRepo;
        _eventoService = eventoService;
    }

    public async Task<string> ExportarRegistradosCsvAsync(string eventoId)
    {
        var todos = await _registroRepo.GetAllAsync().ConfigureAwait(false);
        var pagados = todos.Where(r => r.EventoId == eventoId && r.Estado == EstadoRegistro.Pagado).ToList();

        var eventos = await _eventoService.GetAllAsync().ConfigureAwait(false);
        var evento = eventos.FirstOrDefault(e => e.Id == eventoId);

        var filas = new List<ExportCsvRow>();
        foreach (var registro in pagados)
        {
            var asistente = await _asistenteRepo.GetByIdAsync(registro.AsistenteId).ConfigureAwait(false);
            if (asistente is null || !asistente.AceptaMarketing) continue;

            filas.Add(new ExportCsvRow
            {
                Nombre = asistente.Nombre,
                Email = asistente.Email,
                Empresa = asistente.Empresa,
                Cargo = asistente.Cargo,
                Telefono = asistente.Telefono,
                Evento = evento?.Nombre ?? string.Empty,
                FechaRegistro = registro.RegistradoEn.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                AceptaMarketing = asistente.AceptaMarketing
            });
        }

        var config = new CsvConfiguration(CultureInfo.InvariantCulture) { HasHeaderRecord = true };
        using var writer = new StringWriter(new StringBuilder());
        using var csv = new CsvWriter(writer, config);
        await csv.WriteRecordsAsync(filas).ConfigureAwait(false);
        return writer.ToString();
    }

    private sealed class ExportCsvRow
    {
        public string Nombre { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Empresa { get; set; } = string.Empty;
        public string Cargo { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string Evento { get; set; } = string.Empty;
        public string FechaRegistro { get; set; } = string.Empty;
        public bool AceptaMarketing { get; set; }
    }
}
