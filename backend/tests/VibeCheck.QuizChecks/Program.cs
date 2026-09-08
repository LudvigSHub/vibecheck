using Microsoft.EntityFrameworkCore;
using VibeCheck.Api.DTOs;
using VibeCheck.Api.Services;
using VibeCheck.Data.Data;
using VibeCheck.Data.Models;

// dotnet run --project backend/tests/VibeCheck.QuizChecks
// Isolated in-memory database: never connects to the development database.
var passed = 0;
void Check(bool condition, string name)
{
    if (!condition) throw new Exception($"FAIL: {name}");
    Console.WriteLine($"PASS: {name}");
    passed++;
}
async Task Reject(Func<Task> action, string message, string name)
{
    try { await action(); }
    catch (InvalidOperationException ex) when (ex.Message.Contains(message))
    {
        Check(true, name);
        return;
    }
    throw new Exception($"FAIL: {name}");
}

var pool = Enumerable.Range(1, 25).Select(id => (Id: id,
    Level: id <= 10 ? "Easy" : id <= 20 ? "Medium" : "Hard")).ToArray();
List<int> Select(string level, IEnumerable<(int Id, string Level)>? rows = null, int seed = 1)
    => QuizQuestionSelection.Select(level, rows ?? pool, new Random(seed));
Check(Select("Hard").Count == 10 && Select("Hard").Count(id => id > 20) == 5
    && Select("Hard").All(id => id > 10), "Hard uses all five Hard questions and five Medium");
Check(Select("Easy").Count == 10 && Select("Easy").All(id => id <= 10), "Full own level needs no fallback");
Check(Select("Hard", pool.Where(q => q.Level != "Hard")).All(id => id is > 10 and <= 20),
    "Empty own level falls back to Medium");
Check(Select("Medium", pool.Where(q => q.Level != "Medium")).All(id => id <= 10),
    "Medium prefers Easy when both neighboring levels exist");
Check(Select("Easy", pool.Where(q => q.Id > 20)).Count == 5, "Small pool returns fewer questions");
Check(Select("Hard", []).Count == 0, "Empty pool returns no questions");
Check(Select("Hard", pool.Concat(pool)).Distinct().Count() == 10, "Duplicate candidates cannot duplicate selected questions");
Check(Enumerable.Range(1, 12).Select(seed => string.Join(',', Select("Hard", seed: seed))).Distinct().Count() > 1,
    "Different random seeds produce different rounds");

await using var db = new VibeCheckDbContext(new DbContextOptionsBuilder<VibeCheckDbContext>()
    .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
var easy = new Difficulty { DifficultyID = 1, DifficultyDesc = "Easy" };
var medium = new Difficulty { DifficultyID = 2, DifficultyDesc = "Medium" };
var hard = new Difficulty { DifficultyID = 3, DifficultyDesc = "Hard" };
var levels = new[] { easy, medium, hard };
db.Difficulties.AddRange(levels);
db.Users.Add(new User { Id = 1, UserName = "quiz-test" });
var type = new QuestionType { QuestionTypeID = 1, TypeText = "test", Description = "Choose" };
var word = new Word { WordID = 1, WordDesc = "test", Meaning = new Meaning { MeaningID = 1, MeaningText = "explanation" } };
foreach (var item in pool)
{
    var question = new Question
    {
        QuestionID = item.Id, Difficulty = levels.Single(d => d.DifficultyDesc == item.Level),
        QuestionType = type, Word = word, QuestionDesc = $"Question {item.Id}",
        CorrectAlternativeID = item.Id * 10
    };
    question.QuestionAlternatives.Add(new QuestionAlternative { AlternativeID = item.Id * 10, AlternativeText = "Correct" });
    question.QuestionAlternatives.Add(new QuestionAlternative { AlternativeID = item.Id * 10 + 1, AlternativeText = "Wrong" });
    db.Questions.Add(question);
}
db.Quizzes.AddRange(levels.Select(d => new Quiz { QuizID = d.DifficultyID, Difficulty = d, QuizName = d.DifficultyDesc }));
// Unlock Hard using completed history. No QuizQuestions are needed for new rounds.
db.QuizAttempts.AddRange(new QuizAttempt { UserID = 1, QuizID = 1, Score = 100, CompletedAt = DateTime.UtcNow },
    new QuizAttempt { UserID = 1, QuizID = 2, Score = 100, CompletedAt = DateTime.UtcNow });
await db.SaveChangesAsync();
db.ChangeTracker.Clear();
var service = new QuizService(db);
var round = (await service.StartAttemptAsync(1, 3))!;
var ids = round.Questions.Select(q => q.QuestionId).ToArray();
Check(ids.Length == 10 && ids.Distinct().Count() == 10 && ids.Count(id => id > 20) == 5,
    "Service starts a mixed ten-question Hard attempt without fixed quiz links");
Check((await db.QuizAttemptQuestions.Where(q => q.QuizAttemptID == round.QuizAttemptId)
    .OrderBy(q => q.Order).Select(q => q.QuestionID).ToArrayAsync()).SequenceEqual(ids),
    "Returned order matches persisted attempt questions");
Check((await service.GetQuizListAsync(1)).All(q => q.QuestionCount == 10), "Quiz list counts playable rounds");
var outsideId = pool.First(q => !ids.Contains(q.Id)).Id;
await Reject(() => service.SubmitAnswerAsync(1, round.QuizAttemptId,
    new SubmitAnswerRequestDTO { QuestionId = outsideId, AlternativeId = outsideId * 10 }),
    "ingår inte", "Question outside the attempt is rejected");
var borrowed = ids.First(id => id <= 20);
await Reject(() => service.SubmitAnswerAsync(1, round.QuizAttemptId,
    new SubmitAnswerRequestDTO { QuestionId = borrowed, AlternativeId = outsideId * 10 }),
    "hör inte", "Alternative belonging to another question is rejected");
var answer = await service.SubmitAnswerAsync(1, round.QuizAttemptId,
    new SubmitAnswerRequestDTO { QuestionId = borrowed, AlternativeId = borrowed * 10 });
Check(answer is { IsCorrect: true, TotalCount: 10, AnsweredCount: 1 }, "Borrowed question is graded by backend");
await Reject(() => service.SubmitAnswerAsync(1, round.QuizAttemptId,
    new SubmitAnswerRequestDTO { QuestionId = borrowed, AlternativeId = borrowed * 10 }),
    "redan besvarad", "Duplicate answer is rejected");
Check(await service.SubmitAnswerAsync(2, round.QuizAttemptId,
    new SubmitAnswerRequestDTO { QuestionId = ids[0], AlternativeId = ids[0] * 10 }) is null,
    "Another user cannot answer the attempt");
Check(await new QuizProgressService(db).GetLatestActiveQuizAsync(1)
    is { AnsweredQuestionCount: 1, TotalQuestionCount: 10 }, "Progress uses selected questions");
db.ChangeTracker.Clear();
var result = await service.CompleteAttemptAsync(1, round.QuizAttemptId);
Check(result is { TotalCount: 10, CorrectCount: 1, Score: 10 }, "Partial completion uses all selected questions as denominator");
var next = (await service.StartAttemptAsync(1, 3))!;
foreach (var q in next.Questions)
    await service.SubmitAnswerAsync(1, next.QuizAttemptId,
        new SubmitAnswerRequestDTO { QuestionId = q.QuestionId, AlternativeId = q.QuestionId * 10 });
Check(await service.CompleteAttemptAsync(1, next.QuizAttemptId) is { Score: 100, Passed: true }, "Full round completes correctly");
var abandoned = (await service.StartAttemptAsync(1, 3))!;
await service.AbandonAttemptAsync(1, abandoned.QuizAttemptId);
Check(!await db.QuizAttemptQuestions.AnyAsync(q => q.QuizAttemptID == abandoned.QuizAttemptId),
    "Abandoning removes selected questions");

await using var smallDb = new VibeCheckDbContext(new DbContextOptionsBuilder<VibeCheckDbContext>()
    .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
smallDb.Quizzes.Add(new Quiz { QuizID = 1, QuizName = "Small Hard",
    Difficulty = new Difficulty { DifficultyID = 77, DifficultyDesc = "Hard" } });
await smallDb.SaveChangesAsync();
var smallService = new QuizService(smallDb);
await Reject(() => smallService.StartAttemptAsync(1, 1), "inga frågor", "Empty database prevents starting a round");
Check(!await smallDb.QuizAttempts.AnyAsync(), "Failed empty start leaves no attempt behind");
var fallbackLevel = new Difficulty { DifficultyID = 99, DifficultyDesc = "Medium" };
for (var id = 1; id <= 3; id++)
{
    smallDb.Questions.Add(new Question { QuestionID = id, Difficulty = fallbackLevel,
        QuestionType = new QuestionType { TypeText = $"type{id}", Description = "Choose" },
        Word = new Word { WordDesc = $"word{id}", Meaning = new Meaning { MeaningText = $"meaning{id}" } },
        CorrectAlternativeID = id * 10,
        QuestionAlternatives = [new QuestionAlternative { AlternativeID = id * 10, AlternativeText = "Correct" }] });
}
await smallDb.SaveChangesAsync();
var smallRound = (await smallService.StartAttemptAsync(1, 1))!;
Check(smallRound.Questions.Count == 3 && (await smallService.GetQuizListAsync(1))[0].QuestionCount == 3,
    "Empty Hard level borrows three available Medium questions regardless of database IDs");
var smallQuestion = smallRound.Questions[0];
await smallService.SubmitAnswerAsync(1, smallRound.QuizAttemptId,
    new SubmitAnswerRequestDTO { QuestionId = smallQuestion.QuestionId, AlternativeId = smallQuestion.QuestionId * 10 });
Check(await smallService.CompleteAttemptAsync(1, smallRound.QuizAttemptId) is { TotalCount: 3, Score: 33 },
    "Small round uses actual count for scoring");
var replaced = (await smallService.StartAttemptAsync(1, 1))!;
await smallService.StartAttemptAsync(1, 1);
Check(!await smallDb.QuizAttempts.AnyAsync(a => a.QuizAttemptID == replaced.QuizAttemptId)
    && !await smallDb.QuizAttemptQuestions.AnyAsync(q => q.QuizAttemptID == replaced.QuizAttemptId),
    "Restarting cleans up the previous active attempt and its questions");

Console.WriteLine($"All {passed} quiz checks passed.");
