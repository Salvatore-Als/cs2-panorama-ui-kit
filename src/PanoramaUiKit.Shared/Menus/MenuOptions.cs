using CounterStrikeSharp.API.Core;
using PanoramaUiKit.Shared.Common;

namespace PanoramaUiKit.Shared.Menus;

/// <summary>
/// One level of a menu: title and entries, paginated by the layout's pool size (pager at the bottom,
/// <c>{s:mn_page}</c> = "1 / 3"). <see cref="UiContent.Style"/> (accent) and
/// <see cref="UiContent.Classes"/> go on <c>mn_menu</c>; <see cref="UiContent.Texts"/> are written as
/// <c>mn_&lt;key&gt;</c>.
/// </summary>
public sealed record MenuOptions : UiContent
{
    /// <summary>Slot <c>{s:mn_title}</c>.</summary>
    public required string Title { get; init; }

    /// <summary>Slot <c>{s:mn_subtitle}</c>. Blank collapses it.</summary>
    public string? Subtitle { get; init; }

    public required IReadOnlyList<MenuEntry> Entries { get; init; }

    /// <summary>
    /// Called once when the whole menu leaves the screen (close button, round restart, disconnect,
    /// <see cref="IMenuService.Close"/>). Only the root menu's callback runs; going back from a
    /// sub-menu is not a close.
    /// </summary>
    public Action<CCSPlayerController>? OnClose { get; init; }

    /// <summary>Layout file to draw on, by name (<c>Name = "uikit_...")</c>. Required: there is no default.</summary>
    public required MenuLayout Layout { get; init; }
}
