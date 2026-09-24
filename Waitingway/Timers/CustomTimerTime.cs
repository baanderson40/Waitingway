using System.Globalization;

namespace Waitingway.Timers;

public static class CustomTimerTime
{
    public static string Format(int hour, int minute) => $"{hour:00}:{minute:00}";

    public static bool TryParse(string input, out int hour, out int minute)
    {
        hour = 0;
        minute = 0;

        var parts = input.Trim().Split(':', StringSplitOptions.None);
        if (parts.Length != 2 ||
            parts[0].Length is < 1 or > 2 ||
            parts[1].Length != 2 ||
            !int.TryParse(parts[0], NumberStyles.None, CultureInfo.InvariantCulture, out hour) ||
            !int.TryParse(parts[1], NumberStyles.None, CultureInfo.InvariantCulture, out minute))
            return false;

        return hour is >= 0 and <= 23 && minute is >= 0 and <= 59;
    }
}
