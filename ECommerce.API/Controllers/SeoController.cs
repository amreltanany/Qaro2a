using System.Text;
using System.Xml.Linq;
using ECommerce.API.Helpers;
using ECommerce.API.Options;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace ECommerce.API.Controllers;

public class SeoController : Controller
{
    private readonly SeoOptions _seo;

    public SeoController(IOptions<SeoOptions> seoOptions)
    {
        _seo = seoOptions.Value;
    }

    [HttpGet("/robots.txt")]
    [ResponseCache(Duration = 86400)]
    public ContentResult Robots()
    {
        var baseUrl = SeoHelper.GetBaseUrl(Request, _seo);
        var body = $"""
            User-agent: *
            Allow: /
            Disallow: /Dashboard/
            Disallow: /Admin/
            Disallow: /Home/Login
            Disallow: /Home/Register
            Disallow: /Home/MyAccount
            Disallow: /Home/Wishlist
            Disallow: /Cart/
            Disallow: /swagger

            Sitemap: {baseUrl}/sitemap.xml
            """;

        return Content(body, "text/plain", Encoding.UTF8);
    }

    [HttpGet("/sitemap.xml")]
    [ResponseCache(Duration = 3600)]
    public ContentResult Sitemap()
    {
        var baseUrl = SeoHelper.GetBaseUrl(Request, _seo);
        XNamespace ns = "http://www.sitemaps.org/schemas/sitemap/0.9";
        XNamespace xhtml = "http://www.w3.org/1999/xhtml";

        var urlset = new XElement(ns + "urlset",
            new XAttribute(XNamespace.Xmlns + "xhtml", xhtml));

        foreach (var page in SeoHelper.PublicPages)
        {
            var arUrl = $"{baseUrl}{page.Path}?culture=ar";
            var enUrl = $"{baseUrl}{page.Path}?culture=en";

            urlset.Add(new XElement(ns + "url",
                new XElement(ns + "loc", arUrl),
                new XElement(ns + "changefreq", page.ChangeFreq),
                new XElement(ns + "priority", page.Priority.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture)),
                new XElement(xhtml + "link",
                    new XAttribute("rel", "alternate"),
                    new XAttribute("hreflang", "ar"),
                    new XAttribute("href", arUrl)),
                new XElement(xhtml + "link",
                    new XAttribute("rel", "alternate"),
                    new XAttribute("hreflang", "en"),
                    new XAttribute("href", enUrl)),
                new XElement(xhtml + "link",
                    new XAttribute("rel", "alternate"),
                    new XAttribute("hreflang", "x-default"),
                    new XAttribute("href", arUrl))));

            urlset.Add(new XElement(ns + "url",
                new XElement(ns + "loc", enUrl),
                new XElement(ns + "changefreq", page.ChangeFreq),
                new XElement(ns + "priority", page.Priority.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture)),
                new XElement(xhtml + "link",
                    new XAttribute("rel", "alternate"),
                    new XAttribute("hreflang", "ar"),
                    new XAttribute("href", arUrl)),
                new XElement(xhtml + "link",
                    new XAttribute("rel", "alternate"),
                    new XAttribute("hreflang", "en"),
                    new XAttribute("href", enUrl)),
                new XElement(xhtml + "link",
                    new XAttribute("rel", "alternate"),
                    new XAttribute("hreflang", "x-default"),
                    new XAttribute("href", arUrl))));
        }

        var xml = new XDocument(new XDeclaration("1.0", "utf-8", null), urlset);
        return Content(xml.ToString(), "application/xml", Encoding.UTF8);
    }
}
