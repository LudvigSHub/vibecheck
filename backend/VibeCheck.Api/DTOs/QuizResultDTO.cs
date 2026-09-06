namespace VibeCheck.Api.DTOs;

// Slutresultatet, det som visas på resultatskärmen.
public class QuizResultDTO
{
    public int QuizAttemptId { get; set; }

    public int QuizId { get; set; }

    public string QuizName { get; set; } = string.Empty;

    public int CorrectCount { get; set; }

    public int TotalCount { get; set; }

    // Procent, 0–100. Samma enhet som QuizAttempt.Score redan använder
    // (seedningen sätter 100 och 67, och HomePage skriver ut det med %).
    public int Score { get; set; }

    public bool Passed { get; set; }

    // Bästa resultatet FÖRE det här försöket. Låter resultatskärmen säga
    // "nytt personbästa" eller "ditt bästa är fortfarande 70%".
    public int? PreviousBestScore { get; set; }

    public bool IsNewBest { get; set; }

    // Namnet på quizet som just låstes upp, annars null.
    public string? UnlockedQuizName { get; set; }
}