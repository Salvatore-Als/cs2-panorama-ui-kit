using CounterStrikeSharp.API.Core;
using PanoramaUiKit.Channels;
using PanoramaUiKit.Core;
using PanoramaUiKit.Shared.Common;
using PanoramaUiKit.Shared.Progress;

namespace PanoramaUiKit.Services;

internal sealed class ProgressService(LayoutRegistry registry) : ComponentService<FillChannel, ProgressLayout>(registry), IProgressService
{
    public IUiHandle Show(CCSPlayerController player, ProgressOptions options)
    {
        ProgressLayout layout = options.Layout;
        FillChannel? channel = Channel(layout);

        ProgressPlacement placement = layout.Placements.Count > 0 ? layout.Placements[0] : options.Placement;
        if (layout.Placements.Contains(options.Placement))
        {
            placement = options.Placement;
        }

        ProgressContent content = new(options.Label, options.ValueText, options.ShowPercent, options.Value,
            options.Seconds, options.Direction, options);
        return WhenWarm(channel, player, bar => bar.Show(player, placement.Code(), content));
    }

    public void ShowAll(ProgressOptions options) => ForEachHuman(player => Show(player, options));

    public void Hide(CCSPlayerController player, ProgressPlacement? placement = null, ProgressLayout? layout = null)
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

    public void HideAll(ProgressPlacement? placement = null, ProgressLayout? layout = null)
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
