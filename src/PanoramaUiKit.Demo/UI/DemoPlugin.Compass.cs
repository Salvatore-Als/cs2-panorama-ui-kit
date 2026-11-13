using System.Numerics;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using PanoramaUiKit.Shared.Api;
using PanoramaUiKit.Shared.Common;
using PanoramaUiKit.Shared.Compass;

namespace PanoramaUiKit.Demo;

public sealed partial class DemoPlugin
{
    private const float CompassDemoDistance = 1500f;

    /// <summary>Demo pins: a heading from the player, a shape and a colour.</summary>
    private static readonly (float Heading, string Kind, string Style)[] CompassDemoPins =
    [
        (30f, CompassNames.Zone, UiStyle.Info),
        (140f, CompassNames.Skull, UiStyle.Danger),
        (250f, CompassNames.Diamond, UiStyle.Gold),
        (320f, CompassNames.Crate, UiStyle.Success),
    ];

    /// <summary>
    /// Toggles the compass, following the caller's view, with up to four pins (three by default) placed
    /// 1500 units around where the caller stands. Turn around to see them go by.
    /// </summary>
    [ConsoleCommand("css_uikit_demo_compass", "Toggles a compass that follows the view, with pins around the caller.")]
    [CommandHelper(usage: "[pins] [band]", whoCanExecute: CommandUsage.CLIENT_ONLY)]
    public void OnDemoCompass(CCSPlayerController? player, CommandInfo command)
    {
        if (!TryKitFor(player, command, out IPanoramaUiKit kit, out CCSPlayerController target))
        {
            return;
        }

        if (kit.Compass.IsShowing(target, DemoLayouts.Compass))
        {
            kit.Compass.Hide(target, DemoLayouts.Compass);
            command.ReplyToCommand("[uikit] compass: off");
            return;
        }

        int count = 3;
        if (command.ArgCount > 1 && int.TryParse(command.GetArg(1), out int asked))
        {
            count = Math.Clamp(asked, 0, CompassDemoPins.Length);
        }

        Vector3 origin = Vector3.Zero;
        if (target.PlayerPawn.Value?.AbsOrigin is { } at)
        {
            origin = new Vector3(at.X, at.Y, at.Z);
        }

        List<CompassPin> pins = [];
        foreach ((float heading, string kind, string style) in CompassDemoPins.Take(count))
        {
            // Heading 0 = +Y, clockwise: east (90) is +X.
            float radians = heading * MathF.PI / 180f;
            Vector3 offset = new(MathF.Sin(radians) * CompassDemoDistance, MathF.Cos(radians) * CompassDemoDistance, 0f);
            pins.Add(new CompassPin(origin + offset, kind, style));
        }

        kit.Compass.Show(target, new CompassOptions
        {
            Layout = DemoLayouts.Compass,
            Pins = pins,
            Placement = HasArg(command, "band") ? CompassPlacement.Band : CompassPlacement.UnderHud,
        });
        command.ReplyToCommand($"[uikit] compass: on, {pins.Count} pin(s)");
    }
}
