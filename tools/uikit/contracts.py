"""
Component contracts, loaded from ``contracts/*.json``.

A contract lists what the kit addresses in a layout: panels (by id, with the classes the kit
toggles on them) and text slots (``{s:var}``). Ids may hold placeholders (``t_{p}_{n}``) that repeat
over the layout's pool parameters (placements, depth, rows...).
"""

from __future__ import annotations

import itertools
import json
import re
from dataclasses import dataclass, field
from pathlib import Path
from typing import Union

from uikit.paths import CONTRACTS_DIR

ParamValue = Union[int, list[str]]
"""A pool parameter: a count (``depth=4``) or a list of codes (``placements=tr,bc``)."""

EXTRA_KEY = "<key>"


def _join(value: ParamValue) -> str:
    """``tr,bc`` for a list of codes, ``4`` for a count."""
    if isinstance(value, list):
        return ",".join(value)
    return str(value)


class ContractError(ValueError):
    """A contract, or the parameters given for one, is not usable."""


@dataclass(frozen=True)
class Param:
    name: str
    default: ParamValue
    doc: str


@dataclass(frozen=True)
class PanelSpec:
    """A panel the kit toggles classes on."""

    id: str
    required: bool
    classes: tuple[str, ...]
    doc: str
    tag: str | None = None
    """Required element tag, e.g. ``Button`` for anything the player clicks."""


@dataclass(frozen=True)
class TextSpec:
    """A ``{s:var}`` slot the kit writes. ``extra`` slots are the free ``Texts[key]`` pattern."""

    var: str
    required: bool
    doc: str
    extra: bool = False


@dataclass(frozen=True)
class Contract:
    component: str
    csharp: str
    summary: str
    params: dict[str, Param]
    repeat: dict[str, str]
    """Placeholder -> parameter it repeats over: ``{"p": "placements", "n": "depth"}``."""
    panels: tuple[PanelSpec, ...]
    texts: tuple[TextSpec, ...]
    css: tuple[str, ...]
    """Classes the layout's stylesheet is expected to mention."""

    # ------------------------------------------------------------------ construction

    @classmethod
    def from_json(cls, data: dict) -> Contract:
        return cls(
            component=data["component"],
            csharp=data["csharp"],
            summary=data["summary"],
            params={
                name: Param(name, spec["default"], spec["doc"])
                for name, spec in data["params"].items()
            },
            repeat=dict(data["repeat"]),
            panels=tuple(
                PanelSpec(p["id"], p["required"], tuple(p["classes"]), p["doc"], p.get("tag"))
                for p in data["panels"]
            ),
            texts=tuple(
                TextSpec(spec["var"], spec["required"], spec["doc"], spec.get("extra", False))
                for spec in data["texts"]
            ),
            css=tuple(data["css"]),
        )

    # ------------------------------------------------------------------ parameters

    def defaults(self) -> dict[str, ParamValue]:
        return {name: param.default for name, param in self.params.items()}

    def resolve(self, overrides: dict[str, str] | None = None) -> dict[str, ParamValue]:
        """Defaults overridden by raw string values (lists comma-separated, counts as integers)."""
        values = self.defaults()
        for key, raw in (overrides or {}).items():
            if key not in values:
                known = ", ".join(values) or "none"
                raise ContractError(
                    f"unknown parameter '{key}' for {self.component} (has: {known})"
                )
            if isinstance(values[key], list):
                values[key] = [code for code in raw.split(",") if code]
            else:
                try:
                    values[key] = int(raw)
                except ValueError as exc:
                    raise ContractError(
                        f"parameter '{key}' must be an integer, got '{raw}'"
                    ) from exc
        return values

    # ------------------------------------------------------------------ expansion

    def expand(self, template: str, params: dict[str, ParamValue]) -> list[str]:
        """``t_{p}_{n}`` -> every concrete id over the placeholders the template uses."""
        axes: list[tuple[str, list[str]]] = []
        for placeholder, param in self.repeat.items():
            if "{" + placeholder + "}" not in template:
                continue
            value = params[param]
            if isinstance(value, list):
                axes.append((placeholder, value))
            else:
                axes.append((placeholder, [str(index) for index in range(value)]))

        if not axes:
            return [template]

        expanded = []
        for combo in itertools.product(*(values for _, values in axes)):
            concrete = template
            for (placeholder, _), value in zip(axes, combo):
                concrete = concrete.replace("{" + placeholder + "}", value)
            expanded.append(concrete)

        return expanded

    def extra_patterns(self) -> list[re.Pattern[str]]:
        """Regexes matching any concrete ``Texts[key]`` slot of this contract."""
        patterns = []
        for spec in self.texts:
            if not spec.extra:
                continue
            pattern = re.escape(spec.var.replace(EXTRA_KEY, "\0"))
            for placeholder in self.repeat:
                pattern = pattern.replace(re.escape("{" + placeholder + "}"), r"[a-z0-9]+")

            pattern = "^" + pattern.replace("\0", r"[a-z0-9_-]+") + "$"
            patterns.append(re.compile(pattern))

        return patterns

    def marker(self, params: dict[str, ParamValue] | None = None) -> str:
        """The ``uikit:<component> k=v`` marker a layout carries in a comment."""
        pool = " ".join(
            f"{name}={_join(value)}" for name, value in (params or self.defaults()).items()
        )
        marker = f"uikit:{self.component}"
        if pool:
            marker += f" {pool}"
        return marker


@dataclass
class ContractSet:
    """Every contract, by component name."""

    contracts: dict[str, Contract] = field(default_factory=dict)

    @classmethod
    def load(cls, directory: Path = CONTRACTS_DIR) -> ContractSet:
        contracts = {}
        for path in sorted(directory.glob("*.json")):
            contract = Contract.from_json(json.loads(path.read_text(encoding="utf-8")))
            if contract.component != path.stem:
                raise ContractError(f"{path.name} declares component '{contract.component}'")
            contracts[contract.component] = contract
        return cls(contracts)

    def __getitem__(self, component: str) -> Contract:
        try:
            return self.contracts[component]
        except KeyError:
            known = ", ".join(self.contracts)
            raise ContractError(f"unknown component '{component}' (known: {known})") from None

    def __iter__(self):
        return iter(self.contracts.values())

    def names(self) -> list[str]:
        return list(self.contracts)
