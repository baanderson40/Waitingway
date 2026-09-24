using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;

namespace Waitingway.Windows;

public sealed class TimerNotificationWindow : Window
{
    private readonly List<string> completedTimers = [];

    public TimerNotificationWindow()
        : base("Waitingway Timer Complete##timerNotification", ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoCollapse)
    {
        SizeCondition = ImGuiCond.Always;
    }

    public void ShowCompleted(IEnumerable<string> timerNames)
    {
        completedTimers.AddRange(timerNames);
        IsOpen = true;
    }

    public override void Draw()
    {
        ImGui.TextUnformatted("Timer complete");
        ImGui.Separator();

        foreach (var timerName in completedTimers)
            ImGui.BulletText(timerName);

        if (ImGui.Button("Dismiss"))
        {
            completedTimers.Clear();
            IsOpen = false;
        }
    }
}
