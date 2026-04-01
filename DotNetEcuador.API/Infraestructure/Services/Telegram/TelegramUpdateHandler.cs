using System.Collections.Concurrent;
using DotNetEcuador.API.Infraestructure.Services.Eventos;
using global::Telegram.Bot;
using global::Telegram.Bot.Types;
using global::Telegram.Bot.Types.Enums;
using global::Telegram.Bot.Types.ReplyMarkups;

namespace DotNetEcuador.API.Infraestructure.Services.Telegram;

public class TelegramUpdateHandler
{
    private readonly ITelegramBotClient _bot;
    private readonly IRegistroService _registroService;
    private readonly ILogger<TelegramUpdateHandler> _logger;
    private readonly long _adminChatId;

    private static readonly ConcurrentDictionary<long, PendingAction> Estado = new();

    public TelegramUpdateHandler(
        ITelegramBotClient bot,
        IRegistroService registroService,
        ILogger<TelegramUpdateHandler> logger)
    {
        _bot = bot;
        _registroService = registroService;
        _logger = logger;
        _adminChatId = long.Parse(Environment.GetEnvironmentVariable("TELEGRAM_ADMIN_CHAT_ID") ?? "0");
    }

    public async Task HandleAsync(Update update)
    {
        try
        {
            if (update.CallbackQuery is { } callback)
                await HandleCallbackAsync(callback).ConfigureAwait(false);
            else if (update.Message?.Text is { } texto)
                await HandleMensajeAsync(update.Message.Chat.Id, texto).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error procesando update de Telegram");
        }
    }

    private async Task HandleCallbackAsync(CallbackQuery callback)
    {
        var chatId = callback.Message!.Chat.Id;
        if (chatId != _adminChatId) return;

        await _bot.AnswerCallbackQuery(callback.Id).ConfigureAwait(false);

        var data = callback.Data ?? string.Empty;

        // action:aprobar:{registroId}:{idCorto}:{nombre}
        if (data.StartsWith("action:"))
        {
            var partes = data.Split(':', 5);
            if (partes.Length < 5) return;

            var accion = partes[1] == "aprobar" ? TelegramAccion.Aprobar : TelegramAccion.Rechazar;
            var registroId = partes[2];
            var idCorto = partes[3];
            var nombre = partes[4];
            var accionTexto = accion == TelegramAccion.Aprobar ? "aprobar" : "rechazar";

            Estado[chatId] = new PendingAction
            {
                Accion = accion,
                RegistroId = registroId,
                IdCorto = idCorto,
                NombreAsistente = nombre
            };

            var teclado = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData($"Sí, {accionTexto}", $"confirm:{registroId}"),
                    InlineKeyboardButton.WithCallbackData("Cancelar", $"cancel:{registroId}")
                }
            });

            await _bot.SendMessage(
                chatId: chatId,
                text: $"¿Estás seguro de *{accionTexto}* el registro `{idCorto}` de *{nombre}*?",
                parseMode: ParseMode.Markdown,
                replyMarkup: teclado).ConfigureAwait(false);
        }
        else if (data.StartsWith("confirm:"))
        {
            if (!Estado.TryGetValue(chatId, out var pendiente)) return;

            if (pendiente.Accion == TelegramAccion.Aprobar)
            {
                Estado.TryRemove(chatId, out _);
                await AprobarAsync(chatId, pendiente).ConfigureAwait(false);
            }
            else
            {
                pendiente.EsperandoMotivo = true;
                await _bot.SendMessage(
                    chatId: chatId,
                    text: "Escribe el motivo del rechazo:").ConfigureAwait(false);
            }
        }
        else if (data.StartsWith("cancel:"))
        {
            Estado.TryRemove(chatId, out _);
            await _bot.SendMessage(chatId: chatId, text: "Acción cancelada.").ConfigureAwait(false);
        }
    }

    private async Task HandleMensajeAsync(long chatId, string texto)
    {
        if (chatId != _adminChatId) return;
        if (!Estado.TryGetValue(chatId, out var pendiente)) return;
        if (!pendiente.EsperandoMotivo) return;

        Estado.TryRemove(chatId, out _);
        await RechazarAsync(chatId, pendiente, texto).ConfigureAwait(false);
    }

    private async Task AprobarAsync(long chatId, PendingAction pendiente)
    {
        await _registroService.AprobarAsync(pendiente.RegistroId, string.Empty).ConfigureAwait(false);
        await _bot.SendMessage(
            chatId: chatId,
            text: $"✅ Registro `{pendiente.IdCorto}` aprobado\\. Email con QR enviado al asistente\\.",
            parseMode: ParseMode.MarkdownV2).ConfigureAwait(false);
        _logger.LogInformation("Admin aprobó registro {IdCorto} via Telegram", pendiente.IdCorto);
    }

    private async Task RechazarAsync(long chatId, PendingAction pendiente, string motivo)
    {
        await _registroService.RechazarAsync(pendiente.RegistroId, motivo).ConfigureAwait(false);
        await _bot.SendMessage(
            chatId: chatId,
            text: $"❌ Registro `{pendiente.IdCorto}` rechazado\\. Email enviado al asistente\\.",
            parseMode: ParseMode.MarkdownV2).ConfigureAwait(false);
        _logger.LogInformation("Admin rechazó registro {IdCorto} via Telegram", pendiente.IdCorto);
    }
}
