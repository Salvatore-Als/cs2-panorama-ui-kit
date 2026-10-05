namespace PanoramaUiKit.Shared.Common;

/// <summary>
/// Style names every bundled layout defines. A style is a plain string: the kit adds
/// <c>style-&lt;name&gt;</c> on the component's root panel, nothing more. A custom layout can define
/// its own (<c>"neon"</c> becomes <c>style-neon</c>) - and should define at least these so a plugin
/// written against the default layout still looks right on it.
/// </summary>
public static class UiStyle
{
    public const string Neutral = "neutral";
    public const string Info = "info";
    public const string Success = "success";
    public const string Warn = "warn";
    public const string Danger = "danger";
    public const string Purple = "purple";
    public const string Gold = "gold";

    /// <summary>The ones a layout must define to pass scripts/uikit_contract.py check.</summary>
    public static readonly IReadOnlyList<string> Required = [Neutral, Info, Success, Warn, Danger];
}
