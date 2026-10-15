using PanoramaUiKit.Shared.Common;

namespace PanoramaUiKit.Shared.Cards;

/// <summary>
/// A card layout: one card per placement (four corners and the bottom centre), each with a title, a description and <see cref="Lines"/>
/// label / value lines. Contract "card" (docs/CONTRACTS.md). Declare it once, reuse the instance.
/// </summary>
public sealed class CardLayout : UiLayout
{
    public static readonly IReadOnlyList<CardPlacement> AllPlacements = Enum.GetValues<CardPlacement>();

    /// <summary>A card has no transition, so there is no exit to wait for.</summary>
    public CardLayout()
    {
        ExitSeconds = 0f;
        EntrySeconds = 0f;
    }

    public override string Component => "card";

    /// <summary>Placements the layout has a card for. A card asking for another uses the first one.</summary>
    public IReadOnlyList<CardPlacement> Placements { get; init; } = AllPlacements;

    /// <summary>Lines per card. Extra lines in <see cref="CardOptions.Lines"/> are dropped.</summary>
    public int Lines { get; init; } = 4;
}
