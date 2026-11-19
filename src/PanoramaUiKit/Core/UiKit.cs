using CounterStrikeSharp.API.Core;
using PanoramaUiKit.Channels.Base;
using PanoramaUiKit.Services;
using PanoramaUiKit.Shared.Abilities;
using PanoramaUiKit.Shared.Announcements;
using PanoramaUiKit.Shared.Api;
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

namespace PanoramaUiKit.Core;

/// <summary>The object behind the capability. Owns the registry; services only route to it.</summary>
internal sealed class UiKit : IPanoramaUiKit, IDisposable
{
    private readonly LayoutRegistry _registry;
    private readonly OverlayService _overlays;

    public UiKit(LayoutRegistry registry)
    {
        _registry = registry;

        Toasts = new ToastService(registry);
        Banners = new BannerService(registry);
        Announcements = new AnnounceService(registry);
        Abilities = new AbilityService(registry);
        Match = new MatchService(registry);
        KillFeed = new KillFeedService(registry);
        Victory = new VictoryService(registry);
        Roulettes = new RouletteService(registry);
        Votes = new VoteService(registry);
        Panels = new PanelService(registry);
        Menus = new MenuService(registry);
        Cards = new CardService(registry);
        Targets = new TargetService(registry);
        ProgressBars = new ProgressService(registry);
        Dials = new DialService(registry);
        Screamers = new ScreamerService(registry);
        Compass = new CompassService(registry);
        Inventory = new InventoryService(registry);
        Zone = new ZoneService(registry);
        Comms = new CommsService(registry);
        Recap = new RecapService(registry);
        Minimap = new MinimapService(registry);
        _overlays = new OverlayService(registry);
    }

    public int Version => IPanoramaUiKit.ApiVersion;

    public IToastService Toasts { get; }

    public IBannerService Banners { get; }

    public IAnnounceService Announcements { get; }

    public IAbilityService Abilities { get; }

    public IMatchService Match { get; }

    public IKillFeedService KillFeed { get; }

    public IVictoryService Victory { get; }

    public IRouletteService Roulettes { get; }

    public IVoteService Votes { get; }

    public IPanelService Panels { get; }

    public IMenuService Menus { get; }

    public ICardService Cards { get; }

    public ITargetService Targets { get; }

    public IProgressService ProgressBars { get; }

    public IDialService Dials { get; }

    public IScreamerService Screamers { get; }

    public ICompassService Compass { get; }

    public IInventoryService Inventory { get; }

    public IZoneService Zone { get; }

    public ICommsService Comms { get; }

    public IRecapService Recap { get; }

    public IMinimapService Minimap { get; }

    public IOverlayService Overlays => _overlays;

    public IEnumerable<LayoutChannel> Channels => _registry.Channels;

    public bool Preload(UiLayout layout) => _registry.Get<LayoutChannel>(layout) != null;

    public void ClearAll(CCSPlayerController player) => _registry.ClearAll(player);

    public void ClearEveryone() => _registry.ClearEveryone();

    /// <summary>A player died: what was shown to them only while alive goes.</summary>
    public void PlayerDied(CCSPlayerController player) => _overlays.PlayerDied(player);

    public void Dispose() => _registry.Dispose();
}
