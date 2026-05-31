namespace ECommerce.API.Options;

public class EmailOptions
{
    public const string SectionName = "Email";

    public bool Enabled { get; set; } = true;
    public string FromAddress { get; set; } = "noreply@qaro2a.com";
    public string FromName { get; set; } = "Qaro2a";
    public string SmtpHost { get; set; } = string.Empty;
    public int SmtpPort { get; set; } = 587;
    public string? SmtpUser { get; set; }
    public string? SmtpPassword { get; set; }
    public bool UseSsl { get; set; } = true;
}
