using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using PanoramaUiKit.Shared.Api;
using PanoramaUiKit.Shared.Common;
using PanoramaUiKit.Shared.Recap;

namespace PanoramaUiKit.Demo;

public sealed partial class DemoPlugin
{
    private const int RecapDemoKills = 7;

    /// <summary>
    /// The end of match screen for the caller, won or lost, with seven stats. Three seconds in, the kills
    /// tile goes up by one to show the pulse of a value updated while shown.
    /// </summary>
    [ConsoleCommand("css_uikit_demo_recap", "End of match screen with stats; kills +1 after 3 s.")]
    [CommandHelper(usage: "[win|lose|duo]", whoCanExecute: CommandUsage.CLIENT_ONLY)]
    public void OnDemoRecap(CCSPlayerController? player, CommandInfo command)
    {
        if (!TryKitFor(player, command, out IPanoramaUiKit kit, out CCSPlayerController target))
        {
            return;
        }

        if (HasArg(command, "duo"))
        {
            // A duo won: one row per player instead of the single name and stats line.
            kit.Recap.Show(target, new RecapOptions
            {
                Layout = DemoLayouts.Recap,
                Kicker = "Match over",
                Title = "Last ones standing",
                Place = 1,
                Total = 64,
                Style = UiStyle.Gold,
                Players =
                [
                    new RecapPlayer("Player 1", [RecapStats.Kills(7, best: true), RecapStats.Points(1240), RecapStats.DamageDealt(1873)], Style: UiStyle.Gold),
                    new RecapPlayer("Player 2", [RecapStats.Kills(5), RecapStats.Points(980), RecapStats.DamageDealt(1320)]),
                ],
                RestartSeconds = 15,
                Seconds = 16f,
            });
            return;
        }

        bool won = command.ArgCount < 2 || !command.GetArg(1).Equals("lose", StringComparison.OrdinalIgnoreCase);

        kit.Recap.Show(target, new RecapOptions
        {
            Layout = DemoLayouts.Recap,
            Kicker = won ? "Match over" : "You were eliminated",
            Title = won ? "Last one standing" : "Eliminated",
            Place = won ? 1 : 12,
            Total = 64,
            PlayerSub = "Squad Alpha",
            Style = won ? UiStyle.Gold : UiStyle.Danger,
            Stats =
            [
                RecapStats.Kills(RecapDemoKills, best: true),
                RecapStats.Knockouts(3),
                RecapStats.Points(1250),
                RecapStats.Shots(412),
                RecapStats.DamageDealt(1873),
                RecapStats.DamageTaken(640),
                RecapStats.Survival(new TimeSpan(0, 18, 42)),
            ],
            RestartSeconds = 15,
            Seconds = 16f,
        });

        Later(3f, () =>
        {
            if (target is { IsValid: true })
            {
                kit.Recap.UpdateStat(target, 0, (RecapDemoKills + 1).ToString(), DemoLayouts.Recap);
            }
        });
    }
}
