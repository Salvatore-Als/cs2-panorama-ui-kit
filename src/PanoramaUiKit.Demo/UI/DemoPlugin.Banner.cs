using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using PanoramaUiKit.Shared.Api;
using PanoramaUiKit.Shared.Banners;
using PanoramaUiKit.Shared.Common;

namespace PanoramaUiKit.Demo;

public sealed partial class DemoPlugin
{
    /// <summary>
    /// A banner for everyone, rewritten every second as a countdown: the entry plays once, then only
    /// the text and style change.
    /// </summary>
    [ConsoleCommand("css_uikit_demo_banner", "Banner for everyone with a countdown rewritten in place.")]
    [CommandHelper(usage: "[text]", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
    public void OnDemoBanner(CCSPlayerController? player, CommandInfo command)
    {
        if (!TryKit(command, out IPanoramaUiKit kit))
        {
            return;
        }

        string text = "The round starts";
        if (command.ArgCount > 1)
        {
            text = command.ArgString;
        }

        int left = 6;
        RunEverySecond("banner", () =>
        {
            if (left <= 0)
            {
                kit.Banner(DemoLayouts.Banner, "Go go go!", UiStyle.Danger, seconds: 2f);
                return false;
            }

            string style = UiStyle.Info;
            if (left <= 3)
            {
                style = UiStyle.Warn;
            }

            kit.Banners.ShowAll(new BannerOptions
            {
                Layout = DemoLayouts.Banner,
                Kicker = "Warmup",
                Text = $"{text} in {left}…",
                Style = style,
            });
            left--;
            return true;
        });
    }
}
