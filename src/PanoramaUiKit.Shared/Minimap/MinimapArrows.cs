namespace PanoramaUiKit.Shared.Minimap;

/// <summary>
/// Styles of the player's own arrow (<see cref="MinimapOptions.SelfArrow"/>): class <c>arrow-&lt;name&gt;</c> on the frame.
/// The arrow turns with the player's view; a badge, dot or reticle around it stays still. Pictures in
/// <c>uikit/minimap/arrows/</c>, shipped with the kit addon. A custom layout can add its own names.
/// </summary>
public static class MinimapArrows
{
    /// <summary>Gold arrow in a gold ring: the default, no class.</summary>
    public const string Classic = "classic";

    /// <summary>White arrow with a black outline: readable on any ground.</summary>
    public const string Chevron = "chevron";

    /// <summary>Bright yellow arrow with a drop shadow, no ring.</summary>
    public const string Shadow = "shadow";

    /// <summary>Red and white compass needle.</summary>
    public const string Needle = "needle";

    /// <summary>Plain cyan triangle.</summary>
    public const string Triangle = "triangle";

    /// <summary>A still dot and a white cone showing where the player looks.</summary>
    public const string Cone = "cone";

    /// <summary>Round gold disc with a small point for the direction.</summary>
    public const string Notch = "notch";

    /// <summary>Two-tone diamond, light front, dark back.</summary>
    public const string Diamond = "diamond";

    /// <summary>Gold arrow sending out a ring every 1.8 s.</summary>
    public const string Wave = "wave";

    /// <summary>Blue disc ringed in white, white arrow inside.</summary>
    public const string Disc = "disc";

    /// <summary>Dark hexagon edged in gold, gold arrow inside.</summary>
    public const string Badge = "badge";

    /// <summary>Thick neon green V.</summary>
    public const string Neon = "neon";

    /// <summary>Pink arrow with a thick white outline.</summary>
    public const string Outline = "outline";

    /// <summary>Drop shape, its tip pointing the way.</summary>
    public const string Drop = "drop";

    /// <summary>Still aiming reticle, small red arrow in the middle.</summary>
    public const string Reticle = "reticle";

    /// <summary>Arrow followed by a fading trail.</summary>
    public const string Trail = "trail";

    /// <summary>Navy shield with a gold arrow.</summary>
    public const string Shield = "shield";

    /// <summary>Two-faced arrow, light and dark halves.</summary>
    public const string Bevel = "bevel";

    /// <summary>Small white dot and a large soft halo.</summary>
    public const string Minimal = "minimal";

    public static readonly IReadOnlyList<string> All =
    [
        Classic, Chevron, Shadow, Needle, Triangle, Cone, Notch, Diamond, Wave, Disc,
        Badge, Neon, Outline, Drop, Reticle, Trail, Shield, Bevel, Minimal,
    ];
}
