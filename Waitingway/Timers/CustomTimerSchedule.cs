namespace Waitingway.Timers;

public static class CustomTimerSchedule
{
    private static readonly TimeSpan NowWindow = TimeSpan.FromMinutes(1);
    private static readonly TimeSpan EorzeaDay = TimeSpan.FromMinutes(70);
    private const double EorzeaDaySeconds = 70 * 60;
    private const double EorzeaSecondsPerMinute = EorzeaDaySeconds / (24 * 60);

    public static CustomTimerState GetState(CustomTimerDefinition definition, DateTimeOffset now)
    {
        var occurrence = definition.ClockType == CustomTimerClockType.Eorzea
                             ? GetEorzeaOccurrence(definition, now)
                             : GetLocalOccurrence(definition, now);

        if (now >= occurrence && now < occurrence + NowWindow)
            return new(occurrence, TimeSpan.Zero, true);

        if (now < occurrence)
            return new(occurrence, occurrence - now, false);

        var nextOccurrence = definition.ClockType == CustomTimerClockType.Eorzea
                                  ? occurrence + EorzeaDay
                                  : GetLocalOccurrence(definition, now.AddDays(1));
        return new(nextOccurrence, nextOccurrence - now, false);
    }

    private static DateTimeOffset GetEorzeaOccurrence(CustomTimerDefinition definition, DateTimeOffset now)
    {
        var unixSeconds = (now - DateTimeOffset.UnixEpoch).TotalSeconds;
        var day = Math.Floor(unixSeconds / EorzeaDaySeconds);
        var targetMinutes = NormalizeMinutes(definition);
        var targetSeconds = targetMinutes * EorzeaSecondsPerMinute;
        return DateTimeOffset.UnixEpoch.AddSeconds(day * EorzeaDaySeconds + targetSeconds);
    }

    private static DateTimeOffset GetLocalOccurrence(CustomTimerDefinition definition, DateTimeOffset now)
    {
        var localNow = now.ToLocalTime();
        var target = NormalizeMinutes(definition);
        return ResolveLocalTime(localNow.Date, target / 60, target % 60);
    }

    private static DateTimeOffset ResolveLocalTime(DateTime date, int hour, int minute)
    {
        var zone = TimeZoneInfo.Local;
        var wallTime = DateTime.SpecifyKind(date.Date.AddHours(hour).AddMinutes(minute), DateTimeKind.Unspecified);

        while (zone.IsInvalidTime(wallTime))
            wallTime = wallTime.AddMinutes(1);

        var offset = zone.IsAmbiguousTime(wallTime)
                         ? zone.GetAmbiguousTimeOffsets(wallTime).Max()
                         : zone.GetUtcOffset(wallTime);
        return new DateTimeOffset(wallTime, offset).ToUniversalTime();
    }

    private static int NormalizeMinutes(CustomTimerDefinition definition)
    {
        var hour = Math.Clamp(definition.Hour, 0, 23);
        var minute = Math.Clamp(definition.Minute, 0, 59);
        return hour * 60 + minute;
    }
}
