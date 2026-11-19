using System.Globalization;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using PanoramaUiKit.Shared.Api;
using PanoramaUiKit.Shared.Overlays;

namespace PanoramaUiKit.Demo;

public sealed partial class DemoPlugin
{
    private int _overlayDemoIndex = -1;

    /// <summary>
    /// A full-screen effect: a name (frozen7, burn3, poison, custom1, debug1...), "next" / "prev" to walk through the
    /// bundled ones, "off" to take it off. Optional seconds.
    /// </summary>
    [ConsoleCommand("css_uikit_demo_overlay", "Full-screen overlay effect: frozen1..20, burn1..20, zombie1..3, knock1, poison, custom1..8, debug1/2...")]
    [CommandHelper(usage: "<effect|next|prev|off> [seconds]", whoCanExecute: CommandUsage.CLIENT_ONLY)]
    public void OnDemoOverlay(CCSPlayerController? player, CommandInfo command)
    {
        if (!TryKitFor(player, command, out IPanoramaUiKit kit, out CCSPlayerController target))
        {
            return;
        }

        string effect = command.ArgCount > 1 ? command.GetArg(1).Trim().ToLowerInvariant() : "next";
        if (effect == "off")
        {
            kit.Overlays.Hide(target, DemoLayouts.Overlay);
            command.ReplyToCommand("[uikit] overlay: off");
            return;
        }

        if (effect is "next" or "prev")
        {
            int step = effect == "next" ? 1 : -1;
            _overlayDemoIndex = (_overlayDemoIndex + step + OverlayEffects.All.Count) % OverlayEffects.All.Count;
            effect = OverlayEffects.All[_overlayDemoIndex];
        }

        float seconds = 0f;
        if (command.ArgCount > 2 && float.TryParse(command.GetArg(2), NumberStyles.Float, CultureInfo.InvariantCulture, out float asked))
        {
            seconds = asked;
        }

        kit.Overlay(target, DemoLayouts.Overlay, effect, seconds);
        command.ReplyToCommand($"[uikit] overlay: {effect}");
    }
}
