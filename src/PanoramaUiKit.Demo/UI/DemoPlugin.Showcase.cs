using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using PanoramaUiKit.Shared.Api;
using PanoramaUiKit.Shared.Common;
using PanoramaUiKit.Shared.Menus;

namespace PanoramaUiKit.Demo;

public sealed partial class DemoPlugin
{
    /// <summary>
    /// One menu to show every component without typing commands: a sub-menu per group, one entry per demo
    /// and per variant. An entry runs the demo's own command as the caller (css_uikit_demo_&lt;name&gt; args),
    /// so the menu and the console always show the same thing.
    ///
    /// The menu stays open, so you can stack components. Demos that take the mouse or the menu layout
    /// (menu, panel, vote) close it first.
    /// </summary>
    [ConsoleCommand("css_uikit_demo", "Opens the showcase menu: every component, one click each.")]
    [CommandHelper(whoCanExecute: CommandUsage.CLIENT_ONLY)]
    public void OnDemoShowcase(CCSPlayerController? player, CommandInfo command)
    {
        if (TryKitFor(player, command, out IPanoramaUiKit kit, out CCSPlayerController target))
        {
            kit.Menus.Open(target, ShowcaseRoot());
        }
    }

    private static MenuOptions ShowcaseRoot() => ShowcaseMenu("Panorama UI Kit", "Pick a component to see it", UiStyle.Info,
    [
        MenuEntry.Open("HUD", _ => ShowcaseHud(), "Match bar, compass, zone, minimap, inventory, bars and dials."),
        MenuEntry.Open("Messages", _ => ShowcaseMessages(), "Toasts, banner, announce, cards, comms, kill feed."),
        MenuEntry.Open("Screens", _ => ShowcaseScreens(), "Victory, recap, roulette, screamer, vote, panel, menu."),
        MenuEntry.Open("Full HUD preview", _ => ShowcaseFullHudMenu(),
                       "Everything at once; pick where the zone status goes."),
        MenuEntry.Open("Minimap", _ => ShowcaseMinimap(), "Maps, zones, zoom, tracked players."),
        Demo("Inventory", "css_uikit_demo_inventory", "Item bar at the bottom; each click changes the selection."),
        Demo("Diagnostics", "css_panorama_diag", "PanoramaManager's report: open your console to read it."),
        MenuEntry.Button("Clear everything", p =>
        {
            RunDemo(p, "css_uikit_demo_clear");
            RunDemo(p, "css_uikit_demo_minimap off");
        }, "Removes every kit element on your screen."),
    ]);

    private static MenuOptions ShowcaseHud() => ShowcaseMenu("HUD", "Persistent widgets", UiStyle.Success,
    [
        MenuEntry.Open("Match bar", _ => ShowcaseMenu("Match bar", "40 s countdown", UiStyle.Success,
        [
            Demo("With scores", "css_uikit_demo_match"),
            Demo("Without scores", "css_uikit_demo_match noscore"),
            Demo("Band, no scores", "css_uikit_demo_match noscore band", "Long strip across the top, bar hanging under it."),
            Demo("Band with scores", "css_uikit_demo_match band"),
        ])),
        MenuEntry.Open("Compass", _ => ShowcaseMenu("Compass", "Click again to hide it", UiStyle.Success,
        [
            Demo("Under the HUD, 3 pins", "css_uikit_demo_compass 3"),
            Demo("Under the HUD, 4 pins", "css_uikit_demo_compass 4"),
            Demo("In the band", "css_uikit_demo_compass 3 band", "Pair it with the match bar's band."),
        ])),
        MenuEntry.Open("Zone", _ => ShowcaseMenu("Zone", "Status: waiting, closing, final", UiStyle.Success,
        [
            Demo("Under the HUD", "css_uikit_demo_zone 20"),
            Demo("Under the band", "css_uikit_demo_zone 20 band", "Under the match bar hanging from the band."),
            Demo("Top left", "css_uikit_demo_zone 20 tl"),
            Demo("Top right", "css_uikit_demo_zone 20 tr"),
            Demo("Bottom left", "css_uikit_demo_zone 20 bl"),
            Demo("Bottom right", "css_uikit_demo_zone 20 br"),
            Demo("Right edge", "css_uikit_demo_zone 20 right"),
            Demo("Addon icon slot", "css_uikit_demo_zone 20 slot", "icon-slot-1: empty unless another addon ships it."),
            Demo("Alert alone", "css_uikit_demo_zone alert", "The big card between hazard stripes."),
            Demo("Toggle outside", "css_uikit_demo_zone out"),
            Demo("Hide", "css_uikit_demo_zone off"),
        ])),
        MenuEntry.Open("Minimap", _ => ShowcaseMinimap()),
        MenuEntry.Open("Inventory", _ => ShowcaseMenu("Inventory", "Each show changes the selection", UiStyle.Success,
        [
            Demo("Show / next", "css_uikit_demo_inventory"),
            Demo("Select slot 1", "css_uikit_demo_inventory select 1"),
            Demo("Select slot 3", "css_uikit_demo_inventory select 3"),
            Demo("Hide", "css_uikit_demo_inventory off"),
        ])),
        Demo("Abilities", "css_uikit_demo_abilities", "Cooldown and hold, 12 s."),
        Demo("Progress bars", "css_uikit_demo_progress"),
        Demo("Dials", "css_uikit_demo_dial"),
        MenuEntry.Open("Target text", _ => ShowcaseMenu("Target text", "Around the crosshair", UiStyle.Success,
        [
            Demo("All four", "css_uikit_demo_target"),
            Demo("Above", "css_uikit_demo_target above"),
            Demo("Below", "css_uikit_demo_target below"),
            Demo("Left", "css_uikit_demo_target left"),
            Demo("Right", "css_uikit_demo_target right"),
        ])),
    ]);

    private static MenuOptions ShowcaseMinimap() => ShowcaseMenu("Minimap", "Zones, mate, points of interest", UiStyle.Success,
    [
        Demo("Big map (toggle)", "css_uikit_demo_minimap big", "Centre of the screen: where you are, the zones, everyone."),
        MenuEntry.Open("Maps", _ => ShowcaseMenu("Maps", "Preview a map's picture", UiStyle.Success,
        [
            Demo("Current map", "css_uikit_demo_minimap tr", "Your arrow, everything around you."),
            Demo("Dust II", "css_uikit_demo_minimap tr map de_dust2", "Shipped with the addon."),
            Demo("Mirage", "css_uikit_demo_minimap tr map de_mirage", "Game radar: path not yet verified."),
            Demo("Inferno", "css_uikit_demo_minimap tr map de_inferno"),
            Demo("Nuke", "css_uikit_demo_minimap tr map de_nuke"),
            Demo("Ancient", "css_uikit_demo_minimap tr map de_ancient"),
            Demo("Anubis", "css_uikit_demo_minimap tr map de_anubis"),
            Demo("Overpass", "css_uikit_demo_minimap tr map de_overpass"),
            Demo("Vertigo", "css_uikit_demo_minimap tr map de_vertigo"),
            Demo("Train", "css_uikit_demo_minimap tr map de_train"),
            Demo("Office", "css_uikit_demo_minimap tr map cs_office"),
            Demo("Italy", "css_uikit_demo_minimap tr map cs_italy"),
            Demo("Addon slot 1", "css_uikit_demo_minimap tr map slot-1", "Empty unless another addon ships it."),
        ])),
        Demo("Top right", "css_uikit_demo_minimap tr"),
        Demo("Top left", "css_uikit_demo_minimap tl"),
        Demo("Bottom left", "css_uikit_demo_minimap bl"),
        Demo("Bottom right", "css_uikit_demo_minimap br"),
        Demo("Large, zoom x2", "css_uikit_demo_minimap tr lg zoom 2", "Centred on you, follows you."),
        Demo("Zoom x4", "css_uikit_demo_minimap tr zoom 4"),
        Demo("Shrink the zone", "css_uikit_demo_minimap shrink", "Red zone closes onto the white one over 20 s."),
        Demo("Track other players", "css_uikit_demo_minimap track", "Bots too: kill or kick one and watch its marker."),
        Demo("Hide", "css_uikit_demo_minimap off"),
    ]);

    private static MenuOptions ShowcaseMessages() => ShowcaseMenu("Messages", "Timed notices", UiStyle.Warn,
    [
        MenuEntry.Open("Toast", _ => ShowcaseMenu("Toast", "Stacked cards", UiStyle.Warn,
        [
            Demo("Top centre", "css_uikit_demo_toast"),
            Demo("Bottom, every field", "css_uikit_demo_toast bottom"),
            Demo("Several, every style", "css_uikit_demo_toast multi"),
            Demo("Custom layout (neon)", "css_uikit_demo_toast neon"),
        ])),
        Demo("Banner", "css_uikit_demo_banner", "Countdown rewritten in place."),
        Demo("Announce", "css_uikit_demo_announce"),
        Demo("Cards", "css_uikit_demo_card", "One per placement."),
        Demo("Comms", "css_uikit_demo_comms", "Radio transmission, random face."),
        Demo("Comms, villain photo", "css_uikit_demo_comms villain", "Colour photo, untinted."),
        Demo("Kill feed", "css_uikit_demo_killfeed"),
    ]);

    private static MenuOptions ShowcaseScreens() => ShowcaseMenu("Screens", "Full screen and interactive", UiStyle.Danger,
    [
        Demo("Victory", "css_uikit_demo_victory"),
        Demo("Recap, win", "css_uikit_demo_recap win"),
        Demo("Recap, lose", "css_uikit_demo_recap lose"),
        Demo("Recap, duo", "css_uikit_demo_recap duo", "Two player rows instead of one."),
        Demo("Roulette, fast", "css_uikit_demo_roulette fast"),
        Demo("Roulette, slow", "css_uikit_demo_roulette slow"),
        Demo("Screamer", "css_uikit_demo_screamer"),
        Demo("Vote", "css_uikit_demo_vote", "Closes this menu: the vote takes the mouse.", closeMenu: true),
        Demo("Panel", "css_uikit_demo_panel", "Closes this menu: the panel takes the mouse.", closeMenu: true),
        Demo("Menu demo", "css_uikit_demo_menu", "Replaces this menu.", closeMenu: true),
    ]);

    /// <summary>The whole HUD, the zone status where asked; the minimap takes the top corner the status leaves free.</summary>
    private static MenuOptions ShowcaseFullHudMenu() => ShowcaseMenu("Full HUD preview", "Where does the zone status go?", UiStyle.Info,
    [
        MenuEntry.Button("Under the band", p => ShowcaseFullHud(p, "band", "tr"), "Under the match bar, centred."),
        MenuEntry.Button("Top left", p => ShowcaseFullHud(p, "tl", "tr"), "Minimap top right."),
        MenuEntry.Button("Top right", p => ShowcaseFullHud(p, "tr", "tl"), "Minimap top left."),
        MenuEntry.Button("Bottom left", p => ShowcaseFullHud(p, "bl", "tr")),
        MenuEntry.Button("Bottom right", p => ShowcaseFullHud(p, "br", "tr")),
        Demo("Big map (toggle)", "css_uikit_demo_minimap big", "Over everything, in the centre."),
        MenuEntry.Button("Clear everything", p =>
        {
            RunDemo(p, "css_uikit_demo_clear");
            RunDemo(p, "css_uikit_demo_minimap off");
        }),
    ]);

    private static void ShowcaseFullHud(CCSPlayerController player, string zonePlacement, string minimapCorner)
    {
        RunDemo(player, "css_uikit_demo_match noscore band");
        RunDemo(player, $"css_uikit_demo_minimap {minimapCorner}");
        RunDemo(player, "css_uikit_demo_inventory");
        RunDemo(player, $"css_uikit_demo_zone 20 {zonePlacement}");
        RunDemo(player, "css_uikit_demo_banner");

        // The compass demo toggles: only ask for it when it is not already up.
        if (Kit is { } kit && !kit.Compass.IsShowing(player, DemoLayouts.Compass))
        {
            RunDemo(player, "css_uikit_demo_compass 3 band");
        }
    }

    private static MenuOptions ShowcaseMenu(string title, string subtitle, string style, IReadOnlyList<MenuEntry> entries) => new()
    {
        Layout = DemoLayouts.Menu,
        Title = title,
        Subtitle = subtitle,
        Style = style,
        Entries = entries,
    };

    /// <summary>An entry that runs <paramref name="commandLine"/> as the player, the menu closed first if asked.</summary>
    private static MenuEntry Demo(string label, string commandLine, string? description = null, bool closeMenu = false) =>
        MenuEntry.Button(label, player =>
        {
            if (closeMenu)
            {
                Kit?.Menus.Close(player, DemoLayouts.Menu);
            }

            RunDemo(player, commandLine);
        }, description ?? commandLine);

    /// <summary>Next frame: out of the menu's click handler, so a demo can replace or close the menu.</summary>
    private static void RunDemo(CCSPlayerController player, string commandLine)
    {
        Server.NextFrame(() =>
        {
            if (player.IsValid)
            {
                player.ExecuteClientCommandFromServer(commandLine);
            }
        });
    }
}
