namespace VibeCheck.Data.Models;

public class InflectionType
{
    public int InflectionTypeID { get; set; }

    public string Code { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public int SortOrder { get; set; }

    // Relationships
    public ICollection<WordInflection> WordInflections { get; set; }
        = new List<WordInflection>();
}
