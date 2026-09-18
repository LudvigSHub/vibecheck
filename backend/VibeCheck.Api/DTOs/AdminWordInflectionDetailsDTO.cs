namespace VibeCheck.Api.DTOs;

public class AdminWordInflectionDetailsDTO
{
    public int InflectionTypeId { get; set; }

    public string InflectedText { get; set; } = string.Empty;

    public string TypeName { get; set; } = string.Empty;
}
