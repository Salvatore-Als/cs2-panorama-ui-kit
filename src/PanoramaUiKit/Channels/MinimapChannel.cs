using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using PanoramaManager;
using PanoramaUiKit.Channels.Base;
using PanoramaUiKit.Core;
using PanoramaUiKit.Internal;
using PanoramaUiKit.Shared.Common;
using PanoramaUiKit.Shared.Compass;
using PanoramaUiKit.Shared.Minimap;

namespace PanoramaUiKit.Channels;

/// <summary>
/// The minimap of one layout (contract "minimap"). Two layers:
/// <list type="bullet">
/// <item>global, kept here: the two zones and the global marks, in world units;</item>
/// <item>per viewer: the map, its bounds, the player's own arrow and personal marks.</item>
/// </list>
/// Every viewer is painted from both, in grid cells of its own bounds. The client state only sends what changed,
/// so the follow timer can repaint everything: a player standing still costs nothing.
///
/// The next zone's shrink is the zone bar's pattern on three panels: <c>dur-*</c> dropped (snap to where the zone is
/// now), <c>dur-N</c> after <see cref="RestartSeconds"/>, the target classes one update later. Each zone carries a
/// version; a viewer paints it again only when its version moved, so a running shrink is never restarted.
///
/// The current zone is a hole in a red veil, sized per step rather than scaled (a scale would thin the veil), so the
/// client cannot run its shrink: the kit sends a step every <see cref="ShrinkStepSeconds"/> and the stylesheet glides
/// each one, linear, over that time.
/// </summary>
internal sealed class MinimapChannel : LayoutChannel<MinimapChannel.Viewer>
{
    private const string FrameId = "mm_frame";
    private const string MapId = "mm_map";
    private const int SelfSlot = 0;
    private const int RotSteps = 36;

    private const float MinFollowInterval = 0.05f;

    /// <summary>Measured minimum for an off-then-on class restart to reach the client.</summary>
    private const float RestartSeconds = 0.35f;

    /// <summary>Between the duration landing and the target classes: the panels need a frame at their start.</summary>
    private const float RunGapSeconds = 0.12f;

    private static readonly MinimapZoneId[] ZoneIds = [MinimapZoneId.Next, MinimapZoneId.Current];

    /// <summary>Between two steps of the current zone's shrink; the stylesheet's glide (tools/minimap_css.py SHRINK_STEP).</summary>
    private const float ShrinkStepSeconds = 0.1f;

    internal sealed class Viewer : ViewerState
    {
        public Viewer(ClientState client) : base(client)
        {
        }

        public MinimapOptions? Options { get; set; }

        public MinimapBounds? Bounds { get; set; }

        public string? MapClass { get; set; }

        public readonly Dictionary<int, MinimapMark> Marks = new();

        public readonly Dictionary<MinimapZoneId, int> ZoneVersions = new();

        public MinimapMark? Self { get; set; }

        public readonly Dictionary<int, Tracked> Tracks = new();
    }

    /// <summary>A followed player: by SteamID for a human (survives a reconnect), by entity slot for a bot.</summary>
    internal sealed class Tracked(MinimapTrack track, ulong steamId, int entitySlot)
    {
        public MinimapTrack Track { get; } = track;

        public ulong SteamId { get; } = steamId;

        public int EntitySlot { get; } = entitySlot;

        /// <summary>Last live position, for the faded marker of a dead player.</summary>
        public MinimapMark? Last { get; set; }
    }

    private sealed record ZoneState(MinimapZone? Shape, MinimapZone? From, float StartAt, float Seconds, int Version)
    {
        /// <summary>Where the zone is now: on its way from <see cref="From"/> while a shrink runs.</summary>
        public MinimapZone? Now(float now)
        {
            if (Shape == null || From == null || Seconds <= 0f)
            {
                return Shape;
            }

            float t = Math.Clamp((now - StartAt) / Seconds, 0f, 1f);
            return new MinimapZone(
                From.X + (Shape.X - From.X) * t,
                From.Y + (Shape.Y - From.Y) * t,
                From.Radius + (Shape.Radius - From.Radius) * t);
        }

        public float Left(float now) => Seconds <= 0f ? 0f : Math.Max(0f, StartAt + Seconds - now);
    }

    private readonly MinimapLayout _layout;
    private readonly Dictionary<MinimapZoneId, ZoneState> _zones = new();
    private readonly Dictionary<int, MinimapMark> _globalMarks = new();
    private readonly Dictionary<int, Tracked> _globalTracks = new();
    private int _zoneVersion;
    private bool _tracking;
    /// <summary>When the current zone's shrink last stepped. Its timer dies with the map: a stale one is restarted.</summary>
    private float _shrinkStepAt = float.MinValue;

    private readonly MapCatalog _maps;

    public MinimapChannel(BasePlugin plugin, MinimapLayout layout, MapCatalog maps) : base(plugin, layout)
    {
        _layout = layout;
        _maps = maps;
    }

    protected override Viewer CreateViewer(int slot) => new(NewClient());

    // Per viewer ----------------------------------------------------------------------------------

    public IUiHandle Show(CCSPlayerController player, MinimapOptions options)
    {
        if (player is not { IsValid: true, IsBot: false })
        {
            return UiHandle.Dead;
        }

        Viewer viewer = Ensure(player, out bool fresh);
        if (fresh)
        {
            viewer.Client.Class(player, FrameId, "hidden", true);
        }

        viewer.CancelTimers();
        UiHandle handle = viewer.NewHandle(player, () => Hide(player));

        viewer.Options = options;
        Resolve(viewer);
        viewer.ZoneVersions.Clear();
        if (!options.FollowSelf)
        {
            viewer.Self = null;
        }

        Sample(player, viewer);
        Paint(player, viewer);
        viewer.Client.Class(player, FrameId, "hidden", false);

        if (options.FollowSelf)
        {
            Follow(player, viewer, Math.Max(MinFollowInterval, options.FollowInterval));
        }

        return handle;
    }

    /// <summary>
    /// The picture and bounds of the map a viewer asked for (the server's when <c>Map</c> is null): from the catalog
    /// (dev, plugin, official), else the name itself as a picture with no bounds (nothing placed). Bounds given in the
    /// options win.
    /// </summary>
    private void Resolve(Viewer viewer)
    {
        MinimapOptions options = viewer.Options!;
        string? requested = options.Map ?? Server.MapName;
        MinimapMap? map = _maps.Find(requested);
        string? picture = Names.Token(map?.Picture ?? requested);
        viewer.MapClass = picture == null ? null : MinimapMaps.Class(picture);
        viewer.Bounds = options.Bounds ?? map?.Bounds;
    }

    /// <summary>A map's picture or bounds changed (dev menu, RegisterMap): every open map redrawn with them.</summary>
    public void RefreshMaps()
    {
        foreach (CCSPlayerController player in Humans())
        {
            if (Viewers.TryGetValue(player.Slot, out Viewer? viewer) && viewer.Options != null)
            {
                Resolve(viewer);
                viewer.ZoneVersions.Clear();
                Paint(player, viewer);
            }
        }
    }

    public void SetMark(CCSPlayerController player, MinimapMark mark)
    {
        if (player is not { IsValid: true } || !InPool(mark.Slot))
        {
            return;
        }

        // Kept even before the map is shown: Show paints it.
        Viewer viewer = Ensure(player, out bool fresh);
        if (fresh)
        {
            viewer.Client.Class(player, FrameId, "hidden", true);
        }

        viewer.Tracks.Remove(mark.Slot);
        viewer.Marks[mark.Slot] = mark;
        if (viewer.Options != null)
        {
            PaintMarks(player, viewer);
            PaintView(player, viewer);
        }
    }

    public void ClearMark(CCSPlayerController player, int slot)
    {
        if (player is not { IsValid: true } || !Viewers.TryGetValue(player.Slot, out Viewer? viewer))
        {
            return;
        }

        bool tracked = viewer.Tracks.Remove(slot);
        if ((viewer.Marks.Remove(slot) || tracked) && viewer.Options != null)
        {
            PaintMarks(player, viewer);
        }
    }

    public bool IsShowing(CCSPlayerController player) =>
        Viewers.TryGetValue(player.Slot, out Viewer? viewer) && viewer.Options != null;

    public void Hide(CCSPlayerController player)
    {
        if (player is { IsValid: true } && Viewers.ContainsKey(player.Slot))
        {
            ClearAll(player);
        }
    }

    public void HideEveryone() => ClearEveryone();

    // Global --------------------------------------------------------------------------------------

    public void SetZone(MinimapZoneId id, MinimapZone? zone, float shrinkSeconds)
    {
        float now = Server.CurrentTime;
        MinimapZone? from = null;
        if (zone != null && shrinkSeconds > 0f && _zones.TryGetValue(id, out ZoneState? current))
        {
            from = current.Now(now);
        }

        float seconds = from == null ? 0f : shrinkSeconds;
        _zones[id] = new ZoneState(zone, from, now, seconds, ++_zoneVersion);
        RepaintShowing(PaintZones);

        bool stepping = now >= _shrinkStepAt && now - _shrinkStepAt < ShrinkStepSeconds * 4f;
        if (id == MinimapZoneId.Current && seconds > 0f && !stepping)
        {
            _shrinkStepAt = now;
            Later(ShrinkStepSeconds, ShrinkTick);
        }
    }

    /// <summary>The current zone's next step on every map, while it shrinks; one last at its end.</summary>
    private void ShrinkTick()
    {
        RepaintShowing((player, viewer) => PaintZone(player, viewer, MinimapZoneId.Current));
        if (_zones.TryGetValue(MinimapZoneId.Current, out ZoneState? zone) && zone.Left(Server.CurrentTime) > 0f)
        {
            _shrinkStepAt = Server.CurrentTime;
            Later(ShrinkStepSeconds, ShrinkTick);
            return;
        }

        _shrinkStepAt = float.MinValue;
    }

    public void SetGlobalMark(MinimapMark mark)
    {
        if (!InPool(mark.Slot))
        {
            return;
        }

        _globalTracks.Remove(mark.Slot);
        _globalMarks[mark.Slot] = mark;
        RepaintShowing(PaintMarks);
    }

    public void ClearGlobalMark(int slot)
    {
        bool tracked = _globalTracks.Remove(slot);
        if (_globalMarks.Remove(slot) || tracked)
        {
            RepaintShowing(PaintMarks);
        }
    }

    /// <summary>Hides every map and drops the global layer.</summary>
    public void Reset()
    {
        _zones.Clear();
        _globalMarks.Clear();
        _globalTracks.Clear();
        ClearEveryone();
    }

    // Tracking ------------------------------------------------------------------------------------

    /// <summary>Follows <paramref name="target"/> in a slot: on every map (null viewer) or on one viewer's.</summary>
    public void Track(CCSPlayerController? viewer, CCSPlayerController target, MinimapTrack track)
    {
        if (target is not { IsValid: true } || track.Slot <= SelfSlot || !InPool(track.Slot))
        {
            return;
        }

        Tracked tracked = new(track, target.IsBot ? 0UL : target.SteamID, target.Slot);
        if (viewer == null)
        {
            _globalMarks.Remove(track.Slot);
            _globalTracks[track.Slot] = tracked;
        }
        else
        {
            if (viewer is not { IsValid: true })
            {
                return;
            }

            Viewer state = Ensure(viewer, out bool fresh);
            if (fresh)
            {
                state.Client.Class(viewer, FrameId, "hidden", true);
            }

            state.Marks.Remove(track.Slot);
            state.Tracks[track.Slot] = tracked;
        }

        UpdateTracks();
        if (!_tracking)
        {
            _tracking = true;
            Later(TrackSeconds, TrackTick);
        }
    }

    private const float TrackSeconds = 0.1f;

    /// <summary>Re-armed while anything is tracked, on this layout's own timer: global tracks need no viewer to tick.</summary>
    private void TrackTick()
    {
        if (_globalTracks.Count == 0 && Viewers.Values.All(v => v.Tracks.Count == 0))
        {
            _tracking = false;
            return;
        }

        UpdateTracks();
        Later(TrackSeconds, TrackTick);
    }

    private void UpdateTracks()
    {
        bool globalChanged = false;
        foreach ((int slot, Tracked tracked) in _globalTracks)
        {
            MinimapMark? mark = Resolve(tracked);
            MinimapMark? current = _globalMarks.GetValueOrDefault(slot);
            if (mark == current)
            {
                continue;
            }

            globalChanged = true;
            if (mark == null)
            {
                _globalMarks.Remove(slot);
            }
            else
            {
                _globalMarks[slot] = mark;
            }
        }

        foreach (CCSPlayerController player in Humans())
        {
            if (!Viewers.TryGetValue(player.Slot, out Viewer? viewer))
            {
                continue;
            }

            bool changed = globalChanged;
            foreach ((int slot, Tracked tracked) in viewer.Tracks)
            {
                MinimapMark? mark = Resolve(tracked);
                if (mark == viewer.Marks.GetValueOrDefault(slot))
                {
                    continue;
                }

                changed = true;
                if (mark == null)
                {
                    viewer.Marks.Remove(slot);
                }
                else
                {
                    viewer.Marks[slot] = mark;
                }
            }

            if (changed && viewer.Options != null)
            {
                PaintMarks(player, viewer);
            }
        }
    }

    /// <summary>The marker for a tracked player now: alive = live position, dead = last one faded, gone = none.</summary>
    private static MinimapMark? Resolve(Tracked tracked)
    {
        CCSPlayerController? target = tracked.SteamId != 0UL
            ? Utilities.GetPlayers().FirstOrDefault(p => p is { IsValid: true } && p.SteamID == tracked.SteamId)
            : Utilities.GetPlayerFromSlot(tracked.EntitySlot) is { IsValid: true, IsBot: true } bot ? bot : null;
        if (target == null)
        {
            tracked.Last = null;
            return null;
        }

        MinimapTrack track = tracked.Track;
        string? label = track.Label ?? target.PlayerName;
        CCSPlayerPawn? pawn = target.PlayerPawn.Value;
        if (pawn is { IsValid: true } && target.PawnIsAlive && pawn.AbsOrigin is { } origin)
        {
            tracked.Last = new MinimapMark(track.Slot, track.Kind, origin.X, origin.Y, pawn.EyeAngles.Y, null, label, track.Style);
            return tracked.Last;
        }

        if (!track.ShowDead || tracked.Last == null)
        {
            return null;
        }

        return tracked.Last with { State = MinimapNames.Dead, Label = label };
    }

    private void RepaintShowing(Action<CCSPlayerController, Viewer> paint)
    {
        foreach (CCSPlayerController player in Humans())
        {
            if (Viewers.TryGetValue(player.Slot, out Viewer? viewer) && viewer.Options != null)
            {
                paint(player, viewer);
            }
        }
    }

    /// <summary>Rebuilt after a round restart: the client starts from the layout again, so everything is repainted.</summary>
    protected override void OnPanelEvent(PanelEvent e)
    {
        if (e.Action == PanelAction.Restored
            && Viewers.TryGetValue(e.Player.Slot, out Viewer? viewer)
            && viewer.Options != null)
        {
            viewer.Client.Reset();
            viewer.ZoneVersions.Clear();
            Paint(e.Player, viewer);
            viewer.Client.Class(e.Player, FrameId, "hidden", false);
            return;
        }

        base.OnPanelEvent(e);
    }

    // Painting ------------------------------------------------------------------------------------

    private void Follow(CCSPlayerController player, Viewer viewer, float interval)
    {
        After(viewer, interval, Tick);

        void Tick()
        {
            if (!IsCurrent(player, viewer) || viewer.Options is not { FollowSelf: true })
            {
                return;
            }

            Sample(player, viewer);
            PaintMarks(player, viewer);
            PaintView(player, viewer);
            After(viewer, interval, Tick);
        }
    }

    /// <summary>The player's own arrow from the live pawn. A dead or missing pawn keeps the last one, faded.</summary>
    private static void Sample(CCSPlayerController player, Viewer viewer)
    {
        if (viewer.Options is not { FollowSelf: true })
        {
            return;
        }

        CCSPlayerPawn? pawn = player.PlayerPawn.Value;
        if (pawn is not { IsValid: true } || !player.PawnIsAlive || pawn.AbsOrigin is not { } origin)
        {
            if (viewer.Self != null)
            {
                viewer.Self = viewer.Self with { State = MinimapNames.Dead };
            }

            return;
        }

        viewer.Self = new MinimapMark(SelfSlot, MinimapNames.Self, origin.X, origin.Y, pawn.EyeAngles.Y);
    }

    private void Paint(CCSPlayerController player, Viewer viewer)
    {
        ClientState client = viewer.Client;
        MinimapOptions options = viewer.Options!;

        client.Group(player, FrameId, "at", options.Placement.Class());
        client.Group(player, FrameId, "size", options.Size.Class());
        client.Group(player, FrameId, "style", Names.StyleOrNeutral(options.Style));
        client.Group(player, FrameId, "self", SelfClass(options.SelfScale));
        client.Group(player, FrameId, "arrow", Names.Token(options.SelfArrow) is { } arrow and not MinimapArrows.Classic ? $"arrow-{arrow}" : null);
        client.FreeClasses(player, FrameId, options.Classes);
        client.Group(player, MapId, "map", viewer.MapClass);
        client.OptionalText(player, "mm_title", options.Title);
        client.ExtraTexts(player, "mm", options.Texts);

        PaintZones(player, viewer);
        PaintMarks(player, viewer);
        PaintView(player, viewer);
    }

    /// <summary>Zoom and pan: the player's cell (slot 0) in the centre, or the map's centre without one.</summary>
    private void PaintView(CCSPlayerController player, Viewer viewer)
    {
        ClientState client = viewer.Client;
        int zoom = Zoom(viewer.Options!.Zoom);
        if (zoom <= 100)
        {
            client.Group(player, "mm_zoom", "zoom", null);
            client.Group(player, "mm_pan", "px", null);
            client.Group(player, "mm_pan_y", "py", null);
            return;
        }

        int centre = _layout.Grid / 2;
        (int x, int y) = (centre, centre);
        MinimapMark? self = viewer.Self ?? viewer.Marks.GetValueOrDefault(SelfSlot);
        if (self != null && viewer.Bounds is { } bounds)
        {
            (x, y) = Cell(bounds, self.X, self.Y);
        }

        client.Group(player, "mm_zoom", "zoom", $"zoom-{zoom}");
        client.Group(player, "mm_pan", "px", $"px-{x}");
        client.Group(player, "mm_pan_y", "py", $"py-{y}");
    }

    /// <summary><c>self-NNN</c>: the arrow's scale in 10 % steps, 50 to 250; none at 100 %.</summary>
    private static string? SelfClass(float scale)
    {
        int percent = Math.Clamp((int)Math.Round(scale * 10f) * 10, 50, 250);
        return percent == 100 ? null : $"self-{percent}";
    }

    /// <summary>The nearest zoom class, in percent; 100 or less = none.</summary>
    private int Zoom(float zoom)
    {
        if (zoom <= 1.05f || _layout.Zooms.Count == 0)
        {
            return 100;
        }

        float percent = zoom * 100f;
        return _layout.Zooms.MinBy(z => Math.Abs(z - percent));
    }

    private void PaintMarks(CCSPlayerController player, Viewer viewer)
    {
        for (int slot = 0; slot < _layout.Marks; slot++)
        {
            MinimapMark? mark = slot == SelfSlot && viewer.Self != null
                ? viewer.Self
                : viewer.Marks.GetValueOrDefault(slot) ?? _globalMarks.GetValueOrDefault(slot);
            PaintMark(player, viewer, slot, mark);
        }
    }

    private void PaintMark(CCSPlayerController player, Viewer viewer, int slot, MinimapMark? mark)
    {
        ClientState client = viewer.Client;
        string id = $"mm_mark_{slot}";
        if (mark == null || viewer.Bounds is not { } bounds)
        {
            client.Class(player, id, "hidden", true);
            return;
        }

        (int x, int y) = Cell(bounds, mark.X, mark.Y);
        client.Group(player, id, "x", $"x-{x}");
        client.Group(player, $"{id}_y", "y", $"y-{y}");
        client.Group(player, id, "kind", $"kind-{Names.Token(mark.Kind) ?? MinimapNames.Dot}");
        client.Group(player, id, "state", Names.Token(mark.State) is { } state ? $"state-{state}" : null);
        client.Group(player, id, "style", Names.StyleClass(mark.Style));
        client.Group(player, $"{id}_icon", "rot", mark.Yaw is { } yaw ? $"rot-{Rot(yaw)}" : null);
        client.OptionalText(player, $"{id}_label", mark.Label);
        client.Class(player, id, "hidden", false);
    }

    private void PaintZones(CCSPlayerController player, Viewer viewer)
    {
        foreach (MinimapZoneId id in ZoneIds)
        {
            PaintZone(player, viewer, id);
        }
    }

    private void PaintZone(CCSPlayerController player, Viewer viewer, MinimapZoneId zoneId)
    {
        ClientState client = viewer.Client;
        string id = zoneId == MinimapZoneId.Current ? "mm_zone_cur" : "mm_zone_next";
        string[] panels = [id, $"{id}_y", $"{id}_ring"];

        if (!_zones.TryGetValue(zoneId, out ZoneState? zone) || zone.Shape == null || viewer.Bounds is not { } bounds)
        {
            client.Class(player, id, "hidden", true);
            viewer.ZoneVersions.Remove(zoneId);
            return;
        }

        // Current: where it is now, every time; the client state only sends a step that moved.
        if (zoneId == MinimapZoneId.Current)
        {
            PlaceZone(player, client, id, bounds, zone.Now(Server.CurrentTime)!);
            client.Class(player, id, "hidden", false);
            return;
        }

        if (viewer.ZoneVersions.TryGetValue(zoneId, out int painted) && painted == zone.Version)
        {
            return;
        }

        viewer.ZoneVersions[zoneId] = zone.Version;
        float now = Server.CurrentTime;
        float left = zone.Left(now);

        // Snap to where the zone is now, without a duration.
        foreach (string panel in panels)
        {
            client.Group(player, panel, "dur", null);
        }

        PlaceZone(player, client, id, bounds, zone.Now(now)!);
        client.Class(player, id, "hidden", false);
        if (left <= RestartSeconds + RunGapSeconds)
        {
            PlaceZone(player, client, id, bounds, zone.Shape);
            return;
        }

        string duration = $"dur-{NearestDuration(left - RestartSeconds)}";
        After(viewer, RestartSeconds, () =>
        {
            if (!IsCurrent(player, viewer) || viewer.ZoneVersions.GetValueOrDefault(zoneId) != zone.Version)
            {
                return;
            }

            foreach (string panel in panels)
            {
                client.Group(player, panel, "dur", duration);
            }

            After(viewer, RunGapSeconds, () =>
            {
                if (IsCurrent(player, viewer) && viewer.ZoneVersions.GetValueOrDefault(zoneId) == zone.Version)
                {
                    PlaceZone(player, client, id, bounds, zone.Shape);
                }
            });
        });
    }

    private void PlaceZone(CCSPlayerController player, ClientState client, string id, MinimapBounds bounds, MinimapZone zone)
    {
        (int x, int y) = Cell(bounds, zone.X, zone.Y);
        client.Group(player, id, "x", $"x-{x}");
        client.Group(player, $"{id}_y", "y", $"y-{y}");
        client.Group(player, $"{id}_ring", "r", $"r-{Radius(bounds, zone.Radius)}");
    }

    // Conversions ---------------------------------------------------------------------------------

    private (int X, int Y) Cell(MinimapBounds bounds, float x, float y)
    {
        (float u, float v) = bounds.Normalize(x, y);
        int last = _layout.Grid - 1;
        return (Math.Clamp((int)(u * _layout.Grid), 0, last), Math.Clamp((int)(v * _layout.Grid), 0, last));
    }

    /// <summary><c>r-N</c>: N / Radii of the map's width, rounded up so a small zone stays visible.</summary>
    private int Radius(MinimapBounds bounds, float radius)
    {
        if (bounds.Width <= 0f || radius <= 0f)
        {
            return 0;
        }

        return Math.Clamp((int)Math.Ceiling(radius / bounds.Width * _layout.Radii), 1, _layout.Radii);
    }

    /// <summary>Game yaw to <c>rot-N</c>: clockwise from north, 10 degrees a step.</summary>
    private static int Rot(float yaw)
    {
        float heading = CompassNames.HeadingFromYaw(yaw);
        return (int)Math.Round(heading / (360f / RotSteps)) % RotSteps;
    }

    private int NearestDuration(float seconds) => _layout.Durations.MinBy(d => Math.Abs(d - seconds));

    private bool InPool(int slot) => slot >= 0 && slot < _layout.Marks;
}
