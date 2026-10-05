"""
Panorama UI Kit tooling.

A layout implements a component (toast, banner, ...) by carrying the panel ids, ``{s:...}`` text
slots and classes the kit drives. Nothing in the game checks that - a missing id or a typo in a slot
renders a blank card with no error anywhere. This package is the check, the generator of the
bundled layouts, and the generator of the contract reference (docs/CONTRACTS.md).

Source of truth: ``contracts/*.json`` at the repository root.
"""

__version__ = "1.0.0"
