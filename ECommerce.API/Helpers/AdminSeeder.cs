using ECommerce.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace ECommerce.API.Helpers;

/// <summary>
/// Makes sure the admin account exists after the migrations ran. The account is created only when it is missing,
/// with the password taken from configuration (environment variable Admin__Password), never from the repository.
/// An existing admin is never touched.
/// </summary>
public static class AdminSeeder
{
    public static async Task EnsureAsync(IConfiguration configuration, IServiceProvider services, ILogger logger)
    {
        using var scope = services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        if (await userManager.FindByEmailAsync(UserClaimsHelper.AdminEmail) != null)
            return;

        var password = configuration["Admin:Password"];
        if (string.IsNullOrWhiteSpace(password))
        {
            logger.LogWarning("Admin account {Email} does not exist and Admin:Password is not set: admin was not seeded.", UserClaimsHelper.AdminEmail);
            return;
        }

        var admin = new User
        {
            UserName = UserClaimsHelper.AdminEmail,
            Email = UserClaimsHelper.AdminEmail,
            EmailConfirmed = true,
            FullName = configuration["Admin:FullName"] ?? "Admin"
        };
        var result = await userManager.CreateAsync(admin, password);
        if (result.Succeeded)
            logger.LogInformation("Admin account {Email} seeded.", UserClaimsHelper.AdminEmail);
        else
            logger.LogError("Admin seeding failed: {Errors}", string.Join("; ", result.Errors.Select(e => e.Description)));
    }
}
