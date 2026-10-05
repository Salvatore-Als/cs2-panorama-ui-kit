using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using PanoramaManager;
using PanoramaUiKit.Internal;
using PanoramaUiKit.Shared.Common;

namespace PanoramaUiKit.Channels.Base;

/// <summary>
/// One layout file, one layout entity. The non-generic face the registry and the status command
/// see; components derive from <see cref="LayoutChannel{TViewer}"/>.
/// </summary>
internal abstract class LayoutChannel : IDisposable
{
    protected readonly BasePlugin Plugin;
    protected readonly PanelHandle Panel;

    protected LayoutChannel(BasePlugin plugin, UiLayout layout)
    {
        Plugin = plugin;
        Layout = layout;
        Panel = Panorama.Spawn(layout.Path, new LayoutContract
        {
            RootPanelId = layout.RootPanelId,
            CaptureInput = CapturesInput,
            HideHud = HideHudFlags.None,
            RowCount = 0,
        });

        Panel.OnEvent += OnPanelEvent;
        UiTrace.Event(layout.Name, "spawned");
    }

    public UiLayout Layout { get; }

    /// <summary>
    /// Whether the layout takes the mouse while open. Read in the constructor: overrides must return
    /// a constant, not instance state.
    /// </summary>
    protected virtual bool CapturesInput => false;

    /// <summary>Viewers with something on screen, for the status command.</summary>
    public abstract int ViewerCount { get; }

    /// <summary>
    /// True once the layout entity exists on this map. PanoramaManager spawns it on the first Open,
    /// and that takes ~200 ms: inside a client's command it outlasts the netchan's limit on message
    /// processing and the client is dropped. Run <see cref="WarmUp"/> from a timer instead.
    /// </summary>
    public bool IsWarm { get; private set; }

    /// <summary>A layout was opened for someone, so its entity exists.</summary>
    protected void MarkWarm() => IsWarm = true;

    /// <summary>The map changed: the layout entity is gone with it.</summary>
    public void Cool() => IsWarm = false;

    /// <summary>Spawns the layout entity by opening and closing the layout on <paramref name="player"/>.</summary>
    public void WarmUp(CCSPlayerController player)
    {
        if (IsWarm || player is not { IsValid: true })
        {
            return;
        }

        long started = System.Diagnostics.Stopwatch.GetTimestamp();
        Panel.Open(player);
        Panel.Close(player);
        IsWarm = true;
        UiTrace.Slow(Layout.Name, "warm-up", System.Diagnostics.Stopwatch.GetElapsedTime(started).TotalMilliseconds);
    }

    /// <summary>Removes everything this channel shows to one player, without an exit animation.</summary>
    public virtual void ClearAll(CCSPlayerController player)
    {
        if (player is not { IsValid: true })
        {
            return;
        }

        // Close scrubs every class this panel set for the viewer, so the next Open starts clean.
        bool tracked = Forget(player.Slot);
        if (tracked || Panel.IsOpenFor(player))
        {
            UiTrace.Event(Layout.Name, $"close for slot {player.Slot}");
            Panel.Close(player);
        }
    }

    /// <summary>Removes everything this channel shows to everyone (round reset, map start).</summary>
    public virtual void ClearEveryone()
    {
        foreach (int slot in TrackedSlots().ToList())
        {
            CCSPlayerController? player = Utilities.GetPlayerFromSlot(slot);
            if (player is { IsValid: true })
            {
                ClearAll(player);
            }
            else
            {
                Forget(slot);
            }
        }
    }

    /// <summary>Drops bookkeeping for a slot. Must not touch the player. True if the slot was tracked.</summary>
    public abstract bool Forget(int slot);

    protected abstract IEnumerable<int> TrackedSlots();

    /// <summary>
    /// The library closes on round restart and disconnect, and reopens after a restart with every
    /// per-viewer class scrubbed: by default, whatever we thought was on screen is dropped.
    /// </summary>
    protected virtual void OnPanelEvent(PanelEvent e)
    {
        if (e.Action == PanelAction.Close)
        {
            Forget(e.Player.Slot);
            return;
        }

        if (e.Action == PanelAction.Restored)
        {
            Forget(e.Player.Slot);
            Panel.Close(e.Player);
        }
    }

    protected SafeTimer Later(float seconds, Action action) => SafeTimer.Once(Plugin, Math.Max(0.01f, seconds), action);

    protected static IEnumerable<CCSPlayerController> Humans() => Players.Humans();

    public virtual void Dispose()
    {
        foreach (int slot in TrackedSlots().ToList())
        {
            Forget(slot);
        }

        Panel.OnEvent -= OnPanelEvent;
        Panel.Dispose();
    }
}
