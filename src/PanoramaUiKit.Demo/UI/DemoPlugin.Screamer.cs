using System.Globalization;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using PanoramaUiKit.Shared.Api;
using PanoramaUiKit.Shared.Screamers;

namespace PanoramaUiKit.Demo;

public sealed partial class DemoPlugin
{
    private const float ScreamerDemoSeconds = 1f;

    /// <summary>
    /// A random screamer for one second, or the one named (momo, babadook, jeff, shadow) for the seconds given.
    /// Needs the addon with its picture and sound.
    /// </summary>
    [ConsoleCommand("css_uikit_demo_screamer", "Full-screen picture with its sound.")]
    [CommandHelper(usage: "[name] [seconds]", whoCanExecute: CommandUsage.CLIENT_ONLY)]
    public void OnDemoScreamer(CCSPlayerController? player, CommandInfo command)
    {
        if (!TryKitFor(player, command, out IPanoramaUiKit kit, out CCSPlayerController target))
        {
            return;
        }

        string name = ScreamerNames.All[Random.Shared.Next(ScreamerNames.All.Count)];
        if (command.ArgCount > 1)
        {
            name = command.GetArg(1);
        }

        float seconds = ScreamerDemoSeconds;
        if (command.ArgCount > 2 && float.TryParse(command.GetArg(2), NumberStyles.Float, CultureInfo.InvariantCulture, out float asked) && asked > 0f)
        {
            seconds = asked;
        }

        command.ReplyToCommand($"[uikit] screamer: {name}, {seconds:0.##} s");
        kit.Screamer(target, DemoLayouts.Screamer, name, seconds);
    }
}
