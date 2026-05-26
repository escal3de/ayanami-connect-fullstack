using AyanamiConnect.Domain.Enums;
using CSharpFunctionalExtensions;


namespace AyanamiConnect.Domain;

public class User
{
    public Guid Id { get; private set; }
    public long TelegramId { get; private set; }
    public string? UserName { get; private set; }
    public string FirstName { get; private set; }
    public string? LastName { get; private set; }
    public string LanguageCode { get; private set; }
    public decimal Balance { get; private set; }
    public UserRole Role { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime LastActiveAt { get; private set; }
    public List<BalanceOperation> BalanceOperations { get; private set; } = new();
    public List<PanelClient> PanelClients { get; private set; } = new();
    public List<Subscription> Subscriptions { get; private set; } = new();

    private User(Guid id, long telegramId, string? userName, string firstName, string? lastName, string languageCode,
        decimal balance, UserRole role, DateTime createdAt, DateTime lastActiveAt, List<BalanceOperation>? balanceOperations = null)
    {
        Id = id;
        TelegramId = telegramId;
        UserName = userName;
        FirstName = firstName;
        LastName = lastName;
        LanguageCode = languageCode;
        Balance = balance;
        Role = role;
        CreatedAt = createdAt;
        LastActiveAt = lastActiveAt;
        BalanceOperations = balanceOperations ?? new();
    }

    public static Result<User> Create(long telegramId, string? userName, string firstName, string? lastName,
        UserRole role = UserRole.User)
    {
        var validationResult = Validate(telegramId, userName, firstName, lastName, "ru", role);

        if (validationResult.IsFailure)
            return Result.Failure<User>(validationResult.Error);

        var user = new User(Guid.NewGuid(), telegramId, userName, firstName, lastName,
            "ru", 0, role, DateTime.UtcNow, DateTime.UtcNow);

        return Result.Success(user);
    }

    public static User Load(Guid id, long telegramId, string? userName, string firstName, string? lastName,
        string languageCode, decimal balance, UserRole role, DateTime createdAt, DateTime lastActiveAt)
        => new User(id, telegramId, userName, firstName, lastName, languageCode, balance, role, createdAt,
            lastActiveAt);

    public void AddBalanceOperation(BalanceOperation operation)
    {
        BalanceOperations.Add(operation);
    }

    public decimal AddToBalance(decimal amount) => Balance += amount;

    public void RestoreBalance(decimal balance)
    {
        Balance = balance;
    }

    public bool TryWithdrawFromBalance(decimal amount, out string error)
    {
        error = string.Empty;

        if (amount < 0)
        {
            error = "Amount cannot be negative.";
            return false;
        }

        if (Balance < amount)
        {
            error = "Insufficient balance.";
            return false;
        }

        Balance -= amount;
        return true;
    }

    private static Result Validate(long telegramId, string? userName, string firstName, string? lastName,
        string languageCode, UserRole role)
    {
        if (telegramId <= 0)
            return Result.Failure("Telegram id must not be zero");

        if (string.IsNullOrWhiteSpace(firstName))
            return Result.Failure("First name cannot be empty");

        return Result.Success();
    }

    public void UpdateLastActive()
    {
        LastActiveAt = DateTime.UtcNow;
    }

    public void ChangeRole(UserRole role)
    {
        Role = role;
    }
}
