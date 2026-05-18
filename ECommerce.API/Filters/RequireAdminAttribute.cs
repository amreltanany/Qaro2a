using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ECommerce.API.Filters;

/// <summary>
/// When applied to a controller or action, authenticates with Identity.Application and requires the admin email.
/// If the user is not authenticated or not the admin, returns 403 Forbidden (no redirect to login).
/// </summary>
public class RequireAdminAttribute : Attribute, IAsyncAuthorizationFilter
{
    private const string AdminEmail = "amr_eltanany@outlook.com";

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var result = await context.HttpContext.AuthenticateAsync("Identity.Application");
        var email = result.Principal?.FindFirstValue(ClaimTypes.Email);

        var isAdmin = result.Succeeded
            && !string.IsNullOrWhiteSpace(email)
            && email.Trim().Equals(AdminEmail, StringComparison.OrdinalIgnoreCase);

        if (!isAdmin)
            context.Result = new ForbidResult();
    }
}
