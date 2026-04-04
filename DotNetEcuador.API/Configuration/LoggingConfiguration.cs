using Serilog;
using Serilog.Events;

namespace DotNetEcuador.API.Configuration;

public static class LoggingConfiguration
{
    private const string OutputTemplate =
        "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {SourceContext} {Message:lj}{NewLine}{Exception}";

    public static void ConfigureSerilog(this WebApplicationBuilder builder)
    {
        var logsPath = builder.Configuration["LOGS_PATH"]
            ?? Environment.GetEnvironmentVariable("LOGS_PATH")
            ?? Path.Combine(AppContext.BaseDirectory, "logs");

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
            .MinimumLevel.Override("System", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .WriteTo.Console(outputTemplate: OutputTemplate)
            .WriteTo.File(
                path: Path.Combine(logsPath, "api-dotnetecuador-.log"),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 30,
                outputTemplate: OutputTemplate,
                shared: false,
                flushToDiskInterval: TimeSpan.FromSeconds(1))
            .WriteTo.File(
                path: Path.Combine(logsPath, "errors-dotnetecuador-.log"),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 90,
                restrictedToMinimumLevel: LogEventLevel.Error,
                outputTemplate: OutputTemplate,
                shared: false,
                flushToDiskInterval: TimeSpan.FromSeconds(1))
            .CreateLogger();

        builder.Host.UseSerilog();
    }
}
