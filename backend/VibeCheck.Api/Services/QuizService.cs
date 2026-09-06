using Microsoft.EntityFrameworkCore;
using VibeCheck.Api.DTOs;
using VibeCheck.Data.Data;
using VibeCheck.Data.Models;

namespace VibeCheck.Api.Services;

public class QuizService
{
    private readonly VibeCheckDbContext _context;

    // Andelen rätt (%) som krävs för att låsa upp nästa nivå.
    // Regeln bor här, inte i React – frontend får värdet
    // serverat via QuizListItemDTO.RequiredScore.
    public const int PassScore = 80;

    public QuizService(VibeCheckDbContext context)
    {
        _context = context;
    }

    // ============================================================
    // LISTA
    // ============================================================

    public async Task<List<QuizListItemDTO>> GetQuizListAsync(int userId)
    {
        // Quizen i svårighetsordning. DifficultyID är 1/2/3 för Easy/Medium/Hard
        // eftersom seedningen lägger in dem i den ordningen – ordningen är alltså
        // korrekt men implicit. Vill man göra den explicit är en SortOrder-kolumn
        // på Difficulty rätt lösning, men det kräver en migration.
        var quizzes = await _context.Quizzes
            .AsNoTracking()
            .OrderBy(q => q.DifficultyID)
            .Select(q => new
            {
                q.QuizID,
                q.QuizName,
                q.QuizDescription,
                Difficulty = q.Difficulty.DifficultyDesc,
                QuestionCount = q.QuizQuestions.Count
            })
            .ToListAsync();

        var bestScores = await GetBestScoresAsync(userId);

        var list = new List<QuizListItemDTO>();

        // Upplåsningskedjan. Första quizet är alltid öppet; resten kräver att
        // FÖREGÅENDE quiz klarats med minst PassScore procent.
        string? previousQuizName = null;
        var previousPassed = true;

        foreach (var quiz in quizzes)
        {
            bestScores.TryGetValue(quiz.QuizID, out var bestScore);

            list.Add(new QuizListItemDTO
            {
                QuizId = quiz.QuizID,
                QuizName = quiz.QuizName,
                QuizDescription = quiz.QuizDescription,
                Difficulty = quiz.Difficulty,
                QuestionCount = quiz.QuestionCount,
                BestScore = bestScore,
                IsUnlocked = previousPassed,
                UnlockedBy = previousQuizName,
                RequiredScore = PassScore
            });

            // bestScore är int?. Har quizet aldrig klarats är den null, och
            // null >= 80 är false i C# – precis vad vi vill. Se noteringen nedan.
            previousPassed = bestScore >= PassScore;
            previousQuizName = quiz.QuizName;
        }

        return list;
    }

    // Högsta resultatet per quiz, bara från SLUTFÖRDA försök.
    // Ett avbrutet försök har CompletedAt == null och ska aldrig räknas.
    private async Task<Dictionary<int, int?>> GetBestScoresAsync(int userId)
    {
        return await _context.QuizAttempts
            .AsNoTracking()
            .Where(a => a.UserID == userId && a.CompletedAt != null)
            .GroupBy(a => a.QuizID)
            .Select(g => new
            {
                QuizId = g.Key,
                BestScore = g.Max(a => a.Score)
            })
            .ToDictionaryAsync(x => x.QuizId, x => x.BestScore);
    }

    // ============================================================
    // STARTA
    // ============================================================

    public async Task<StartQuizAttemptDTO?> StartAttemptAsync(int userId, int quizId)
    {
        var quiz = await _context.Quizzes
            .AsNoTracking()
            .FirstOrDefaultAsync(q => q.QuizID == quizId);

        if (quiz is null)
        {
            return null;
        }

        // Upplåsningen kontrolleras här och inte bara i frontend. En låst knapp
        // i React hindrar ingen från att skicka samma POST i Postman.
        // Vi återanvänder listan i stället för att skriva regeln en gång till.
        var quizList = await GetQuizListAsync(userId);
        var listItem = quizList.First(q => q.QuizId == quizId);

        if (!listItem.IsUnlocked)
        {
            throw new InvalidOperationException(
                $"Quizet '{quiz.QuizName}' är låst. " +
                $"Klara '{listItem.UnlockedBy}' med minst {PassScore}% först.");
        }

        // Städa bort påbörjade försök på samma quiz. De uppstår när någon stänger
        // fliken i stället för att svara på avbryt-rutan, och ett halvfärdigt
        // försök ska aldrig ligga kvar. Svaren följer med tack vare cascade.
        var abandoned = await _context.QuizAttempts
            .Where(a =>
                a.UserID == userId &&
                a.QuizID == quizId &&
                a.CompletedAt == null)
            .ToListAsync();

        _context.QuizAttempts.RemoveRange(abandoned);

        var attempt = new QuizAttempt
        {
            UserID = userId,
            QuizID = quizId,
            AttemptDate = DateTime.UtcNow
        };

        _context.QuizAttempts.Add(attempt);

        await _context.SaveChangesAsync();

        // Frågorna – utan facit. Varken CorrectAlternativeID eller ordets
        // betydelse följer med hit; de går ut först när frågan är besvarad.
        var questions = await _context.QuizQuestions
            .AsNoTracking()
            .Where(qq => qq.QuizID == quizId)
            .OrderBy(qq => qq.QuizQuestionID)
            .Select(qq => new QuizQuestionDTO
            {
                QuestionId = qq.Question.QuestionID,
                Prompt = qq.Question.QuestionType.Description,
                Body = qq.Question.QuestionDesc,
                QuestionType = qq.Question.QuestionType.TypeText,
                Alternatives = qq.Question.QuestionAlternatives
                    .OrderBy(a => a.AlternativeID)
                    .Select(a => new QuizAlternativeDTO
                    {
                        AlternativeId = a.AlternativeID,
                        AlternativeText = a.AlternativeText
                    })
                    .ToList()
            })
            .ToListAsync();

        return new StartQuizAttemptDTO
        {
            QuizAttemptId = attempt.QuizAttemptID,
            QuizId = quiz.QuizID,
            QuizName = quiz.QuizName,
            Questions = questions
        };
    }

    // ============================================================
    // SVARA
    // ============================================================

    public async Task<AnswerResultDTO?> SubmitAnswerAsync(
        int userId,
        int attemptId,
        SubmitAnswerRequestDTO request)
    {
        var attempt = await _context.QuizAttempts
            .FirstOrDefaultAsync(a =>
                a.QuizAttemptID == attemptId &&
                a.UserID == userId);

        // Både "finns inte" och "tillhör någon annan" ger null. Att skilja på
        // dem i svaret vore att berätta för en angripare vilka ID:n som finns.
        if (attempt is null)
        {
            return null;
        }

        if (attempt.CompletedAt != null)
        {
            throw new InvalidOperationException("Quizet är redan avslutat.");
        }

        // Frågan måste ingå i just det här quizet. Utan kontrollen kan man
        // svara på vilken fråga som helst i databasen och plocka poäng.
        var belongsToQuiz = await _context.QuizQuestions
            .AnyAsync(qq =>
                qq.QuizID == attempt.QuizID &&
                qq.QuestionID == request.QuestionId);

        if (!belongsToQuiz)
        {
            throw new InvalidOperationException(
                $"Fråga {request.QuestionId} ingår inte i det här quizet.");
        }

        var alreadyAnswered = await _context.QuizAttemptAnswers
            .AnyAsync(a =>
                a.QuizAttemptID == attemptId &&
                a.QuestionID == request.QuestionId);

        if (alreadyAnswered)
        {
            throw new InvalidOperationException("Frågan är redan besvarad.");
        }

        var question = await _context.Questions
            .AsNoTracking()
            .Include(q => q.Word)
                .ThenInclude(w => w.Meaning)
            .Include(q => q.QuestionAlternatives)
            .FirstAsync(q => q.QuestionID == request.QuestionId);

        var selected = question.QuestionAlternatives
            .FirstOrDefault(a => a.AlternativeID == request.AlternativeId);

        if (selected is null)
        {
            throw new InvalidOperationException(
                $"Alternativ {request.AlternativeId} hör inte till fråga {request.QuestionId}.");
        }

        var isCorrect = selected.AlternativeID == question.CorrectAlternativeID;

        _context.QuizAttemptAnswers.Add(new QuizAttemptAnswer
        {
            QuizAttemptID = attemptId,
            QuestionID = question.QuestionID,
            SelectedAlternativeID = selected.AlternativeID,
            IsCorrect = isCorrect
        });

        await _context.SaveChangesAsync();

        var correct = question.QuestionAlternatives
            .First(a => a.AlternativeID == question.CorrectAlternativeID);

        var answeredCount = await _context.QuizAttemptAnswers
            .CountAsync(a => a.QuizAttemptID == attemptId);

        var totalCount = await _context.QuizQuestions
            .CountAsync(qq => qq.QuizID == attempt.QuizID);

        return new AnswerResultDTO
        {
            IsCorrect = isCorrect,
            CorrectAlternativeId = correct.AlternativeID,
            CorrectAlternativeText = correct.AlternativeText,
            Explanation = question.Word.Meaning.MeaningText,
            AnsweredCount = answeredCount,
            TotalCount = totalCount
        };
    }

    // ============================================================
    // AVSLUTA
    // ============================================================

    public async Task<QuizResultDTO?> CompleteAttemptAsync(int userId, int attemptId)
    {
        var attempt = await _context.QuizAttempts
            .Include(a => a.Quiz)
            .FirstOrDefaultAsync(a =>
                a.QuizAttemptID == attemptId &&
                a.UserID == userId);

        if (attempt is null)
        {
            return null;
        }

        if (attempt.CompletedAt != null)
        {
            throw new InvalidOperationException("Quizet är redan avslutat.");
        }

        var totalCount = await _context.QuizQuestions
            .CountAsync(qq => qq.QuizID == attempt.QuizID);

        var correctCount = await _context.QuizAttemptAnswers
            .CountAsync(a => a.QuizAttemptID == attemptId && a.IsCorrect);

        // Nämnaren är antalet frågor i quizet, inte antalet besvarade.
        // Annars skulle tre rätt av tre besvarade ge 100% på ett tiofrågorsquiz.
        var score = totalCount == 0
            ? 0
            : (int)Math.Round(correctCount * 100.0 / totalCount);

        // Hämtas FÖRE vi sparar. Gör vi det efteråt räknas det här försöket
        // in i sitt eget "tidigare bästa" och IsNewBest blir alltid false.
        var bestScores = await GetBestScoresAsync(userId);
        bestScores.TryGetValue(attempt.QuizID, out var previousBest);

        attempt.Score = score;
        attempt.QuizPassed = score >= PassScore;
        attempt.CompletedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // Låstes nästa nivå upp just nu? Bara om resultatet räckte OCH det
        // tidigare bästa inte redan gjorde det – annars vore den redan öppen.
        string? unlockedQuizName = null;

        if (score >= PassScore && !(previousBest >= PassScore))
        {
            unlockedQuizName = await _context.Quizzes
                .AsNoTracking()
                .Where(q => q.DifficultyID > attempt.Quiz.DifficultyID)
                .OrderBy(q => q.DifficultyID)
                .Select(q => q.QuizName)
                .FirstOrDefaultAsync();
        }

        return new QuizResultDTO
        {
            QuizAttemptId = attempt.QuizAttemptID,
            QuizId = attempt.QuizID,
            QuizName = attempt.Quiz.QuizName,
            CorrectCount = correctCount,
            TotalCount = totalCount,
            Score = score,
            Passed = attempt.QuizPassed ?? false,
            PreviousBestScore = previousBest,
            IsNewBest = previousBest is null || score > previousBest,
            UnlockedQuizName = unlockedQuizName
        };
    }

    // ============================================================
    // AVBRYTA
    // ============================================================

    public async Task<bool> AbandonAttemptAsync(int userId, int attemptId)
    {
        var attempt = await _context.QuizAttempts
            .FirstOrDefaultAsync(a =>
                a.QuizAttemptID == attemptId &&
                a.UserID == userId);

        if (attempt is null)
        {
            return false;
        }

        if (attempt.CompletedAt != null)
        {
            throw new InvalidOperationException(
                "Ett avslutat quiz kan inte tas bort.");
        }

        // Svaren försvinner med försöket. QuizAttemptAnswer -> QuizAttempt är
        // konfigurerad med OnDelete(DeleteBehavior.Cascade) i DbContext, så
        // databasen städar barnen åt oss.
        _context.QuizAttempts.Remove(attempt);

        await _context.SaveChangesAsync();

        return true;
    }
}