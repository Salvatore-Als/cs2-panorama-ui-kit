"""
Entry point: `python -m screamer` (from tools/), or `screamer` once installed.

    python -m screamer                                  writes into workshop/ (the kit addon)
    python -m screamer --addon ../../halloween/assets   writes into another addon's content folder
"""

from __future__ import annotations

import argparse
import sys
from pathlib import Path

# Run as a folder (python tools/screamer): no parent package, make the package importable.
if not __package__:
    sys.path.insert(0, str(Path(__file__).resolve().parents[1]))


def main() -> None:
    parser = argparse.ArgumentParser(prog="screamer", description="Create the pictures of the kit screamers.")
    parser.add_argument("--addon", type=Path, help="addon content root (holds panorama/); default: workshop/ of this repository")
    args = parser.parse_args()

    from screamer import paths

    if args.addon is not None:
        if not args.addon.is_dir():
            parser.error(f"{args.addon} is not a folder")
        paths.set_addon(args.addon)

    # Imported after set_addon: the modules read the paths at import time
    from screamer.gui import run

    run()


if __name__ == "__main__":
    main()
