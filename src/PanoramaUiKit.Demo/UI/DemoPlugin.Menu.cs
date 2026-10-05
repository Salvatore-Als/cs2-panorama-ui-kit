using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using PanoramaUiKit.Shared.Api;
using PanoramaUiKit.Shared.Common;
using PanoramaUiKit.Shared.Menus;

namespace PanoramaUiKit.Demo;

public sealed partial class DemoPlugin
{
    /// <summary>Three full pages at the layout's 8 entries per page.</summary>
    private const int MenuDemoEntries = 24;

    private static readonly string[] MenuChoices = ["Easy", "Normal", "Hard"];

    /// <summary>
    /// Menu 1: entry 1 opens Menu 2, entry 2 is a toggle, entry 3 a select, the rest are buttons.
    /// Menu 2: same kinds. Every action prints in chat what was pressed; the back button returns to
    /// Menu 1 on the page it was left on, toggles and selects as they were left. Both have 3 pages.
    /// </summary>
    [ConsoleCommand("css_uikit_demo_menu", "Menu with a sub-menu, toggle, select, buttons, back and 3 pages each.")]
    [CommandHelper(whoCanExecute: CommandUsage.CLIENT_ONLY)]
    public void OnDemoMenu(CCSPlayerController? player, CommandInfo command)
    {
        if (!TryKitFor(player, command, out IPanoramaUiKit kit, out CCSPlayerController target))
        {
            return;
        }

        kit.Menus.Open(target, BuildMenu(1, "Root menu", UiStyle.Info, includeSubmenu: true));
    }

    private static string OnOff(bool on)
    {
        if (on)
        {
            return "on";
        }

        return "off";
    }

    private static MenuOptions BuildMenu(int number, string subtitle, string style, bool includeSubmenu)
    {
        string name = $"Menu {number}";
        List<MenuEntry> entries = [];

        if (includeSubmenu)
        {
            entries.Add(MenuEntry.Open("Menu 2", _ => BuildMenu(2, "Sub-menu: use ‹ to go back", UiStyle.Success, includeSubmenu: false),
                                       "Opens the sub-menu."));
        }
        else
        {
            entries.Add(MenuEntry.Button($"{name} - Item {entries.Count + 1}", p => p.PrintToChat($"{name} - Item 1")));
        }

        entries.Add(MenuEntry.Toggle($"{name} - Toggle", on: true,
                                     (p, on) => p.PrintToChat($"{name} - Toggle {OnOff(on)}"),
                                     "Flips on / off; the state is kept while the menu is open."));
        entries.Add(MenuEntry.Select($"{name} - Select", MenuChoices, selected: 1,
                                     (p, _, choice) => p.PrintToChat($"{name} - Select: {choice}"),
                                     "Click to step through the choices."));

        for (int i = entries.Count + 1; i <= MenuDemoEntries; i++)
        {
            int item = i;
            entries.Add(MenuEntry.Button($"{name} - Item {item}", p => p.PrintToChat($"{name} - Item {item}")));
        }

        return new MenuOptions
        {
            Layout = DemoLayouts.Menu,
            Title = name,
            Subtitle = subtitle,
            Style = style,
            Entries = entries,
        };
    }
}
