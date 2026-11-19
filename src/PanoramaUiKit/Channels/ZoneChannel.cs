using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using PanoramaManager;
using PanoramaUiKit.Channels.Base;
using PanoramaUiKit.Internal;
using PanoramaUiKit.Shared.Common;
using PanoramaUiKit.Shared.Compass;
using PanoramaUiKit.Shared.Zone;

namespace PanoramaUiKit.Channels;

/// <summary>
/// The safe zone on one layout (contract "zone"), per viewer: a status widget <c>zn_status</c> and an alert
/// <c>zn_alert</c>, each with its own timers and handle. The layout closes once neither is up.
///
/// The countdown is ticked here, once a second. The bar is a clip run by the client, as in
/// <see cref="FillChannel"/>: <c>step-N</c> snaps it to the share of the phase left (no <c>dur-*</c>, no
/// transition), <c>dur-N</c> lands, and <c>run</c> one update later drains it. A restart drops <c>dur-*</c>
/// and <c>run</c> together and waits <see cref="RestartSeconds"/> before the next <c>run</c>.
/// </summary>
internal sealed class ZoneChannel : LayoutChannel<ZoneChannel.Viewer>
{
    private const string StatusId = "zn_status";
    private const string AlertId = "zn_alert";
    private const string FillId = "zn_bar_fill";

    /// <summary>Measured minimum for an off-then-on class restart to reach the client.</summary>
    private const float RestartSeconds = 0.35f;

    /// <summary>Between the duration landing and <c>run</c>: the fill needs a frame at its start state.</summary>
    private const float RunGapSeconds = 0.12f;

    /// <summary>A status sent again whose end moved less than this keeps its running bar.</summary>
    private const float SameEndSeconds = 1.5f;

    internal sealed class Viewer : ViewerState
    {
        public Viewer(ClientState client) : base(client)
        {
        }

        public readonly List<SafeTimer> StatusTimers = [];
        public readonly List<SafeTimer> AlertTimers = [];

        public ZoneOptions? Status;
        public float EndsAt;
        public UiHandle? StatusHandle;

        public bool AlertVisible;
        public UiHandle? AlertHandle;

        /// <summary>The bar has <c>dur-*</c> / <c>run</c> on, draining towards <see cref="BarEndsAt"/>.</summary>
        public bool BarRunning;
        public float BarEndsAt;
        public float BarTotal;
        public float BarResetAt = float.MinValue;

        public bool StatusVisible => Status != null;

        public static void Kill(List<SafeTimer> timers)
        {
            foreach (SafeTimer timer in timers)
            {
                timer.Kill();
            }

            timers.Clear();
        }

        public override void Release()
        {
            Kill(StatusTimers);
            Kill(AlertTimers);
            StatusHandle?.MarkGone();
            AlertHandle?.MarkGone();
            base.Release();
        }
    }

    private readonly ZoneLayout _layout;

    public ZoneChannel(BasePlugin plugin, ZoneLayout layout) : base(plugin, layout)
    {
        _layout = layout;
    }

    protected override Viewer CreateViewer(int slot) => new(NewClient());

    // Status --------------------------------------------------------------------------------------

    public IUiHandle Show(CCSPlayerController player, ZoneOptions options)
    {
        if (player is not { IsValid: true, IsBot: false })
        {
            return UiHandle.Dead;
        }

        Viewer viewer = Open(player);
        return ShowStatus(player, viewer, options, Server.CurrentTime + Math.Max(0f, options.Seconds));
    }

    private UiHandle ShowStatus(CCSPlayerController player, Viewer viewer, ZoneOptions options, float endsAt)
    {
        Viewer.Kill(viewer.StatusTimers);
        viewer.StatusHandle?.MarkGone();

        UiHandle handle = new(player);
        handle.OnClear = () => HideStatus(player, viewer, handle);
        viewer.StatusHandle = handle;
        viewer.Status = options;
        viewer.EndsAt = endsAt;

        PaintStatus(player, viewer, options);
        PaintBar(player, viewer, options, endsAt);
        viewer.Client.Class(player, StatusId, "hidden", false);
        StartCountdown(player, viewer, options);
        return handle;
    }

    public void SetOutside(CCSPlayerController player, bool outside)
    {
        if (player is { IsValid: true } && TryOpenViewer(player, out Viewer viewer) && viewer.Status != null)
        {
            viewer.Status = viewer.Status with { Outside = outside };
            viewer.Client.Class(player, StatusId, "outside", outside);
        }
    }

    private void PaintStatus(CCSPlayerController player, Viewer viewer, ZoneOptions options)
    {
        ClientState client = viewer.Client;

        client.OptionalText(player, "zn_phase", options.Phase);
        client.Text(player, "zn_state", options.StateText?.Trim() ?? options.State.DefaultText());
        client.OptionalText(player, "zn_info", options.Info);
        client.ExtraTexts(player, "zn", options.Texts);

        client.Group(player, StatusId, "state", options.State.Class());
        client.Group(player, StatusId, "style", Names.StyleOrNeutral(options.Style));
        client.Group(player, StatusId, "at", options.Placement.Class());
        client.Group(player, StatusId, "icon", IconClass(options.Icon));
        client.FreeClasses(player, StatusId, options.Classes);
        client.Class(player, StatusId, "outside", options.Outside);
    }

    private void StartCountdown(CCSPlayerController player, Viewer viewer, ZoneOptions options)
    {
        if (options.Seconds <= 0f)
        {
            viewer.Client.OptionalText(player, "zn_timer", null);
            return;
        }

        Tick();

        void Tick()
        {
            if (!IsCurrent(player, viewer) || viewer.Status != options)
            {
                return;
            }

            int left = Math.Max(0, (int)Math.Ceiling(viewer.EndsAt - Server.CurrentTime));
            viewer.Client.OptionalText(player, "zn_timer", Timer(options.TimerFormat, left));
            if (left > 0)
            {
                viewer.StatusTimers.Add(Later(1f, Tick));
            }
        }
    }

    /// <summary><c>string.Format</c> with minutes and seconds; a broken pattern shows m:ss.</summary>
    private static string Timer(string pattern, int seconds)
    {
        int minutes = seconds / 60;
        int rest = seconds % 60;
        try
        {
            return string.Format(pattern, minutes, rest);
        }
        catch (FormatException)
        {
            return $"{minutes}:{rest:00}";
        }
    }

    // Bar -----------------------------------------------------------------------------------------

    private void PaintBar(CCSPlayerController player, Viewer viewer, ZoneOptions options, float endsAt)
    {
        ClientState client = viewer.Client;
        bool hidden = options.TotalSeconds <= 0f;
        client.Class(player, "zn_bar", "hidden", hidden);
        if (hidden)
        {
            StopBar(player, viewer);
            return;
        }

        float now = Server.CurrentTime;
        float left = Math.Max(0f, endsAt - now);

        // The same phase sent again (a plugin pushing every second): the running bar is already right.
        if (viewer.BarRunning && Math.Abs(viewer.BarTotal - options.TotalSeconds) < 0.01f
            && Math.Abs(viewer.BarEndsAt - endsAt) < SameEndSeconds)
        {
            return;
        }

        StopBar(player, viewer);
        int step = (int)Math.Round(Math.Clamp(left / options.TotalSeconds, 0f, 1f) * _layout.Steps);
        client.Group(player, FillId, "step", $"step-{step}");
        if (left <= 0f || step == 0)
        {
            return;
        }

        viewer.BarRunning = true;
        viewer.BarEndsAt = endsAt;
        viewer.BarTotal = options.TotalSeconds;

        string duration = $"dur-{NearestDuration(left)}";
        float wait = Math.Max(0f, viewer.BarResetAt + RestartSeconds - now);
        viewer.StatusTimers.Add(Later(wait, () =>
        {
            if (!IsCurrent(player, viewer) || viewer.Status != options)
            {
                return;
            }

            client.Group(player, FillId, "dur", duration);
            viewer.StatusTimers.Add(Later(RunGapSeconds, () =>
            {
                if (IsCurrent(player, viewer) && viewer.Status == options)
                {
                    client.Class(player, FillId, "run", true);
                }
            }));
        }));
    }

    /// <summary>Back to a still fill: duration and run dropped together, so it snaps instead of sliding back.</summary>
    private static void StopBar(CCSPlayerController player, Viewer viewer)
    {
        if (!viewer.BarRunning)
        {
            return;
        }

        viewer.Client.Group(player, FillId, "dur", null);
        viewer.Client.Class(player, FillId, "run", false);
        viewer.BarRunning = false;
        viewer.BarResetAt = Server.CurrentTime;
    }

    private int NearestDuration(float seconds) => _layout.Durations.MinBy(d => Math.Abs(d - seconds));

    private void HideStatus(CCSPlayerController player, Viewer viewer, UiHandle? handle)
    {
        if (!IsCurrent(player, viewer) || viewer.Status == null || (handle != null && viewer.StatusHandle != handle))
        {
            return;
        }

        Viewer.Kill(viewer.StatusTimers);
        viewer.StatusHandle?.MarkGone();
        viewer.StatusHandle = null;
        viewer.Status = null;
        StopBar(player, viewer);

        if (!viewer.AlertVisible)
        {
            ClearAll(player);
            return;
        }

        viewer.Client.Class(player, StatusId, "hidden", true);
    }

    // Alert ---------------------------------------------------------------------------------------

    public IUiHandle Alert(CCSPlayerController player, ZoneAlertOptions options)
    {
        if (player is not { IsValid: true, IsBot: false })
        {
            return UiHandle.Dead;
        }

        Viewer viewer = Open(player);
        Viewer.Kill(viewer.AlertTimers);
        viewer.AlertHandle?.MarkGone();

        UiHandle handle = new(player);
        handle.OnClear = () => HideAlert(player, viewer, handle);
        viewer.AlertHandle = handle;

        ClientState client = viewer.Client;
        client.OptionalText(player, "zn_alert_kicker", options.Kicker);
        client.Text(player, "zn_alert_title", options.Title.Trim());
        client.OptionalText(player, "zn_alert_desc", options.Description);
        client.ExtraTexts(player, "zn_alert", options.Texts);
        client.Group(player, AlertId, "style", Names.StyleOrNeutral(options.Style));
        client.Group(player, AlertId, "icon", IconClass(options.Icon));
        client.FreeClasses(player, AlertId, options.Classes);

        // Already up: repainted in place. Otherwise "show" one update after "hidden" goes, for the entry.
        if (!viewer.AlertVisible)
        {
            client.Class(player, AlertId, "hidden", false);
            viewer.AlertTimers.Add(Later(Layout.EntrySeconds, () =>
            {
                if (IsCurrent(player, viewer) && viewer.AlertHandle == handle)
                {
                    client.Class(player, AlertId, "show", true);
                }
            }));
        }
        else
        {
            client.Class(player, AlertId, "show", true);
        }

        viewer.AlertVisible = true;

        if (options.Seconds > 0f)
        {
            float exitAt = Math.Max(0.1f, options.Seconds - Layout.ExitSeconds);
            viewer.AlertTimers.Add(Later(exitAt, () => HideAlert(player, viewer, handle)));
        }

        return handle;
    }

    /// <summary>Drops "show", then hides the alert (or closes the layout) once its exit has played.</summary>
    private void HideAlert(CCSPlayerController player, Viewer viewer, UiHandle? handle)
    {
        if (!IsCurrent(player, viewer) || !viewer.AlertVisible || (handle != null && viewer.AlertHandle != handle))
        {
            return;
        }

        Viewer.Kill(viewer.AlertTimers);
        viewer.AlertHandle?.MarkGone();
        viewer.AlertHandle = null;
        viewer.Client.Class(player, AlertId, "show", false);
        viewer.AlertTimers.Add(Later(Layout.ExitSeconds, () =>
        {
            if (!IsCurrent(player, viewer) || viewer.AlertHandle != null)
            {
                return;
            }

            viewer.AlertVisible = false;
            if (!viewer.StatusVisible)
            {
                ClearAll(player);
                return;
            }

            viewer.Client.Class(player, AlertId, "hidden", true);
        }));
    }

    // Shared --------------------------------------------------------------------------------------

    private static string? IconClass(string? icon) => Names.Token(icon) is { } name ? ZoneIcons.Class(name) : null;

    /// <summary>
    /// The player's viewer, the layout opened if needed. A layout just opened shows whatever its parts were left
    /// with, so both are written "hidden" first (see AnchoredChannel.HideOthers); the caller drops it on its own.
    /// </summary>
    private Viewer Open(CCSPlayerController player)
    {
        Viewer viewer = Ensure(player, out bool fresh);
        if (fresh)
        {
            viewer.Client.Class(player, StatusId, "hidden", true);
            viewer.Client.Class(player, AlertId, "hidden", true);
        }

        return viewer;
    }

    public void Hide(CCSPlayerController player)
    {
        if (player is { IsValid: true } && Viewers.ContainsKey(player.Slot))
        {
            ClearAll(player);
        }
    }

    public void HideEveryone() => ClearEveryone();

    /// <summary>
    /// After a round restart the library reopens the layout with every class scrubbed. The status is painted
    /// again with the time it has left; the alert, timed, is dropped.
    /// </summary>
    protected override void OnPanelEvent(PanelEvent e)
    {
        if (e.Action == PanelAction.Restored && Viewers.TryGetValue(e.Player.Slot, out Viewer? viewer))
        {
            ZoneOptions? status = viewer.Status;
            float endsAt = viewer.EndsAt;
            UiHandle? handle = viewer.StatusHandle;

            Forget(e.Player.Slot);
            if (status == null)
            {
                Panel.Close(e.Player);
                return;
            }

            UiHandle restored = ShowStatus(e.Player, Open(e.Player), status, endsAt);
            handle?.Link(restored);
            return;
        }

        base.OnPanelEvent(e);
    }
}
