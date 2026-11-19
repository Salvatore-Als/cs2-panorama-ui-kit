"""
Writes the overlay effects: the EFFECTS block of uikit_overlay.css and the textures they use.

    python tools/overlay_fx.py

Writes uikit_overlay_fx.css (its own stylesheet, so the overlay does not depend on it) and the textures.

An effect is a full-screen look (frozen, burning, poisoned...) drawn by the overlay layout's one effect panel, ov_fx:
the kit puts fx-<name> on it (IOverlayService.Show), the stylesheet does the rest. ov_fx holds a fixed pool -
LAYERS full-screen panels, PARTICLES small ones, a title and a text label - and each effect styles the part of the
pool it uses; everything else stays collapsed.

The effects were picked from the overlay lab page (one HTML proposal each); this file is their Panorama translation:
gradients as gradient( radial | linear ), inner glows as box-shadow inset, tints as a colour panel with
-s2-mix-blend-mode, blur as world-blur, patterns as SVG textures (written to images/custom_game/uikit/overlay/fx/,
compiled to .vsvg_c by Workshop Tools). Never edit the block or the textures by hand: edit EFFECTS below, run this.

Sizes assume Panorama's 1920 x 1080 reference: 1 cqw of the lab page = 19.2px.
"""

from __future__ import annotations

import math
import random
from dataclasses import dataclass, field, replace
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent / "workshop" / "panorama"
STYLESHEET = ROOT / "styles" / "custom_game" / "uikit_overlay_fx.css"
"""Its own stylesheet, included after uikit_overlay.css: a rule Panorama refuses here cannot take the overlay down with it."""
BASE = ROOT / "styles" / "custom_game" / "uikit_overlay.css"
TEXTURES = ROOT / "images" / "custom_game" / "uikit" / "overlay" / "fx"
TEXTURE_URL = "s2r://panorama/images/custom_game/uikit/overlay/fx"
CSHARP = Path(__file__).resolve().parent.parent / "src" / "PanoramaUiKit.Shared" / "Overlays" / "OverlayEffects.cs"

LAYERS = 8
PARTICLES = 40
INTENSITY = 0.9
W, H = 1920, 1080
CQW = W / 100
BEGIN, END = "/* EFFECTS:BEGIN */", "/* EFFECTS:END */"


def clear(c: str) -> str:
    return c[:7] + "00"


def ascii_only(text: str) -> str:
    import unicodedata
    return unicodedata.normalize("NFKD", text).encode("ascii", "ignore").decode("ascii")


def num(v: float) -> str:
    return f"{v:.3f}".rstrip("0").rstrip(".")


# Layers ---------------------------------------------------------------------------------------------

@dataclass
class Layer:
    css: list[str]
    opacity: float = 1.0
    anim: tuple | None = None

    def a(self, name: str, dur: float, timing: str = "ease-in-out", delay: float = 0.0) -> "Layer":
        return replace(self, anim=(name, dur, timing, delay))


@dataclass
class Particles:
    n: int
    color: str
    size: float
    anim: str
    dur: float
    area: str = "all"
    sx: float | None = None


@dataclass
class Title:
    css: list[str]
    anim: tuple | None = None


@dataclass
class Effect:
    name: str
    doc: str
    layers: list = field(default_factory=list)


def radial(c: str, a: float = 35, b: float = 100, at=(50, 50)) -> Layer:
    """Clear in the middle, colour at the edges: a vignette."""
    return Layer([f"background-color: gradient( radial, {at[0]}% {at[1]}%, 0% 0%, 71% 71%, from( {clear(c)} ), "
                  f"color-stop( {num(a / 100)}, {clear(c)} ), to( {c} ) );"])


def spot(c: str, stop: float = 30, at=(50, 50), rx: float = 71, ry: float = 71) -> Layer:
    """Colour in the middle, clear at the edges."""
    return Layer([f"background-color: gradient( radial, {at[0]}% {at[1]}%, 0% 0%, {rx}% {ry}%, from( {c} ), "
                  f"color-stop( {num(stop / 100)}, {clear(c)} ), to( {clear(c)} ) );"])


def band(c: str, a: float, mid: float, b: float) -> Layer:
    """A ring: clear, colour at mid, clear."""
    return Layer([f"background-color: gradient( radial, 50% 50%, 0% 0%, 71% 71%, from( {clear(c)} ), color-stop( {num(a / 100)}, {clear(c)} ), "
                  f"color-stop( {num(mid / 100)}, {c} ), color-stop( {num(b / 100)}, {clear(c)} ), to( {clear(c)} ) );"])


SIDES = {"to top": ("0% 100%", "0% 0%"), "to bottom": ("0% 0%", "0% 100%"), "to right": ("0% 0%", "100% 0%"), "to left": ("100% 0%", "0% 0%")}


def edge(direction: str, c: str, stop: float = 35) -> Layer:
    start, end = SIDES[direction]
    return Layer([f"background-color: gradient( linear, {start}, {end}, from( {c} ), color-stop( {num(stop / 100)}, {clear(c)} ), to( {clear(c)} ) );"])


def inset(c: str, blur: float = 6, spread: float = 1) -> Layer:
    return Layer([f"box-shadow: inset {c} 0px 0px {round(blur * CQW)}px {round(spread * CQW)}px;"])


def fill(c: str, blend: str = "normal", o: float = 1) -> Layer:
    css = [f"background-color: {c};"]
    if blend != "normal":
        css.append(f"-s2-mix-blend-mode: {blend};")
    return Layer(css, o)


def blur(cqw: float) -> Layer:
    std = min(10, round(cqw * 20))
    return Layer([f"world-blur: gaussian( {std}.0 );"])


def texture(name: str, o: float = 1) -> Layer:
    return Layer([f'background-image: url( "{TEXTURE_URL}/{name}.vsvg" );', "background-size: 100% 100%;", "background-repeat: no-repeat;"], o)


def corners(c: str, r: float = 40) -> list[Layer]:
    diag = math.hypot(W, H)
    rx, ry = round(r / 100 * diag / W * 100), round(r / 100 * diag / H * 100)
    return [Layer([f"background-color: gradient( radial, {x}% {y}%, 0% 0%, {rx}% {ry}%, from( {c} ), to( {clear(c)} ) );"])
            for x, y in ((0, 0), (100, 0), (0, 100), (100, 100))]


def ring(c: str, dur: float = 1.6) -> Layer:
    return band(c, 52, 56, 61).a("ring", dur, "ease-out")


# Textures (SVG, 1920 x 1080 viewBox stretched over the screen) ---------------------------------------

def svg(body: str) -> str:
    return f"<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 160 90' preserveAspectRatio='none'>{body}</svg>\n"


def tex_shards(r: random.Random, c: str) -> str:
    p = ""
    for i in range(22):
        side, t, w, h = i % 4, r.random(), 4 + r.random() * 9, 8 + r.random() * 22
        if side == 0:
            x = t * 160; pts = [(x, 0), (x + w, 0), (x + w / 2 + (r.random() - .5) * 6, h)]
        elif side == 1:
            x = t * 160; pts = [(x, 90), (x + w, 90), (x + w / 2, 90 - h)]
        elif side == 2:
            y = t * 90; pts = [(0, y), (0, y + w), (h * 1.3, y + w / 2)]
        else:
            y = t * 90; pts = [(160, y), (160, y + w), (160 - h * 1.3, y + w / 2)]
        p += f"<polygon points='{' '.join(f'{a:.1f},{b:.1f}' for a, b in pts)}' fill='{c}' fill-opacity='{.35 + r.random() * .45:.2f}'/>"
    return svg(p)


def tex_icicles(r: random.Random, c: str) -> str:
    p, x = "", 0.0
    while x < 160:
        w, h = 3 + r.random() * 6, 6 + r.random() * 20
        p += f"<polygon points='{x:.1f},0 {x + w:.1f},0 {x + w / 2:.1f},{h:.1f}' fill='{c}' fill-opacity='{.5 + r.random() * .4:.2f}'/>"
        x += w * (.7 + r.random() * .5)
    return svg(p)


def tex_cracks(r: random.Random, c: str) -> str:
    cx, cy, d = 40 + r.random() * 80, 22 + r.random() * 46, ""
    for i in range(11):
        ang = i / 11 * math.tau + r.random() * .4
        x, y = cx, cy
        d += f"M{cx:.1f} {cy:.1f}"
        for _ in range(6):
            ln = 6 + r.random() * 12
            x += math.cos(ang + (r.random() - .5) * .9) * ln
            y += math.sin(ang + (r.random() - .5) * .9) * ln * .7
            d += f" L{x:.1f} {y:.1f}"
    return svg(f"<path d='{d}' fill='none' stroke='{c}' stroke-width='.45' stroke-opacity='.85'/><circle cx='{cx:.1f}' cy='{cy:.1f}' r='2.2' fill='{c}' fill-opacity='.5'/>")


def tex_frost(r: random.Random, c: str) -> str:
    d = ""
    for ox, oy, dx, dy in ((0, 0, 1, 1), (160, 0, -1, 1), (0, 90, 1, -1), (160, 90, -1, -1)):
        for _ in range(26):
            dist, a = r.random() * 34, r.random() * math.pi / 2
            x, y = ox + dx * math.cos(a) * dist * 1.4, oy + dy * math.sin(a) * dist
            ln, b = 2 + r.random() * 5, r.random() * math.pi
            for k in range(3):
                g = b + k * math.pi / 3
                d += f"M{x - math.cos(g) * ln:.1f} {y - math.sin(g) * ln:.1f} L{x + math.cos(g) * ln:.1f} {y + math.sin(g) * ln:.1f}"
    return svg(f"<path d='{d}' stroke='{c}' stroke-width='.35' stroke-opacity='.9'/>")


def tex_noise(r: random.Random, c: str, n: int = 700) -> str:
    return svg("".join(f"<rect x='{r.random() * 160:.1f}' y='{r.random() * 90:.1f}' width='.7' height='.7' fill='{c}' fill-opacity='{r.random() * .9:.2f}'/>" for _ in range(n)))


def tex_hex(_: random.Random, c: str) -> str:
    s, h, d = 6, 6 * math.sqrt(3) / 2, ""
    for row in range(12):
        for col in range(18):
            cx, cy = col * s * 1.5, row * h * 2 + (h if col % 2 else 0)
            d += "M" + " L".join(f"{cx + s * .9 * math.cos(math.pi / 3 * k):.1f} {cy + s * .9 * math.sin(math.pi / 3 * k):.1f}" for k in range(6)) + " Z"
    return svg(f"<path d='{d}' fill='none' stroke='{c}' stroke-width='.3'/>")


def tex_scan(_: random.Random, c: str) -> str:
    return svg("".join(f"<rect x='0' y='{y * .9:.1f}' width='160' height='.25' fill='{c}'/>" for y in range(100)))


def tex_bubbles(r: random.Random, c: str) -> str:
    return svg("".join(f"<circle cx='{r.random() * 160:.1f}' cy='{(r.random() * 22 if r.random() < .5 else 68 + r.random() * 22):.1f}' r='{1.5 + r.random() * 3.5:.1f}' fill='none' stroke='{c}' stroke-width='.6'/>" for _ in range(11)))


def tex_drips(r: random.Random, c: str) -> str:
    p = "<defs><linearGradient id='g' x1='0' y1='0' x2='0' y2='1'>" \
        f"<stop offset='0' stop-color='{c}'/><stop offset='.7' stop-color='{c}' stop-opacity='.8'/><stop offset='1' stop-color='{c}' stop-opacity='0'/></linearGradient></defs>"
    for _ in range(7):
        x, w, h = 4 + r.random() * 150, .6 + r.random() * .9, 16 + r.random() * 40
        p += f"<rect x='{x:.1f}' y='0' width='{w:.2f}' height='{h:.1f}' rx='{w / 2:.2f}' fill='url(#g)'/>"
    return svg(p)


TEXTURE_KINDS = {"shards": tex_shards, "icicles": tex_icicles, "cracks": tex_cracks, "frost": tex_frost, "noise": tex_noise,
                 "hex": tex_hex, "scan": tex_scan, "bubbles": tex_bubbles, "drips": tex_drips}

TEXTURE_SPECS = {
    "shards_ice": ("shards", "#e6f7ff"), "icicles": ("icicles", "#e3f5ff"), "cracks_ice": ("cracks", "#e8f8ff"),
    "frost": ("frost", "#ffffff"), "noise_soot": ("noise", "#000000"), "noise_lime": ("noise", "#e8ff7a"),
    "noise_green": ("noise", "#c8ffb8"), "hex_blue": ("hex", "#6fb2ff"), "scan_orange": ("scan", "#ff7a1f"),
    "scan_cyan": ("scan", "#7ae8ff"), "scan_green": ("scan", "#0a3d0a"), "bubbles_hot": ("bubbles", "#ffb066"),
    "drips_blood": ("drips", "#9a0012"),
}


# The effects ------------------------------------------------------------------------------------------

def title(color: str, glow: str, anim: tuple | None = None, size: int = 144, top: int | None = None) -> Title:
    css = [f"color: {color};", f"font-size: {size}px;", f"text-shadow: 0px 0px 24px 2.0 {glow};"]
    if top is not None:
        css += ["vertical-align: top;", f"margin-top: {top}px;"]
    return Title(css, anim)


EFFECTS: list[Effect] = [
    # Frozen.
    Effect("frozen1", "Givre aux coins.", corners("#dff4ffd9", 42)),
    Effect("frozen2", "Cadre de glace : bord intérieur bleu glace et liseré blanc.", [inset("#bfe9ffe6", 8, 1.3), inset("#ffffffaa", 1.5, .2)]),
    Effect("frozen3", "Éclats de glace plantés sur les bords.", [texture("shards_ice"), radial("#7fc3ff55", 50)]),
    Effect("frozen4", "Buée : souffle blanc qui monte du bas et respire.", [spot("#ffffffbb", 55, (50, 100), 71, 141).a("pulse", 3.2)]),
    Effect("frozen5", "Froid : monde bleuté, bords bleu nuit.", [fill("#5aa9ff", "multiply", .55), radial("#0a2a55cc", 45)]),
    Effect("frozen6", "Givre qui gagne : avance des coins vers le centre.", [*(l.a("creep", 6) for l in corners("#e8f8ffe0", 46)), texture("frost")]),
    Effect("frozen7", "Neige qui tombe devant la vue.", [Particles(34, "#ffffff", .5, "fall", 5)]),
    Effect("frozen8", "Vitre gelée : monde flou derrière un verre blanchi.", [blur(.45), fill("#e6f6ff", "normal", .35)]),
    Effect("frozen9", "Ralenti : bandes bleu nuit, monde bleuté.", [edge("to bottom", "#0b2440f2", 12), edge("to top", "#0b2440f2", 12), fill("#4f8fd9", "multiply", .4)]),
    Effect("frozen10", "Coup de froid : flash bleu-blanc puis givre.", [fill("#d8f1ff", "normal", .9).a("flash", 2.4, "ease-out"), *corners("#dff4ffcc", 38)]),
    Effect("frozen11", "Hypothermie : pulsation bleue lente.", [radial("#1d5fa8d9", 30, 95).a("throb", 2.8)]),
    Effect("frozen12", "Stalactites qui pendent du haut.", [texture("icicles"), edge("to bottom", "#9fd2ff55", 25)]),
    Effect("frozen13", "Craquelures de glace devant les yeux.", [texture("cracks_ice"), fill("#9fd2ff", "multiply", .3)]),
    Effect("frozen14", "Aurore : voile vert-cyan qui ondule en haut.", [Layer(["background-color: gradient( linear, 0% 0%, 12% 100%, from( #38f5c066 ), color-stop( 0.3, #3a7bff44 ), color-stop( 0.55, #3a7bff00 ), to( #3a7bff00 ) );"]).a("drift", 8), radial("#04162acc", 45)]),
    Effect("frozen15", "Engourdi : vignette bleu nuit lourde.", [radial("#06213ff2", 22, 80)]),
    Effect("frozen16", "Scintillement : paillettes de glace.", [Particles(24, "#ffffff", .45, "twinkle", 2.2), *corners("#cfeeffaa", 36)]),
    Effect("frozen17", "Banquise : glace qui monte du bas.", [edge("to top", "#d9f2ffd9", 22), texture("frost", .6)]),
    Effect("frozen18", "Blizzard : écran blanchi, neige rapide.", [fill("#f2f8ff", "normal", .55).a("pulse", 2.5), Particles(40, "#ffffff", .4, "fall", 1.6, sx=-14)]),
    Effect("frozen19", "Cryo néon : contour bleu glace qui pulse.", [inset("#4fd8ff", 4, .55).a("pulse", 1.4)]),
    Effect("frozen20", "Gelé : teinte bleue, givre et glace aux coins.", [fill("#9fd6ff", "multiply", .5), texture("frost"), *corners("#e8f8ffcc", 40)]),
    # Burning.
    Effect("burn1", "Flammes qui lèchent le bas de l'écran.", [edge("to top", "#ff5a00e6", 32).a("flicker", .9), edge("to top", "#ffc93c99", 16).a("flicker", .7, "ease-in-out", .3)]),
    Effect("burn2", "Bord incandescent.", [inset("#ff6a00", 6, .9).a("pulse", 1.2)]),
    Effect("burn3", "Braises qui montent et s'éteignent.", [Particles(28, "#ffb238", .55, "spark", 2.6, area="bottom"), edge("to top", "#ff4a0088", 22)]),
    Effect("burn4", "Chaleur : monde flou et orangé.", [blur(.25).a("pulse", 1.8), fill("#ff7a1f", "overlay", .35)]),
    Effect("burn5", "Fumée grise qui passe.", [spot("#2a2a2acc", 50, (30, 80)).a("drift", 6), spot("#333333cc", 45, (75, 70)).a("drift", 6), spot("#2a2a2a99", 45, (60, 30)).a("drift", 8, "ease-in-out", -3)]),
    Effect("burn6", "Bords carbonisés, liseré rougeoyant.", [*corners("#120804f2", 40), band("#ff6a1a77", 58, 68, 80).a("flicker", 1.2)]),
    Effect("burn7", "Embrasement : flash orange puis voile rouge.", [fill("#ffb347", "normal", .9).a("flash", 2, "ease-out"), radial("#ff3a00aa", 35)]),
    Effect("burn8", "Cendres qui tombent lentement.", [Particles(30, "#a3a3a3", .45, "fall", 7), fill("#5a4a3a", "multiply", .25)]),
    Effect("burn9", "Feu au cœur : pulsation rouge-orange rapide.", [radial("#d01800d9", 30, 95).a("throb", 1.2)]),
    Effect("burn10", "Calciné : monde brun-noir, grain de suie.", [fill("#3a1d0a", "multiply", .6), texture("noise_soot", .6), radial("#0d0400e6", 40)]),
    Effect("burn11", "Les deux côtés flambent.", [edge("to right", "#ff5500cc", 20).a("flicker", .8), edge("to left", "#ff5500cc", 20).a("flicker", .8, "ease-in-out", .4)]),
    Effect("burn12", "Lueur orange additive.", [fill("#ff7b1f", "screen", .35).a("pulse", 1.5)]),
    Effect("burn13", "Napalm : monde orangé, feu jaune au sol.", [fill("#ff8c00", "multiply", .65), edge("to top", "#ffd24ad9", 40).a("flicker", .6)]),
    Effect("burn14", "Étincelles vives et rapides.", [Particles(20, "#fff0a0", .35, "spark", 1.1)]),
    Effect("burn15", "Fumée noire qui gonfle.", [radial("#0a0806f2", 18, 80).a("breathe", 4)]),
    Effect("burn16", "Cloques : bulles de chaleur sur les bords.", [texture("bubbles_hot"), radial("#5a1a0099", 50)]),
    Effect("burn17", "Ligne de feu : deux fronts qui se croisent en bas.", [edge("to top", "#ff6a00d9", 26).a("drift", 1.4), edge("to top", "#ffb000aa", 18).a("drift", 1.1, "ease-in-out", -.5)]),
    Effect("burn18", "Surchauffe : balayage orange et bord chaud.", [texture("scan_orange", .25).a("flicker", 1.4), inset("#ff6a00aa", 4, .5)]),
    Effect("burn19", "Brûlure lente : halo ambré qui respire.", [radial("#c4520ad9", 35).a("breathe", 3)]),
    Effect("burn20", "En feu : flammes en bas et bord incandescent.", [edge("to top", "#ff5a00e6", 34).a("flicker", .8), inset("#ff6a00", 4, .5)]),
    # Zombie.
    Effect("zombie1", "Décomposé : monde sépia délavé, bords sombres.", [fill("#b08a5a", "multiply", .55), radial("#20160ad9", 40)]),
    Effect("zombie2", "Rage : rouge sang qui scintille, bords noirs.", [fill("#7a0006", "multiply", .7).a("flicker", .5, "linear"), radial("#000000e6", 35, 95)]),
    Effect("zombie3", "Fièvre jaune : monde jauni, bords bruns.", [fill("#c9b21e", "multiply", .55), radial("#3b3300cc", 40)]),
    # Knocked.
    Effect("knock1", "Ralenti dramatique, à terre : bandes noires, monde assombri, battement lent.", [edge("to bottom", "#000000f5", 14), edge("to top", "#000000f5", 14), fill("#3a0a0a", "multiply", .5), radial("#5a0008aa", 40).a("throb", 2.4)]),
    # Other states.
    Effect("poison", "Teinte violette et pulsation nauséeuse.", [fill("#7a2fb8", "multiply", .5), radial("#2a0a3dd9", 35).a("throb", 1.8)]),
    Effect("electrocuted", "Flashs blancs et bord bleu qui grésille.", [fill("#dff4ff", "normal", .9).a("bolt", 1.2, "linear"), inset("#7ad7ff", 3, .4).a("flicker", .3, "linear")]),
    Effect("flashbang", "Aveuglé : écran blanc qui revient lentement.", [fill("#ffffff", "normal", .95).a("flash", 3.5, "ease-out")]),
    Effect("bleeding", "Saignement : coulures et vignette qui bat.", [texture("drips_blood").a("drip", 3.2, "ease-in"), radial("#8a000cb3", 40).a("throb", 1.4)]),
    Effect("dizzy", "Étourdi : vignette qui tangue, monde flou.", [radial("#000000cc", 40).a("sway", 3), blur(.2)]),
    Effect("radiation", "Teinte acide et grain qui grésille.", [fill("#c8ff1a", "multiply", .4), texture("noise_lime", .6).a("flicker", .25, "linear"), radial("#1a2a00cc", 40)]),
    Effect("adrenaline", "Bord doré qui bat vite.", [inset("#ffd60a", 5, .6).a("throb", .7), radial("#ffb80055", 55)]),
    Effect("stealth", "Furtif : monde teinté cyan, balayage discret.", [fill("#3ad1ff", "overlay", .3), band("#7ae8ff55", 45, 50, 55).a("scan", 3, "linear"), texture("scan_cyan", .15)]),
    Effect("shield", "Bouclier : nid d'abeille bleu et bord.", [inset("#3a8bff", 3.5, .45), texture("hex_blue", .35)]),
    Effect("heal", "Soin : halo vert, particules qui montent.", [radial("#2ecc71b3", 40).a("pulse", 1.6), Particles(14, "#7dffb0", .7, "rise", 2.8)]),
    Effect("slowed", "Ralenti : monde bleu-violet, vignette lente.", [fill("#2d3a8c", "multiply", .4), radial("#000a1ee6", 30, 95).a("breathe", 4)]),
    Effect("haste", "Accéléré : traînées blanches sur les côtés.", [edge("to right", "#ffffff55", 9).a("drift", .4, "linear"), edge("to left", "#ffffff55", 9).a("drift", .4, "linear", -.2), radial("#00000077", 50)]),
    Effect("stunned", "Assommé : blanc qui palpite, une onde au centre.", [fill("#ffffff", "normal", .7).a("pulse", .9), ring("#ffffffcc", 1.2)]),
    Effect("smoke", "Fumigène : écran voilé gris, volutes.", [fill("#9aa3a8", "normal", .8).a("pulse", 4), Particles(10, "#c3c9cc", 7, "floaty", 8)]),
    Effect("nausea", "Nausée : teinte verdâtre, vignette qui tangue.", [fill("#9acd32", "overlay", .4), radial("#00000088", 40).a("sway", 2.4)]),
    Effect("thermal", "Thermique : couleurs décalées vers l'orange et le bleu.", [fill("#ff4d00", "hue", .7), fill("#0026ff", "multiply", .3)]),
    Effect("nightvision", "Vision nocturne : vert lumineux, grain et lignes.", [fill("#39ff14", "multiply", .6), texture("scan_green", .2), texture("noise_green", .35), radial("#001400e6", 40)]),
    Effect("concussion", "Choc sourd : onde blanche et flou.", [ring("#ffffffcc", 1.4), blur(.2).a("pulse", 1.2)]),
    Effect("spotted", "Repéré : bord rouge qui clignote.", [inset("#ff2d2d", 3, .35).a("tick", 1, "ease-out")]),
    Effect("divine", "Protection divine : halo doré, poussière qui monte.", [radial("#ffd86bb3", 45).a("pulse", 2), Particles(18, "#fff3b0", .5, "rise", 4)]),
    # Custom: a full-screen picture your addon ships (a logo, a splash), nothing to change in the kit.
    *(Effect(f"custom{n}", f"Effet perso {n} : l'image panorama/images/custom_game/uikit/overlay/custom/custom_{n}.png (1920 x 1080, transparente "
             f"autour de ce qu'elle montre) + custom_{n}_png.vtex, livrées par ton addon, sur tout l'écran. Rien tant qu'elle n'existe pas.",
             [Layer([f'background-image: url( "s2r://panorama/images/custom_game/uikit/overlay/custom/custom_{n}_png.vtex" );',
                     "background-size: 100% 100%;", "background-repeat: no-repeat;"], 1 / INTENSITY)]) for n in range(1, 9)),
    # Check: one plain cyan screen from this stylesheet (its twin debug1 is in uikit_overlay.css).
    Effect("debug2", "Témoin : écran cyan uni, écrit dans uikit_overlay_fx.css. S'il ne s'affiche pas mais que debug1 oui, cette feuille ne charge pas.",
           [Layer(["background-color: #00e5ff;"], .5)]),
]


# Keyframes: opacity ones are written for the layer's own opacity, so one name per (animation, opacity). -----

OPACITY = {
    "pulse": [((0, 100), .45), ((50,), 1)],
    "throb": [((0, 45, 100), .35), ((9,), 1), ((18,), .55), ((27,), .95)],
    "flicker": [((0, 51, 100), 1), ((12,), .45), ((22,), .9), ((37,), .55), ((66,), .35), ((81,), .85)],
    "flash": [((0,), 1), ((22, 100), 0)],
    "bolt": [((0, 5, 9, 13, 100), 0), ((2, 7, 11), 1)],
    "tick": [((0,), 1), ((35, 100), .15)],
}

TRANSFORM = {
    "breathe": [((0, 100), "scale3d( 1, 1, 1 )"), ((50,), "scale3d( 1.09, 1.09, 1 )")],
    "creep": [((0, 100), "scale3d( 1.4, 1.4, 1 )"), ((50,), "scale3d( 1, 1, 1 )")],
    "drift": [((0, 100), "translate3d( -77px, 0px, 0px )"), ((50,), "translate3d( 77px, 0px, 0px )")],
    "shake": [((0, 100), "translate3d( 0px, 0px, 0px )"), ((20,), "translate3d( -23px, 9px, 0px )"), ((40,), "translate3d( 19px, -11px, 0px )"),
              ((60,), "translate3d( -15px, -6px, 0px )"), ((80,), "translate3d( 17px, 10px, 0px )")],
    "sway": [((0, 100), "rotateZ( -2.5deg ) scale3d( 1.08, 1.08, 1 )"), ((50,), "rotateZ( 2.5deg ) scale3d( 1.08, 1.08, 1 )")],
    "scan": [((0,), "translate3d( 0px, -1080px, 0px )"), ((100,), "translate3d( 0px, 1080px, 0px )")],
    "drip": [((0,), "scale3d( 1, 0, 1 )"), ((70, 100), "scale3d( 1, 1, 1 )")],
}


def letters(n: int) -> str:
    """A number as letters for an animation name: Panorama names are kept to plain words (90 -> 'jA')."""
    digits = "ABCDEFGHIJ"
    return "".join(digits[int(d)] for d in str(n))


class Keyframes:
    def __init__(self) -> None:
        self.rules: dict[str, str] = {}

    def add(self, name: str, steps: list[tuple[tuple, str]]) -> str:
        if name not in self.rules:
            # One block per step: Panorama refuses "0%, 100% { }" (Invalid keyframe selector).
            ordered = sorted(((p, css) for at, css in steps for p in at), key=lambda step: step[0])
            body = "\n".join(f"    {p}% {{ {css} }}" for p, css in ordered)
            self.rules[name] = f"@keyframes '{name}' {{\n{body}\n}}"
        return name

    def layer(self, anim: str, opacity: float) -> str:
        if anim in OPACITY:
            o = round(opacity * 100)
            return self.add(f"fx{anim}{letters(o)}", [(at, f"opacity: {num(opacity * f)};") for at, f in OPACITY[anim]])
        if anim == "ring":
            o = round(opacity * 100)
            return self.add(f"fxring{letters(o)}", [((0,), f"transform: scale3d( 0.12, 0.12, 1 ); opacity: {num(opacity)};"), ((100,), "transform: scale3d( 1.7, 1.7, 1 ); opacity: 0;")])
        return self.add(f"fx{anim}", [(at, f"transform: {css};") for at, css in TRANSFORM[anim]])

    def particle(self, anim: str, opacity: float, sx: int) -> str:
        o, op = round(opacity * 100), num(opacity)
        name = f"fxp{anim}{letters(o)}{'left' if sx < 0 else 'right' if sx > 0 else 'mid'}{letters(abs(sx))}"
        if anim == "rise":
            return self.add(name, [((0,), "transform: translate3d( 0px, 0px, 0px ); opacity: 0;"), ((15,), f"opacity: {op};"), ((100,), "transform: translate3d( 0px, -1140px, 0px ); opacity: 0;")])
        if anim == "fall":
            return self.add(name, [((0,), "transform: translate3d( 0px, -130px, 0px ); opacity: 0;"), ((10,), f"opacity: {op};"),
                                   ((100,), f"transform: translate3d( {sx}px, 1170px, 0px ); opacity: {num(opacity * .6)};")])
        if anim == "spark":
            return self.add(name, [((0,), f"transform: translate3d( 0px, 0px, 0px ); opacity: {op};"), ((100,), f"transform: translate3d( {sx}px, -370px, 0px ); opacity: 0;")])
        if anim == "floaty":
            return self.add(name, [((0, 100), f"transform: translate3d( 0px, 0px, 0px ); opacity: {op};"), ((50,), f"transform: translate3d( {sx}px, -54px, 0px ); opacity: {op};")])
        if anim == "twinkle":
            return self.add(name, [((0, 100), "transform: scale3d( 0.3, 0.3, 1 ); opacity: 0;"), ((50,), f"transform: scale3d( 1, 1, 1 ); opacity: {op};")])
        raise ValueError(anim)


def animation(name: str, dur: float, timing: str, delay: float) -> list[str]:
    css = [f"animation-name: {name};", f"animation-duration: {num(dur)}s;", f"animation-timing-function: {timing};", "animation-iteration-count: infinite;"]
    if delay:
        # Positive only: a negative delay is not used anywhere in Panorama's own stylesheets.
        css.append(f"animation-delay: {num(abs(delay))}s;")
    return css


def render(effects: list[Effect]) -> str:
    kf = Keyframes()
    out = [BEGIN, "/* Written by tools/overlay_fx.py. Do not edit: edit EFFECTS there and run it. */", ""]
    for fx in effects:
        sel = f".ov-fx.fx-{fx.name}"
        # Plain ASCII in the stylesheet: the docs are French, the compiler gets no accents.
        out.append(f"/* {fx.name}: {ascii_only(fx.doc)} */")
        n = 0
        for part in fx.layers:
            if isinstance(part, Layer):
                if n >= LAYERS:
                    raise SystemExit(f"{fx.name}: more than {LAYERS} layers")
                opacity = part.opacity * INTENSITY
                css = ["visibility: visible;", *part.css, f"opacity: {num(opacity)};"]
                if part.anim:
                    name, dur, timing, delay = part.anim
                    css += animation(kf.layer(name, opacity), dur, timing, delay)
                    if name == "drip":
                        css.append("transform-origin: 50% 0%;")
                out.append(f"{sel} .ov-fx-l{n} {{ {' '.join(css)} }}")
                n += 1
            elif isinstance(part, Particles):
                r = random.Random(fx.name)
                opacity = INTENSITY
                for i in range(min(part.n, PARTICLES)):
                    x = r.random() * 100
                    y = (70 + r.random() * 30 if part.area == "bottom" else -5 + r.random() * 10 if part.anim == "fall"
                         else 75 + r.random() * 30 if part.anim == "rise" else r.random() * 100)
                    size = round(part.size * (.6 + r.random() * .8) * CQW)
                    dur = part.dur * (.7 + r.random() * .6)
                    sx = round(part.sx * CQW) if part.sx is not None else (-96, 0, 96)[i % 3]
                    css = ["visibility: visible;", f"x: {round(x / 100 * W)}px;", f"y: {round(y / 100 * H)}px;", f"width: {size}px;", f"height: {size}px;",
                           f"border-radius: {size}px;", f"background-color: {part.color};"]
                    if part.size > 2:
                        css.append(f"blur: gaussian( {min(10, round(size * .1))}.0 );")
                    css += animation(kf.particle(part.anim, opacity, sx), dur, "linear", round(r.random() * dur, 2))
                    out.append(f"{sel} .ov-fx-p.p{i} {{ {' '.join(css)} }}")
            elif isinstance(part, Title):
                css = list(part.css)
                if part.anim:
                    name, dur, timing, delay = part.anim
                    css += animation(kf.layer(name, 1), dur, timing, delay)
                out.append(f"{sel} .ov-fx-title {{ {' '.join(css)} }}")
        out.append("")
    out += list(kf.rules.values())
    out.append(END)
    return "\n".join(out)


def pascal(name: str) -> str:
    head = name.rstrip("0123456789")
    return head[:1].upper() + head[1:] + name[len(head):]


def csharp(effects: list[Effect]) -> str:
    consts = "\n\n".join(f"    /// <summary>{fx.doc}</summary>\n    public const string {pascal(fx.name)} = \"{fx.name}\";" for fx in effects)
    names = ", ".join(pascal(fx.name) for fx in effects if not fx.name.startswith(("custom", "debug")))
    return f"""// Written by tools/overlay_fx.py from its EFFECTS list. Do not edit: edit the list and run it.
namespace PanoramaUiKit.Shared.Overlays;

/// <summary>
/// The full-screen effects the bundled overlay stylesheet has a rule for: <c>fx-&lt;name&gt;</c> on <c>ov_fx</c>
/// (<see cref="IOverlayService.Show"/>). A custom layout can add its own names.
/// </summary>
public static class OverlayEffects
{{
{consts}

    /// <summary>Témoin : écran magenta uni, écrit dans uikit_overlay.css (son jumeau <see cref="Debug2"/> est dans uikit_overlay_fx.css).</summary>
    public const string Debug1 = "debug1";

    /// <summary>Témoin : ov_fx lui-même en jaune, sans ses enfants, dans uikit_overlay.css.</summary>
    public const string Debug3 = "debug3";

    /// <summary>Témoin : la boîte des particules en vert (jamais masquée), dans uikit_overlay.css.</summary>
    public const string Debug4 = "debug4";

    /// <summary>Effet perso <paramref name="n"/> (1 à 8) : <c>custom&lt;n&gt;</c>, l'image <c>uikit/overlay/custom/custom_&lt;n&gt;.png</c> de ton addon.</summary>
    public static string Custom(int n) => $"custom{{n}}";

    /// <summary>The bundled looks, without the custom slots and the checks.</summary>
    public static readonly IReadOnlyList<string> All = [{names}];
}}
"""


def main() -> None:
    TEXTURES.mkdir(parents=True, exist_ok=True)
    for name, (kind, color) in TEXTURE_SPECS.items():
        (TEXTURES / f"{name}.svg").write_text(TEXTURE_KINDS[kind](random.Random(name), color), encoding="utf-8")

    header = ("/*\n    uikit_overlay_fx - the overlay's full-screen effects (fx-<name> on ov_fx). Contract \"overlay\".\n"
              "    GENERATED by tools/overlay_fx.py: edit EFFECTS there and run it. Included by uikit_overlay.xml after\n"
              "    uikit_overlay.css, which holds the pool's base rules.\n*/\n\n")
    STYLESHEET.write_text(header + render(EFFECTS) + "\n", encoding="utf-8")

    base = BASE.read_text(encoding="utf-8")
    if BEGIN in base:
        base = base[:base.index(BEGIN)].rstrip("\n") + "\n" + base[base.index(END) + len(END):].lstrip("\n")
        BASE.write_text(base, encoding="utf-8")
    CSHARP.write_text(csharp(EFFECTS), encoding="utf-8")
    print(f"{len(EFFECTS)} effects, {len(TEXTURE_SPECS)} textures -> {STYLESHEET.name}")


if __name__ == "__main__":
    main()
