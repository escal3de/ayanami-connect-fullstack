using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;

namespace AyanamiConnect.API.Common;

public sealed class TelegramMiniAppAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly ILogger<TelegramMiniAppAuthenticationHandler> _logger;

    public TelegramMiniAppAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory loggerFactory,
        UrlEncoder encoder)
        : base(options, loggerFactory, encoder)
    {
        _logger = loggerFactory.CreateLogger<TelegramMiniAppAuthenticationHandler>();
    }

    public const string SchemeName = "Telegram";
    private const string InitDataHeaderName = "X-Telegram-InitData";
    private const string DebugTelegramIdHeaderName = "X-Debug-Telegram-Id";
    private const string AdminRole = "Admin";
    private const string WebAppDataKey = "WebAppData";
    private static readonly TimeSpan MaxInitDataAge = TimeSpan.FromDays(1);

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        foreach (var header in Request.Headers)
        {
            _logger.LogInformation("Header: {Key} = {Value}", header.Key, header.Value);
        }
        
        var environment = Context.RequestServices.GetRequiredService<IHostEnvironment>();
        var configuration = Context.RequestServices.GetRequiredService<IConfiguration>();

        if (Request.Headers.TryGetValue(DebugTelegramIdHeaderName, out var debugTelegramIdValues))
        {
            return Task.FromResult(BuildDebugResult(configuration, debugTelegramIdValues.ToString()));
        }

        if (!Request.Headers.TryGetValue(InitDataHeaderName, out var initDataValues))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var initData = initDataValues.ToString();
        var botToken = configuration["Telegram:BotToken"];
        
        if (string.IsNullOrWhiteSpace(botToken))
            return Task.FromResult(AuthenticateResult.Fail("Telegram bot token is not configured."));

        if (!TryValidateInitData(initData, botToken, out var user, out var failure))
        {
            _logger.LogError("Authentication failed: {FailureReason}. Data: {InitData}", failure, initData);
            return Task.FromResult(AuthenticateResult.Fail(failure));
        }

        var principal = BuildPrincipal(user, configuration);
        return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(principal, SchemeName)));
    }

    private AuthenticateResult BuildDebugResult(IConfiguration configuration, string value)
    {
        if (!long.TryParse(value, out var telegramId)) return AuthenticateResult.NoResult();
    
        var user = new TelegramInitDataUser { Id = telegramId };
        var principal = BuildPrincipal(user, configuration); // Теперь мы используем BuildPrincipal
        return AuthenticateResult.Success(new AuthenticationTicket(principal, SchemeName));
    }

    private ClaimsPrincipal BuildPrincipal(TelegramInitDataUser user, IConfiguration configuration)
    {
        var identity = new ClaimsIdentity(SchemeName);
        identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()));
        
        var adminId = configuration.GetValue<long?>("Telegram:AdminTelegramId");
        if (adminId.HasValue && adminId.Value == user.Id)
            identity.AddClaim(new Claim(ClaimTypes.Role, AdminRole));

        return new ClaimsPrincipal(identity);
    }

    private static bool TryValidateInitData(string initData, string botToken, out TelegramInitDataUser user, out string failure)
    {
        user = null!; failure = string.Empty;
        var query = QueryHelpers.ParseQuery(initData);
        if (!query.TryGetValue("hash", out var hash)) { failure = "Hash missing"; return false; }

        var dataCheckString = string.Join('\n', query.Where(x => x.Key != "hash").OrderBy(x => x.Key).Select(x => $"{x.Key}={x.Value}"));
        var secretKey = HMACSHA256.HashData(Encoding.UTF8.GetBytes("WebAppData"), Encoding.UTF8.GetBytes(botToken));
        var calcHash = Convert.ToHexString(HMACSHA256.HashData(secretKey, Encoding.UTF8.GetBytes(dataCheckString))).ToLower();

        if (!CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(calcHash), Encoding.UTF8.GetBytes(hash.ToString().ToLower())))
        {
            failure = "Signature invalid"; return false;
        }

        var userJson = query["user"];
        user = JsonSerializer.Deserialize<TelegramInitDataUser>(userJson!);
        return user != null;
    }

    public class TelegramInitDataUser
    {
        [JsonPropertyName("id")] public long Id { get; set; }
        [JsonPropertyName("username")] public string? Username { get; set; }
        [JsonPropertyName("first_name")] public string? FirstName { get; set; }
    }
}