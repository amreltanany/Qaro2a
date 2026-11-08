using System.Text.RegularExpressions;
using Microsoft.AspNetCore.DataProtection;

namespace ECommerce.API.Helpers;

/// <summary>
/// Keeps robots out of the "Contact us" form: a hidden trap field, a signed time stamp (a person needs a few seconds to type),
/// and a content check for the usual cold-outreach / loan / SEO / reseller pitches. Bots get a normal "sent" page and nothing is saved.
/// </summary>
public class ContactSpamGuard
{
    public const string TrapField = "website";   // hidden from people, bots fill every field
    public const string TokenField = "_t";
    private const int MinSeconds = 4;            // nobody fills name, email, subject and message faster
    private const int MaxHours = 6;

    private readonly IDataProtector _protector;
    public ContactSpamGuard(IDataProtectionProvider provider) => _protector = provider.CreateProtector("Qaro2a.ContactForm.v1");

    public string NewToken() => _protector.Protect(DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString());

    public enum Verdict { Ok, Spam, ReloadPage }

    private static readonly Regex Foreign = new(@"[\p{IsCyrillic}\p{IsBengali}\p{IsDevanagari}\p{IsThai}\p{IsCJKUnifiedIdeographs}\p{IsHiragana}\p{IsKatakana}\p{IsHangulSyllables}\p{IsGreekandCoptic}\p{IsHebrew}]", RegexOptions.Compiled);
    private static readonly Regex Link = new(@"https?://|www\.|\b[a-z0-9-]+\.(?:com|org|net|io|info|biz|xyz|ru|co|me|shop|site|online|top|click)\b", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly string[] Pitches =
    {
        "reseller", "wholesale", "your price", "about price", "business loan", "loan ", "lending", "investment", "financing", "soft loan",
        "b2b", "lead generation", "email list", "mailing list", "seo ", "backlink", "guest post", "marketing agency", "web design", "website design",
        "crypto", "bitcoin", "casino", "viagra", "forex", "outsourc", "free shipping", "% off", "shop now", "click here", "unsubscribe", "reply with \"no\""
    };

    public Verdict Check(string? trap, string? token, string name, string email, string subject, string message)
    {
        if (!string.IsNullOrWhiteSpace(trap)) return Verdict.Spam;

        long issued;
        try { issued = long.Parse(_protector.Unprotect(token ?? "")); }
        catch { return Verdict.ReloadPage; }                       // no / broken / foreign token: ask the person to reload the page
        var age = DateTimeOffset.UtcNow.ToUnixTimeSeconds() - issued;
        if (age < MinSeconds) return Verdict.Spam;                 // filled in faster than a human can type
        if (age > MaxHours * 3600) return Verdict.ReloadPage;      // page left open for hours

        var all = $"{name} {subject} {message}";
        if (email.EndsWith("@qaro2a.com", StringComparison.OrdinalIgnoreCase)) return Verdict.Spam;   // pretending to be us
        if (Foreign.IsMatch(all)) return Verdict.Spam;             // the shop speaks Arabic and English
        if (Link.IsMatch(message) || Link.IsMatch(subject)) return Verdict.Spam;
        var lower = (" " + all + " ").ToLowerInvariant();
        if (Pitches.Any(p => lower.Contains(p))) return Verdict.Spam;
        return Verdict.Ok;
    }
}
