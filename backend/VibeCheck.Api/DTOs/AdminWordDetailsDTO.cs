namespace VibeCheck.Api.DTOs;

public class AdminWordDetailsDTO
{
    public int WordId { get; set; }

    public string Word { get; set; } = string.Empty;

    public string Meaning { get; set; } = string.Empty;

    public List<string> Examples { get; set; } = [];

    public List<AdminTagListItemDTO> Tags { get; set; } = [];

    public bool IsUsedInQuiz { get; set; }
}
