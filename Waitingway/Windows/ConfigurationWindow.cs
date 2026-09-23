using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;

namespace Waitingway.Windows;

public sealed class ConfigurationWindow : Window
{
    private readonly Configuration configuration;
    private readonly Action save;

    public ConfigurationWindow(Configuration configuration, Action save)
        : base("Waitingway##configuration", ImGuiWindowFlags.NoCollapse)
    {
        this.configuration = configuration;
        this.save = save;
        Size = new Vector2(420, 250);
        SizeCondition = ImGuiCond.FirstUseEver;
    }

    public override void Draw()
    {
        ImGui.TextUnformatted("Timer display");
        ImGui.Separator();

        var oceanFishingEnabled = configuration.OceanFishingEnabled;
        if (ImGui.Checkbox("Show Ocean Fishing timer", ref oceanFishingEnabled))
        {
            configuration.OceanFishingEnabled = oceanFishingEnabled;
            save();
        }

        var deployablesEnabled = configuration.DeployablesEnabled;
        if (ImGui.Checkbox("Show deployables timer", ref deployablesEnabled))
        {
            configuration.DeployablesEnabled = deployablesEnabled;
            save();
        }

        var retainersEnabled = configuration.RetainersEnabled;
        if (ImGui.Checkbox("Show retainer ventures timer", ref retainersEnabled))
        {
            configuration.RetainersEnabled = retainersEnabled;
            save();
        }

        var compactText = configuration.CompactText;
        if (ImGui.Checkbox("Use compact server bar text", ref compactText))
        {
            configuration.CompactText = compactText;
            save();
        }

        ImGui.Spacing();
        ImGui.TextWrapped("Ocean Fishing registration opens every two hours at even server-time hours and remains open for 15 minutes. Times in the tooltip use your computer's local time zone.");
        ImGui.TextWrapped("Deployables use AutoRetainer's optional IPC data and include all airships and submersibles with a recorded return time.");
        ImGui.TextWrapped("Retainers include every active venture reported by AutoRetainer. Both timers hide when AutoRetainer is unavailable or no matching data exists.");
        ImGui.TextWrapped("Click the server bar entry to open these settings.");
    }
}
