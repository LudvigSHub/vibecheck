namespace VibeCheck.Api.DTOs;

public class AdminUpdateWordDTO
{
    public string Word { get; set; } = string.Empty;

    public string Meaning { get; set; } = string.Empty;

    public List<string> Examples { get; set; } = [];

    public List<int> TagIds { get; set; } = [];

    // Null behåller befintliga böjningar. En tom lista tar bort alla.
    public List<AdminWordInflectionDTO>? Inflections { get; set; }
}
