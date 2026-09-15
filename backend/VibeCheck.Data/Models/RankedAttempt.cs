using System;
using System.Collections.Generic;
using System.Text;

namespace VibeCheck.Data.Models;

// En spelad omgång av det rankade quizet. Skild från QuizAttempt med flit:
// poängen här är antal rätt och inte procent, och omgången ska varken
// påverka upplåsningar eller statistiken på startsidan.
public class RankedAttempt
{
    public int RankedAttemptID { get; set; }

    public int UserID { get; set; }

    public DateTime StartedAt { get; set; }

    // Sätts vid start till StartedAt + tidsgränsen, och lagras i stället för
    // att räknas fram vid varje kontroll. Ändrar ni gränsen från en minut
    // senare skrivs inte reglerna om för omgångar som redan spelats.
    public DateTime EndsAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    // Antal rätt svar. null tills omgången avslutats.
    public int? Score { get; set; }

    public int? WrongCount { get; set; }

    // "TimeUp", "ThreeWrong" eller "OutOfQuestions".
    public string? EndReason { get; set; }

    // Relationships
    public User User { get; set; } = null!;

    public ICollection<RankedAttemptAnswer> RankedAttemptAnswers { get; set; }
        = new List<RankedAttemptAnswer>();
}
