using ECommerce.Domain.Common;

namespace ECommerce.Domain.Entities;

public class Publish : BaseEntity
{
    /// <summary>Null when the manuscript was sent by a visitor without an account.</summary>
    public string? UserId { get; private set; }
    public User? User { get; private set; }
    public string? FullName { get; private set; }
    public string MobileNumber { get; private set; } = string.Empty;
    public string PdfFilePath { get; private set; } = string.Empty;

    protected Publish() { }

    public Publish(string? userId, string mobileNumber, string pdfFilePath, string? fullName = null)
    {
        UserId = userId;
        MobileNumber = mobileNumber;
        PdfFilePath = pdfFilePath;
        FullName = fullName;
    }
}
