using CounterStrikeSharp.API.Core;
using PanoramaUiKit.Channels;
using PanoramaUiKit.Core;
using PanoramaUiKit.Shared.Common;
using PanoramaUiKit.Shared.Minimap;

namespace PanoramaUiKit.Services;

internal sealed class MinimapService(LayoutRegistry registry) : ComponentService<MinimapChannel, MinimapLayout>(registry), IMinimapService
{
    public void RegisterMap(string map, MinimapMap picture)
    {
        if (!string.IsNullOrWhiteSpace(map))
        {
            Registry.Maps.Register(map, picture);
        }
    }

    public MinimapMap? FindMap(string map) => Registry.Maps.Find(map);

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

    public void TrackPlayers(MinimapLayout? layout, IReadOnlyList<CCSPlayerController> targets, MinimapTrackList? list = null) =>
        Each(layout, channel => TrackList(list, targets,
                                          (target, track) => channel.Track(null, target, track),
                                          slot => channel.ClearGlobalMark(slot)));

    public void TrackPlayersFor(CCSPlayerController viewer, MinimapLayout layout, IReadOnlyList<CCSPlayerController> targets,
                                MinimapTrackList? list = null)
    {
        if (Channel(layout) is { } channel)
        {
            TrackList(list, targets, (target, track) => channel.Track(viewer, target, track), slot => channel.ClearMark(viewer, slot));
        }
    }

    /// <summary>One track per player in the list's slots, the rest of the range cleared.</summary>
    private static void TrackList(MinimapTrackList? list, IReadOnlyList<CCSPlayerController> targets,
                                  Action<CCSPlayerController, MinimapTrack> track, Action<int> clear)
    {
        list ??= new MinimapTrackList();
        List<CCSPlayerController> valid = targets.Where(t => t is { IsValid: true }).ToList();
        for (int i = 0; i < list.Slots; i++)
        {
            if (i < valid.Count)
            {
                track(valid[i], list.Track(i));
            }
            else
            {
                clear(list.FirstSlot + i);
            }
        }
    }

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
