using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Components;
using Dalamud.Interface.Windowing;
using Waitingway.Timers;

namespace Waitingway.Windows;

public sealed class ConfigurationWindow : Window
{
    private readonly Configuration configuration;
    private readonly Action save;
    private readonly Dictionary<Guid, string> timeInputs = [];

    public ConfigurationWindow(Configuration configuration, Action save)
        : base("Waitingway##configuration", ImGuiWindowFlags.NoCollapse)
    {
        this.configuration = configuration;
        this.save = save;
        Size = new Vector2(760, 460);
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

        ImGui.Separator();
        ImGui.TextUnformatted("Custom timer notifications");
        var chatNotificationsEnabled = configuration.ChatNotificationsEnabled;
        if (ImGui.Checkbox("Print completion in chat", ref chatNotificationsEnabled))
        {
            configuration.ChatNotificationsEnabled = chatNotificationsEnabled;
            save();
        }

        var toastNotificationsEnabled = configuration.ToastNotificationsEnabled;
        if (ImGui.Checkbox("Show Dalamud toast", ref toastNotificationsEnabled))
        {
            configuration.ToastNotificationsEnabled = toastNotificationsEnabled;
            save();
        }

        var popupNotificationsEnabled = configuration.PopupNotificationsEnabled;
        if (ImGui.Checkbox("Show popup window", ref popupNotificationsEnabled))
        {
            configuration.PopupNotificationsEnabled = popupNotificationsEnabled;
            save();
        }

        ImGui.Separator();
        ImGui.TextUnformatted("Custom timers");
        if (ImGui.Button("+ Add Timer"))
        {
            configuration.CustomTimers ??= [];
            configuration.CustomTimers.Add(new CustomTimerDefinition
            {
                Name = $"Timer {configuration.CustomTimers.Count + 1}",
            });
            save();
        }

        configuration.CustomTimers ??= [];
        var removeIndex = -1;
        var changed = false;
        var style = ImGui.GetStyle();
        var enableColumnWidth = ImGui.GetFrameHeight() + style.CellPadding.X * 2 + 6;
        var deleteButtonSize = new Vector2(ImGui.GetFrameHeight(), ImGui.GetFrameHeight());
        var deleteColumnWidth = deleteButtonSize.X + style.CellPadding.X * 2 + 6;
        if (ImGui.BeginTable("##customTimers", 5, ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg | ImGuiTableFlags.Resizable))
        {
            ImGui.TableSetupColumn("##enabled", ImGuiTableColumnFlags.WidthFixed, enableColumnWidth);
            ImGui.TableSetupColumn("Name", ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableSetupColumn("Time", ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableSetupColumn("Type", ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableSetupColumn("##delete", ImGuiTableColumnFlags.WidthFixed, deleteColumnWidth);
            ImGui.TableHeadersRow();

            for (var i = 0; i < configuration.CustomTimers.Count; i++)
            {
                var timer = configuration.CustomTimers[i];
                ImGui.PushID(timer.Id.ToString());
                ImGui.TableNextRow();

                ImGui.TableSetColumnIndex(0);
                var cellStart = ImGui.GetCursorPosX();
                ImGui.SetCursorPosX(cellStart + Math.Max(0, (enableColumnWidth - ImGui.GetFrameHeight()) / 2));
                var enabled = timer.Enabled;
                if (ImGui.Checkbox("##enabled", ref enabled))
                {
                    timer.Enabled = enabled;
                    changed = true;
                }

                ImGui.TableSetColumnIndex(1);
                ImGui.SetNextItemWidth(-1);
                var name = timer.Name;
                if (ImGui.InputText("##name", ref name, 128))
                {
                    timer.Name = name;
                    changed = true;
                }

                ImGui.TableSetColumnIndex(2);
                if (!timeInputs.TryGetValue(timer.Id, out var timeText))
                    timeText = CustomTimerTime.Format(timer.Hour, timer.Minute);

                ImGui.SetNextItemWidth(-1);
                var timeSubmitted = ImGui.InputText("##time", ref timeText, 5, ImGuiInputTextFlags.EnterReturnsTrue);
                timeInputs[timer.Id] = timeText;
                if (timeSubmitted)
                {
                    if (CustomTimerTime.TryParse(timeText, out var hour, out var minute))
                    {
                        timer.Hour = hour;
                        timer.Minute = minute;
                        timeInputs[timer.Id] = CustomTimerTime.Format(hour, minute);
                        changed = true;
                    }
                    else
                        timeInputs[timer.Id] = CustomTimerTime.Format(timer.Hour, timer.Minute);
                }

                ImGui.TableSetColumnIndex(3);
                var clockType = timer.ClockType.ToString();
                ImGui.SetNextItemWidth(-1);
                if (ImGui.BeginCombo("##clockType", clockType))
                {
                    foreach (var option in Enum.GetValues<CustomTimerClockType>())
                    {
                        var selected = timer.ClockType == option;
                        if (ImGui.Selectable(option.ToString(), selected))
                        {
                            timer.ClockType = option;
                            changed = true;
                        }

                        if (selected)
                            ImGui.SetItemDefaultFocus();
                    }

                    ImGui.EndCombo();
                }

                ImGui.TableSetColumnIndex(4);
                var deleteCellStart = ImGui.GetCursorPosX();
                ImGui.SetCursorPosX(deleteCellStart + (deleteColumnWidth - style.CellPadding.X * 2 - deleteButtonSize.X) / 2);
                if (ImGuiComponents.IconButton(FontAwesomeIcon.Trash, deleteButtonSize))
                    removeIndex = i;

                if (ImGui.IsItemHovered())
                    ImGui.SetTooltip("Delete timer");

                ImGui.PopID();
                if (removeIndex >= 0)
                    break;
            }

            ImGui.EndTable();
        }

        if (removeIndex >= 0)
        {
            timeInputs.Remove(configuration.CustomTimers[removeIndex].Id);
            configuration.CustomTimers.RemoveAt(removeIndex);
            changed = true;
        }

        if (changed)
            save();

    }
}
