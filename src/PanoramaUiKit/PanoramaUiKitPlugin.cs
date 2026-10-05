using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Core.Capabilities;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using PanoramaManager;
using PanoramaUiKit.Channels.Base;
using PanoramaUiKit.Core;
using PanoramaUiKit.Internal;
using PanoramaUiKit.Shared.Api;
using PanoramaUiKit.Shared.Common;

namespace PanoramaUiKit;

/// <summary>
/// Panorama UI Kit: toasts, banners, announcements, ability bar, match bar, kill feed and victory
/// screen for every plugin on the server, through the <see cref="IPanoramaUiKit"/> capability.
/// </summary>
public sealed class PanoramaUiKitPlugin : BasePlugin, IPluginConfig<UiKitConfig>
{
    public override string ModuleName => "Panorama UI Kit";

    public override string ModuleVersion => "1.0.0";

    public override string ModuleAuthor => "Kriax";

    public override string ModuleDescription => "Shared Panorama HUD components (toast, banner, announce, abilities, match, kill feed, victory, panel, menu) with swappable layouts.";

    public UiKitConfig Config { get; set; } = new();

    private static readonly PluginCapability<IPanoramaUiKit> Capability = new(IPanoramaUiKit.CapabilityName);

    private const float WarmStepSeconds = 0.4f;

    private UiKit? _kit;
    private LayoutRegistry? _registry;
    private bool _warming;

    public void OnConfigParsed(UiKitConfig config)
    {
        Config = config;
    }

    public override void Load(bool hotReload)
    {
        Panorama.Init(this);
        UiTrace.Attach(Logger);

        LayoutRegistry registry = new LayoutRegistry(this, Logger);
        _kit = new UiKit(registry);
        Capabilities.RegisterPluginCapability(Capability, () => _kit!);


        _registry = registry;
        registry.WarmWanted += ScheduleWarmUp;

        RegisterListener<Listeners.OnClientDisconnect>(slot => registry.Forget(slot));
        RegisterListener<Listeners.OnClientPutInServer>(_ => ScheduleWarmUp());
        RegisterListener<Listeners.OnMapStart>(_ =>
        {
            _kit?.ClearEveryone();
            registry.CoolAll();
            _warming = false;
        });
    }

    /// <summary>
    /// Spawns the layout entities one by one, from timers and not from a client's command: the first
    /// Open of a layout takes ~200 ms, and inside the message processing of a client that is long
    /// enough for the netchan to drop them (NETWORK_DISCONNECT_OVERFLOW / "excessive CPU usage").
    /// </summary>
    private void ScheduleWarmUp()
    {
        if (_warming)
        {
            return;
        }

        _warming = true;
        SafeTimer.Once(this, WarmStepSeconds, WarmStep);
    }

    private void WarmStep()
    {
        CCSPlayerController? human = Players.Humans().FirstOrDefault();
        if (_registry == null || human == null)
        {
            _warming = false;
            return;
        }

        if (_registry.WarmNext(human))
        {
            SafeTimer.Once(this, WarmStepSeconds, WarmStep);
            return;
        }

        _warming = false;
    }

    public override void Unload(bool hotReload)
    {
        _kit?.Dispose();
        _kit = null;
        Panorama.Shutdown();
    }

    [ConsoleCommand("css_uikit_status", "Lists the Panorama UI Kit layouts and who sees them.")]
    [CommandHelper(whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
    public void OnStatus(CCSPlayerController? caller, CommandInfo command)
    {
        if (!Allowed(caller, command) || _kit == null)
        {
            return;
        }

        command.ReplyToCommand($"[UiKit] API v{_kit.Version}, capability '{IPanoramaUiKit.CapabilityName}'");
        foreach (LayoutChannel channel in _kit.Channels)
        {
            UiLayout layout = channel.Layout;
            command.ReplyToCommand($"  live    {layout.Component,-9} {layout.Name,-24} root={layout.RootPanelId} viewers={channel.ViewerCount}");
        }
    }

    [ConsoleCommand("css_uikit_trace", "Logs every write the kit sends to a client. Usage: css_uikit_trace on|off")]
    [CommandHelper(minArgs: 1, usage: "on|off", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
    public void OnTrace(CCSPlayerController? caller, CommandInfo command)
    {
        if (!Allowed(caller, command))
        {
            return;
        }

        UiTrace.Enabled = command.GetArg(1).Equals("on", StringComparison.OrdinalIgnoreCase);

        string state = "off";
        if (UiTrace.Enabled)
        {
            state = "on";
        }

        command.ReplyToCommand($"[UiKit] trace {state}");
    }

    [ConsoleCommand("css_uikit_clear", "Clears every Panorama UI Kit element for everyone.")]
    [CommandHelper(whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
    public void OnClear(CCSPlayerController? caller, CommandInfo command)
    {
        if (!Allowed(caller, command))
        {
            return;
        }

        _kit?.ClearEveryone();
        command.ReplyToCommand("[UiKit] Cleared.");
    }

    private bool Allowed(CCSPlayerController? caller, CommandInfo command)
    {
        if (caller == null || AdminManager.PlayerHasPermissions(caller, Config.AdminPermission))
        {
            return true;
        }

        command.ReplyToCommand("[UiKit] You do not have access to this command.");
        return false;
    }
}
