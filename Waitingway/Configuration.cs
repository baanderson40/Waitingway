using Dalamud.Configuration;

namespace Waitingway;

[Serializable]
public sealed class Configuration : IPluginConfiguration
{
    public int Version { get; set; }

    public bool OceanFishingEnabled { get; set; } = true;

    public bool DeployablesEnabled { get; set; } = true;

    public bool RetainersEnabled { get; set; } = true;

    public bool CompactText { get; set; }
}
