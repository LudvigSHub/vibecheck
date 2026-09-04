namespace VibeCheck.Api.DTOs;

public class AdminCreateWordDTO
{
    public string Word { get; set; } = string.Empty;

    public string Meaning { get; set; } = string.Empty;

    public List<string> Examples { get; set; } = [];

    public List<int> TagIds { get; set; } = [];
}