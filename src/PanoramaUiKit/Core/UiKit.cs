using CounterStrikeSharp.API.Core;
using PanoramaUiKit.Channels.Base;
using PanoramaUiKit.Services;
using PanoramaUiKit.Shared.Abilities;
using PanoramaUiKit.Shared.Announcements;
using PanoramaUiKit.Shared.Api;
using PanoramaUiKit.Shared.Banners;
using PanoramaUiKit.Shared.Common;
using PanoramaUiKit.Shared.KillFeed;
using PanoramaUiKit.Shared.Match;
using PanoramaUiKit.Shared.Menus;
using PanoramaUiKit.Shared.Panels;
using PanoramaUiKit.Shared.Roulette;
using PanoramaUiKit.Shared.Toasts;
using PanoramaUiKit.Shared.Victory;
using PanoramaUiKit.Shared.Votes;

namespace PanoramaUiKit.Core;

/// <summary>The object behind the capability. Owns the registry; services only route to it.</summary>
internal sealed class UiKit : IPanoramaUiKit, IDisposable
{
    private readonly LayoutRegistry _registry;

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

    public IEnumerable<LayoutChannel> Channels => _registry.Channels;

    public bool Preload(UiLayout layout) => _registry.Get<LayoutChannel>(layout) != null;

    public void ClearAll(CCSPlayerController player) => _registry.ClearAll(player);

    public void ClearEveryone() => _registry.ClearEveryone();

    public void Dispose() => _registry.Dispose();
}
