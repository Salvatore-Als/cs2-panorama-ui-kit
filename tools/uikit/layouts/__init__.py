"""
Layout generation.

- :func:`generate_bundled` renders the kit's own ``uikit_*`` layouts at their default pool sizes.
- :func:`scaffold` starts a custom layout for a component: the XML from the same builder (so it
  passes its contract from the first line) and a copy of the bundled stylesheet to restyle.
"""

from __future__ import annotations

import shutil
from dataclasses import dataclass, replace
from pathlib import Path

from uikit.contracts import ContractSet, ParamValue
from uikit.layouts.builders import BUILDERS
from uikit.paths import LAYOUTS_DIR, STYLES_DIR

BUNDLED_PREFIX = "uikit_"


@dataclass(frozen=True)
class Written:
    layout: Path
    stylesheet: Path | None = None


def bundled_name(component: str) -> str:
    return f"{BUNDLED_PREFIX}{component}"


def generate_bundled(contracts: ContractSet, layouts_dir: Path = LAYOUTS_DIR) -> list[Path]:
    """Writes every bundled layout. Stylesheets are hand-written and left alone."""
    layouts_dir.mkdir(parents=True, exist_ok=True)
    written = []
    for component, build in BUILDERS.items():
        contract = contracts[component]
        name = bundled_name(component)
        document = build(name, name, contract, contract.defaults())
        path = layouts_dir / f"{name}.xml"
        path.write_text(document.render(), encoding="utf-8")
        written.append(path)
    return written


def scaffold(
    contracts: ContractSet,
    component: str,
    name: str,
    params: dict[str, ParamValue],
    *,
    force: bool = False,
    layouts_dir: Path = LAYOUTS_DIR,
    styles_dir: Path = STYLES_DIR,
) -> Written:
    """New ``<name>.xml`` and ``<name>.css``. Refuses to overwrite unless ``force``."""
    if name.startswith(BUNDLED_PREFIX):
        raise ValueError(f"'{BUNDLED_PREFIX}*' names are reserved for the bundled layouts")

    contract = contracts[component]
    layout_path = layouts_dir / f"{name}.xml"
    style_path = styles_dir / f"{name}.css"
    for path in (layout_path, style_path):
        if path.exists() and not force:
            raise FileExistsError(f"{path} exists (use --force to overwrite)")

    document = replace(
        BUILDERS[component](name, name, contract, params),
        generated_by=f"python -m uikit scaffold {component} {name}",
    )
    layout_path.write_text(document.render(), encoding="utf-8")

    bundled_css = styles_dir / f"{bundled_name(component)}.css"
    if bundled_css.is_file():
        shutil.copyfile(bundled_css, style_path)
        return Written(layout_path, style_path)
    return Written(layout_path)
