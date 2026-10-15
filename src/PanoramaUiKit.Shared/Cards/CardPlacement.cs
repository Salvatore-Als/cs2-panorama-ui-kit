using System.Text.Json.Serialization;

namespace PanoramaUiKit.Shared.Cards;

/// <summary>Where a card is pinned: a corner, or the bottom centre. Top left is where the radar sits.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<CardPlacement>))]
public enum CardPlacement
{
    TopLeft,
    TopRight,
    BottomLeft,
    BottomRight,

    /// <summary>Above the HUD, in the middle of the screen.</summary>
    BottomCenter,
}
