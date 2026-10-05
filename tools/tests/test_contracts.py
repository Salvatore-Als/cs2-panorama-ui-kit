from __future__ import annotations

import pytest

from uikit.contracts import ContractError, ContractSet


def test_every_component_has_a_contract(contracts: ContractSet) -> None:
    assert set(contracts.names()) == {
        "toast",
        "banner",
        "announce",
        "abilities",
        "match",
        "killfeed",
        "victory",
        "roulette",
        "vote",
        "panel",
        "menu",
    }


def test_resolve_overrides_counts_and_lists(contracts: ContractSet) -> None:
    params = contracts["toast"].resolve({"depth": "2", "placements": "tr,bc"})
    assert params == {"placements": ["tr", "bc"], "depth": 2}


def test_resolve_rejects_unknown_and_malformed(contracts: ContractSet) -> None:
    with pytest.raises(ContractError):
        contracts["toast"].resolve({"rows": "3"})
    with pytest.raises(ContractError):
        contracts["toast"].resolve({"depth": "three"})


def test_expand_repeats_over_every_axis(contracts: ContractSet) -> None:
    toast = contracts["toast"]
    ids = toast.expand("t_{p}_{n}_title", toast.resolve({"depth": "2", "placements": "tr,bc"}))
    assert ids == ["t_tr_0_title", "t_tr_1_title", "t_bc_0_title", "t_bc_1_title"]


def test_extra_pattern_matches_free_keys_only(contracts: ContractSet) -> None:
    (pattern,) = contracts["toast"].extra_patterns()
    assert pattern.match("t_tr_0_footer")
    assert not pattern.match("b_footer")


def test_marker_round_trips_defaults(contracts: ContractSet) -> None:
    assert contracts["banner"].marker() == "uikit:banner"
    assert contracts["killfeed"].marker() == "uikit:killfeed rows=5"


def test_unknown_component_is_a_contract_error(contracts: ContractSet) -> None:
    with pytest.raises(ContractError):
        contracts["scoreboard"]
