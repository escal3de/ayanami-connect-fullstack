using AyanamiConnect.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AyanamiConnect.Persistence.Configurations;

public class SubscriptionConfiguration : IEntityTypeConfiguration<SubscriptionEntity>
{
    public void Configure(EntityTypeBuilder<SubscriptionEntity> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Email)
            .HasMaxLength(128)
            .IsRequired();
        
        builder.Property(x => x.Name)
            .HasMaxLength(128)
            .IsRequired();
        
        builder.Property(x => x.StartedAt)
            .IsRequired();
        
        builder.Property(x => x.EndedAt)
            .IsRequired();

        builder.Property(x => x.Price)
            .IsRequired();
        
        builder.Property(x => x.Status)
            .IsRequired();
        
        builder.Property(x => x.Plans)
            .IsRequired();
        
        builder.HasOne(x => x.User)
            .WithMany(x => x.Subscriptions)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasOne(x => x.Inbound)
            .WithMany(x => x.Subscriptions)
            .HasForeignKey(x => x.InboundId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
