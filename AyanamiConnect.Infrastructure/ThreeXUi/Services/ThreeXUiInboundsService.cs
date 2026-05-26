using AyanamiConnect.Application.Abstractions.EternalServices;
using AyanamiConnect.Application.Contracts.ThreeXUi;
using AyanamiConnect.Domain.Additional;
using AyanamiConnect.Infrastructure.ThreeXUi.Contracts;
using AyanamiConnect.Infrastructure.ThreeXUi.Mapping;
using CSharpFunctionalExtensions;

namespace AyanamiConnect.Infrastructure.ThreeXUi.Services;

public class ThreeXUiInboundsService(IThreeXUiClient client) : IThreeXUiInboundsService
{
    private readonly IThreeXUiClient _client = client;

    public async Task<Result<IEnumerable<ThreeXUiPanelInbound>>> GetInboundsAsync(CancellationToken cancellationToken)
    {
        var response =
            await _client.GetAsync<ThreeXUiResponse<List<InboundResponse>>>("inbounds/list", cancellationToken);

        if (response.IsFailure)
            return Result.Failure<IEnumerable<ThreeXUiPanelInbound>>(response.Error);

        if (!response.Value!.Success)
            return Result.Failure<IEnumerable<ThreeXUiPanelInbound>>(response.Value.Msg);

        var inbounds = response.Value.Obj!
            .Select(x => x.ToApplicationModel())
            .ToList();

        return Result.Success<IEnumerable<ThreeXUiPanelInbound>>(inbounds);
    }

    public async Task<Result<ThreeXUiPanelInbound>> GetInboundAsync(int panelInboundId,
        CancellationToken cancellationToken)
    {
        var response =
            await _client.GetAsync<ThreeXUiResponse<InboundResponse>>($"inbounds/get/{panelInboundId}",
                cancellationToken);

        if (response.IsFailure)
            return Result.Failure<ThreeXUiPanelInbound>(response.Error);

        if (!response.Value!.Success)
            return Result.Failure<ThreeXUiPanelInbound>(response.Value.Msg);

        if (response.Value.Obj is null)
            return Result.Failure<ThreeXUiPanelInbound>($"Inbound {panelInboundId} response has empty object.");

        var inbound = response.Value.Obj.ToApplicationModel();
        return Result.Success(inbound);
    }

    public async Task<Result> CreateInboundAsync(Inbound inbound, CancellationToken cancellationToken)
    {
        var request = inbound.ToRequest();
        var response =
            await _client.PostAsync<CreateInboundRequest, InboundResponse>("inbounds/add", request,
                cancellationToken);

        if (response.IsFailure)
            return Result.Failure(response.Error);

        return Result.Success();
    }

    public async Task<Result> DeleteInboundAsync(int panelInboundId, CancellationToken cancellationToken)
    {
        var response = await _client.DeleteAsync($"inbounds/del/{panelInboundId}", cancellationToken);

        if (response.IsFailure)
            return Result.Failure(response.Error);

        return Result.Success();
    }
}
