using AyanamiConnect.Domain.Enums;
using CSharpFunctionalExtensions;

namespace AyanamiConnect.Domain;

public class Subscription
{
    public string Email { get; private set; } = string.Empty;
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public DateTime StartedAt { get; private set; }
    public DateTime EndedAt { get; private set; }
    public decimal Price { get; private set; }
    public SubscriptionStatus Status { get; private set; }
    public SubscriptionPlans Plans { get; private set; }

    private Subscription(
        string email,
        Guid id,
        string name,
        DateTime startedAt,
        DateTime endedAt,
        decimal price,
        SubscriptionStatus status,
        SubscriptionPlans plans)
    {
        Email = email;
        Id = id;
        Name = name;
        StartedAt = startedAt;
        EndedAt = endedAt;
        Price = price;
        Status = status;
        Plans = plans;
    }

    public static Result<Subscription> Create(
        Guid id,
        string email,
        string name,
        DateTime startedAt,
        DateTime endedAt,
        decimal price = 0,
        SubscriptionStatus status = SubscriptionStatus.Active,
        SubscriptionPlans plans = SubscriptionPlans.Trial)
    {
        if (string.IsNullOrWhiteSpace(email))
            return Result.Failure<Subscription>("Subscription email cannot be empty.");

        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<Subscription>("Subscription name cannot be empty.");

        if (endedAt <= startedAt)
            return Result.Failure<Subscription>("Subscription end date must be greater than start date.");

        return Result.Success(new Subscription(email, id, name, startedAt, endedAt, price, status, plans));
    }

    public static Subscription Load(
        string email,
        Guid id,
        string name,
        DateTime startedAt,
        DateTime endedAt,
        decimal price,
        SubscriptionStatus status,
        SubscriptionPlans plans)
        => new(email, id, name, startedAt, endedAt, price, status, plans);

    public void RestoreFrom(Subscription snapshot)
    {
        Email = snapshot.Email;
        Id = snapshot.Id;
        Name = snapshot.Name;
        StartedAt = snapshot.StartedAt;
        EndedAt = snapshot.EndedAt;
        Price = snapshot.Price;
        Status = snapshot.Status;
        Plans = snapshot.Plans;
    }

    public bool IsActive => Status == SubscriptionStatus.Active && EndedAt > DateTime.UtcNow;

    public bool IsExpired => EndedAt <= DateTime.UtcNow;

    public void ExtendBy(TimeSpan duration, DateTime? baseDate = null)
    {
        var effectiveBaseDate = baseDate ?? EndedAt;

        if (effectiveBaseDate < DateTime.UtcNow)
            effectiveBaseDate = DateTime.UtcNow;

        EndedAt = effectiveBaseDate.Add(duration);
        Status = SubscriptionStatus.Active;
    }

    public void ChangePlan(SubscriptionPlans plans, decimal price)
    {
        Name = plans.ToString();
        Plans = plans;
        Price = price;
        Status = SubscriptionStatus.Active;
    }

    public void MarkExpired()
        => Status = SubscriptionStatus.Expired;
}
