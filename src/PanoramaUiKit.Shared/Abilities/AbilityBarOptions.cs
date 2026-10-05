using PanoramaUiKit.Shared.Common;

namespace PanoramaUiKit.Shared.Abilities;

/// <summary>
/// The whole bar for one player. <see cref="UiContent.Style"/> goes on <c>ab_bar</c> (accent), as do
/// <see cref="UiContent.Classes"/>; <see cref="UiContent.Texts"/> are written as <c>ab_&lt;key&gt;</c>.
/// </summary>
public sealed record AbilityBarOptions : UiContent
{
    /// <summary>Left to right. More than the layout has are ignored; an empty list hides the bar.</summary>
    public required IReadOnlyList<AbilitySlot> Slots { get; init; }

    /// <summary>Layout file to draw on, by name (<c>Name = "uikit_...")</c>. Required: there is no default.</summary>
    public required AbilityLayout Layout { get; init; }
}
