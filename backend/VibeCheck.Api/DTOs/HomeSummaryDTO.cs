namespace VibeCheck.Api.DTOs;

public class HomeSummaryDTO
{
    public int? BestScore { get; set; }

    public int CompletedQuizCount { get; set; }

    public int CurrentStreak { get; set; }

    public ActiveQuizProgressDTO? ActiveQuiz { get; set; }
}
