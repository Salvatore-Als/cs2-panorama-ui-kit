"""
One builder per component: the bundled markup, for any file name and pool size.

Builders only produce the XML structure and the contract ids/slots; the look lives in the
stylesheet. ``generate`` renders them with the bundled names and sizes, ``scaffold`` with a custom
name and pool so a new layout starts out passing its contract.
"""

from __future__ import annotations

from typing import Callable

from uikit.contracts import Contract, ParamValue
from uikit.layouts.xml import LayoutDocument, indent

Builder = Callable[[str, str, Contract, dict[str, ParamValue]], LayoutDocument]
"""(file name, stylesheet name, contract, pool params) -> document."""


def _count(params: dict[str, ParamValue], key: str) -> int:
    value = params[key]
    if not isinstance(value, int) or value < 1:
        raise ValueError(f"'{key}' must be a positive count, got {value!r}")
    return value


def _codes(params: dict[str, ParamValue], key: str) -> list[str]:
    value = params[key]
    if not isinstance(value, list) or not value:
        raise ValueError(f"'{key}' must be a non-empty list, got {value!r}")
    return value


def toast(
    name: str,
    stylesheet: str,
    contract: Contract,
    params: dict[str, ParamValue],
) -> LayoutDocument:
    placements = _codes(params, "placements")
    depth = _count(params, "depth")

    def card(card_id: str) -> str:
        return f"""\
<Panel id="{card_id}" class="toast hidden" hittest="false">
	<Panel class="toast-accent" hittest="false" />
	<Panel class="toast-body" hittest="false">
		<Label id="{card_id}_kicker" class="toast-kicker" hittest="false" text="{{s:{card_id}_kicker}}" />
		<Label class="toast-title" hittest="false" text="{{s:{card_id}_title}}" />
		<Label id="{card_id}_desc" class="toast-desc" hittest="false" text="{{s:{card_id}_desc}}" />
	</Panel>
	<Panel id="{card_id}_track" class="toast-track" hittest="false">
		<Panel id="{card_id}_bar" class="toast-bar" hittest="false" />
	</Panel>
</Panel>"""

    stacks = []
    for place in placements:
        cards = "\n".join(card(f"t_{place}_{i}") for i in range(depth))
        stack = f'<Panel class="toast-stack stack-{place}" hittest="false">\n'
        stacks.append(stack + indent(cards, 1) + "</Panel>")

    return LayoutDocument(
        name=name,
        stylesheet=stylesheet,
        marker=contract.marker(params),
        body="\n".join(stacks),
        description=f"Toasts: {len(placements)} placement(s) x {depth} card(s). Card t_<place>_<n>.",
    )


def banner(
    name: str,
    stylesheet: str,
    contract: Contract,
    params: dict[str, ParamValue],
) -> LayoutDocument:
    body = """\
<Panel id="b_card" class="banner" hittest="false">
	<Panel class="banner-line" hittest="false" />
	<Panel class="banner-body" hittest="false">
		<Label id="b_kicker" class="banner-kicker" hittest="false" text="{s:b_kicker}" />
		<Label class="banner-text" hittest="false" text="{s:b_text}" />
	</Panel>
	<Panel class="banner-line" hittest="false" />
</Panel>"""
    return LayoutDocument(
        name=name,
        stylesheet=stylesheet,
        marker=contract.marker(params),
        body=body,
        description="Banner: one line near the top, flanked by accent rules.",
    )


def announce(
    name: str,
    stylesheet: str,
    contract: Contract,
    params: dict[str, ParamValue],
) -> LayoutDocument:
    body = """\
<Panel id="a_card" class="announce" hittest="false">
	<Label id="a_kicker" class="announce-kicker" hittest="false" text="{s:a_kicker}" />
	<Label class="announce-title" hittest="false" text="{s:a_title}" />
	<Label id="a_desc" class="announce-desc" hittest="false" text="{s:a_desc}" />
</Panel>"""
    return LayoutDocument(
        name=name,
        stylesheet=stylesheet,
        marker=contract.marker(params),
        body=body,
        description=(
            "Announce: centred card, kicker / title / description.\n"
            "The title slams in only under animate-title."
        ),
    )


def abilities(
    name: str,
    stylesheet: str,
    contract: Contract,
    params: dict[str, ParamValue],
) -> LayoutDocument:
    slots = _count(params, "slots")

    def slot(i: int) -> str:
        return f"""\
<Panel id="ab_{i}" class="ab-slot hidden" hittest="false">
	<Panel class="ab-card" hittest="false">
		<Label class="ab-key" hittest="false" text="{{s:ab_{i}_key}}" />
		<Label class="ab-cd" hittest="false" text="{{s:ab_{i}_cd}}" />
		<Panel class="ab-gauge" hittest="false">
			<Panel id="ab_{i}_bar" class="ab-bar" hittest="false" />
			<Panel id="ab_{i}_hold" class="ab-hold" hittest="false" />
		</Panel>
	</Panel>
	<Label class="ab-name" hittest="false" text="{{s:ab_{i}_name}}" />
</Panel>"""

    tiles = indent("\n".join(slot(i) for i in range(slots)), 1)
    body = '<Panel id="ab_bar" class="ab-bar-row" hittest="false">\n' + tiles + "</Panel>"
    return LayoutDocument(
        name=name,
        stylesheet=stylesheet,
        marker=contract.marker(params),
        body=body,
        description=f"Ability bar, bottom centre: {slots} tile(s) ab_<n>.",
    )


def match(
    name: str,
    stylesheet: str,
    contract: Contract,
    params: dict[str, ParamValue],
) -> LayoutDocument:
    body = """\
<Panel id="m_bar" class="match-bar" hittest="false">
	<Panel class="match-band" hittest="false">
		<Panel class="match-band-line" hittest="false" />
	</Panel>
	<Panel class="match-core" hittest="false">
		<Panel class="match-row" hittest="false">
			<Panel id="m_left" class="match-side" hittest="false">
				<Label class="match-value" hittest="false" text="{s:m_left_value}" />
				<Label class="match-label" hittest="false" text="{s:m_left_label}" />
			</Panel>
			<Panel class="match-mid" hittest="false">
				<Label class="match-center" hittest="false" text="{s:m_center}" />
				<Label id="m_center_sub" class="match-sub" hittest="false" text="{s:m_center_sub}" />
			</Panel>
			<Panel id="m_right" class="match-side" hittest="false">
				<Label class="match-value" hittest="false" text="{s:m_right_value}" />
				<Label class="match-label" hittest="false" text="{s:m_right_label}" />
			</Panel>
		</Panel>
		<Panel id="m_scores" class="match-scores hidden" hittest="false">
			<Panel class="match-cell" hittest="false">
				<Panel id="m_left_tab" class="match-tab" hittest="false">
					<Label class="match-score" hittest="false" text="{s:m_left_score}" />
				</Panel>
			</Panel>
			<Panel class="match-gap" hittest="false" />
			<Panel class="match-cell" hittest="false">
				<Panel id="m_right_tab" class="match-tab" hittest="false">
					<Label class="match-score" hittest="false" text="{s:m_right_score}" />
				</Panel>
			</Panel>
		</Panel>
	</Panel>
</Panel>"""
    return LayoutDocument(
        name=name,
        stylesheet=stylesheet,
        marker=contract.marker(params),
        body=body,
        description=(
            "Match bar, glued to the top edge: count + title / timer / count + title,\n"
            "with an optional score tab under each side. Same text for everyone.\n"
            "band (MatchOptions.Band) adds a long strip across the top and hangs the bar under it."
        ),
    )


def killfeed(
    name: str,
    stylesheet: str,
    contract: Contract,
    params: dict[str, ParamValue],
) -> LayoutDocument:
    rows = _count(params, "rows")

    def row(i: int) -> str:
        return f"""\
<Panel id="kf_{i}" class="kf-row hidden" hittest="false">
	<Label id="kf_{i}_attacker" class="kf-attacker" hittest="false" text="{{s:kf_{i}_attacker}}" />
	<Label id="kf_{i}_tag" class="kf-tag" hittest="false" text="{{s:kf_{i}_tag}}" />
	<Label class="kf-victim" hittest="false" text="{{s:kf_{i}_victim}}" />
</Panel>"""

    feed = indent("\n".join(row(i) for i in range(rows)), 1)
    body = '<Panel class="kf-list" hittest="false">\n' + feed + "</Panel>"
    return LayoutDocument(
        name=name,
        stylesheet=stylesheet,
        marker=contract.marker(params),
        body=body,
        description=f"Kill feed, top right: {rows} row(s) kf_<n>, newest on top.",
    )


def victory(
    name: str,
    stylesheet: str,
    contract: Contract,
    params: dict[str, ParamValue],
) -> LayoutDocument:
    chips = _count(params, "chips")

    def chip(i: int) -> str:
        return f"""\
<Panel id="v_chip_{i}" class="victory-chip hidden" hittest="false">
	<Panel class="victory-chip-accent" hittest="false" />
	<Label class="victory-chip-label" hittest="false" text="{{s:v_chip_{i}_label}}" />
	<Label class="victory-chip-name" hittest="false" text="{{s:v_chip_{i}_name}}" />
</Panel>"""

    chip_block = indent("\n".join(chip(i) for i in range(chips)), 4)
    body = f"""\
<Panel id="v_screen" class="victory-screen" hittest="false">
	<Panel class="victory-dim" hittest="false" />
	<Panel class="victory-card" hittest="false">
		<Panel class="victory-card-accent" hittest="false" />
		<Panel class="victory-body" hittest="false">
			<Label id="v_kicker" class="victory-kicker" hittest="false" text="{{s:v_kicker}}" />
			<Label class="victory-title" hittest="false" text="{{s:v_title}}" />
			<Panel class="victory-rule" hittest="false" />
			<Label id="v_subtitle" class="victory-subtitle" hittest="false" text="{{s:v_subtitle}}" />
			<Panel id="v_chips" class="victory-chips" hittest="false">
{chip_block}			</Panel>
			<Panel id="v_restart" class="victory-restart hidden" hittest="false">
				<Label class="victory-restart-label" hittest="false" text="{{s:v_restart_label}}" />
				<Label class="victory-restart-eta" hittest="false" text="{{s:v_restart_eta}}" />
			</Panel>
		</Panel>
	</Panel>
</Panel>"""
    return LayoutDocument(
        name=name,
        stylesheet=stylesheet,
        marker=contract.marker(params),
        body=body,
        description=(
            f"End of round screen: dark veil, one card, {chips} chip(s), restart countdown.\n"
            'Elements enter one after the other through transition-delay: one "show", one write.'
        ),
    )


def roulette(
    name: str,
    stylesheet: str,
    contract: Contract,
    params: dict[str, ParamValue],
) -> LayoutDocument:
    cells = _count(params, "cells")
    winner = params["winner"]
    if not isinstance(winner, int) or not 0 <= winner < cells:
        raise ValueError(f"'winner' must be one of the {cells} cells (0..{cells - 1}), got {winner!r}")

    def cell(i: int) -> str:
        return f"""\
<Panel id="r_cell_{i}" class="roulette-cell" hittest="false">
	<Panel class="roulette-cell-text" hittest="false">
		<Label class="roulette-cell-name" hittest="false" text="{{s:r_cell_{i}_name}}" />
		<Label id="r_cell_{i}_sub" class="roulette-cell-sub" hittest="false" text="{{s:r_cell_{i}_sub}}" />
	</Panel>
	<Panel class="roulette-cell-accent" hittest="false" />
</Panel>"""

    cell_block = indent("\n".join(cell(i) for i in range(cells)), 5)
    body = f"""\
<Panel id="r_screen" class="roulette-screen" hittest="false">
	<Panel class="roulette-dim" hittest="false" />
	<Panel class="roulette-card" hittest="false">
		<Panel class="roulette-card-accent" hittest="false" />
		<Panel class="roulette-body" hittest="false">
			<Label id="r_kicker" class="roulette-kicker" hittest="false" text="{{s:r_kicker}}" />
			<Label class="roulette-title" hittest="false" text="{{s:r_title}}" />
			<Panel class="roulette-rule" hittest="false" />
			<Panel class="roulette-window" hittest="false">
				<Panel id="r_strip" class="roulette-strip" hittest="false">
{cell_block}				</Panel>
				<Panel class="roulette-fade roulette-fade-left" hittest="false" />
				<Panel class="roulette-fade roulette-fade-right" hittest="false" />
				<Panel class="roulette-marker" hittest="false" />
				<Panel class="roulette-pointer roulette-pointer-top" hittest="false" />
				<Panel class="roulette-pointer roulette-pointer-bottom" hittest="false" />
			</Panel>
			<Panel id="r_result" class="roulette-result hidden" hittest="false">
				<Label class="roulette-result-label" hittest="false" text="{{s:r_result_label}}" />
				<Label class="roulette-result-name" hittest="false" text="{{s:r_result_name}}" />
				<Label id="r_result_sub" class="roulette-result-sub" hittest="false" text="{{s:r_result_sub}}" />
			</Panel>
		</Panel>
	</Panel>
</Panel>"""
    return LayoutDocument(
        name=name,
        stylesheet=stylesheet,
        marker=contract.marker(params),
        body=body,
        description=(
            f"Roulette: dark veil, one card, a strip of {cells} cards under a marker; "
            f"the strip stops on card {winner}.\n"
            'One "spin" class moves the strip, "dur-N" sets how long it takes.'
        ),
    )


def vote(
    name: str,
    stylesheet: str,
    contract: Contract,
    params: dict[str, ParamValue],
) -> LayoutDocument:
    choices = _count(params, "choices")

    def choice(i: int) -> str:
        return f"""\
<Button id="vt_choice_{i}" class="vote-choice hidden">
	<Panel class="vote-choice-line" hittest="false">
		<Label class="vote-choice-label" hittest="false" text="{{s:vt_choice_{i}_label}}" />
		<Label class="vote-choice-count" hittest="false" text="{{s:vt_choice_{i}_count}}" />
		<Label class="vote-choice-pct" hittest="false" text="{{s:vt_choice_{i}_pct}}" />
	</Panel>
	<Label id="vt_choice_{i}_sub" class="vote-choice-sub" hittest="false" text="{{s:vt_choice_{i}_sub}}" />
	<Panel class="vote-track" hittest="false">
		<Panel id="vt_choice_{i}_fill" class="vote-fill" hittest="false" />
	</Panel>
</Button>"""

    choice_block = indent("\n".join(choice(i) for i in range(choices)), 3)
    body = f"""\
<Panel id="vt_screen" class="vote-screen" hittest="false">
	<Panel class="vote-card" hittest="false">
		<Panel class="vote-card-accent" hittest="false" />
		<Panel class="vote-head" hittest="false">
			<Panel class="vote-head-copy" hittest="false">
				<Label id="vt_kicker" class="vote-kicker" hittest="false" text="{{s:vt_kicker}}" />
				<Label class="vote-title" hittest="false" text="{{s:vt_title}}" />
			</Panel>
			<Button id="vt_close" class="vote-close">
				<Label class="vote-close-label" hittest="false" text="✕" />
			</Button>
		</Panel>
		<Panel class="vote-choices" hittest="false">
{choice_block}		</Panel>
		<Panel class="vote-foot" hittest="false">
			<Label class="vote-total" hittest="false" text="{{s:vt_total}}" />
			<Panel class="vote-pager" hittest="false">
				<Button id="vt_prev" class="vote-step">
					<Label class="vote-step-label" hittest="false" text="‹" />
				</Button>
				<Label class="vote-page" hittest="false" text="{{s:vt_page}}" />
				<Button id="vt_next" class="vote-step">
					<Label class="vote-step-label" hittest="false" text="›" />
				</Button>
			</Panel>
			<Label class="vote-time" hittest="false" text="{{s:vt_time}}" />
		</Panel>
	</Panel>
</Panel>"""
    return LayoutDocument(
        name=name,
        stylesheet=stylesheet,
        marker=contract.marker(params),
        body=body,
        description=(
            f"Vote: a compact card on the right, a question, {choices} choice(s) per page "
            "with a share, a count and a bar, pager, total, countdown.\n"
            "Takes the mouse. Clicks: vt_close, vt_prev, vt_next, vt_choice_<n>. "
            "Bars: p-0..p-20 on vt_choice_<n>_fill."
        ),
    )


def panel(
    name: str,
    stylesheet: str,
    contract: Contract,
    params: dict[str, ParamValue],
) -> LayoutDocument:
    pages = _count(params, "pages")
    stats = _count(params, "stats")
    rows = _count(params, "rows")

    def page(n: int) -> str:
        return f"""\
<Button id="p_page_{n}" class="pn-page hidden">
	<Panel class="pn-page-mark" hittest="false" />
	<Label class="pn-page-label" hittest="false" text="{{s:p_page_{n}_label}}" />
</Button>"""

    def stat(n: int) -> str:
        return f"""\
<Panel id="p_stat_{n}" class="pn-stat hidden" hittest="false">
	<Label class="pn-stat-key" hittest="false" text="{{s:p_stat_{n}_key}}" />
	<Label class="pn-stat-value" hittest="false" text="{{s:p_stat_{n}_value}}" />
</Panel>"""

    def row(n: int) -> str:
        row_id = f"p_row_{n}"
        return f"""\
<Panel id="{row_id}" class="pn-row hidden" hittest="false">
	<Panel class="pn-row-copy" hittest="false">
		<Label class="pn-row-title" hittest="false" text="{{s:{row_id}_title}}" />
		<Label class="pn-row-desc" hittest="false" text="{{s:{row_id}_desc}}" />
	</Panel>
	<Label class="pn-row-value" hittest="false" text="{{s:{row_id}_value}}" />
	<Panel class="pn-stepper" hittest="false">
		<Button id="{row_id}_prev" class="pn-step">
			<Label class="pn-step-label" hittest="false" text="‹" />
		</Button>
		<Label class="pn-step-value" hittest="false" text="{{s:{row_id}_value}}" />
		<Button id="{row_id}_next" class="pn-step">
			<Label class="pn-step-label" hittest="false" text="›" />
		</Button>
	</Panel>
	<Button id="{row_id}_btn" class="pn-btn">
		<Label class="pn-btn-label" hittest="false" text="{{s:{row_id}_btn}}" />
	</Button>
	<Button id="{row_id}_toggle" class="pn-toggle">
		<Panel class="pn-knob" hittest="false" />
	</Button>
</Panel>"""

    page_block = indent("\n".join(page(i) for i in range(pages)), 4)
    stat_block = indent("\n".join(stat(i) for i in range(stats)), 5)
    row_block = indent("\n".join(row(i) for i in range(rows)), 5)
    body = f"""\
<Panel id="p_panel" class="pn-panel" hittest="false">
	<Panel class="pn-dim" hittest="false" />
	<Panel class="pn-modal" hittest="false">
		<Panel class="pn-head" hittest="false">
			<Panel class="pn-head-copy" hittest="false">
				<Label class="pn-title" hittest="false" text="{{s:p_title}}" />
				<Label class="pn-subtitle" hittest="false" text="{{s:p_subtitle}}" />
			</Panel>
			<Button id="p_close" class="pn-close">
				<Label class="pn-close-x" hittest="false" text="✕" />
			</Button>
		</Panel>
		<Panel class="pn-body" hittest="false">
			<Panel class="pn-menu" hittest="false">
{page_block}			</Panel>
			<Panel class="pn-content">
				<Panel id="p_stats" class="pn-stats" hittest="false">
{stat_block}				</Panel>
				<Panel class="pn-rows" hittest="false">
{row_block}				</Panel>
			</Panel>
		</Panel>
		<Panel class="pn-foot" hittest="false">
			<Label class="pn-hint" hittest="false" text="{{s:p_hint}}" />
		</Panel>
	</Panel>
</Panel>"""
    return LayoutDocument(
        name=name,
        stylesheet=stylesheet,
        marker=contract.marker(params),
        body=body,
        description=(
            f"Panel: header, {pages} page(s) in the left menu, {stats} stat(s), {rows} row(s).\n"
            "Takes the mouse. Clicks: p_close, p_page_<n>, p_row_<n>_btn / _toggle / _prev / _next."
        ),
    )


def menu(
    name: str,
    stylesheet: str,
    contract: Contract,
    params: dict[str, ParamValue],
) -> LayoutDocument:
    items = _count(params, "items")
    choices = _count(params, "choices")

    def choice(n: int, c: int) -> str:
        choice_id = f"mn_choice_{n}_{c}"
        return f"""\
<Button id="{choice_id}" class="mn-choice hidden">
	<Label class="mn-choice-label" hittest="false" text="{{s:{choice_id}_label}}" />
	<Label class="mn-choice-check" hittest="false" text="✓" />
</Button>"""

    def entry(n: int) -> str:
        item_id = f"mn_item_{n}"
        drop = indent("\n".join(choice(n, c) for c in range(choices)), 2)
        return f"""\
<Panel id="mn_entry_{n}" class="mn-entry hidden" hittest="false">
	<Button id="{item_id}" class="mn-item">
		<Label class="mn-item-index" hittest="false" text="{n + 1}" />
		<Panel class="mn-item-copy" hittest="false">
			<Label class="mn-item-label" hittest="false" text="{{s:{item_id}_label}}" />
			<Label id="{item_id}_desc" class="mn-item-desc" hittest="false" text="{{s:{item_id}_desc}}" />
		</Panel>
		<Label id="{item_id}_value" class="mn-item-value" hittest="false" text="{{s:{item_id}_value}}" />
		<Panel class="mn-item-switch" hittest="false">
			<Panel class="mn-item-knob" hittest="false" />
		</Panel>
		<Label class="mn-item-arrow" hittest="false" text="›" />
	</Button>
	<Panel class="mn-drop">
{drop}	</Panel>
</Panel>"""

    item_block = indent("\n".join(entry(i) for i in range(items)), 3)
    body = f"""\
<Panel id="mn_menu" class="mn-menu" hittest="false">
	<Panel class="mn-card" hittest="false">
		<Panel class="mn-head" hittest="false">
			<Button id="mn_back" class="mn-icon mn-back">
				<Label class="mn-icon-label" hittest="false" text="‹" />
			</Button>
			<Panel class="mn-head-copy" hittest="false">
				<Label class="mn-title" hittest="false" text="{{s:mn_title}}" />
				<Label id="mn_subtitle" class="mn-subtitle" hittest="false" text="{{s:mn_subtitle}}" />
			</Panel>
			<Button id="mn_close" class="mn-icon mn-close">
				<Label class="mn-icon-label" hittest="false" text="✕" />
			</Button>
		</Panel>
		<Label id="mn_path" class="mn-path" hittest="false" text="{{s:mn_path}}" />
		<Panel class="mn-items" hittest="false">
{item_block}		</Panel>
		<Panel class="mn-pager" hittest="false">
			<Button id="mn_prev" class="mn-icon mn-step">
				<Label class="mn-icon-label" hittest="false" text="‹" />
			</Button>
			<Label class="mn-page" hittest="false" text="{{s:mn_page}}" />
			<Button id="mn_next" class="mn-icon mn-step">
				<Label class="mn-icon-label" hittest="false" text="›" />
			</Button>
		</Panel>
	</Panel>
</Panel>"""
    return LayoutDocument(
        name=name,
        stylesheet=stylesheet,
        marker=contract.marker(params),
        body=body,
        description=(
            f"Menu: header with back / close, breadcrumb, {items} entries per page, pager.\n"
            f"A select entry lists (scrollable listbox) up to {choices} choices under it.\n"
            "Takes the mouse. Clicks: mn_close, mn_back, mn_prev, mn_next, "
            "mn_item_<n>, mn_choice_<n>_<c>."
        ),
    )


def card(
    name: str,
    stylesheet: str,
    contract: Contract,
    params: dict[str, ParamValue],
) -> LayoutDocument:
    placements = _codes(params, "placements")
    lines = _count(params, "lines")

    def line(place: str, i: int) -> str:
        line_id = f"c_{place}_line_{i}"
        return f"""\
<Panel id="{line_id}" class="card-line hidden" hittest="false">
	<Label id="{line_id}_label" class="card-line-label" hittest="false" text="{{s:{line_id}_label}}" />
	<Label id="{line_id}_value" class="card-line-value" hittest="false" text="{{s:{line_id}_value}}" />
</Panel>"""

    def one(place: str) -> str:
        rows = indent("\n".join(line(place, i) for i in range(lines)), 1)
        return (
            f'<Panel id="c_{place}" class="card card-{place} hidden" hittest="false">\n'
            f'\t<Label id="c_{place}_title" class="card-title" hittest="false" text="{{s:c_{place}_title}}" />\n'
            f'\t<Label id="c_{place}_desc" class="card-desc" hittest="false" text="{{s:c_{place}_desc}}" />\n'
            + rows
            + "</Panel>"
        )

    return LayoutDocument(
        name=name,
        stylesheet=stylesheet,
        marker=contract.marker(params),
        body="\n".join(one(place) for place in placements),
        description=(
            f"Card: {len(placements)} corner(s) x title / description / {lines} line(s). Every part is optional.\n"
            "No animation: the card is there when `hidden` goes."
        ),
    )


def target(
    name: str,
    stylesheet: str,
    contract: Contract,
    params: dict[str, ParamValue],
) -> LayoutDocument:
    positions = _codes(params, "positions")

    def one(place: str) -> str:
        return f"""\
<Panel class="target-zone target-zone-{place}" hittest="false">
	<Panel id="tg_{place}" class="target target-{place} size-md hidden" hittest="false">
		<Label class="target-text" hittest="false" text="{{s:tg_{place}_text}}" />
	</Panel>
</Panel>"""

    return LayoutDocument(
        name=name,
        stylesheet=stylesheet,
        marker=contract.marker(params),
        body="\n".join(one(place) for place in positions),
        description=(
            f"Target: {len(positions)} one-line label(s) around the crosshair (screen centre).\n"
            "Each sits in a half-screen zone whose inner edge is the centre. size-sm/md/lg/xl set the text size."
        ),
    )


def progress(
    name: str,
    stylesheet: str,
    contract: Contract,
    params: dict[str, ParamValue],
) -> LayoutDocument:
    placements = _codes(params, "placements")

    def one(place: str) -> str:
        return f"""\
<Panel class="progress-zone progress-zone-{place}" hittest="false">
	<Panel id="pg_{place}" class="progress progress-{place} hidden" hittest="false">
		<Panel class="progress-head" hittest="false">
			<Label id="pg_{place}_label" class="progress-label" hittest="false" text="{{s:pg_{place}_label}}" />
			<Label id="pg_{place}_value" class="progress-value" hittest="false" text="{{s:pg_{place}_value}}" />
		</Panel>
		<Panel class="progress-track" hittest="false">
			<Panel id="pg_{place}_fill" class="progress-fill" hittest="false" />
		</Panel>
	</Panel>
</Panel>"""

    return LayoutDocument(
        name=name,
        stylesheet=stylesheet,
        marker=contract.marker(params),
        body="\n".join(one(place) for place in placements),
        description=(
            f"Progress: {len(placements)} bar(s), each a label / value row over a track.\n"
            "The fill is a clip: step-N for a pushed value, timed + dur-N + run for a bar that runs alone."
        ),
    )


def dial(
    name: str,
    stylesheet: str,
    contract: Contract,
    params: dict[str, ParamValue],
) -> LayoutDocument:
    placements = _codes(params, "placements")

    def one(place: str) -> str:
        return f"""\
<Panel id="dl_{place}" class="dial dial-{place} size-md hidden" hittest="false">
	<Panel class="dial-body" hittest="false">
		<Panel class="dial-track" hittest="false" />
		<Panel id="dl_{place}_fill" class="dial-fill" hittest="false" />
		<Label id="dl_{place}_value" class="dial-value" hittest="false" text="{{s:dl_{place}_value}}" />
	</Panel>
	<Label id="dl_{place}_label" class="dial-label" hittest="false" text="{{s:dl_{place}_label}}" />
</Panel>"""

    return LayoutDocument(
        name=name,
        stylesheet=stylesheet,
        marker=contract.marker(params),
        body="\n".join(one(place) for place in placements),
        description=(
            f"Dial: {len(placements)} ring(s), a value in the middle and a label under.\n"
            "The fill is a radial clip: step-N for a pushed value, timed + dur-N + run for a ring that runs alone.\n"
            "size-sm/md/lg/xl set the ring size."
        ),
    )


def screamer(
    name: str,
    stylesheet: str,
    contract: Contract,
    params: dict[str, ParamValue],
) -> LayoutDocument:
    body = '<Panel id="sc_image" class="screamer hidden" hittest="false" />'
    return LayoutDocument(
        name=name,
        stylesheet=stylesheet,
        marker=contract.marker(params),
        body=body,
        description=(
            "Screamer: one full-screen picture.\n"
            "scare-<name> on sc_image sets the picture; the kit plays the sound from the server."
        ),
    )


def recap(
    name: str,
    stylesheet: str,
    contract: Contract,
    params: dict[str, ParamValue],
) -> LayoutDocument:
    stats = _count(params, "stats")
    rows = _count(params, "rows")
    row_stats = _count(params, "row_stats")

    def row_stat(r: int, i: int) -> str:
        return f"""\
<Panel id="rc_row_{r}_stat_{i}" class="recap-stat recap-row-stat hidden" hittest="false">
	<Panel id="rc_row_{r}_stat_{i}_icon" class="recap-stat-icon" hittest="false" />
	<Label class="recap-stat-value" hittest="false" text="{{s:rc_row_{r}_stat_{i}_value}}" />
	<Label class="recap-stat-label" hittest="false" text="{{s:rc_row_{r}_stat_{i}_label}}" />
</Panel>"""

    def row(r: int) -> str:
        cells = indent("\n".join(row_stat(r, i) for i in range(row_stats)), 2)
        return f"""\
<Panel id="rc_row_{r}" class="recap-row hidden" hittest="false">
	<Panel class="recap-row-who" hittest="false">
		<Label class="recap-row-name" hittest="false" text="{{s:rc_row_{r}_name}}" />
		<Label id="rc_row_{r}_sub" class="recap-row-sub" hittest="false" text="{{s:rc_row_{r}_sub}}" />
	</Panel>
	<Panel class="recap-row-stats" hittest="false">
{cells}	</Panel>
</Panel>"""

    row_block = indent("\n".join(row(r) for r in range(rows)), 4)

    def stat(i: int) -> str:
        return f"""\
<Panel id="rc_stat_{i}" class="recap-stat hidden" hittest="false">
	<Panel class="recap-stat-accent" hittest="false" />
	<Panel id="rc_stat_{i}_icon" class="recap-stat-icon" hittest="false" />
	<Label class="recap-stat-value" hittest="false" text="{{s:rc_stat_{i}_value}}" />
	<Label class="recap-stat-label" hittest="false" text="{{s:rc_stat_{i}_label}}" />
</Panel>"""

    stat_block = indent("\n".join(stat(i) for i in range(stats)), 4)
    body = f"""\
<Panel id="rc_screen" class="recap-screen" hittest="false">
	<Panel class="recap-veil" hittest="false" />
	<Panel class="recap-glow" hittest="false" />
	<Panel class="recap-head" hittest="false">
		<Label id="rc_kicker" class="recap-kicker" hittest="false" text="{{s:rc_kicker}}" />
		<Panel id="rc_place_row" class="recap-place-row" hittest="false">
			<Label class="recap-place" hittest="false" text="{{s:rc_place}}" />
			<Label id="rc_place_total" class="recap-place-total" hittest="false" text="{{s:rc_place_total}}" />
		</Panel>
		<Label class="recap-title" hittest="false" text="{{s:rc_title}}" />
		<Panel class="recap-rule" hittest="false" />
	</Panel>
	<Panel class="recap-foot" hittest="false">
		<Panel class="recap-strip" hittest="false">
			<Panel class="recap-strip-accent" hittest="false" />
			<Panel class="recap-strip-body" hittest="false">
				<Panel id="rc_solo" class="recap-player" hittest="false">
					<Label class="recap-player-name" hittest="false" text="{{s:rc_player}}" />
					<Label id="rc_player_sub" class="recap-player-sub" hittest="false" text="{{s:rc_player_sub}}" />
				</Panel>
				<Panel id="rc_stats" class="recap-stats" hittest="false">
{stat_block}				</Panel>
				<Panel id="rc_rows" class="recap-rows hidden" hittest="false">
{row_block}				</Panel>
			</Panel>
		</Panel>
		<Panel id="rc_restart" class="recap-restart hidden" hittest="false">
			<Label class="recap-restart-label" hittest="false" text="{{s:rc_restart_label}}" />
			<Label class="recap-restart-eta" hittest="false" text="{{s:rc_restart_eta}}" />
		</Panel>
	</Panel>
</Panel>"""
    return LayoutDocument(
        name=name,
        stylesheet=stylesheet,
        marker=contract.marker(params),
        body=body,
        description=(
            f"End of match screen: veil, placement and headline at the top, a strip at the bottom with\n"
            f"the player's name and {stats} stat tile(s). icon-<name> on rc_stat_<n>_icon sets a tile's picture.\n"
            'Elements enter one after the other through transition-delay: one "show", one write.\n'
            "pulse-a / pulse-b swap on a tile whose value changed while shown.\n"
            f"Or {rows} player row(s) of {row_stats} stat(s) (a duo, a squad) in place of the single player."
        ),
    )


def zone(
    name: str,
    stylesheet: str,
    contract: Contract,
    params: dict[str, ParamValue],
) -> LayoutDocument:
    hazard = "\n".join('<Panel class="zone-hazard-bar" hittest="false" />' for _ in range(16))
    stripes = '<Panel class="zone-hazard" hittest="false">\n' + indent(hazard, 1) + "</Panel>"
    body = f"""\
<Panel class="zone-area zone-area-status" hittest="false">
	<Panel id="zn_status" class="zone-status hidden" hittest="false">
		<Panel class="zone-row" hittest="false">
			<Panel class="zone-icon" hittest="false" />
			<Label id="zn_phase" class="zone-phase" hittest="false" text="{{s:zn_phase}}" />
			<Label class="zone-state" hittest="false" text="{{s:zn_state}}" />
			<Label id="zn_timer" class="zone-timer" hittest="false" text="{{s:zn_timer}}" />
		</Panel>
		<Panel id="zn_bar" class="zone-bar" hittest="false">
			<Panel id="zn_bar_fill" class="zone-bar-fill" hittest="false" />
		</Panel>
		<Label id="zn_info" class="zone-info" hittest="false" text="{{s:zn_info}}" />
	</Panel>
</Panel>
<Panel class="zone-area zone-area-alert" hittest="false">
	<Panel id="zn_alert" class="zone-alert hidden" hittest="false">
		<Panel class="zone-alert-flash" hittest="false" />
{indent(stripes, 2)}		<Panel class="zone-alert-body" hittest="false">
			<Panel class="zone-alert-icon" hittest="false" />
			<Label id="zn_alert_kicker" class="zone-alert-kicker" hittest="false" text="{{s:zn_alert_kicker}}" />
			<Label class="zone-alert-title" hittest="false" text="{{s:zn_alert_title}}" />
			<Label id="zn_alert_desc" class="zone-alert-desc" hittest="false" text="{{s:zn_alert_desc}}" />
		</Panel>
{indent(stripes, 2)}	</Panel>
</Panel>"""
    return LayoutDocument(
        name=name,
        stylesheet=stylesheet,
        marker=contract.marker(params),
        body=body,
        description=(
            "Zone: a status widget under the compass (phase, state, countdown, bar, info line)\n"
            "and an alert in the upper third, framed by hazard stripes.\n"
            "The bar is a clip: step-N snaps it to the share left, dur-N + run drain it."
        ),
    )


def minimap(
    name: str,
    stylesheet: str,
    contract: Contract,
    params: dict[str, ParamValue],
) -> LayoutDocument:
    marks = _count(params, "marks")

    def zone(z: str) -> str:
        return f"""\
<Panel id="mm_zone_{z}" class="mm-zone mm-zone-{z} hidden" hittest="false">
	<Panel id="mm_zone_{z}_y" class="mm-zone-y" hittest="false">
		<Panel id="mm_zone_{z}_ring" class="mm-ring" hittest="false" />
	</Panel>
</Panel>"""

    def mark(m: int) -> str:
        return f"""\
<Panel id="mm_mark_{m}" class="mm-mark hidden" hittest="false">
	<Panel id="mm_mark_{m}_y" class="mm-mark-y" hittest="false">
		<Panel class="mm-mark-halo" hittest="false" />
		<Panel id="mm_mark_{m}_icon" class="mm-mark-icon" hittest="false" />
		<Label id="mm_mark_{m}_label" class="mm-mark-label" hittest="false" text="{{s:mm_mark_{m}_label}}" />
	</Panel>
</Panel>"""

    # Next under current, markers over both, in slot order (z-index by kind does the rest).
    layers = "\n".join([zone("next"), zone("cur"), *(mark(m) for m in range(marks))])
    body = f"""\
<Panel id="mm_frame" class="minimap hidden" hittest="false">
	<Panel class="minimap-veil" hittest="false" />
	<Panel class="minimap-box" hittest="false">
		<Panel id="mm_map" class="minimap-map" hittest="false">
			<Panel id="mm_zoom" class="mm-zoom" hittest="false">
				<Panel id="mm_pan" class="mm-pan" hittest="false">
					<Panel id="mm_pan_y" class="mm-pan-y" hittest="false">
						<Panel class="minimap-world" hittest="false" />
{indent(layers, 6)}					</Panel>
				</Panel>
			</Panel>
		</Panel>
	</Panel>
	<Label id="mm_title" class="minimap-title" hittest="false" text="{{s:mm_title}}" />
</Panel>"""
    return LayoutDocument(
        name=name,
        stylesheet=stylesheet,
        marker=contract.marker(params),
        body=body,
        description=(
            f"Minimap: map picture, current and next zone circles, {marks} marker(s).\n"
            "Positions are x-N / y-N on two nested panels (one transform per axis), radii r-N on the ring,\n"
            "a shrink is dur-N then the target classes, run by the client.\n"
            "Zoom: everything on the map sits in mm_zoom (zoom-*, a scale) > mm_pan / mm_pan_y (px-* / py-*, centre on the player)."
        ),
    )


COMMS_SCANLINES = 45
"""1px lines, 4px apart: they cover the 180px portrait."""


def comms(
    name: str,
    stylesheet: str,
    contract: Contract,
    params: dict[str, ParamValue],
) -> LayoutDocument:
    scanlines = indent(
        "\n".join('<Panel class="comms-scanline" hittest="false" />' for _ in range(COMMS_SCANLINES)), 1
    )
    bars = "\n".join(
        f'\t<Panel class="comms-signal-bar comms-signal-bar-{i}" hittest="false" />' for i in range(1, 5)
    )
    body = f"""\
<Panel id="cm_card" class="comms" hittest="false">
	<Panel class="comms-head" hittest="false">
		<Panel class="comms-rec" hittest="false" />
		<Label id="cm_kicker" class="comms-kicker" hittest="false" text="{{s:cm_kicker}}" />
		<Panel class="comms-signal" hittest="false">
{indent(bars, 2)}		</Panel>
	</Panel>
	<Panel class="comms-body" hittest="false">
		<Panel class="comms-portrait" hittest="false">
			<Panel class="comms-portrait-glow" hittest="false" />
			<Panel id="cm_face" class="comms-face" hittest="false">
				<Panel class="comms-face-layer comms-face-ghost-r" hittest="false" />
				<Panel class="comms-face-layer comms-face-ghost-b" hittest="false" />
				<Panel class="comms-face-layer comms-face-main" hittest="false" />
			</Panel>
			<Panel class="comms-scanlines" hittest="false">
{indent(scanlines, 3)}			</Panel>
			<Panel class="comms-scanband" hittest="false" />
			<Panel class="comms-noise" hittest="false" />
			<Panel class="comms-corner comms-corner-tl" hittest="false" />
			<Panel class="comms-corner comms-corner-br" hittest="false" />
		</Panel>
		<Panel class="comms-copy" hittest="false">
			<Label id="cm_name" class="comms-name" hittest="false" text="{{s:cm_name}}" />
			<Label class="comms-text" hittest="false" text="{{s:cm_text}}" />
			<Label id="cm_footer" class="comms-footer" hittest="false" text="{{s:cm_footer}}" />
		</Panel>
	</Panel>
</Panel>"""
    return LayoutDocument(
        name=name,
        stylesheet=stylesheet,
        marker=contract.marker(params),
        body=body,
        description=(
            "Comms: radio transmission card on the left. Kicker with rec dot and signal bars,\n"
            "a hologram portrait (face-<name> on cm_face: three layers, the main one and two colour ghosts,\n"
            f"under {COMMS_SCANLINES} scanlines, a moving scan band and a static layer), name, text, footer."
        ),
    )


COMPASS_CARDINALS = {0: "N", 45: "NE", 90: "E", 135: "SE", 180: "S", 225: "SW", 270: "W", 315: "NW"}
"""Letters on the compass strip, by heading."""


def compass(
    name: str,
    stylesheet: str,
    contract: Contract,
    params: dict[str, ParamValue],
) -> LayoutDocument:
    pins = _count(params, "pins")

    # One cell per 15 degrees, -180..540: the kit keeps the heading unwrapped within -90..450, and
    # the viewport shows 90 degrees either side of it.
    def cell(angle: int) -> str:
        heading = angle % 360
        letter = COMPASS_CARDINALS.get(heading)
        if letter is None:
            return '<Panel class="compass-cell" hittest="false"><Panel class="compass-tick" hittest="false" /></Panel>'

        kind = "compass-cardinal" if heading % 90 == 0 else "compass-ordinal"
        if heading == 0:
            kind += " compass-north"
        return f"""\
<Panel class="compass-cell {kind}" hittest="false">
	<Label class="compass-letter" hittest="false" text="{letter}" />
	<Panel class="compass-tick" hittest="false" />
</Panel>"""

    def pin(i: int) -> str:
        return f"""\
<Panel id="cp_pin_{i}" class="compass-pin hidden" hittest="false">
	<Panel class="compass-pin-icon" hittest="false" />
	<Panel class="compass-pin-arrow" hittest="false" />
	<Label id="cp_pin_{i}_dist" class="compass-pin-dist" hittest="false" text="{{s:cp_pin_{i}_dist}}" />
</Panel>"""

    cells = indent("\n".join(cell(angle) for angle in range(-180, 541, 15)), 4)
    pin_block = indent("\n".join(pin(i) for i in range(pins)), 3)
    body = f"""\
<Panel id="cp_bar" class="compass hidden" hittest="false">
	<Panel class="compass-frame" hittest="false">
		<Panel class="compass-view" hittest="false">
			<Panel id="cp_strip" class="compass-strip" hittest="false">
{cells}			</Panel>
			<Panel class="compass-fade compass-fade-l" hittest="false" />
			<Panel class="compass-fade compass-fade-r" hittest="false" />
		</Panel>
		<Panel class="compass-pins" hittest="false">
{pin_block}		</Panel>
		<Panel class="compass-caret" hittest="false" />
	</Panel>
	<Label class="compass-deg" hittest="false" text="{{s:cp_deg}}" />
</Panel>"""
    return LayoutDocument(
        name=name,
        stylesheet=stylesheet,
        marker=contract.marker(params),
        body=body,
        description=(
            f"Compass: a strip of 15-degree cells (-180..540) in a clipped viewport, {pins} pin(s) over it.\n"
            "hd-N on cp_strip slides the heading under the caret; rel-N / edge-l / edge-r on a pin place it."
        ),
    )


def inventory(
    name: str,
    stylesheet: str,
    contract: Contract,
    params: dict[str, ParamValue],
) -> LayoutDocument:
    slots = _count(params, "slots")

    def tile(i: int) -> str:
        return f"""\
<Panel id="inv_{i}" class="inv-slot hidden" hittest="false">
	<Panel class="inv-tile" hittest="false">
		<Panel class="inv-accent" hittest="false" />
		<Panel id="inv_{i}_icon" class="inv-icon" hittest="false" />
		<Label id="inv_{i}_key" class="inv-key" hittest="false" text="{{s:inv_{i}_key}}" />
		<Label id="inv_{i}_count" class="inv-count" hittest="false" text="{{s:inv_{i}_count}}" />
	</Panel>
	<Label id="inv_{i}_name" class="inv-name" hittest="false" text="{{s:inv_{i}_name}}" />
</Panel>"""

    tiles = "\n".join(tile(i) for i in range(slots))
    body = '<Panel id="inv_bar" class="inv-bar" hittest="false">\n' + indent(tiles, 1) + "</Panel>"
    return LayoutDocument(
        name=name,
        stylesheet=stylesheet,
        marker=contract.marker(params),
        body=body,
        description=(
            f"Inventory: a row of {slots} tile(s) at the bottom centre. Tile inv_<n>.\n"
            "item-<name> on inv_<n>_icon sets the icon; the name shows under the selected tile."
        ),
    )


BUILDERS: dict[str, Builder] = {
    "toast": toast,
    "banner": banner,
    "announce": announce,
    "abilities": abilities,
    "match": match,
    "killfeed": killfeed,
    "victory": victory,
    "roulette": roulette,
    "vote": vote,
    "panel": panel,
    "menu": menu,
    "card": card,
    "target": target,
    "progress": progress,
    "dial": dial,
    "screamer": screamer,
    "inventory": inventory,
    "compass": compass,
    "comms": comms,
    "zone": zone,
    "recap": recap,
    "minimap": minimap,
}
