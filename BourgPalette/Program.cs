using BourgPalette.Data;
using Microsoft.EntityFrameworkCore;
using DomainPokemon = BourgPalette.Models.Pokemon;

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

    app.MapGet("/pokedex", async (ApplicationDbContext db) => await db.Pokemons.ToListAsync());

        // Legendary filter isn't defined on the domain model; return all for now
        app.MapGet("/pokedex/legendary", async (ApplicationDbContext db) =>
            await db.Pokemons.ToListAsync());

        app.MapGet("/pokedex/{id}", async (int id, ApplicationDbContext db) =>
        {
            var pokemon = await db.Pokemons.FindAsync(id);
            return pokemon is DomainPokemon p
                ? Results.Ok(p)
                : Results.NotFound();
        });

        app.MapPost("/pokedex", async (DomainPokemon pokemon, ApplicationDbContext db) =>
        {
            db.Pokemons.Add(pokemon);
            await db.SaveChangesAsync();
            return Results.Created($"/pokedex/{pokemon.Id}", pokemon);
        });

        app.MapPut("/pokedex/{id}", async (int id, DomainPokemon inputPokemon, ApplicationDbContext db) =>
        {
            var pokemon = await db.Pokemons.FindAsync(id);

            if (pokemon is null) return Results.NotFound();

            // Copy over values from input to tracked entity
            db.Entry(pokemon).CurrentValues.SetValues(inputPokemon);
            await db.SaveChangesAsync();
            return Results.NoContent();
        });

        app.MapDelete("/pokedex/{id}", async (int id, ApplicationDbContext db) =>
        {
            if (await db.Pokemons.FindAsync(id) is DomainPokemon pokemon)
            {
                db.Pokemons.Remove(pokemon);
                await db.SaveChangesAsync();
                return Results.Ok(pokemon);
            }

            return Results.NotFound();
        });

        app.Run();
    }
}