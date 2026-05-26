using AyanamiConnect.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AyanamiConnect.Persistence.Configurations;

public class BalanceOperationConfiguration : IEntityTypeConfiguration<BalanceOperationEntity>
{
    public void Configure(EntityTypeBuilder<BalanceOperationEntity> builder)
    {
        builder.ToTable("BalanceOperations");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(x => x.Note)
            .IsRequired()
            .HasMaxLength(1024);

        builder.Property(x => x.Kind)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(x => x.Amount)
            .HasColumnType("numeric(18,2)");

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.HasIndex(x => new { x.UserId, x.CreatedAt });

        builder.HasOne(x => x.User)
            .WithMany(x => x.BalanceOperations)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
