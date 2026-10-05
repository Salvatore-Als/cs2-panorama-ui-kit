using PanoramaUiKit.Shared.Common;

namespace PanoramaUiKit.Shared.Announcements;

/// <summary>
/// Centred card: reveal, objective, round intro. Kicker / title / description.
/// </summary>
public sealed record AnnounceOptions : UiContent
{
    /// <summary>Big line. Slot <c>{s:a_title}</c>.</summary>
    public required string Title { get; init; }

    /// <summary>Line under the title. Slot <c>{s:a_desc}</c>. Blank collapses it.</summary>
    public string? Description { get; init; }

    /// <summary>Eyebrow above the title. Slot <c>{s:a_kicker}</c>. Blank collapses it.</summary>
    public string? Kicker { get; init; }

    /// <summary>
    /// Plays the title's entry animation (the "slam": scale down + flash). Class <c>animate-title</c>
    /// on <c>a_card</c>; a layout only animates the title under that class.
    /// </summary>
    public bool AnimateTitle { get; init; } = true;

    /// <summary>Seconds on screen. Zero or less stays until hidden.</summary>
    public float Seconds { get; init; } = 5f;

    /// <summary>Layout file to draw on, by name (<c>Name = "uikit_...")</c>. Required: there is no default.</summary>
    public required AnnounceLayout Layout { get; init; }
}
