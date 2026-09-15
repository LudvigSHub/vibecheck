namespace VibeCheck.Api.DTOs;

// Serverns dom över ett svar i det rankade quizet.
//
// Medvetet tunnare än AnswerResultDTO: här finns ingen Explanation, inget
// ord och inga böjningar. Det rankade quizet ska visa rätt eller fel och
// gå vidare direkt – inte stanna upp och lära ut.
public class RankedAnswerResultDTO
{
    public bool IsCorrect { get; set; }

    // Visas bara när man svarat fel, men skickas alltid – vid rätt svar
    // är det ändå samma alternativ som användaren valde.
    public int CorrectAlternativeId { get; set; }

    public string CorrectAlternativeText { get; set; } = string.Empty;

    public int CorrectCount { get; set; }

    public int WrongCount { get; set; }

    // Sant när servern anser att omgången är slut. Klienten ska då sluta
    // fråga och gå vidare till resultatet.
    public bool RoundOver { get; set; }

    // "TimeUp", "ThreeWrong" eller "OutOfQuestions". null när omgången pågår.
    public string? EndReason { get; set; }
}