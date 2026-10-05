using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using PanoramaUiKit.Channels.Base;
using PanoramaUiKit.Internal;
using PanoramaUiKit.Shared.Common;
using PanoramaUiKit.Shared.Victory;

namespace PanoramaUiKit.Channels;

/// <summary>
/// End of round screen on one layout (contract "victory"), per viewer.
///
/// Elements of the bundled layout enter one after the other through transition-delay, so the whole
/// sequence is one "show" class and a single network write. The restart countdown is ticked here,
/// once a second, per viewer. A round restart destroys the layout entity and closes it for everyone,
/// which is usually how this screen ends.
/// </summary>
internal sealed class VictoryChannel : LayoutChannel<VictoryChannel.Viewer>
{
    private const string ScreenId = "v_screen";

    internal sealed class Viewer : ViewerState
    {
        public Viewer(ClientState client) : base(client)
        {
        }

        public bool Visible { get; set; }
    }

    private readonly VictoryLayout _layout;

    public VictoryChannel(BasePlugin plugin, VictoryLayout layout) : base(plugin, layout)
    {
        _layout = layout;
    }

    protected override Viewer CreateViewer(int slot) => new(NewClient());

    public IUiHandle Show(CCSPlayerController player, VictoryOptions options)
    {
        if (player is not { IsValid: true, IsBot: false })
        {
            return UiHandle.Dead;
        }

        Viewer viewer = Ensure(player, out bool fresh);
        viewer.CancelTimers();
        UiHandle handle = viewer.NewHandle(player, () => Hide(player));

        Paint(player, viewer, options);
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

    private void Paint(CCSPlayerController player, Viewer viewer, VictoryOptions options)
    {
        ClientState client = viewer.Client;

        client.OptionalText(player, "v_kicker", options.Kicker);
        client.Text(player, "v_title", options.Title.Trim());
        client.OptionalText(player, "v_subtitle", options.Subtitle);
        client.Text(player, "v_restart_label", options.RestartLabel);
        client.ExtraTexts(player, "v", options.Texts);

        client.Group(player, ScreenId, "style", Names.StyleOrNeutral(options.Style));
        client.FreeClasses(player, ScreenId, options.Classes);

        int count = Math.Min(options.Chips.Count, _layout.Chips);
        client.Class(player, "v_chips", "hidden", count == 0);
        for (int i = 0; i < _layout.Chips; i++)
        {
            string id = $"v_chip_{i}";
            bool visible = i < count;
            if (visible)
            {
                VictoryChip chip = options.Chips[i];
                client.Text(player, $"{id}_label", chip.Label);
                client.Text(player, $"{id}_name", chip.Name);
                client.Group(player, id, "style", Names.StyleOrNeutral(chip.Style));
            }

            client.Class(player, id, "hidden", !visible);
        }
    }

    private void StartCountdown(CCSPlayerController player, Viewer viewer, VictoryOptions options)
    {
        bool hidden = options.RestartSeconds <= 0;
        viewer.Client.Class(player, "v_restart", "hidden", hidden);
        if (hidden)
        {
            viewer.Client.Text(player, "v_restart_eta", string.Empty);
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
            viewer.Client.Text(player, "v_restart_eta", Names.Format(options.RestartFormat, left));
            if (left > 0)
            {
                After(viewer, 1f, Tick);
            }
        }
    }
}
