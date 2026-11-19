namespace PanoramaUiKit.Shared.Minimap;

/// <summary>
/// Marker kinds and states the bundled stylesheet has a rule for: <c>kind-&lt;name&gt;</c> and
/// <c>state-&lt;name&gt;</c> on <c>mm_mark_&lt;m&gt;</c>. A custom layout can add its own.
/// Kinds also set the stacking order: self over mate over ping over boss over npc over loot.
/// </summary>
public static class MinimapNames
{
    /// <summary>The player: an arrow, turned by <see cref="MinimapMark.Yaw"/>.</summary>
    public const string Self = "self";

    /// <summary>A teammate: a cyan arrow.</summary>
    public const string Mate = "mate";

    public const string Npc = "npc";
    public const string NpcBoss = "npc-boss";
    public const string Ping = "ping";
    public const string Loot = "loot";
    public const string Dot = "dot";

    /// <summary>Several markers in one: put the count in <see cref="MinimapMark.Label"/> ("x5").</summary>
    public const string Cluster = "cluster";

    public static readonly IReadOnlyList<string> Kinds = [Self, Mate, Npc, NpcBoss, Ping, Loot, Dot, Cluster];

    /// <summary>Red and pulsing.</summary>
    public const string Downed = "downed";

    /// <summary>Faded.</summary>
    public const string Dead = "dead";

    /// <summary>Pulsing in its own colour.</summary>
    public const string Alerted = "alerted";

    public static readonly IReadOnlyList<string> States = [Downed, Dead, Alerted];
}
