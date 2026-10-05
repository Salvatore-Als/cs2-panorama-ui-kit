using CounterStrikeSharp.API.Core;
using PanoramaManager;
using PanoramaUiKit.Internal;
using PanoramaUiKit.Shared.Common;

namespace PanoramaUiKit.Channels.Base;

/// <summary>A viewer of an interactive layout: shown or not, and who to tell when it closes.</summary>
internal class InteractiveViewer : ViewerState
{
    public InteractiveViewer(ClientState client) : base(client)
    {
    }

    public bool Shown { get; set; }

    public Action<CCSPlayerController>? OnClose { get; set; }
}

/// <summary>
/// A layout the player clicks (panel, menu): takes the mouse, repaints its whole state on every
/// click, and runs the caller's OnClose once when it really leaves the screen.
///
/// Subclasses paint (<see cref="Paint"/>) and handle their buttons (<see cref="OnButton"/>); this
/// owns the rest: open-or-reuse, reveal, animated close, the round-restart repaint, and the
/// long-lived <c>touched</c> memory that lets the client cache skip classes still in their
/// authored state.
/// </summary>
internal abstract class InteractiveChannel<TViewer> : LayoutChannel<TViewer> where TViewer : InteractiveViewer
{
    private readonly Dictionary<int, HashSet<(string Panel, string Class)>> _touched = new();

    protected InteractiveChannel(BasePlugin plugin, UiLayout layout) : base(plugin, layout)
    {
    }

    protected override bool CapturesInput => true;

    /// <summary>Id of the panel that carries "show", and the close button.</summary>
    protected abstract string RevealId { get; }

    protected abstract string CloseButtonId { get; }

    /// <summary>Whether (panel id, class) is on as authored: the pooled entries' "hidden".</summary>
    protected abstract bool AuthoredOn(string panelId, string className);

    protected abstract void Paint(CCSPlayerController player, TViewer viewer);

    protected abstract void OnButton(CCSPlayerController player, TViewer viewer, string elementId);

    /// <summary>A cache that skips classes still as authored, remembering per slot what was ever moved.</summary>
    protected ClientState NewInteractiveClient(int slot)
    {
        if (!_touched.TryGetValue(slot, out HashSet<(string, string)>? touched))
        {
            touched = [];
            _touched[slot] = touched;
        }

        return new ClientState(Panel, Layout.Name, AuthoredOn, touched);
    }

    /// <summary>
    /// Shows the player's viewer, prepared by <paramref name="prepare"/>: reused (and repainted in
    /// place) when the layout is already open for them, attached and revealed otherwise. Replacing
    /// what was open is not a close: the previous OnClose does not run.
    /// </summary>
    protected UiHandle Present(CCSPlayerController player, Action<TViewer> prepare, Action<CCSPlayerController>? onClose)
    {
        TViewer viewer = Ensure(player, out bool fresh);

        // A pending exit (Close just before this) must not close what replaces it.
        viewer.CancelTimers();
        viewer.OnClose = onClose;
        UiHandle handle = viewer.NewHandle(player, () => Close(player));
        prepare(viewer);
        Paint(player, viewer);

        if (fresh)
        {
            RevealLater(player, viewer, RevealId, () => viewer.Shown = true);
        }
        else if (!viewer.Shown)
        {
            viewer.Client.Class(player, RevealId, "show", true);
            viewer.Shown = true;
        }

        return handle;
    }

    /// <summary>Repaints now, for content that changed outside a click.</summary>
    public void Refresh(CCSPlayerController player)
    {
        if (player is { IsValid: true } && Viewers.TryGetValue(player.Slot, out TViewer? viewer))
        {
            Paint(player, viewer);
        }
    }

    public bool IsOpenFor(CCSPlayerController player) => Viewers.ContainsKey(player.Slot);

    /// <summary>Plays the exit, then closes. OnClose runs once the panel is gone.</summary>
    public void Close(CCSPlayerController player)
    {
        if (player is { IsValid: true } && Viewers.TryGetValue(player.Slot, out TViewer? viewer))
        {
            viewer.Shown = false;
            Dismiss(player, viewer, RevealId);
        }
    }

    /// <summary>Immediate close, no exit animation; OnClose still runs.</summary>
    public override void ClearAll(CCSPlayerController player)
    {
        if (player is not { IsValid: true })
        {
            return;
        }

        Closed(player);
        if (Panel.IsOpenFor(player))
        {
            Panel.Close(player);
        }
    }

    protected override void OnPanelEvent(PanelEvent e)
    {
        CCSPlayerController player = e.Player;

        switch (e.Action)
        {
            case PanelAction.Close:
                Closed(player);
                return;

            // Rebuilt after a round restart: the client starts from the layout again, repaint all.
            case PanelAction.Restored when Viewers.TryGetValue(player.Slot, out TViewer? restored):
                restored.Client.Reset();
                Paint(player, restored);
                restored.Client.Class(player, RevealId, "show", true);
                restored.Shown = true;
                return;

            case PanelAction.Button when e.ElementId is string elementId && Viewers.TryGetValue(player.Slot, out TViewer? viewer):
                if (elementId == CloseButtonId)
                {
                    Close(player);
                    return;
                }

                OnButton(player, viewer, elementId);

                // The click may have closed the layout or replaced what it shows.
                if (IsCurrent(player, viewer))
                {
                    Paint(player, viewer);
                }

                return;
        }
    }

    /// <summary>Drops the viewer and runs its OnClose, once.</summary>
    private void Closed(CCSPlayerController player)
    {
        if (!Viewers.Remove(player.Slot, out TViewer? viewer))
        {
            return;
        }

        viewer.Release();
        if (player is { IsValid: true })
        {
            viewer.OnClose?.Invoke(player);
        }
    }

    /// <summary><c>prefix12suffix</c> -> 12.</summary>
    protected static bool TryIndex(string id, string prefix, string suffix, out int index)
    {
        index = -1;
        if (!id.StartsWith(prefix, StringComparison.Ordinal) || !id.EndsWith(suffix, StringComparison.Ordinal)
            || id.Length <= prefix.Length + suffix.Length)
        {
            return false;
        }

        return int.TryParse(id[prefix.Length..^suffix.Length], out index);
    }

    public override void Dispose()
    {
        _touched.Clear();
        base.Dispose();
    }
}
