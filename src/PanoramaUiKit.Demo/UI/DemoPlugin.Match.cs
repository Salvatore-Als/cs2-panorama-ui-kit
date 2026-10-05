using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;
using PanoramaUiKit.Shared.Api;
using PanoramaUiKit.Shared.Common;
using PanoramaUiKit.Shared.Match;

namespace PanoramaUiKit.Demo;

public sealed partial class DemoPlugin
{
    private const int MatchDemoSeconds = 40;
    private const int MatchUrgentSeconds = 10;

    /// <summary>
    /// The match bar for everyone: T and CT alive counts (read live) with their titles, a 40 second
    /// countdown that turns urgent under 10, and the two team scores in tabs under the sides.
    /// <c>noscore</c> hides the score tabs.
    /// </summary>
    [ConsoleCommand("css_uikit_demo_match", "Match bar: T / CT alive, team scores, 40 s countdown. Argument 'noscore' hides the scores.")]
    [CommandHelper(usage: "[noscore]", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
    public void OnDemoMatch(CCSPlayerController? player, CommandInfo command)
    {
        if (!TryKit(command, out IPanoramaUiKit kit))
        {
            return;
        }

        bool showScore = !command.GetArg(1).Equals("noscore", StringComparison.OrdinalIgnoreCase);
        int left = MatchDemoSeconds;
        RunEverySecond("match", () =>
        {
            if (left < 0)
            {
                kit.Match.Hide(DemoLayouts.Match);
                return false;
            }

            bool urgent = left <= MatchUrgentSeconds;
            string? centerSub = "Live";
            if (urgent)
            {
                centerSub = "Last seconds";
            }

            kit.Match.Update(new MatchOptions
            {
                Layout = DemoLayouts.Match,
                LeftLabel = "T",
                LeftValue = AliveCount(CsTeam.Terrorist).ToString(),
                LeftStyle = UiStyle.Gold,
                LeftScore = TeamScore(CsTeam.Terrorist),
                Center = MatchOptions.FormatTime(left),
                CenterSub = centerSub,
                RightLabel = "CT",
                RightValue = AliveCount(CsTeam.CounterTerrorist).ToString(),
                RightStyle = UiStyle.Info,
                RightScore = TeamScore(CsTeam.CounterTerrorist),
                ShowScore = showScore,
                Urgent = urgent,
            });
            left--;
            return true;
        });
    }

    private static int AliveCount(CsTeam team)
    {
        return Utilities.GetPlayers().Count(p => p is { IsValid: true, PawnIsAlive: true } && p.Team == team);
    }

    /// <summary>The team's round score, read from the game's team manager.</summary>
    private static int TeamScore(CsTeam team)
    {
        foreach (CCSTeam manager in Utilities.FindAllEntitiesByDesignerName<CCSTeam>("cs_team_manager"))
        {
            if (manager.TeamNum == (byte)team)
            {
                return manager.Score;
            }
        }

        return 0;
    }
}
