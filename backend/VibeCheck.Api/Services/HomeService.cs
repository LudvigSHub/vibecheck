using Microsoft.EntityFrameworkCore;
using VibeCheck.Api.DTOs;
using VibeCheck.Data.Data;

namespace VibeCheck.Api.Services;

public class HomeService
{
    private readonly VibeCheckDbContext _context;

    public HomeService(VibeCheckDbContext context)
    {
        _context = context;
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

        return summary ?? new HomeSummaryDTO();
    }
}
