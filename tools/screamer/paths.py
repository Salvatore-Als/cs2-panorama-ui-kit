"""Locations. Everything is resolved from this file or from --addon, never from the working directory."""

from __future__ import annotations

import re
from pathlib import Path

# tools/screamer/paths.py -> panorama-ui-kit/
REPO_ROOT: Path = Path(__file__).resolve().parents[2]

# The kit addon content (holds panorama/, sounds/, soundevents/).
WORKSHOP_DIR: Path = REPO_ROOT / "workshop"

# The kit stylesheet: one rule .screamer.scare-<name> per screamer, the source of truth for the names.
STYLESHEET: Path = WORKSHOP_DIR / "panorama" / "styles" / "custom_game" / "uikit_screamer.css"
SCARE_PATTERN = re.compile(r"\.scare-([a-z][a-z0-9_-]*)")

# Root of the addon content the tool writes into. The kit's workshop/ by default; set_addon()
# before importing the other modules.
ADDON_ROOT: Path = WORKSHOP_DIR
IMAGES_DIR: Path


def set_addon(root: Path) -> None:
    """Writes into another addon (halloween/assets...). Call it before importing gui and vtex."""
    global ADDON_ROOT, IMAGES_DIR
    ADDON_ROOT = root.resolve()
    IMAGES_DIR = ADDON_ROOT / "panorama" / "images" / "custom_game" / "uikit" / "screamer"


set_addon(WORKSHOP_DIR)


def read_names(stylesheet: Path = STYLESHEET) -> tuple[str, ...]:
    """Screamer names of the kit stylesheet, in file order (same list as ScreamerNames.All)."""
    names: list[str] = []
    for name in SCARE_PATTERN.findall(stylesheet.read_text(encoding="utf-8")):
        if name not in names:
            names.append(name)
    return tuple(names)


NAMES: tuple[str, ...] = read_names()


def display(path: Path) -> str:
    """Path relative to the addon when it is inside it, absolute otherwise."""
    try:
        return str(path.resolve().relative_to(ADDON_ROOT))
    except ValueError:
        return str(path)
