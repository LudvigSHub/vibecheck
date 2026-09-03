namespace VibeCheck.Api.DTOs;

public class QuizQuestionDTO
{
    public int QuestionId { get; set; }

    // Frågetypens generella formulering, från QuestionType.Description.
    // T.ex. "Är påståendet ovan sant eller falskt?"
    public string Prompt { get; set; } = string.Empty;

    // Själva situationen eller påståendet, från Question.QuestionDesc.
    public string Body { get; set; } = string.Empty;

    // "Multiple Choice" eller "True or False".
    // Frontend kan använda den för att rendera 2 respektive 4 alternativ olika.
    public string QuestionType { get; set; } = string.Empty;

    public List<QuizAlternativeDTO> Alternatives { get; set; } = new();
}