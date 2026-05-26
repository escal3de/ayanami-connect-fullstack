using AyanamiConnect.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AyanamiConnect.Persistence.Configurations;

public class PanelClientConfiguration : IEntityTypeConfiguration<PanelClientEntity>
{
    public void Configure(EntityTypeBuilder<PanelClientEntity> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Email)
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(x => x.Uuid)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(x => x.SubId)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(x => x.ExpiryTime)
            .IsRequired();

        builder.Property(x => x.TotalGB)
            .IsRequired();

        builder.Property(x => x.LimitIp)
            .IsRequired();

        builder.Property(x => x.Flow)
            .HasMaxLength(64);

        builder.Property(x => x.Enable)
            .IsRequired();

        builder.Property(x => x.Reset)
            .IsRequired();

        builder.HasOne(x => x.User)
            .WithMany(x => x.PanelClients)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
