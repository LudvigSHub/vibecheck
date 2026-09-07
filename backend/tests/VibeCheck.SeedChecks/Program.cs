using System.Text.Json;
using VibeCheck.Data.Models;
using VibeCheck.Data.Seed;

// Run with dotnet run --project backend/tests/VibeCheck.SeedChecks.
// These checks use the real seed files but never connect to a database.
var words = JsonSerializer.Deserialize<List<WordSeed>>(
    File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Seed", "words.json")))!
    .Select((w, i) => new Word { WordID = 1000 + i, WordDesc = w.WordDesc }).ToList();
var seeds = JsonSerializer.Deserialize<List<WordInflectionSeed?>>(
    File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Seed", "wordinflections.json")))!;
var types = new[]
{
    "verb_present", "verb_past", "verb_supinum", "verb_future",
    "noun_singular_definite", "noun_plural_indefinite", "noun_plural_definite",
    "adjective_neuter", "adjective_plural", "adjective_definite"
}.Select((code, i) => new InflectionType { InflectionTypeID = 2000 + i, Code = code }).ToList();

List<WordInflection> Plan(IEnumerable<WordInflectionSeed?> input, IEnumerable<WordInflection>? existing = null)
    => WordInflectionSeeder.CreateMissingInflections(input, words, types, existing ?? []);

var passed = 0;
void Check(bool condition, string name)
{
    if (!condition)
        throw new Exception($"FAIL: {name}");
    Console.WriteLine($"PASS: {name}");
    passed++;
}

void Reject(IEnumerable<WordInflectionSeed?> input, string expected, string name)
{
    try
    {
        Plan(input);
    }
    catch (InvalidOperationException ex)
    {
        Check(ex.Message.Contains(expected), name);
        return;
    }
    throw new Exception($"FAIL: {name} (input was accepted)");
}

WordInflectionSeed Row(string word = "clown", string type = "noun_plural_indefinite", string text = "clowner")
    => new() { Word = word, InflectionTypeCode = type, InflectedText = text };

var initial = Plan(seeds);
Check(initial.Count == seeds.Count && initial.Count > 0, "Real seed file resolves all word and type references");
Check(initial.All(i => i.WordID >= 1000 && i.InflectionTypeID >= 2000), "Uses resolved IDs, not hardcoded IDs");
Check(Plan(seeds, initial).Count == 0, "Second run adds no duplicates");

var partial = initial.Take(5).ToList();
var missing = Plan(seeds, partial);
Check(missing.Count == initial.Count - partial.Count, "Partially seeded database receives only missing forms");
Check(Plan(seeds, partial.Concat(missing)).Count == 0, "Completed partial database is stable on another run");

var custom = new WordInflection { WordID = words[0].WordID, InflectionTypeID = types[0].InflectionTypeID, InflectedText = "manuell form" };
Check(Plan(seeds, initial.Append(custom)).Count == 0 && custom.InflectedText == "manuell form", "Preserves manually added data");
Check(Plan([Row(text: "CLOWNER")], initial).Count == 0, "Case changes do not duplicate an existing form");
Check(Plan([Row(word: "CLOWN", type: "NOUN_PLURAL_INDEFINITE")], initial).Count == 0, "Reference lookup ignores letter case");
Check(Plan([
    Row("keff", "adjective_plural", "keffa"),
    Row("keff", "adjective_definite", "keffa")]).Count == 2, "Same text can have two grammatical types");
Check(Plan([
    Row("NPC", "noun_singular_definite", "NPC:n"),
    Row("NPC", "noun_singular_definite", "NPC:en")]).Count == 2, "Explicit spelling variants can share a type");

Reject([Row(), Row(text: "CLOWNER")], "duplicate", "Rejects duplicate rows in seed file");
Reject([Row(), Row(word: "clowm")], "clowm", "Rejects unknown word before returning any additions");
Reject([Row(type: "verb_psat")], "verb_psat", "Rejects misspelled type code");
Reject([Row(text: " ")], "must not be empty", "Rejects empty inflection text");
Reject([Row(word: "")], "must not be empty", "Rejects empty word reference");
Reject([Row(type: "")], "must not be empty", "Rejects empty type reference");
Reject([Row(text: "clowner ")], "whitespace", "Rejects accidental surrounding whitespace");
Reject([null], "Row 1 is null", "Rejects null rows with a useful error");
Check(Plan([]).Count == 0, "Empty seed list adds nothing");

Console.WriteLine($"{passed} checks passed; {initial.Count} forms for {initial.Select(i => i.WordID).Distinct().Count()} of {words.Count} words.");
