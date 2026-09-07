namespace VibeCheck.Data.Models;

public class WordInflection
{
    public int WordInflectionID { get; set; }

    public int WordID { get; set; }

    public int InflectionTypeID { get; set; }

    public string InflectedText { get; set; } = string.Empty;

    // Relationships
    public Word Word { get; set; } = null!;

    public InflectionType InflectionType { get; set; } = null!;
}
