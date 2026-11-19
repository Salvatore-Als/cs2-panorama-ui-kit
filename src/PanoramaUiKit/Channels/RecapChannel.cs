using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using PanoramaUiKit.Channels.Base;
using PanoramaUiKit.Internal;
using PanoramaUiKit.Shared.Common;
using PanoramaUiKit.Shared.Recap;

namespace PanoramaUiKit.Channels;

/// <summary>
/// End of match screen on one layout (contract "recap"), per viewer.
///
/// Like the victory screen, the bundled layout staggers its entry through transition-delay, so the whole
/// sequence is one "show". A Show while up repaints in place, and a tile whose value changed gets the other
/// one of <c>pulse-a</c> / <c>pulse-b</c>: the same class off then on in one tick sends nothing, a swap
/// between two names always arrives and restarts the animation.
/// </summary>
internal sealed class RecapChannel : LayoutChannel<RecapChannel.Viewer>
{
    private const string ScreenId = "rc_screen";

    internal sealed class Viewer : ViewerState
    {
        public Viewer(ClientState client) : base(client)
        {
        }

        public bool Visible { get; set; }

        /// <summary>Value written on each shown tile, to pulse the ones that change.</summary>
        public Dictionary<int, string> Values { get; } = new();

        /// <summary>Pulse class each tile got last; the next pulse is the other one.</summary>
        public Dictionary<int, string> Pulses { get; } = new();
    }

    private readonly RecapLayout _layout;

    public RecapChannel(BasePlugin plugin, RecapLayout layout) : base(plugin, layout)
    {
        _layout = layout;
    }

    protected override Viewer CreateViewer(int slot) => new(NewClient());

    public IUiHandle Show(CCSPlayerController player, RecapOptions options)
    {
        if (player is not { IsValid: true, IsBot: false })
        {
            return UiHandle.Dead;
        }

        Viewer viewer = Ensure(player, out bool fresh);
        viewer.CancelTimers();
        UiHandle handle = viewer.NewHandle(player, () => Hide(player));

        // Only a screen already on display pulses: a fresh one plays its entry instead.
        Paint(player, viewer, options, pulse: viewer.Visible && !fresh);
        StartCountdown(player, viewer, options);

        // Already up: repainted in place. Fresh: "show" one update after the open. Leaving: back at once.
        if (fresh)
        {
            RevealLater(player, viewer, ScreenId);
        }
        else if (!viewer.Visible)
        {
            viewer.Client.Class(player, ScreenId, "show", true);
        }

        viewer.Visible = true;

        if (options.Seconds > 0f)
        {
            After(viewer, options.Seconds, () => Hide(player));
        }

        return handle;
    }

    /// <summary>Rewrites one tile's value and pulses it. False when the tile is not on screen.</summary>
    public bool UpdateStat(CCSPlayerController player, int index, string value)
    {
        if (player is not { IsValid: true }
            || !TryOpenViewer(player, out Viewer viewer)
            || !viewer.Visible
            || !viewer.Values.ContainsKey(index))
        {
            return false;
        }

        WriteValue(player, viewer, index, value.Trim(), pulse: true);
        return true;
    }

    /// <summary>Plays the exit for this viewer, then closes the panel.</summary>
    public void Hide(CCSPlayerController player)
    {
        if (player is { IsValid: true } && Viewers.TryGetValue(player.Slot, out Viewer? viewer))
        {
            viewer.Visible = false;
            Dismiss(player, viewer, ScreenId);
        }
    }

    public void HideEveryone()
    {
        foreach (CCSPlayerController player in Humans())
        {
            Hide(player);
        }
    }

    private void Paint(CCSPlayerController player, Viewer viewer, RecapOptions options, bool pulse)
    {
        ClientState client = viewer.Client;

        client.OptionalText(player, "rc_kicker", options.Kicker);
        client.Text(player, "rc_title", options.Title.Trim());
        client.Text(player, "rc_player", (options.Player ?? player.PlayerName).Trim());
        client.OptionalText(player, "rc_player_sub", options.PlayerSub);
        client.Text(player, "rc_restart_label", options.RestartLabel);
        client.ExtraTexts(player, "rc", options.Texts);

        client.Class(player, "rc_place_row", "hidden", options.Place == null);
        if (options.Place != null)
        {
            client.Text(player, "rc_place", Names.Format(options.PlaceFormat, options.Place.Value));
        }

        string? total = null;
        if (options.Total != null)
        {
            total = Names.Format(options.TotalFormat, options.Total.Value);
        }

        client.OptionalText(player, "rc_place_total", total);

        client.Group(player, ScreenId, "style", Names.StyleOrNeutral(options.Style));
        client.FreeClasses(player, ScreenId, options.Classes);

        bool rows = options.Players.Count > 0;
        client.Class(player, "rc_solo", "hidden", rows);
        client.Class(player, "rc_rows", "hidden", !rows);
        PaintRows(player, client, rows ? options.Players : []);

        int count = rows ? 0 : Math.Min(options.Stats.Count, _layout.Stats);
        client.Class(player, "rc_stats", "hidden", count == 0);
        for (int i = 0; i < _layout.Stats; i++)
        {
            string id = $"rc_stat_{i}";
            bool visible = i < count;
            if (visible)
            {
                RecapStat stat = options.Stats[i];
                WriteValue(player, viewer, i, stat.Value.Trim(), pulse);
                client.Text(player, $"{id}_label", stat.Label.Trim());
                client.Group(player, $"{id}_icon", "icon", RecapIcons.Class(Names.Token(stat.Icon) ?? "none"));
                client.Group(player, id, "style", Names.StyleOrNeutral(stat.Style));
                client.Class(player, id, "best", stat.Best);
            }
            else
            {
                viewer.Values.Remove(i);
            }

            client.Class(player, id, "hidden", !visible);
        }
    }

    /// <summary>A duo or a squad: one row per player, its stats on the same line. Unused rows and stats hidden.</summary>
    private void PaintRows(CCSPlayerController player, ClientState client, IReadOnlyList<RecapPlayer> players)
    {
        for (int r = 0; r < _layout.Rows; r++)
        {
            string row = $"rc_row_{r}";
            if (r >= players.Count)
            {
                client.Class(player, row, "hidden", true);
                continue;
            }

            RecapPlayer who = players[r];
            client.Text(player, $"{row}_name", who.Name.Trim());
            client.OptionalText(player, $"{row}_sub", who.Sub);
            client.Group(player, row, "style", Names.StyleOrNeutral(who.Style));

            for (int i = 0; i < _layout.RowStats; i++)
            {
                string id = $"{row}_stat_{i}";
                bool visible = i < who.Stats.Count;
                if (visible)
                {
                    RecapStat stat = who.Stats[i];
                    client.Text(player, $"{id}_value", stat.Value.Trim());
                    client.Text(player, $"{id}_label", stat.Label.Trim());
                    client.Group(player, $"{id}_icon", "icon", RecapIcons.Class(Names.Token(stat.Icon) ?? "none"));
                    client.Group(player, id, "style", Names.StyleOrNeutral(stat.Style));
                    client.Class(player, id, "best", stat.Best);
                }

                client.Class(player, id, "hidden", !visible);
            }

            client.Class(player, row, "hidden", false);
        }
    }

    /// <summary>Writes a tile's value; with <paramref name="pulse"/>, swaps its pulse class if the value changed.</summary>
    private static void WriteValue(CCSPlayerController player, Viewer viewer, int index, string value, bool pulse)
    {
        string id = $"rc_stat_{index}";
        bool changed = viewer.Values.TryGetValue(index, out string? previous) && previous != value;
        viewer.Values[index] = value;
        viewer.Client.Text(player, $"{id}_value", value);

        if (!pulse || !changed)
        {
            return;
        }

        string next = "pulse-a";
        if (viewer.Pulses.TryGetValue(index, out string? last) && last == "pulse-a")
        {
            next = "pulse-b";
        }

        viewer.Pulses[index] = next;
        viewer.Client.Group(player, id, "pulse", next);
    }

    private void StartCountdown(CCSPlayerController player, Viewer viewer, RecapOptions options)
    {
        bool hidden = options.RestartSeconds <= 0;
        viewer.Client.Class(player, "rc_restart", "hidden", hidden);
        if (hidden)
        {
            viewer.Client.Text(player, "rc_restart_eta", string.Empty);
            return;
        }

        float endsAt = Server.CurrentTime + options.RestartSeconds;
        Tick();

        void Tick()
        {
            if (!IsCurrent(player, viewer))
            {
                return;
            }

            int left = Math.Max(0, (int)Math.Ceiling(endsAt - Server.CurrentTime));
            viewer.Client.Text(player, "rc_restart_eta", Names.Format(options.RestartFormat, left));
            if (left > 0)
            {
                After(viewer, 1f, Tick);
            }
        }
    }
}
