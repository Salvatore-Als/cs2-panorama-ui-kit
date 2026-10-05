using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using PanoramaManager;
using PanoramaUiKit.Channels.Base;
using PanoramaUiKit.Internal;
using PanoramaUiKit.Shared.Common;

namespace PanoramaUiKit.Channels;

/// <summary>
/// What a single-card component writes. Built by the banner / announce services from their options.
/// <see cref="Flags"/> are kit-owned classes on the card (<c>animate-title</c>), set beside the caller's.
/// </summary>
internal sealed record CardContent(
    string Title,
    string? Description,
    string? Kicker,
    float Seconds,
    UiContent Extras,
    IReadOnlyList<string>? Flags = null);

/// <param name="Prefix">Slot prefix of the contract: <c>b</c> for banner, <c>a</c> for announce.</param>
/// <param name="UpdateInPlace">A Show while visible rewrites instead of exit-then-enter.</param>
/// <param name="RepaintOnRestore">Paint again after a round restart instead of dropping.</param>
/// <param name="TitleSlot">Suffix of the main text slot: <c>b_text</c>, <c>a_title</c>.</param>
internal sealed record CardBehaviour(string Prefix, bool UpdateInPlace, bool RepaintOnRestore, string TitleSlot)
{
    public static readonly CardBehaviour Banner = new("b", UpdateInPlace: true, RepaintOnRestore: true, TitleSlot: "text");

    public static readonly CardBehaviour Announce = new("a", UpdateInPlace: false, RepaintOnRestore: false, TitleSlot: "title");
}

/// <summary>
/// One card per viewer on one layout: the banner (<c>b_*</c>) and announce (<c>a_*</c>) contracts,
/// same slots under a different prefix.
///
/// <see cref="CardBehaviour.UpdateInPlace"/> (banner): a Show while the card is up rewrites text and
/// style and keeps it on screen - a countdown can call it every second. Otherwise (announce) the card
/// plays its exit, then the new one enters.
/// </summary>
internal sealed class CardChannel : LayoutChannel<CardChannel.Viewer>
{
    internal sealed class Viewer : ViewerState
    {
        public Viewer(ClientState client) : base(client)
        {
        }

        public bool Visible { get; set; }

        public CardContent? Content { get; set; }
    }

    private readonly CardBehaviour _behaviour;
    private readonly string _cardId;

    public CardChannel(BasePlugin plugin, UiLayout layout, CardBehaviour behaviour) : base(plugin, layout)
    {
        _behaviour = behaviour;
        _cardId = $"{behaviour.Prefix}_card";
    }

    protected override Viewer CreateViewer(int slot) => new(NewClient());

    public IUiHandle Show(CCSPlayerController player, CardContent content)
    {
        if (player is not { IsValid: true, IsBot: false })
        {
            return UiHandle.Dead;
        }

        Viewer viewer = Ensure(player, out bool fresh);
        viewer.CancelTimers();
        UiHandle handle = viewer.NewHandle(player, () => Hide(player));

        // Two shows in one tick: the first goes through, the next waits and the latest one wins.
        float wait = 0f;
        if (!fresh)
        {
            wait = viewer.PaceDelay(Server.CurrentTime, WriteGapSeconds);
        }

        if (wait > 0f)
        {
            After(viewer, wait, () => Apply(player, viewer, content, fresh: false));
            return handle;
        }

        Apply(player, viewer, content, fresh);
        return handle;
    }

    private void Apply(CCSPlayerController player, Viewer viewer, CardContent content, bool fresh)
    {
        if (!IsCurrent(player, viewer))
        {
            return;
        }

        viewer.MarkApplied(Server.CurrentTime);

        if (!fresh && viewer.Visible && _behaviour.UpdateInPlace)
        {
            Paint(player, viewer, content);
            ScheduleExpiry(player, viewer, content);
            return;
        }

        if (!fresh && viewer.Visible)
        {
            // Exit first, then the new card enters.
            viewer.Visible = false;
            viewer.Client.Class(player, _cardId, "show", false);
            After(viewer, Layout.ExitSeconds, () => Begin(player, viewer, content));
        }
        else
        {
            Begin(player, viewer, content);
        }
    }

    /// <summary>Plays the exit for this viewer, then closes the panel.</summary>
    public void Hide(CCSPlayerController player)
    {
        if (player is { IsValid: true } && Viewers.TryGetValue(player.Slot, out Viewer? viewer))
        {
            viewer.Visible = false;
            Dismiss(player, viewer, _cardId);
        }
    }

    public void HideEveryone()
    {
        foreach (CCSPlayerController player in Humans())
        {
            Hide(player);
        }
    }

    private void Begin(CCSPlayerController player, Viewer viewer, CardContent content)
    {
        if (!IsCurrent(player, viewer))
        {
            return;
        }

        Paint(player, viewer, content);
        viewer.Visible = true;
        RevealLater(player, viewer, _cardId);
        ScheduleExpiry(player, viewer, content);
    }

    private void Paint(CCSPlayerController player, Viewer viewer, CardContent content)
    {
        string prefix = _behaviour.Prefix;
        ClientState client = viewer.Client;
        viewer.Content = content;

        client.OptionalText(player, $"{prefix}_kicker", content.Kicker);
        client.Text(player, $"{prefix}_{_behaviour.TitleSlot}", content.Title.Trim());
        client.OptionalText(player, $"{prefix}_desc", content.Description);
        client.ExtraTexts(player, prefix, content.Extras.Texts);

        client.Group(player, _cardId, "style", Names.StyleOrNeutral(content.Extras.Style));
        client.FreeClasses(player, _cardId, [.. content.Extras.Classes ?? [], .. content.Flags ?? []]);
    }

    private void ScheduleExpiry(CCSPlayerController player, Viewer viewer, CardContent content)
    {
        if (content.Seconds > 0f)
        {
            After(viewer, content.Seconds, () => Hide(player));
        }
    }

    /// <summary>
    /// After a round restart the library reopens the panel with every class scrubbed. A sticky card
    /// (banner) is painted again; a timed one (announce) is dropped.
    /// </summary>
    protected override void OnPanelEvent(PanelEvent e)
    {
        if (e.Action == PanelAction.Restored
            && _behaviour.RepaintOnRestore
            && Viewers.TryGetValue(e.Player.Slot, out Viewer? viewer)
            && viewer is { Visible: true, Content: not null })
        {
            CardContent content = viewer.Content;
            Forget(e.Player.Slot);
            Show(e.Player, content);
            return;
        }

        base.OnPanelEvent(e);
    }
}
