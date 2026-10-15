using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using PanoramaUiKit.Channels.Base;
using PanoramaUiKit.Internal;
using PanoramaUiKit.Shared.Common;
using PanoramaUiKit.Shared.Progress;

namespace PanoramaUiKit.Channels;

/// <param name="Value">Pushed: 0 to 1.</param>
/// <param name="Seconds">Timed: how long it runs. Zero or less = pushed.</param>
/// <param name="Size">Size class (<c>size-md</c>) for layouts that have sizes (the dial); null otherwise.</param>
internal sealed record ProgressContent(
    string? Label,
    string? ValueText,
    bool ShowPercent,
    float Value,
    float Seconds,
    ProgressDirection Direction,
    UiContent Extras,
    string? Size = null) : BoxContent
{
    /// <summary>How long a finished timed bar stays on its last frame before it hides.</summary>
    public const float LingerSeconds = 0.25f;

    public bool Timed => Seconds > 0f;

    public override float Lifetime => Timed ? Seconds + LingerSeconds : 0f;
}

/// <summary>
/// A fill on one layout: progress bars (contract "progress", ids <c>pg_*</c>) and radial dials
/// (contract "dial", ids <c>dl_*</c>) share every slot and class but the prefix and the shape of the
/// clip. One box per placement.
///
/// The fill is a clip, in one of two modes (runtime-behaviour.md):
/// - pushed: a <c>step-N</c> class. A steady value costs nothing: the client state skips an unchanged class.
/// - timed: <c>timed</c> + <c>dur-N</c> set first, <c>run</c> one network update later, then the
///   client runs it alone. <c>run</c> goes at least <see cref="RestartSeconds"/> after the last reset of
///   the same fill, or the client may never see the reset; <c>dur-*</c> and <c>run</c> are dropped
///   together, never with a new <c>dur-*</c> in the same update.
/// </summary>
internal sealed class FillChannel : AnchoredChannel<ProgressContent>
{
    /// <summary>Measured minimum for an off-then-on class restart to reach the client.</summary>
    private const float RestartSeconds = 0.35f;

    /// <summary>Between the duration landing and <c>run</c>: the fill needs a frame at its start state.</summary>
    private const float RunGapSeconds = 0.12f;

    private readonly string _prefix;
    private readonly int _steps;
    private readonly IReadOnlyList<int> _durations;

    /// <param name="prefix">Slot prefix of the contract: <c>pg</c> for progress, <c>dl</c> for dial.</param>
    public FillChannel(BasePlugin plugin, UiLayout layout, IReadOnlyList<string> keys, string prefix, int steps, IReadOnlyList<int> durations)
        : base(plugin, layout, keys)
    {
        _prefix = prefix;
        _steps = steps;
        _durations = durations;
    }

    protected override string BoxId(string key) => $"{_prefix}_{key}";

    protected override void Paint(CCSPlayerController player, BoxViewer<ProgressContent> viewer, Box<ProgressContent> box,
                                  ProgressContent content)
    {
        ClientState client = viewer.Client;
        string id = box.Id;

        client.OptionalText(player, $"{id}_label", content.Label);
        client.OptionalText(player, $"{id}_value", ValueText(content));
        client.ExtraTexts(player, id, content.Extras.Texts);
        client.Group(player, id, "style", Names.StyleOrNeutral(content.Extras.Style));
        client.FreeClasses(player, id, content.Extras.Classes);
        if (content.Size != null)
        {
            client.Group(player, id, "size", content.Size);
        }

        if (content.Timed)
        {
            PaintTimed(player, viewer, box, content);
        }
        else
        {
            PaintPushed(player, viewer, box, content);
        }
    }

    protected override void Unpaint(CCSPlayerController player, BoxViewer<ProgressContent> viewer, Box<ProgressContent> box)
    {
        if (box.TimedActive)
        {
            DropTimed(player, viewer, box);
        }
    }

    private static string? ValueText(ProgressContent content)
    {
        if (!string.IsNullOrWhiteSpace(content.ValueText))
        {
            return content.ValueText;
        }

        if (content.ShowPercent && !content.Timed)
        {
            return $"{(int)Math.Round(Math.Clamp(content.Value, 0f, 1f) * 100f)}%";
        }

        return null;
    }

    private void PaintPushed(CCSPlayerController player, BoxViewer<ProgressContent> viewer, Box<ProgressContent> box,
                             ProgressContent content)
    {
        if (box.TimedActive)
        {
            DropTimed(player, viewer, box);
        }

        int step = (int)Math.Round(Math.Clamp(content.Value, 0f, 1f) * _steps);
        viewer.Client.Group(player, $"{box.Id}_fill", "step", $"step-{step}");
    }

    private void PaintTimed(CCSPlayerController player, BoxViewer<ProgressContent> viewer, Box<ProgressContent> box,
                            ProgressContent content)
    {
        ClientState client = viewer.Client;
        string fill = $"{box.Id}_fill";

        client.Group(player, fill, "step", null);

        // Restarted while running: back to the start first, and give that reset time to be seen.
        if (box.TimedActive)
        {
            client.Group(player, fill, "dur", null);
            client.Class(player, fill, "run", false);
            box.ResetAt = Server.CurrentTime;
        }

        client.Class(player, fill, "timed", true);
        client.Class(player, fill, "up", content.Direction == ProgressDirection.Fill);
        box.TimedActive = true;

        string duration = $"dur-{NearestDuration(content.Seconds)}";
        float wait = Math.Max(0f, box.ResetAt + RestartSeconds - Server.CurrentTime);
        if (wait <= 0f)
        {
            Start(player, viewer, box, content, duration);
            return;
        }

        box.Timers.Add(Later(wait, () => Start(player, viewer, box, content, duration)));
    }

    /// <summary>Applies the duration, and <c>run</c> one update later.</summary>
    private void Start(CCSPlayerController player, BoxViewer<ProgressContent> viewer, Box<ProgressContent> box,
                       ProgressContent content, string duration)
    {
        if (!IsCurrent(player, viewer) || box.Content != content)
        {
            return;
        }

        string fill = $"{box.Id}_fill";
        viewer.Client.Group(player, fill, "dur", duration);
        box.Timers.Add(Later(RunGapSeconds, () =>
        {
            if (IsCurrent(player, viewer) && box.Content == content)
            {
                viewer.Client.Class(player, fill, "run", true);
            }
        }));
    }

    /// <summary>Back to the plain fill: duration and run dropped together, so it snaps instead of sliding back.</summary>
    private void DropTimed(CCSPlayerController player, BoxViewer<ProgressContent> viewer, Box<ProgressContent> box)
    {
        ClientState client = viewer.Client;
        string fill = $"{box.Id}_fill";

        client.Group(player, fill, "dur", null);
        client.Class(player, fill, "run", false);
        client.Class(player, fill, "timed", false);
        client.Class(player, fill, "up", false);
        box.TimedActive = false;
        box.ResetAt = Server.CurrentTime;
    }

    private int NearestDuration(float seconds) => _durations.MinBy(d => Math.Abs(d - seconds));
}
