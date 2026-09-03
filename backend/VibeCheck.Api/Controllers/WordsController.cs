using Microsoft.AspNetCore.Mvc;
using VibeCheck.Api.Services;

namespace VibeCheck.Api.Controllers;

[ApiController]
[Route("api/[controller]")]

//Controllern är döpt WordsController och inte WordOfTheDayController med flit, den kommer få sällskap av GET /api/words, GET /api/words/{id} och sökning när ordboken byggs tänker jag? //Jonathan
public class WordsController : ControllerBase
{
    private readonly WordOfTheDayService _wordOfTheDayService;
    private readonly WordStashService _wordStashService;
    // Svensk tid, inte UTC. Annars byts dagens ord vid 01:00 eller 02:00 beroende på sommartid, i stället för vid midnatt.
    private static readonly TimeZoneInfo SwedishTime =
        TimeZoneInfo.FindSystemTimeZoneById("Europe/Stockholm");

    public WordsController(
        WordOfTheDayService wordOfTheDayService,
        WordStashService wordStashService)
    {
        _wordOfTheDayService = wordOfTheDayService;
        _wordStashService = wordStashService;
    }

    // GET /api/words
    // GET /api/words?search=fire
    // GET /api/words?tag=ungdomsslang
    // GET /api/words?search=fire&tag=ungdomsslang
    [HttpGet]
    public async Task<IActionResult> GetWords(
        [FromQuery] string? search,
        [FromQuery] string? tag)
    {
        var words = await _wordStashService.GetWordsAsync(search, tag);

        return Ok(words);
    }

    // GET /api/words/{id}
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetWord(int id)
    {
        var word = await _wordStashService.GetWordByIdAsync(id);

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
}