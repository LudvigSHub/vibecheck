namespace VibeCheck.Api.DTOs
{
    public class QuizDemoQuestionDTO
    {
        public int WordId { get; set; }

        public string Question { get; set; } = string.Empty;

        public string Quote { get; set; } = string.Empty;

        public List<QuizDemoOptionDTO> Options { get; set; } = new();

        public string CorrectId { get; set; } = string.Empty;

        public string Explanation { get; set; } = string.Empty;

    }
}
