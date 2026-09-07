namespace VibeCheck.Api.DTOs;

public class WordStashDTO
{
    public int WordId { get; set; }

    public string Word { get; set; } = string.Empty;

    public string Meaning { get; set; } = string.Empty;

    public List<string> Examples { get; set; } = new();

    public List<WordTagDTO> Tags { get; set; } = new();

    public bool IsInappropriate { get; set; }
}

public class WordTagDTO
{
    public int TagId { get; set; }

    public string TagName { get; set; } = string.Empty;
}