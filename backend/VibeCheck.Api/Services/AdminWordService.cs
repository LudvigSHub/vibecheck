using Microsoft.EntityFrameworkCore;
using VibeCheck.Api.DTOs;
using VibeCheck.Data.Data;
using VibeCheck.Data.Models;

namespace VibeCheck.Api.Services;

public class AdminWordService
{
    private readonly VibeCheckDbContext _context;

    public AdminWordService(VibeCheckDbContext context)
    {
        _context = context;
    }

    public async Task<List<AdminWordListItemDTO>> GetAllAsync()
    {
        return await _context.Words
            .AsNoTracking()
            .OrderBy(word => word.WordDesc)
            .Select(word => new AdminWordListItemDTO
            {
                WordId = word.WordID,
                Word = word.WordDesc,
                Meaning = word.Meaning.MeaningText,
                ExampleCount = word.WordExamples.Count(),

                Tags = word.WordTags
                    .OrderBy(wordTag => wordTag.Tag.TagName)
                    .Select(wordTag => new AdminTagListItemDTO
                    {
                        TagId = wordTag.TagID,
                        TagName = wordTag.Tag.TagName
                    })
                    .ToList(),

                IsUsedInQuiz = word.Questions.Any()
            })
            .ToListAsync();
    }

    public async Task<AdminWordListItemDTO> CreateAsync(AdminCreateWordDTO request)
    {
        // Trimma bort onödiga mellanslag i början/slutet.
        // Det gör att t.ex. " rizz " behandlas som "rizz".
        var wordText = request.Word.Trim();
        var meaningText = request.Meaning.Trim();

        // Rensa meningsexempel:
        // - trimma varje exempel
        // - ta bort tomma exempel
        // - ta bort dubletter, oavsett stora/små bokstäver
        var examples = request.Examples
            .Select(example => example.Trim())
            .Where(example => !string.IsNullOrWhiteSpace(example))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        // Samma tagg ska inte kunna skickas flera gånger.
        // Exempel: [1, 1, 3] blir [1, 3].
        var tagIds = request.TagIds
            .Distinct()
            .ToList();

        // Validering av obligatoriska fält
        if (string.IsNullOrWhiteSpace(wordText))
        {
            throw new InvalidOperationException("Ord måste anges.");
        }

        if (string.IsNullOrWhiteSpace(meaningText))
        {
            throw new InvalidOperationException("Betydelse måste anges.");
        }

        if (examples.Count == 0)
        {
            throw new InvalidOperationException("Minst ett meningsexempel måste anges.");
        }

        if (tagIds.Count == 0)
        {
            throw new InvalidOperationException("Minst en tagg måste väljas.");
        }


        // Kontrollera att ordet inte redan finns

        // Vi vill inte skapa dubletter i ordboken.
        // Om ordet redan finns avbryts skapandet innan något sparas.
        var wordExists = await _context.Words
            .AnyAsync(word => word.WordDesc == wordText);

        if (wordExists)
        {
            throw new InvalidOperationException(
                $"Ordet '{wordText}' finns redan i ordboken.");
        }

        // Kontrollera valda taggar 

        // Vi hämtar endast taggar som redan finns i databasen.
        // Create Word får alltså INTE skapa nya taggar.
        //
        // Det är viktigt eftersom admin ska välja från befintliga taggar,
        // istället för att kunna skapa felstavade eller nästan identiska taggar.
        var tags = await _context.Tags
            .Where(tag => tagIds.Contains(tag.TagID))
            .OrderBy(tag => tag.TagName)
            .ToListAsync();

        // Om antalet hittade taggar inte matchar antalet skickade TagIds
        // betyder det att minst ett ID inte finns i databasen.
        if (tags.Count != tagIds.Count)
        {
            throw new InvalidOperationException(
                "En eller flera av de valda taggarna finns inte.");
        }

        // Återanvänd betydelsen om exakt samma betydelse redan finns.
        var meaning = await _context.Meanings
            .FirstOrDefaultAsync(m => m.MeaningText == meaningText);

        if (meaning is null)
        {
            meaning = new Meaning
            {
                MeaningText = meaningText
            };
        }

        // Skapa själva ordet

        // Vi kopplar ordet till antingen den befintliga
        // eller den nyss skapade Meaning-instansen.
        var word = new Word
        {
            WordDesc = wordText,
            Meaning = meaning
        };

        // Lägg till meningsexempel

        // Varje exempel skapas som ett WordExample
        // och kopplas till ordet via navigation property.
        foreach (var example in examples)
        {
            word.WordExamples.Add(new WordExample
            {
                ExampleText = example
            });
        }

        // Koppla ordet till befintliga taggar

        // Vi skapar bara WordTag-relationerna.
        // Själva Tag-objekten är de som redan finns i databasen.
        foreach (var tag in tags)
        {
            word.WordTags.Add(new WordTag
            {
                Tag = tag
            });
        }

        // Spara allt i databasen

        // EF Core kommer här att:
        // - skapa Word
        // - skapa eventuell ny Meaning
        // - skapa WordExamples
        // - skapa WordTags
        // - sätta rätt foreign keys
        _context.Words.Add(word);

        await _context.SaveChangesAsync();

        // Returnera det skapade ordet som DTO

        // Vi returnerar samma typ av objekt som används i adminlistan.
        // IsUsedInQuiz är false eftersom ett helt nytt ord ännu
        // inte kan användas av någon befintlig quizfråga.
        return new AdminWordListItemDTO
        {
            WordId = word.WordID,
            Word = word.WordDesc,
            Meaning = meaning.MeaningText,
            ExampleCount = word.WordExamples.Count,
            Tags = tags
                .Select(tag => new AdminTagListItemDTO
                {
                    TagId = tag.TagID,
                    TagName = tag.TagName
                })
                .ToList(),

            IsUsedInQuiz = false
        };
    }

    public async Task<AdminWordDetailsDTO?> GetByIdAsync(int wordId)
    {
        return await _context.Words
            .AsNoTracking()
            .Where(word => word.WordID == wordId)
            .Select(word => new AdminWordDetailsDTO
            {
                WordId = word.WordID,
                Word = word.WordDesc,
                Meaning = word.Meaning.MeaningText,

                Examples = word.WordExamples
                    .OrderBy(example => example.ExampleID)
                    .Select(example => example.ExampleText)
                    .ToList(),

                Tags = word.WordTags
                    .OrderBy(wordTag => wordTag.Tag.TagName)
                    .Select(wordTag => new AdminTagListItemDTO
                    {
                        TagId = wordTag.TagID,
                        TagName = wordTag.Tag.TagName
                    })
                    .ToList(),

                IsUsedInQuiz = word.Questions.Any()
            })
            .FirstOrDefaultAsync();
    }
}