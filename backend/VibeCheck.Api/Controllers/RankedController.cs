using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VibeCheck.Api.DTOs;
using VibeCheck.Api.Services;

namespace VibeCheck.Api.Controllers;

// Hela controllern kräver inloggning: bara inloggade får spela rankat.
[ApiController]
[Authorize]
[Route("api/[controller]")]
public class RankedController : ControllerBase
{
    private readonly RankedQuizService _rankedQuizService;

    public RankedController(RankedQuizService rankedQuizService)
    {
        _rankedQuizService = rankedQuizService;
    }

    private bool TryGetUserId(out int userId)
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);

        return int.TryParse(claim, out userId);
    }

    // POST /api/ranked/attempts
    [HttpPost("attempts")]
    public async Task<ActionResult<StartRankedAttemptDTO>> StartAttempt()
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var attempt = await _rankedQuizService.StartAttemptAsync(userId);

        return Ok(attempt);
    }

    // POST /api/ranked/attempts/{attemptId}/answers
    [HttpPost("attempts/{attemptId:int}/answers")]
    public async Task<ActionResult<RankedAnswerResultDTO>> SubmitAnswer(
        int attemptId,
        SubmitAnswerRequestDTO request)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        try
        {
            var result = await _rankedQuizService.SubmitAnswerAsync(
                userId,
                attemptId,
                request);

            if (result is null)
            {
                return NotFound(new { message = "Omgången finns inte." });
            }

            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // POST /api/ranked/attempts/{attemptId}/complete
    [HttpPost("attempts/{attemptId:int}/complete")]
    public async Task<ActionResult<RankedResultDTO>> CompleteAttempt(int attemptId)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        try
        {
            var result = await _rankedQuizService.CompleteAttemptAsync(
                userId,
                attemptId);

            if (result is null)
            {
                return NotFound(new { message = "Omgången finns inte." });
            }

            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}