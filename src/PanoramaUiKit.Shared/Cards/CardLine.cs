namespace PanoramaUiKit.Shared.Cards;

/// <summary>One line of a card: a muted label on the left, a value on the right. Either can be left out.</summary>
/// <param name="Label">Slot <c>{s:c_&lt;corner&gt;_line_&lt;n&gt;_label}</c>.</param>
/// <param name="Value">Slot <c>{s:c_&lt;corner&gt;_line_&lt;n&gt;_value}</c>.</param>
public readonly record struct CardLine(string? Label, string? Value = null)
{
    /// <summary>True when there is nothing to draw: the line is hidden.</summary>
    public bool IsBlank => string.IsNullOrWhiteSpace(Label) && string.IsNullOrWhiteSpace(Value);
}
