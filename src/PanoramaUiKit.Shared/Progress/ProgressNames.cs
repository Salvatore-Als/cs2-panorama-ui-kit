namespace PanoramaUiKit.Shared.Progress;

/// <summary>Enum to layout name mapping. These strings are the contract with the .xml / .css.</summary>
public static class ProgressNames
{
    public static string Code(this ProgressPlacement placement) => placement switch
    {
        ProgressPlacement.Top => "t",
        ProgressPlacement.Crosshair => "c",
        ProgressPlacement.Bottom => "b",
        _ => "c",
    };
}
