using CounterStrikeSharp.API.Core;
using PanoramaUiKit.Channels;
using PanoramaUiKit.Core;
using PanoramaUiKit.Shared.Cards;
using PanoramaUiKit.Shared.Common;

namespace PanoramaUiKit.Services;

internal sealed class CardService(LayoutRegistry registry) : ComponentService<HudCardChannel, CardLayout>(registry), ICardService
{
    public IUiHandle Show(CCSPlayerController player, CardOptions options)
    {
        CardLayout layout = options.Layout;
        HudCardChannel? channel = Channel(layout);

        CardPlacement placement = layout.Placements.Count > 0 ? layout.Placements[0] : options.Placement;
        if (layout.Placements.Contains(options.Placement))
        {
            placement = options.Placement;
        }

        List<CardLine> lines = (options.Lines ?? []).Where(line => !line.IsBlank).Take(layout.Lines).ToList();

        // Nothing to say, nothing on screen.
        if (string.IsNullOrWhiteSpace(options.Title) && string.IsNullOrWhiteSpace(options.Description) && lines.Count == 0)
        {
            channel?.Hide(player, placement.Code());
            return Dead;
        }

        HudCardContent content = new(options.Title, options.Description, lines, options.Seconds, options);
        return WhenWarm(channel, player, card => card.Show(player, placement.Code(), content));
    }

    public void ShowAll(CardOptions options) => ForEachHuman(player => Show(player, options));

    public void Hide(CCSPlayerController player, CardPlacement? placement = null, CardLayout? layout = null)
    {
        foreach (HudCardChannel channel in Targets(layout))
        {
            if (placement == null)
            {
                channel.HideAll(player);
            }
            else
            {
                channel.Hide(player, placement.Value.Code());
            }
        }
    }

    public void HideAll(CardPlacement? placement = null, CardLayout? layout = null)
    {
        foreach (HudCardChannel channel in Targets(layout))
        {
            if (placement == null)
            {
                channel.HideEveryone();
                continue;
            }

            ForEachHuman(player => channel.Hide(player, placement.Value.Code()));
        }
    }
}
