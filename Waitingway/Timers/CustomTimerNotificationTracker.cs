namespace Waitingway.Timers;

public sealed class CustomTimerNotificationTracker
{
    private readonly Dictionary<Guid, Snapshot> snapshots = [];
    private DateTimeOffset? lastUpdate;

    public IReadOnlyList<CustomTimerDefinition> Check(IEnumerable<CustomTimerDefinition> definitions, DateTimeOffset now)
    {
        var enabledDefinitions = definitions
            .Where(timer => timer is not null && timer.Enabled)
            .ToArray();
        var currentIds = enabledDefinitions.Select(timer => timer.Id).ToHashSet();
        var reset = lastUpdate is null || now <= lastUpdate;
        var completed = new List<CustomTimerDefinition>();

        foreach (var definition in enabledDefinitions)
        {
            var state = CustomTimerSchedule.GetState(definition, now);
            var signature = new TimerSignature(definition.Hour, definition.Minute, definition.ClockType);

            if (reset || !snapshots.TryGetValue(definition.Id, out var previous) || previous.Signature != signature)
            {
                snapshots[definition.Id] = new Snapshot(signature, state.NextOccurrence, state.IsNow);
                continue;
            }

            if ((state.IsNow && !previous.IsNow) ||
                (state.NextOccurrence > previous.NextOccurrence && now >= previous.NextOccurrence))
                completed.Add(definition);

            snapshots[definition.Id] = new Snapshot(signature, state.NextOccurrence, state.IsNow);
        }

        foreach (var id in snapshots.Keys.Where(id => !currentIds.Contains(id)).ToArray())
            snapshots.Remove(id);

        lastUpdate = now;
        return completed;
    }

    private readonly record struct TimerSignature(int Hour, int Minute, CustomTimerClockType ClockType);

    private readonly record struct Snapshot(
        TimerSignature Signature,
        DateTimeOffset NextOccurrence,
        bool IsNow);
}
