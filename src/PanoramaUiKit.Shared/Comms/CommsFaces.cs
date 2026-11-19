namespace PanoramaUiKit.Shared.Comms;

/// <summary>
/// The faces the bundled stylesheet has a rule for. Each name is a class <c>face-&lt;name&gt;</c> on
/// <c>cm_face</c>, carrying the picture (<c>panorama/images/custom_game/uikit/comms/&lt;name&gt;.vsvg</c>).
/// A custom layout can add its own names.
/// </summary>
public static class CommsFaces
{
    public const string Warden = "warden";
    public const string Jackal = "jackal";
    public const string Doctor = "doctor";
    public const string Oracle = "oracle";

    /// <summary>A colour photo (PNG), drawn untinted: the style still colours the frame and the text.</summary>
    public const string Villain = "villain";

    /// <summary>A silhouette with a question mark. Used when no face is named.</summary>
    public const string Unknown = "unknown";

    public static readonly IReadOnlyList<string> All = [Warden, Jackal, Doctor, Oracle, Villain, Unknown];

    /// <summary>Class of a face on <c>cm_face</c>: <c>face-&lt;name&gt;</c>.</summary>
    public static string Class(string name) => $"face-{name}";
}
