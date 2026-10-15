namespace PanoramaUiKit.Shared.Dials;

/// <summary>Enum to layout name mapping. These strings are the contract with the .xml / .css.</summary>
public static class DialNames
{
    public static string Code(this DialPlacement placement) => placement switch
    {
        DialPlacement.Crosshair => "c",
        DialPlacement.Bottom => "b",
        DialPlacement.BottomLeft => "bl",
        DialPlacement.BottomRight => "br",
        _ => "c",
    };

    public static string Class(this DialSize size) => size switch
    {
        DialSize.Small => "size-sm",
        DialSize.Large => "size-lg",
        DialSize.ExtraLarge => "size-xl",
        _ => "size-md",
    };
}
