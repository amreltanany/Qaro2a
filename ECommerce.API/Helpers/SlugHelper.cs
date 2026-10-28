using System.Text.RegularExpressions;

namespace ECommerce.API.Helpers
{
    public static class SlugHelper
    {
        /// <summary>"وليمة للغربان (النسخة العالمية)" -> "وليمة-للغربان-النسخة-العالمية"</summary>
        public static string Slug(string? text)
        {
            if (string.IsNullOrWhiteSpace(text)) return string.Empty;
            var s = Regex.Replace(text, @"[\(\)\[\]{}:؛،,.!؟?""'«»/\\|]", " ");
            s = Regex.Replace(s.Trim(), @"\s+", "-");
            return s.Trim('-');
        }

        /// <summary>/Home/Product/5046/قصر-البارون (percent-encoded; browsers show it decoded)</summary>
        public static string ProductPath(int id, string? name)
        {
            var slug = Slug(name);
            return slug.Length == 0 ? $"/Home/Product/{id}" : $"/Home/Product/{id}/{Uri.EscapeDataString(slug)}";
        }
    }
}
