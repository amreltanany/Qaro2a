using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace ECommerce.API.Helpers;

public static class UserClaimsHelper
{
    public const string AdminEmail = "amr_eltanany@outlook.com";

    public static string? GetEmail(ClaimsPrincipal? user)
    {
        if (user?.Identity?.IsAuthenticated != true)
            return null;

        return user.FindFirstValue(ClaimTypes.Email)
            ?? user.FindFirstValue(JwtRegisteredClaimNames.Email)
            ?? user.FindFirstValue("email");
    }

    public static bool IsAdminEmail(string? email) =>
        !string.IsNullOrWhiteSpace(email)
        && email.Trim().Equals(AdminEmail, StringComparison.OrdinalIgnoreCase);

    public static string? GetUserId(ClaimsPrincipal? user)
    {
        if (user?.Identity?.IsAuthenticated != true)
            return null;

        return user.FindFirstValue(JwtRegisteredClaimNames.NameId)
            ?? user.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? user.FindFirstValue("nameid")
            ?? user.FindFirstValue("sub");
    }

    public static string? GetUserIdFromJwtString(string? token)
    {
        if (string.IsNullOrEmpty(token))
            return null;

        try
        {
            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
            return jwt.Claims.FirstOrDefault(c =>
                    c.Type == JwtRegisteredClaimNames.NameId
                    || c.Type == ClaimTypes.NameIdentifier
                    || c.Type == "nameid"
                    || c.Type == "sub")
                ?.Value;
        }
        catch
        {
            return null;
        }
    }
}
