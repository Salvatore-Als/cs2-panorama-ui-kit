using CounterStrikeSharp.API.Core;
using PanoramaUiKit.Channels.Base;
using PanoramaUiKit.Internal;
using PanoramaUiKit.Shared.Common;
using PanoramaUiKit.Shared.Targets;

namespace PanoramaUiKit.Channels;

internal sealed record TargetContent(string Text, TargetSize Size, float Seconds, UiContent Extras) : BoxContent
{
    public override float Lifetime => Seconds;
}

/// <summary>Text around the crosshair on one target layout (contract "target"): one box per position.</summary>
internal sealed class TargetChannel : AnchoredChannel<TargetContent>
{
    public TargetChannel(BasePlugin plugin, UiLayout layout, IReadOnlyList<string> keys) : base(plugin, layout, keys)
    {
    }

    protected override string BoxId(string key) => $"tg_{key}";

    protected override void Paint(CCSPlayerController player, BoxViewer<TargetContent> viewer, Box<TargetContent> box,
                                  TargetContent content)
    {
        ClientState client = viewer.Client;
        string id = box.Id;

        client.Text(player, $"{id}_text", content.Text.Trim());
        client.Group(player, id, "size", content.Size.Class());
        client.ExtraTexts(player, id, content.Extras.Texts);
        client.Group(player, id, "style", Names.StyleOrNeutral(content.Extras.Style));
        client.FreeClasses(player, id, content.Extras.Classes);
    }
}
