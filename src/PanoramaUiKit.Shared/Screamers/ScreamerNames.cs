namespace PanoramaUiKit.Shared.Screamers;

/// <summary>
/// The screamers the bundled stylesheet has a rule for. Each name is a class <c>scare-&lt;name&gt;</c>
/// carrying the picture (<c>panorama/images/custom_game/uikit/screamer/&lt;name&gt;_png.vtex</c>) and the
/// sound, the same for every one (<see cref="Sound"/>, played by the server). A custom layout can add its own names.
/// </summary>
public static class ScreamerNames
{
    /// <summary>The bundled sound, shipped with the kit addon. Default of <see cref="ScreamerOptions.Sound"/>.</summary>
    public const string Sound = "sounds/jumpscare.vsnd";

    public const string Momo = "momo";
    public const string Babadook = "babadook";
    public const string Jeff = "jeff";
    public const string Shadow = "shadow";

    public static readonly IReadOnlyList<string> All = [Momo, Babadook, Jeff, Shadow];

    /// <summary>Class of a screamer on <c>sc_image</c>: <c>scare-&lt;name&gt;</c>.</summary>
    public static string Class(string name) => $"scare-{name}";
}
