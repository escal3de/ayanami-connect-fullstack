using AyanamiConnect.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AyanamiConnect.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<UserEntity>
{
    public void Configure(EntityTypeBuilder<UserEntity> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.TelegramId)
            .IsRequired();

        builder.HasIndex(x => x.TelegramId)
            .IsUnique();
        
        builder.Property(x => x.UserName)
            .HasMaxLength(32);
        
        builder.Property(x => x.FirstName)
            .HasMaxLength(128)
            .IsRequired();
        
        builder.Property(x => x.LastName)
            .HasMaxLength(128);
        
        builder.Property(x => x.LanguageCode)
            .HasMaxLength(2)
            .IsRequired();

        builder.Property(x => x.Balance)
            .IsRequired();
        
        builder.Property(x => x.Role)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();
        
        builder.Property(x => x.CreatedAt)
            .IsRequired();
        
        builder.Property(x => x.LastActiveAt)
            .IsRequired();
        
        builder.HasMany(x => x.Subscriptions)
            .WithOne(x => x.User)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
