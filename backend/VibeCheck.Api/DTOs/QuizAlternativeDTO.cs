namespace VibeCheck.Api.DTOs;

// Ett svarsalternativ så som klienten får se det.
// Notera vad som INTE finns här: ingenting som avslöjar om det är rätt.
public class QuizAlternativeDTO
{
    public int AlternativeId { get; set; }

    public string AlternativeText { get; set; } = string.Empty;
}