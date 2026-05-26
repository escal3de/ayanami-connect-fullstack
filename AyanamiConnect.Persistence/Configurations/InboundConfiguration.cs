using AyanamiConnect.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AyanamiConnect.Persistence.Configurations;

public class InboundConfiguration : IEntityTypeConfiguration<InboundEntity>
{
    public void Configure(EntityTypeBuilder<InboundEntity> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.PanelInboundId)
            .IsRequired();
        
        builder.Property(x => x.Remark)
            .HasMaxLength(128)
            .IsRequired();
        
        builder.Property(x => x.ServerAddress)
            .HasMaxLength(256)
            .IsRequired();
        
        builder.Property(x => x.Port)
            .HasMaxLength(6)
            .IsRequired();

        builder.Property(x => x.Protocol)
            .HasMaxLength(32)
            .IsRequired();
        
        builder.Property(x => x.IsActive)
            .IsRequired();
        
        builder.Property(x => x.MaxClientsLimit)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();
        
        builder.HasMany(x => x.Subscriptions)
            .WithOne(x => x.Inbound)
            .HasForeignKey(x => x.InboundId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}