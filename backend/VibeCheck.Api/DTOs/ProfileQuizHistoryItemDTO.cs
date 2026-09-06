namespace VibeCheck.Api.DTOs
{
    public class ProfileQuizHistoryItemDTO
    {
        public string QuizName { get; set; } = string.Empty;

        public string Topic { get; set; } = string.Empty;

        public int Score { get; set; }

        public DateTime CompletedAt { get; set; }
    }
}
