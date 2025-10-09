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
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // Create Admin role if missing
            if (!await roleManager.RoleExistsAsync(Roles.Admin))
            {
                logger.LogInformation("Creating role {Role}", Roles.Admin);
                var roleResult = await roleManager.CreateAsync(new IdentityRole(Roles.Admin));
                if (!roleResult.Succeeded)
                {
                    var errs = string.Join(", ", roleResult.Errors.Select(e => e.Description));
                    logger.LogError("Failed to create role {Role}. Errors: {Errors}", Roles.Admin, errs);
                    return;
                }
            }

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

                var addUserToRoleResult = await userManager.AddToRoleAsync(user, Roles.Admin);
                if (!addUserToRoleResult.Succeeded)
                {
                    var errs = string.Join(", ", addUserToRoleResult.Errors.Select(e => e.Description));
                    logger.LogError("Failed to add user to role {Role}. Errors: {Errors}", Roles.Admin, errs);
                    return;
                }

                logger.LogInformation("Admin user created with role {Role}", Roles.Admin);
            }
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Seeding failed");
        }
    }
}