using System.Net.Http.Json;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AyanamiConnect.BOT;

public static class Program
{
    public static async Task Main(string[] args)
    {
        var token = Environment.GetEnvironmentVariable("TELEGRAM_BOT_TOKEN");
        // Берем URL бэкенда из переменной окружения, либо используем локальный Kestrel по умолчанию
        var backendUrl = Environment.GetEnvironmentVariable("BACKEND_API_URL") ?? "http://localhost:5118";

        if (string.IsNullOrEmpty(token))
        {
            throw new Exception("Критическая ошибка: TELEGRAM_BOT_TOKEN не задан!");
        }
        
        var webAppUrl = Environment.GetEnvironmentVariable("TELEGRAM_WEBAPP_URL") ?? "http://localhost:5173";
        var canUseWebAppButton = Uri.TryCreate(webAppUrl, UriKind.Absolute, out var parsedWebAppUrl)
                                 && parsedWebAppUrl.Scheme.Equals(Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase)
                                 && !parsedWebAppUrl.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase)
                                 && !parsedWebAppUrl.Host.Equals("127.0.0.1", StringComparison.OrdinalIgnoreCase);

        using var telegramClient = new HttpClient
        {
            BaseAddress = new Uri($"https://api.telegram.org/bot{token}/")
        };

        var meResponse = await telegramClient.GetFromJsonAsync<TelegramResponse<TelegramUser>>("getMe");
        if (meResponse is null || !meResponse.Ok || meResponse.Result is null)
            throw new InvalidOperationException("Telegram bot token is invalid or Telegram API is unavailable.");

        Console.WriteLine($"Bot started: @{meResponse.Result.Username}");

        var offset = 0;

        while (true)
        {
            try
            {
                var updatesUrl = $"getUpdates?timeout=30&offset={offset}&allowed_updates=%5B%22message%22%5D";
                var updatesResponse = await telegramClient.GetFromJsonAsync<TelegramResponse<List<Update>>>(updatesUrl);

                if (updatesResponse?.Ok == true && updatesResponse.Result is { Count: > 0 } updates)
                {
                    foreach (var update in updates)
                    {
                        offset = update.UpdateId + 1;

                        var message = update.Message;
                        if (message?.Text is null)
                            continue;

                        if (!message.Text.StartsWith("/start", StringComparison.OrdinalIgnoreCase))
                            continue;

                        var user = message.From;
                        if (user is null)
                            continue;

                        // Логируем в консоль бота данные входящего юзера
                        Console.WriteLine($"[Лог]: Пытаемся зарегистрировать ТГ ID: {user.Id} (@{user.Username})");

                        // --- ОТПРАВЛЯЕМ ЗАПРОС НА БЭКЕНД ---
                        try
                        {
                            using var backendClient = new HttpClient();
                            backendClient.BaseAddress = new Uri(backendUrl);

                            // Анонимный объект полностью совпадает по именам свойств с твоим DTO на бэкенде
                            var registerData = new
                            {
                                telegramId = user.Id,
                                username = user.Username,
                                firstName = user.FirstName,
                                lastName = user.LastName
                            };

                            // Стучимся на твой эндпоинт POST /api/users/
                            var backendResponse = await backendClient.PostAsJsonAsync("api/users", registerData);

                            if (backendResponse.IsSuccessStatusCode)
                            {
                                Console.WriteLine($"[Бэкенд]: Юзер {user.Id} успешно обработан бэкендом.");
                            }
                            else
                            {
                                var errorContent = await backendResponse.Content.ReadAsStringAsync();
                                Console.WriteLine($"[Предупреждение бэкенда]: Код {backendResponse.StatusCode}, Ответ: {errorContent}");
                            }
                        }
                        catch (Exception ex)
                        {
                            // Если бэкенд недоступен, логируем ошибку, но не даем боту упасть
                            Console.WriteLine($"[Ошибка связи с бэкендом]: Не удалось достучаться до API. Ошибка: {ex.Message}");
                        }
                        // ------------------------------------

                        var responseText = BuildStartMessage(user);
                        await SendMessageAsync(telegramClient, message.Chat.Id, responseText, canUseWebAppButton ? webAppUrl : null);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                await Task.Delay(TimeSpan.FromSeconds(2));
            }
        }
    }

    private static string BuildStartMessage(TelegramUser user)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Добро пожаловать в Ayanami Connect.");
        sb.AppendLine($"Пользователь: {user.FirstName ?? user.Username ?? user.Id.ToString()}");
        sb.AppendLine("Открой мини-аппку, чтобы управлять подпиской, балансом и подключением.");
        return sb.ToString();
    }

    private static async Task SendMessageAsync(HttpClient httpClient, long chatId, string text, string? webAppUrl)
    {
        var payload = new Dictionary<string, string>
        {
            ["chat_id"] = chatId.ToString(),
            ["text"] = text
        };

        if (!string.IsNullOrWhiteSpace(webAppUrl))
        {
            var replyMarkup = new
            {
                inline_keyboard = new[]
                {
                    new[]
                    {
                        new
                        {
                            text = "Открыть Ayanami Connect",
                            web_app = new { url = webAppUrl }
                        }
                    }
                }
            };

            payload["reply_markup"] = JsonSerializer.Serialize(replyMarkup);
        }

        var content = new FormUrlEncodedContent(payload);

        using var response = await httpClient.PostAsync("sendMessage", content);
        response.EnsureSuccessStatusCode();
    }
}

public sealed record TelegramResponse<T>(
    [property: JsonPropertyName("ok")] bool Ok,
    [property: JsonPropertyName("result")] T? Result);

public sealed record Update(
    [property: JsonPropertyName("update_id")] int UpdateId,
    [property: JsonPropertyName("message")] TelegramMessage? Message);

public sealed record TelegramMessage(
    [property: JsonPropertyName("message_id")] int MessageId,
    [property: JsonPropertyName("from")] TelegramUser? From,
    [property: JsonPropertyName("chat")] TelegramChat Chat,
    [property: JsonPropertyName("text")] string? Text);

public sealed record TelegramChat(
    [property: JsonPropertyName("id")] long Id);

public sealed record TelegramUser(
    [property: JsonPropertyName("id")] long Id,
    [property: JsonPropertyName("is_bot")] bool IsBot,
    [property: JsonPropertyName("first_name")] string? FirstName,
    [property: JsonPropertyName("last_name")] string? LastName,
    [property: JsonPropertyName("username")] string? Username,
    [property: JsonPropertyName("language_code")] string? LanguageCode);