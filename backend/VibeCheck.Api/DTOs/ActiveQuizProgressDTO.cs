namespace VibeCheck.Api.DTOs;

public class ActiveQuizProgressDTO
{
    public int QuizAttemptId { get; set; }

    public int QuizId { get; set; }

    public string QuizName { get; set; } = string.Empty;

    public int AnsweredQuestionCount { get; set; }

    public int TotalQuestionCount { get; set; }
}
