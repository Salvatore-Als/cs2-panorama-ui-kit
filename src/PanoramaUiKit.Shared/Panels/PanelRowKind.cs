namespace PanoramaUiKit.Shared.Panels;

/// <summary>Which parts of a row show: class <c>kind-&lt;name&gt;</c> on <c>p_row_&lt;n&gt;</c>.</summary>
public enum PanelRowKind
{
    /// <summary>Section title, no control.</summary>
    Header,

    /// <summary>Title, description and a button.</summary>
    Action,

    /// <summary>Title, description and an on/off switch.</summary>
    Toggle,

    /// <summary>Title, description and a read-only value on the right.</summary>
    Info,

    /// <summary>Title, description and ‹ value › arrows to step through choices.</summary>
    Stepper,

    /// <summary>A paragraph inside the list: optional title and the description as body text, on the row background.</summary>
    Text,

    /// <summary>Free text between rows: no row background, no divider, just the paragraph.</summary>
    Paragraph,
}
