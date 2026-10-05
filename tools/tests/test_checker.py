from __future__ import annotations

from pathlib import Path

import pytest

from uikit.checker import LayoutChecker, Severity
from uikit.contracts import ContractSet
from uikit.layouts import bundled_name, generate_bundled, scaffold
from uikit.layouts.builders import BUILDERS
from uikit.paths import STYLES_DIR


@pytest.fixture()
def workspace(tmp_path: Path) -> tuple[Path, Path]:
    layouts = tmp_path / "layout"
    styles = tmp_path / "styles"
    layouts.mkdir()
    styles.mkdir()
    for css in STYLES_DIR.glob("uikit_*.css"):
        (styles / css.name).write_text(css.read_text(encoding="utf-8"), encoding="utf-8")
    return layouts, styles


@pytest.mark.parametrize("component", sorted(BUILDERS))
def test_bundled_layouts_pass_their_contract(
    contracts: ContractSet,
    workspace,
    component: str,
) -> None:
    layouts, styles = workspace
    generate_bundled(contracts, layouts)

    report = LayoutChecker(contracts, styles).check(layouts / f"{bundled_name(component)}.xml")
    assert report.ok, report.render()
    assert not report.of(Severity.WARNING), report.render()


def test_scaffold_with_custom_pool_passes(contracts: ContractSet, workspace) -> None:
    layouts, styles = workspace
    params = contracts["toast"].resolve({"depth": "3", "placements": "tr"})
    written = scaffold(
        contracts,
        "toast",
        "neon_toast",
        params,
        layouts_dir=layouts,
        styles_dir=styles,
    )
    assert written.stylesheet is not None and written.stylesheet.is_file()
    report = LayoutChecker(contracts, styles).check(written.layout)
    assert report.ok, report.render()


def test_scaffold_refuses_bundled_names_and_overwrites(contracts: ContractSet, workspace) -> None:
    layouts, styles = workspace
    params = contracts["banner"].defaults()
    with pytest.raises(ValueError):
        scaffold(
            contracts,
            "banner",
            "uikit_banner",
            params,
            layouts_dir=layouts,
            styles_dir=styles,
        )
    scaffold(contracts, "banner", "big_banner", params, layouts_dir=layouts, styles_dir=styles)
    with pytest.raises(FileExistsError):
        scaffold(contracts, "banner", "big_banner", params, layouts_dir=layouts, styles_dir=styles)


def _broken(layouts: Path, contracts: ContractSet, replacements: dict[str, str]) -> Path:
    generate_bundled(contracts, layouts)
    source = (layouts / "uikit_toast.xml").read_text(encoding="utf-8")
    for old, new in replacements.items():
        source = source.replace(old, new)
    path = layouts / "broken_toast.xml"
    path.write_text(source, encoding="utf-8")
    return path


def test_wrong_root_missing_id_and_typo_are_reported(contracts: ContractSet, workspace) -> None:
    layouts, styles = workspace
    path = _broken(layouts, contracts, {
        'id="t_bc_2"': 'id="t_bc_x"',
        "{s:t_tr_1_title}": "{s:t_tr_1_titel}",
    })
    report = LayoutChecker(contracts, styles).check(path)
    errors = " | ".join(report.of(Severity.ERROR))
    assert "broken_toast_root" in errors
    assert "t_bc_2" in errors
    assert "{s:t_tr_1_title}" in errors
    assert any("typo" in w for w in report.of(Severity.WARNING))


def test_layout_without_marker_fails(contracts: ContractSet, workspace) -> None:
    layouts, styles = workspace
    path = layouts / "plain.xml"
    path.write_text("<root><Panel /></root>", encoding="utf-8")
    report = LayoutChecker(contracts, styles).check(path)
    assert not report.ok


def test_clickable_ids_must_be_buttons(contracts: ContractSet, workspace) -> None:
    layouts, styles = workspace
    generate_bundled(contracts, layouts)
    source = (layouts / "uikit_panel.xml").read_text(encoding="utf-8")
    source = source.replace(
        '<Button id="p_close" class="pn-close">',
        '<Panel id="p_close" class="pn-close">',
        1,
    )
    source = source.replace(
        '<Label class="pn-close-x" hittest="false" text="✕" />\n\t\t\t\t\t\t</Button>',
        '<Label class="pn-close-x" hittest="false" text="✕" />\n\t\t\t\t\t\t</Panel>',
        1,
    )
    path = layouts / "broken_panel.xml"
    path.write_text(source.replace("uikit_panel_root", "broken_panel_root"), encoding="utf-8")
    report = LayoutChecker(contracts, styles).check(path)
    assert any("must be a <Button>" in e for e in report.of(Severity.ERROR)), report.render()
