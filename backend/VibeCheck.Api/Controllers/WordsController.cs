using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using VibeCheck.Api.DTOs;
using VibeCheck.Api.Services;

namespace VibeCheck.Api.Controllers;

[ApiController]
[Route("api/[controller]")]

//Controllern är döpt WordsController och inte WordOfTheDayController med flit, den kommer få sällskap av GET /api/words, GET /api/words/{id} och sökning när ordboken byggs tänker jag? //Jonathan
public class WordsController : ControllerBase
{
    private readonly WordOfTheDayService _wordOfTheDayService;
    private readonly QuizDemoService _quizDemoService;
    private readonly WordStashService _wordStashService;
    private readonly WordVoteService _wordVoteService;

    // Svensk tid, inte UTC. Annars byts dagens ord vid 01:00 eller 02:00 beroende på sommartid, i stället för vid midnatt.
    private static readonly TimeZoneInfo SwedishTime =
        TimeZoneInfo.FindSystemTimeZoneById("Europe/Stockholm");

    public WordsController(
        WordOfTheDayService wordOfTheDayService,
        WordStashService wordStashService,
        WordVoteService wordVoteService,
        QuizDemoService quizDemoService)
    {
        _wordOfTheDayService = wordOfTheDayService;
        _wordStashService = wordStashService;
        _quizDemoService = quizDemoService;
        _wordVoteService = wordVoteService;
    }

    // GET /api/words
    // GET /api/words?search=fire
    // GET /api/words?tags=ungdomsslang
    // GET /api/words?search=fire&tags=ungdomsslang
    [HttpGet]
    public async Task<IActionResult> GetWords(
    [FromQuery] string? search,
    [FromQuery] string[]? tags)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

        int? userId = null;

        if (int.TryParse(userIdClaim, out var parsedUserId))
        {
            userId = parsedUserId;
        }

        var words = await _wordStashService.GetWordsAsync(
            search,
            tags,
            userId);

        return Ok(words);
    }

    // GET /api/words/{id}
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetWord(int id)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

        int? userId = null;

        if (int.TryParse(userIdClaim, out var parsedUserId))
        {
            userId = parsedUserId;
        }

        var word = await _wordStashService.GetWordByIdAsync(
            id,
            userId);

        if (word is null)
        {
            return NotFound(new
            {
                message = "Ordet hittades inte."
            });
        }

        return Ok(word);
    }

    // GET /api/words/tags
    [HttpGet("tags")]
    public async Task<IActionResult> GetTags()
    {
        var tags = await _wordStashService.GetTagsAsync();

        return Ok(tags);
    }

    // GET /api/words/word-of-the-day
    [HttpGet("word-of-the-day")]
    public async Task<IActionResult> GetWordOfTheDay()
    {
        var nowInSweden = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, SwedishTime);

        var today = DateOnly.FromDateTime(nowInSweden.DateTime);

        var word = await _wordOfTheDayService.GetForDateAsync(today);

        if (word is null)
        {
            return NotFound(new { message = "Det finns inga ord i databasen än." });
        }

        return Ok(word);
    }

    // GET /api/words/quiz-demo?count=10
    [HttpGet("quiz-demo")]
    public async Task<IActionResult> GetQuizDemo([FromQuery] int count = 10)
    {
        var questions = await _quizDemoService.GetQuestionsAsync(count);

        return Ok(questions);
    }

    [Authorize]
    [HttpPost("{wordId:int}/vote")]
    public async Task<IActionResult> Vote(
    int wordId,
    [FromBody] WordVoteDTO vote)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        await _wordVoteService.VoteAsync(
            wordId,
            userId,
            vote.IsPositive);

        return Ok(new { message = "Röst sparad." });
    }

    [Authorize]
    [HttpDelete("{wordId:int}/vote")]
    public async Task<IActionResult> RemoveVote(int wordId)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        await _wordVoteService.RemoveVoteAsync(wordId, userId);

        return Ok(new { message = "Röst borttagen." });
    }
}