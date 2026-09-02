using Microsoft.AspNetCore.Mvc;
using VibeCheck.Api.Services;

namespace VibeCheck.Api.Controllers;

[ApiController]
[Route("api/[controller]")]

//Controllern är döpt WordsController och inte WordOfTheDayController med flit, den kommer få sällskap av GET /api/words, GET /api/words/{id} och sökning när ordboken byggs tänker jag? //Jonathan
public class WordsController : ControllerBase
{
    private readonly WordOfTheDayService _wordOfTheDayService;

    // Svensk tid, inte UTC. Annars byts dagens ord vid 01:00 eller 02:00 beroende på sommartid, i stället för vid midnatt.
    private static readonly TimeZoneInfo SwedishTime =
        TimeZoneInfo.FindSystemTimeZoneById("Europe/Stockholm");

    public WordsController(WordOfTheDayService wordOfTheDayService)
    {
        _wordOfTheDayService = wordOfTheDayService;
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