namespace VibeCheck.Data.Models;

public class Difficulty
{
    public int DifficultyID { get; set; }

    public string DifficultyDesc { get; set; } = string.Empty;

    // Relationships
    public ICollection<Question> Questions { get; set; }
        = new List<Question>();

    public ICollection<Quiz> Quizzes { get; set; }
        = new List<Quiz>();
}