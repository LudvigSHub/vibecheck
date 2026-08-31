using Microsoft.AspNetCore.Identity;

namespace VibeCheck.Data.Models;

public class User : IdentityUser<int>
{
    //public bool IsAdmin { get; set; }

    public ICollection<QuizAttempt> QuizAttempts { get; set; } = new List<QuizAttempt>();
    public ICollection<WordVote> WordVotes { get; set; } = new List<WordVote>();
}
