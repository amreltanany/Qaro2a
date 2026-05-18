using System.ComponentModel.DataAnnotations;

namespace ECommerce.Application.DTOs.Account
{
    public record LoginDto(
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        string Email,
        [Required(ErrorMessage = "Password is required.")]
        string Password);

    public record RegisterDto(
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        string Email,
        [Required(ErrorMessage = "Password is required.")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters.")]
        string Password,
        [Required(ErrorMessage = "Full name is required.")]
        [MinLength(1, ErrorMessage = "Full name cannot be empty.")]
        string FullName,
        [Range(typeof(bool), "true", "true", ErrorMessage = "You must agree to the Terms of Service.")]
        bool AgreeToTerms);

    public record AuthResponseDto(string Email, string Token, string FullName);
}
