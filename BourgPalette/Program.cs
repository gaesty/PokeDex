using BourgPalette.Data;
using Microsoft.EntityFrameworkCore;
using DomainPokemon = BourgPalette.Models.Pokemon;
using BourgPalette.DTOs;
using BourgPalette.Middleware;
using Microsoft.AspNetCore.Identity;
using BourgPalette.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using BourgPalette.Services;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Prometheus;


class Program
{
    static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Logging.AddSimpleConsole(c => c.SingleLine = true);

        var postgresConn = Environment.GetEnvironmentVariable("POSTGRES_CONNECTION_STRING")
                    ?? builder.Configuration["POSTGRES_CONNECTION_STRING"]
                    ?? builder.Configuration.GetConnectionString("Postgres")
                    ?? "Host=localhost;Port=5434;Username=trainerUser;Password=pokedexPassword;Database=pokedex";

        // DbContext & Identity (single context)
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(postgresConn));
        builder.Services.AddIdentityCore<ApplicationUser>()
            .AddEntityFrameworkStores<ApplicationDbContext>();

        // JWT Authentication
        var jwtSecret = builder.Configuration["JWT:secret"];
        if (string.IsNullOrWhiteSpace(jwtSecret))
        {
            if (builder.Environment.IsDevelopment())
            {
                jwtSecret = Guid.NewGuid().ToString("N");
                // Inject generated dev secret back into configuration so TokenService sees it
                builder.Configuration["JWT:secret"] = jwtSecret;
            }
            else
            {
                throw new InvalidOperationException("JWT:secret is not configured. Use user-secrets or environment variable JWT__secret.");
            }
        }

        builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,
                    ValidAudience = builder.Configuration["JWT:ValidAudience"],
                    ValidIssuer = builder.Configuration["JWT:ValidIssuer"],
                    ClockSkew = TimeSpan.Zero,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
                };
            });

        builder.Services.AddDatabaseDeveloperPageExceptionFilter();
        builder.Services.AddHealthChecks();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddControllers();
        builder.Services.AddAuthorization();
        builder.Services.AddOpenApiDocument(config =>
        {
            config.DocumentName = "PokeDex API";
            config.Title = "PokeDex API v1";
            config.Version = "v1";
            // Proper HTTP Bearer scheme so Postman & Swagger UI handle "Bearer" prefix automatically
            config.AddSecurity("JWT", Array.Empty<string>(), new NSwag.OpenApiSecurityScheme
            {
                Type = NSwag.OpenApiSecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = NSwag.OpenApiSecurityApiKeyLocation.Header,
                Description = "JWT Authorization header using the Bearer scheme. Example: 'Bearer eyJhbGciOiJI...'"
            });
            config.OperationProcessors.Add(new NSwag.Generation.Processors.Security.AspNetCoreOperationSecurityScopeProcessor("JWT"));
        });

        // Middleware
        builder.Services.AddTransient<ErrorHandlingMiddleware>();
    // Services
        builder.Services.AddScoped<BourgPalette.Services.ITokenService, BourgPalette.Services.TokenService>();
        builder.Services.AddHttpClient<WeatherService>();
        // Redis cache: configuration via REDIS_CONNECTION or appsettings (fallback to in-memory if not set)
        var redisConn = Environment.GetEnvironmentVariable("REDIS_CONNECTION") ?? builder.Configuration["Redis:Connection"];
        if (!string.IsNullOrWhiteSpace(redisConn))
        {
            builder.Services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisConn;
                options.InstanceName = builder.Configuration["Redis:InstanceName"] ?? "pokedex:";
            });
        }
        else
        {
            builder.Services.AddDistributedMemoryCache();
        }
        builder.Services.AddScoped<IWeatherService>(sp =>
        {
            var inner = sp.GetRequiredService<WeatherService>();
            var cache = sp.GetRequiredService<IDistributedCache>();
            return new CachedWeatherService(inner, cache, sp.GetRequiredService<ILogger<CachedWeatherService>>());
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

        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

    app.UseMiddleware<ErrorHandlingMiddleware>();
    // Prometheus metrics: expose /metrics and instrument HTTP
    app.UseMetricServer();
    app.UseHttpMetrics();

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

        await DbSeeder.SeedData(app);

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}