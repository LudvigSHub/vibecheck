using System;
using System.Collections.Generic;
using System.Text;

namespace VibeCheck.Data.Models;

public class RankedAttemptAnswer
{
    public int RankedAttemptAnswerID { get; set; }

    public int RankedAttemptID { get; set; }

    public int QuestionID { get; set; }

    public int SelectedAlternativeID { get; set; }

    public bool IsCorrect { get; set; }

    // Behövs inte för poängen, men gör det möjligt att i efterhand se
    // om en omgång ser rimlig ut.
    public DateTime AnsweredAt { get; set; }

    // Relationships
    public RankedAttempt RankedAttempt { get; set; } = null!;

    public Question Question { get; set; } = null!;

    public QuestionAlternative SelectedAlternative { get; set; } = null!;
}