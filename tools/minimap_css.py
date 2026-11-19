"""
Writes the sizes of the two minimap stylesheets: the GEOMETRY block of uikit_minimap.css, and uikit_bigmap.css whole.

    python tools/minimap_css.py

Both layouts are contract "minimap" and share every class. uikit_minimap.css is hand-written (the look) except for its
GEOMETRY block, which this script writes from the map and marker sizes: panel sizes, the x-/y- ladders, the zone
rings, the pan. uikit_bigmap.css is the same look three times as big in the centre of the screen: this file's look
with the frame swapped and the geometry written for the bigger map. Edit the look in uikit_minimap.css, the sizes
here; never edit uikit_bigmap.css or the GEOMETRY block by hand.

The rule every size here follows: a child moved entirely outside its parent's box is not drawn, even with
overflow: noclip. So a marker's two panels are as big as the map (its icon in their top-left cell), and a zone's two
panels are as big as its widest circle, pulled back by margins so that their centre is the zone's.
"""

from __future__ import annotations

import re
from dataclasses import dataclass
from pathlib import Path

STYLES = Path(__file__).resolve().parent.parent / "workshop" / "panorama" / "styles" / "custom_game"
SOURCE = STYLES / "uikit_minimap.css"
TARGET = STYLES / "uikit_bigmap.css"

GRID = 768
"""One grid for both maps (MinimapLayout.Grid): 1px cells on the big map, a third of a pixel on the corner one. The
same on both so a layout declared without sizes still places correctly on either stylesheet."""
RADII = 768
"""Radius steps (MinimapLayout.Radii): r-768 is a radius as wide as the map, one step a pixel on the big map."""

GLIDE = 0.1
"""Seconds a marker and the pan take to reach a new cell, linear: about the time between two cells at a run, so the
next target lands while the last glide is ending and the motion reads as continuous."""

SHRINK_STEP = 0.1
"""Seconds between two steps of the current zone during a shrink: MinimapChannel.ShrinkStepSeconds."""
BEGIN = "/* GEOMETRY:BEGIN */"
END = "/* GEOMETRY:END */"


@dataclass(frozen=True)
class Geometry:
    map: int
    """Side of the map, px."""
    box: int
    """Side of a marker's cell (the icon's box), px."""
    scale: float
    """Marker icons, halo and label against the corner map's."""

    @property
    def cell(self) -> float:
        return self.map / GRID

    @property
    def veil(self) -> int:
        """Border of the current zone: from the circle's edge to past the farthest corner (zone centre on a corner)."""
        return round(self.map * 1.5)

    @property
    def zone(self) -> int:
        """Side of a zone's panels: the widest current-zone circle (radius = the map, plus the veil)."""
        return 2 * (self.map + self.veil)

    def px(self, value: float) -> int:
        return round(value * self.scale)


ICONS = {"": 14, "kind-self": 18, "kind-mate": 16, "kind-npc-boss": 20, "kind-cluster": 16}


def geometry(g: Geometry) -> str:
    cell = g.cell
    centre = lambda n: n * cell + cell / 2  # noqa: E731
    pull = round(g.box / 2 - g.zone / 2)
    halo = g.px(22)
    out = [
        BEGIN,
        f"/* Written by tools/minimap_css.py: {g.map}px map, {GRID} x {GRID} grid ({cell:.3g}px cells), {g.box}px marker cell. Do not edit. */",
        "",
        "/* Markers: both panels as big as the map, the icon in their top-left cell. */",
        ".mm-mark, .mm-mark-y {",
        f"    width: {g.map}px;",
        f"    height: {g.map}px;",
        "    overflow: noclip;",
        "    transition-property: position;",
        f"    transition-duration: {GLIDE:g}s;",
        "    transition-timing-function: linear;",
        "}",
        "",
        f".mm-mark-icon {{ width: {g.box}px; height: {g.box}px; background-size: {g.px(ICONS[''])}px {g.px(ICONS[''])}px; }}",
    ]
    for kind, size in ICONS.items():
        if kind == "kind-self":
            # In percent of its box: a SelfScale grows the box, an arrow style sets its own share of it.
            share = round(g.px(size) / g.box * 100)
            out.append(f".mm-mark.{kind} .mm-mark-icon {{ background-size: {share}% {share}%; }}")
        elif kind:
            out.append(f".mm-mark.{kind} .mm-mark-icon {{ background-size: {g.px(size)}px {g.px(size)}px; }}")
    out += [
        f".mm-mark-halo {{ width: {halo}px; height: {halo}px; x: {(g.box - halo) // 2}px; y: {(g.box - halo) // 2}px; }}",
        "",
        "/* The player's arrow scaled: self-NNN on the frame (MinimapOptions.SelfScale). Box, picture and halo grow about the cell's centre. */",
        *self_scales(g, halo),
        "",
        f".mm-mark-label {{ width: 160px; x: {g.box // 2 - 80}px; y: {g.px(21)}px; }}",
        "",
        f"/* Zones: panels {g.zone}px square, centred on the zone (margins pull them back by half), the ring in their middle. */",
        ".mm-zone, .mm-zone-y {",
        f"    width: {g.zone}px;",
        f"    height: {g.zone}px;",
        "    overflow: noclip;",
        "    transition-property: position;",
        "    transition-duration: 0.0s;",
        "    transition-timing-function: linear;",
        "}",
        "",
        f".mm-zone {{ margin-left: {pull}px; }}",
        f".mm-zone-y {{ margin-top: {pull}px; }}",
        "",
        f"/* Current: the kit sends a step every {SHRINK_STEP:g} s while it shrinks; its centre glides, linear, over that time. */",
        f".mm-zone-cur, .mm-zone-cur .mm-zone-y {{ transition-duration: {SHRINK_STEP:g}s; }}",
        "",
        ".mm-ring {",
        "    horizontal-align: center;",
        "    vertical-align: center;",
        "    transition-property: transform;",
        "    transition-duration: 0.0s;",
        "    transition-timing-function: linear;",
        "}",
        "",
        "/* Next: a ring as wide as two maps, scaled by r-N. Thick at full size so a small one keeps a border. */",
        f".mm-zone-next .mm-ring {{ width: {2 * g.map}px; height: {2 * g.map}px; border: {round(6 * g.map / 256)}px solid #f2f4f7cc; transform: scale3d( 0.0, 0.0, 1.0 ); }}",
        "",
        f"/* Current: a circle of the zone's radius plus a {g.veil}px border, the veil; sized, not scaled. */",
        f".mm-zone-cur .mm-ring {{ width: {2 * g.veil}px; height: {2 * g.veil}px; border: {g.veil}px solid #c8281a59; transition-duration: 0.0s; }}",
        "",
        "/* Grid: x-N / y-N, the centre of cell N minus half the marker cell. */",
    ]
    # Whole pixels: the compiler has only ever been given whole ones here.
    out += [f".x-{n} {{ x: {round(centre(n) - g.box / 2)}px; }}" for n in range(GRID)]
    out += [f".y-{n} {{ y: {round(centre(n) - g.box / 2)}px; }}" for n in range(GRID)]
    out.append("")
    out.append(f"/* Radius: r-N, N/{RADII} of the map's width. */")
    for n in range(RADII + 1):
        out.append(f".mm-zone-next .mm-ring.r-{n} {{ transform: scale3d( {n / RADII:.4f}, {n / RADII:.4f}, 1.0 ); }}")
    for n in range(RADII + 1):
        side = 2 * (round(g.map * n / RADII) + g.veil)
        out.append(f".mm-zone-cur .mm-ring.r-{n} {{ width: {side}px; height: {side}px; }}")
    out.append("")
    out.append(f"/* Pan: px-N / py-N put the centre of cell N in the middle of the viewport; same glide as the markers. */")
    out.append(f".mm-pan, .mm-pan-y {{ transition-duration: {GLIDE:g}s; }}")
    out += [f".mm-pan.px-{n} {{ transform: translateX( {round(g.map / 2 - centre(n))}px ); }}" for n in range(GRID)]
    out += [f".mm-pan-y.py-{n} {{ transform: translateY( {round(g.map / 2 - centre(n))}px ); }}" for n in range(GRID)]
    out.append(END)
    return "\n".join(out)


def self_scales(g: Geometry, halo: int) -> list[str]:
    out = []
    for percent in range(50, 251, 10):
        if percent == 100:
            continue
        s = percent / 100
        box, ring = round(g.box * s), round(halo * s)
        at, ring_at = round((g.box - box) / 2), round((g.box - ring) / 2)
        out.append(f".minimap.self-{percent} .mm-mark.kind-self .mm-mark-icon {{ width: {box}px; height: {box}px; x: {at}px; y: {at}px; }}")
        out.append(f".minimap.self-{percent} .mm-mark.kind-self .mm-mark-halo {{ width: {ring}px; height: {ring}px; x: {ring_at}px; y: {ring_at}px; }}")
    return out


def with_geometry(css: str, g: Geometry) -> str:
    start, end = css.index(BEGIN), css.index(END) + len(END)
    return css[:start] + geometry(g) + css[end:]


SMALL = Geometry(map=256, box=24, scale=1.0)
BIG = Geometry(map=768, box=32, scale=1.4)

BIG_HEADER = f"""/*
    uikit_bigmap - the big map, in the centre of the screen: where you are, the zones, everyone tracked.
    Contract "minimap" (docs/CONTRACTS.md), like uikit_minimap: same ids, same classes, same API - open it with
    IMinimapService.Show on a MinimapLayout named "uikit_bigmap", close it with Hide.

    GENERATED from uikit_minimap.css by tools/minimap_css.py: {BIG.map}px map, {BIG.cell:.3g}px cells, bigger markers,
    a dark veil over the screen. at-* and size-* are accepted and ignored: it is always centred.
    Everything else (pictures, zoom, ladders, zones) works as described in uikit_minimap.css.
*/
"""

BIG_FRAME = """/* Frame: the whole screen, the veil filling it, the map in its middle. ------------ */

/*
    The frame is the screen, not a box sized by its content: a veil inside a content-sized frame grows the frame
    to the veil's size, and the map, at the frame's top-left corner, ends up off screen - a veil and nothing else.
    No flow: the veil, the map and the title are layered, each placed by its own alignment.
*/
.minimap {
    width: 100%;
    height: 100%;
    overflow: noclip;
}

.minimap.at-tr, .minimap.at-tl, .minimap.at-bl, .minimap.at-br { margin: 0px; }
.minimap.size-sm, .minimap.size-md, .minimap.size-lg { ui-scale: 100%; }

.minimap-veil {
    visibility: visible;
    width: 100%;
    height: 100%;
    background-color: #05080cb3;
}

.minimap-box {
    horizontal-align: center;
    vertical-align: center;
    padding: 5px;
    border-radius: 8px;
    border: 1px solid #ffffff26;
    background-color: #12171ef2;
    box-shadow: #00000099 0px 8px 30px 0px;
}

/* Above the map: the map is centred, so the title gets the band over it. */
.minimap .minimap-title {
    vertical-align: top;
    margin-top: 48px;
}
"""


def big(css: str) -> str:
    css = css[css.index("*/") + 2:].lstrip("\n")
    frame_start = css.index("/* Frame")
    frame_end = css.index(".minimap.style-info")
    css = css[:frame_start] + BIG_FRAME + "\n" + css[frame_end:]
    css = re.sub(r"/\* A screen veil for a stylesheet that wants one.*?\n\.minimap-veil \{\n    visibility: collapse;\n\}\n\n", "", css, flags=re.S)

    # The look's own sizes: map, label and title fonts. The geometry is written again below.
    start, end = css.index(BEGIN), css.index(END) + len(END)
    look = css[:start].replace(f"{SMALL.map}px", f"{BIG.map}px")
    look = look.replace("    font-size: 11px;\n    color: #f2f4f7;", "    font-size: 15px;\n    color: #f2f4f7;")
    look = look.replace("    margin-top: 5px;\n    font-family: Stratum2 Bold;\n    font-size: 13px;",
                        "    margin-top: 8px;\n    font-family: Stratum2 Bold;\n    font-size: 20px;")
    return BIG_HEADER + "\n" + look + geometry(BIG) + css[end:]


def main() -> None:
    css = with_geometry(SOURCE.read_text(encoding="utf-8"), SMALL)
    SOURCE.write_text(css, encoding="utf-8")
    print(f"wrote {SOURCE}")
    TARGET.write_text(big(css).rstrip("\n") + "\n", encoding="utf-8")
    print(f"wrote {TARGET}")


if __name__ == "__main__":
    main()
