namespace VibeCheck.Data.Models;

// Kopplar en utvald fråga till ett försök och sparar dess plats i omgången.
public class QuizAttemptQuestion
{
    public int QuizAttemptID { get; set; }
    public int QuestionID { get; set; }
    public int Order { get; set; }

    public QuizAttempt QuizAttempt { get; set; } = null!;
    public Question Question { get; set; } = null!;
}
