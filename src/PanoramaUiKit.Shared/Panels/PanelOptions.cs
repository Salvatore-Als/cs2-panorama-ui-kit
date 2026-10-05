using CounterStrikeSharp.API.Core;
using PanoramaUiKit.Shared.Common;

namespace PanoramaUiKit.Shared.Panels;

/// <summary>
/// A whole panel: header, pages in the left menu, footer hint. <see cref="UiContent.Style"/> is the
/// accent (top edge, selected page) and goes on <c>p_panel</c> with <see cref="UiContent.Classes"/>;
/// <see cref="UiContent.Texts"/> are written as <c>p_&lt;key&gt;</c>.
/// </summary>
public sealed record PanelOptions : UiContent
{
    /// <summary>Slot <c>{s:p_title}</c>.</summary>
    public required string Title { get; init; }

    /// <summary>Slot <c>{s:p_subtitle}</c>.</summary>
    public string Subtitle { get; init; } = string.Empty;

    /// <summary>Footer line, slot <c>{s:p_hint}</c>. Blank hides the footer (class <c>no-hint</c>).</summary>
    public string Hint { get; init; } = string.Empty;

    /// <summary>Left menu, top to bottom. Only the first layout <c>Pages</c> are shown.</summary>
    public required IReadOnlyList<PanelPage> Pages { get; init; }

    /// <summary>
    /// Called once when this panel leaves the player's screen: close button, round restart,
    /// disconnect, <see cref="IPanelService.Close"/>. Not called when another Open replaces it.
    /// </summary>
    public Action<CCSPlayerController>? OnClose { get; init; }

    /// <summary>Layout file to draw on, by name (<c>Name = "uikit_...")</c>. Required: there is no default.</summary>
    public required PanelLayout Layout { get; init; }
}
