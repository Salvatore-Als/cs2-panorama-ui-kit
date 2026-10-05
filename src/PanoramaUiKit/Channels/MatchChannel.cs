using CounterStrikeSharp.API.Core;
using PanoramaManager;
using PanoramaUiKit.Channels.Base;
using PanoramaUiKit.Internal;
using PanoramaUiKit.Shared.Match;

namespace PanoramaUiKit.Channels;

/// <summary>
/// Top status bar on one layout (contract "match"), the same for every human.
///
/// Text is shared (<see cref="SharedText"/>: written once, replayed to whoever opens later); classes
/// are per viewer. <see cref="Update"/> also opens the bar for anyone who does not have it yet, so
/// late joiners and round restarts heal on the next call.
/// </summary>
internal sealed class MatchChannel : LayoutChannel<ViewerState>
{
    private const string BarId = "m_bar";
    private const string Dash = "—";

    private readonly SharedText _text;

    /// <summary>True between an Update and a Hide: the bar is meant to be on screen.</summary>
    private bool _active;

    public MatchChannel(BasePlugin plugin, MatchLayout layout) : base(plugin, layout)
    {
        _text = new SharedText(Panel);
    }

    protected override ViewerState CreateViewer(int slot) => new(NewClient());

    public void Update(MatchOptions options)
    {
        _active = true;

        _text.Write("m_left_label", options.LeftLabel ?? string.Empty);
        _text.Write("m_left_value", options.LeftValue ?? Dash);
        _text.Write("m_center", options.Center ?? Dash);
        _text.Write("m_center_sub", options.CenterSub?.Trim() ?? string.Empty);
        _text.Write("m_right_label", options.RightLabel ?? string.Empty);
        _text.Write("m_right_value", options.RightValue ?? Dash);
        _text.Write("m_left_score", options.LeftScore?.ToString() ?? string.Empty);
        _text.Write("m_right_score", options.RightScore?.ToString() ?? string.Empty);
        _text.ExtraTexts("m", options.Texts);

        string style = Names.StyleOrNeutral(options.Style);
        string leftStyle = Names.StyleOrNeutral(options.LeftStyle);
        string rightStyle = Names.StyleOrNeutral(options.RightStyle);
        bool subEmpty = string.IsNullOrWhiteSpace(options.CenterSub);
        bool scoresHidden = !options.ShowScore || (options.LeftScore == null && options.RightScore == null);

        foreach (CCSPlayerController player in Humans())
        {
            ClientState client = Ensure(player, out _).Client;
            client.Group(player, BarId, "style", style);
            client.FreeClasses(player, BarId, options.Classes);
            client.Class(player, BarId, "urgent", options.Urgent);
            client.Class(player, "m_center_sub", "empty", subEmpty);
            client.Class(player, "m_scores", "hidden", scoresHidden);
            client.Group(player, "m_left", "style", leftStyle);
            client.Group(player, "m_left_tab", "style", leftStyle);
            client.Group(player, "m_right", "style", rightStyle);
            client.Group(player, "m_right_tab", "style", rightStyle);
        }
    }

    /// <summary>Hides the bar for everyone. The next Update shows it again.</summary>
    public void Hide()
    {
        _active = false;
        ClearEveryone();
    }

    public override void ClearEveryone()
    {
        _active = false;
        base.ClearEveryone();
    }

    /// <summary>
    /// The library reopens the bar by itself after a round restart, classes scrubbed. While active
    /// it stays open and the next Update repaints this viewer; otherwise it is closed.
    /// </summary>
    protected override void OnPanelEvent(PanelEvent e)
    {
        if (e.Action == PanelAction.Restored && _active)
        {
            Forget(e.Player.Slot);
            return;
        }

        base.OnPanelEvent(e);
    }
}
