using AyanamiConnect.Application.Abstractions.EternalServices;
using AyanamiConnect.Application.Contracts.ThreeXUi;
using CSharpFunctionalExtensions;

namespace AyanamiConnect.Infrastructure.ThreeXUi.Services;

public class ThreeXUiClientsService(
    IThreeXUiClient client,
    IThreeXUiInboundsService inbounds)
    : IThreeXUiClientsService
{
    private readonly IThreeXUiClient _client = client;
    private readonly IThreeXUiInboundsService _inbounds = inbounds;

    public async Task<Result<PanelClientResponse>> GetByIdAsync(int clientId, CancellationToken cancellationToken)
        => await FindClientAsync(x => x.Id == clientId, cancellationToken);

    public async Task<Result<PanelClientResponse>> GetByUuidAsync(string clientUuid, CancellationToken cancellationToken)
        => await FindClientAsync(x => string.Equals(x.Uuid, clientUuid, StringComparison.OrdinalIgnoreCase), cancellationToken);

    public async Task<Result<PanelClientResponse>> GetBySubIdAsync(string subId, CancellationToken cancellationToken)
        => await FindClientAsync(x => string.Equals(x.SubId, subId, StringComparison.OrdinalIgnoreCase), cancellationToken);

    public async Task<Result<IEnumerable<PanelClientResponse>>> GetAllAsync(CancellationToken cancellationToken)
    {
        var inboundsResult = await _inbounds.GetInboundsAsync(cancellationToken);
        if (inboundsResult.IsFailure)
            return Result.Failure<IEnumerable<PanelClientResponse>>(inboundsResult.Error);

        var clients = inboundsResult.Value
            .SelectMany(x => x.ClientStats ?? Enumerable.Empty<PanelClientResponse>())
            .ToList();

        return Result.Success<IEnumerable<PanelClientResponse>>(clients);
    }

    public async Task<Result> CreateClientAsync(CreatePanelClientRequest request, CancellationToken cancellationToken)
    {
        var inboundsResult = await _inbounds.GetInboundsAsync(cancellationToken);
        
        if (inboundsResult.IsFailure)
            return Result.Failure(inboundsResult.Error);

        foreach (var inbound in inboundsResult.Value)
        {
            var requestForInbound = request with
            {
                Email = $"{request.Email}_{inbound.Id}"
            };

            var response = await _client.PostFormAsync(
                "inbounds/addClient",
                BuildFormData(inbound.Id, requestForInbound),
                cancellationToken);

            if (response.IsFailure)
                return Result.Failure(response.Error);

            var verifyResult = await _inbounds.GetInboundsAsync(cancellationToken);
            
            if (verifyResult.IsFailure)
                return Result.Failure(verifyResult.Error);

            var verifiedInbound = verifyResult.Value.FirstOrDefault(x => x.Id == inbound.Id);
            
            if (verifiedInbound is null)
                return Result.Failure($"Panel did not return inbound {inbound.Id} after creating client '{requestForInbound.Email}'.");

            var created = verifiedInbound.ClientStats?.Any(x =>
                string.Equals(x.Uuid, request.Id, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(x.SubId, request.SubId, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(x.Email, requestForInbound.Email, StringComparison.OrdinalIgnoreCase)) == true;

            if (!created)
                return Result.Failure($"Panel rejected client '{requestForInbound.Email}' for inbound {inbound.Id}.");
        }

        return Result.Success();
    }

    public async Task<Result> DeleteClientByIdAsync(int clientId, CancellationToken cancellationToken)
        => await DeleteClientsAsync(x => x.Id == clientId, cancellationToken);

    public async Task<Result> DeleteClientByUuidAsync(string clientUuid, CancellationToken cancellationToken)
        => await DeleteClientsAsync(x => string.Equals(x.Uuid, clientUuid, StringComparison.OrdinalIgnoreCase), cancellationToken);

    public async Task<Result> DeleteClientBySubIdAsync(string subId, CancellationToken cancellationToken)
        => await DeleteClientsAsync(x => string.Equals(x.SubId, subId, StringComparison.OrdinalIgnoreCase), cancellationToken);

    public async Task<Result> DeleteClientByEmailAsync(string email, CancellationToken cancellationToken)
        => await DeleteClientsAsync(x => string.Equals(x.Email, email, StringComparison.OrdinalIgnoreCase), cancellationToken);

    private async Task<Result<PanelClientResponse>> FindClientAsync(
        Func<PanelClientResponse, bool> predicate,
        CancellationToken cancellationToken)
    {
        var clients = await GetAllAsync(cancellationToken);
        
        if (clients.IsFailure)
            return Result.Failure<PanelClientResponse>(clients.Error);

        var client = clients.Value.FirstOrDefault(predicate);
        
        if (client is null)
            return Result.Failure<PanelClientResponse>("Client not found.");

        return Result.Success(client);
    }

    private async Task<Result> DeleteClientsAsync(
        Func<PanelClientResponse, bool> predicate,
        CancellationToken cancellationToken)
    {
        var inboundsResult = await _inbounds.GetInboundsAsync(cancellationToken);
        
        if (inboundsResult.IsFailure)
            return Result.Failure(inboundsResult.Error);

        var anyDeleted = false;
        var errors = new List<string>();

        foreach (var inbound in inboundsResult.Value)
        {
            var matchingClients = inbound.ClientStats?.Where(predicate).ToList() ?? [];

            if (matchingClients.Count == 0)
                continue;

            foreach (var client in matchingClients)
            {
                var response = await _client.PostFormAsync(
                    $"inbounds/{inbound.Id}/delClient/{client.Uuid}",
                    BuildDeleteFormData(inbound.Id, client),
                    cancellationToken);

                if (response.IsFailure)
                {
                    errors.Add(response.Error);
                    continue;
                }

                anyDeleted = true;
            }
        }

        if (anyDeleted)
            return Result.Success();

        if (errors.Count != 0)
            return Result.Failure(string.Join("; ", errors));

        return Result.Failure("Client not found.");
    }

    private static IReadOnlyDictionary<string, string> BuildFormData(int inboundId, CreatePanelClientRequest request)
        => new Dictionary<string, string>
        {
            ["id"] = inboundId.ToString(),
            ["settings"] = System.Text.Json.JsonSerializer.Serialize(new
            {
                clients = new[]
                {
                    new
                    {
                        id = request.Id,
                        flow = request.Flow,
                        email = request.Email,
                        limitIp = request.LimitIp,
                        totalGB = request.TotalGB,
                        expiryTime = request.ExpiryTime,
                        enable = request.Enable,
                        tgId = request.TgId,
                        subId = request.SubId,
                        comment = request.Comment,
                        reset = request.Reset
                    }
                }
            })
        };

    private static IReadOnlyDictionary<string, string> BuildDeleteFormData(int inboundId, PanelClientResponse client)
        => new Dictionary<string, string>
        {
            ["id"] = inboundId.ToString(),
            ["settings"] = System.Text.Json.JsonSerializer.Serialize(new
            {
                clients = new[]
                {
                    new
                    {
                        id = client.Uuid,
                        email = client.Email,
                        subId = client.SubId
                    }
                }
            })
        };
}
