using CounterStrikeSharp.API.Core;
using PanoramaUiKit.Channels.Base;
using PanoramaUiKit.Internal;
using PanoramaUiKit.Shared.Cards;
using PanoramaUiKit.Shared.Common;

namespace PanoramaUiKit.Channels;

/// <summary>What a card writes. Lines are already trimmed to the layout's pool and stripped of blank ones.</summary>
internal sealed record HudCardContent(
    string? Title,
    string? Description,
    IReadOnlyList<CardLine> Lines,
    float Seconds,
    UiContent Extras) : BoxContent
{
    public override float Lifetime => Seconds;
}

/// <summary>
/// Info cards on one card layout (contract "card"): one box per corner. Unused parts collapse
/// ("empty" on a label, "hidden" on a line), so any mix of title, description and lines works.
/// </summary>
internal sealed class HudCardChannel : AnchoredChannel<HudCardContent>
{
    private readonly CardLayout _layout;

    public HudCardChannel(BasePlugin plugin, CardLayout layout) : base(plugin, layout, layout.Placements.Select(p => p.Code()).ToList())
    {
        _layout = layout;
    }

    protected override string BoxId(string key) => $"c_{key}";

    protected override void Paint(CCSPlayerController player, BoxViewer<HudCardContent> viewer, Box<HudCardContent> box,
                                  HudCardContent content)
    {
        ClientState client = viewer.Client;
        string id = box.Id;

        client.OptionalText(player, $"{id}_title", content.Title);
        client.OptionalText(player, $"{id}_desc", content.Description);

        // Lines past the new content only need blanking if an earlier content filled them.
        int touched = Math.Max(content.Lines.Count, box.Painted);
        for (int i = 0; i < touched && i < _layout.Lines; i++)
        {
            string line = $"{id}_line_{i}";
            bool used = i < content.Lines.Count;
            if (used)
            {
                client.OptionalText(player, $"{line}_label", content.Lines[i].Label);
                client.OptionalText(player, $"{line}_value", content.Lines[i].Value);
            }

            client.Class(player, line, "hidden", !used);
        }

        box.Painted = content.Lines.Count;

        client.ExtraTexts(player, id, content.Extras.Texts);
        client.Group(player, id, "style", Names.StyleOrNeutral(content.Extras.Style));
        client.FreeClasses(player, id, content.Extras.Classes);
    }
}
