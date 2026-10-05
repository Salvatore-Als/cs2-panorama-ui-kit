using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using PanoramaUiKit.Shared.Api;
using PanoramaUiKit.Shared.Common;
using PanoramaUiKit.Shared.Victory;

namespace PanoramaUiKit.Demo;

public sealed partial class DemoPlugin
{
    /// <summary>
    /// The victory screen for the caller: chips and an 8 second restart countdown ticked by the kit.
    /// Per player, so a real mode would show each side its own result.
    /// </summary>
    [ConsoleCommand("css_uikit_demo_victory", "Victory screen for the caller, 8 s restart countdown.")]
    [CommandHelper(whoCanExecute: CommandUsage.CLIENT_ONLY)]
    public void OnDemoVictory(CCSPlayerController? player, CommandInfo command)
    {
        if (!TryKitFor(player, command, out IPanoramaUiKit kit, out CCSPlayerController target))
        {
            return;
        }

        kit.Victory.Show(target, new VictoryOptions
        {
            Layout = DemoLayouts.Victory,
            Kicker = "Round over",
            Title = "Team A wins",
            Subtitle = "Victory subtitle: why the round ended.",
            Style = UiStyle.Info,
            Chips =
            [
                new VictoryChip("MVP", "Player A", UiStyle.Gold),
                new VictoryChip("Most kills", "Player B", UiStyle.Danger),
                new VictoryChip("Most damage", "Player C", UiStyle.Info),
            ],
            RestartSeconds = 8,
            RestartLabel = "Next round in",
            Seconds = 9f,
        });
    }
}
