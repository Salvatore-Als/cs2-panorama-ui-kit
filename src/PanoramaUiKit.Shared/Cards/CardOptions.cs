using PanoramaUiKit.Shared.Common;

namespace PanoramaUiKit.Shared.Cards;

/// <summary>
/// Everything a card can be. Title, description and lines are all optional and collapse on their
/// own: a card can be a title, a description, a list of lines, or any mix. With none of them the
/// card is hidden.
/// </summary>
public sealed record CardOptions : UiContent
{
    /// <summary>Headline. Slot <c>{s:c_&lt;corner&gt;_title}</c>. Collapsed when blank.</summary>
    public string? Title { get; init; }

    /// <summary>Paragraph. Slot <c>{s:c_&lt;corner&gt;_desc}</c>. Collapsed when blank.</summary>
    public string? Description { get; init; }

    /// <summary>Label / value lines, in order. Blank lines are skipped; more than the layout has are dropped.</summary>
    public IReadOnlyList<CardLine>? Lines { get; init; }

    /// <summary>Falls back to the layout's first placement when the layout does not have this one.</summary>
    public CardPlacement Placement { get; init; } = CardPlacement.TopLeft;

    /// <summary>Seconds on screen. Zero or less stays until hidden.</summary>
    public float Seconds { get; init; }

    /// <summary>Layout file to draw on, by name (<c>Name = "uikit_...")</c>. Required: there is no default.</summary>
    public required CardLayout Layout { get; init; }
}
