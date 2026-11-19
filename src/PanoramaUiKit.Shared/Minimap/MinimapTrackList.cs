namespace PanoramaUiKit.Shared.Minimap;

/// <summary>
/// How <see cref="IMinimapService.TrackPlayers"/> lays out a list of players: a range of marker slots, one player per
/// slot in list order, a colour each from <see cref="Colors"/> in turn.
/// </summary>
public sealed record MinimapTrackList
{
    /// <summary>First marker slot of the range (0 is the viewer's own arrow).</summary>
    public int FirstSlot { get; init; } = 1;

    /// <summary>
    /// Slots in the range. Players past it are not drawn; slots of the range a shorter list leaves free are cleared, so
    /// calling again with fewer players takes the others off.
    /// </summary>
    public int Slots { get; init; } = 32;

    /// <summary><c>kind-&lt;name&gt;</c> of every marker, see <see cref="MinimapNames"/>. An arrow kind turns with the player's view.</summary>
    public string Kind { get; init; } = MinimapNames.Mate;

    /// <summary>
    /// Colours (<see cref="MinimapColors"/> or <see cref="Common.UiStyle"/> names), the n-th player taking the n-th, round
    /// and round. Null or empty = the kind's colour for everyone.
    /// </summary>
    public IReadOnlyList<string>? Colors { get; init; } = MinimapColors.All;

    /// <summary>The player's name under their marker. False = no label.</summary>
    public bool Names { get; init; } = true;

    /// <summary>Dead: keep the marker where they fell, faded. False = hide it until they respawn.</summary>
    public bool ShowDead { get; init; } = true;

    /// <summary>The track for the player at <paramref name="index"/> of the list.</summary>
    public MinimapTrack Track(int index) => new()
    {
        Slot = FirstSlot + index,
        Kind = Kind,
        Style = Colors is { Count: > 0 } colors ? colors[index % colors.Count] : null,
        Label = Names ? null : string.Empty,
        ShowDead = ShowDead,
    };
}
