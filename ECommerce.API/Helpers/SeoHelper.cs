using ECommerce.API.Options;
using Microsoft.AspNetCore.Http;

namespace ECommerce.API.Helpers;

public static class SeoHelper
{
    public static string GetBaseUrl(HttpRequest request, SeoOptions seo)
    {
        if (!string.IsNullOrWhiteSpace(seo.BaseUrl))
            return seo.BaseUrl.TrimEnd('/');

        return $"{request.Scheme}://{request.Host}";
    }

    public static string BuildLocalizedUrl(
        HttpRequest request,
        SeoOptions seo,
        string culture,
        string? path = null,
        IQueryCollection? query = null)
    {
        var baseUrl = GetBaseUrl(request, seo);
        path ??= $"{request.PathBase}{request.Path}";

        var parts = new List<string> { $"culture={Uri.EscapeDataString(culture)}" };

        var source = query ?? request.Query;
        foreach (var pair in source)
        {
            if (string.Equals(pair.Key, "culture", StringComparison.OrdinalIgnoreCase))
                continue;

            foreach (var value in pair.Value)
                parts.Add($"{Uri.EscapeDataString(pair.Key)}={Uri.EscapeDataString(value ?? string.Empty)}");
        }

        return $"{baseUrl}{path}?{string.Join("&", parts)}";
    }

    public static string GetBrandName(SeoOptions seo, bool isArabic) =>
        isArabic
            ? (string.IsNullOrWhiteSpace(seo.SiteNameAr) ? seo.SiteName : seo.SiteNameAr)
            : seo.SiteName;

    public static string GetDefaultDescription(SeoOptions seo, bool isArabic) =>
        isArabic
            ? (string.IsNullOrWhiteSpace(seo.DefaultDescriptionAr) ? seo.DefaultDescription : seo.DefaultDescriptionAr)
            : seo.DefaultDescription;

    public static string? GetDefaultKeywords(SeoOptions seo, bool isArabic)
    {
        var keywords = isArabic ? seo.DefaultKeywordsAr : seo.DefaultKeywords;
        return string.IsNullOrWhiteSpace(keywords) ? null : keywords;
    }

    public static readonly (string Path, string ChangeFreq, double Priority)[] PublicPages =
    [
        ("/", "weekly", 1.0),
        ("/Home/Shop", "daily", 0.9),
        ("/Home/Podcast", "weekly", 0.9),
        ("/Home/About", "monthly", 0.8),
        ("/Home/Blog", "weekly", 0.8),
        ("/Publish", "monthly", 0.7),
        ("/Contact", "monthly", 0.6)
    ];
}
