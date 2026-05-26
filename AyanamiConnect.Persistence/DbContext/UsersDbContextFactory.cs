using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AyanamiConnect.Persistence.DbContext;

public sealed class UsersDbContextFactory : IDesignTimeDbContextFactory<UsersDbContext>
{
    public UsersDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<UsersDbContext>();

        optionsBuilder.UseNpgsql(
            "Host=localhost;Port=5432;Database=ayanami_connect;Username=ayanami;Password=Stasenko_2026!VPNVPNVPN");

        return new UsersDbContext(optionsBuilder.Options);
    }
}
