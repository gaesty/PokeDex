using BourgPalette.Data;
using Microsoft.EntityFrameworkCore;
using DomainPokemon = BourgPalette.Models.Pokemon;
using BourgPalette.DTOs;

class Program
{
    static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Logging.AddSimpleConsole(c => c.SingleLine = true);

    var postgresConn = Environment.GetEnvironmentVariable("POSTGRES_CONNECTION_STRING")
                ?? builder.Configuration["POSTGRES_CONNECTION_STRING"]
                ?? builder.Configuration.GetConnectionString("Postgres")
                ?? "Host=localhost;Port=5434;Username=trainerUser;Password=pokedexPassword;Database=pokedex";

    // Use only ApplicationDbContext
    builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(postgresConn));

        builder.Services.AddDatabaseDeveloperPageExceptionFilter();
        builder.Services.AddHealthChecks();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddControllers();
        builder.Services.AddOpenApiDocument(config =>
        {
            config.DocumentName = "PokeDex API";
            config.Title = "PokeDex API v1";
            config.Version = "v1";
        });

        var app = builder.Build();

        using (var scope = app.Services.CreateScope())
        {
            var scopedServices = scope.ServiceProvider;
            var logger = scopedServices.GetRequiredService<ILogger<Program>>();

            var ormDb = scopedServices.GetRequiredService<ApplicationDbContext>();
            try
            {
                ormDb.Database.Migrate();
                logger.LogInformation("ApplicationDbContext migrations applied.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to apply migrations for ApplicationDbContext");
            }
        }

        var enableSwagger = app.Environment.IsDevelopment() ||
                             app.Configuration.GetValue<bool>("Swagger:Enabled");

    app.MapGet("/", () => "Welcome to the PokeDex API!");

        app.MapGet("/health", async (ApplicationDbContext db) =>
        {
            try
            {
                await db.Database.ExecuteSqlRawAsync("SELECT 1");
                return Results.Ok("Healthy");
            }
            catch (Exception ex)
            {
                return Results.Problem($"Unhealthy: {ex.Message}", statusCode: 503);
            }
        });

        app.MapGet("/dbinfo", async (ApplicationDbContext db) =>
        {
            var canConnect = await db.Database.CanConnectAsync();
            var entityCount = db.Model.GetEntityTypes().Count();
            var pendingMigrations = await db.Database.GetPendingMigrationsAsync();
            return Results.Ok(new
            {
                Entities = entityCount,
                CanConnect = canConnect,
                PendingMigrations = pendingMigrations.ToArray()
            });
        });

        app.MapGet("/ping", () => Results.Ok("pong"));

        if (enableSwagger)
        {
            app.UseOpenApi();
            app.UseSwaggerUi(config =>
            {
                config.DocumentTitle = "PokeDex API";
                config.Path = "/swagger";
                config.DocumentPath = "/swagger/{documentName}/swagger.json";
                config.DocExpansion = "list";
            });
        }

        app.MapControllers();

        app.Run();
    }
}