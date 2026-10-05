using CounterStrikeSharp.API.Core;
using PanoramaUiKit.Channels.Base;
using PanoramaUiKit.Internal;
using PanoramaUiKit.Shared.Common;
using PanoramaUiKit.Shared.Roulette;

namespace PanoramaUiKit.Channels;

/// <summary>
/// Case-opening roulette on one layout (contract "roulette"), per viewer.
///
/// The winner is drawn here, before anything is shown; the strip is the show. Sequence:
/// 1. the cards are written a few at a time (a strip is dozens of writes, spread over several
///    updates), the screen is shown once they are in;
/// 2. after <see cref="RouletteLayout.SpinDelaySeconds"/> "spin" is added: the stylesheet moves the
///    strip most of the way at full speed at once (an ease-out would start slowly), then "settle",
///    added <see cref="RouletteLayout.SettleAfterFraction"/> of the way through the "dur-N" time,
///    slows it onto the winner's card (plus one of a few "jit-N" offsets, so it does not stop dead
///    centre every time);
/// 3. when the strip has stopped "done" and "won" mark the winner, the result line is shown, and the
///    callback runs. The result stays <see cref="RouletteOptions.HoldSeconds"/>, then the screen leaves.
///
/// The callback runs exactly once: at the stop, or as "interrupted" if the viewer is dropped first.
/// </summary>
internal sealed class RouletteChannel : LayoutChannel<RouletteChannel.Viewer>
{
    private const string ScreenId = "r_screen";
    private const string StripId = "r_strip";

    /// <summary>Offsets the stylesheet has for where in the winning card the strip stops.</summary>
    private const int JitterSteps = 5;

    /// <summary>Cards written per network update.</summary>
    private const int CellsPerWrite = 12;

    /// <summary>The strip has stopped on the client a moment after the server says so: ping, one tick.</summary>
    private const float StopMarginSeconds = 0.1f;

    internal sealed record Pending(CCSPlayerController Player, RouletteOptions Options, int Index);

    internal sealed class Viewer : ViewerState
    {
        public Viewer(ClientState client) : base(client)
        {
        }

        /// <summary>The draw, until its callback has run.</summary>
        public Pending? Pending { get; set; }

        /// <summary>Runs the callback once. No-op when it already has.</summary>
        public void Deliver(bool interrupted)
        {
            if (Pending is not { } pending)
            {
                return;
            }

            Pending = null;
            RouletteItem item = pending.Options.Items[pending.Index];
            pending.Options.OnResult?.Invoke(new RouletteResult(pending.Player, item, pending.Index, interrupted));
        }

        public override void Release()
        {
            try
            {
                Deliver(interrupted: true);
            }
            finally
            {
                base.Release();
            }
        }
    }

    private readonly RouletteLayout _layout;

    public RouletteChannel(BasePlugin plugin, RouletteLayout layout) : base(plugin, layout)
    {
        _layout = layout;
    }

    protected override Viewer CreateViewer(int slot) => new(NewClient());

    public IUiHandle Spin(CCSPlayerController player, RouletteOptions options)
    {
        if (player is not { IsValid: true, IsBot: false })
        {
            return UiHandle.Dead;
        }

        int[]? strip = RouletteDraw.Strip(options.Items, _layout.Cells, _layout.WinnerCell, Random.Shared, out int winner);
        if (strip == null)
        {
            return UiHandle.Dead;
        }

        Viewer viewer = Ensure(player, out bool fresh);
        if (!fresh)
        {
            return Replace(player, viewer, options);
        }

        UiHandle handle = viewer.NewHandle(player, () => Hide(player));
        viewer.Pending = new Pending(player, options, winner);

        int seconds = NearestDuration(options.Seconds);
        Paint(player, viewer, options, strip, winner, seconds);

        // Cards in a few updates, then the screen, then the spin.
        float written = WriteCards(player, viewer, options, strip);
        After(viewer, written, () => RevealLater(player, viewer, ScreenId, () => StartSpin(player, viewer, options, winner, seconds)));

        return handle;
    }

    /// <summary>A spin while one is up: the old one ends, and the new one starts once the screen has left.</summary>
    private IUiHandle Replace(CCSPlayerController player, Viewer viewer, RouletteOptions options)
    {
        Hide(player);

        UiHandle provisional = new(player);
        Later(_layout.ExitSeconds + 0.1f, () => provisional.Link(Spin(player, options)));
        return provisional;
    }

    /// <summary>Ends the spin for this viewer, interrupted if it had not stopped, then closes the panel.</summary>
    public void Hide(CCSPlayerController player)
    {
        if (player is { IsValid: true } && Viewers.TryGetValue(player.Slot, out Viewer? viewer))
        {
            Dismiss(player, viewer, ScreenId);
            viewer.Deliver(interrupted: true);
        }
    }

    public void HideEveryone()
    {
        foreach (CCSPlayerController player in Humans())
        {
            Hide(player);
        }
    }

    /// <summary>Everything but the cards: texts, styles, the strip's duration and stopping offset.</summary>
    private void Paint(CCSPlayerController player, Viewer viewer, RouletteOptions options, int[] strip, int winner, int seconds)
    {
        ClientState client = viewer.Client;
        RouletteItem won = options.Items[winner];

        client.OptionalText(player, "r_kicker", options.Kicker);
        client.Text(player, "r_title", options.Title.Trim());
        client.Text(player, "r_result_label", options.ResultLabel);
        client.Text(player, "r_result_name", won.Name);
        client.OptionalText(player, "r_result_sub", won.Sub);
        client.ExtraTexts(player, "r", options.Texts);

        client.Group(player, ScreenId, "style", Names.StyleOrNeutral(options.Style));
        client.FreeClasses(player, ScreenId, options.Classes);

        bool anySub = options.Items.Any(item => !string.IsNullOrWhiteSpace(item.Sub));
        client.Class(player, StripId, "no-sub", !anySub);
        client.Group(player, StripId, "dur", $"dur-{seconds}");
        client.Group(player, StripId, "jit", $"jit-{Random.Shared.Next(JitterSteps)}");

        client.Class(player, "r_result", "hidden", true);
    }

    /// <summary>Writes the cards in groups, one network update apart. Returns how long that takes.</summary>
    private float WriteCards(CCSPlayerController player, Viewer viewer, RouletteOptions options, int[] strip)
    {
        float delay = 0f;
        for (int from = 0; from < strip.Length; from += CellsPerWrite)
        {
            int start = from;
            if (delay <= 0f)
            {
                PaintCells(player, viewer, options, strip, start);
            }
            else
            {
                After(viewer, delay, () => PaintCells(player, viewer, options, strip, start));
            }

            delay += WriteGapSeconds;
        }

        return delay;
    }

    private void PaintCells(CCSPlayerController player, Viewer viewer, RouletteOptions options, int[] strip, int from)
    {
        if (!IsCurrent(player, viewer))
        {
            return;
        }

        ClientState client = viewer.Client;
        bool anySub = options.Items.Any(item => !string.IsNullOrWhiteSpace(item.Sub));
        int end = Math.Min(strip.Length, from + CellsPerWrite);
        for (int i = from; i < end; i++)
        {
            RouletteItem item = options.Items[strip[i]];
            string id = $"r_cell_{i}";

            client.Text(player, $"{id}_name", item.Name.Trim());
            if (anySub)
            {
                client.OptionalText(player, $"{id}_sub", item.Sub);
            }

            client.Group(player, id, "style", Names.StyleOrNeutral(item.Style));
        }
    }

    private void StartSpin(CCSPlayerController player, Viewer viewer, RouletteOptions options, int winner, int seconds)
    {
        After(viewer, _layout.SpinDelaySeconds, () =>
        {
            if (!IsCurrent(player, viewer))
            {
                return;
            }

            viewer.Client.Class(player, StripId, "spin", true);
            After(viewer, seconds * _layout.SettleAfterFraction, () => Settle(player, viewer));
            After(viewer, seconds + StopMarginSeconds, () => Stop(player, viewer, options));
        });
    }

    private void Settle(CCSPlayerController player, Viewer viewer)
    {
        if (IsCurrent(player, viewer))
        {
            viewer.Client.Class(player, StripId, "settle", true);
        }
    }

    private void Stop(CCSPlayerController player, Viewer viewer, RouletteOptions options)
    {
        if (!IsCurrent(player, viewer))
        {
            return;
        }

        ClientState client = viewer.Client;
        client.Class(player, StripId, "done", true);
        client.Class(player, $"r_cell_{_layout.WinnerCell}", "won", true);
        client.Class(player, "r_result", "hidden", false);

        if (options.HoldSeconds > 0f)
        {
            After(viewer, options.HoldSeconds, () => Hide(player));
        }

        // Last: the callback may clear the handle or start another spin.
        viewer.Deliver(interrupted: false);
    }

    private int NearestDuration(float seconds) => _layout.Durations.MinBy(d => Math.Abs(d - seconds));
}
