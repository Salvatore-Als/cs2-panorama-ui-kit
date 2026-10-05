from __future__ import annotations

import pytest

from uikit.contracts import ContractSet


@pytest.fixture(scope="session")
def contracts() -> ContractSet:
    return ContractSet.load()
