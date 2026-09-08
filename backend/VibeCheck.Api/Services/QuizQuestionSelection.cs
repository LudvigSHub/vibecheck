namespace VibeCheck.Api.Services;

public static class QuizQuestionSelection
{
    public const int QuestionLimit = 10;

    // Egen nivå används först, sedan reservnivåerna i den här ordningen.
    public static string[] LevelOrder(string level) => level switch
    {
        "Easy" => ["Easy", "Medium", "Hard"],
        "Medium" => ["Medium", "Easy", "Hard"],
        "Hard" => ["Hard", "Medium", "Easy"],
        _ => [level]
    };

    public static List<int> Select(string level, IEnumerable<(int Id, string Level)> candidates,
        Random? random = null)
    {
        // Använd den vanliga slumpgeneratorn om ingen skickats in, exempelvis från ett test.
        random ??= Random.Shared;
        var pool = candidates.DistinctBy(q => q.Id).ToList();
        var selected = new List<int>();
        foreach (var difficulty in LevelOrder(level))
        {
            var ids = pool.Where(q => q.Level == difficulty).Select(q => q.Id).ToArray();
            // ids är frågornas ID:n. Blanda dem och ta bara så många som fortfarande behövs.
            random.Shuffle(ids);
            selected.AddRange(ids.Take(QuestionLimit - selected.Count));
            if (selected.Count == QuestionLimit) break;
        }
        // Blanda även slutlistan så att inlånade frågor inte alltid hamnar sist.
        var result = selected.ToArray();
        random.Shuffle(result);
        return result.ToList();
    }
}
