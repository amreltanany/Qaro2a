using System.ComponentModel.DataAnnotations;

namespace ECommerce.Application.DTOs.Account
{
    public record LoginDto(
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        [MaxLength(256, ErrorMessage = "Email is too long.")]
        string Email,
        [Required(ErrorMessage = "Password is required.")]
        string Password);

    public record RegisterDto(
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        [MaxLength(256, ErrorMessage = "Email is too long.")]
        string Email,
        [Required(ErrorMessage = "Password is required.")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters.")]
        [MaxLength(128, ErrorMessage = "Password is too long.")]
        string Password,
        [Required(ErrorMessage = "Full name is required.")]
        [MinLength(1, ErrorMessage = "Full name cannot be empty.")]
        [MaxLength(100, ErrorMessage = "Full name is too long.")]
        string FullName,
        [Range(typeof(bool), "true", "true", ErrorMessage = "You must agree to the Terms of Service.")]
        bool AgreeToTerms);

    public record AuthResponseDto(string Email, string Token, string FullName);

    public record ForgotPasswordDto(
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        [MaxLength(256, ErrorMessage = "Email is too long.")]
        string Email);

    public record ResetPasswordDto(
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        [MaxLength(256, ErrorMessage = "Email is too long.")]
        string Email,
        [Required(ErrorMessage = "Verification code is required.")]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "Verification code must be 6 digits.")]
        string Code,
        [Required(ErrorMessage = "Password is required.")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters.")]
        [MaxLength(128, ErrorMessage = "Password is too long.")]
        string NewPassword);
}
