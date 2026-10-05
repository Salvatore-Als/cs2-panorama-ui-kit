using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using PanoramaUiKit.Shared.Api;
using PanoramaUiKit.Shared.Common;
using PanoramaUiKit.Shared.KillFeed;

namespace PanoramaUiKit.Demo;

public sealed partial class DemoPlugin
{
    /// <summary>
    /// Five rows pushed one after the other: with and without an attacker (a feed that never names
    /// the killer just leaves it out), one style per row.
    /// </summary>
    [ConsoleCommand("css_uikit_demo_killfeed", "Pushes five kill feed rows.")]
    [CommandHelper(whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
    public void OnDemoKillFeed(CCSPlayerController? player, CommandInfo command)
    {
        if (!TryKit(command, out IPanoramaUiKit kit))
        {
            return;
        }

        KillFeedEntry[] entries =
        [
            new() { Layout = DemoLayouts.KillFeed, Attacker = "Player A", Tag = "AK-47", Victim = "Player B", Style = UiStyle.Danger },
            new() { Layout = DemoLayouts.KillFeed, Attacker = "Player C", Tag = "Headshot", Victim = "Player A", Style = UiStyle.Warn },
            new() { Layout = DemoLayouts.KillFeed, Tag = "Eliminated", Victim = "Player D", Style = UiStyle.Neutral },
            new() { Layout = DemoLayouts.KillFeed, Attacker = "Player E", Tag = "Grenade", Victim = "Player F", Style = UiStyle.Purple },
            new() { Layout = DemoLayouts.KillFeed, Tag = "Respawned", Victim = "Player B", Style = UiStyle.Success },
        ];

        for (int i = 0; i < entries.Length; i++)
        {
            KillFeedEntry entry = entries[i];
            Later(0.6f * i + 0.01f, () => kit.KillFeed.Push(entry));
        }
    }
}
