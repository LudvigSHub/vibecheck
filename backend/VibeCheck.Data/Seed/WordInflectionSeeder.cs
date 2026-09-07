using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using VibeCheck.Data.Data;
using VibeCheck.Data.Models;

namespace VibeCheck.Data.Seed;

public static class WordInflectionSeeder
{
    public static async Task SeedAsync(VibeCheckDbContext context)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Seed", "wordinflections.json");
        var json = await File.ReadAllTextAsync(path);
        var seeds = JsonSerializer.Deserialize<List<WordInflectionSeed?>>(json,
            new JsonSerializerOptions
            {
                // A misspelled property must not silently disappear during deserialization.
                UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow
            }) ?? throw new InvalidOperationException("wordinflections.json must contain an array.");

        var words = await context.Words.AsNoTracking().ToListAsync();
        var types = await context.InflectionTypes.AsNoTracking().ToListAsync();
        var existing = await context.WordInflections.AsNoTracking().ToListAsync();

        // Validate the whole file before tracking or saving any new inflections.
        var missing = CreateMissingInflections(seeds, words, types, existing);
        if (missing.Count == 0)
            return;

        await context.WordInflections.AddRangeAsync(missing);
        await context.SaveChangesAsync();
    }

    // Kept independent of the database so validation and repeated runs can be checked safely.
    public static List<WordInflection> CreateMissingInflections(
        IEnumerable<WordInflectionSeed?> seeds,
        IEnumerable<Word> words,
        IEnumerable<InflectionType> types,
        IEnumerable<WordInflection> existing)
    {
        var wordsByText = words.ToDictionary(w => w.WordDesc, StringComparer.OrdinalIgnoreCase);
        var typesByCode = types.ToDictionary(t => t.Code, StringComparer.OrdinalIgnoreCase);
        var existingKeys = existing
            .Select(i => (i.WordID, i.InflectionTypeID, Text: i.InflectedText.ToUpperInvariant()))
            .ToHashSet();
        var seedKeys = new HashSet<(int WordID, int InflectionTypeID, string Text)>();
        var missing = new List<WordInflection>();
        var errors = new List<string>();
        var row = 0;

        foreach (var seed in seeds)
        {
            row++;
            if (seed == null)
            {
                errors.Add($"Row {row} is null.");
                continue;
            }

            var label = $"Row {row} (word '{seed.Word}', type '{seed.InflectionTypeCode}')";
            if (string.IsNullOrWhiteSpace(seed.Word)
                || string.IsNullOrWhiteSpace(seed.InflectionTypeCode)
                || string.IsNullOrWhiteSpace(seed.InflectedText))
            {
                errors.Add($"{label}: Word, InflectionTypeCode and InflectedText must not be empty.");
                continue;
            }

            if (seed.Word != seed.Word.Trim()
                || seed.InflectionTypeCode != seed.InflectionTypeCode.Trim()
                || seed.InflectedText != seed.InflectedText.Trim())
            {
                errors.Add($"{label}: remove leading or trailing whitespace.");
                continue;
            }

            var wordFound = wordsByText.TryGetValue(seed.Word, out var word);
            var typeFound = typesByCode.TryGetValue(seed.InflectionTypeCode, out var type);
            if (!wordFound)
                errors.Add($"{label}: word does not exist in the database. Seed the word first.");
            if (!typeFound)
                errors.Add($"{label}: unknown inflection type code.");
            if (!wordFound || !typeFound)
                continue;

            var key = (word!.WordID, type!.InflectionTypeID, seed.InflectedText.ToUpperInvariant());
            if (!seedKeys.Add(key))
            {
                errors.Add($"{label}: duplicate inflection '{seed.InflectedText}' in the seed file.");
                continue;
            }

            // Same text may belong to different types (e.g. keffa in plural and definite).
            // Different texts for one type are allowed as explicit spelling variants.
            if (existingKeys.Contains(key))
                continue;

            missing.Add(new WordInflection
            {
                WordID = word.WordID,
                InflectionTypeID = type.InflectionTypeID,
                InflectedText = seed.InflectedText
            });
        }

        if (errors.Count > 0)
            throw new InvalidOperationException(
                "wordinflections.json validation failed:\n" + string.Join("\n", errors));

        return missing;
    }
}
