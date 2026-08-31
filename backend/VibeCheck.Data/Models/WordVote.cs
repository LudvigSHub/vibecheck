namespace VibeCheck.Data.Models;

public class WordVote
{
    public int WordVoteID { get; set; }

    public int WordID { get; set; }

    public int UserID { get; set; }

    public bool IsPositive { get; set; }

    // Relationships
    public Word Word { get; set; } = null!;

    public User User { get; set; } = null!;
}