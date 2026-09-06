namespace VibeCheck.Api.DTOs;

// Svaret när ett quiz startas: ett kvitto på försöket plus alla frågor.
public class StartQuizAttemptDTO
{
    public int QuizAttemptId { get; set; }

    public int QuizId { get; set; }

    public string QuizName { get; set; } = string.Empty;

    public List<QuizQuestionDTO> Questions { get; set; } = new();
}