namespace VibeCheck.Data.Models;

public class Tag
{
    public int TagID { get; set; }

    public string TagName { get; set; } = string.Empty;

    // Relationships
    public ICollection<WordTag> WordTags { get; set; }
        = new List<WordTag>();
}