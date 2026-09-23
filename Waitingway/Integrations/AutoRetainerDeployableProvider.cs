using Dalamud.Plugin.Ipc;
using Dalamud.Plugin.Ipc.Exceptions;
using Dalamud.Plugin;
using Waitingway.Timers;

namespace Waitingway.Integrations;

public sealed class AutoRetainerProvider : IDisposable
{
    private static readonly TimeSpan RefreshInterval = TimeSpan.FromSeconds(5);

    private readonly ICallGateSubscriber<List<ulong>> registeredCharacterIds;
    private readonly ICallGateSubscriber<ulong, AutoRetainerCharacterData> characterData;
    private DateTime nextRefreshUtc = DateTime.MinValue;

    public AutoRetainerProvider(IDalamudPluginInterface pluginInterface)
    {
        registeredCharacterIds = pluginInterface.GetIpcSubscriber<List<ulong>>("AutoRetainer.GetRegisteredCIDs");
        characterData = pluginInterface.GetIpcSubscriber<ulong, AutoRetainerCharacterData>("AutoRetainer.GetOfflineCharacterData");
    }

    public bool IsAvailable { get; private set; }

    public IReadOnlyList<DeployableSnapshot> Snapshots { get; private set; } = Array.Empty<DeployableSnapshot>();

    public IReadOnlyList<RetainerSnapshot> RetainerSnapshots { get; private set; } = Array.Empty<RetainerSnapshot>();

    public void Update(DateTime utcNow)
    {
        if (utcNow < nextRefreshUtc)
            return;

        nextRefreshUtc = utcNow + RefreshInterval;
        try
        {
            var snapshots = new List<DeployableSnapshot>();
            var retainerSnapshots = new List<RetainerSnapshot>();
            foreach (var cid in registeredCharacterIds.InvokeFunc())
            {
                var data = characterData.InvokeFunc(cid);
                if (data is null)
                    continue;

                AddSnapshots(snapshots, data, data.OfflineAirshipData, DeployableType.Airship);
                AddSnapshots(snapshots, data, data.OfflineSubmarineData, DeployableType.Submersible);
                AddRetainerSnapshots(retainerSnapshots, data, data.RetainerData);
            }

            Snapshots = snapshots;
            RetainerSnapshots = retainerSnapshots;
            IsAvailable = true;
        }
        catch (IpcError)
        {
            Snapshots = Array.Empty<DeployableSnapshot>();
            RetainerSnapshots = Array.Empty<RetainerSnapshot>();
            IsAvailable = false;
        }
    }

    public void Dispose()
    {
        Snapshots = Array.Empty<DeployableSnapshot>();
        RetainerSnapshots = Array.Empty<RetainerSnapshot>();
        IsAvailable = false;
    }

    private static void AddSnapshots(
        ICollection<DeployableSnapshot> destination,
        AutoRetainerCharacterData character,
        IEnumerable<AutoRetainerVesselData> vessels,
        DeployableType type)
    {
        foreach (var vessel in vessels ?? [])
        {
            if (string.IsNullOrWhiteSpace(vessel.Name))
                continue;

            destination.Add(new(
                character.Name,
                character.World,
                vessel.Name,
                type,
                vessel.ReturnTime));
        }
    }

    private static void AddRetainerSnapshots(
        ICollection<RetainerSnapshot> destination,
        AutoRetainerCharacterData character,
        IEnumerable<AutoRetainerRetainerData> retainers)
    {
        foreach (var retainer in retainers ?? [])
        {
            if (!retainer.HasVenture || string.IsNullOrWhiteSpace(retainer.Name) || retainer.VentureEndsAt <= 0)
                continue;

            destination.Add(new(character.Name, character.World, retainer.Name, retainer.VentureEndsAt));
        }
    }
}

// Local DTOs intentionally contain only fields needed by Waitingway. Dalamud serializes IPC results by field name.
public sealed class AutoRetainerCharacterData
{
    public string Name = string.Empty;
    public string World = string.Empty;
    public List<AutoRetainerVesselData> OfflineAirshipData = [];
    public List<AutoRetainerVesselData> OfflineSubmarineData = [];
    public List<AutoRetainerRetainerData> RetainerData = [];
}

public sealed class AutoRetainerRetainerData
{
    public string Name = string.Empty;
    public bool HasVenture;
    public long VentureEndsAt;
}

public sealed class AutoRetainerVesselData
{
    public string Name = string.Empty;
    public uint ReturnTime;
}
