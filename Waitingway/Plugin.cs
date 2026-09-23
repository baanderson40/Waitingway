using Dalamud.Game.Command;
using Dalamud.Game.Gui.Dtr;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using Waitingway.Integrations;
using Waitingway.Timers;
using Waitingway.Windows;

namespace Waitingway;

public sealed class Plugin : IDalamudPlugin
{
    private const string CommandName = "/waitingway";

    [PluginService]
    internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;

    [PluginService]
    internal static ICommandManager CommandManager { get; private set; } = null!;

    [PluginService]
    internal static IDtrBar DtrBar { get; private set; } = null!;

    [PluginService]
    internal static IFramework Framework { get; private set; } = null!;

    [PluginService]
    internal static IPluginLog Log { get; private set; } = null!;

    private readonly Configuration configuration;
    private readonly WindowSystem windowSystem = new("Waitingway");
    private readonly ConfigurationWindow configurationWindow;
    private readonly IDtrBarEntry oceanFishingEntry;
    private readonly IDtrBarEntry deployableEntry;
    private readonly IDtrBarEntry retainerEntry;
    private readonly IWaitingwayTimer oceanFishingTimer = new OceanFishingTimer();
    private readonly DeployableTimer deployableTimer = new();
    private readonly RetainerTimer retainerTimer = new();
    private readonly AutoRetainerProvider autoRetainerProvider;
    private DateTime lastDisplayUpdateUtc = DateTime.MinValue;

    public Plugin()
    {
        configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
        autoRetainerProvider = new AutoRetainerProvider(PluginInterface);
        configurationWindow = new ConfigurationWindow(configuration, SaveConfiguration);
        windowSystem.AddWindow(configurationWindow);

        oceanFishingEntry = DtrBar.Get(oceanFishingTimer.Title);
        oceanFishingEntry.OnClick = _ => configurationWindow.Toggle();
        deployableEntry = DtrBar.Get(deployableTimer.Title);
        deployableEntry.OnClick = _ => configurationWindow.Toggle();
        retainerEntry = DtrBar.Get(retainerTimer.Title);
        retainerEntry.OnClick = _ => configurationWindow.Toggle();

        CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = "Open settings, or use '/waitingway toggle [deployables|retainers]'.",
            ShowInHelp = true,
        });

        PluginInterface.UiBuilder.Draw += windowSystem.Draw;
        PluginInterface.UiBuilder.OpenMainUi += configurationWindow.Toggle;
        PluginInterface.UiBuilder.OpenConfigUi += configurationWindow.Toggle;
        Framework.Update += OnFrameworkUpdate;

        UpdateDisplay(DateTime.UtcNow);
    }

    public string Name => "Waitingway";

    public void Dispose()
    {
        Framework.Update -= OnFrameworkUpdate;
        PluginInterface.UiBuilder.Draw -= windowSystem.Draw;
        PluginInterface.UiBuilder.OpenMainUi -= configurationWindow.Toggle;
        PluginInterface.UiBuilder.OpenConfigUi -= configurationWindow.Toggle;
        CommandManager.RemoveHandler(CommandName);
        oceanFishingEntry.Remove();
        deployableEntry.Remove();
        retainerEntry.Remove();
        autoRetainerProvider.Dispose();
        windowSystem.RemoveAllWindows();
    }

    private void OnCommand(string command, string arguments)
    {
        var toggleTarget = arguments.Trim();
        if (string.Equals(toggleTarget, "toggle", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(toggleTarget, "toggle ocean", StringComparison.OrdinalIgnoreCase))
        {
            configuration.OceanFishingEnabled = !configuration.OceanFishingEnabled;
            SaveConfiguration();
            UpdateDisplay(DateTime.UtcNow);
            return;
        }

        if (string.Equals(toggleTarget, "toggle deployables", StringComparison.OrdinalIgnoreCase))
        {
            configuration.DeployablesEnabled = !configuration.DeployablesEnabled;
            SaveConfiguration();
            UpdateDisplay(DateTime.UtcNow);
            return;
        }

        if (string.Equals(toggleTarget, "toggle retainers", StringComparison.OrdinalIgnoreCase))
        {
            configuration.RetainersEnabled = !configuration.RetainersEnabled;
            SaveConfiguration();
            UpdateDisplay(DateTime.UtcNow);
            return;
        }

        configurationWindow.Toggle();
    }

    private void OnFrameworkUpdate(IFramework framework)
    {
        var now = DateTime.UtcNow;
        if (now - lastDisplayUpdateUtc < TimeSpan.FromSeconds(1))
            return;

        UpdateDisplay(now);
    }

    private void UpdateDisplay(DateTime utcNow)
    {
        lastDisplayUpdateUtc = utcNow;
        autoRetainerProvider.Update(utcNow);
        deployableTimer.SetData(autoRetainerProvider.IsAvailable, autoRetainerProvider.Snapshots);
        retainerTimer.SetData(autoRetainerProvider.IsAvailable, autoRetainerProvider.RetainerSnapshots);

        oceanFishingEntry.Shown = configuration.OceanFishingEnabled;
        deployableEntry.Shown = configuration.DeployablesEnabled && deployableTimer.IsAvailable;
        retainerEntry.Shown = configuration.RetainersEnabled && retainerTimer.IsAvailable;

        if (oceanFishingEntry.Shown)
        {
            oceanFishingEntry.Text = new SeString(new TextPayload(oceanFishingTimer.GetDisplayText(utcNow, configuration.CompactText)));
            oceanFishingEntry.Tooltip = new SeString(new TextPayload(oceanFishingTimer.GetTooltip(utcNow)));
        }

        if (deployableEntry.Shown)
        {
            deployableEntry.Text = new SeString(new TextPayload(deployableTimer.GetDisplayText(utcNow, configuration.CompactText)));
            deployableEntry.Tooltip = new SeString(new TextPayload(deployableTimer.GetTooltip(utcNow)));
        }

        if (retainerEntry.Shown)
        {
            retainerEntry.Text = new SeString(new TextPayload(retainerTimer.GetDisplayText(utcNow, configuration.CompactText)));
            retainerEntry.Tooltip = new SeString(new TextPayload(retainerTimer.GetTooltip(utcNow)));
        }
    }

    private void SaveConfiguration()
    {
        PluginInterface.SavePluginConfig(configuration);
        Log.Debug("Waitingway configuration saved.");
    }
}
