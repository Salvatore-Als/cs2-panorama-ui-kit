using CounterStrikeSharp.API.Core;
using PanoramaUiKit.Channels.Base;
using PanoramaUiKit.Internal;
using PanoramaUiKit.Shared.Common;
using PanoramaUiKit.Shared.Menus;

namespace PanoramaUiKit.Channels;

/// <summary>
/// Menus on one layout (contract "menu"). Per viewer, a stack of levels: the root menu and the
/// sub-menus opened from it, each remembering its page, the state of its toggles and selects and
/// which select's dropdown is open, so going back lands where the player left, as they left it.
/// </summary>
internal sealed class MenuChannel : InteractiveChannel<MenuChannel.Viewer>
{
    private const string MenuId = "mn_menu";
    private const string BackId = "mn_back";
    private const string PrevId = "mn_prev";
    private const string NextId = "mn_next";
    private const string ItemPrefix = "mn_item_";
    private const string EntryPrefix = "mn_entry_";
    private const string ChoicePrefix = "mn_choice_";
    private const string PathSeparator = "  ›  ";

    private static readonly MenuEntryKind[] Kinds = Enum.GetValues<MenuEntryKind>();

    /// <summary>One open menu: its page and the toggle / select state of its entries, by entry index.</summary>
    internal sealed class Level
    {
        public required MenuOptions Menu { get; init; }

        public int Page { get; set; }

        public Dictionary<int, bool> Toggles { get; } = [];

        public Dictionary<int, int> Choices { get; } = [];

        /// <summary>Entry index whose dropdown is open, if any: one at a time.</summary>
        public int? OpenSelect { get; set; }

        public bool IsOn(int index)
        {
            if (Toggles.TryGetValue(index, out bool on))
            {
                return on;
            }

            return Menu.Entries[index].On;
        }

        public int Choice(int index)
        {
            MenuEntry entry = Menu.Entries[index];
            if (entry.Choices.Count == 0)
            {
                return 0;
            }

            if (!Choices.TryGetValue(index, out int choice))
            {
                choice = entry.Selected;
            }

            return Math.Clamp(choice, 0, entry.Choices.Count - 1);
        }
    }

    internal sealed class Viewer : InteractiveViewer
    {
        public Viewer(ClientState client) : base(client)
        {
        }

        public List<Level> Stack { get; } = [];

        public Level Current => Stack[^1];
    }

    private readonly MenuLayout _layout;

    public MenuChannel(BasePlugin plugin, MenuLayout layout) : base(plugin, layout)
    {
        _layout = layout;
    }

    protected override string RevealId => MenuId;

    protected override string CloseButtonId => "mn_close";

    protected override Viewer CreateViewer(int slot) => new(NewInteractiveClient(slot));

    /// <summary>Entries and the choices of their dropdowns are authored hidden.</summary>
    protected override bool AuthoredOn(string panelId, string className)
    {
        if (className != "hidden")
        {
            return false;
        }

        return TryIndex(panelId, EntryPrefix, string.Empty, out _)
            || panelId.StartsWith(ChoicePrefix, StringComparison.Ordinal);
    }

    /// <summary>Opens <paramref name="menu"/> as the root, replacing whatever menu the player had here.</summary>
    public IUiHandle Open(CCSPlayerController player, MenuOptions menu)
    {
        if (player is not { IsValid: true, IsBot: false })
        {
            return UiHandle.Dead;
        }

        return Present(player, viewer =>
        {
            viewer.Stack.Clear();
            viewer.Stack.Add(new Level { Menu = menu });
        }, menu.OnClose);
    }

    public void Push(CCSPlayerController player, MenuOptions menu)
    {
        if (!Viewers.TryGetValue(player.Slot, out Viewer? viewer))
        {
            Open(player, menu);
            return;
        }

        viewer.Stack.Add(new Level { Menu = menu });
        Paint(player, viewer);
    }

    public void Back(CCSPlayerController player)
    {
        if (!Viewers.TryGetValue(player.Slot, out Viewer? viewer))
        {
            return;
        }

        if (viewer.Stack.Count <= 1)
        {
            Close(player);
            return;
        }

        viewer.Stack.RemoveAt(viewer.Stack.Count - 1);
        Paint(player, viewer);
    }

    // ------------------------------------------------------------------ paint

    protected override void Paint(CCSPlayerController player, Viewer viewer)
    {
        ClientState client = viewer.Client;
        Level level = viewer.Current;
        MenuOptions menu = level.Menu;

        int size = _layout.Items;
        int pages = Math.Max(1, (menu.Entries.Count + size - 1) / size);
        level.Page = Math.Clamp(level.Page, 0, pages - 1);
        bool nested = viewer.Stack.Count > 1;

        client.Text(player, "mn_title", menu.Title);
        client.OptionalText(player, "mn_subtitle", menu.Subtitle);
        string? path = null;
        if (nested)
        {
            path = string.Join(PathSeparator, viewer.Stack.Select(l => l.Menu.Title));
        }

        client.OptionalText(player, "mn_path", path);
        client.Text(player, "mn_page", $"{level.Page + 1} / {pages}");
        client.ExtraTexts(player, "mn", menu.Texts);

        client.Class(player, MenuId, "no-back", !nested);
        client.Class(player, MenuId, "no-pager", pages <= 1);
        client.Class(player, PrevId, "disabled", level.Page == 0);
        client.Class(player, NextId, "disabled", level.Page >= pages - 1);
        client.Group(player, MenuId, "style", Names.StyleOrNeutral(menu.Style));
        client.FreeClasses(player, MenuId, menu.Classes);

        for (int i = 0; i < size; i++)
        {
            string entryId = $"{EntryPrefix}{i}";
            int index = level.Page * size + i;
            bool used = index < menu.Entries.Count;
            if (used)
            {
                PaintEntry(player, client, level, index, i);
            }

            client.Class(player, entryId, "hidden", !used);
        }
    }

    private void PaintEntry(CCSPlayerController player, ClientState client, Level level, int index, int slot)
    {
        MenuEntry entry = level.Menu.Entries[index];
        string entryId = $"{EntryPrefix}{slot}";
        string itemId = $"{ItemPrefix}{slot}";
        bool select = entry.Kind == MenuEntryKind.Select && entry.Choices.Count > 0;

        string? value = entry.Value;
        if (select)
        {
            value = entry.Choices[level.Choice(index)];
        }

        // A toggle shows its switch, not a value.
        if (entry.Kind == MenuEntryKind.Toggle)
        {
            value = null;
        }

        client.Text(player, $"{itemId}_label", entry.Label);
        client.OptionalText(player, $"{itemId}_desc", entry.Description);
        client.OptionalText(player, $"{itemId}_value", value);

        foreach (MenuEntryKind kind in Kinds)
        {
            client.Class(player, entryId, $"kind-{kind.ToString().ToLowerInvariant()}", kind == entry.Kind);
        }

        client.Class(player, entryId, "on", entry.Kind == MenuEntryKind.Toggle && level.IsOn(index));
        client.Class(player, entryId, "open", select && level.OpenSelect == index);
        client.Class(player, entryId, "disabled", entry.Disabled);
        client.Group(player, entryId, "style", Names.StyleClass(entry.Style));

        PaintChoices(player, client, level, index, slot, entry, select);
    }

    /// <summary>The dropdown of a select: one button per choice (up to the layout's pool), the current one active.</summary>
    private void PaintChoices(CCSPlayerController player, ClientState client, Level level, int index, int slot,
                              MenuEntry entry, bool select)
    {
        int current = level.Choice(index);
        for (int c = 0; c < _layout.Choices; c++)
        {
            string choiceId = $"{ChoicePrefix}{slot}_{c}";
            bool used = select && c < entry.Choices.Count;
            if (used)
            {
                client.Text(player, $"{choiceId}_label", entry.Choices[c]);
            }

            client.Class(player, choiceId, "hidden", !used);
            client.Class(player, choiceId, "active", used && c == current);
        }
    }

    // ------------------------------------------------------------------ clicks

    protected override void OnButton(CCSPlayerController player, Viewer viewer, string elementId)
    {
        Level level = viewer.Current;
        switch (elementId)
        {
            case BackId:
                Back(player);
                return;
            case PrevId:
                level.Page--;
                level.OpenSelect = null;
                return;
            case NextId:
                level.Page++;
                level.OpenSelect = null;
                return;
        }

        if (elementId.StartsWith(ChoicePrefix, StringComparison.Ordinal))
        {
            OnChoice(player, level, elementId);
            return;
        }

        if (!TryIndex(elementId, ItemPrefix, string.Empty, out int slotIndex))
        {
            return;
        }

        int index = level.Page * _layout.Items + slotIndex;
        if (index >= level.Menu.Entries.Count || level.Menu.Entries[index].Disabled)
        {
            return;
        }

        MenuEntry entry = level.Menu.Entries[index];
        if (entry.Kind == MenuEntryKind.Select)
        {
            ToggleDropdown(level, index);
            return;
        }

        // Any other click closes an open dropdown.
        level.OpenSelect = null;
        switch (entry.Kind)
        {
            case MenuEntryKind.Button:
                entry.OnClick?.Invoke(player);
                break;

            case MenuEntryKind.Toggle:
                bool on = !level.IsOn(index);
                level.Toggles[index] = on;
                entry.OnToggle?.Invoke(player, on);
                break;

            case MenuEntryKind.Submenu when entry.Submenu != null:
                viewer.Stack.Add(new Level { Menu = entry.Submenu(player) });
                break;
        }
    }

    /// <summary>Opens the select's dropdown, or closes it when it is the one already open.</summary>
    private static void ToggleDropdown(Level level, int index)
    {
        if (level.OpenSelect == index)
        {
            level.OpenSelect = null;
            return;
        }

        level.OpenSelect = index;
    }

    /// <summary><c>mn_choice_&lt;slot&gt;_&lt;choice&gt;</c>: picks that choice and closes the dropdown.</summary>
    private void OnChoice(CCSPlayerController player, Level level, string elementId)
    {
        string[] parts = elementId[ChoicePrefix.Length..].Split('_');
        if (parts.Length != 2 || !int.TryParse(parts[0], out int slot) || !int.TryParse(parts[1], out int choice))
        {
            return;
        }

        int index = level.Page * _layout.Items + slot;
        if (index >= level.Menu.Entries.Count)
        {
            return;
        }

        MenuEntry entry = level.Menu.Entries[index];
        level.OpenSelect = null;
        if (entry.Kind != MenuEntryKind.Select || choice >= entry.Choices.Count || choice >= _layout.Choices)
        {
            return;
        }

        level.Choices[index] = choice;
        entry.OnChoose?.Invoke(player, choice, entry.Choices[choice]);
    }
}
