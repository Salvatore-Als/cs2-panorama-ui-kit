using PanoramaUiKit.Shared.Abilities;
using PanoramaUiKit.Shared.Announcements;
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

namespace PanoramaUiKit.Demo;

/// <summary>
/// The layout file each demo draws on, named explicitly: there is no default. <c>Name</c> is the
/// file name without folder or extension - <c>uikit_toast</c> is
/// <c>panorama/layout/custom_game/uikit_toast.xml</c>. To draw on your own file, change the name
/// (and the pool sizes if your layout declares other ones, see docs/CONTRACTS.md).
/// </summary>
internal static class DemoLayouts
{
    public static readonly ToastLayout Toast = new() { Name = "uikit_toast" };

    /// <summary>
    /// A toast layout of our own, to try the customisation: the same contract as <see cref="Toast"/> with
    /// another file, another look and only two placements. Its pool sizes are the ones it was built with.
    /// </summary>
    public static readonly ToastLayout NeonToast = new()
    {
        Name = "neon_toast",
        Placements = [ToastPlacement.TopCenter, ToastPlacement.BottomLeft],
        Depth = 3,
    };

    public static readonly BannerLayout Banner = new() { Name = "uikit_banner" };

    public static readonly AnnounceLayout Announce = new() { Name = "uikit_announce" };

    public static readonly AbilityLayout Abilities = new() { Name = "uikit_abilities" };

    public static readonly MatchLayout Match = new() { Name = "uikit_match" };

    public static readonly KillFeedLayout KillFeed = new() { Name = "uikit_killfeed" };

    public static readonly VictoryLayout Victory = new() { Name = "uikit_victory" };

    public static readonly RouletteLayout Roulette = new() { Name = "uikit_roulette" };

    public static readonly VoteLayout Vote = new() { Name = "uikit_vote" };

    public static readonly PanelLayout Panel = new() { Name = "uikit_panel" };

    public static readonly MenuLayout Menu = new() { Name = "uikit_menu" };

    /// <summary>Every layout the demo draws on, to spawn them at load.</summary>
    public static IEnumerable<UiLayout> All =>
    [
        Toast, NeonToast, Banner, Announce, Abilities, Match, KillFeed, Victory, Roulette, Vote, Panel, Menu,
    ];
}
