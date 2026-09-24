using Dalamud.Configuration;
using Waitingway.Timers;

namespace Waitingway;

[Serializable]
public sealed class Configuration : IPluginConfiguration
{
    public int Version { get; set; }

    public bool OceanFishingEnabled { get; set; } = true;

    public bool DeployablesEnabled { get; set; } = true;

    public bool RetainersEnabled { get; set; } = true;

    public bool CompactText { get; set; }

    public bool ChatNotificationsEnabled { get; set; }

    public bool ToastNotificationsEnabled { get; set; }

    public bool PopupNotificationsEnabled { get; set; }

    public List<CustomTimerDefinition> CustomTimers { get; set; } = [];
}
