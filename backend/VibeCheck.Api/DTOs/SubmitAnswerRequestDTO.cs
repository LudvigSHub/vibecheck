namespace VibeCheck.Api.DTOs;

// Det klienten skickar in när användaren klickar på ett alternativ.
// QuestionId följer med trots att AlternativeId räcker för att hitta frågan –
// servern kan då kontrollera att de hör ihop och avvisa hittepå-kombinationer.
public class SubmitAnswerRequestDTO
{
    public int QuestionId { get; set; }

    public int AlternativeId { get; set; }
}