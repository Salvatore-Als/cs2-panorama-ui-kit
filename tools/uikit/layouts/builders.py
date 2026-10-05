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
</Panel>"""
    return LayoutDocument(
        name=name,
        stylesheet=stylesheet,
        marker=contract.marker(params),
        body=body,
        description=(
            "Match bar, glued to the top edge: count + title / timer / count + title,\n"
            "with an optional score tab under each side. Same text for everyone."
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
	<Label class="roulette-cell-name" hittest="false" text="{{s:r_cell_{i}_name}}" />
	<Label id="r_cell_{i}_sub" class="roulette-cell-sub" hittest="false" text="{{s:r_cell_{i}_sub}}" />
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
			<Panel class="roulette-window" hittest="false">
				<Panel id="r_strip" class="roulette-strip" hittest="false">
{cell_block}				</Panel>
				<Panel class="roulette-fade roulette-fade-left" hittest="false" />
				<Panel class="roulette-fade roulette-fade-right" hittest="false" />
				<Panel class="roulette-marker" hittest="false" />
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
			<Panel class="pn-content" hittest="false">
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
		<Label class="mn-item-caret" hittest="false" text="▾" />
		<Panel class="mn-item-switch" hittest="false">
			<Panel class="mn-item-knob" hittest="false" />
		</Panel>
		<Label class="mn-item-arrow" hittest="false" text="›" />
	</Button>
	<Panel class="mn-drop" hittest="false">
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
            f"A select entry opens a dropdown of up to {choices} choices under it.\n"
            "Takes the mouse. Clicks: mn_close, mn_back, mn_prev, mn_next, "
            "mn_item_<n>, mn_choice_<n>_<c>."
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
}
