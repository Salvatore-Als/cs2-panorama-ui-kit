using CounterStrikeSharp.API.Core;
using PanoramaUiKit.Shared.Abilities;
using PanoramaUiKit.Shared.Announcements;
using PanoramaUiKit.Shared.Common;
using PanoramaUiKit.Shared.KillFeed;
using PanoramaUiKit.Shared.Match;
using PanoramaUiKit.Shared.Menus;
using PanoramaUiKit.Shared.Panels;
using PanoramaUiKit.Shared.Roulette;
using PanoramaUiKit.Shared.Banners;
using PanoramaUiKit.Shared.Toasts;
using PanoramaUiKit.Shared.Victory;
using PanoramaUiKit.Shared.Votes;

namespace PanoramaUiKit.Shared.Api;

/// <summary>
/// Entry point of the kit, exposed by the PanoramaUiKit plugin as a CounterStrikeSharp capability.
///
/// <code>
/// private static readonly PluginCapability&lt;IPanoramaUiKit&gt; UiKit = new(IPanoramaUiKit.CapabilityName);
///
/// public override void OnAllPluginsLoaded(bool hotReload)
/// {
///     IPanoramaUiKit? kit = UiKit.Get();
///     kit?.Toast(player, Layouts.Toast, "Toast title", style: UiStyle.Danger);
/// }
/// </code>
///
/// Every component draws on a layout file the caller names (<c>Layout</c> on the options, required):
/// <c>new ToastLayout { Name = "uikit_toast" }</c> for the bundled one, or any file that implements
/// the component's contract (docs/CONTRACTS.md) - same ids, same <c>{s:...}</c> slots, same classes,
/// its own look.
///
/// Native calls are not thread-safe: after an await, come back through Server.NextFrame first.
/// </summary>
public interface IPanoramaUiKit
{
    public const string CapabilityName = "panoramauikit:api";

    /// <summary>Bumped on breaking changes to this interface.</summary>
    public const int ApiVersion = 1;

    /// <summary><see cref="ApiVersion"/> of the running kit, for consumers built against another one.</summary>
    int Version { get; }

    /// <summary>Stacked, auto-dismissing notifications. Per player.</summary>
    IToastService Toasts { get; }

    /// <summary>One sticky line across the top. Per player, with an everyone shortcut.</summary>
    IBannerService Banners { get; }

    /// <summary>Big centred card (reveal, objective, round intro). One per player.</summary>
    IAnnounceService Announcements { get; }

    /// <summary>Per-player ability bar (keys, cooldowns, hold gauges). Tick-friendly.</summary>
    IAbilityService Abilities { get; }

    /// <summary>Server-wide top status bar (sides + timer). Tick-friendly.</summary>
    IMatchService Match { get; }

    /// <summary>Server-wide feed, newest on top (kills, events).</summary>
    IKillFeedService KillFeed { get; }

    /// <summary>End of round screen with chips and a restart countdown. Per player.</summary>
    IVictoryService Victory { get; }

    /// <summary>Case-opening roulette: a strip of cards slows down on a weighted draw, with a callback. Per player.</summary>
    IRouletteService Roulettes { get; }

    /// <summary>Screen of a vote: question, choices with live counts, countdown, clicks. Takes the mouse.</summary>
    IVoteService Votes { get; }

    /// <summary>Modal panel with pages in a left menu, typed rows, clicks. Takes the mouse.</summary>
    IPanelService Panels { get; }

    /// <summary>Menus and sub-menus with back and pagination. Takes the mouse.</summary>
    IMenuService Menus { get; }


    /// <summary>
    /// Spawns the layout entity now instead of on first use. Call it for every layout your plugin
    /// draws on from OnAllPluginsLoaded: creating an entity in the same tick as its first writes
    /// sends the client one big burst, which can overflow its connection.
    /// Returns false when the layout was refused (see the server log: the file is already used as
    /// another component, or the declared pool is invalid).
    /// </summary>
    bool Preload(UiLayout layout);

    /// <summary>Removes everything the kit shows to this player, every component, every layout.</summary>
    void ClearAll(CCSPlayerController player);

    /// <summary>Removes everything the kit shows to everyone.</summary>
    void ClearEveryone();
}
