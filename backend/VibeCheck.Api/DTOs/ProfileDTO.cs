namespace VibeCheck.Api.DTOs
{
    public class ProfileDTO
    {
        public string UserName { get; set; } = string.Empty;

        public int? BestScore { get; set; }

        public int CompletedQuizCount { get; set; }

        public int CurrentStreak { get; set; }

        public List<ProfileQuizHistoryItemDTO> QuizHistory { get; set; } = [];
    }
}
