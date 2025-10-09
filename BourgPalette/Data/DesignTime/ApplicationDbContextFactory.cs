using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace BourgPalette.Data.DesignTime;

public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

        // Try env var first to align with docker compose
        var conn = Environment.GetEnvironmentVariable("POSTGRES_CONNECTION_STRING")
                  ?? "Host=localhost;Port=5434;Username=trainerUser;Password=pokedexPassword;Database=pokedex";

        optionsBuilder.UseNpgsql(conn);
        return new ApplicationDbContext(optionsBuilder.Options);
    }
}
