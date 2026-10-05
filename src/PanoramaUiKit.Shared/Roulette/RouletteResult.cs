using CounterStrikeSharp.API.Core;

namespace PanoramaUiKit.Shared.Roulette;

/// <summary>What <see cref="RouletteOptions.OnResult"/> is called with.</summary>
/// <param name="Player">Who spun. May be gone by now when <paramref name="Interrupted"/>.</param>
/// <param name="Item">The item drawn, the same instance that was in <see cref="RouletteOptions.Items"/>.</param>
/// <param name="Index">Position of <paramref name="Item"/> in <see cref="RouletteOptions.Items"/>.</param>
/// <param name="Interrupted">
/// True when the roulette ended before the strip stopped: cleared, replaced by another spin, the
/// player left, or the round restarted. The item was still drawn: award it or not, your call.
/// </param>
public sealed record RouletteResult(CCSPlayerController Player, RouletteItem Item, int Index, bool Interrupted);
