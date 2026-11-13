namespace PanoramaUiKit.Shared.Minimap;

/// <summary>
/// Map pictures the bundled stylesheet has a rule for, each a class <c>map-&lt;name&gt;</c> on <c>mm_map</c>.
///
/// <see cref="Dust2"/> ships with the kit addon (<c>uikit/minimap/maps/de_dust2.png</c>, 1024px). The other official
/// maps use the game's own radar pictures (<c>panorama/images/overheadmaps/&lt;map&gt;_radar_psd</c>),
/// which every client already has: nothing to ship. Their <see cref="Official"/> bounds come from the game's
/// overview files; check them against <c>resource/overviews/&lt;map&gt;.txt</c> after a map update.
///
/// <see cref="Slot"/> names a picture the addon does not ship, for another addon (a custom map):
/// <c>panorama/images/custom_game/uikit/minimap/maps/slot_&lt;n&gt;.png</c> with its <c>slot_&lt;n&gt;_png.vtex</c>,
/// with <see cref="MinimapBounds"/> of your own.
/// </summary>
public static class MinimapMaps
{
    public const string Dust2 = "de_dust2";
    public const string Mirage = "de_mirage";
    public const string Inferno = "de_inferno";
    public const string Nuke = "de_nuke";
    public const string Overpass = "de_overpass";
    public const string Vertigo = "de_vertigo";
    public const string Ancient = "de_ancient";
    public const string Anubis = "de_anubis";
    public const string Train = "de_train";
    public const string Office = "cs_office";
    public const string Italy = "cs_italy";

    /// <summary>Slots in the bundled stylesheet: <c>slot-1</c> to <c>slot-8</c>.</summary>
    public const int Slots = 8;

    /// <summary>Bounds of the official maps, from their overview files (<c>pos_x</c>, <c>pos_y</c>, <c>scale</c>, 1024px).</summary>
    public static readonly IReadOnlyDictionary<string, MinimapBounds> Official = new Dictionary<string, MinimapBounds>(StringComparer.OrdinalIgnoreCase)
    {
        [Dust2] = MinimapBounds.FromOverview(-2476f, 3239f, 4.4f),
        [Mirage] = MinimapBounds.FromOverview(-3230f, 1713f, 5.0f),
        [Inferno] = MinimapBounds.FromOverview(-2087f, 3870f, 4.9f),
        [Nuke] = MinimapBounds.FromOverview(-3453f, 2887f, 7.0f),
        [Overpass] = MinimapBounds.FromOverview(-4831f, 1781f, 5.2f),
        [Vertigo] = MinimapBounds.FromOverview(-3168f, 1762f, 4.0f),
        [Ancient] = MinimapBounds.FromOverview(-2953f, 2164f, 5.0f),
        [Anubis] = MinimapBounds.FromOverview(-2796f, 3328f, 5.22f),
        [Train] = MinimapBounds.FromOverview(-2308f, 2078f, 4.082077f),
        [Office] = MinimapBounds.FromOverview(-1838f, 1858f, 4.1f),
        [Italy] = MinimapBounds.FromOverview(-2647f, 2592f, 4.6f),
    };

    /// <summary>Name of picture slot <paramref name="n"/> (1 to <see cref="Slots"/>): <c>slot-&lt;n&gt;</c>.</summary>
    public static string Slot(int n) => $"slot-{n}";

    /// <summary>Class of a map: <c>map-&lt;name&gt;</c>.</summary>
    public static string Class(string name) => $"map-{name}";
}
