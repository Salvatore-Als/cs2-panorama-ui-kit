using CounterStrikeSharp.API.Core;
using PanoramaUiKit.Shared.Common;

namespace PanoramaUiKit.Shared.Cards;

/// <summary>
/// Info cards pinned to a corner of the screen. One per corner and player: a new card on a corner
/// that already has one rewrites it in place, so it is safe to call every second.
/// </summary>
public interface ICardService
{
    IUiHandle Show(CCSPlayerController player, CardOptions options);

    /// <summary>Same card to every connected human.</summary>
    void ShowAll(CardOptions options);

    /// <summary>Hides this player's card in that corner, or every corner when null. Null layout = on every card layout.</summary>
    void Hide(CCSPlayerController player, CardPlacement? placement = null, CardLayout? layout = null);

    void HideAll(CardPlacement? placement = null, CardLayout? layout = null);
}
