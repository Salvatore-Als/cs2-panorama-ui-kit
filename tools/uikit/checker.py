"""
Checks a layout file against the contract it declares.

The layout names its contract in a marker comment, anywhere in the file::

    <!-- uikit:toast placements=tr,bc depth=3 -->

Errors are what makes the kit draw nothing or the wrong thing (missing required id or slot, wrong
root id, unparsable XML). Warnings are probable mistakes (typo'd slot, text on a non-Label,
stylesheet missing a contract class). Notes are informational (optional slot absent, extra slot).
"""

from __future__ import annotations

import difflib
import re
import xml.etree.ElementTree as ET
from dataclasses import dataclass, field
from enum import Enum
from pathlib import Path

from uikit.contracts import Contract, ContractError, ContractSet, ParamValue
from uikit.paths import LAYOUTS_DIR, STYLES_DIR, display

MARKER_RE = re.compile(r"uikit:(\w+)((?:[ \t]+\w+=[\w,]+)*)")
SLOT_RE = re.compile(r"\{s:([A-Za-z0-9_]+)\}")
INCLUDE_RE = re.compile(r'<include\s+src="([^"]+)"')
CSS_CLASS_RE = re.compile(r"\.([A-Za-z0-9_-]+)")
CSS_COMMENT_RE = re.compile(r"/\*.*?\*/", re.S)

TYPO_CUTOFF = 0.85


class Severity(Enum):
    ERROR = "error"
    WARNING = "warn"
    NOTE = "note"


@dataclass(frozen=True)
class Finding:
    severity: Severity
    message: str


@dataclass
class Report:
    path: Path
    component: str | None = None
    findings: list[Finding] = field(default_factory=list)

    def add(self, severity: Severity, message: str) -> None:
        self.findings.append(Finding(severity, message))

    def of(self, severity: Severity) -> list[str]:
        return [f.message for f in self.findings if f.severity is severity]

    @property
    def ok(self) -> bool:
        return not self.of(Severity.ERROR)

    def render(self, verbose: bool = True) -> str:
        status = "ok"
        if not self.ok:
            status = "FAIL"

        header = f"[{status}] {display(self.path)}"
        if self.component:
            header += f"  ({self.component})"

        lines = [header]
        for severity in Severity:
            if severity is Severity.NOTE and not verbose:
                continue
            lines += [f"   {severity.value:<6} {message}" for message in self.of(severity)]
        return "\n".join(lines)


@dataclass(frozen=True)
class Marker:
    component: str
    overrides: dict[str, str]

    @classmethod
    def find(cls, text: str) -> Marker | None:
        match = MARKER_RE.search(text)
        if match is None:
            return None
        overrides = dict(pair.split("=", 1) for pair in match.group(2).split())
        return cls(match.group(1), overrides)


@dataclass
class ParsedLayout:
    """Ids and text slots of a layout, in document order."""

    ids: dict[str, ET.Element]
    slots: dict[str, ET.Element]
    duplicate_ids: list[str]


def parse_layout(text: str) -> ParsedLayout:
    tree = ET.fromstring(text)
    ids: dict[str, ET.Element] = {}
    slots: dict[str, ET.Element] = {}
    duplicates: list[str] = []
    for element in tree.iter():
        panel_id = element.attrib.get("id")
        if panel_id is not None:
            if panel_id in ids:
                duplicates.append(panel_id)
            ids[panel_id] = element
        for var in SLOT_RE.findall(element.attrib.get("text", "")):
            slots[var] = element
    return ParsedLayout(ids, slots, duplicates)


def stylesheet_classes(text: str, styles_dir: Path = STYLES_DIR) -> set[str] | None:
    """Every ``.class`` the included stylesheets mention. None when none could be found locally."""
    found = False
    classes: set[str] = set()
    for src in INCLUDE_RE.findall(text):
        css = styles_dir / Path(src).name.replace(".vcss_c", ".css")
        if css.is_file():
            found = True
            source = CSS_COMMENT_RE.sub("", css.read_text(encoding="utf-8"))
            classes |= set(CSS_CLASS_RE.findall(source))
    if not found:
        return None
    return classes


class LayoutChecker:
    def __init__(self, contracts: ContractSet, styles_dir: Path = STYLES_DIR) -> None:
        self._contracts = contracts
        self._styles_dir = styles_dir

    def check(self, path: Path, marker: Marker | None = None) -> Report:
        """Checks ``path`` against ``marker``, or the marker found in the file."""
        report = Report(path)
        text = path.read_text(encoding="utf-8")

        marker = marker or Marker.find(text)
        if marker is None:
            report.add(
                Severity.ERROR,
                "no <!-- uikit:<component> ... --> marker naming the contract this layout implements",
            )
            return report

        report.component = marker.component
        try:
            contract = self._contracts[marker.component]
            params = contract.resolve(marker.overrides)
        except ContractError as exc:
            report.add(Severity.ERROR, str(exc))
            return report

        try:
            layout = parse_layout(text)
        except ET.ParseError as exc:
            report.add(Severity.ERROR, f"XML does not parse: {exc}")
            return report

        for duplicate in layout.duplicate_ids:
            report.add(Severity.ERROR, f"duplicate id '{duplicate}'")

        self._check_root(report, path, layout)
        self._check_panels(report, contract, params, layout)
        self._check_texts(report, contract, params, layout)
        self._check_stylesheet(report, contract, text)
        return report

    def check_directory(self, directory: Path = LAYOUTS_DIR) -> list[Report]:
        """Every layout in ``directory`` that carries a marker."""
        return [
            self.check(path)
            for path in sorted(directory.glob("*.xml"))
            if Marker.find(path.read_text(encoding="utf-8")) is not None
        ]

    # ------------------------------------------------------------------ rules

    @staticmethod
    def _check_root(report: Report, path: Path, layout: ParsedLayout) -> None:
        root_id = f"{path.stem}_root"
        root = layout.ids.get(root_id)
        if root is None:
            report.add(Severity.ERROR, f"no panel with id '{root_id}': the root id is <file name>_root")
            return

        if "hidden" not in root.attrib.get("class", "").split():
            report.add(
                Severity.WARNING,
                f"'{root_id}' is not authored 'hidden': the layout shows to every player",
            )

    @staticmethod
    def _check_panels(
        report: Report,
        contract: Contract,
        params: dict[str, ParamValue],
        layout: ParsedLayout,
    ) -> None:
        for spec in contract.panels:
            for panel_id in contract.expand(spec.id, params):
                element = layout.ids.get(panel_id)
                if element is not None:
                    if spec.tag and element.tag != spec.tag:
                        report.add(
                            Severity.ERROR,
                            f"'{panel_id}' is a <{element.tag}>; it must be a <{spec.tag}> to receive clicks",
                        )
                    continue

                if spec.required:
                    report.add(Severity.ERROR, f"missing panel id '{panel_id}'")
                else:
                    classes = ", ".join(spec.classes)
                    report.add(Severity.NOTE, f"no panel id '{panel_id}' ({classes} will have no effect)")

    @staticmethod
    def _check_texts(
        report: Report,
        contract: Contract,
        params: dict[str, ParamValue],
        layout: ParsedLayout,
    ) -> None:
        known: set[str] = set()
        for spec in contract.texts:
            if spec.extra:
                continue

            for var in contract.expand(spec.var, params):
                known.add(var)
                if var in layout.slots:
                    continue

                if spec.required:
                    report.add(Severity.ERROR, f"missing text slot {{s:{var}}} ({spec.doc})")
                else:
                    report.add(Severity.NOTE, f"no text slot {{s:{var}}} ({spec.doc} will not show)")

        extras = contract.extra_patterns()
        for var, element in layout.slots.items():
            if element.tag != "Label":
                report.add(
                    Severity.WARNING,
                    f"{{s:{var}}} is on a <{element.tag}>: only a Label renders text",
                )

            if var in known:
                continue

            close = difflib.get_close_matches(var, known, n=1, cutoff=TYPO_CUTOFF)
            if close:
                report.add(Severity.WARNING, f"{{s:{var}}} looks like a typo of {{s:{close[0]}}}")
            elif any(pattern.match(var) for pattern in extras):
                key = var.rsplit("_", 1)[-1]
                report.add(Severity.NOTE, f'extra slot {{s:{var}}}, filled from Texts["{key}"]')
            else:
                report.add(
                    Severity.WARNING,
                    f"{{s:{var}}} is not part of the {contract.component} contract: nothing writes it",
                )

    def _check_stylesheet(self, report: Report, contract: Contract, text: str) -> None:
        classes = stylesheet_classes(text, self._styles_dir)
        if classes is None:
            report.add(
                Severity.WARNING,
                f"no included stylesheet found in {display(self._styles_dir)}",
            )
            return
        for cls in contract.css:
            if cls not in classes:
                report.add(Severity.WARNING, f"stylesheet never mentions .{cls}")
