"""Repository locations. Everything is resolved from this file, never from the working directory."""

from __future__ import annotations

from pathlib import Path

REPO_ROOT: Path = Path(__file__).resolve().parents[2]
CONTRACTS_DIR: Path = REPO_ROOT / "contracts"
PANORAMA_DIR: Path = REPO_ROOT / "panorama"
LAYOUTS_DIR: Path = PANORAMA_DIR / "layout" / "custom_game"
STYLES_DIR: Path = PANORAMA_DIR / "styles" / "custom_game"
CONTRACTS_DOC: Path = REPO_ROOT / "docs" / "CONTRACTS.md"
CONTRACTS_DOCS_DIR: Path = REPO_ROOT / "docs" / "contracts"


def display(path: Path) -> str:
    """Path relative to the repository when it is inside it, absolute otherwise."""
    try:
        return str(path.resolve().relative_to(REPO_ROOT))
    except ValueError:
        return str(path)
