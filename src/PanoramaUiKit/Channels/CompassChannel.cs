using System.Globalization;
using System.Numerics;
using CounterStrikeSharp.API.Core;
using PanoramaManager;
using PanoramaUiKit.Channels.Base;
using PanoramaUiKit.Internal;
using PanoramaUiKit.Shared.Common;
using PanoramaUiKit.Shared.Compass;

namespace PanoramaUiKit.Channels;

/// <summary>
/// The compass of one layout (contract "compass"), per viewer: the strip <c>cp_strip</c> and a pool of pins.
///
/// The heading is one class of a ladder, <c>hd-N</c> (2.5 degrees a step). The strip runs from -180 to 540
/// degrees, so the heading is kept unwrapped, near the last one: a turn past north moves the strip on
/// instead of sliding it a whole turn back. Only after one and a half turns one way does it jump back by 360.
/// Pins sit in the viewport, not on the strip, at their heading relative to the player's (<c>rel-N</c>).
///
/// While following, the view is read on a timer per viewer and the whole thing repainted: the client state
/// only sends what changed, so a player standing still costs nothing.
/// </summary>
internal sealed class CompassChannel : LayoutChannel<CompassChannel.Viewer>
{
    private const string BarId = "cp_bar";
    private const string StripId = "cp_strip";

    /// <summary>Degrees per class of the strip and pin ladders.</summary>
    private const float StepDegrees = 2.5f;

    /// <summary>Lowest and highest unwrapped heading the strip can show: <c>hd-0</c> and <c>hd-216</c>.</summary>
    private const float MinHeading = -90f;
    private const float MaxHeading = 450f;

    /// <summary>Pins further than this from the caret go to an edge: <c>rel-0</c>..<c>rel-72</c> cover -90..90.</summary>
    private const float PinRange = 90f;

    private const float MinFollowInterval = 0.05f;

    /// <summary>Game units to metres: a unit is an inch.</summary>
    private const float MetresPerUnit = 0.0254f;

    internal sealed class Viewer : ViewerState
    {
        public Viewer(ClientState client) : base(client)
        {
        }

        public CompassOptions? Options { get; set; }

        public IReadOnlyList<CompassPin> Pins { get; set; } = [];

        /// <summary>Heading painted last, unwrapped (may be below 0 or above 360). Null before the first paint.</summary>
        public float? Unwrapped { get; set; }

        /// <summary>Last heading and position read from the pawn, kept while the player is dead.</summary>
        public float Heading { get; set; }

        public Vector3? Origin { get; set; }
    }

    private readonly CompassLayout _layout;

    public CompassChannel(BasePlugin plugin, CompassLayout layout) : base(plugin, layout)
    {
        _layout = layout;
    }

    protected override Viewer CreateViewer(int slot) => new(NewClient());

    public IUiHandle Show(CCSPlayerController player, CompassOptions options)
    {
        if (player is not { IsValid: true, IsBot: false })
        {
            return UiHandle.Dead;
        }

        Viewer viewer = Ensure(player, out _);
        viewer.CancelTimers();
        UiHandle handle = viewer.NewHandle(player, () => Hide(player));
        viewer.Options = options;
        viewer.Pins = options.Pins;

        Sample(player, viewer);
        Paint(player, viewer);
        viewer.Client.Class(player, BarId, "hidden", false);

        if (options.Follow)
        {
            Follow(player, viewer, Math.Max(MinFollowInterval, options.FollowInterval));
        }

        return handle;
    }

    public bool SetPins(CCSPlayerController player, IReadOnlyList<CompassPin> pins)
    {
        if (player is not { IsValid: true }
            || !Viewers.TryGetValue(player.Slot, out Viewer? viewer)
            || viewer.Options == null)
        {
            return false;
        }

        viewer.Pins = pins;
        Paint(player, viewer);
        return true;
    }

    public bool IsShowing(CCSPlayerController player) =>
        Viewers.TryGetValue(player.Slot, out Viewer? viewer) && viewer.Options != null;

    /// <summary>Closes the layout for this player; the follow timer goes with the viewer.</summary>
    public void Hide(CCSPlayerController player)
    {
        if (player is { IsValid: true } && Viewers.ContainsKey(player.Slot))
        {
            ClearAll(player);
        }
    }

    public void HideEveryone() => ClearEveryone();

    /// <summary>Rebuilt after a round restart: the client starts from the layout again, so everything is repainted.</summary>
    protected override void OnPanelEvent(PanelEvent e)
    {
        if (e.Action == PanelAction.Restored
            && Viewers.TryGetValue(e.Player.Slot, out Viewer? viewer)
            && viewer.Options != null)
        {
            viewer.Client.Reset();
            Paint(e.Player, viewer);
            viewer.Client.Class(e.Player, BarId, "hidden", false);
            return;
        }

        base.OnPanelEvent(e);
    }

    /// <summary>Reads the view every <paramref name="interval"/> and repaints, for as long as the viewer lives.</summary>
    private void Follow(CCSPlayerController player, Viewer viewer, float interval)
    {
        After(viewer, interval, Tick);

        void Tick()
        {
            if (!IsCurrent(player, viewer) || viewer.Options == null)
            {
                return;
            }

            Sample(player, viewer);
            Paint(player, viewer);
            After(viewer, interval, Tick);
        }
    }

    /// <summary>Heading (following) and position from the live pawn. A dead or missing pawn keeps the last ones.</summary>
    private static void Sample(CCSPlayerController player, Viewer viewer)
    {
        CompassOptions options = viewer.Options!;
        if (!options.Follow)
        {
            viewer.Heading = CompassNames.Wrap360(options.Heading);
        }

        CCSPlayerPawn? pawn = player.PlayerPawn.Value;
        if (pawn is not { IsValid: true } || !player.PawnIsAlive)
        {
            return;
        }

        if (options.Follow)
        {
            viewer.Heading = CompassNames.HeadingFromYaw(pawn.EyeAngles.Y);
        }

        if (pawn.AbsOrigin is { } origin)
        {
            viewer.Origin = new Vector3(origin.X, origin.Y, origin.Z);
        }
    }

    private void Paint(CCSPlayerController player, Viewer viewer)
    {
        ClientState client = viewer.Client;
        CompassOptions options = viewer.Options!;

        client.Group(player, BarId, "style", Names.StyleOrNeutral(options.Style));
        client.Group(player, BarId, "at", options.Placement.Class());
        client.FreeClasses(player, BarId, options.Classes);
        client.ExtraTexts(player, "cp", options.Texts);

        float unwrapped = Unwrap(viewer.Heading, viewer.Unwrapped);
        viewer.Unwrapped = unwrapped;
        int step = (int)Math.Round((unwrapped - MinHeading) / StepDegrees);
        client.Group(player, StripId, "hd", $"hd-{step}");
        client.Text(player, "cp_deg", ((int)Math.Round(viewer.Heading) % 360).ToString(CultureInfo.InvariantCulture));

        for (int i = 0; i < _layout.Pins; i++)
        {
            string id = $"cp_pin_{i}";
            if (i >= viewer.Pins.Count)
            {
                client.Class(player, id, "hidden", true);
                continue;
            }

            PaintPin(player, client, id, viewer.Pins[i], viewer);
            client.Class(player, id, "hidden", false);
        }
    }

    private static void PaintPin(CCSPlayerController player, ClientState client, string id, CompassPin pin, Viewer viewer)
    {
        float bearing;
        string? distance = null;
        if (pin.Bearing is { } fixedBearing)
        {
            bearing = fixedBearing;
        }
        else if (viewer.Origin is { } origin)
        {
            bearing = CompassNames.HeadingTo(origin.X, origin.Y, pin.Position.X, pin.Position.Y);
            if (pin.ShowDistance)
            {
                distance = Distance(Vector2.Distance(new Vector2(origin.X, origin.Y), new Vector2(pin.Position.X, pin.Position.Y)));
            }
        }
        else
        {
            // No position read yet (spawned dead): nothing to measure from.
            bearing = viewer.Heading;
        }

        // -180..180 from the caret, clockwise positive.
        float relative = CompassNames.Wrap360(bearing - viewer.Heading + 180f) - 180f;
        if (relative < -PinRange)
        {
            client.Group(player, id, "rel", null);
            client.Group(player, id, "edge", "edge-l");
        }
        else if (relative > PinRange)
        {
            client.Group(player, id, "rel", null);
            client.Group(player, id, "edge", "edge-r");
        }
        else
        {
            client.Group(player, id, "edge", null);
            client.Group(player, id, "rel", $"rel-{(int)Math.Round((relative + PinRange) / StepDegrees)}");
        }

        client.Group(player, id, "kind", CompassNames.Class(Names.Token(pin.Kind) ?? CompassNames.Dot));
        client.Group(player, id, "style", Names.StyleOrNeutral(pin.Style));
        client.OptionalText(player, $"{id}_dist", distance);
    }

    /// <summary>
    /// <paramref name="heading"/> (0..360) as the turn nearest to <paramref name="previous"/> that the strip can
    /// show. With none in range, the heading itself: the strip jumps back once.
    /// </summary>
    private static float Unwrap(float heading, float? previous)
    {
        if (previous is not { } last)
        {
            return heading;
        }

        float best = heading;
        foreach (float candidate in new[] { heading - 360f, heading + 360f })
        {
            if (candidate >= MinHeading && candidate <= MaxHeading && Math.Abs(candidate - last) < Math.Abs(best - last))
            {
                best = candidate;
            }
        }

        return best;
    }

    private static string Distance(float units)
    {
        float metres = units * MetresPerUnit;
        if (metres >= 1000f)
        {
            return (metres / 1000f).ToString("0.0", CultureInfo.InvariantCulture) + "km";
        }

        return ((int)Math.Round(metres)).ToString(CultureInfo.InvariantCulture) + "m";
    }
}
