using Microsoft.EntityFrameworkCore;
using VibeCheck.Api.DTOs;
using VibeCheck.Data.Data;

namespace VibeCheck.Api.Services;

public class WordOfTheDayService
{
    private readonly VibeCheckDbContext _context;

    // Nollpunkten för räkningen. Ändras den här ändras hela schemat för
    // vilket ord som visas vilken dag, så låt den ligga.

    //En "Epoch" (epok) är en specifik tidpunkt eller en utmärkande period som markerar början på en ny utveckling eller en stor historisk förändring.
    private static readonly DateOnly Epoch = new(2026, 1, 1);

    // Primtal. Sprider ut valet så att orden inte kommer i ID-ordning, men
    // besöker ändå varje ord exakt en gång innan cykeln börjar om.
    private const int Stride = 7919;

    public WordOfTheDayService(VibeCheckDbContext context)
    {
        _context = context;
    }

    // Samma datum ger alltid samma ord. Det är hela poängen – annars vore
    // det inte "dagens ord" utan "ett slumpat ord".
    public async Task<WordOfTheDayDTO?> GetForDateAsync(DateOnly date)
    {
        //Hämtar antal ord i ordlista
        var wordCount = await _context.Words.CountAsync();
        //Om det inte finns några ord, returnera inget.
        if (wordCount == 0)
        {
            return null;
        }

        //Tar det datum vi ger metoden (Dagens datum) minus epoch.
        //För att "Dagens ord" kräver att alla användare ser exakt samma ord samma dag. Genom att omvandla ett datum till ett unikt dagnummer får koden ett stabilt tal att räkna på.
        var daysSinceEpoch = date.DayNumber - Epoch.DayNumber;

        // long för att undvika overflow, och det extra varvet nedanför för
        // att C#:s modulo kan ge negativa resultat för datum före Epoch.
        var raw = (long)daysSinceEpoch * Stride % wordCount;

        var index = (int)((raw + wordCount) % wordCount);

        return await _context.Words
            .OrderBy(w => w.WordID)
            .Skip(index)
            .Take(1)
            .Select(w => new WordOfTheDayDTO
            {
                WordId = w.WordID,
                Word = w.WordDesc,
                Meaning = w.Meaning.MeaningText,
                Example = w.WordExamples
                    .OrderBy(e => e.ExampleID)
                    .Select(e => e.ExampleText)
                    .FirstOrDefault(),
                Date = date
            })
            .FirstOrDefaultAsync();
    }
}