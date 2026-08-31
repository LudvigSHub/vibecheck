namespace VibeCheck.Data.Models;

public class WordExample
{
    public int ExampleID { get; set; }

    public int WordID { get; set; }

    public string ExampleText { get; set; } = string.Empty;

    // Relationship
    public Word Word { get; set; } = null!;
}