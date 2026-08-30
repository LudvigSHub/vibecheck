namespace VibeCheck.Data.Models;

public class QuizQuestion
{
    public int QuizQuestionID { get; set; }

    public int QuizID { get; set; }

    public int QuestionID { get; set; }

    // Relationships
    public Quiz Quiz { get; set; } = null!;

    public Question Question { get; set; } = null!;
}