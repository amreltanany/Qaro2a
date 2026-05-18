using System.ComponentModel.DataAnnotations;

namespace ECommerce.Application.DTOs.Contact;

public class ContactSubmissionDto
{
    [Required(ErrorMessage = "Name is required.")]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress]
    [MaxLength(256)]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Subject is required.")]
    [MaxLength(500)]
    public string Subject { get; set; } = string.Empty;

    [Required(ErrorMessage = "Message is required.")]
    [MaxLength(5000)]
    public string Message { get; set; } = string.Empty;
}
