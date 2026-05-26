using CSharpFunctionalExtensions;

namespace AyanamiConnect.Domain;

public class PanelClient
{
    public Guid Id { get; private set; }
    public string Email { get; private set; } = string.Empty;
    public string Uuid { get; private set; } = string.Empty;
    public string SubId { get; private set; } = string.Empty;
    public long ExpiryTime { get; private set; }
    public long TotalGB { get; private set; }
    public int LimitIp { get; private set; }
    public string Flow { get; private set; } = string.Empty;
    public bool Enable { get; private set; }
    public int Reset { get; private set; }

    private PanelClient(
        Guid id,
        string email,
        string uuid,
        string subId,
        long expiryTime,
        long totalGB,
        int limitIp,
        string flow,
        bool enable,
        int reset)
    {
        Id = id;
        Email = email;
        Uuid = uuid;
        SubId = subId;
        ExpiryTime = expiryTime;
        TotalGB = totalGB;
        LimitIp = limitIp;
        Flow = flow;
        Enable = enable;
        Reset = reset;
    }

    public static Result<PanelClient> Create(
        Guid id,
        string email,
        string uuid,
        string subId,
        long expiryTime,
        long totalGB,
        int limitIp,
        string flow,
        bool enable,
        int reset)
    {
        if (id == Guid.Empty)
            return Result.Failure<PanelClient>("Panel client id cannot be empty.");

        if (string.IsNullOrWhiteSpace(email))
            return Result.Failure<PanelClient>("Panel client email cannot be empty.");

        if (string.IsNullOrWhiteSpace(uuid))
            return Result.Failure<PanelClient>("Panel client uuid cannot be empty.");

        if (string.IsNullOrWhiteSpace(subId))
            return Result.Failure<PanelClient>("Panel client subId cannot be empty.");

        return Result.Success(new PanelClient(
            id,
            email,
            uuid,
            subId,
            expiryTime,
            totalGB,
            limitIp,
            flow,
            enable,
            reset));
    }

    public static PanelClient Load(
        Guid id,
        string email,
        string uuid,
        string subId,
        long expiryTime,
        long totalGB,
        int limitIp,
        string flow,
        bool enable,
        int reset)
        => new(id, email, uuid, subId, expiryTime, totalGB, limitIp, flow, enable, reset);

    public void RestoreFrom(PanelClient snapshot)
    {
        Id = snapshot.Id;
        Email = snapshot.Email;
        Uuid = snapshot.Uuid;
        SubId = snapshot.SubId;
        ExpiryTime = snapshot.ExpiryTime;
        TotalGB = snapshot.TotalGB;
        LimitIp = snapshot.LimitIp;
        Flow = snapshot.Flow;
        Enable = snapshot.Enable;
        Reset = snapshot.Reset;
    }

    public void SetExpiryTime(long expiryTime)
        => ExpiryTime = expiryTime;

    public void SetEnable(bool enable)
        => Enable = enable;
}
