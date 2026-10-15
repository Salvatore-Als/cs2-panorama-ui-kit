using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using PanoramaManager;
using PanoramaUiKit.Channels.Base;
using PanoramaUiKit.Internal;
using PanoramaUiKit.Shared.Common;

namespace PanoramaUiKit.Channels;

/// <summary>What an anchored box is told to show. <see cref="Lifetime"/> is how long it stays up.</summary>
internal abstract record BoxContent
{
    /// <summary>Seconds until the box hides itself. Zero or less stays until hidden.</summary>
    public abstract float Lifetime { get; }
}

/// <summary>One box of the pool (a corner, a position, a bar) for one viewer. Its timers are its own.</summary>
internal sealed class Box<TContent> where TContent : BoxContent
{
    public required string Key;
    public required string Id;
    public bool Visible;
    public TContent? Content;
    public UiHandle? Handle;
    public readonly List<SafeTimer> Timers = [];

    /// <summary>Lines painted last time: the ones beyond the next content have to be blanked. Used by cards.</summary>
    public int Painted;

    /// <summary>A timed fill is applied (timed, dur-N, run). Used by progress bars.</summary>
    public bool TimedActive;

    /// <summary>When the fill was last reset. Used by progress bars.</summary>
    public float ResetAt = float.MinValue;

    public void KillTimers()
    {
        foreach (SafeTimer timer in Timers)
        {
            timer.Kill();
        }

        Timers.Clear();
    }
}

internal sealed class BoxViewer<TContent> : ViewerState where TContent : BoxContent
{
    public BoxViewer(ClientState client) : base(client)
    {
    }

    public Dictionary<string, Box<TContent>> Boxes { get; } = new();

    public bool AnyVisible => Boxes.Values.Any(b => b.Visible);

    public override void Release()
    {
        foreach (Box<TContent> box in Boxes.Values)
        {
            box.KillTimers();
            box.Handle?.MarkGone();
        }

        base.Release();
    }
}

/// <summary>
/// Boxes pinned to a fixed place on one layout, per viewer: the card (corners), target (around the
/// crosshair) and progress (bars) contracts. Each box is a panel authored hidden; showing it paints
/// its text and drops "hidden", hiding puts it back. There is no entry or exit animation, so nothing
/// here waits on the client.
///
/// A Show on a box that is already up rewrites it in place. The layout is closed for a viewer once
/// their last box is hidden.
/// </summary>
internal abstract class AnchoredChannel<TContent> : LayoutChannel<BoxViewer<TContent>> where TContent : BoxContent
{
    protected AnchoredChannel(BasePlugin plugin, UiLayout layout) : base(plugin, layout)
    {
    }

    protected override BoxViewer<TContent> CreateViewer(int slot) => new(NewClient());

    /// <summary>Panel id of the box for a placement code: <c>c_tl</c>, <c>tg_b</c>, <c>pg_c</c>.</summary>
    protected abstract string BoxId(string key);

    /// <summary>Writes the content's text and classes. The box is shown by the caller afterwards.</summary>
    protected abstract void Paint(CCSPlayerController player, BoxViewer<TContent> viewer, Box<TContent> box, TContent content);

    /// <summary>The box is about to be hidden: undo whatever would otherwise run on while it is gone.</summary>
    protected virtual void Unpaint(CCSPlayerController player, BoxViewer<TContent> viewer, Box<TContent> box)
    {
    }

    public IUiHandle Show(CCSPlayerController player, string key, TContent content)
    {
        if (player is not { IsValid: true, IsBot: false })
        {
            return UiHandle.Dead;
        }

        BoxViewer<TContent> viewer = Ensure(player, out _);
        if (!viewer.Boxes.TryGetValue(key, out Box<TContent>? box))
        {
            box = new Box<TContent> { Key = key, Id = BoxId(key) };
            viewer.Boxes[key] = box;
        }

        box.KillTimers();
        box.Handle?.MarkGone();

        UiHandle handle = new(player);
        handle.OnClear = () => Retire(player, viewer, box, handle);
        box.Handle = handle;
        box.Content = content;

        Paint(player, viewer, box, content);
        viewer.Client.Class(player, box.Id, "hidden", false);
        box.Visible = true;

        if (content.Lifetime > 0f)
        {
            box.Timers.Add(Later(content.Lifetime, () => Retire(player, viewer, box, handle)));
        }

        return handle;
    }

    /// <summary>Hides one box for one player. Nothing happens if it is not up.</summary>
    public void Hide(CCSPlayerController player, string key)
    {
        if (player is { IsValid: true }
            && Viewers.TryGetValue(player.Slot, out BoxViewer<TContent>? viewer)
            && viewer.Boxes.TryGetValue(key, out Box<TContent>? box))
        {
            Retire(player, viewer, box, box.Handle);
        }
    }

    public void HideAll(CCSPlayerController player)
    {
        if (player is { IsValid: true } && Viewers.ContainsKey(player.Slot))
        {
            ClearAll(player);
        }
    }

    public void HideEveryone() => ClearEveryone();

    /// <summary>Hides <paramref name="box"/> if <paramref name="handle"/> is still what is showing in it.</summary>
    private void Retire(CCSPlayerController player, BoxViewer<TContent> viewer, Box<TContent> box, UiHandle? handle)
    {
        if (!IsCurrent(player, viewer) || !box.Visible || box.Handle != handle)
        {
            return;
        }

        box.KillTimers();
        box.Handle?.MarkGone();
        box.Handle = null;
        box.Content = null;
        box.Visible = false;
        Unpaint(player, viewer, box);

        // The last one: close the layout instead of writing "hidden" first.
        if (!viewer.AnyVisible)
        {
            ClearAll(player);
            return;
        }

        viewer.Client.Class(player, box.Id, "hidden", true);
    }

    /// <summary>
    /// After a round restart the library reopens the layout with every class scrubbed. A box without
    /// a lifetime is painted again; a timed one is dropped.
    /// </summary>
    protected override void OnPanelEvent(PanelEvent e)
    {
        if (e.Action == PanelAction.Restored
            && Viewers.TryGetValue(e.Player.Slot, out BoxViewer<TContent>? viewer))
        {
            List<(string Key, TContent Content)> sticky = viewer.Boxes.Values
                .Where(b => b is { Visible: true, Content: not null } && b.Content.Lifetime <= 0f)
                .Select(b => (b.Key, b.Content!))
                .ToList();

            Forget(e.Player.Slot);
            foreach ((string key, TContent content) in sticky)
            {
                Show(e.Player, key, content);
            }

            if (sticky.Count == 0)
            {
                Panel.Close(e.Player);
            }

            return;
        }

        base.OnPanelEvent(e);
    }
}
