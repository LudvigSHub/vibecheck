using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using VibeCheck.Api.DTOs;
using VibeCheck.Api.Services;

namespace VibeCheck.Api.Controllers;

// Ingen [Authorize]: topplistan är publik. Skickas ändå en giltig token med
// fyller autentiseringen i User åt oss, och då kan svaret innehålla
// besökarens egen rad. Samma mönster som WordsController använder för
// röstningen på WordStash.
[ApiController]
[Route("api/[controller]")]
public class LeaderboardController : ControllerBase
{
    private readonly RankedQuizService _rankedQuizService;

    public LeaderboardController(RankedQuizService rankedQuizService)
    {
        _rankedQuizService = rankedQuizService;
    }

    // GET /api/leaderboard
    // GET /api/leaderboard?week=previous
    [HttpGet]
    public async Task<ActionResult<LeaderboardDTO>> GetLeaderboard(
        [FromQuery] string week = "current")
    {
        // Bara två veckor är åtkomliga. Ett okänt värde avvisas i stället för
        // att tyst tolkas som "current" – då hade en felstavning sett ut som
        // ett tomt resultat.
        bool previousWeek;

        if (string.Equals(week, "current", StringComparison.OrdinalIgnoreCase))
        {
            previousWeek = false;
        }
        else if (string.Equals(week, "previous", StringComparison.OrdinalIgnoreCase))
        {
            previousWeek = true;
        }
        else
        {
            return BadRequest(new
            {
                message = "week måste vara 'current' eller 'previous'."
            });
        }

        int? userId = null;

        if (int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var parsed))
        {
            userId = parsed;
        }

        var leaderboard = await _rankedQuizService.GetLeaderboardAsync(
            previousWeek,
            userId);

        return Ok(leaderboard);
    }
}