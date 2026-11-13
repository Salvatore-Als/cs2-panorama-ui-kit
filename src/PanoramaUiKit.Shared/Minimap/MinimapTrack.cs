namespace PanoramaUiKit.Shared.Minimap;

/// <summary>
/// A player followed by a marker (<see cref="IMinimapService.TrackPlayer"/>): the kit reads their position and view
/// every tick and moves the marker in <see cref="Slot"/>. Dead, the marker stays where they fell, faded (or goes, with
/// <see cref="ShowDead"/> off); disconnected, it goes; back on the server (same SteamID), it comes back.
/// </summary>
public sealed record MinimapTrack
{
    /// <summary>Marker slot, 1 to <see cref="MinimapLayout.Marks"/> - 1 (0 is the viewer's own arrow).</summary>
    public required int Slot { get; init; }

    /// <summary><c>kind-&lt;name&gt;</c>, see <see cref="MinimapNames"/>. An arrow kind turns with the player's view.</summary>
    public string Kind { get; init; } = MinimapNames.Mate;

    /// <summary>Text under the marker. Null = the player's name, blank = none.</summary>
    public string? Label { get; init; }

    /// <summary><c>style-&lt;name&gt;</c> over the kind's colour; null = the kind's.</summary>
    public string? Style { get; init; }

    /// <summary>Dead: keep the marker where they fell, <c>state-dead</c>. False = hide it until they respawn.</summary>
    public bool ShowDead { get; init; } = true;
}
