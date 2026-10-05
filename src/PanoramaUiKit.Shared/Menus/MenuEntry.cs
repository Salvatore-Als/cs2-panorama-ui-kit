using CounterStrikeSharp.API.Core;
using PanoramaUiKit.Shared.Common;

namespace PanoramaUiKit.Shared.Menus;

/// <summary>
/// One entry of a menu. Build it with the factory of its kind: <see cref="Button"/>,
/// <see cref="Toggle"/>, <see cref="Select"/>, <see cref="Open"/>.
///
/// Toggle and select state is kept by the kit, per player and per open menu, starting from
/// <see cref="On"/> / <see cref="Selected"/>: a menu declared once works without the caller storing
/// anything. The callbacks report each change.
/// </summary>
public sealed record MenuEntry
{
    public required MenuEntryKind Kind { get; init; }

    /// <summary>Slot <c>{s:mn_item_&lt;n&gt;_label}</c>.</summary>
    public required string Label { get; init; }

    /// <summary>Second line. Slot <c>{s:mn_item_&lt;n&gt;_desc}</c>. Blank collapses it.</summary>
    public string? Description { get; init; }

    /// <summary>
    /// Right-hand value of a button or sub-menu entry (a price, a count). Slot
    /// <c>{s:mn_item_&lt;n&gt;_value}</c>; a select shows its current choice there instead.
    /// </summary>
    public string? Value { get; init; }

    /// <summary><c>style-&lt;name&gt;</c> on the entry. See <see cref="UiStyle"/>.</summary>
    public string Style { get; init; } = UiStyle.Neutral;

    /// <summary>Greyed out (class <c>disabled</c>); its clicks are dropped.</summary>
    public bool Disabled { get; init; }

    /// <summary>Button: clicked.</summary>
    public Action<CCSPlayerController>? OnClick { get; init; }

    /// <summary>Toggle: initial state.</summary>
    public bool On { get; init; }

    /// <summary>Toggle: flipped, with the new state.</summary>
    public Action<CCSPlayerController, bool>? OnToggle { get; init; }

    /// <summary>Select: the choices, cycled in order.</summary>
    public IReadOnlyList<string> Choices { get; init; } = [];

    /// <summary>Select: index of the initial choice.</summary>
    public int Selected { get; init; }

    /// <summary>Select: stepped, with the new index and its choice.</summary>
    public Action<CCSPlayerController, int, string>? OnChoose { get; init; }

    /// <summary>Submenu: built on click, so it can be live.</summary>
    public Func<CCSPlayerController, MenuOptions>? Submenu { get; init; }

    public static MenuEntry Button(string label, Action<CCSPlayerController> onClick, string? description = null) =>
        new() { Kind = MenuEntryKind.Button, Label = label, Description = description, OnClick = onClick };

    public static MenuEntry Toggle(string label, bool on, Action<CCSPlayerController, bool> onToggle, string? description = null) =>
        new() { Kind = MenuEntryKind.Toggle, Label = label, Description = description, On = on, OnToggle = onToggle };

    public static MenuEntry Select(string label, IReadOnlyList<string> choices, int selected,
                                   Action<CCSPlayerController, int, string> onChoose, string? description = null) =>
        new() { Kind = MenuEntryKind.Select, Label = label, Description = description, Choices = choices, Selected = selected, OnChoose = onChoose };

    public static MenuEntry Open(string label, Func<CCSPlayerController, MenuOptions> submenu, string? description = null) =>
        new() { Kind = MenuEntryKind.Submenu, Label = label, Description = description, Submenu = submenu };
}
