using Microsoft.EntityFrameworkCore;
using VibeCheck.Api.DTOs;
using VibeCheck.Data.Data;

namespace VibeCheck.Api.Services;

public class HomeService
{
    private readonly VibeCheckDbContext _context;
    private readonly QuizProgressService _quizProgressService;

    public HomeService(
        VibeCheckDbContext context,
        QuizProgressService quizProgressService)
    {
        _context = context;
        _quizProgressService = quizProgressService;
    }

    public async Task<HomeSummaryDTO> GetSummaryAsync(int userId)
    {
        var completedAttempts = _context.QuizAttempts
            .AsNoTracking()
            .Where(attempt =>
                attempt.UserID == userId &&
                attempt.CompletedAt != null);

        var summary = await completedAttempts
            .GroupBy(attempt => 1)
            .Select(group => new HomeSummaryDTO
            {
                BestScore = group.Max(attempt => attempt.Score),
                CompletedQuizCount = group.Count()
            })
            .FirstOrDefaultAsync();

        summary ??= new HomeSummaryDTO();

        var completionDates = await completedAttempts
            .Select(attempt => attempt.CompletedAt!.Value.Date)
            .Distinct()
            .OrderByDescending(date => date)
            .ToListAsync();

        summary.CurrentStreak = CalculateCurrentStreak(
            completionDates,
            DateTime.UtcNow.Date);

        summary.ActiveQuiz =
            await _quizProgressService.GetLatestActiveQuizAsync(userId);

        return summary;
    }

    private static int CalculateCurrentStreak(
        IReadOnlyList<DateTime> completionDates,
        DateTime today)
    {
        if (completionDates.Count == 0 || completionDates[0] < today.AddDays(-1))
        {
            return 0;
        }

        var expectedDate = completionDates[0] == today
            ? today
            : today.AddDays(-1);
        var streak = 0;

        foreach (var completionDate in completionDates)
        {
            if (completionDate != expectedDate)
            {
                break;
            }

            streak++;
            expectedDate = expectedDate.AddDays(-1);
        }

        return streak;
    }
}
