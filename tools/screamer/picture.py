"""Builds a screamer picture: the source fitted on a black canvas of the screen size."""

from __future__ import annotations

from dataclasses import dataclass
from pathlib import Path

from PIL import Image

DEFAULT_SIZE: tuple[int, int] = (1920, 1080)
BACKGROUND: tuple[int, int, int] = (0, 0, 0)


@dataclass(frozen=True)
class Placement:
    """Where the source lands on the canvas, and at which size."""

    width: int
    height: int
    left: int
    top: int


def place(source_size: tuple[int, int], canvas_size: tuple[int, int]) -> Placement:
    """
    Fits the source inside the canvas, keeping its aspect ratio: scaled up or down until it
    touches the canvas edges (the full width for a picture wider than the screen ratio, the full
    height otherwise). Black fills the rest. The result is always centered.
    """
    source_width, source_height = source_size
    canvas_width, canvas_height = canvas_size
    if source_width <= 0 or source_height <= 0:
        raise ValueError("source picture is empty")
    if canvas_width <= 0 or canvas_height <= 0:
        raise ValueError("canvas size must be positive")

    scale = min(canvas_width / source_width, canvas_height / source_height)
    width = max(1, round(source_width * scale))
    height = max(1, round(source_height * scale))
    left = (canvas_width - width) // 2
    top = (canvas_height - height) // 2
    return Placement(width, height, left, top)


def compose(source: Image.Image, canvas_size: tuple[int, int] = DEFAULT_SIZE) -> Image.Image:
    """Returns an opaque RGB picture of canvas_size: the source centered on black."""
    placement = place(source.size, canvas_size)

    # Transparent pixels of the source become black, like the canvas around it.
    flat = Image.new("RGB", source.size, BACKGROUND)
    rgba = source.convert("RGBA")
    flat.paste(rgba, mask=rgba.getchannel("A"))

    if flat.size != (placement.width, placement.height):
        flat = flat.resize((placement.width, placement.height), Image.Resampling.LANCZOS)

    canvas = Image.new("RGB", canvas_size, BACKGROUND)
    canvas.paste(flat, (placement.left, placement.top))
    return canvas


def build(source_path: Path, target_path: Path, canvas_size: tuple[int, int] = DEFAULT_SIZE) -> Placement:
    """Writes the screamer picture of source_path to target_path (PNG) and returns its placement."""
    with Image.open(source_path) as source:
        source.load()
        placement = place(source.size, canvas_size)
        picture = compose(source, canvas_size)

    target_path.parent.mkdir(parents=True, exist_ok=True)
    picture.save(target_path, "PNG", optimize=True)
    return placement
