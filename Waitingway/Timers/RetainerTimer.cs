namespace Waitingway.Timers;

public sealed class RetainerTimer : IWaitingwayTimer
{
    private const int TooltipRowLimit = 12;
    private IReadOnlyList<RetainerSnapshot> snapshots = Array.Empty<RetainerSnapshot>();
    private bool providerAvailable;

    public string Id => "retainers";

    public string Title => "Waitingway: Retainers";

    public bool IsAvailable => providerAvailable && snapshots.Count > 0;

    public void SetData(bool isProviderAvailable, IReadOnlyList<RetainerSnapshot> data)
    {
        providerAvailable = isProviderAvailable;
        snapshots = data;
    }

    public string GetDisplayText(DateTime utcNow, bool compact)
    {
        var state = GetState(utcNow);
        if (state.ReadyCount > 0)
            return compact ? $"Ret: {state.ReadyCount} ready" : $"Retainers: {state.ReadyCount} ready";

        if (state.NextReturn is not { } next)
            return compact ? "Ret: none" : "Retainers: none";

        var remaining = RetainerTracker.FormatRemaining(next.VentureEndsAtUnixSeconds, new DateTimeOffset(utcNow), compact);
        return compact ? $"Ret: {remaining}" : $"Retainers: {remaining}";
    }

    public string GetTooltip(DateTime utcNow)
    {
        if (!providerAvailable)
            return "AutoRetainer data unavailable.";

        var state = GetState(utcNow);
        if (state.TotalCount == 0)
            return "No active retainer ventures found.";

        var lines = new List<string>
        {
            $"{state.ReadyCount} ready / {state.TotalCount} active",
        };
        lines.AddRange(state.Ready.Take(TooltipRowLimit).Select(snapshot =>
            $"Ready: {RetainerTracker.FormatOwner(snapshot)} - {snapshot.Name}"));

        var remainingRows = Math.Max(0, TooltipRowLimit - state.Ready.Count);
        lines.AddRange(state.Pending.Take(remainingRows).Select(snapshot =>
            $"{RetainerTracker.FormatOwner(snapshot)} - {snapshot.Name}: " +
            RetainerTracker.FormatRemaining(snapshot.VentureEndsAtUnixSeconds, new DateTimeOffset(utcNow), false)));

        if (state.Ready.Count + state.Pending.Count > TooltipRowLimit)
            lines.Add($"... and {state.Ready.Count + state.Pending.Count - TooltipRowLimit} more.");

        return string.Join("\n", lines);
    }

    private RetainerState GetState(DateTime utcNow)
        => RetainerTracker.GetState(snapshots, new DateTimeOffset(utcNow));
}
