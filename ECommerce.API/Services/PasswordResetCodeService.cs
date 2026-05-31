using ECommerce.Application.Services.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace ECommerce.API.Services;

public sealed class PasswordResetCodeService : IPasswordResetCodeService
{
    private static readonly TimeSpan CodeLifetime = TimeSpan.FromMinutes(15);
    private readonly IMemoryCache _cache;

    public PasswordResetCodeService(IMemoryCache cache) => _cache = cache;

    public string GenerateAndStore(string email, string userId)
    {
        var normalizedEmail = NormalizeEmail(email);
        var code = Random.Shared.Next(100000, 999999).ToString();
        _cache.Set(GetCacheKey(normalizedEmail), new ResetEntry(userId, code), CodeLifetime);
        return code;
    }

    public bool TryValidate(string email, string code, out string? userId)
    {
        userId = null;
        var normalizedEmail = NormalizeEmail(email);
        if (!_cache.TryGetValue(GetCacheKey(normalizedEmail), out ResetEntry? entry) || entry is null)
            return false;

        if (!string.Equals(entry.Code, code.Trim(), StringComparison.Ordinal))
            return false;

        userId = entry.UserId;
        return true;
    }

    public void Remove(string email) =>
        _cache.Remove(GetCacheKey(NormalizeEmail(email)));

    private static string GetCacheKey(string normalizedEmail) => $"pwd-reset:{normalizedEmail}";

    private static string NormalizeEmail(string email) => email.Trim().ToUpperInvariant();

    private sealed record ResetEntry(string UserId, string Code);
}
