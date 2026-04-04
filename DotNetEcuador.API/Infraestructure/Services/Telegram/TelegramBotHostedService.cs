using global::Telegram.Bot;
using global::Telegram.Bot.Types;

namespace DotNetEcuador.API.Infraestructure.Services.Telegram;

public class TelegramBotHostedService : BackgroundService
{
    private readonly ITelegramBotClient _bot;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<TelegramBotHostedService> _logger;
    private int _offset;

    public TelegramBotHostedService(
        ITelegramBotClient bot,
        IServiceScopeFactory scopeFactory,
        ILogger<TelegramBotHostedService> logger)
    {
        _bot = bot;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Telegram bot iniciado");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var updates = await _bot.GetUpdates(
                    offset: _offset,
                    timeout: 30,
                    cancellationToken: stoppingToken).ConfigureAwait(false);

                foreach (var update in updates)
                {
                    _offset = update.Id + 1;
                    _ = ProcessUpdateAsync(update);
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en polling de Telegram. Reintentando en 5s.");
                await Task.Delay(5000, stoppingToken).ConfigureAwait(false);
            }
        }
    }

    private async Task ProcessUpdateAsync(Update update)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var handler = scope.ServiceProvider.GetRequiredService<TelegramUpdateHandler>();
            await handler.HandleAsync(update).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error procesando update {UpdateId}", update.Id);
        }
    }
}
