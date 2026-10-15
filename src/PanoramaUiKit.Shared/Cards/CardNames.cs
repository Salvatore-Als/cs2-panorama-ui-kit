namespace PanoramaUiKit.Shared.Cards;

/// <summary>Enum to layout name mapping. These strings are the contract with the .xml / .css.</summary>
public static class CardNames
{
    public static string Code(this CardPlacement placement) => placement switch
    {
        CardPlacement.TopLeft => "tl",
        CardPlacement.TopRight => "tr",
        CardPlacement.BottomLeft => "bl",
        CardPlacement.BottomRight => "br",
        CardPlacement.BottomCenter => "bc",
        _ => "tl",
    };

    /// <summary>Card id: <c>c_tl</c>. Every other slot of the card is this plus a suffix.</summary>
    public static string CardId(CardPlacement placement) => $"c_{placement.Code()}";
}
