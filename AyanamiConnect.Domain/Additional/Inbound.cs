using CSharpFunctionalExtensions;

namespace AyanamiConnect.Domain.Additional;

public class Inbound
{
    public Guid Id { get; private set; }
    public int PanelInboundId { get; private set; }
    public string Remark { get; private set; } // - условно человеческое имя (#1 Германия)
    public string ServerAddress { get; private set; }
    public int Port { get; private set; }
    public string Protocol { get; private set; }
    public bool IsActive { get; private set; }
    public int MaxClientsLimit { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public List<Subscription> Subscriptions { get; private set; } = new();

    private Inbound(Guid id, int panelInboundId, string remark, string serverAddress, int port, string protocol,
        bool isActive, int maxClientsLimit, DateTime createdAt)
    {
        Id = id;
        PanelInboundId = panelInboundId;
        Remark = remark;
        ServerAddress = serverAddress;
        Port = port;
        Protocol = protocol;
        IsActive = isActive;
        MaxClientsLimit = maxClientsLimit;
        CreatedAt = createdAt;
    }

    public static Result<Inbound> Create(int panelInboundId, string remark, string serverAddress, int port,
        string protocol, bool isActive, int maxClientsLimit)
    {
        var validationResult =
            Validate(panelInboundId, remark, serverAddress, port, protocol, isActive, maxClientsLimit);

        if (validationResult.IsFailure)
            return Result.Failure<Inbound>(validationResult.Error);

        var inbound = new Inbound(Guid.NewGuid(), panelInboundId, remark, serverAddress, port, protocol, isActive,
            maxClientsLimit, DateTime.UtcNow);

        return Result.Success(inbound);
    }

    public static Inbound Load(Guid id, int panelInboundId, string remark, string serverAddress, int port,
        string protocol, bool isActive, int maxClientsLimit, DateTime createdAt)
        => new Inbound(id, panelInboundId, remark, serverAddress, port, protocol, isActive, maxClientsLimit, createdAt);

    private static Result Validate(int panelInboundId, string remark, string serverAddress, int port,
        string protocol, bool isActive, int maxClientsLimit)
    {
        if (panelInboundId < 0)
            return Result.Failure("Panel inbound id cannot be negative");

        if (string.IsNullOrWhiteSpace(remark))
            return Result.Failure("Remark cannot be empty");

        if (string.IsNullOrWhiteSpace(serverAddress))
            return Result.Failure("Server address cannot be empty");

        if (port <= 0 || port >= 65535)
            return Result.Failure("Invalid port value");

        if (string.IsNullOrWhiteSpace(protocol))
            return Result.Failure("Protocol cannot be empty");

        if (maxClientsLimit <= 0)
            return Result.Failure("Max clients limit must be greater than 0");

        return Result.Success();
    }
}