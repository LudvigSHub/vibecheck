namespace VibeCheck.Data.Models;

public class Meaning
{
    public int MeaningID { get; set; }

    public string MeaningText { get; set; } = string.Empty;

    // Relationships
    public ICollection<Word> Words { get; set; } = new List<Word>();
}