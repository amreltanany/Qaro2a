using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using ECommerce.Application.DTOs.Account;
using Microsoft.AspNetCore.Http;
using ECommerce.Application.Services.Interfaces;
using ECommerce.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

[Route("api/[controller]")]
[ApiController]
public class AccountController : ControllerBase
{
    private readonly UserManager<User> _userManager;
    private readonly ITokenService _tokenService;

    public AccountController(UserManager<User> userManager, ITokenService tokenService)
    {
        _userManager = userManager;
        _tokenService = tokenService;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login(LoginDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null)
            return StatusCode(StatusCodes.Status401Unauthorized, new { message = "Invalid email or password." });

        var result = await _userManager.CheckPasswordAsync(user, dto.Password);
        if (!result)
            return StatusCode(StatusCodes.Status401Unauthorized, new { message = "Invalid email or password." });

        var response = new AuthResponseDto(
            user.Email ?? string.Empty,
            _tokenService.CreateToken(user),
            user.FullName ?? string.Empty);
        return Ok(response);
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