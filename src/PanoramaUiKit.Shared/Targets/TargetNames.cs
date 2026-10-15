namespace PanoramaUiKit.Shared.Targets;

/// <summary>Enum to layout name mapping. These strings are the contract with the .xml / .css.</summary>
public static class TargetNames
{
    public static string Code(this TargetPosition position) => position switch
    {
        TargetPosition.Above => "t",
        TargetPosition.Below => "b",
        TargetPosition.Left => "l",
        TargetPosition.Right => "r",
        _ => "b",
    };

    public static string Class(this TargetSize size) => size switch
    {
        TargetSize.Small => "size-sm",
        TargetSize.Large => "size-lg",
        TargetSize.ExtraLarge => "size-xl",
        _ => "size-md",
    };
}
