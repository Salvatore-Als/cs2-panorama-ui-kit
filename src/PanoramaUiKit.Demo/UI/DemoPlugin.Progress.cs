using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using PanoramaUiKit.Shared.Api;
using PanoramaUiKit.Shared.Common;
using PanoramaUiKit.Shared.Progress;

namespace PanoramaUiKit.Demo;

public sealed partial class DemoPlugin
{
    private const int ProgressDemoSeconds = 10;

    /// <summary>
    /// Two kinds of bar at once. Under the crosshair, a timed one: one call, the client runs it alone.
    /// At the top, a pushed one: the value is written every second and the bar eases to it.
    /// </summary>
    [ConsoleCommand("css_uikit_demo_progress", "A timed bar under the crosshair and a pushed one at the top.")]
    [CommandHelper(whoCanExecute: CommandUsage.CLIENT_ONLY)]
    public void OnDemoProgress(CCSPlayerController? player, CommandInfo command)
    {
        if (!TryKitFor(player, command, out IPanoramaUiKit kit, out CCSPlayerController target))
        {
            return;
        }

        kit.Progress(target, DemoLayouts.Progress, "Defusing", seconds: 5f, style: UiStyle.Warn);

        kit.ProgressBars.Show(target, new ProgressOptions
        {
            Layout = DemoLayouts.Progress,
            Placement = ProgressPlacement.Bottom,
            Label = "Time left",
            Seconds = 8f,
            Direction = ProgressDirection.Drain,
            ValueText = "8s",
            Style = UiStyle.Danger,
        });

        int tick = 0;
        RunEverySecond($"progress:{target.Slot}", () =>
        {
            if (!target.IsValid)
            {
                return false;
            }

            if (tick > ProgressDemoSeconds)
            {
                kit.ProgressBars.Hide(target, ProgressPlacement.Top, DemoLayouts.Progress);
                return false;
            }

            kit.ProgressBars.Show(target, new ProgressOptions
            {
                Layout = DemoLayouts.Progress,
                Placement = ProgressPlacement.Top,
                Label = "Zone captured",
                Value = tick / (float)ProgressDemoSeconds,
                ShowPercent = true,
                Style = UiStyle.Info,
            });
            tick++;
            return true;
        });
    }
}
