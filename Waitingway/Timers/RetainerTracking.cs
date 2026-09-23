namespace Waitingway.Timers;

public readonly record struct RetainerSnapshot(
    string CharacterName,
    string World,
    string Name,
    long VentureEndsAtUnixSeconds);

public readonly record struct RetainerState(
    int ReadyCount,
    int TotalCount,
    RetainerSnapshot? NextReturn,
    IReadOnlyList<RetainerSnapshot> Ready,
    IReadOnlyList<RetainerSnapshot> Pending);

public static class RetainerTracker
{
    public static RetainerState GetState(IEnumerable<RetainerSnapshot> snapshots, DateTimeOffset now)
    {
        var active = snapshots
            .Where(snapshot => snapshot.VentureEndsAtUnixSeconds > 0)
            .OrderBy(snapshot => snapshot.VentureEndsAtUnixSeconds)
            .ThenBy(snapshot => snapshot.CharacterName, StringComparer.OrdinalIgnoreCase)
            .ThenBy(snapshot => snapshot.Name, StringComparer.OrdinalIgnoreCase)
            .ToArray();
        var nowUnixSeconds = now.ToUnixTimeSeconds();
        var ready = active.Where(snapshot => snapshot.VentureEndsAtUnixSeconds <= nowUnixSeconds).ToArray();
        var pending = active.Where(snapshot => snapshot.VentureEndsAtUnixSeconds > nowUnixSeconds).ToArray();
        RetainerSnapshot? nextReturn = pending.Length > 0 ? pending[0] : null;

        return new(ready.Length, active.Length, nextReturn, ready, pending);
    }

    public static string FormatRemaining(long ventureEndsAtUnixSeconds, DateTimeOffset now, bool compact)
    {
        var seconds = Math.Max(0, ventureEndsAtUnixSeconds - now.ToUnixTimeSeconds());
        return OceanFishingSchedule.FormatRemaining(TimeSpan.FromSeconds(seconds), compact);
    }

    public static string FormatOwner(RetainerSnapshot snapshot)
    {
        if (string.IsNullOrWhiteSpace(snapshot.World))
            return snapshot.CharacterName;

        return $"{snapshot.CharacterName}@{snapshot.World}";
    }
}
