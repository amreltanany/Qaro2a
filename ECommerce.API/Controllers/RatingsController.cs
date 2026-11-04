using ECommerce.API.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ECommerce.API.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/[controller]")]
public class RatingsController : ControllerBase
{
    private readonly RatingStore _ratings;
    public RatingsController(RatingStore ratings) => _ratings = ratings;

    public record RateRequest(int ProductId, int Stars);

    // one rating per visitor and book (signed-in user, or the guest cookie); the same visitor can change it
    [HttpPost]
    [EnableRateLimiting("rating")]
    public async Task<IActionResult> Rate([FromBody] RateRequest req)
    {
        if (req == null || req.ProductId <= 0 || req.Stars < 1 || req.Stars > 5)
            return BadRequest(new { message = "Stars must be between 1 and 5." });

        var key = UserClaimsHelper.GetUserId(User)
                  ?? UserClaimsHelper.GetUserIdFromJwtString(Request.Cookies["token"])
                  ?? CartOwner.GuestKey(HttpContext, true);
        if (string.IsNullOrEmpty(key)) return BadRequest();

        var result = await _ratings.RateAsync(req.ProductId, key, req.Stars);
        if (result == null) return NotFound();
        return Ok(new { average = result.Value.Avg, count = result.Value.Count, mine = req.Stars });
    }
}
