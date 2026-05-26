using AyanamiConnect.Application.Abstractions.EternalServices;
using AyanamiConnect.Application.Abstractions.Repositories;
using CSharpFunctionalExtensions;

namespace AyanamiConnect.Application.Handlers.Users;

public class DeleteUserHandler(IUsersRepository repository, IThreeXUiClientsService clientses)
{
    private readonly IUsersRepository _repository = repository;
    private readonly IThreeXUiClientsService _clientses = clientses;

    public async Task<Result> HandleAsync(long telegramId, CancellationToken cancellationToken)
    {
        var users = await _repository.GetAllByTelegramIdAsync(telegramId, cancellationToken);

        if (users.Count == 0)
            return Result.Failure($"User with telegram id {telegramId} not found");

        var panelClients = users
            .SelectMany(x => x.PanelClients)
            .GroupBy(x => x.Uuid, StringComparer.OrdinalIgnoreCase)
            .Select(x => x.First())
            .ToList();

        foreach (var panelClient in panelClients)
        {
            var deleteResult = await _clientses.DeleteClientBySubIdAsync(panelClient.SubId, cancellationToken);

            if (deleteResult.IsFailure)
                deleteResult = await _clientses.DeleteClientByUuidAsync(panelClient.Uuid, cancellationToken);

            if (deleteResult.IsFailure)
                return Result.Failure(deleteResult.Error);
        }

        foreach (var user in users)
        {
            await _repository.DeleteAsync(user, cancellationToken);
        }

        return Result.Success();
    }
}
