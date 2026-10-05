namespace PanoramaUiKit.Shared.Toasts;

/// <summary>Enum to layout name mapping. These strings are the contract with the .xml / .css.</summary>
public static class ToastNames
{
    public static string Code(this ToastPlacement placement) => placement switch
    {
        ToastPlacement.TopLeft => "tl",
        ToastPlacement.TopCenter => "tc",
        ToastPlacement.TopRight => "tr",
        ToastPlacement.MiddleLeft => "ml",
        ToastPlacement.MiddleRight => "mr",
        ToastPlacement.BottomLeft => "bl",
        ToastPlacement.BottomCenter => "bc",
        ToastPlacement.BottomRight => "br",
        _ => "tr",
    };

    public static ToastAnimation DefaultAnimation(this ToastPlacement placement) => placement switch
    {
        ToastPlacement.TopLeft or ToastPlacement.MiddleLeft or ToastPlacement.BottomLeft => ToastAnimation.FromLeft,
        ToastPlacement.TopRight or ToastPlacement.MiddleRight or ToastPlacement.BottomRight => ToastAnimation.FromRight,
        ToastPlacement.TopCenter => ToastAnimation.FromTop,
        _ => ToastAnimation.FromBottom,
    };

    public static string Class(this ToastAnimation animation) => animation switch
    {
        ToastAnimation.FromLeft => "anim-from-left",
        ToastAnimation.FromRight => "anim-from-right",
        ToastAnimation.FromTop => "anim-from-top",
        ToastAnimation.FromBottom => "anim-from-bottom",
        _ => "anim-fade",
    };

    /// <summary>Card id: <c>t_tr_0</c>. Every other slot of the card is this plus a suffix.</summary>
    public static string CardId(ToastPlacement placement, int index) => $"t_{placement.Code()}_{index}";
}
