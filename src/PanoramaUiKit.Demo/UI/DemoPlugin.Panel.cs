using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;
using PanoramaUiKit.Shared.Api;
using PanoramaUiKit.Shared.Common;
using PanoramaUiKit.Shared.Panels;

namespace PanoramaUiKit.Demo;

public sealed partial class DemoPlugin
{
    /// <summary>Per-player state the panel's controls change. The panel only ever reads it.</summary>
    private sealed class PanelDemoState
    {
        public bool Notifications = true;
        public bool Compact;
        public int Difficulty = 1;
        public int Volume = 50;
        public int? OpenSection;
        public int Clicks;
    }

    private static readonly string[] Difficulties = ["Easy", "Normal", "Hard", "Expert"];

    private readonly Dictionary<int, PanelDemoState> _panelStates = new();

    /// <summary>
    /// A panel with five pages in the left menu, one per kind of content: read-only info and stats,
    /// switches and steppers, a live list, buttons calling the rest of the kit, an accordion.
    /// Built from a function: rebuilt after every click and every second.
    /// </summary>
    [ConsoleCommand("css_uikit_demo_panel", "Panel with pages: info, settings, live list, actions, accordion.")]
    [CommandHelper(whoCanExecute: CommandUsage.CLIENT_ONLY)]
    public void OnDemoPanel(CCSPlayerController? player, CommandInfo command)
    {
        if (!TryKitFor(player, command, out IPanoramaUiKit kit, out CCSPlayerController target))
        {
            return;
        }

        _panelStates.TryAdd(target.Slot, new PanelDemoState());
        kit.Panels.Open(target, viewer => BuildDemoPanel(kit, viewer));
    }

    private PanelOptions BuildDemoPanel(IPanoramaUiKit kit, CCSPlayerController viewer)
    {
        if (!_panelStates.TryGetValue(viewer.Slot, out PanelDemoState? state))
        {
            state = new PanelDemoState();
        }


        return new PanelOptions
        {
            Layout = DemoLayouts.Panel,
            Title = "Panel title",
            Subtitle = "Subtitle: what this panel is for.",
            Hint = "Hint line - click a page on the left, or ✕ to close.",
            Style = UiStyle.Info,
            Pages =
            [
                OverviewPage(),
                SettingsPage(state),
                PlayersPage(),
                ActionsPage(kit, state),
                AccordionPage(state),
            ],
            OnClose = closed => _panelStates.Remove(closed.Slot),
        };
    }

    private static PanelPage OverviewPage()
    {
        List<CCSPlayerController> players = Utilities.GetPlayers().Where(p => p is { IsValid: true, IsBot: false }).ToList();

        return new PanelPage("Overview",
        [
            PanelRow.Paragraph("A paragraph is free text: no row ground, no divider. Use it for an intro, "
                               + "rules or any explanation between groups of rows. It wraps over as many lines as it needs."),
            PanelRow.Header("Section header"),
            PanelRow.Text("Text row", "Body text inside the list look, with an optional title."),
            PanelRow.Info("Info row", "Title, description and a value on the right.", "Value"),
            PanelRow.Info("Coloured value", "The row style colours the value.", "Success", UiStyle.Success),
            PanelRow.Info("Live value", "Rebuilt every second while the panel is open.", DateTime.Now.ToString("HH:mm:ss"), UiStyle.Info),
            PanelRow.Paragraph("A coloured paragraph: the style colours the text.", UiStyle.Warn),
        ])
        {
            Stats =
            [
                new PanelStat("Players", players.Count.ToString()),
                new PanelStat("Map", Server.MapName, UiStyle.Info),
                new PanelStat("Max players", Server.MaxPlayers.ToString()),
                new PanelStat(string.Empty, DateTime.Now.ToString("HH:mm"), UiStyle.Gold),
            ],
        };
    }

    private static PanelPage SettingsPage(PanelDemoState state)
    {
        // No Stats on this page: the strip is hidden.
        return new PanelPage("Settings",
        [
            PanelRow.Paragraph("Every control changes your state; the panel repaints right after the click."),
            PanelRow.Header("Toggles"),
            PanelRow.Toggle("Notifications", "Switch row: the click flips your state, the panel repaints.", state.Notifications,
                             _ => state.Notifications = !state.Notifications),
            PanelRow.Toggle("Compact mode", "Another switch.", state.Compact, _ => state.Compact = !state.Compact),
            PanelRow.Header("Steppers"),
            PanelRow.Stepper("Difficulty", "Step through a list of choices.", Difficulties[state.Difficulty],
                             _ => state.Difficulty = (state.Difficulty + Difficulties.Length - 1) % Difficulties.Length,
                             _ => state.Difficulty = (state.Difficulty + 1) % Difficulties.Length),
            PanelRow.Stepper("Volume", "Step a number, clamped to 0-100.", $"{state.Volume}%",
                             _ => state.Volume = Math.Max(0, state.Volume - 10),
                             _ => state.Volume = Math.Min(100, state.Volume + 10)),
            PanelRow.Toggle("Disabled row", "Greyed out; its clicks are dropped.", false, _ => { }) with { Disabled = true },
        ]);
    }

    private static PanelPage PlayersPage()
    {
        List<PanelRow> rows = [PanelRow.Header("Connected players")];
        foreach (CCSPlayerController player in Utilities.GetPlayers().Where(p => p.IsValid && !p.IsHLTV))
        {
            (string team, string style) = player.Team switch
            {
                CsTeam.CounterTerrorist => ("CT", UiStyle.Info),
                CsTeam.Terrorist => ("T", UiStyle.Warn),
                _ => ("Spectator", UiStyle.Neutral),
            };

            string status = "Dead";
            if (player.PawnIsAlive)
            {
                status = "Alive";
            }

            rows.Add(PanelRow.Info(player.PlayerName, $"{status} · slot {player.Slot}", team, style));
        }

        if (rows.Count == 1)
        {
            rows.Add(PanelRow.Text(string.Empty, "Nobody connected."));
        }

        return new PanelPage("Players", rows)
        {
            Stats =
            [
                new PanelStat("CT", Utilities.GetPlayers().Count(p => p.Team == CsTeam.CounterTerrorist).ToString(), UiStyle.Info),
                new PanelStat("T", Utilities.GetPlayers().Count(p => p.Team == CsTeam.Terrorist).ToString(), UiStyle.Warn),
            ],
        };
    }

    private static PanelPage ActionsPage(IPanoramaUiKit kit, PanelDemoState state)
    {
        return new PanelPage("Actions",
        [
            PanelRow.Header("Buttons"),
            PanelRow.Action("Counter", $"Clicked {state.Clicks} time(s).", "Click", _ => state.Clicks++),
            PanelRow.Action("Send a toast", "Buttons can call anything, here the toast service.", "Toast",
                            p => kit.Toast(p, DemoLayouts.Toast, "From the panel", "Sent by a panel button.", UiStyle.Success), UiStyle.Success),
            PanelRow.Action("Show a banner", "Banner for everyone, 3 seconds.", "Banner",
                            _ => kit.Banner(DemoLayouts.Banner, "Banner from the panel", UiStyle.Warn, seconds: 3f), UiStyle.Gold),
            PanelRow.Action("Close", "A button can close the panel.", "Close",
                            p => kit.Panels.Close(p, DemoLayouts.Panel), UiStyle.Danger),
        ]);
    }

    /// <summary>Accordion: opening a section folds the one that was open; clicking the open one folds it.</summary>
    private static void ToggleSection(PanelDemoState state, int index)
    {
        if (state.OpenSection == index)
        {
            state.OpenSection = null;
            return;
        }

        state.OpenSection = index;
    }

    private static PanelPage AccordionPage(PanelDemoState state)
    {
        (string Title, string Body, string Style)[] sections =
        [
            ("First section", "Body of the first section, shown only while it is open.", UiStyle.Info),
            ("Second section", "Body of the second section.", UiStyle.Success),
            ("Third section", "Body of the third section.", UiStyle.Danger),
        ];

        List<PanelRow> rows = [PanelRow.Header("Accordion: one section open at a time")];
        for (int i = 0; i < sections.Length; i++)
        {
            int index = i;
            bool open = state.OpenSection == i;
            (string title, string body, string style) = sections[i];

            string button = "Show";
            if (open)
            {
                button = "Hide";
            }

            rows.Add(PanelRow.Action(title, "Selected + nested rows.", button, _ => ToggleSection(state, index), style)
                     with { Selected = open });
            if (!open)
            {
                continue;
            }

            rows.Add(PanelRow.Text(string.Empty, body, style) with { Nested = true });
            rows.Add(PanelRow.Info("Detail", "A nested info row.", "F", style) with { Nested = true });
        }

        return new PanelPage("Accordion", rows);
    }
}
