namespace VibeCheck.Data.Models;

public class Word
{
    public int WordID { get; set; }

    public string WordDesc { get; set; } = string.Empty;

    public int MeaningID { get; set; }

    // Relationships
    public Meaning Meaning { get; set; } = null!;

    public ICollection<WordExample> WordExamples { get; set; }
        = new List<WordExample>();

    public ICollection<WordVote> WordVotes { get; set; }
        = new List<WordVote>();

    public ICollection<WordTag> WordTags { get; set; }
        = new List<WordTag>();

    public ICollection<Question> Questions { get; set; }
        = new List<Question>();
}