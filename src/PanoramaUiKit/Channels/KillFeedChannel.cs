using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using PanoramaManager;
using PanoramaUiKit.Channels.Base;
using PanoramaUiKit.Internal;
using PanoramaUiKit.Shared.KillFeed;

namespace PanoramaUiKit.Channels;

/// <summary>
/// Kill feed on one layout (contract "killfeed"), the same for every human.
///
/// Entries live in a fixed pool of rows, newest on top: a push shifts every entry down one row. Row
/// text is shared (<see cref="SharedText"/>); row classes are per viewer. Row 0 replays its entry
/// animation by ping-ponging flash-a / flash-b: a same-tick off-then-on of one class reaches nobody.
/// </summary>
internal sealed class KillFeedChannel : LayoutChannel<KillFeedChannel.Viewer>
{
    private sealed class Entry
    {
        public required KillFeedEntry Content;
        public SafeTimer? Expiry;
    }

    internal sealed class Viewer : ViewerState
    {
        public Viewer(ClientState client) : base(client)
        {
        }

        /// <summary>Which flash-* row 0 wears; the next entry swaps to the other.</summary>
        public bool FlashA { get; set; }
    }

    private readonly KillFeedLayout _layout;
    private readonly SharedText _text;
    private readonly List<Entry> _entries = [];

    public KillFeedChannel(BasePlugin plugin, KillFeedLayout layout) : base(plugin, layout)
    {
        _layout = layout;
        _text = new SharedText(Panel);
    }

    protected override Viewer CreateViewer(int slot) => new(NewClient());

    public void Push(KillFeedEntry content)
    {
        Entry entry = new() { Content = content };
        _entries.Insert(0, entry);
        while (_entries.Count > _layout.Rows)
        {
            _entries[^1].Expiry?.Kill();
            _entries.RemoveAt(_entries.Count - 1);
        }

        if (content.Seconds > 0f)
        {
            entry.Expiry = Later(content.Seconds, () => Expire(entry));
        }

        WriteRows();
        foreach (CCSPlayerController player in Humans())
        {
            Viewer viewer = Ensure(player, out _);
            Paint(player, viewer);
            Flash(player, viewer);
        }
    }

    /// <summary>Empties the feed and closes it for everyone.</summary>
    public void Clear()
    {
        foreach (Entry entry in _entries)
        {
            entry.Expiry?.Kill();
        }

        _entries.Clear();
        ClearEveryone();
    }

    private void Expire(Entry entry)
    {
        if (!_entries.Remove(entry))
        {
            return;
        }

        WriteRows();
        foreach ((int slot, Viewer viewer) in Viewers.ToList())
        {
            CCSPlayerController? player = Utilities.GetPlayerFromSlot(slot);
            if (player is { IsValid: true })
            {
                Paint(player, viewer);
            }
            else
            {
                Forget(slot);
            }
        }
    }

    /// <summary>Shared text of every live row; only what changed is sent.</summary>
    private void WriteRows()
    {
        for (int i = 0; i < _entries.Count; i++)
        {
            KillFeedEntry content = _entries[i].Content;
            _text.Write($"kf_{i}_victim", content.Victim.Trim());
            _text.Write($"kf_{i}_attacker", content.Attacker?.Trim() ?? string.Empty);
            _text.Write($"kf_{i}_tag", content.Tag?.Trim() ?? string.Empty);
            _text.ExtraTexts($"kf_{i}", content.Texts);
        }
    }

    /// <summary>Per-viewer row classes: visibility, style, collapsed parts.</summary>
    private void Paint(CCSPlayerController player, Viewer viewer)
    {
        ClientState client = viewer.Client;
        for (int i = 0; i < _layout.Rows; i++)
        {
            string id = $"kf_{i}";
            bool visible = i < _entries.Count;
            if (visible)
            {
                KillFeedEntry content = _entries[i].Content;
                client.Group(player, id, "style", Names.StyleOrNeutral(content.Style));
                client.FreeClasses(player, id, content.Classes);
                client.Class(player, $"{id}_attacker", "empty", string.IsNullOrWhiteSpace(content.Attacker));
                client.Class(player, $"{id}_tag", "empty", string.IsNullOrWhiteSpace(content.Tag));
            }

            client.Class(player, id, "hidden", !visible);
        }
    }

    /// <summary>Replays row 0's entry: swap to the other flash-* name, never off-then-on.</summary>
    private static void Flash(CCSPlayerController player, Viewer viewer)
    {
        viewer.FlashA = !viewer.FlashA;

        string flash = "flash-b";
        if (viewer.FlashA)
        {
            flash = "flash-a";
        }

        viewer.Client.Group(player, "kf_0", "flash", flash);
    }

    /// <summary>Reopened after a round restart with classes scrubbed: forget, the next push repaints.</summary>
    protected override void OnPanelEvent(PanelEvent e)
    {
        if (e.Action is PanelAction.Close or PanelAction.Restored)
        {
            Forget(e.Player.Slot);
        }
    }

    public override void Dispose()
    {
        foreach (Entry entry in _entries)
        {
            entry.Expiry?.Kill();
        }

        _entries.Clear();
        base.Dispose();
    }
}
