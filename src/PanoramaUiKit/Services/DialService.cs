using CounterStrikeSharp.API.Core;
using PanoramaUiKit.Channels;
using PanoramaUiKit.Core;
using PanoramaUiKit.Shared.Common;
using PanoramaUiKit.Shared.Dials;

namespace PanoramaUiKit.Services;

internal sealed class DialService(LayoutRegistry registry) : ComponentService<FillChannel, DialLayout>(registry), IDialService
{
    public IUiHandle Show(CCSPlayerController player, DialOptions options)
    {
        DialLayout layout = options.Layout;
        FillChannel? channel = Channel(layout);

        DialPlacement placement = layout.Placements.Count > 0 ? layout.Placements[0] : options.Placement;
        if (layout.Placements.Contains(options.Placement))
        {
            placement = options.Placement;
        }

        ProgressContent content = new(options.Label, options.ValueText, options.ShowPercent, options.Value,
            options.Seconds, options.Direction, options, options.Size.Class());
        return WhenWarm(channel, player, dial => dial.Show(player, placement.Code(), content));
    }

    public void ShowAll(DialOptions options) => ForEachHuman(player => Show(player, options));

    public void Hide(CCSPlayerController player, DialPlacement? placement = null, DialLayout? layout = null)
    {
        foreach (FillChannel channel in Targets(layout))
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

    public void HideAll(DialPlacement? placement = null, DialLayout? layout = null)
    {
        foreach (FillChannel channel in Targets(layout))
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
