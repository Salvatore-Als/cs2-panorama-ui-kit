using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using PanoramaUiKit.Channels.Base;
using PanoramaUiKit.Internal;
using PanoramaUiKit.Shared.Common;
using PanoramaUiKit.Shared.Votes;

namespace PanoramaUiKit.Channels;

/// <summary>
/// The screen of a vote on one layout (contract "vote"), per viewer. It draws what it is given
/// (<see cref="VoteOptions"/>) and reports clicks; the tally is the caller's.
///
/// The bar of a choice is a ladder of <c>p-0</c>..<c>p-20</c> classes (5 % steps) on its fill panel, a
/// <c>clip</c> in the stylesheet, so a text update next to it does not make it snap back. The countdown
/// is ticked here, once a second, per viewer; at zero the screen is locked and clicks are dropped.
/// A repaint after a click or an update only sends what changed. A vote longer than the layout's pool is
/// paged: the viewer keeps its page, and a click on slot n of page p is choice p * pool + n.
/// </summary>
internal sealed class VoteChannel : InteractiveChannel<VoteChannel.Viewer>
{
    private const string ScreenId = "vt_screen";
    private const string ChoicePrefix = "vt_choice_";
    private const string PrevId = "vt_prev";
    private const string NextId = "vt_next";

    /// <summary>The fill bar moves in steps of this many percent: p-0..p-20.</summary>
    private const int FillStepPercent = 5;

    internal sealed class Viewer : InteractiveViewer
    {
        public Viewer(ClientState client) : base(client)
        {
        }

        public required VoteOptions Options { get; set; }

        public int Page { get; set; }

        /// <summary>The countdown ran out.</summary>
        public bool TimedOut { get; set; }

        /// <summary>Server time the countdown ends at. Meaningless without <see cref="VoteOptions.Seconds"/>.</summary>
        public float EndsAt { get; set; }

        public bool IsLocked => TimedOut || Options.Locked;
    }

    private readonly VoteLayout _layout;

    public VoteChannel(BasePlugin plugin, VoteLayout layout) : base(plugin, layout)
    {
        _layout = layout;
    }

    protected override string RevealId => ScreenId;

    protected override string CloseButtonId => "vt_close";

    protected override Viewer CreateViewer(int slot) => new(NewInteractiveClient(slot))
    {
        Options = null!,
    };

    /// <summary>Choice buttons are authored hidden.</summary>
    protected override bool AuthoredOn(string panelId, string className)
    {
        return className == "hidden" && TryIndex(panelId, ChoicePrefix, string.Empty, out _);
    }

    public IUiHandle Show(CCSPlayerController player, VoteOptions options)
    {
        if (player is not { IsValid: true, IsBot: false })
        {
            return UiHandle.Dead;
        }

        return Present(player, viewer =>
        {
            viewer.Options = options;
            viewer.TimedOut = false;
            StartCountdown(player, viewer);
        }, options.OnClose);
    }

    /// <summary>New counts for an open vote. The countdown and the close callback stay as they were.</summary>
    public void Update(CCSPlayerController player, VoteOptions options)
    {
        if (player is { IsValid: true } && Viewers.TryGetValue(player.Slot, out Viewer? viewer))
        {
            viewer.Options = options;
            Paint(player, viewer);
        }
    }

    public void CloseEveryone()
    {
        foreach (CCSPlayerController player in Humans())
        {
            Close(player);
        }
    }

    protected override void OnButton(CCSPlayerController player, Viewer viewer, string elementId)
    {
        if (elementId == PrevId)
        {
            viewer.Page--;
            return;
        }

        if (elementId == NextId)
        {
            viewer.Page++;
            return;
        }

        if (viewer.IsLocked || !TryIndex(elementId, ChoicePrefix, string.Empty, out int slot))
        {
            return;
        }

        VoteOptions options = viewer.Options;
        int index = (ClampPage(viewer) * _layout.Choices) + slot;
        if (index >= options.Choices.Count)
        {
            return;
        }

        options.OnVote?.Invoke(new VoteEvent(player, index));
    }

    private int PageCount(VoteOptions options) => Math.Max(1, (int)Math.Ceiling(options.Choices.Count / (double)_layout.Choices));

    /// <summary>The viewer's page, pulled back into range when an update shortened the vote.</summary>
    private int ClampPage(Viewer viewer)
    {
        viewer.Page = Math.Clamp(viewer.Page, 0, PageCount(viewer.Options) - 1);
        return viewer.Page;
    }

    protected override void Paint(CCSPlayerController player, Viewer viewer)
    {
        ClientState client = viewer.Client;
        VoteOptions options = viewer.Options;

        client.OptionalText(player, "vt_kicker", options.Kicker);
        client.Text(player, "vt_title", options.Title.Trim());
        client.ExtraTexts(player, "vt", options.Texts);
        client.Text(player, "vt_time", TimeText(viewer));

        client.Group(player, ScreenId, "style", Names.StyleOrNeutral(options.Style));
        client.FreeClasses(player, ScreenId, options.Classes);
        client.Class(player, ScreenId, "locked", viewer.IsLocked);
        client.Class(player, ScreenId, "no-close", !options.Closable);

        long total = 0;
        int most = 0;
        foreach (VoteChoice choice in options.Choices)
        {
            int votes = Math.Max(0, choice.Votes);
            total += votes;
            most = Math.Max(most, votes);
        }

        client.Text(player, "vt_total", Names.Format(options.TotalFormat, total));

        int pages = PageCount(options);
        int page = ClampPage(viewer);
        client.Class(player, ScreenId, "no-pager", pages <= 1);
        client.Text(player, "vt_page", $"{page + 1} / {pages}");
        client.Class(player, PrevId, "disabled", page <= 0);
        client.Class(player, NextId, "disabled", page >= pages - 1);

        for (int slot = 0; slot < _layout.Choices; slot++)
        {
            string id = $"{ChoicePrefix}{slot}";
            int index = (page * _layout.Choices) + slot;
            bool visible = index < options.Choices.Count;
            if (visible)
            {
                PaintChoice(player, client, id, options.Choices[index], total, most, options.Selected == index);
            }

            client.Class(player, id, "hidden", !visible);
        }
    }

    private static void PaintChoice(CCSPlayerController player, ClientState client, string id, VoteChoice choice,
                                    long total, int most, bool selected)
    {
        int votes = Math.Max(0, choice.Votes);
        int percent = 0;
        if (total > 0)
        {
            percent = (int)Math.Round(votes * 100.0 / total);
        }

        int step = (int)Math.Round(percent / (double)FillStepPercent);

        client.Text(player, $"{id}_label", choice.Label.Trim());
        client.OptionalText(player, $"{id}_sub", choice.Sub);
        client.Text(player, $"{id}_count", votes.ToString());
        client.Text(player, $"{id}_pct", $"{percent}%");

        client.Group(player, id, "style", Names.StyleOrNeutral(choice.Style));
        client.Group(player, $"{id}_fill", "p", $"p-{step}");
        client.Class(player, id, "selected", selected);
        client.Class(player, id, "leading", votes > 0 && votes == most);
    }

    private void StartCountdown(CCSPlayerController player, Viewer viewer)
    {
        if (viewer.Options.Seconds <= 0f)
        {
            return;
        }

        viewer.EndsAt = Server.CurrentTime + viewer.Options.Seconds;
        After(viewer, 1f, () => Tick(player, viewer));
    }

    private void Tick(CCSPlayerController player, Viewer viewer)
    {
        if (!IsCurrent(player, viewer))
        {
            return;
        }

        viewer.Client.Text(player, "vt_time", TimeText(viewer));
        if (SecondsLeft(viewer) > 0)
        {
            After(viewer, 1f, () => Tick(player, viewer));
            return;
        }

        viewer.TimedOut = true;
        viewer.Client.Class(player, ScreenId, "locked", true);
    }

    private static int SecondsLeft(Viewer viewer) => Math.Max(0, (int)Math.Ceiling(viewer.EndsAt - Server.CurrentTime));

    private static string TimeText(Viewer viewer)
    {
        if (viewer.Options.Seconds <= 0f)
        {
            return string.Empty;
        }

        return Names.Format(viewer.Options.TimeFormat, SecondsLeft(viewer));
    }
}
