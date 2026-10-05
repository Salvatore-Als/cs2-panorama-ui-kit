using CounterStrikeSharp.API.Core;
using PanoramaUiKit.Internal;
using PanoramaUiKit.Shared.Common;

namespace PanoramaUiKit.Channels.Base;

/// <summary>
/// A layout with one <typeparamref name="TViewer"/> per player: the bookkeeping every component
/// shares (attach, look up, forget), and the entry / exit choreography of a card that slides in on
/// "show" and is closed once its exit has played.
/// </summary>
internal abstract class LayoutChannel<TViewer> : LayoutChannel where TViewer : ViewerState
{
    /// <summary>
    /// Minimum time between two writes of the same kind to one viewer (two toasts, two banner
    /// updates). Several shows in one tick reach the client as one burst; spread out they do not.
    /// </summary>
    protected const float WriteGapSeconds = 0.12f;

    protected readonly Dictionary<int, TViewer> Viewers = new();

    protected LayoutChannel(BasePlugin plugin, UiLayout layout) : base(plugin, layout)
    {
    }

    public override int ViewerCount => Viewers.Count;

    protected override IEnumerable<int> TrackedSlots() => Viewers.Keys;

    public override bool Forget(int slot)
    {
        if (!Viewers.Remove(slot, out TViewer? viewer))
        {
            return false;
        }

        viewer.Release();
        return true;
    }

    /// <summary>The player's viewer if the layout is open for them; a viewer whose panel is gone is dropped.</summary>
    protected bool TryOpenViewer(CCSPlayerController player, out TViewer viewer)
    {
        if (Viewers.TryGetValue(player.Slot, out TViewer? found) && Panel.IsOpenFor(player))
        {
            viewer = found;
            return true;
        }

        Forget(player.Slot);
        viewer = null!;
        return false;
    }

    /// <summary>A fresh viewer for the player, layout opened. Anything left of a previous one is dropped first.</summary>
    protected TViewer Attach(CCSPlayerController player)
    {
        Forget(player.Slot);
        TViewer viewer = CreateViewer(player.Slot);
        Viewers[player.Slot] = viewer;
        UiTrace.Event(Layout.Name, $"open for slot {player.Slot}");
        long started = System.Diagnostics.Stopwatch.GetTimestamp();
        Panel.Open(player);
        MarkWarm();
        UiTrace.Slow(Layout.Name, "Panel.Open", System.Diagnostics.Stopwatch.GetElapsedTime(started).TotalMilliseconds);
        return viewer;
    }

    /// <summary>The player's viewer if open, a fresh one otherwise. <paramref name="fresh"/> says which.</summary>
    protected TViewer Ensure(CCSPlayerController player, out bool fresh)
    {
        if (TryOpenViewer(player, out TViewer viewer))
        {
            fresh = false;
            return viewer;
        }

        fresh = true;
        return Attach(player);
    }

    protected abstract TViewer CreateViewer(int slot);

    /// <summary>A diff cache that sends every class once: right for layouts that toggle few classes.</summary>
    protected ClientState NewClient() => new(Panel, Layout.Name);

    /// <summary>True while <paramref name="viewer"/> is still this player's viewer.</summary>
    protected bool IsCurrent(CCSPlayerController player, TViewer viewer) =>
        player is { IsValid: true } && Viewers.TryGetValue(player.Slot, out TViewer? current) && current == viewer;

    /// <summary>Runs <paramref name="action"/> later, cancelled with the viewer's other timers.</summary>
    protected void After(TViewer viewer, float seconds, Action action) => viewer.Track(Later(seconds, action));

    /// <summary>
    /// Adds "show" on <paramref name="panelId"/> one network update later: the panel needs a frame
    /// in its hidden state for the entry transition to have something to start from.
    /// </summary>
    protected void RevealLater(CCSPlayerController player, TViewer viewer, string panelId, Action? shown = null)
    {
        After(viewer, Layout.EntrySeconds, () =>
        {
            if (!IsCurrent(player, viewer))
            {
                return;
            }

            viewer.Client.Class(player, panelId, "show", true);
            shown?.Invoke();
        });
    }

    /// <summary>Drops "show" on <paramref name="panelId"/>, then clears the player once the exit has played.</summary>
    protected void Dismiss(CCSPlayerController player, TViewer viewer, string panelId)
    {
        viewer.CancelTimers();
        viewer.ReleaseHandle();
        viewer.Client.Class(player, panelId, "show", false);
        After(viewer, Layout.ExitSeconds, () =>
        {
            if (IsCurrent(player, viewer))
            {
                ClearAll(player);
            }
        });
    }
}
