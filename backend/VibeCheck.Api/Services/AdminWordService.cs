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

    public async Task<AdminWordDetailsDTO?> UpdateAsync(
    int wordId,
    AdminUpdateWordDTO request)
    {
        // Rensa inkommande värden på samma sätt som vid Create.
        var wordText = request.Word.Trim();
        var meaningText = request.Meaning.Trim();

        var examples = request.Examples
            .Select(example => example.Trim())
            .Where(example => !string.IsNullOrWhiteSpace(example))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var tagIds = request.TagIds
            .Distinct()
            .ToList();


        // -----------------------------------------
        // Validering av obligatoriska fält
        // -----------------------------------------

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
            throw new InvalidOperationException(
                "Minst ett meningsexempel måste anges.");
        }

        if (tagIds.Count == 0)
        {
            throw new InvalidOperationException(
                "Minst en tagg måste väljas.");
        }


        // -----------------------------------------
        // Hämta ordet som ska redigeras
        // -----------------------------------------

        // Här behöver vi tracking eftersom vi faktiskt ska ändra datan.
        // Vi laddar även befintliga exempel och taggkopplingar
        // eftersom de ska kunna ersättas.
        var word = await _context.Words
            .Include(word => word.WordExamples)
            .Include(word => word.WordTags)
            .FirstOrDefaultAsync(word => word.WordID == wordId);

        if (word is null)
        {
            return null;
        }


        // -----------------------------------------
        // Kontrollera att det nya ordet inte krockar
        // med något ANNAT ord
        // -----------------------------------------

        var duplicateWordExists = await _context.Words
            .AnyAsync(existingWord =>
                existingWord.WordID != wordId &&
                existingWord.WordDesc == wordText);

        if (duplicateWordExists)
        {
            throw new InvalidOperationException(
                $"Ordet '{wordText}' finns redan i ordboken.");
        }


        // -----------------------------------------
        // Kontrollera valda taggar
        // -----------------------------------------

        // Precis som vid Create får Update bara använda
        // taggar som redan finns i databasen.
        var tags = await _context.Tags
            .Where(tag => tagIds.Contains(tag.TagID))
            .OrderBy(tag => tag.TagName)
            .ToListAsync();

        if (tags.Count != tagIds.Count)
        {
            throw new InvalidOperationException(
                "En eller flera av de valda taggarna finns inte.");
        }


        // -----------------------------------------
        // Hitta eller skapa Meaning
        // -----------------------------------------

        // Vi ändrar INTE texten på word.Meaning direkt.
        // Flera Word kan enligt datamodellen dela samma Meaning.
        //
        // I stället letar vi efter rätt Meaning och kopplar
        // ordet till den.
        var meaning = await _context.Meanings
            .FirstOrDefaultAsync(m => m.MeaningText == meaningText);

        if (meaning is null)
        {
            meaning = new Meaning
            {
                MeaningText = meaningText
            };
        }


        // -----------------------------------------
        // Uppdatera Word och Meaning-relationen
        // -----------------------------------------

        word.WordDesc = wordText;
        word.Meaning = meaning;


        // -----------------------------------------
        // Ersätt befintliga meningsexempel
        // -----------------------------------------

        // Det enklaste och tydligaste här är att ta bort
        // de gamla exemplen och skapa relationerna på nytt.
        _context.WordExamples.RemoveRange(word.WordExamples);

        word.WordExamples.Clear();

        foreach (var example in examples)
        {
            word.WordExamples.Add(new WordExample
            {
                ExampleText = example
            });
        }


        // -----------------------------------------
        // Ersätt befintliga taggkopplingar
        // -----------------------------------------

        // Vi tar bara bort WordTag-kopplingarna.
        // Själva Tag-raderna i databasen påverkas inte.
        _context.WordTags.RemoveRange(word.WordTags);

        word.WordTags.Clear();

        foreach (var tag in tags)
        {
            word.WordTags.Add(new WordTag
            {
                Tag = tag
            });
        }


        // -----------------------------------------
        // Spara ändringarna
        // -----------------------------------------

        await _context.SaveChangesAsync();


        // -----------------------------------------
        // Returnera den uppdaterade detaljvyn
        // -----------------------------------------

        return new AdminWordDetailsDTO
        {
            WordId = word.WordID,
            Word = word.WordDesc,
            Meaning = meaning.MeaningText,

            Examples = word.WordExamples
                .OrderBy(example => example.ExampleID)
                .Select(example => example.ExampleText)
                .ToList(),

            Tags = tags
                .Select(tag => new AdminTagListItemDTO
                {
                    TagId = tag.TagID,
                    TagName = tag.TagName
                })
                .ToList(),

            IsUsedInQuiz = await _context.Questions
                .AnyAsync(question => question.WordID == word.WordID)
        };
    }

    public async Task<bool> DeleteAsync(int wordId)
    {
        // -----------------------------------------
        // Hämta ordet som ska tas bort
        // -----------------------------------------

        var word = await _context.Words
            .FirstOrDefaultAsync(word => word.WordID == wordId);

        // Om ordet inte finns returnerar vi false.
        // Controllern kan sedan översätta detta till 404 Not Found.
        if (word is null)
        {
            return false;
        }


        // -----------------------------------------
        // Kontrollera om ordet används i quiz
        // -----------------------------------------

        // Question -> Word har Restrict-delete i datamodellen.
        // Ett ord som används av en quizfråga ska därför inte få tas bort.
        //
        // Vi kontrollerar detta själva innan Delete så att admin får
        // ett tydligt felmeddelande istället för ett EF/SQL-fel.
        var isUsedInQuiz = await _context.Questions
            .AnyAsync(question => question.WordID == wordId);

        if (isUsedInQuiz)
        {
            throw new InvalidOperationException(
                $"Ordet '{word.WordDesc}' kan inte tas bort eftersom det används i en quizfråga.");
        }


        // -----------------------------------------
        // Ta bort ordet
        // -----------------------------------------

        // Relationerna WordExamples, WordTags och WordVotes är konfigurerade
        // med cascade delete, så deras kopplingar/rader tas bort tillsammans
        // med ordet.
        //
        // Själva Tag-raderna tas INTE bort.
        //
        // Vi tar inte heller bort Meaning automatiskt, eftersom flera Word
        // enligt datamodellen kan dela samma Meaning.
        _context.Words.Remove(word);

        await _context.SaveChangesAsync();

        return true;
    }
}