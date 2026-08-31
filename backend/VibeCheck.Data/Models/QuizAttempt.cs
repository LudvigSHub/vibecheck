namespace VibeCheck.Data.Models;

public class QuizAttempt
{
    public int QuizAttemptID { get; set; }

    public int UserID { get; set; }

    public int QuizID { get; set; }

    public int? Score { get; set; }

    public bool? QuizPassed { get; set; }

    public DateTime AttemptDate { get; set; }

    public DateTime? CompletedAt { get; set; }

    // Relationships
    public User User { get; set; } = null!;

    public Quiz Quiz { get; set; } = null!;

    public ICollection<QuizAttemptAnswer> QuizAttemptAnswers { get; set; }
        = new List<QuizAttemptAnswer>();
}