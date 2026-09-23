namespace Waitingway.Timers;

public sealed class DeployableTimer : IWaitingwayTimer
{
    private const int TooltipRowLimit = 12;
    private IReadOnlyList<DeployableSnapshot> snapshots = Array.Empty<DeployableSnapshot>();
    private bool providerAvailable;

    public string Id => "deployables";

    public string Title => "Waitingway: Deployables";

    public bool IsAvailable => providerAvailable && snapshots.Any(snapshot => snapshot.ReturnTimeUnixSeconds > 0);

    public void SetData(bool isProviderAvailable, IReadOnlyList<DeployableSnapshot> data)
    {
        providerAvailable = isProviderAvailable;
        snapshots = data;
    }

    public string GetDisplayText(DateTime utcNow, bool compact)
    {
        var state = GetState(utcNow);
        if (state.ReadyCount > 0)
            return compact ? $"Dep: {state.ReadyCount} ready" : $"Deployables: {state.ReadyCount} ready";

        if (state.NextReturn is not { } next)
            return compact ? "Dep: none" : "Deployables: none";

        var remaining = DeployableTracker.FormatRemaining(next.ReturnTimeUnixSeconds, new DateTimeOffset(utcNow), compact);
        return compact ? $"Dep: {remaining}" : $"Deployables: {remaining}";
    }

    public string GetTooltip(DateTime utcNow)
    {
        if (!providerAvailable)
            return "AutoRetainer data unavailable.";

        var state = GetState(utcNow);
        if (state.TotalCount == 0)
            return "No deployed airships or submersibles found.";

        var lines = new List<string>
        {
            $"{state.ReadyCount} ready / {state.TotalCount} deployed",
        };
        lines.AddRange(state.Ready.Take(TooltipRowLimit).Select(snapshot =>
            $"Ready: {DeployableTracker.FormatOwner(snapshot)} - {snapshot.Name} ({DeployableTracker.FormatType(snapshot.Type)})"));

        var remainingRows = Math.Max(0, TooltipRowLimit - state.Ready.Count);
        lines.AddRange(state.Pending.Take(remainingRows).Select(snapshot =>
            $"{DeployableTracker.FormatOwner(snapshot)} - {snapshot.Name} ({DeployableTracker.FormatType(snapshot.Type)}): " +
            DeployableTracker.FormatRemaining(snapshot.ReturnTimeUnixSeconds, new DateTimeOffset(utcNow), false)));

        if (state.Ready.Count + state.Pending.Count > TooltipRowLimit)
            lines.Add($"... and {state.Ready.Count + state.Pending.Count - TooltipRowLimit} more.");

        return string.Join("\n", lines);
    }

    private DeployableState GetState(DateTime utcNow)
        => DeployableTracker.GetState(snapshots, new DateTimeOffset(utcNow));
}
