namespace VibeCheck.Api.DTOs;

// Serverns dom över ett enskilt svar.
public class AnswerResultDTO
{
    public bool IsCorrect { get; set; }

    // Skickas först nu, när frågan är besvarad och det inte längre går att fuska.
    public int CorrectAlternativeId { get; set; }

    public string CorrectAlternativeText { get; set; } = string.Empty;

    // Ordets betydelse, hämtad via Question.Word.Meaning.
    // Ligger här och inte i QuizQuestionDTO: betydelsen av "yap" avslöjar
    // svaret lika säkert som CorrectAlternativeId skulle göra.
    public string Explanation { get; set; } = string.Empty;

    // Räknas fram på servern, inte i React. Servern vet hur många svar som
    // faktiskt ligger i databasen, även om användaren laddar om sidan mitt i.
    public int AnsweredCount { get; set; }

    public int TotalCount { get; set; }
}