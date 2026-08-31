namespace VibeCheck.Data.Models;

public class QuizAttemptAnswer
{
    public int AttemptAnswerID { get; set; }

    public int QuizAttemptID { get; set; }

    public int QuestionID { get; set; }

    public int SelectedAlternativeID { get; set; }

    public bool IsCorrect { get; set; }

    // Relationships
    public QuizAttempt QuizAttempt { get; set; } = null!;

    public Question Question { get; set; } = null!;

    public QuestionAlternative SelectedAlternative { get; set; } = null!;
}