using System.Text.Json.Serialization;

namespace PanoramaUiKit.Shared.Progress;

[JsonConverter(typeof(JsonStringEnumConverter<ProgressPlacement>))]
public enum ProgressPlacement
{
    /// <summary>Under the top edge of the screen: a countdown, a capture.</summary>
    Top,

    /// <summary>Under the crosshair: an action the player is doing (use, defuse, channel).</summary>
    Crosshair,

    /// <summary>Above the HUD, bottom centre.</summary>
    Bottom,
}

/// <summary>What a bar that runs by itself does.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<ProgressDirection>))]
public enum ProgressDirection
{
    /// <summary>Starts empty, ends full: a cast, a defuse.</summary>
    Fill,

    /// <summary>Starts full, ends empty: a time limit.</summary>
    Drain,
}
