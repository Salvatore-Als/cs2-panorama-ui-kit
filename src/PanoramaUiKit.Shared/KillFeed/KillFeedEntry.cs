using PanoramaUiKit.Shared.Common;

namespace PanoramaUiKit.Shared.KillFeed;

/// <summary>
/// One row of the feed. Rows are a pool, newest on top: every push shifts the others down, so a
/// row's slots follow its entry. <see cref="UiContent.Style"/> and <see cref="UiContent.Classes"/>
/// go on the row (<c>kf_&lt;n&gt;</c>), <see cref="UiContent.Texts"/> are written as <c>kf_&lt;n&gt;_&lt;key&gt;</c>.
/// </summary>
public sealed record KillFeedEntry : UiContent
{
    /// <summary>Slot <c>{s:kf_&lt;n&gt;_victim}</c>.</summary>
    public required string Victim { get; init; }

    /// <summary>Slot <c>{s:kf_&lt;n&gt;_attacker}</c>. Blank collapses it - a feed that never names the killer just never sets it.</summary>
    public string? Attacker { get; init; }

    /// <summary>Caption between the two (weapon, "Éliminé", "HS"). Slot <c>{s:kf_&lt;n&gt;_tag}</c>. Blank collapses it.</summary>
    public string? Tag { get; init; }

    /// <summary>Seconds before the row leaves.</summary>
    public float Seconds { get; init; } = 7f;

    /// <summary>Layout file to draw on, by name (<c>Name = "uikit_...")</c>. Required: there is no default.</summary>
    public required KillFeedLayout Layout { get; init; }
}
