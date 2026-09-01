namespace VibeCheck.Data.Data;

public class QuestionAlternativeSeed
{
    public string Question { get; set; } = string.Empty;

    public string AlternativeText { get; set; } = string.Empty;

    public bool IsCorrect { get; set; }
}