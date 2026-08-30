namespace VibeCheck.Data.Models;

public class Quiz
{
    public int QuizID { get; set; }

    public string QuizName { get; set; } = string.Empty;

    public string QuizDescription { get; set; } = string.Empty;

    public int DifficultyID { get; set; }

    // Relationships
    public Difficulty Difficulty { get; set; } = null!;

    public ICollection<QuizQuestion> QuizQuestions { get; set; }
        = new List<QuizQuestion>();

    public ICollection<QuizAttempt> QuizAttempts { get; set; }
        = new List<QuizAttempt>();
}