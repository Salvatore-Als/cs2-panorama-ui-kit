using CounterStrikeSharp.API.Core;
using PanoramaUiKit.Channels.Base;
using PanoramaUiKit.Internal;
using PanoramaUiKit.Shared.Common;
using PanoramaUiKit.Shared.Overlays;

namespace PanoramaUiKit.Channels;

internal sealed record OverlayContent(string Effect, float Seconds, bool KeepOnDeath, UiContent Extras) : BoxContent
{
    public override float Lifetime => Seconds;
}

/// <summary>
/// The full-screen effect of one overlay layout (contract "overlay"): a single box, <c>ov_fx</c>, whose look is one
/// class, <c>fx-&lt;name&gt;</c>. Retiring it closes the layout, which scrubs the class.
/// </summary>
internal sealed class OverlayChannel : AnchoredChannel<OverlayContent>
{
    public const string Key = "fx";

    public OverlayChannel(BasePlugin plugin, OverlayLayout layout) : base(plugin, layout, [Key])
    {
    }

    protected override string BoxId(string key) => $"ov_{key}";

    public bool IsShowing(CCSPlayerController player) =>
        Viewers.TryGetValue(player.Slot, out BoxViewer<OverlayContent>? viewer) && viewer.AnyVisible;

    /// <summary>The player died: an effect shown with <c>KeepOnDeath = false</c> goes.</summary>
    public void PlayerDied(CCSPlayerController player)
    {
        if (Viewers.TryGetValue(player.Slot, out BoxViewer<OverlayContent>? viewer)
            && viewer.Boxes.TryGetValue(Key, out Box<OverlayContent>? box)
            && box is { Visible: true, Content.KeepOnDeath: false })
        {
            Hide(player, Key);
        }
    }

    protected override void Paint(CCSPlayerController player, BoxViewer<OverlayContent> viewer, Box<OverlayContent> box,
                                  OverlayContent content)
    {
        ClientState client = viewer.Client;
        client.Group(player, box.Id, "fx", $"fx-{content.Effect}");
        client.Group(player, box.Id, "style", Names.StyleOrNeutral(content.Extras.Style));
        client.FreeClasses(player, box.Id, content.Extras.Classes);
    }
}
