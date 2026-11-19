namespace PanoramaUiKit.Shared.Minimap;

/// <summary>
/// Marker colours the bundled stylesheets have a rule for, beside the <see cref="Common.UiStyle"/> names: pass one as
/// <see cref="MinimapMark.Style"/> (<c>style-&lt;name&gt;</c>, icon and label). A custom layout can add its own.
/// </summary>
public static class MinimapColors
{
    public const string White = "white";
    public const string Red = "red";
    public const string Orange = "orange";
    public const string Yellow = "yellow";
    public const string Green = "green";
    public const string Cyan = "cyan";
    public const string Blue = "blue";
    public const string Pink = "pink";

    public static readonly IReadOnlyList<string> All = [White, Red, Orange, Yellow, Green, Cyan, Blue, Pink];
}
