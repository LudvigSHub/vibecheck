namespace VibeCheck.Data.Models;

public class QuestionAlternative
{
    public int AlternativeID { get; set; }

    public int QuestionID { get; set; }

    public string AlternativeText { get; set; } = string.Empty;

    // Relationships
    public Question Question { get; set; } = null!;

    public ICollection<QuizAttemptAnswer> QuizAttemptAnswers { get; set; }
        = new List<QuizAttemptAnswer>();
}