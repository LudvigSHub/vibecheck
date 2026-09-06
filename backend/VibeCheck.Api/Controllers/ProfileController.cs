using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VibeCheck.Api.DTOs;
using VibeCheck.Api.Services;

namespace VibeCheck.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class ProfileController : ControllerBase
{
    private readonly ProfileService _profileService;

    public ProfileController(ProfileService profileService)
    {
        _profileService = profileService;
    }

    // GET /api/profile/me
    [HttpGet("me")]
    public async Task<ActionResult<ProfileDTO>> GetMe()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        var profile = await _profileService.GetProfileAsync(userId);

        if (profile == null)
        {
            return NotFound();
        }

        return Ok(profile);
    }
}