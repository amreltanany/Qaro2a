using ECommerce.Domain.Common;

namespace ECommerce.Domain.Entities;

public class Contact : BaseEntity
{
    public string UserName { get; private set; } = string.Empty;
    public string UserEmail { get; private set; } = string.Empty;
    public string Subject { get; private set; } = string.Empty;
    public string Message { get; private set; } = string.Empty;

    protected Contact() { }

    public Contact(string userName, string userEmail, string subject, string message)
    {
        UserName = userName;
        UserEmail = userEmail;
        Subject = subject;
        Message = message;
    }
}
