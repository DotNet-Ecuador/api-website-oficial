using DotNetEcuador.API.Infraestructure.Services;
using DotNetEcuador.API.Infraestructure.Services.Eventos;
using DotNetEcuador.API.Infraestructure.Services.Telegram;
using DotNetEcuador.API.Services.Auth;
using DotNetEcuador.API.Models;
using Telegram.Bot;

namespace DotNetEcuador.API.Configuration;

public static class ServicesConfiguration
{
    public static void ConfigureApplicationServices(this IServiceCollection services)
    {
        // Register application services
        services.AddScoped<CommunityService>();
        services.AddScoped<IVolunteerApplicationService, VolunteerApplicationService>();
        services.AddScoped<IAreaOfInterestService, AreaOfInterestService>();

        // Register authentication services
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IPasswordService, PasswordService>();

        // Register email notification service
        services.AddScoped<IEmailNotificationService, EmailNotificationService>();

        // Register file storage
        services.AddSingleton<IFileStorageService, FileStorageService>();

        // Register Telegram bot
        var telegramToken = Environment.GetEnvironmentVariable("TELEGRAM_DOTNETECUADOR_BOT_TOKEN");
        if (!string.IsNullOrEmpty(telegramToken))
        {
            services.AddSingleton<ITelegramBotClient>(_ => new TelegramBotClient(telegramToken));
            services.AddSingleton<ITelegramBotService, TelegramBotService>();
            services.AddScoped<TelegramUpdateHandler>();
            services.AddHostedService<TelegramBotHostedService>();
        }
        else
        {
            services.AddSingleton<ITelegramBotService, NullTelegramBotService>();
        }

        // Register eventos services
        services.AddScoped<IEventoService, EventoService>();
        services.AddScoped<IRegistroService, RegistroService>();
        services.AddScoped<IQrService, QrService>();
        services.AddScoped<IEmailEventoService, EmailEventoService>();
        services.AddScoped<IExportService, ExportService>();
    }
}