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

    [HttpPut("me/username")]
    public async Task<IActionResult> UpdateUserName(
    UpdateUserNameRequestDTO request)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        var result = await _profileService.UpdateUserNameAsync(
            userId,
            request.NewUserName);

        if (!result.Succeeded)
        {
            return BadRequest(result.Errors.Select(e => e.Description));
        }

        return NoContent();
    }

    [HttpPut("me/password")]
    public async Task<IActionResult> ChangePassword(
        ChangePasswordRequestDTO request)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        var result = await _profileService.ChangePasswordAsync(
            userId,
            request.CurrentPassword,
            request.NewPassword);

        if (!result.Succeeded)
        {
            return BadRequest(result.Errors.Select(e => e.Description));
        }

        return NoContent();
    }
}   