using System.Globalization;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using PanoramaUiKit.Shared.Api;
using PanoramaUiKit.Shared.Common;
using PanoramaUiKit.Shared.Minimap;

namespace PanoramaUiKit.Demo;

public sealed partial class DemoPlugin
{
    private const float MinimapDemoZoneRadius = 1800f;
    private const float MinimapDemoNextRadius = 800f;
    private const int MinimapDemoShrinkSeconds = 20;

    /// <summary>The player's arrow on the demo maps: style and size, kept between calls.</summary>
    private string _minimapDemoArrow = MinimapArrows.Classic;

    private float _minimapDemoSelf = 1f;

    /// <summary>Viewers following everyone else on their demo maps ("follow").</summary>
    private readonly HashSet<int> _minimapDemoFollowing = [];

    /// <summary>Marker slots of the coloured points ("colors") and of the followed players ("follow").</summary>
    private const int MinimapDemoColorSlot = 20;

    private static readonly MinimapTrackList MinimapDemoFollowList = new() { FirstSlot = 30, Slots = 32 };

    /// <summary>Centre the demo placed its zones around: the caller, or the middle of a map previewed elsewhere.</summary>
    private (float X, float Y)? _minimapDemoCentre;

    private static readonly Dictionary<string, MinimapPlacement> MinimapDemoPlacements = new(StringComparer.OrdinalIgnoreCase)
    {
        ["tl"] = MinimapPlacement.TopLeft,
        ["tr"] = MinimapPlacement.TopRight,
        ["bl"] = MinimapPlacement.BottomLeft,
        ["br"] = MinimapPlacement.BottomRight,
    };

    private static readonly Dictionary<string, MinimapSize> MinimapDemoSizes = new(StringComparer.OrdinalIgnoreCase)
    {
        ["sm"] = MinimapSize.Small,
        ["md"] = MinimapSize.Medium,
        ["lg"] = MinimapSize.Large,
    };

    /// <summary>
    /// The current map with the caller's arrow, a red zone around them, the next one in white, a teammate and a few
    /// points of interest. A corner (tl, tr, bl, br), a size (sm, md, lg) and "zoom N" (1.5 to 4) change the view;
    /// "shrink" runs the red zone down onto the white one over 20 s; "track" follows every other player (bots too)
    /// on your map; "off" hides it and drops the zones. "map &lt;name&gt;" previews another map's picture (de_mirage,
    /// slot-1, ...): everything is then placed around its centre and your arrow is off, your position meaning
    /// nothing there. "big" toggles the big map in the centre (uikit_bigmap) instead of the corner one.
    /// "arrow &lt;name&gt;" picks the player's arrow (MinimapArrows), "self &lt;n&gt;" its size (0.5 to 2.5), both kept for the
    /// next calls; "colors" adds a point in each MinimapColors colour around you; "follow" toggles every other player
    /// on your maps, one colour each (TrackPlayersFor with a list).
    /// </summary>
    [ConsoleCommand("css_uikit_demo_minimap", "Minimap of the current map: your arrow, zones, a mate and points of interest.")]
    [CommandHelper(usage: "[tl|tr|bl|br] [sm|md|lg] [zoom <n>] [map <name>] [arrow <name>] [self <n>] [big] [colors|follow|shrink|track|off]", whoCanExecute: CommandUsage.CLIENT_ONLY)]
    public void OnDemoMinimap(CCSPlayerController? player, CommandInfo command)
    {
        if (!TryKitFor(player, command, out IPanoramaUiKit kit, out CCSPlayerController target))
        {
            return;
        }

        if (HasArg(command, "off"))
        {
            kit.Minimap.Reset();
            return;
        }

        // Personal marks go on both: the corner map and the big one show the same thing.
        MinimapLayout corner = DemoLayouts.Minimap;
        MinimapLayout bigMap = DemoLayouts.BigMap;
        MinimapLayout[] layouts = [corner, bigMap];

        MinimapPlacement placement = MinimapPlacement.TopRight;
        MinimapSize size = MinimapSize.Medium;
        float zoom = 1f;
        string? map = null;
        for (int i = 1; i < command.ArgCount; i++)
        {
            string arg = command.GetArg(i).Trim();
            if (MinimapDemoPlacements.TryGetValue(arg, out MinimapPlacement asCorner))
            {
                placement = asCorner;
            }
            else if (MinimapDemoSizes.TryGetValue(arg, out MinimapSize named))
            {
                size = named;
            }
            else if (arg.Equals("zoom", StringComparison.OrdinalIgnoreCase) && i + 1 < command.ArgCount
                     && float.TryParse(command.GetArg(i + 1), NumberStyles.Float, CultureInfo.InvariantCulture, out float asked))
            {
                zoom = asked;
            }
            else if (arg.Equals("map", StringComparison.OrdinalIgnoreCase) && i + 1 < command.ArgCount)
            {
                map = command.GetArg(i + 1).Trim().ToLowerInvariant();
            }
            else if (arg.Equals("arrow", StringComparison.OrdinalIgnoreCase) && i + 1 < command.ArgCount)
            {
                _minimapDemoArrow = command.GetArg(i + 1).Trim().ToLowerInvariant();
            }
            else if (arg.Equals("self", StringComparison.OrdinalIgnoreCase) && i + 1 < command.ArgCount
                     && float.TryParse(command.GetArg(i + 1), NumberStyles.Float, CultureInfo.InvariantCulture, out float self))
            {
                _minimapDemoSelf = self;
            }
        }

        bool preview = map != null && !map.Equals(Server.MapName, StringComparison.OrdinalIgnoreCase);
        MinimapBounds? previewBounds = null;
        if (preview && !MinimapMaps.Official.TryGetValue(map!, out previewBounds))
        {
            // A slot or an unknown name: the picture alone, on a square of 8192 units.
            previewBounds = new MinimapBounds(-4096f, -4096f, 4096f, 4096f);
        }

        if (HasArg(command, "track"))
        {
            // Slots 7 and up: 0 is your arrow, 1-6 the demo's own marks.
            int slot = 7;
            foreach (CCSPlayerController other in Utilities.GetPlayers().Where(p => p is { IsValid: true, IsHLTV: false } && p.Slot != target.Slot))
            {
                // 7 to 19: the coloured points start at 20.
                if (slot >= MinimapDemoColorSlot)
                {
                    break;
                }

                MinimapTrack track = new() { Slot = slot++, Kind = MinimapNames.Mate };
                foreach (MinimapLayout layout in layouts)
                {
                    kit.Minimap.TrackPlayerFor(target, layout, other, track);
                }
            }

            command.ReplyToCommand($"[uikit] minimap: following {slot - 7} player(s); kill one, kick one, watch the marker");
            return;
        }

        if (HasArg(command, "follow"))
        {
            List<CCSPlayerController> others = [];
            if (_minimapDemoFollowing.Add(target.Slot))
            {
                others = Utilities.GetPlayers().Where(p => p is { IsValid: true, IsHLTV: false } && p.Slot != target.Slot).ToList();
            }
            else
            {
                _minimapDemoFollowing.Remove(target.Slot);
            }

            foreach (MinimapLayout layout in layouts)
            {
                kit.Minimap.TrackPlayersFor(target, layout, others, MinimapDemoFollowList);
            }

            command.ReplyToCommand(others.Count > 0 ? $"[uikit] minimap: following {others.Count} player(s), one colour each" : "[uikit] minimap: follow stopped");
            return;
        }

        if (HasArg(command, "colors"))
        {
            if (target.PlayerPawn.Value?.AbsOrigin is not { } here)
            {
                command.ReplyToCommand("[uikit] minimap: spawn first");
                return;
            }

            for (int i = 0; i < MinimapColors.All.Count; i++)
            {
                double angle = i * Math.Tau / MinimapColors.All.Count;
                string color = MinimapColors.All[i];
                kit.Minimap.SetGlobalMark(null, new MinimapMark(MinimapDemoColorSlot + i, MinimapNames.Dot,
                    here.X + (float)Math.Cos(angle) * 700f, here.Y + (float)Math.Sin(angle) * 700f, Label: color, Style: color));
            }

            command.ReplyToCommand("[uikit] minimap: one point per colour, in a ring around you");
            return;
        }

        if (HasArg(command, "shrink"))
        {
            if (_minimapDemoCentre is not { } centre)
            {
                command.ReplyToCommand("[uikit] minimap: show it first");
                return;
            }

            (float cx, float cy) = centre;
            kit.Minimap.SetZone(null, MinimapZoneId.Current, new MinimapZone(cx + 300f, cy - 200f, MinimapDemoNextRadius), MinimapDemoShrinkSeconds);
            command.ReplyToCommand($"[uikit] minimap: the red zone closes onto the white one over {MinimapDemoShrinkSeconds} s");
            return;
        }

        float x;
        float y;
        if (previewBounds != null)
        {
            x = (previewBounds.MinX + previewBounds.MaxX) / 2f;
            y = (previewBounds.MinY + previewBounds.MaxY) / 2f;
        }
        else if (target.PlayerPawn.Value?.AbsOrigin is { } at)
        {
            x = at.X;
            y = at.Y;
        }
        else
        {
            command.ReplyToCommand("[uikit] minimap: spawn first, the demo places everything around you");
            return;
        }

        _minimapDemoCentre = (x, y);

        kit.Minimap.SetZone(null, MinimapZoneId.Current, new MinimapZone(x, y, MinimapDemoZoneRadius));
        kit.Minimap.SetZone(null, MinimapZoneId.Next, new MinimapZone(x + 300f, y - 200f, MinimapDemoNextRadius));
        kit.Minimap.SetGlobalMark(null, new MinimapMark(2, MinimapNames.Npc, x + 900f, y + 600f));
        kit.Minimap.SetGlobalMark(null, new MinimapMark(3, MinimapNames.NpcBoss, x - 1200f, y - 700f, Label: "Boss"));
        kit.Minimap.SetGlobalMark(null, new MinimapMark(4, MinimapNames.Loot, x - 500f, y + 1100f));
        kit.Minimap.SetGlobalMark(null, new MinimapMark(5, MinimapNames.Cluster, x + 1400f, y - 1100f, Label: "x4"));
        foreach (MinimapLayout layout in layouts) { kit.Minimap.SetMark(target, layout, new MinimapMark(1, MinimapNames.Mate, x + 400f, y + 250f, Yaw: 200f, Label: "Mate")); }
        foreach (MinimapLayout layout in layouts) { kit.Minimap.SetMark(target, layout, new MinimapMark(6, MinimapNames.Ping, x - 350f, y - 450f)); }

        string shown = map ?? Server.MapName;
        bool big = HasArg(command, "big");
        if (big && kit.Minimap.IsShowing(target, bigMap))
        {
            kit.Minimap.Hide(target, bigMap);
            command.ReplyToCommand("[uikit] minimap: big map closed");
            return;
        }

        kit.Minimap.Show(target, new MinimapOptions
        {
            Layout = big ? bigMap : corner,
            Map = shown,
            Bounds = previewBounds,
            FollowSelf = !preview,
            Placement = placement,
            Size = size,
            Zoom = zoom,
            Title = shown,
            Style = UiStyle.Neutral,
            SelfArrow = _minimapDemoArrow,
            SelfScale = _minimapDemoSelf,
        });

        MinimapBounds? known = previewBounds ?? kit.Minimap.FindMap(shown)?.Bounds;
        string where = "no bounds known for this map: nothing is placed, only the background shows (try: map de_dust2)";
        if (known != null)
        {
            // Same maths as the kit: the cell your arrow should be in, to check a calibration by eye.
            (float u, float v) = known.Normalize(x, y);
            int grid = DemoLayouts.Minimap.Grid;
            where = string.Create(CultureInfo.InvariantCulture,
                $"you at ({x:0}, {y:0}) = {u * 100f:0}% from the left, {v * 100f:0}% from the top, cell x-{Math.Clamp((int)(u * grid), 0, grid - 1)} y-{Math.Clamp((int)(v * grid), 0, grid - 1)}");
        }

        command.ReplyToCommand($"[uikit] minimap: {(big ? bigMap : corner).Name}, {shown}, {(big ? "big" : placement.ToString())}, zoom {zoom.ToString(CultureInfo.InvariantCulture)}, arrow {_minimapDemoArrow} x{_minimapDemoSelf.ToString(CultureInfo.InvariantCulture)}; {where}");
    }
}
