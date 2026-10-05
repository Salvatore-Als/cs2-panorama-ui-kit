using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Core.Capabilities;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Timers;
using Microsoft.Extensions.Logging;
using PanoramaUiKit.Shared.Api;
using PanoramaUiKit.Shared.Common;
using Timer = CounterStrikeSharp.API.Modules.Timers.Timer;

namespace PanoramaUiKit.Demo;

/// <summary>
/// Demo consumer of the kit. One partial file per component, one command each:
///
///   UI/DemoPlugin.Toast.cs      css_uikit_demo_toast
///   UI/DemoPlugin.Banner.cs     css_uikit_demo_banner [text]
///   UI/DemoPlugin.Announce.cs   css_uikit_demo_announce
///   UI/DemoPlugin.Abilities.cs  css_uikit_demo_abilities
///   UI/DemoPlugin.Match.cs      css_uikit_demo_match
///   UI/DemoPlugin.KillFeed.cs   css_uikit_demo_killfeed
///   UI/DemoPlugin.Victory.cs    css_uikit_demo_victory
///   UI/DemoPlugin.Roulette.cs   css_uikit_demo_roulette [fast|slow]
///   UI/DemoPlugin.Vote.cs       css_uikit_demo_vote
///   UI/DemoPlugin.Panel.cs      css_uikit_demo_panel
///   UI/DemoPlugin.Menu.cs       css_uikit_demo_menu
///
/// Every demo names the layout file it draws on (DemoLayouts.cs): there is no default.
/// This file holds what they share: the capability, and the helpers to resolve it and to loop.
/// </summary>
public sealed partial class DemoPlugin : BasePlugin
{
    public override string ModuleName => "Panorama UI Kit Demo";

    public override string ModuleVersion => "1.0.0";

    public override string ModuleAuthor => "Kriax";

    public override string ModuleDescription => "Demo consumer of the Panorama UI Kit API.";

    private static readonly PluginCapability<IPanoramaUiKit> UiKitCapability = new(IPanoramaUiKit.CapabilityName);

    private readonly Dictionary<string, Timer> _loops = new();

    /// <summary>Resolved on every use: survives a hot reload of the kit.</summary>
    private static IPanoramaUiKit? Kit => UiKitCapability.Get();

    public override void OnAllPluginsLoaded(bool hotReload)
    {
        IPanoramaUiKit? kit = Kit;
        if (kit == null)
        {
            Logger.LogError("Panorama UI Kit not found: install PanoramaUiKit and shared/PanoramaUiKit.Shared.");
            return;
        }

        if (kit.Version != IPanoramaUiKit.ApiVersion)
        {
            Logger.LogWarning("Panorama UI Kit API v{Running}, demo built against v{Built}.", kit.Version, IPanoramaUiKit.ApiVersion);
        }

        // Spawn the layouts now, while nobody is being sent anything: creating a layout entity on the
        // first command, in the same tick as its first writes, sends one big burst to the client.
        foreach (UiLayout layout in DemoLayouts.All)
        {
            if (!kit.Preload(layout))
            {
                Logger.LogError("Layout {Layout} was refused, see the Panorama UI Kit log.", layout);
            }
        }
    }

    public override void Unload(bool hotReload)
    {
        foreach (Timer timer in _loops.Values)
        {
            timer.Kill();
        }

        _loops.Clear();
    }

    [ConsoleCommand("css_uikit_demo_clear", "Clears every kit element for the caller.")]
    [CommandHelper(whoCanExecute: CommandUsage.CLIENT_ONLY)]
    public void OnClear(CCSPlayerController? player, CommandInfo command)
    {
        if (TryKitFor(player, command, out IPanoramaUiKit kit, out CCSPlayerController target))
        {
            kit.ClearAll(target);
        }
    }

    /// <summary>The kit, for commands that target everyone.</summary>
    private static bool TryKit(CommandInfo command, out IPanoramaUiKit kit)
    {
        IPanoramaUiKit? resolved = Kit;
        if (resolved == null)
        {
            command.ReplyToCommand("[UiKit Demo] Panorama UI Kit is not loaded.");
            kit = null!;
            return false;
        }

        kit = resolved;
        return true;
    }

    /// <summary>The kit and a valid caller, for commands that target the caller.</summary>
    private static bool TryKitFor(CCSPlayerController? player, CommandInfo command, out IPanoramaUiKit kit, out CCSPlayerController target)
    {
        target = null!;
        if (!TryKit(command, out kit) || player is not { IsValid: true })
        {
            return false;
        }

        target = player;
        return true;
    }

    /// <summary>
    /// Runs <paramref name="step"/> now, then every second while it returns true. Starting a loop
    /// under a key already running replaces it.
    /// </summary>
    private void RunEverySecond(string key, Func<bool> step)
    {
        if (_loops.Remove(key, out Timer? previous))
        {
            previous.Kill();
        }

        if (!step())
        {
            return;
        }

        Timer? timer = null;
        timer = AddTimer(1f, () =>
        {
            if (!step())
            {
                timer?.Kill();
                _loops.Remove(key);
            }
        }, TimerFlags.REPEAT | TimerFlags.STOP_ON_MAPCHANGE);
        _loops[key] = timer;
    }

    /// <summary>One-shot delayed call, dropped on map change.</summary>
    private void Later(float seconds, Action action)
    {
        AddTimer(seconds, action, TimerFlags.STOP_ON_MAPCHANGE);
    }
}
