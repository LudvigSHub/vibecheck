namespace VibeCheck.Api.DTOs;

// Slutresultatet av en omgång, plus var det placerade sig.
public class RankedResultDTO
{
    public int RankedAttemptId { get; set; }

    // Antal rätt – ett poäng per korrekt svar. Inte procent, till skillnad
    // från de vanliga quizen.
    public int Score { get; set; }

    public int WrongCount { get; set; }

    public int AnsweredCount { get; set; }

    public string EndReason { get; set; } = string.Empty;

    public bool IsNewPersonalBest { get; set; }

    // Bästa resultatet den här veckan FÖRE omgången. null om det var
    // veckans första.
    public int? PreviousBestScore { get; set; }

    // Placering på veckans lista efter att resultatet räknats in.
    public int Rank { get; set; }

    public int TotalPlayers { get; set; }

    public DateOnly WeekStart { get; set; }
}