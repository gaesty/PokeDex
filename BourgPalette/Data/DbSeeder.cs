using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using BourgPalette.Models;
using BourgPalette.Constants;

namespace BourgPalette.Data;

public static class DbSeeder
{
    public static async Task SeedData(WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var loggerFactory = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger(nameof(DbSeeder));

        try
        {
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            // Seed default admin user if no users exist
            if (!userManager.Users.Any())
            {
                var user = new ApplicationUser
                {
                    Name = "Admin",
                    UserName = "admin@gmail.com",
                    Email = "admin@gmail.com",
                    EmailConfirmed = true,
                    SecurityStamp = Guid.NewGuid().ToString()
                };

                var createUserResult = await userManager.CreateAsync(user, "Admin@123");
                if (!createUserResult.Succeeded)
                {
                    var errs = string.Join(", ", createUserResult.Errors.Select(e => e.Description));
                    logger.LogError("Failed to create admin user. Errors: {Errors}", errs);
                    return;
                }

                logger.LogInformation("Admin user created (roles disabled)");
            }
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Seeding failed");
        }
    }
}