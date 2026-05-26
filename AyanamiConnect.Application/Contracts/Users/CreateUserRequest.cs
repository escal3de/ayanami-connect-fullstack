namespace AyanamiConnect.Application.Contracts.Users;

public record CreateUserRequest(
    long TelegramId,
    string? UserName,
    string FirstName,
    string? LastName);