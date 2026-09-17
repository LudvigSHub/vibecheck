using System;
using System.Collections.Generic;
using System.Text;

namespace VibeCheck.Data.Models;

// Bästa resultatet per användare och vecka. Raden uppdateras bara när
// någon slår sitt eget resultat — och då följer tidsstämpeln med.
public class LeaderboardEntry
{
    public int LeaderboardEntryID { get; set; }

    // Måndagens datum i svensk tid för den vecka resultatet tillhör.
    // DateOnly och inte DateTime: en vecka har ingen klockslagsdel, och
    // typen gör det omöjligt att av misstag jämföra mot en tidpunkt.
    public DateOnly WeekStart { get; set; }

    public int UserID { get; set; }

    public int Score { get; set; }

    // Avgör ordningen vid lika poäng — tidigast uppnått rankas högst.
    public DateTime AchievedAt { get; set; }

    public int RankedAttemptID { get; set; }

    // Relationships
    public User User { get; set; } = null!;

    public RankedAttempt RankedAttempt { get; set; } = null!;
}