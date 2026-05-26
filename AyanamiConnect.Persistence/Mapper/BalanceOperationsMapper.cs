using AyanamiConnect.Domain;
using AyanamiConnect.Persistence.Entities;

namespace AyanamiConnect.Persistence.Mapper;

public static class BalanceOperationsMapper
{
    public static BalanceOperationEntity ToEntity(this BalanceOperation operation, Guid userId)
        => new()
        {
            Id = operation.Id,
            UserId = userId,
            Kind = operation.Kind,
            Title = operation.Title,
            Amount = operation.Amount,
            Note = operation.Note,
            CreatedAt = operation.CreatedAt
        };

    public static BalanceOperation ToDomain(this BalanceOperationEntity entity)
        => BalanceOperation.Load(
            entity.Id,
            entity.Kind,
            entity.Title,
            entity.Amount,
            entity.Note,
            entity.CreatedAt);
}
