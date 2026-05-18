namespace ECommerce.API.Options;

public class SeoOptions
{
    public const string SectionName = "Seo";

    public string SiteName { get; set; } = "Qaro2a";

    public string DefaultDescription { get; set; } =
        "Discover fashion and style at Qaro2a — curated clothing and accessories with a smooth shopping experience.";

    /// <summary>Optional. Google largely ignores keywords; leave empty to omit the tag.</summary>
    public string? DefaultKeywords { get; set; }

    /// <summary>Production base URL without trailing slash, e.g. https://www.yourstore.com. Used for canonical and Open Graph absolute URLs.</summary>
    public string? BaseUrl { get; set; }

    /// <summary>App-relative path starting with / or ~/ for the default share image (e.g. /logo/brand.png).</summary>
    public string DefaultOgImage { get; set; } =
        "/logo/429827302_747764070661978_6323408778619379006_n-removebg-preview.png";

    /// <summary>Optional @handle for twitter:site (no @ prefix in config).</summary>
    public string? TwitterHandle { get; set; }

    public string OgLocale { get; set; } = "en_US";

    /// <summary>Optional sameAs URLs for Organization JSON-LD (e.g. social profiles).</summary>
    public string[]? SameAs { get; set; }
}
