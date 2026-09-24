namespace Waitingway.Timers;

public sealed class CustomTimer : IWaitingwayTimer
{
    private const int TooltipRowLimit = 12;
    private IReadOnlyList<CustomTimerDefinition> timers = Array.Empty<CustomTimerDefinition>();

    public string Id => "custom-timers";

    public string Title => "Waitingway: Custom Timers";

    public bool IsAvailable => timers.Any(timer => timer.Enabled);

    public void SetTimers(IEnumerable<CustomTimerDefinition> definitions)
    {
        timers = definitions.Where(timer => timer is not null && timer.Enabled).ToArray();
    }

    public string GetDisplayText(DateTime utcNow, bool compact)
    {
        var next = GetStates(utcNow).FirstOrDefault();
        if (next.Definition is null)
            return "Custom Timers";

        var value = next.State.IsNow
                        ? "Now"
                        : OceanFishingSchedule.FormatRemaining(next.State.Remaining, compact);
        return $"{DisplayName(next.Definition)}: {value}";
    }

    public string GetTooltip(DateTime utcNow)
    {
        var states = GetStates(utcNow);
        if (states.Count == 0)
            return "No custom timers configured.";

        var lines = states.Take(TooltipRowLimit).Select(timer =>
        {
            var value = timer.State.IsNow
                            ? "Now"
                            : $"in {OceanFishingSchedule.FormatRemaining(timer.State.Remaining, false)}";
            return $"{DisplayName(timer.Definition)} [{timer.Definition.ClockType}] {timer.Definition.Hour:00}:{timer.Definition.Minute:00}: {value}";
        }).ToList();

        if (states.Count > TooltipRowLimit)
            lines.Add($"... and {states.Count - TooltipRowLimit} more.");

        return string.Join("\n", lines);
    }

    private List<CustomTimerDisplayState> GetStates(DateTime utcNow)
    {
        var now = new DateTimeOffset(DateTime.SpecifyKind(utcNow, DateTimeKind.Utc));
        return timers
            .Select(timer => new CustomTimerDisplayState(timer, CustomTimerSchedule.GetState(timer, now)))
            .OrderByDescending(timer => timer.State.IsNow)
            .ThenBy(timer => timer.State.Remaining)
            .ToList();
    }

    private static string DisplayName(CustomTimerDefinition definition)
        => string.IsNullOrWhiteSpace(definition.Name) ? "Custom Timer" : definition.Name;

    private readonly record struct CustomTimerDisplayState(CustomTimerDefinition Definition, CustomTimerState State);
}
