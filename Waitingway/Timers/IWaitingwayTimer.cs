namespace Waitingway.Timers;

public interface IWaitingwayTimer
{
    string Id { get; }

    string Title { get; }

    bool IsAvailable { get; }

    string GetDisplayText(DateTime utcNow, bool compact);

    string GetTooltip(DateTime utcNow);
}
