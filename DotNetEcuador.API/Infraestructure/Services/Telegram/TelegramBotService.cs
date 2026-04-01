using DotNetEcuador.API.Models.Eventos;
using global::Telegram.Bot;
using global::Telegram.Bot.Types;
using global::Telegram.Bot.Types.Enums;
using global::Telegram.Bot.Types.ReplyMarkups;

namespace DotNetEcuador.API.Infraestructure.Services.Telegram;

public class TelegramBotService : ITelegramBotService
{
    private readonly ITelegramBotClient _bot;
    private readonly long _adminChatId;
    private readonly string _uploadsPath;

    public TelegramBotService(ITelegramBotClient bot)
    {
        _bot = bot;
        _adminChatId = long.Parse(Environment.GetEnvironmentVariable("TELEGRAM_ADMIN_CHAT_ID") ?? "0");
        _uploadsPath = Environment.GetEnvironmentVariable("UPLOADS_PATH")
            ?? Path.Combine(AppContext.BaseDirectory, "uploads", "comprobantes");
    }

    public async Task NotificarComprobanteAsync(Registro registro, Asistente asistente, Evento evento, string rutaRelativa)
    {
        if (_adminChatId == 0) return;

        var texto = $"🧾 *Nuevo comprobante subido*\n\n" +
                    $"*Evento:* {EscaparMarkdown(evento.Nombre)}\n" +
                    $"*Asistente:* {EscaparMarkdown(asistente.Nombre)}\n" +
                    $"*Email:* {EscaparMarkdown(asistente.Email)}\n" +
                    $"*ID Corto:* `{registro.IdCorto}`\n" +
                    $"*Monto:* ${evento.Precio:F2}";

        var teclado = new InlineKeyboardMarkup(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData("✅ Aprobar", $"action:aprobar:{registro.Id}:{registro.IdCorto}:{asistente.Nombre}"),
                InlineKeyboardButton.WithCallbackData("❌ Rechazar", $"action:rechazar:{registro.Id}:{registro.IdCorto}:{asistente.Nombre}")
            }
        });

        var rutaFisica = Path.Combine(_uploadsPath, rutaRelativa.Replace('/', Path.DirectorySeparatorChar));

        if (File.Exists(rutaFisica))
        {
            await using var stream = new FileStream(rutaFisica, FileMode.Open, FileAccess.Read, FileShare.Read);
            var extension = Path.GetExtension(rutaFisica).ToLowerInvariant();

            if (extension == ".pdf")
            {
                await _bot.SendDocument(
                    chatId: _adminChatId,
                    document: InputFile.FromStream(stream, Path.GetFileName(rutaFisica)),
                    caption: texto,
                    parseMode: ParseMode.Markdown,
                    replyMarkup: teclado).ConfigureAwait(false);
            }
            else
            {
                await _bot.SendPhoto(
                    chatId: _adminChatId,
                    photo: InputFile.FromStream(stream, Path.GetFileName(rutaFisica)),
                    caption: texto,
                    parseMode: ParseMode.Markdown,
                    replyMarkup: teclado).ConfigureAwait(false);
            }
        }
        else
        {
            await _bot.SendMessage(
                chatId: _adminChatId,
                text: texto,
                parseMode: ParseMode.Markdown,
                replyMarkup: teclado).ConfigureAwait(false);
        }
    }

    private static string EscaparMarkdown(string texto) =>
        texto.Replace("_", "\\_").Replace("*", "\\*").Replace("[", "\\[").Replace("`", "\\`");
}
