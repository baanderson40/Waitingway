namespace Waitingway.Timers;

public enum OceanFishingTimerStatus
{
    Closed,
    Open
}

public readonly record struct OceanFishingWindow(DateTime Start, DateTime End);

public readonly record struct OceanFishingTimerState(
    OceanFishingTimerStatus Status,
    TimeSpan Remaining,
    DateTime NextWindowStart)
{
    public bool IsOpen => Status == OceanFishingTimerStatus.Open;
}

public static class OceanFishingSchedule
{
    public static readonly TimeSpan WindowLength = TimeSpan.FromMinutes(15);
    public static readonly TimeSpan Interval = TimeSpan.FromHours(2);

    public static OceanFishingTimerState GetState(DateTime utcNow)
    {
        utcNow = EnsureUtc(utcNow);
        var window = GetUpcomingWindow(utcNow);

        if (utcNow >= window.Start && utcNow < window.End)
            return new(OceanFishingTimerStatus.Open, window.End - utcNow, window.Start);

        return new(OceanFishingTimerStatus.Closed, window.Start - utcNow, window.Start);
    }

    public static OceanFishingWindow GetUpcomingWindow(DateTime utcNow)
    {
        utcNow = EnsureUtc(utcNow);
        var evenHour = utcNow.Hour - utcNow.Hour % 2;
        var windowStart = new DateTime(utcNow.Year, utcNow.Month, utcNow.Day, evenHour, 0, 0, DateTimeKind.Utc);

        if (utcNow >= windowStart + WindowLength)
            windowStart += Interval;

        return new(windowStart, windowStart + WindowLength);
    }

    public static string FormatRemaining(TimeSpan remaining, bool compact)
    {
        remaining = remaining < TimeSpan.Zero ? TimeSpan.Zero : remaining;
        var totalSeconds = (long)remaining.TotalSeconds;
        var hours = totalSeconds / 3600;
        var minutes = totalSeconds / 60 % 60;
        var seconds = totalSeconds % 60;

        if (hours > 0)
            return compact ? $"{hours}:{minutes:00}:{seconds:00}" : $"{hours:00}:{minutes:00}:{seconds:00}";

        return $"{minutes:00}:{seconds:00}";
    }

    private static DateTime EnsureUtc(DateTime value)
        => value.Kind == DateTimeKind.Utc ? value : value.ToUniversalTime();
}
