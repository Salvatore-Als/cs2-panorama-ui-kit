namespace PanoramaUiKit.Shared.Recap;

/// <summary>
/// The stat icons the bundled stylesheet has a rule for. Each name is a class <c>icon-&lt;name&gt;</c> carrying
/// the picture (<c>panorama/images/custom_game/uikit/stats/&lt;name&gt;.vsvg</c>), white and tinted by the tile's
/// style. A custom layout can add its own names.
/// </summary>
public static class RecapIcons
{
    public const string Kills = "kills";
    public const string Knockouts = "knockouts";
    public const string Points = "points";
    public const string Shots = "shots";
    public const string DamageDealt = "damage_dealt";
    public const string DamageTaken = "damage_taken";
    public const string Headshots = "headshots";
    public const string Revives = "revives";
    public const string Survival = "survival";
    public const string Accuracy = "accuracy";
    public const string Distance = "distance";
    public const string Assists = "assists";

    public static readonly IReadOnlyList<string> All =
    [
        Kills, Knockouts, Points, Shots, DamageDealt, DamageTaken, Headshots, Revives, Survival, Accuracy, Distance, Assists,
    ];

    /// <summary>Class of an icon on <c>rc_stat_&lt;n&gt;_icon</c>: <c>icon-&lt;name&gt;</c>.</summary>
    public static string Class(string name) => $"icon-{name}";
}
