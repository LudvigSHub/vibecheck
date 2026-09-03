using Microsoft.EntityFrameworkCore;
using VibeCheck.Api.DTOs;
using VibeCheck.Data.Data;

namespace VibeCheck.Api.Services;

public class QuizProgressService
{
    private readonly VibeCheckDbContext _context;

    public QuizProgressService(VibeCheckDbContext context)
    {
        _context = context;
    }

    public async Task<ActiveQuizProgressDTO?> GetLatestActiveQuizAsync(int userId)
    {
        return await _context.QuizAttempts
            .AsNoTracking()
            .Where(attempt =>
                attempt.UserID == userId &&
                attempt.CompletedAt == null)
            .OrderByDescending(attempt => attempt.AttemptDate)
            .Select(attempt => new ActiveQuizProgressDTO
            {
                QuizAttemptId = attempt.QuizAttemptID,
                QuizId = attempt.QuizID,
                QuizName = attempt.Quiz.QuizName,
                AnsweredQuestionCount = attempt.QuizAttemptAnswers.Count,
                TotalQuestionCount = attempt.Quiz.QuizQuestions.Count
            })
            .FirstOrDefaultAsync();
    }
}
