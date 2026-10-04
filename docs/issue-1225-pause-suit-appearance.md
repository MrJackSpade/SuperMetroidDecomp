# #1225: Suit appearance after equipment-menu changes

Affected version: 0.4.13-smoke1198 (76ff1c82), the latest local test build handed
to the player. Report: Samus's suit graphic remains stale after changing suits
in the pause equipment menu and returning to gameplay.

## Diagnosis and fix

The menu updates EquippedItems immediately, but gameplay CGRAM is retained
separately from the pause-screen palette. Unpause restored beam art and reconciled
movement without refreshing the selected suit colors.

Native state $11 calls ContinueInitialising_GameplayResume ($82:A2E3), which runs
Samus command $0C ($90:F29E). That calls UpdateSamusPoseDueToChangeOfEquipment;
its $91:E6CB explicitly calls LoadSamusSuitPalette. Cross-checked against pinned
InsaneFirebat disassembly revision 362be646929cf8e483f692b73a6561cfc2dc1d0d for
NTSC J/U 1.0, banks 82, 90 and 91.

The fix restores this existing suit-palette operation during UnpausingB, before
its accepted NMI. Normal installed suit colors and the native Gravity > Varia >
Power selection rule remain owned by Samus.LoadSuitPalette.

## Focused regression

Guarded DebugRunner command:

    --pause-suit-appearance-audit <installed-content-directory>

The fixture binds installed artwork, uses a stationary synthetic floor in a
loaded room, and drives real Start/R/Down/A menu inputs through both fades.
It checks the initial palette and actual equipped bits, then all 15 opaque
Samus colors in the gameplay render snapshot immediately after pause teardown
and again after the full gameplay fade-in.

Before the change, toggling Varia off left captured color 2 at $02FF instead
of Power Suit $03BD. The regression fails on that visible palette mismatch.
After the change, Varia -> Power -> Varia -> Gravity -> Varia all pass at both
resume boundaries. This confirms the reported color-selection contract; it is
not whole-game or player validation. Release DebugRunner build passes.

#1225 remains open awaiting player validation.
