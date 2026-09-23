namespace Waitingway.Timers;

public sealed class OceanFishingTimer : IWaitingwayTimer
{
    public string Id => "ocean-fishing";

    public string Title => "Waitingway: Ocean Fishing";

    public bool IsAvailable => true;

    public string GetDisplayText(DateTime utcNow, bool compact)
    {
        var state = OceanFishingSchedule.GetState(utcNow);
        var remaining = OceanFishingSchedule.FormatRemaining(state.Remaining, compact);

        return state.IsOpen
                   ? compact ? $"OF: Open {remaining}" : $"Ocean Fishing: Open {remaining}"
                   : compact ? $"OF: {remaining}" : $"Ocean Fishing: {remaining}";
    }

    public string GetTooltip(DateTime utcNow)
    {
        var state = OceanFishingSchedule.GetState(utcNow);
        var localStart = ToLocalTime(state.NextWindowStart);
        return state.IsOpen
                   ? $"Ocean Fishing registration is open until {FormatLocalTime(localStart + OceanFishingSchedule.WindowLength)}."
                   : $"Next Ocean Fishing registration: {FormatLocalTime(localStart)}.";
    }

    private static DateTimeOffset ToLocalTime(DateTime utcTime)
        => new DateTimeOffset(DateTime.SpecifyKind(utcTime, DateTimeKind.Utc)).ToLocalTime();

    private static string FormatLocalTime(DateTimeOffset localTime)
        => $"{localTime:t} local (UTC{localTime:zzz})";
}
