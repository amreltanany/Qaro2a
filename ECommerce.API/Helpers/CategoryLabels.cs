namespace ECommerce.API.Helpers
{
    /// <summary>Category names are stored in Arabic; this gives the English label when the visitor browses in English.</summary>
    public static class CategoryLabels
    {
        private static readonly Dictionary<string, string> En = new()
        {
            ["اجتماعي ودراما"] = "Social & Drama",
            ["فانتازيا وخيال علمي"] = "Fantasy & Sci-Fi",
            ["جريمة وغموض وإثارة"] = "Crime, Mystery & Thriller",
            ["رومانسي"] = "Romance",
            ["تدريب وهوايات"] = "Training & Hobbies",
            ["نفسي"] = "Psychological",
            ["تاريخي وسير"] = "History & Biography",
            ["فكر وثقافة"] = "Thought & Culture",
            ["رعب"] = "Horror",
        };

        public static string Localize(string? name, bool isArabic)
        {
            if (string.IsNullOrWhiteSpace(name)) return string.Empty;
            return !isArabic && En.TryGetValue(name.Trim(), out var en) ? en : name;
        }
    }
}
