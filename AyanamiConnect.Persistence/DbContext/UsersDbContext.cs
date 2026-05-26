using AyanamiConnect.Persistence.Configurations;
using AyanamiConnect.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace AyanamiConnect.Persistence.DbContext;

public class UsersDbContext(DbContextOptions<UsersDbContext> options) : Microsoft.EntityFrameworkCore.DbContext(options)
{
    public DbSet<UserEntity> Users => Set<UserEntity>();
    public DbSet<PanelClientEntity> PanelClients => Set<PanelClientEntity>();
    public DbSet<InboundEntity> Inbounds => Set<InboundEntity>();
    public DbSet<SubscriptionEntity> Subscriptions => Set<SubscriptionEntity>();
    public DbSet<BalanceOperationEntity> BalanceOperations => Set<BalanceOperationEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new PanelClientConfiguration());
        modelBuilder.ApplyConfiguration(new InboundConfiguration());
        modelBuilder.ApplyConfiguration(new SubscriptionConfiguration());
        modelBuilder.ApplyConfiguration(new BalanceOperationConfiguration());

        base.OnModelCreating(modelBuilder);
    }
}
