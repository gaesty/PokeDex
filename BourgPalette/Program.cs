using Microsoft.EntityFrameworkCore;
class Program
{
    static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Database configuration: use SQLite by default so we can scaffold ORM now.
        // You can switch to PostgreSQL later by adding the Npgsql provider and calling UseNpgsql.
        var sqliteConn = builder.Configuration.GetConnectionString("Sqlite")
                         ?? builder.Configuration["ConnectionStrings:Sqlite"]
                         ?? "Data Source=pokedex.db";

    builder.Services.AddDbContext<PokedexDB>(opt => opt.UseSqlite(sqliteConn));
    builder.Services.AddDbContext<BourgPalette.Data.ApplicationDbContext>(opt => opt.UseSqlite(sqliteConn));
        builder.Services.AddDatabaseDeveloperPageExceptionFilter();

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddOpenApiDocument(config =>
        {
            config.DocumentName = "PokeDex API";
            config.Title = "PokeDex API v1";
            config.Version = "v1";
        });

    // Enable Swagger UI in Development or when explicitly enabled via configuration
    var enableSwagger = builder.Environment.IsDevelopment() ||
                builder.Configuration.GetValue<bool>("Swagger:Enabled");

    var app = builder.Build();

    app.MapGet("/", () => "Welcome to the PokeDex API!");

    app.MapGet("/health", async (PokedexDB db) =>
        {
            try
            {
                // lightweight check: try simple query; for InMemory, this is instant
                await db.Database.ExecuteSqlRawAsync("SELECT 1");
                return Results.Ok("Healthy");
            }
            catch (Exception ex)
            {
                return Results.Problem($"Unhealthy: {ex.Message}", statusCode: 503);
            }
        });

        app.MapGet("/dbinfo", async (BourgPalette.Data.ApplicationDbContext db) =>
        {
            // Quick check: number of tables tracked (by EF model)
            var entityCount = db.Model.GetEntityTypes().Count();
            return Results.Ok(new { Entities = entityCount });
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

        app.MapGet("/pokedex", async (PokedexDB db) =>
            await db.Pokemons.ToListAsync());

        app.MapGet("/pokedex/legendary", async (PokedexDB db) =>
            await db.Pokemons.Where(t => t.IsLegendary).ToListAsync());

        app.MapGet("/pokedex/{id}", async (int id, PokedexDB db) =>
            await db.Pokemons.FindAsync(id)
                is Pokemon pokemon
                    ? Results.Ok(pokemon)
                    : Results.NotFound());

        app.MapPost("/pokedex", async (Pokemon pokemon, PokedexDB db) =>
        {
            db.Pokemons.Add(pokemon);
            await db.SaveChangesAsync();

            return Results.Created($"/pokedex/{pokemon.Id}", pokemon);
        });

        app.MapPut("/pokedex/{id}", async (int id, Pokemon inputPokemon, PokedexDB db) =>
        {
            var pokemon = await db.Pokemons.FindAsync(id);

            if (pokemon is null) return Results.NotFound();

            pokemon.Name = inputPokemon.Name;
            pokemon.IsLegendary = inputPokemon.IsLegendary;

            await db.SaveChangesAsync();

            return Results.NoContent();
        });

        app.MapDelete("/pokedex/{id}", async (int id, PokedexDB db) =>
        {
            if (await db.Pokemons.FindAsync(id) is Pokemon pokemon)
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