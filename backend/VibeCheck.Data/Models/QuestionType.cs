namespace VibeCheck.Data.Models;

public class QuestionType
{
    public int QuestionTypeID { get; set; }

    public string TypeText { get; set; } = string.Empty;

    // Relationships
    public ICollection<Question> Questions { get; set; }
        = new List<Question>();
}