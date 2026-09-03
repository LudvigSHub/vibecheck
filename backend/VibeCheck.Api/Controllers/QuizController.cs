using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VibeCheck.Api.DTOs;
using VibeCheck.Api.Services;

namespace VibeCheck.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class QuizController : ControllerBase
{
    private readonly QuizService _quizService;

    public QuizController(QuizService quizService)
    {
        _quizService = quizService;
    }

    // Användarens id läses ur token, aldrig ur anropet. Fick klienten skicka
    // med det själv kunde vem som helst spela quiz i någon annans namn.
    private bool TryGetUserId(out int userId)
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);

        return int.TryParse(claim, out userId);
    }

    // GET /api/quiz
    [HttpGet]
    public async Task<ActionResult<List<QuizListItemDTO>>> GetQuizzes()
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var quizzes = await _quizService.GetQuizListAsync(userId);

        return Ok(quizzes);
    }

    // POST /api/quiz/{quizId}/attempts
    [HttpPost("{quizId:int}/attempts")]
    public async Task<ActionResult<StartQuizAttemptDTO>> StartAttempt(int quizId)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        try
        {
            var attempt = await _quizService.StartAttemptAsync(userId, quizId);

            if (attempt is null)
            {
                return NotFound(new { message = "Quizet finns inte." });
            }

            return Ok(attempt);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // POST /api/quiz/attempts/{attemptId}/answers
    [HttpPost("attempts/{attemptId:int}/answers")]
    public async Task<ActionResult<AnswerResultDTO>> SubmitAnswer(
        int attemptId,
        SubmitAnswerRequestDTO request)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        try
        {
            var result = await _quizService.SubmitAnswerAsync(
                userId,
                attemptId,
                request);

            if (result is null)
            {
                return NotFound(new { message = "Quizförsöket finns inte." });
            }

            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // POST /api/quiz/attempts/{attemptId}/complete
    [HttpPost("attempts/{attemptId:int}/complete")]
    public async Task<ActionResult<QuizResultDTO>> CompleteAttempt(int attemptId)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        try
        {
            var result = await _quizService.CompleteAttemptAsync(userId, attemptId);

            if (result is null)
            {
                return NotFound(new { message = "Quizförsöket finns inte." });
            }

            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // DELETE /api/quiz/attempts/{attemptId}
    [HttpDelete("attempts/{attemptId:int}")]
    public async Task<IActionResult> AbandonAttempt(int attemptId)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        try
        {
            var removed = await _quizService.AbandonAttemptAsync(userId, attemptId);

            if (!removed)
            {
                return NotFound(new { message = "Quizförsöket finns inte." });
            }

            // 204: lyckades, men det finns ingenting att skicka tillbaka.
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}