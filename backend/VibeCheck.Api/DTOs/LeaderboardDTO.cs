namespace VibeCheck.Api.DTOs;

public class LeaderboardDTO
{
    public DateOnly WeekStart { get; set; }

    public bool IsCurrentWeek { get; set; }

    // Topp 100.
    public List<LeaderboardRowDTO> Rows { get; set; } = new();

    // Den inloggades egen rad, även när hen inte ryms i topp 100.
    // null för utloggade besökare och för den som inte spelat den veckan.
    public LeaderboardRowDTO? CurrentUserRow { get; set; }
}