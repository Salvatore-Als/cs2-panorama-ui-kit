using CounterStrikeSharp.API.Core;
using PanoramaUiKit.Shared.Common;

namespace PanoramaUiKit.Shared.Panels;

/// <summary>
/// One row of a page. Build them with the static helpers. Callbacks get the player who clicked; the
/// panel rebuilds and repaints right after, so a callback only changes your state, never the UI.
/// </summary>
public sealed record PanelRow
{
    public required PanelRowKind Kind { get; init; }

    /// <summary>Slot <c>{s:p_row_&lt;n&gt;_title}</c>. Blank collapses it (class <c>no-title</c>).</summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>Slot <c>{s:p_row_&lt;n&gt;_desc}</c>. Blank collapses it (class <c>no-desc</c>).</summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>Right-hand value (Info) or current choice (Stepper). Slot <c>{s:p_row_&lt;n&gt;_value}</c>.</summary>
    public string Value { get; init; } = string.Empty;

    /// <summary>Button label (Action). Slot <c>{s:p_row_&lt;n&gt;_btn}</c>.</summary>
    public string Button { get; init; } = string.Empty;

    /// <summary>Colour of the button, value or title: <c>style-&lt;name&gt;</c> on the row. See <see cref="UiStyle"/>.</summary>
    public string Style { get; init; } = UiStyle.Neutral;

    /// <summary>Switch state (Toggle): class <c>on</c>.</summary>
    public bool On { get; init; }

    /// <summary>Greyed out (class <c>disabled</c>); its clicks are dropped.</summary>
    public bool Disabled { get; init; }

    /// <summary>Highlighted (class <c>selected</c>): an open accordion row.</summary>
    public bool Selected { get; init; }

    /// <summary>Indented under the row above (class <c>nested</c>): an accordion row's details.</summary>
    public bool Nested { get; init; }

    /// <summary>Button pressed (Action) or switch flipped (Toggle).</summary>
    public Action<CCSPlayerController>? OnClick { get; init; }

    /// <summary>Stepper ‹ arrow.</summary>
    public Action<CCSPlayerController>? OnPrevious { get; init; }

    /// <summary>Stepper › arrow.</summary>
    public Action<CCSPlayerController>? OnNext { get; init; }

    public static PanelRow Header(string title) =>
        new() { Kind = PanelRowKind.Header, Title = title };

    public static PanelRow Text(string title, string description, string style = UiStyle.Neutral) =>
        new() { Kind = PanelRowKind.Text, Title = title, Description = description, Style = style };

    /// <summary>Plain text outside the list look. Slot <c>p_row_&lt;n&gt;_desc</c>; style colours the text.</summary>
    public static PanelRow Paragraph(string text, string style = UiStyle.Neutral) =>
        new() { Kind = PanelRowKind.Paragraph, Description = text, Style = style };

    public static PanelRow Info(string title, string description, string value, string style = UiStyle.Neutral) =>
        new() { Kind = PanelRowKind.Info, Title = title, Description = description, Value = value, Style = style };

    public static PanelRow Action(string title, string description, string button, Action<CCSPlayerController> onClick,
                                  string style = UiStyle.Neutral) =>
        new() { Kind = PanelRowKind.Action, Title = title, Description = description, Button = button, OnClick = onClick, Style = style };

    public static PanelRow Toggle(string title, string description, bool on, Action<CCSPlayerController> onToggle) =>
        new() { Kind = PanelRowKind.Toggle, Title = title, Description = description, On = on, OnClick = onToggle };

    public static PanelRow Stepper(string title, string description, string value,
                                   Action<CCSPlayerController> onPrevious, Action<CCSPlayerController> onNext) =>
        new() { Kind = PanelRowKind.Stepper, Title = title, Description = description, Value = value, OnPrevious = onPrevious, OnNext = onNext };
}
