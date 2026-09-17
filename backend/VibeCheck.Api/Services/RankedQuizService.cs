using Microsoft.EntityFrameworkCore;
using VibeCheck.Api.DTOs;
using VibeCheck.Data.Data;
using VibeCheck.Data.Models;

namespace VibeCheck.Api.Services;

public class RankedQuizService
{
    private readonly VibeCheckDbContext _context;

    // Reglerna bor här och skickas ut till frontend via DTO:erna, så att
    // de bara är definierade på ett ställe.
    public const int RoundSeconds = 60;
    public const int MaxWrongAnswers = 3;
    public const int LeaderboardSize = 100;

    // Marginal för nätverkslatens och små klockskillnader. Utan den kan ett
    // svar som skickades i tid avvisas för att det kom fram en aning sent.
    private const int GraceSeconds = 2;

    public RankedQuizService(VibeCheckDbContext context)
    {
        _context = context;
    }

    // ============================================================
    // STARTA
    // ============================================================

    public async Task<StartRankedAttemptDTO> StartAttemptAsync(int userId)
    {
        var now = DateTime.UtcNow;

        // Städa bort påbörjade omgångar. De uppstår när någon laddar om
        // sidan mitt i. En oavslutad omgång har aldrig skrivit till
        // topplistan, så den kan tas bort utan följdverkningar.
        var unfinished = await _context.RankedAttempts
            .Where(a => a.UserID == userId && a.CompletedAt == null)
            .ToListAsync();

        _context.RankedAttempts.RemoveRange(unfinished);

        var attempt = new RankedAttempt
        {
            UserID = userId,
            StartedAt = now,
            EndsAt = now.AddSeconds(RoundSeconds)
        };

        _context.RankedAttempts.Add(attempt);

        await _context.SaveChangesAsync();

        // Alla frågor, oavsett svårighetsgrad. Ingen AsSplitQuery här:
        // projektionen har bara EN kollektion (alternativen), och då blir
        // det ingen kartesisk produkt att dela upp.
        var questions = await _context.Questions
            .AsNoTracking()
            .Select(q => new QuizQuestionDTO
            {
                QuestionId = q.QuestionID,
                Prompt = q.QuestionType.Description,
                Body = q.QuestionDesc,
                QuestionType = q.QuestionType.TypeText,
                Alternatives = q.QuestionAlternatives
                    .OrderBy(a => a.AlternativeID)
                    .Select(a => new QuizAlternativeDTO
                    {
                        AlternativeId = a.AlternativeID,
                        AlternativeText = a.AlternativeText
                    })
                    .ToList()
            })
            .ToListAsync();

        // Frågeordningen blandas, men inte alternativen inom en fråga –
        // "Sant" ska stå före "Falskt" varje gång.
        var shuffled = questions.ToArray();
        Random.Shared.Shuffle(shuffled);

        return new StartRankedAttemptDTO
        {
            RankedAttemptId = attempt.RankedAttemptID,
            SecondsRemaining = RoundSeconds,
            EndsAt = attempt.EndsAt,
            MaxWrongAnswers = MaxWrongAnswers,
            Questions = shuffled.ToList()
        };
    }

    // ============================================================
    // SVARA
    // ============================================================

    public async Task<RankedAnswerResultDTO?> SubmitAnswerAsync(
        int userId,
        int attemptId,
        SubmitAnswerRequestDTO request)
    {
        var attempt = await _context.RankedAttempts
            .FirstOrDefaultAsync(a =>
                a.RankedAttemptID == attemptId &&
                a.UserID == userId);

        // "Finns inte" och "tillhör någon annan" ger samma svar med flit.
        if (attempt is null)
        {
            return null;
        }

        if (attempt.CompletedAt != null)
        {
            throw new InvalidOperationException("Omgången är redan avslutad.");
        }

        var now = DateTime.UtcNow;

        // Tiden kontrolleras här, mot serverns klocka och mot EndsAt som
        // sattes vid start. Klientens nedräkning är bara visning.
        if (now > attempt.EndsAt.AddSeconds(GraceSeconds))
        {
            var (correct, wrong) = await CountAnswersAsync(attemptId);

            return new RankedAnswerResultDTO
            {
                IsCorrect = false,
                CorrectCount = correct,
                WrongCount = wrong,
                RoundOver = true,
                EndReason = "TimeUp"
            };
        }

        var question = await _context.Questions
            .AsNoTracking()
            .Include(q => q.QuestionAlternatives)
            .FirstOrDefaultAsync(q => q.QuestionID == request.QuestionId);

        if (question is null)
        {
            throw new InvalidOperationException(
                $"Fråga {request.QuestionId} finns inte.");
        }

        var alreadyAnswered = await _context.RankedAttemptAnswers
            .AnyAsync(a =>
                a.RankedAttemptID == attemptId &&
                a.QuestionID == request.QuestionId);

        if (alreadyAnswered)
        {
            throw new InvalidOperationException("Frågan är redan besvarad.");
        }

        var selected = question.QuestionAlternatives
            .FirstOrDefault(a => a.AlternativeID == request.AlternativeId);

        if (selected is null)
        {
            throw new InvalidOperationException(
                $"Alternativ {request.AlternativeId} hör inte till fråga {request.QuestionId}.");
        }

        var isCorrect = selected.AlternativeID == question.CorrectAlternativeID;

        _context.RankedAttemptAnswers.Add(new RankedAttemptAnswer
        {
            RankedAttemptID = attemptId,
            QuestionID = question.QuestionID,
            SelectedAlternativeID = selected.AlternativeID,
            IsCorrect = isCorrect,
            AnsweredAt = now
        });

        await _context.SaveChangesAsync();

        var (correctCount, wrongCount) = await CountAnswersAsync(attemptId);

        var totalQuestions = await _context.Questions.CountAsync();

        string? endReason = null;

        if (wrongCount >= MaxWrongAnswers)
        {
            endReason = "ThreeWrong";
        }
        else if (correctCount + wrongCount >= totalQuestions)
        {
            endReason = "OutOfQuestions";
        }

        var correctAlternative = question.QuestionAlternatives
            .First(a => a.AlternativeID == question.CorrectAlternativeID);

        return new RankedAnswerResultDTO
        {
            IsCorrect = isCorrect,
            CorrectAlternativeId = correctAlternative.AlternativeID,
            CorrectAlternativeText = correctAlternative.AlternativeText,
            CorrectCount = correctCount,
            WrongCount = wrongCount,
            RoundOver = endReason != null,
            EndReason = endReason
        };
    }

    private async Task<(int Correct, int Wrong)> CountAnswersAsync(int attemptId)
    {
        var correct = await _context.RankedAttemptAnswers
            .CountAsync(a => a.RankedAttemptID == attemptId && a.IsCorrect);

        var wrong = await _context.RankedAttemptAnswers
            .CountAsync(a => a.RankedAttemptID == attemptId && !a.IsCorrect);

        return (correct, wrong);
    }

    // ============================================================
    // AVSLUTA
    // ============================================================

    public async Task<RankedResultDTO?> CompleteAttemptAsync(
        int userId,
        int attemptId)
    {
        var attempt = await _context.RankedAttempts
            .FirstOrDefaultAsync(a =>
                a.RankedAttemptID == attemptId &&
                a.UserID == userId);

        if (attempt is null)
        {
            return null;
        }

        var (correctCount, wrongCount) = await CountAnswersAsync(attemptId);

        // Redan avslutad: returnera det sparade resultatet i stället för att
        // klaga. Ett omsänt anrop efter en tappad uppkoppling ska inte se ut
        // som ett fel, och poängen kan ändå inte ändras.
        if (attempt.CompletedAt != null)
        {
            return await BuildResultAsync(attempt, correctCount, wrongCount,
                previousBestScore: null, isNewPersonalBest: false);
        }

        var now = DateTime.UtcNow;
        var totalQuestions = await _context.Questions.CountAsync();

        // Vilken av de tre avslutsorsakerna gäller? Marginalen åt andra
        // hållet här: klientens nedräkning får slå till strax före serverns,
        // annars nekas ett helt legitimt avslut.
        string endReason;

        if (wrongCount >= MaxWrongAnswers)
        {
            endReason = "ThreeWrong";
        }
        else if (correctCount + wrongCount >= totalQuestions)
        {
            endReason = "OutOfQuestions";
        }
        else if (now >= attempt.EndsAt.AddSeconds(-GraceSeconds))
        {
            endReason = "TimeUp";
        }
        else
        {
            throw new InvalidOperationException("Omgången pågår fortfarande.");
        }

        attempt.CompletedAt = now;
        attempt.Score = correctCount;
        attempt.WrongCount = wrongCount;
        attempt.EndReason = endReason;

        // Veckan bestäms av när omgången avslutades, inte när den startades.
        // En omgång som börjar 23:59:30 på söndagen tillhör alltså nästa
        // vecka – marginellt, men regeln måste vara entydig.
        var week = CompetitionWeek.StartOf(now);

        var entry = await _context.LeaderboardEntries
            .FirstOrDefaultAsync(e =>
                e.UserID == userId &&
                e.WeekStart == week);

        int? previousBestScore = entry?.Score;
        bool isNewPersonalBest;

        if (entry is null)
        {
            _context.LeaderboardEntries.Add(new LeaderboardEntry
            {
                UserID = userId,
                WeekStart = week,
                Score = correctCount,
                AchievedAt = now,
                RankedAttemptID = attempt.RankedAttemptID
            });

            isNewPersonalBest = true;
        }
        else if (correctCount > entry.Score)
        {
            // Tidsstämpeln följer med det nya resultatet. Raden betyder
            // "ditt bästa den här veckan, uppnått då".
            entry.Score = correctCount;
            entry.AchievedAt = now;
            entry.RankedAttemptID = attempt.RankedAttemptID;

            isNewPersonalBest = true;
        }
        else
        {
            // Sämre eller lika: raden lämnas orörd.
            isNewPersonalBest = false;
        }

        await _context.SaveChangesAsync();

        return await BuildResultAsync(attempt, correctCount, wrongCount,
            previousBestScore, isNewPersonalBest);
    }

    private async Task<RankedResultDTO> BuildResultAsync(
        RankedAttempt attempt,
        int correctCount,
        int wrongCount,
        int? previousBestScore,
        bool isNewPersonalBest)
    {
        var week = CompetitionWeek.StartOf(attempt.CompletedAt!.Value);

        var myEntry = await _context.LeaderboardEntries
            .AsNoTracking()
            .FirstAsync(e =>
                e.UserID == attempt.UserID &&
                e.WeekStart == week);

        // Placeringen räknas ut i stället för att lagras: antalet spelare
        // som ligger före mig, plus ett. Före = högre poäng, eller samma
        // poäng men uppnådd tidigare.
        var ahead = await _context.LeaderboardEntries
            .CountAsync(e =>
                e.WeekStart == week &&
                (e.Score > myEntry.Score ||
                    (e.Score == myEntry.Score &&
                     e.AchievedAt < myEntry.AchievedAt)));

        var totalPlayers = await _context.LeaderboardEntries
            .CountAsync(e => e.WeekStart == week);

        return new RankedResultDTO
        {
            RankedAttemptId = attempt.RankedAttemptID,
            Score = correctCount,
            WrongCount = wrongCount,
            AnsweredCount = correctCount + wrongCount,
            EndReason = attempt.EndReason ?? string.Empty,
            IsNewPersonalBest = isNewPersonalBest,
            PreviousBestScore = previousBestScore,
            Rank = ahead + 1,
            TotalPlayers = totalPlayers,
            WeekStart = week
        };
    }

    // ============================================================
    // TOPPLISTAN
    // ============================================================

    public async Task<LeaderboardDTO> GetLeaderboardAsync(
        bool previousWeek,
        int? userId)
    {
        var week = previousWeek
            ? CompetitionWeek.Previous()
            : CompetitionWeek.Current();

        var top = await _context.LeaderboardEntries
            .AsNoTracking()
            .Where(e => e.WeekStart == week)
            .OrderByDescending(e => e.Score)
            .ThenBy(e => e.AchievedAt)
            .Take(LeaderboardSize)
            .Select(e => new
            {
                e.UserID,
                UserName = e.User.UserName!,
                e.Score,
                e.AchievedAt
            })
            .ToListAsync();

        // Placeringen sätts här och inte i SQL. Med hundra rader är det
        // billigare än ett fönsteruttryck, och ordningen är redan avgjord
        // av OrderBy ovan.
        var rows = top
            .Select((e, index) => new LeaderboardRowDTO
            {
                Rank = index + 1,
                UserName = e.UserName,
                Score = e.Score,
                AchievedAt = e.AchievedAt
            })
            .ToList();

        var result = new LeaderboardDTO
        {
            WeekStart = week,
            IsCurrentWeek = !previousWeek,
            Rows = rows
        };

        if (userId is null)
        {
            return result;
        }

        // Ligger den inloggade redan i topp 100 behöver vi inte fråga igen.
        var inTop = top.FirstOrDefault(e => e.UserID == userId);

        if (inTop != null)
        {
            result.CurrentUserRow = rows[top.IndexOf(inTop)];
            return result;
        }

        var mine = await _context.LeaderboardEntries
            .AsNoTracking()
            .Where(e => e.UserID == userId && e.WeekStart == week)
            .Select(e => new
            {
                UserName = e.User.UserName!,
                e.Score,
                e.AchievedAt
            })
            .FirstOrDefaultAsync();

        if (mine is null)
        {
            return result;
        }

        var ahead = await _context.LeaderboardEntries
            .CountAsync(e =>
                e.WeekStart == week &&
                (e.Score > mine.Score ||
                    (e.Score == mine.Score && e.AchievedAt < mine.AchievedAt)));

        result.CurrentUserRow = new LeaderboardRowDTO
        {
            Rank = ahead + 1,
            UserName = mine.UserName,
            Score = mine.Score,
            AchievedAt = mine.AchievedAt
        };

        return result;
    }
}