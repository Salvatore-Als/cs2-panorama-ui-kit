using CounterStrikeSharp.API.Core;
using PanoramaUiKit.Channels.Base;
using PanoramaUiKit.Internal;
using PanoramaUiKit.Shared.Common;
using PanoramaUiKit.Shared.Screamers;

namespace PanoramaUiKit.Channels;

internal sealed record ScreamerContent(string Name, float Seconds, UiContent Extras) : BoxContent
{
    public override float Lifetime => Seconds;
}

/// <summary>
/// The full-screen picture of one screamer layout (contract "screamer"): a single box, <c>sc_image</c>.
/// <c>scare-&lt;name&gt;</c> carries the picture; the sound is played by the service, on the client, in the same tick.
/// Retiring the box closes the layout, which scrubs the class.
/// </summary>
internal sealed class ScreamerChannel : AnchoredChannel<ScreamerContent>
{
    public const string Key = "image";

    /// <summary>Screamers waiting for their sound to start, per slot.</summary>
    private readonly Dictionary<int, SafeTimer> _pending = new();

    public ScreamerChannel(BasePlugin plugin, ScreamerLayout layout) : base(plugin, layout, [Key])
    {
    }

    protected override string BoxId(string key) => $"sc_{key}";

    public bool IsShowing(CCSPlayerController player)
    {
        return _pending.ContainsKey(player.Slot)
               || (Viewers.TryGetValue(player.Slot, out BoxViewer<ScreamerContent>? viewer) && viewer.AnyVisible);
    }

    /// <summary>
    /// Shows <paramref name="content"/> after <paramref name="seconds"/>, then links <paramref name="handle"/> to it.
    /// A hide in the meantime cancels it.
    /// </summary>
    public void ShowLater(CCSPlayerController player, float seconds, ScreamerContent content, UiHandle handle)
    {
        int slot = player.Slot;
        CancelPending(slot);
        _pending[slot] = Later(seconds, () =>
        {
            _pending.Remove(slot);
            if (player is { IsValid: true })
            {
                handle.Link(Show(player, Key, content));
            }
        });
    }

    public override bool Forget(int slot)
    {
        bool pending = CancelPending(slot);
        return base.Forget(slot) || pending;
    }

    /// <summary>Round reset, map start, HideAll: the waiting screamers go too (a map change kills their timers).</summary>
    public override void ClearEveryone()
    {
        foreach (int slot in _pending.Keys.ToList())
        {
            CancelPending(slot);
        }

        base.ClearEveryone();
    }

    public bool CancelPending(int slot)
    {
        if (!_pending.Remove(slot, out SafeTimer? timer))
        {
            return false;
        }

        timer.Kill();
        return true;
    }

    /// <summary>Name of the screamer up for this player, null when none.</summary>
    public string? ShowingName(CCSPlayerController player)
    {
        if (!Viewers.TryGetValue(player.Slot, out BoxViewer<ScreamerContent>? viewer)
            || !viewer.Boxes.TryGetValue(Key, out Box<ScreamerContent>? box)
            || !box.Visible)
        {
            return null;
        }

        return box.Content?.Name;
    }

    protected override void Paint(CCSPlayerController player, BoxViewer<ScreamerContent> viewer, Box<ScreamerContent> box,
                                  ScreamerContent content)
    {
        ClientState client = viewer.Client;
        string id = box.Id;

        client.Group(player, id, "scare", ScreamerNames.Class(content.Name));
        client.FreeClasses(player, id, content.Extras.Classes);
    }
}
