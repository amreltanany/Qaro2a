namespace ECommerce.API.Options;

public class SeoOptions
{
    public const string SectionName = "Seo";

    public string SiteName { get; set; } = "Qaro2a";

    /// <summary>Arabic brand name shown in titles and structured data (e.g. قروءه).</summary>
    public string SiteNameAr { get; set; } = "قروءه";

    /// <summary>Alternate spellings for structured data and discoverability.</summary>
    public string[] AlternateNames { get; set; } = ["Qaro2a", "قروءه", "قروءة"];

    public string DefaultDescription { get; set; } =
        "Discover books and publishing at Qaro2a — curated titles, podcasts, and literary dialogue with a smooth reading experience.";

    public string DefaultDescriptionAr { get; set; } =
        "قروءه منصة أدبية للبودكاست ومراجعات الكتب والنشر. تسوق كتباً مختارة واستمع إلى حلقات بودكاست قروءه.";

    /// <summary>Optional. Google largely ignores keywords; leave empty to omit the tag.</summary>
    public string? DefaultKeywords { get; set; }

    public string? DefaultKeywordsAr { get; set; }

    /// <summary>Production base URL without trailing slash, e.g. https://qaro2a.com. Used for canonical and Open Graph absolute URLs.</summary>
    public string? BaseUrl { get; set; }

    /// <summary>App-relative path starting with / or ~/ for the default share image (e.g. /logo/brand.png).</summary>
    public string DefaultOgImage { get; set; } =
        "/logo/429827302_747764070661978_6323408778619379006_n-removebg-preview.png";

    /// <summary>Optional @handle for twitter:site (no @ prefix in config).</summary>
    public string? TwitterHandle { get; set; }

    public string OgLocale { get; set; } = "en_US";

    public string OgLocaleAr { get; set; } = "ar_EG";

    /// <summary>Main YouTube channel URL for sameAs and podcast structured data.</summary>
    public string? YouTubeUrl { get; set; }

    /// <summary>Optional Google Search Console HTML verification token (content value only).</summary>
    public string? GoogleSiteVerification { get; set; }

    /// <summary>Optional sameAs URLs for Organization JSON-LD (e.g. social profiles).</summary>
    public string[]? SameAs { get; set; }
}
