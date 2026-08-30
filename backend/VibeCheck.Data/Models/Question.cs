namespace VibeCheck.Data.Models;

public class Question
{
    public int QuestionID { get; set; }

    public string QuestionDesc { get; set; } = string.Empty;

    public int QuestionTypeID { get; set; }

    public int DifficultyID { get; set; }

    public int? CorrectAlternativeID { get; set; }

    // Relationships
    public QuestionType QuestionType { get; set; } = null!;

    public Difficulty Difficulty { get; set; } = null!;

    public ICollection<QuestionAlternative> QuestionAlternatives { get; set; }
        = new List<QuestionAlternative>();

    public ICollection<QuizQuestion> QuizQuestions { get; set; }
        = new List<QuizQuestion>();

    public ICollection<QuizAttemptAnswer> QuizAttemptAnswers { get; set; }
        = new List<QuizAttemptAnswer>();
}