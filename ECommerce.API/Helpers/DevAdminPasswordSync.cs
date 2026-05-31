using ECommerce.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace ECommerce.API.Helpers;

public static class DevAdminPasswordSync
{
    public static async Task ApplyIfConfiguredAsync(IConfiguration configuration, IServiceProvider services, ILogger logger)
    {
        var password = configuration["DevAdmin:Password"];
        if (string.IsNullOrWhiteSpace(password))
            return;

        using var scope = services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var adminUser = await userManager.FindByEmailAsync(UserClaimsHelper.AdminEmail);
        if (adminUser is null)
        {
            logger.LogWarning("Dev admin password sync skipped: user {Email} not found.", UserClaimsHelper.AdminEmail);
            return;
        }

        var resetToken = await userManager.GeneratePasswordResetTokenAsync(adminUser);
        var result = await userManager.ResetPasswordAsync(adminUser, resetToken, password);
        if (result.Succeeded)
            logger.LogInformation("Dev admin password synced for {Email}.", adminUser.Email);
        else
            logger.LogWarning("Dev admin password sync failed: {Errors}", string.Join("; ", result.Errors.Select(e => e.Description)));
    }
}
