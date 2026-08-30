namespace VibeCheck.Data.Models;

public class WordTag
{
    public int WordID { get; set; }

    public int TagID { get; set; }

    // Relationships
    public Word Word { get; set; } = null!;

    public Tag Tag { get; set; } = null!;
}