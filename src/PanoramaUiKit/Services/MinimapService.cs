using CounterStrikeSharp.API.Core;
using PanoramaUiKit.Channels;
using PanoramaUiKit.Core;
using PanoramaUiKit.Shared.Common;
using PanoramaUiKit.Shared.Minimap;

namespace PanoramaUiKit.Services;

internal sealed class MinimapService(LayoutRegistry registry) : ComponentService<MinimapChannel, MinimapLayout>(registry), IMinimapService
{
    public IUiHandle Show(CCSPlayerController player, MinimapOptions options) =>
        WhenWarm(Channel(options.Layout), player, channel => channel.Show(player, options));

    public void ShowAll(MinimapOptions options) => ForEachHuman(player => Show(player, options));

    public void SetZone(MinimapLayout? layout, MinimapZoneId id, MinimapZone? zone, float shrinkSeconds = 0f) =>
        Each(layout, channel => channel.SetZone(id, zone, shrinkSeconds));

    public void SetGlobalMark(MinimapLayout? layout, MinimapMark mark) => Each(layout, channel => channel.SetGlobalMark(mark));

    public void ClearGlobalMark(MinimapLayout? layout, int slot) => Each(layout, channel => channel.ClearGlobalMark(slot));

    public void SetMark(CCSPlayerController player, MinimapLayout layout, MinimapMark mark) => Channel(layout)?.SetMark(player, mark);

    public void ClearMark(CCSPlayerController player, MinimapLayout layout, int slot) => Channel(layout)?.ClearMark(player, slot);

    public void TrackPlayer(MinimapLayout? layout, CCSPlayerController target, MinimapTrack track) =>
        Each(layout, channel => channel.Track(null, target, track));

    public void TrackPlayerFor(CCSPlayerController viewer, MinimapLayout layout, CCSPlayerController target, MinimapTrack track) =>
        Channel(layout)?.Track(viewer, target, track);

    public bool IsShowing(CCSPlayerController player, MinimapLayout? layout = null) =>
        player is { IsValid: true } && Targets(layout).Any(channel => channel.IsShowing(player));

    public void Hide(CCSPlayerController player, MinimapLayout? layout = null)
    {
        foreach (MinimapChannel channel in Targets(layout))
        {
            channel.Hide(player);
        }
    }

    public void HideAll(MinimapLayout? layout = null)
    {
        foreach (MinimapChannel channel in Targets(layout))
        {
            channel.HideEveryone();
        }
    }

    public void Reset(MinimapLayout? layout = null)
    {
        foreach (MinimapChannel channel in Targets(layout))
        {
            channel.Reset();
        }
    }

    /// <summary>The layout given, or every minimap layout the kit knows.</summary>
    private void Each(MinimapLayout? layout, Action<MinimapChannel> action)
    {
        foreach (MinimapChannel channel in Targets(layout))
        {
            action(channel);
        }
    }
}
