using System.Text.Json.Serialization;

namespace PanoramaUiKit.Shared.Targets;

/// <summary>Where the text sits relative to the crosshair (the screen centre).</summary>
[JsonConverter(typeof(JsonStringEnumConverter<TargetPosition>))]
public enum TargetPosition
{
    Above,
    Below,
    Left,
    Right,
}
