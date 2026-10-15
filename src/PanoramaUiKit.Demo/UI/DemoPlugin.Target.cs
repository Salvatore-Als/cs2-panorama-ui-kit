using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using PanoramaUiKit.Shared.Api;
using PanoramaUiKit.Shared.Common;
using PanoramaUiKit.Shared.Targets;

namespace PanoramaUiKit.Demo;

public sealed partial class DemoPlugin
{
    private const float TargetDemoSeconds = 8f;

    /// <summary>The four positions at once, one size each, for 8 seconds. An optional position (above, below, left, right) shows just that one.</summary>
    [ConsoleCommand("css_uikit_demo_target", "Text around the crosshair.")]
    [CommandHelper(usage: "[above|below|left|right]", whoCanExecute: CommandUsage.CLIENT_ONLY)]
    public void OnDemoTarget(CCSPlayerController? player, CommandInfo command)
    {
        if (!TryKitFor(player, command, out IPanoramaUiKit kit, out CCSPlayerController target))
        {
            return;
        }

        if (command.ArgCount > 1 && Enum.TryParse(command.GetArg(1), ignoreCase: true, out TargetPosition only))
        {
            kit.Target(target, DemoLayouts.Target, $"Position: {only}", only, seconds: TargetDemoSeconds);
            return;
        }

        kit.Target(target, DemoLayouts.Target, "Small", TargetPosition.Above, TargetSize.Small, seconds: TargetDemoSeconds);
        kit.Target(target, DemoLayouts.Target, "Press E to open", TargetPosition.Below, TargetSize.Medium, UiStyle.Success, TargetDemoSeconds);
        kit.Target(target, DemoLayouts.Target, "Large", TargetPosition.Left, TargetSize.Large, UiStyle.Warn, TargetDemoSeconds);
        kit.Target(target, DemoLayouts.Target, "Extra large", TargetPosition.Right, TargetSize.ExtraLarge, UiStyle.Danger, TargetDemoSeconds);
    }
}
