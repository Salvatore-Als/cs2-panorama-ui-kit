"""
Command line: ``python -m uikit <command>`` (from tools/), or ``uikit <command>`` once installed.

    check FILE...                    check layouts against the contract their marker names
    check-all                        check every layout in panorama/layout/custom_game with a marker
    slots COMPONENT [k=v ...]        what a layout of COMPONENT must contain, for these pool sizes
    scaffold COMPONENT NAME [k=v]    start a custom layout (XML + stylesheet copy) that passes its contract
    generate                         regenerate the bundled uikit_* layouts
    docs                             regenerate docs/CONTRACTS.md and docs/contracts/*.md

Exit code 1 on any contract error, so check / check-all can gate a build.
"""

from __future__ import annotations

import argparse
import sys
from pathlib import Path
from typing import Sequence

from uikit import __version__
from uikit.checker import LayoutChecker, Report, Severity
from uikit.contracts import ContractError, ContractSet
from uikit.docs import render_contract_page, render_contracts_index, render_reference, render_slots
from uikit.layouts import generate_bundled, scaffold
from uikit.paths import CONTRACTS_DOC, CONTRACTS_DOCS_DIR, display

EXIT_OK = 0
EXIT_FAILED = 1


def _pairs(values: Sequence[str]) -> dict[str, str]:
    """``["depth=3", "placements=tr,bc"]`` -> dict. Rejects anything without '='."""
    pairs = {}
    for value in values:
        key, sep, raw = value.partition("=")
        if not sep or not key:
            raise argparse.ArgumentTypeError(f"expected key=value, got '{value}'")
        pairs[key] = raw
    return pairs


def _print_reports(reports: list[Report], verbose: bool) -> int:
    for report in reports:
        if verbose or report.of(Severity.ERROR) or report.of(Severity.WARNING):
            print(report.render(verbose))
    failed = sum(not r.ok for r in reports)
    print(f"uikit: {len(reports)} layout(s) checked, {failed} failing")
    if failed:
        return EXIT_FAILED
    return EXIT_OK


# --------------------------------------------------------------------------- commands

def cmd_check(args: argparse.Namespace, contracts: ContractSet) -> int:
    checker = LayoutChecker(contracts)
    return _print_reports([checker.check(Path(f)) for f in args.files], verbose=True)


def cmd_check_all(args: argparse.Namespace, contracts: ContractSet) -> int:
    return _print_reports(LayoutChecker(contracts).check_directory(), verbose=args.verbose)


def cmd_slots(args: argparse.Namespace, contracts: ContractSet) -> int:
    contract = contracts[args.component]
    print(render_slots(contract, contract.resolve(_pairs(args.params))))
    return EXIT_OK


def cmd_scaffold(args: argparse.Namespace, contracts: ContractSet) -> int:
    contract = contracts[args.component]
    written = scaffold(
        contracts,
        args.component,
        args.name,
        contract.resolve(_pairs(args.params)),
        force=args.force,
    )
    print(f"layout      {display(written.layout)}")
    if written.stylesheet:
        print(f"stylesheet  {display(written.stylesheet)}")

    csharp = contract.csharp.split(" /")[0]
    print(f'\nUse it from C#: new {csharp} {{ Name = "{args.name}", ... }}')
    return EXIT_OK


def cmd_generate(args: argparse.Namespace, contracts: ContractSet) -> int:
    for path in generate_bundled(contracts):
        print(display(path))
    return EXIT_OK


def cmd_docs(args: argparse.Namespace, contracts: ContractSet) -> int:
    CONTRACTS_DOC.parent.mkdir(parents=True, exist_ok=True)
    CONTRACTS_DOC.write_text(render_reference(contracts), encoding="utf-8")
    print(display(CONTRACTS_DOC))

    CONTRACTS_DOCS_DIR.mkdir(parents=True, exist_ok=True)
    kept = {"README.md"}
    for contract in contracts:
        path = CONTRACTS_DOCS_DIR / f"{contract.component}.md"
        path.write_text(render_contract_page(contract), encoding="utf-8")
        kept.add(path.name)
        print(display(path))

    for stale in CONTRACTS_DOCS_DIR.glob("*.md"):
        if stale.name not in kept:
            stale.unlink()

    index = CONTRACTS_DOCS_DIR / "README.md"
    index.write_text(render_contracts_index(contracts), encoding="utf-8")
    print(display(index))
    return EXIT_OK


# --------------------------------------------------------------------------- parser

def build_parser(components: list[str]) -> argparse.ArgumentParser:
    parser = argparse.ArgumentParser(prog="uikit", description="Panorama UI Kit layout tooling.")
    parser.add_argument("--version", action="version", version=f"%(prog)s {__version__}")
    sub = parser.add_subparsers(dest="command", required=True, metavar="command")

    check = sub.add_parser("check", help="check layout files against their contract")
    check.add_argument("files", nargs="+", metavar="FILE")
    check.set_defaults(handler=cmd_check)

    check_all = sub.add_parser("check-all", help="check every layout that carries a uikit marker")
    check_all.add_argument(
        "-v",
        "--verbose",
        action="store_true",
        help="also print passing layouts and notes",
    )
    check_all.set_defaults(handler=cmd_check_all)

    slots = sub.add_parser("slots", help="list the ids and text slots a layout needs")
    slots.add_argument("component", choices=components)
    slots.add_argument("params", nargs="*", metavar="KEY=VALUE")
    slots.set_defaults(handler=cmd_slots)

    scaffold_cmd = sub.add_parser("scaffold", help="start a custom layout that passes its contract")
    scaffold_cmd.add_argument("component", choices=components)
    scaffold_cmd.add_argument("name", help="file name without extension, e.g. neon_toast")
    scaffold_cmd.add_argument("params", nargs="*", metavar="KEY=VALUE")
    scaffold_cmd.add_argument("--force", action="store_true", help="overwrite existing files")
    scaffold_cmd.set_defaults(handler=cmd_scaffold)

    generate = sub.add_parser("generate", help="regenerate the bundled uikit_* layouts")
    generate.set_defaults(handler=cmd_generate)

    docs = sub.add_parser("docs", help="regenerate docs/CONTRACTS.md and docs/contracts/*.md")
    docs.set_defaults(handler=cmd_docs)

    return parser


def main(argv: Sequence[str] | None = None) -> int:
    contracts = ContractSet.load()
    args = build_parser(contracts.names()).parse_args(argv)
    try:
        return args.handler(args, contracts)
    except (ContractError, ValueError, FileExistsError, argparse.ArgumentTypeError) as exc:
        print(f"uikit: error: {exc}", file=sys.stderr)
        return EXIT_FAILED


if __name__ == "__main__":
    sys.exit(main())
