using System.Text.Json.Serialization;

namespace PanoramaUiKit.Shared.Minimap;

/// <summary>A zone circle on the map: centre and radius, world units.</summary>
public sealed record MinimapZone(float X, float Y, float Radius);

/// <summary>Which of the two circles: <c>mm_zone_cur</c> (red, the safe zone now) or <c>mm_zone_next</c> (white, the next one).</summary>
[JsonConverter(typeof(JsonStringEnumConverter<MinimapZoneId>))]
public enum MinimapZoneId
{
    Current,
    Next,
}
