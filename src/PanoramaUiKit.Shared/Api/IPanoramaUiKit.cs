using CounterStrikeSharp.API.Core;
using PanoramaUiKit.Shared.Abilities;
using PanoramaUiKit.Shared.Announcements;
using PanoramaUiKit.Shared.Banners;
using PanoramaUiKit.Shared.Cards;
using PanoramaUiKit.Shared.Common;
using PanoramaUiKit.Shared.Comms;
using PanoramaUiKit.Shared.Compass;
using PanoramaUiKit.Shared.Dials;
using PanoramaUiKit.Shared.Inventory;
using PanoramaUiKit.Shared.KillFeed;
using PanoramaUiKit.Shared.Match;
using PanoramaUiKit.Shared.Menus;
using PanoramaUiKit.Shared.Minimap;
using PanoramaUiKit.Shared.Overlays;
using PanoramaUiKit.Shared.Panels;
using PanoramaUiKit.Shared.Progress;
using PanoramaUiKit.Shared.Recap;
using PanoramaUiKit.Shared.Roulette;
using PanoramaUiKit.Shared.Screamers;
using PanoramaUiKit.Shared.Targets;
using PanoramaUiKit.Shared.Toasts;
using PanoramaUiKit.Shared.Victory;
using PanoramaUiKit.Shared.Votes;
using PanoramaUiKit.Shared.Zone;

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

    /// <summary>Info cards pinned to a screen corner (over the radar, above the HUD). Title, description, lines. Per player.</summary>
    ICardService Cards { get; }

    /// <summary>Text next to the crosshair: above, below, left or right. Per player.</summary>
    ITargetService Targets { get; }

    /// <summary>Progress bars: pushed by value, or timed and run by the client. Per player.</summary>
    IProgressService ProgressBars { get; }

    /// <summary>Radial dials (cooldown, charge, channel): pushed by value, or timed and run by the client. Per player.</summary>
    IDialService Dials { get; }

    /// <summary>Full-screen picture with its sound, the picture from one stylesheet class, the sound played by the kit. Per player.</summary>
    IScreamerService Screamers { get; }

    /// <summary>Compass strip at the top of the screen that follows the player's view, with pins on world points. Per player.</summary>
    ICompassService Compass { get; }

    /// <summary>Row of item tiles (icon, count, key hint, one selected), the icons from stylesheet classes. Per player.</summary>
    IInventoryService Inventory { get; }

    /// <summary>Safe zone: status under the compass (phase, countdown, bar) and an alert when it moves. Per player.</summary>
    IZoneService Zone { get; }

    /// <summary>Radio transmission: a face as a glitching hologram, a name and a line of text. Per player.</summary>
    ICommsService Comms { get; }

    /// <summary>End of match screen: placement, headline, the player's name and stat tiles with icons. Per player.</summary>
    IRecapService Recap { get; }

    /// <summary>Map in a corner: the map picture, the zone circles and markers, the player's arrow followed by the kit. Zones and global marks for everyone, the rest per player.</summary>
    IMinimapService Minimap { get; }

    /// <summary>A full-screen effect over the view: frozen, burning, knocked down, poisoned, or your own (a logo) as a custom effect. Per player.</summary>
    IOverlayService Overlays { get; }

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
