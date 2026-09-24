namespace Waitingway.Timers;

public enum CustomTimerClockType
{
    Local,
    Eorzea,
}

[Serializable]
public sealed class CustomTimerDefinition
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public bool Enabled { get; set; } = true;

    public string Name { get; set; } = "New Timer";

    public int Hour { get; set; }

    public int Minute { get; set; }

    public CustomTimerClockType ClockType { get; set; } = CustomTimerClockType.Local;
}

public readonly record struct CustomTimerState(
    DateTimeOffset NextOccurrence,
    TimeSpan Remaining,
    bool IsNow);
