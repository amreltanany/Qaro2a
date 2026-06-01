using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using ECommerce.Application.DTOs.Account;
using Microsoft.AspNetCore.Http;
using ECommerce.Application.Services.Interfaces;
using ECommerce.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

[Route("api/[controller]")]
[ApiController]
public class AccountController : ControllerBase
{
    private readonly UserManager<User> _userManager;
    private readonly ITokenService _tokenService;
    private readonly IEmailService _emailService;
    private readonly IPasswordResetCodeService _passwordResetCodeService;
    private readonly ILogger<AccountController> _logger;

    public AccountController(
        UserManager<User> userManager,
        ITokenService tokenService,
        IEmailService emailService,
        IPasswordResetCodeService passwordResetCodeService,
        ILogger<AccountController> logger)
    {
        _userManager = userManager;
        _tokenService = tokenService;
        _emailService = emailService;
        _passwordResetCodeService = passwordResetCodeService;
        _logger = logger;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginDto dto)
    {
        if (dto is null)
            return BadRequest(new { message = "Invalid login request." });

        var email = dto.Email?.Trim();
        var password = dto.Password ?? string.Empty;
        var user = await _userManager.FindByEmailAsync(email ?? string.Empty);
        if (user == null)
            return StatusCode(StatusCodes.Status401Unauthorized, new { message = "Invalid email or password." });

        var result = await _userManager.CheckPasswordAsync(user, password);
        if (!result)
            return StatusCode(StatusCodes.Status401Unauthorized, new { message = "Invalid email or password." });

        var response = new AuthResponseDto(
            user.Email ?? string.Empty,
            _tokenService.CreateToken(user),
            user.FullName ?? string.Empty);
        return Ok(response);
    }

    [AllowAnonymous]
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
    {
        var email = dto.Email?.Trim();
        if (string.IsNullOrWhiteSpace(email))
            return BadRequest(new { message = "Email is required." });

        var user = await _userManager.FindByEmailAsync(email);
        if (user is not null)
        {
            var code = _passwordResetCodeService.GenerateAndStore(email, user.Id);
            var subject = "Qaro2a — Password reset code";
            var body = $"""
                <p>Hello{(string.IsNullOrWhiteSpace(user.FullName) ? "" : $", {System.Net.WebUtility.HtmlEncode(user.FullName)}")},</p>
                <p>Your password reset code is:</p>
                <p style="font-size:24px;font-weight:bold;letter-spacing:4px;">{code}</p>
                <p>This code expires in 15 minutes. If you did not request a reset, you can ignore this email.</p>
                """;
            try
            {
                await _emailService.SendAsync(email, subject, body);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Password reset email failed for {Email}", email);
                return StatusCode(StatusCodes.Status503ServiceUnavailable, new
                {
                    message = "We could not send the reset email right now. Please try again later or contact support."
                });
            }
        }

        return Ok(new { message = "If an account exists for that email, a reset code has been sent." });
    }

    [AllowAnonymous]
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
    {
        var email = dto.Email?.Trim();
        if (string.IsNullOrWhiteSpace(email))
            return BadRequest(new { message = "Email is required." });

        if (!_passwordResetCodeService.TryValidate(email, dto.Code, out var userId) || string.IsNullOrEmpty(userId))
            return BadRequest(new { message = "Invalid or expired verification code." });

        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
            return BadRequest(new { message = "Invalid or expired verification code." });

        var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
        var result = await _userManager.ResetPasswordAsync(user, resetToken, dto.NewPassword);
        if (!result.Succeeded)
        {
            var msg = string.Join(" ", result.Errors.Select(e => e.Description));
            return BadRequest(new { message = string.IsNullOrWhiteSpace(msg) ? "Could not reset password." : msg });
        }

        _passwordResetCodeService.Remove(email);
        return Ok(new { message = "Password updated successfully. You can now log in." });
    }

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponseDto>> Register(RegisterDto dto)
    {
        if (await _userManager.FindByEmailAsync(dto.Email) != null)
            return BadRequest(new { message = "This email is already registered." });

        var user = new User { UserName = dto.Email, Email = dto.Email, FullName = dto.FullName };
        var result = await _userManager.CreateAsync(user, dto.Password);

        if (!result.Succeeded)
        {
            var msg = string.Join(" ", result.Errors.Select(e => e.Description));
            return BadRequest(new { message = string.IsNullOrWhiteSpace(msg) ? "Registration could not be completed." : msg });
        }

        var response = new AuthResponseDto(
            user.Email ?? string.Empty,
            _tokenService.CreateToken(user),
            user.FullName ?? string.Empty);
        return CreatedAtAction(nameof(GetCurrentUser), new { }, response);
    }

    [Authorize]
    [HttpGet("me")]
    public IActionResult GetCurrentUser()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue(JwtRegisteredClaimNames.NameId)
            ?? User.FindFirstValue("sub");
        if (string.IsNullOrEmpty(userId)) return Unauthorized();
        return Ok(new { userId });
    }
}