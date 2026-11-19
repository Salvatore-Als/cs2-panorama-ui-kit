using System.Text.Json.Serialization;

namespace PanoramaUiKit.Shared.Compass;

/// <summary>
/// Where the compass sits: <c>at-*</c> on <c>cp_bar</c>. A zone status at the <see cref="Zone.ZonePlacement"/> of
/// the same name stays right under it.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<CompassPlacement>))]
public enum CompassPlacement
{
    /// <summary>Top centre, under the game's team bar (alive players, round timer). The default.</summary>
    UnderHud,

    /// <summary>Against the top edge, over the team bar: for a server that hides it.</summary>
    Top,

    /// <summary>Bottom centre, above the inventory row.</summary>
    Bottom,

    /// <summary>Left edge, under the radar and the money.</summary>
    TopLeft,

    /// <summary>Right edge, under the kill feed (room for about five lines of it).</summary>
    TopRight,

    /// <summary>Right edge, vertically centred: away from every part of the game's HUD.</summary>
    Right,

    /// <summary>
    /// In the strip of a match bar with <see cref="Match.MatchOptions.Band"/>, over the game's team bar. A zone
    /// status at <see cref="Zone.ZonePlacement.Band"/> goes under the bar hanging from the strip.
    /// </summary>
    Band,
}

public static class CompassPlacementExtensions
{
    /// <summary>Class on the panel: <c>at-hud</c>, <c>at-top</c>, <c>at-bottom</c>, <c>at-tl</c>, <c>at-tr</c>, <c>at-right</c>, <c>at-band</c>.</summary>
    public static string Class(this CompassPlacement placement) => placement switch
    {
        CompassPlacement.Top => "at-top",
        CompassPlacement.Bottom => "at-bottom",
        CompassPlacement.TopLeft => "at-tl",
        CompassPlacement.TopRight => "at-tr",
        CompassPlacement.Right => "at-right",
        CompassPlacement.Band => "at-band",
        _ => "at-hud",
    };
}
