namespace VibeCheck.Api.DTOs;

// Svaret när en omgång startas: kvittot, tiden och alla frågor.
public class StartRankedAttemptDTO
{
    public int RankedAttemptId { get; set; }

    // Hur lång tid som återstår, räknat av servern. Klienten startar sin
    // nedräkning från det här talet i stället för att jämföra klockor –
    // webbläsarens klocka kan gå fel, serverns är den som gäller.
    public int SecondsRemaining { get; set; }

    // Den faktiska deadlinen i UTC. Klienten behöver den inte för
    // nedräkningen, men den gör felsökning begriplig.
    public DateTime EndsAt { get; set; }

    // Hur många fel som avslutar omgången. Ligger här av samma skäl som
    // RequiredScore i QuizListItemDTO: regeln ska bo på ett ställe.
    public int MaxWrongAnswers { get; set; }

    // Alla frågor, blandade, utan facit.
    public List<QuizQuestionDTO> Questions { get; set; } = new();
}