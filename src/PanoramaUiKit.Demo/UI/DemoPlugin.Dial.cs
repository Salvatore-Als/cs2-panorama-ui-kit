using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using PanoramaUiKit.Shared.Api;
using PanoramaUiKit.Shared.Common;
using PanoramaUiKit.Shared.Dials;
using PanoramaUiKit.Shared.Progress;

namespace PanoramaUiKit.Demo;

public sealed partial class DemoPlugin
{
    private const int DialDemoSeconds = 10;

    /// <summary>
    /// Timed dials: a cooldown draining around the crosshair, a charge filling at the bottom left. And a
    /// pushed one at the bottom right, written every second. One size each.
    /// </summary>
    [ConsoleCommand("css_uikit_demo_dial", "Radial dials: timed around the crosshair and bottom left, pushed bottom right.")]
    [CommandHelper(whoCanExecute: CommandUsage.CLIENT_ONLY)]
    public void OnDemoDial(CCSPlayerController? player, CommandInfo command)
    {
        if (!TryKitFor(player, command, out IPanoramaUiKit kit, out CCSPlayerController target))
        {
            return;
        }

        kit.Dial(target, DemoLayouts.Dial, "Dash", seconds: 8f, size: DialSize.Large, style: UiStyle.Warn);
        kit.Dial(target, DemoLayouts.Dial, "Charge", seconds: 6f, DialPlacement.BottomLeft, ProgressDirection.Fill, DialSize.Medium, UiStyle.Success);

        int tick = 0;
        RunEverySecond($"dial:{target.Slot}", () =>
        {
            if (!target.IsValid)
            {
                return false;
            }

            if (tick > DialDemoSeconds)
            {
                kit.Dials.Hide(target, DialPlacement.BottomRight, DemoLayouts.Dial);
                return false;
            }

            kit.Dials.Show(target, new DialOptions
            {
                Layout = DemoLayouts.Dial,
                Placement = DialPlacement.BottomRight,
                Label = "Shield",
                Value = 1f - tick / (float)DialDemoSeconds,
                ShowPercent = true,
                Size = DialSize.Small,
                Style = UiStyle.Info,
            });
            tick++;
            return true;
        });
    }
}
