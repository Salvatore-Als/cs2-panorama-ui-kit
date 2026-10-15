using System.Text.Json.Serialization;

namespace PanoramaUiKit.Shared.Dials;

[JsonConverter(typeof(JsonStringEnumConverter<DialPlacement>))]
public enum DialPlacement
{
    /// <summary>A ring centred on the crosshair.</summary>
    Crosshair,

    /// <summary>Above the HUD, bottom centre.</summary>
    Bottom,

    /// <summary>Above the HUD, bottom left.</summary>
    BottomLeft,

    /// <summary>Above the HUD, bottom right.</summary>
    BottomRight,
}

/// <summary>Ring size, applied as <c>size-sm</c>, <c>size-md</c>, <c>size-lg</c> or <c>size-xl</c>.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<DialSize>))]
public enum DialSize
{
    Small,
    Medium,
    Large,
    ExtraLarge,
}
