using ECommerce.Domain.Common;

namespace ECommerce.Domain.Entities;

public class Publish : BaseEntity
{
    public string UserId { get; private set; } = string.Empty;
    public User? User { get; private set; }
    public string MobileNumber { get; private set; } = string.Empty;
    public string PdfFilePath { get; private set; } = string.Empty;

    protected Publish() { }

    public Publish(string userId, string mobileNumber, string pdfFilePath)
    {
        UserId = userId;
        MobileNumber = mobileNumber;
        PdfFilePath = pdfFilePath;
    }
}
