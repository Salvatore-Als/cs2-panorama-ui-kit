using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using PanoramaUiKit.Shared.Api;
using PanoramaUiKit.Shared.Common;
using PanoramaUiKit.Shared.Roulette;

namespace PanoramaUiKit.Demo;

public sealed partial class DemoPlugin
{
    /// <summary>The prizes of the demo roulette: the rarer, the lower the weight.</summary>
    private static readonly RouletteItem[] DemoPrizes =
    [
        new("Common prize", "Weight 40", UiStyle.Neutral, 40f),
        new("Uncommon prize", "Weight 25", UiStyle.Info, 25f),
        new("Rare prize", "Weight 20", UiStyle.Success, 20f),
        new("Epic prize", "Weight 10", UiStyle.Purple, 10f),
        new("Legendary prize", "Weight 4", UiStyle.Warn, 4f),
        new("Mythic prize", "Weight 1", UiStyle.Gold, 1f),
    ];

    /// <summary>
    /// A case-opening roulette for the caller: six prizes with weights, 4 seconds, and a callback that
    /// runs when the strip has stopped. <c>fast</c>: 2 seconds. <c>slow</c>: 8 seconds.
    /// </summary>
    [ConsoleCommand("css_uikit_demo_roulette", "Roulette for the caller. Arguments: fast (2 seconds) | slow (8 seconds).")]
    [CommandHelper(usage: "[fast|slow]", whoCanExecute: CommandUsage.CLIENT_ONLY)]
    public void OnDemoRoulette(CCSPlayerController? player, CommandInfo command)
    {
        if (!TryKitFor(player, command, out IPanoramaUiKit kit, out CCSPlayerController target))
        {
            return;
        }

        // The speed of the strip is its duration: the distance it covers is fixed by the layout.
        string mode = command.GetArg(1).ToLowerInvariant();
        float seconds = 4f;
        float hold = 3f;
        if (mode == "fast")
        {
            seconds = 2f;
            hold = 1f;
        }
        else if (mode == "slow")
        {
            seconds = 8f;
        }

        kit.Roulettes.Spin(target, new RouletteOptions
        {
            Layout = DemoLayouts.Roulette,
            Kicker = "Case opening",
            Title = "Open the case",
            Items = DemoPrizes,
            Seconds = seconds,
            HoldSeconds = hold,
            Style = UiStyle.Gold,
            OnResult = result => OnDemoRouletteResult(kit, result),
        });
    }

    /// <summary>What a mode would do with the draw: give the item, log it, tell the player.</summary>
    private static void OnDemoRouletteResult(IPanoramaUiKit kit, RouletteResult result)
    {
        if (result.Interrupted)
        {
            return;
        }

        if (result.Player.IsValid)
        {
            kit.Toast(result.Player, DemoLayouts.Toast, result.Item.Name, "Drawn by the roulette.", result.Item.Style);
        }
    }
}
