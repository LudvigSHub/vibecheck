namespace VibeCheck.Api.DTOs
{
    public class WordOfTheDayDTO
    {
        public int WordId { get; set; }

        public string Word { get; set; } = string.Empty;

        public string Meaning { get; set; } = string.Empty;

        // Kan vara null – alla ord har inte ett exempel inlagt.
        public string? Example { get; set; }

        // Vilket datum servern räknade fram ordet för. Bra vid felsökning.
        public DateOnly Date { get; set; }
    }
}
