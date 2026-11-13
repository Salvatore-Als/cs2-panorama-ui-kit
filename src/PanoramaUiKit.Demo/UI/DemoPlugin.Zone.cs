using System.Globalization;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using PanoramaUiKit.Shared.Api;
using PanoramaUiKit.Shared.Common;
using PanoramaUiKit.Shared.Zone;

namespace PanoramaUiKit.Demo;

public sealed partial class DemoPlugin
{
    private const int ZoneDemoWaitSeconds = 30;
    private const int ZoneDemoCloseSeconds = 20;

    /// <summary>Run of the zone demo per slot: a newer run (or "off") makes the pending phases of the old one do nothing.</summary>
    private readonly Dictionary<int, int> _zoneRuns = new();

    private readonly HashSet<int> _zoneOutside = [];

    private static readonly Dictionary<string, ZonePlacement> ZoneDemoPlacements = new(StringComparer.OrdinalIgnoreCase)
    {
        ["hud"] = ZonePlacement.UnderHud,
        ["top"] = ZonePlacement.Top,
        ["band"] = ZonePlacement.Band,
        ["bottom"] = ZonePlacement.Bottom,
        ["tl"] = ZonePlacement.TopLeft,
        ["tr"] = ZonePlacement.TopRight,
        ["bl"] = ZonePlacement.BottomLeft,
        ["br"] = ZonePlacement.BottomRight,
        ["right"] = ZonePlacement.Right,
    };

    /// <summary>
    /// The status alone: zone 1 waits [seconds] (30 by default), closes for 20 s, then the final zone.
    /// A placement word (hud, top, band, bottom, tl, tr, bl, br, right) moves it; "slot" uses the addon-provided
    /// icon slots. "alert" shows the alert alone, "out" toggles outside the safe zone, "off" hides everything.
    /// </summary>
    [ConsoleCommand("css_uikit_demo_zone", "Safe zone status (waiting, closing, final), or its alert alone.")]
    [CommandHelper(usage: "[seconds|alert|out|off] [hud|top|band|bottom|tl|tr|bl|br|right] [slot]", whoCanExecute: CommandUsage.CLIENT_ONLY)]
    public void OnDemoZone(CCSPlayerController? player, CommandInfo command)
    {
        if (!TryKitFor(player, command, out IPanoramaUiKit kit, out CCSPlayerController target))
        {
            return;
        }

        int slot = target.Slot;
        string arg = command.ArgCount > 1 ? command.GetArg(1).Trim().ToLowerInvariant() : string.Empty;
        if (arg == "off")
        {
            _zoneRuns[slot] = _zoneRuns.GetValueOrDefault(slot) + 1;
            _zoneOutside.Remove(slot);
            kit.Zone.Hide(target, DemoLayouts.Zone);
            return;
        }

        if (arg == "out")
        {
            bool outside = !_zoneOutside.Remove(slot);
            if (outside)
            {
                _zoneOutside.Add(slot);
            }

            command.ReplyToCommand($"[uikit] zone: outside = {outside}");
            kit.Zone.SetOutside(target, outside, DemoLayouts.Zone);
            return;
        }

        bool slots = HasArg(command, "slot");
        string? statusIcon = slots ? ZoneIcons.Slot(1) : null;
        if (arg == "alert")
        {
            kit.Zone.Alert(target, new ZoneAlertOptions
            {
                Layout = DemoLayouts.Zone,
                Kicker = "Danger",
                Title = "The zone is closing",
                Description = "Anyone left outside takes damage.",
                Style = UiStyle.Danger,
                Icon = slots ? ZoneIcons.Slot(2) : null,
            });
            return;
        }

        ZonePlacement placement = ZonePlacement.UnderHud;
        for (int i = 1; i < command.ArgCount; i++)
        {
            if (ZoneDemoPlacements.TryGetValue(command.GetArg(i).Trim(), out ZonePlacement named))
            {
                placement = named;
            }
        }

        int wait = ZoneDemoWaitSeconds;
        if (int.TryParse(arg, NumberStyles.Integer, CultureInfo.InvariantCulture, out int asked) && asked > 0)
        {
            wait = asked;
        }

        int run = _zoneRuns.GetValueOrDefault(slot) + 1;
        _zoneRuns[slot] = run;
        bool Current() => target.IsValid && _zoneRuns.GetValueOrDefault(slot) == run;

        command.ReplyToCommand($"[uikit] zone: waiting {wait} s, closing {ZoneDemoCloseSeconds} s, then final");
        kit.Zone.Show(target, new ZoneOptions
        {
            Layout = DemoLayouts.Zone,
            Phase = "Zone 1",
            State = ZoneState.Waiting,
            Seconds = wait,
            TotalSeconds = wait,
            Info = "Safe zone 240 m",
            Outside = _zoneOutside.Contains(slot),
            Placement = placement,
            Icon = statusIcon,
        });

        Later(wait, () =>
        {
            if (!Current())
            {
                return;
            }

            kit.Zone.Show(target, new ZoneOptions
            {
                Layout = DemoLayouts.Zone,
                Phase = "Zone 1",
                State = ZoneState.Closing,
                Seconds = ZoneDemoCloseSeconds,
                TotalSeconds = ZoneDemoCloseSeconds,
                Info = "Safe zone 180 m",
                Outside = _zoneOutside.Contains(slot),
                Placement = placement,
                Icon = statusIcon,
            });
        });

        Later(wait + ZoneDemoCloseSeconds, () =>
        {
            if (!Current())
            {
                return;
            }

            kit.Zone.Show(target, new ZoneOptions
            {
                Layout = DemoLayouts.Zone,
                Phase = "Zone 2",
                State = ZoneState.Final,
                Info = "12 alive",
                Outside = _zoneOutside.Contains(slot),
                Placement = placement,
                Icon = statusIcon,
            });
        });
    }
}
