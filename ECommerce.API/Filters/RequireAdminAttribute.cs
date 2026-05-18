using ECommerce.API.Helpers;
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
    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var result = await context.HttpContext.AuthenticateAsync("Identity.Application");
        var email = UserClaimsHelper.GetEmail(result.Principal);

        var isAdmin = result.Succeeded && UserClaimsHelper.IsAdminEmail(email);

        if (!isAdmin)
            context.Result = new ForbidResult();
    }
}
