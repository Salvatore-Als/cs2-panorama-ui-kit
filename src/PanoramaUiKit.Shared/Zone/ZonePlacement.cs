using System.Text.Json.Serialization;

namespace PanoramaUiKit.Shared.Zone;

/// <summary>
/// Where the zone status sits: <c>at-*</c> on <c>zn_status</c>. The top ones line up with the compass at the
/// placement of the same name (<see cref="Compass.CompassPlacement"/>): the status goes right under it.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<ZonePlacement>))]
public enum ZonePlacement
{
    /// <summary>Top centre, under a compass under the game's team bar. The default.</summary>
    UnderHud,

    /// <summary>Top centre, under a compass against the top edge.</summary>
    Top,

    /// <summary>Under a match bar with its band (and a compass in the band).</summary>
    Band,

    /// <summary>Bottom centre, above a compass above the inventory row.</summary>
    Bottom,

    /// <summary>Top-left corner, under the radar and the money.</summary>
    TopLeft,

    /// <summary>Top-right corner, under the kill feed.</summary>
    TopRight,

    /// <summary>Bottom-left corner, above the health and armour.</summary>
    BottomLeft,

    /// <summary>Bottom-right corner, above the ammo.</summary>
    BottomRight,

    /// <summary>Right edge, vertically centred.</summary>
    Right,
}

public static class ZonePlacementExtensions
{
    /// <summary>Class on the panel: <c>at-hud</c>, <c>at-top</c>, <c>at-band</c>, <c>at-bottom</c>, <c>at-tl</c>, <c>at-tr</c>, <c>at-bl</c>, <c>at-br</c>, <c>at-right</c>.</summary>
    public static string Class(this ZonePlacement placement) => placement switch
    {
        ZonePlacement.Top => "at-top",
        ZonePlacement.Band => "at-band",
        ZonePlacement.Bottom => "at-bottom",
        ZonePlacement.TopLeft => "at-tl",
        ZonePlacement.TopRight => "at-tr",
        ZonePlacement.BottomLeft => "at-bl",
        ZonePlacement.BottomRight => "at-br",
        ZonePlacement.Right => "at-right",
        _ => "at-hud",
    };
}
