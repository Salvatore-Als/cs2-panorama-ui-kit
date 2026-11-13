"""
Writes uikit_bigmap.css from uikit_minimap.css: the same map, three times as big, in the centre of the screen.

    python tools/bigmap_css.py

Both layouts are contract "minimap" and share every class, so the big one is the small one's stylesheet with
the sizes changed and the ladders (x-/y-, px-/py-) recomputed for the bigger map. Edit uikit_minimap.css, then
run this again; never edit uikit_bigmap.css by hand.
"""

from __future__ import annotations

import re
from pathlib import Path

STYLES = Path(__file__).resolve().parent.parent / "workshop" / "panorama" / "styles" / "custom_game"
SOURCE = STYLES / "uikit_minimap.css"
TARGET = STYLES / "uikit_bigmap.css"

GRID = 64
SMALL_MAP, BIG_MAP = 256, 768
SMALL_BOX, BIG_BOX = 24, 32

HEADER = f"""/*
    uikit_bigmap - the big map, in the centre of the screen: where you are, the zones, everyone tracked.
    Contract "minimap" (docs/CONTRACTS.md), like uikit_minimap: same ids, same classes, same API - open it with
    IMinimapService.Show on a MinimapLayout named "uikit_bigmap", close it with Hide.

    GENERATED from uikit_minimap.css by tools/bigmap_css.py: {BIG_MAP}px map, {BIG_MAP // GRID}px cells, bigger markers,
    a dark veil over the screen. at-* and size-* are accepted and ignored: it is always centred.
    Everything else (pictures, zoom, ladders, zones) works as described in uikit_minimap.css.
*/
"""

FRAME = """/* Frame: centred, over a veil. ------------------------------------------------- */

.minimap {
    flow-children: down;
    horizontal-align: center;
    vertical-align: center;
    overflow: noclip;
}

.minimap.at-tr, .minimap.at-tl, .minimap.at-bl, .minimap.at-br { horizontal-align: center; vertical-align: center; margin: 0px; }
.minimap.size-sm, .minimap.size-md, .minimap.size-lg { ui-scale: 100%; }

/*
    The veil: a panel in the frame, out of its flow, bigger than any screen and centred on the frame (which is
    centred on the screen). Not a box-shadow with a huge spread: that makes the frame's layer enormous, and the
    map stopped redrawing what moves on it.
*/
.minimap-veil {
    visibility: visible;
    ignore-parent-flow: true;
    width: 4000px;
    height: 2400px;
    horizontal-align: center;
    vertical-align: center;
    background-color: #05080cb3;
}

.minimap-box {
    padding: 5px;
    border-radius: 8px;
    border: 1px solid #ffffff26;
    background-color: #12171ef2;
    box-shadow: #00000099 0px 8px 30px 0px;
}
"""


def ladder(prefix: str, selector: str, axis: str, value) -> str:
    return "\n".join(f"{selector}.{prefix}-{n} {{ transform: translate{axis}( {value(n):g}px ); }}" for n in range(GRID))


def main() -> None:
    css = SOURCE.read_text(encoding="utf-8")
    scale = BIG_MAP / SMALL_MAP
    cell = BIG_MAP / GRID

    # Header and frame.
    css = css[css.index("*/") + 2:].lstrip("\n")
    frame_start = css.index("/* Frame")
    frame_end = css.index(".minimap.style-info")
    css = css[:frame_start] + FRAME + "\n" + css[frame_end:]
    css = re.sub(r"/\* A screen veil for a stylesheet that wants one.*?\n\.minimap-veil \{\n    visibility: collapse;\n\}\n\n", "", css, flags=re.S)

    # Sizes.
    css = css.replace(f"{SMALL_MAP}px", f"{BIG_MAP}px")
    css = css.replace(f"width: {SMALL_MAP * 2}px;", f"width: {BIG_MAP * 2}px;")
    css = css.replace(f"height: {SMALL_MAP * 2}px;", f"height: {BIG_MAP * 2}px;")
    ring_offset = SMALL_BOX // 2 - SMALL_MAP
    css = css.replace(f"x: {ring_offset}px;", f"x: {BIG_BOX // 2 - BIG_MAP}px;")
    css = css.replace(f"y: {ring_offset}px;", f"y: {BIG_BOX // 2 - BIG_MAP}px;")
    css = css.replace("border: 8px solid #e0432f;", f"border: {round(8 * scale)}px solid #e0432f;")
    css = css.replace("border: 6px solid #f2f4f7cc;", f"border: {round(6 * scale)}px solid #f2f4f7cc;")

    # Markers: icons and labels about 1.4x.
    def bigger(match: re.Match) -> str:
        return f"{match.group(1)}{round(int(match.group(2)) * 1.4)}px"

    css = re.sub(r"(\.mm-mark-icon[^{]*\{[^}]*?width: )(\d+)px", bigger, css)
    css = re.sub(r"(\.mm-mark-icon[^{]*\{[^}]*?height: )(\d+)px", bigger, css)
    # The box markers and zone centres share (the ladders are computed for it): after the icons, so it is not grown.
    css = css.replace(f"width: {SMALL_BOX}px;\n    height: {SMALL_BOX}px;", f"width: {BIG_BOX}px;\n    height: {BIG_BOX}px;")
    css = css.replace(".mm-mark-halo {\n    width: 22px;\n    height: 22px;", ".mm-mark-halo {\n    width: 31px;\n    height: 31px;")
    css = css.replace("    y: 21px;\n    font-family: Stratum2 Bold;\n    font-size: 11px;",
                      "    y: 30px;\n    font-family: Stratum2 Bold;\n    font-size: 15px;")
    css = css.replace("    margin-top: 5px;\n    font-family: Stratum2 Bold;\n    font-size: 13px;",
                      "    margin-top: 8px;\n    font-family: Stratum2 Bold;\n    font-size: 20px;")

    # Ladders, recomputed.
    css = re.sub(r"(\.x-0 \{.*?\n)(?:\.x-\d+ \{[^\n]*\n)*", lambda _: ladder("x", "", "X", lambda n: n * cell + cell / 2 - BIG_BOX / 2) + "\n", css, flags=re.S)
    css = re.sub(r"(\.y-0 \{.*?\n)(?:\.y-\d+ \{[^\n]*\n)*", lambda _: ladder("y", "", "Y", lambda n: n * cell + cell / 2 - BIG_BOX / 2) + "\n", css, flags=re.S)
    css = re.sub(r"(\.mm-pan\.px-0 \{.*?\n)(?:\.mm-pan\.px-\d+ \{[^\n]*\n)*", lambda _: ladder("px", ".mm-pan", "X", lambda n: BIG_MAP / 2 - (n * cell + cell / 2)) + "\n", css, flags=re.S)
    css = re.sub(r"(\.mm-pan-y\.py-0 \{.*?\n)(?:\.mm-pan-y\.py-\d+ \{[^\n]*\n)*", lambda _: ladder("py", ".mm-pan-y", "Y", lambda n: BIG_MAP / 2 - (n * cell + cell / 2)) + "\n", css, flags=re.S)

    TARGET.write_text(HEADER + "\n" + css.rstrip("\n") + "\n", encoding="utf-8")
    print(f"wrote {TARGET}")


if __name__ == "__main__":
    main()
