using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Timers;
using PanoramaUiKit.Channels.Base;
using PanoramaUiKit.Internal;
using PanoramaUiKit.Shared.Common;
using PanoramaUiKit.Shared.Panels;
using CssTimer = CounterStrikeSharp.API.Modules.Timers.Timer;

namespace PanoramaUiKit.Channels;

/// <summary>
/// Modal panel on one layout (contract "panel"): header, pages in a left menu, stats, typed rows.
///
/// The caller hands a function that builds <see cref="PanelOptions"/>; the panel is rebuilt on open,
/// after every click and once a second while live, so changing content stays current without the
/// caller pushing anything.
/// </summary>
internal sealed class PanelChannel : InteractiveChannel<PanelChannel.Viewer>
{
    private const float RefreshSeconds = 1f;
    private static readonly PanelRowKind[] Kinds = Enum.GetValues<PanelRowKind>();
    private static readonly string[] PooledPrefixes = ["p_page_", "p_stat_", "p_row_"];

    internal sealed class Viewer : InteractiveViewer
    {
        public Viewer(ClientState client) : base(client)
        {
        }

        public Func<CCSPlayerController, PanelOptions> Build { get; set; } = _ => throw new InvalidOperationException("no panel");

        /// <summary>Rebuilt every second (opened from a build function).</summary>
        public bool Live { get; set; }

        public int Page { get; set; }

        /// <summary>What was painted last: the clicks resolve against it.</summary>
        public PanelOptions? Options { get; set; }
    }

    private readonly PanelLayout _layout;
    private readonly CssTimer _refresh;

    public PanelChannel(BasePlugin plugin, PanelLayout layout) : base(plugin, layout)
    {
        _layout = layout;
        _refresh = plugin.AddTimer(RefreshSeconds, RefreshLive, TimerFlags.REPEAT);
    }

    protected override string RevealId => "p_panel";

    protected override string CloseButtonId => "p_close";

    protected override Viewer CreateViewer(int slot) => new(NewInteractiveClient(slot));

    protected override bool AuthoredOn(string panelId, string className) =>
        className == "hidden" && PooledPrefixes.Any(prefix => panelId.StartsWith(prefix, StringComparison.Ordinal));

    public IUiHandle Open(CCSPlayerController player, Func<CCSPlayerController, PanelOptions> build, bool live, int page)
    {
        if (player is not { IsValid: true, IsBot: false })
        {
            return UiHandle.Dead;
        }

        // Built here for OnClose; Paint builds again, which is what a live panel does anyway.
        PanelOptions first = build(player);
        return Present(player, viewer =>
        {
            viewer.Build = build;
            viewer.Live = live;
            viewer.Page = page;
        }, first.OnClose);
    }

    public void ShowPage(CCSPlayerController player, int page)
    {
        if (player is { IsValid: true } && Viewers.TryGetValue(player.Slot, out Viewer? viewer))
        {
            viewer.Page = page;
            Paint(player, viewer);
        }
    }

    private void RefreshLive()
    {
        foreach ((int slot, Viewer viewer) in Viewers.ToList())
        {
            if (!viewer.Live)
            {
                continue;
            }

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

    // ------------------------------------------------------------------ paint

    protected override void Paint(CCSPlayerController player, Viewer viewer)
    {
        PanelOptions options = viewer.Build(player);
        viewer.Options = options;
        viewer.OnClose = options.OnClose;
        ClientState client = viewer.Client;

        int pages = Math.Min(options.Pages.Count, _layout.Pages);
        viewer.Page = Math.Clamp(viewer.Page, 0, Math.Max(0, pages - 1));

        client.Text(player, "p_title", options.Title);
        client.Text(player, "p_subtitle", options.Subtitle);
        client.Text(player, "p_hint", options.Hint);
        client.ExtraTexts(player, "p", options.Texts);

        client.Class(player, RevealId, "no-hint", string.IsNullOrWhiteSpace(options.Hint));
        client.Group(player, RevealId, "style", Names.StyleOrNeutral(options.Style));
        client.FreeClasses(player, RevealId, options.Classes);

        for (int p = 0; p < _layout.Pages; p++)
        {
            string id = $"p_page_{p}";
            bool used = p < pages;
            client.Class(player, id, "hidden", !used);
            client.Class(player, id, "active", used && p == viewer.Page);
            if (used)
            {
                client.Text(player, $"{id}_label", options.Pages[p].Label);
            }
        }

        IReadOnlyList<PanelStat> stats = [];
        IReadOnlyList<PanelRow> rows = [];
        if (pages > 0)
        {
            stats = options.Pages[viewer.Page].Stats;
            rows = options.Pages[viewer.Page].Rows;
        }

        PaintStats(player, client, stats);
        PaintRows(player, client, rows);
    }

    private void PaintStats(CCSPlayerController player, ClientState client, IReadOnlyList<PanelStat> stats)
    {
        // No stat on this page: the strip goes, margin included.
        client.Class(player, "p_stats", "hidden", stats.Count == 0 || _layout.Stats == 0);

        for (int s = 0; s < _layout.Stats; s++)
        {
            string id = $"p_stat_{s}";
            bool used = s < stats.Count;
            client.Class(player, id, "hidden", !used);
            if (!used)
            {
                continue;
            }

            client.Text(player, $"{id}_key", stats[s].Key);
            client.Text(player, $"{id}_value", stats[s].Value);
            client.Class(player, id, "no-key", string.IsNullOrWhiteSpace(stats[s].Key));
            client.Group(player, id, "style", Names.StyleClass(stats[s].Style));
        }
    }

    private void PaintRows(CCSPlayerController player, ClientState client, IReadOnlyList<PanelRow> rows)
    {
        for (int r = 0; r < _layout.Rows; r++)
        {
            string id = $"p_row_{r}";
            bool used = r < rows.Count;
            client.Class(player, id, "hidden", !used);
            if (!used)
            {
                continue;
            }

            PanelRow row = rows[r];
            client.Text(player, $"{id}_title", row.Title);
            client.Text(player, $"{id}_desc", row.Description);
            client.Text(player, $"{id}_value", row.Value);
            client.Text(player, $"{id}_btn", row.Button);

            foreach (PanelRowKind kind in Kinds)
            {
                client.Class(player, id, $"kind-{kind.ToString().ToLowerInvariant()}", kind == row.Kind);
            }

            client.Group(player, id, "style", Names.StyleClass(row.Style));
            client.Class(player, id, "no-title", row.Title.Length == 0);
            client.Class(player, id, "no-desc", row.Description.Length == 0);
            client.Class(player, id, "on", row.On);
            client.Class(player, id, "disabled", row.Disabled);
            client.Class(player, id, "selected", row.Selected);
            client.Class(player, id, "nested", row.Nested);
        }
    }

    // ------------------------------------------------------------------ clicks

    protected override void OnButton(CCSPlayerController player, Viewer viewer, string elementId)
    {
        if (viewer.Options is not PanelOptions options)
        {
            return;
        }

        if (TryIndex(elementId, "p_page_", string.Empty, out int page))
        {
            if (page < Math.Min(options.Pages.Count, _layout.Pages))
            {
                viewer.Page = page;
            }

            return;
        }

        IReadOnlyList<PanelRow> rows = [];
        if (options.Pages.Count > 0)
        {
            rows = options.Pages[viewer.Page].Rows;
        }

        PanelRow? Row(int index)
        {
            if (index < rows.Count)
            {
                return rows[index];
            }

            return null;
        }

        (PanelRow? row, Action<CCSPlayerController>? callback) = elementId switch
        {
            _ when TryIndex(elementId, "p_row_", "_btn", out int i) => (Row(i), Row(i)?.OnClick),
            _ when TryIndex(elementId, "p_row_", "_toggle", out int i) => (Row(i), Row(i)?.OnClick),
            _ when TryIndex(elementId, "p_row_", "_prev", out int i) => (Row(i), Row(i)?.OnPrevious),
            _ when TryIndex(elementId, "p_row_", "_next", out int i) => (Row(i), Row(i)?.OnNext),
            _ => (null, null),
        };

        if (row is { Disabled: false })
        {
            callback?.Invoke(player);
        }
    }

    public override void Dispose()
    {
        _refresh.Kill();
        base.Dispose();
    }
}
