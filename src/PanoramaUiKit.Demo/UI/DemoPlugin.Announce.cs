using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using PanoramaUiKit.Shared.Announcements;
using PanoramaUiKit.Shared.Api;
using PanoramaUiKit.Shared.Common;

namespace PanoramaUiKit.Demo;

public sealed partial class DemoPlugin
{
    /// <summary>
    /// A first card, then a second one that replaces it (exit, then entry) without the title
    /// animation.
    /// </summary>
    [ConsoleCommand("css_uikit_demo_announce", "Two announce cards, the second without title animation.")]
    [CommandHelper(whoCanExecute: CommandUsage.CLIENT_ONLY)]
    public void OnDemoAnnounce(CCSPlayerController? player, CommandInfo command)
    {
        if (!TryKitFor(player, command, out IPanoramaUiKit kit, out CCSPlayerController target))
        {
            return;
        }

        kit.Announce(target, DemoLayouts.Announce, "Announce title", "Announce description, under the title.", UiStyle.Danger,
                     seconds: 4f, kicker: "Kicker");

        Later(4.6f, () =>
        {
            if (!target.IsValid)
            {
                return;
            }

            kit.Announcements.Show(target, new AnnounceOptions
            {
                Layout = DemoLayouts.Announce,
                Kicker = "Second card",
                Title = "Replaces the first",
                Description = "No title animation this time (AnimateTitle = false).",
                Style = UiStyle.Info,
                AnimateTitle = false,
                Seconds = 4f,
            });
        });
    }
}
