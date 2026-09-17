namespace VibeCheck.Api.DTOs;

public class LeaderboardRowDTO
{
    public int Rank { get; set; }

    public string UserName { get; set; } = string.Empty;

    public int Score { get; set; }

    // Tidpunkten resultatet uppnåddes. Avgör ordningen vid lika poäng,
    // och visas i listan.
    public DateTime AchievedAt { get; set; }
}