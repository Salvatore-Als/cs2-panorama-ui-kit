using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using PanoramaManager;
using PanoramaUiKit.Channels.Base;
using PanoramaUiKit.Internal;
using PanoramaUiKit.Shared.Comms;
using PanoramaUiKit.Shared.Common;

namespace PanoramaUiKit.Channels;

/// <summary>
/// A radio transmission on one layout (contract "comms"), per viewer: one card, <c>cm_card</c>.
///
/// The card opens with its power-on (<c>show</c>) under <c>static</c> for <see cref="CommsOptions.ConnectSeconds"/>.
/// A Show while it is up repaints in place and plays a static burst: <c>burst-a</c> and <c>burst-b</c> take
/// turns, since an off-then-on of one class in the same tick never reaches the client.
/// </summary>
internal sealed class CommsChannel : LayoutChannel<CommsChannel.Viewer>
{
    private const string CardId = "cm_card";
    private const string FaceId = "cm_face";

    private static readonly string[] GlitchFlags = ["glitch-hard"];

    internal sealed class Viewer : ViewerState
    {
        public Viewer(ClientState client) : base(client)
        {
        }

        public bool Visible { get; set; }

        public CommsOptions? Content { get; set; }

        /// <summary>The burst class played last: the next swap plays the other one.</summary>
        public bool BurstA { get; set; }
    }

    public CommsChannel(BasePlugin plugin, CommsLayout layout) : base(plugin, layout)
    {
    }

    protected override Viewer CreateViewer(int slot) => new(NewClient());

    public IUiHandle Show(CCSPlayerController player, CommsOptions options)
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
            After(viewer, wait, () => Apply(player, viewer, options, fresh: false));
            return handle;
        }

        Apply(player, viewer, options, fresh);
        return handle;
    }

    /// <summary>New text on the transmission up, nothing else: no burst, the timer keeps running.</summary>
    public bool SetText(CCSPlayerController player, string text)
    {
        if (player is not { IsValid: true }
            || !Viewers.TryGetValue(player.Slot, out Viewer? viewer)
            || viewer is not { Visible: true, Content: { } content })
        {
            return false;
        }

        viewer.Content = content with { Text = text };
        viewer.Client.Text(player, "cm_text", text.Trim());
        return true;
    }

    /// <summary>Plays the power-off for this viewer, then closes the panel.</summary>
    public void Hide(CCSPlayerController player)
    {
        if (player is { IsValid: true } && Viewers.TryGetValue(player.Slot, out Viewer? viewer))
        {
            viewer.Visible = false;
            Dismiss(player, viewer, CardId);
        }
    }

    public void HideEveryone()
    {
        foreach (CCSPlayerController player in Humans())
        {
            Hide(player);
        }
    }

    private void Apply(CCSPlayerController player, Viewer viewer, CommsOptions options, bool fresh)
    {
        if (!IsCurrent(player, viewer))
        {
            return;
        }

        viewer.MarkApplied(Server.CurrentTime);
        Paint(player, viewer, options);

        if (!fresh && viewer.Visible)
        {
            // Already up: swapped in place, behind a burst of static.
            viewer.BurstA = !viewer.BurstA;
            viewer.Client.Group(player, CardId, "burst", viewer.BurstA ? "burst-a" : "burst-b");
        }
        else
        {
            // Fresh: "show" one update after the open. Leaving: back at once, the power-off is cut short.
            Connect(player, viewer, options);
            if (fresh)
            {
                RevealLater(player, viewer, CardId);
            }
            else
            {
                viewer.Client.Class(player, CardId, "show", true);
            }

            viewer.Visible = true;
        }

        if (options.Seconds > 0f)
        {
            After(viewer, options.Seconds, () => Hide(player));
        }
    }

    /// <summary><c>static</c> for the connect time, then the face settles.</summary>
    private void Connect(CCSPlayerController player, Viewer viewer, CommsOptions options)
    {
        bool connecting = options.ConnectSeconds > 0f;
        viewer.Client.Class(player, CardId, "static", connecting);
        if (!connecting)
        {
            return;
        }

        After(viewer, Layout.EntrySeconds + options.ConnectSeconds, () =>
        {
            if (IsCurrent(player, viewer))
            {
                viewer.Client.Class(player, CardId, "static", false);
            }
        });
    }

    private static void Paint(CCSPlayerController player, Viewer viewer, CommsOptions options)
    {
        ClientState client = viewer.Client;
        viewer.Content = options;

        client.OptionalText(player, "cm_kicker", options.Kicker);
        client.OptionalText(player, "cm_name", options.Name);
        client.Text(player, "cm_text", options.Text.Trim());
        client.OptionalText(player, "cm_footer", options.Footer);
        client.ExtraTexts(player, "cm", options.Texts);

        string face = Names.Token(options.Face) ?? CommsFaces.Unknown;
        client.Group(player, FaceId, "face", CommsFaces.Class(face));

        client.Group(player, CardId, "style", Names.StyleOrNeutral(options.Style));
        client.FreeClasses(player, CardId, [.. options.Classes ?? [], .. options.Glitch ? GlitchFlags : []]);
    }

    /// <summary>
    /// After a round restart the library reopens the panel with every class scrubbed. A transmission without
    /// a lifetime is shown again; a timed one is dropped.
    /// </summary>
    protected override void OnPanelEvent(PanelEvent e)
    {
        if (e.Action == PanelAction.Restored
            && Viewers.TryGetValue(e.Player.Slot, out Viewer? viewer)
            && viewer is { Visible: true, Content: { Seconds: <= 0f } content })
        {
            Forget(e.Player.Slot);
            Show(e.Player, content);
            return;
        }

        base.OnPanelEvent(e);
    }
}
