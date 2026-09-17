namespace VibeCheck.Api.Services;

// Tävlingsveckan: måndag 00:00 till söndag 23:59, svensk tid.
//
// Tidsstämplar lagras i UTC i hela kodbasen, men veckogränsen är en lokal
// företeelse. Att räkna veckan direkt på UTC-datumet ger fel svar varje
// söndagsnatt – två timmar på sommaren, en på vintern – eftersom det då
// redan är måndag i Sverige men fortfarande söndag i UTC.
public static class CompetitionWeek
{
    // Samma tidszon som WordsController använder för dagens ord.
    // IANA-id fungerar även på Windows sedan .NET 6.
    private static readonly TimeZoneInfo SwedishTime =
        TimeZoneInfo.FindSystemTimeZoneById("Europe/Stockholm");

    // Vilken vecka tillhör den här tidpunkten? Svaret är måndagens datum.
    public static DateOnly StartOf(DateTime utc)
    {
        // SpecifyKind skyddar mot att EF lämnar tillbaka Unspecified –
        // då hade DateTimeOffset antagit serverns lokala tid i stället.
        var instant = new DateTimeOffset(
            DateTime.SpecifyKind(utc, DateTimeKind.Utc));

        var local = TimeZoneInfo.ConvertTime(instant, SwedishTime);

        // DayOfWeek räknar söndag som 0. Vi vill ha måndag som dag 0, så
        // vi förskjuter med sex och tar resten vid division med sju:
        // söndag blir 6, måndag 0, tisdag 1 och så vidare.
        var daysSinceMonday = ((int)local.DayOfWeek + 6) % 7;

        return DateOnly
            .FromDateTime(local.DateTime)
            .AddDays(-daysSinceMonday);
    }

    public static DateOnly Current() => StartOf(DateTime.UtcNow);

    public static DateOnly Previous() => Current().AddDays(-7);
}