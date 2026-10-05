using CounterStrikeSharp.API.Core;

namespace PanoramaUiKit.Shared.Common;

/// <summary>
/// One thing on one player's screen. Safe to keep forever: <see cref="Clear"/> after it is gone does
/// nothing. A Show that could not show anything (bot, invalid player, refused layout) returns a
/// handle that is already gone.
/// </summary>
public interface IUiHandle
{
    CCSPlayerController? Player { get; }

    bool IsVisible { get; }

    /// <summary>Dismisses early, still playing the exit animation.</summary>
    void Clear();
}
