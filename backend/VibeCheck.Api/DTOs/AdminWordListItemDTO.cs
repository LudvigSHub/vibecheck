namespace VibeCheck.Api.DTOs;

public class AdminWordListItemDTO
{
    public int WordId { get; set; }

    public string Word { get; set; } = string.Empty;

    public string Meaning { get; set; } = string.Empty;

    public int ExampleCount { get; set; }

    public List<AdminTagListItemDTO> Tags { get; set; } = [];

    public bool IsUsedInQuiz { get; set; }
}