using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using PanoramaUiKit.Shared.Api;
using PanoramaUiKit.Shared.Cards;
using PanoramaUiKit.Shared.Common;

namespace PanoramaUiKit.Demo;

public sealed partial class DemoPlugin
{
    private const int CardDemoSeconds = 10;

    /// <summary>
    /// One card per corner, each a different mix: full, title only, description only, lines only.
    /// The top left one counts a line up every second: same card, rewritten in place.
    /// </summary>
    [ConsoleCommand("css_uikit_demo_card", "Four cards, one per corner, each with a different set of parts.")]
    [CommandHelper(whoCanExecute: CommandUsage.CLIENT_ONLY)]
    public void OnDemoCard(CCSPlayerController? player, CommandInfo command)
    {
        if (!TryKitFor(player, command, out IPanoramaUiKit kit, out CCSPlayerController target))
        {
            return;
        }

        kit.Cards.Show(target, new CardOptions
        {
            Layout = DemoLayouts.Card,
            Placement = CardPlacement.TopRight,
            Title = "Only a title",
            Style = UiStyle.Info,
            Seconds = CardDemoSeconds,
        });

        kit.Cards.Show(target, new CardOptions
        {
            Layout = DemoLayouts.Card,
            Placement = CardPlacement.BottomLeft,
            Description = "Only a description. It wraps on several lines when it is long enough to need them.",
            Seconds = CardDemoSeconds,
        });

        kit.Cards.Show(target, new CardOptions
        {
            Layout = DemoLayouts.Card,
            Placement = CardPlacement.BottomRight,
            Lines =
            [
                new CardLine("Kills", "12"),
                new CardLine("Deaths", "4"),
                new CardLine("Only a label"),
                new CardLine(null, "Only a value"),
            ],
            Style = UiStyle.Success,
            Seconds = CardDemoSeconds,
        });

        int tick = 0;
        RunEverySecond($"card:{target.Slot}", () =>
        {
            if (!target.IsValid)
            {
                return false;
            }

            if (tick >= CardDemoSeconds)
            {
                kit.Cards.Hide(target, CardPlacement.TopLeft, DemoLayouts.Card);
                return false;
            }

            kit.Cards.Show(target, new CardOptions
            {
                Layout = DemoLayouts.Card,
                Placement = CardPlacement.TopLeft,
                Title = "Objective",
                Description = "Hold the zone until the timer ends.",
                Lines = [new CardLine("Time left", $"{CardDemoSeconds - tick}s"), new CardLine("Captured", $"{tick * 10}%")],
                Style = UiStyle.Warn,
            });
            tick++;
            return true;
        });
    }
}
