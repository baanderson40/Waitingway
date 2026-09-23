namespace Waitingway.Timers;

public enum DeployableType
{
    Airship,
    Submersible,
}

public readonly record struct DeployableSnapshot(
    string CharacterName,
    string World,
    string Name,
    DeployableType Type,
    long ReturnTimeUnixSeconds);

public readonly record struct DeployableState(
    int ReadyCount,
    int TotalCount,
    DeployableSnapshot? NextReturn,
    IReadOnlyList<DeployableSnapshot> Ready,
    IReadOnlyList<DeployableSnapshot> Pending);

public static class DeployableTracker
{
    public static DeployableState GetState(IEnumerable<DeployableSnapshot> snapshots, DateTimeOffset now)
    {
        var deployed = snapshots
            .Where(snapshot => snapshot.ReturnTimeUnixSeconds > 0)
            .OrderBy(snapshot => snapshot.ReturnTimeUnixSeconds)
            .ThenBy(snapshot => snapshot.CharacterName, StringComparer.OrdinalIgnoreCase)
            .ThenBy(snapshot => snapshot.Name, StringComparer.OrdinalIgnoreCase)
            .ToArray();
        var nowUnixSeconds = now.ToUnixTimeSeconds();
        var ready = deployed.Where(snapshot => snapshot.ReturnTimeUnixSeconds <= nowUnixSeconds).ToArray();
        var pending = deployed.Where(snapshot => snapshot.ReturnTimeUnixSeconds > nowUnixSeconds).ToArray();

        DeployableSnapshot? nextReturn = pending.Length > 0 ? pending[0] : null;
        return new(ready.Length, deployed.Length, nextReturn, ready, pending);
    }

    public static string FormatRemaining(long returnTimeUnixSeconds, DateTimeOffset now, bool compact)
    {
        var seconds = Math.Max(0, returnTimeUnixSeconds - now.ToUnixTimeSeconds());
        return OceanFishingSchedule.FormatRemaining(TimeSpan.FromSeconds(seconds), compact);
    }

    public static string FormatType(DeployableType type) => type == DeployableType.Airship ? "Airship" : "Submersible";

    public static string FormatOwner(DeployableSnapshot snapshot)
    {
        if (string.IsNullOrWhiteSpace(snapshot.World))
            return snapshot.CharacterName;

        return $"{snapshot.CharacterName}@{snapshot.World}";
    }
}
