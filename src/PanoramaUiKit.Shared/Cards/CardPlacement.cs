using System.Text.Json.Serialization;

namespace PanoramaUiKit.Shared.Cards;

/// <summary>The corner a card is pinned to. Top left is where the radar sits.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<CardPlacement>))]
public enum CardPlacement
{
    TopLeft,
    TopRight,
    BottomLeft,
    BottomRight,
}
