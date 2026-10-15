using CounterStrikeSharp.API.Core;
using PanoramaUiKit.Channels;
using PanoramaUiKit.Core;
using PanoramaUiKit.Shared.Common;
using PanoramaUiKit.Shared.Targets;

namespace PanoramaUiKit.Services;

internal sealed class TargetService(LayoutRegistry registry) : ComponentService<TargetChannel, TargetLayout>(registry), ITargetService
{
    public IUiHandle Show(CCSPlayerController player, TargetOptions options)
    {
        TargetLayout layout = options.Layout;
        TargetChannel? channel = Channel(layout);

        TargetPosition position = layout.Positions.Count > 0 ? layout.Positions[0] : options.Position;
        if (layout.Positions.Contains(options.Position))
        {
            position = options.Position;
        }

        // Nothing to say, nothing on screen.
        if (string.IsNullOrWhiteSpace(options.Text))
        {
            channel?.Hide(player, position.Code());
            return Dead;
        }

        TargetContent content = new(options.Text, options.Size, options.Seconds, options);
        return WhenWarm(channel, player, target => target.Show(player, position.Code(), content));
    }

    public void ShowAll(TargetOptions options) => ForEachHuman(player => Show(player, options));

    public void Hide(CCSPlayerController player, TargetPosition? position = null, TargetLayout? layout = null)
    {
        foreach (TargetChannel channel in Targets(layout))
        {
            if (position == null)
            {
                channel.HideAll(player);
            }
            else
            {
                channel.Hide(player, position.Value.Code());
            }
        }
    }

    public void HideAll(TargetPosition? position = null, TargetLayout? layout = null)
    {
        foreach (TargetChannel channel in Targets(layout))
        {
            if (position == null)
            {
                channel.HideEveryone();
                continue;
            }

            ForEachHuman(player => channel.Hide(player, position.Value.Code()));
        }
    }
}
