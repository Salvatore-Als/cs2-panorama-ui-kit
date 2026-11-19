using System.Text.Json.Serialization;

namespace PanoramaUiKit.Shared.Minimap;

/// <summary>Corner of the screen: <c>at-*</c> on <c>mm_frame</c>.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<MinimapPlacement>))]
public enum MinimapPlacement
{
    /// <summary>Top-right, over the kill feed. The default: the game's radar keeps the top-left.</summary>
    TopRight,

    /// <summary>Top-left, over the game's radar: hide the radar with it.</summary>
    TopLeft,

    /// <summary>Bottom-left, above the health and armour.</summary>
    BottomLeft,

    /// <summary>Bottom-right, above the ammo.</summary>
    BottomRight,
}

/// <summary>Size of the map: <c>size-*</c> on <c>mm_frame</c>, a <c>ui-scale</c> of the 256px map.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<MinimapSize>))]
public enum MinimapSize
{
    /// <summary>205px.</summary>
    Small,

    /// <summary>256px.</summary>
    Medium,

    /// <summary>320px.</summary>
    Large,
}

public static class MinimapPlacementExtensions
{
    /// <summary><c>at-tr</c>, <c>at-tl</c>, <c>at-bl</c>, <c>at-br</c>.</summary>
    public static string Class(this MinimapPlacement placement) => placement switch
    {
        MinimapPlacement.TopLeft => "at-tl",
        MinimapPlacement.BottomLeft => "at-bl",
        MinimapPlacement.BottomRight => "at-br",
        _ => "at-tr",
    };

    /// <summary><c>size-sm</c>, <c>size-md</c>, <c>size-lg</c>.</summary>
    public static string Class(this MinimapSize size) => size switch
    {
        MinimapSize.Small => "size-sm",
        MinimapSize.Large => "size-lg",
        _ => "size-md",
    };
}
