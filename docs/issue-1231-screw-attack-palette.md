# #1231: Screw Attack suit colors

The player reported that Screw Attack did not properly turn Samus green.
Affected version: 0.4.14-smoke1228 (v0.4.15 tags the same gameplay commit).

## Cartridge contract and cause

Pinned disassembly revision 362be646929cf8e483f692b73a6561cfc2dc1d0d,
NTSC J/U 1.0, bank_91.asm: Handle_ScrewAttack_SpeedBoosting_Palette
compares the animation frame with $001B at $91:DA04. BPL at $91:DA07
restores the normal suit palette for frame 27 onward. Frame zero resets
the palette phase at $91:DA43; frames 1 through 26 cycle the Screw Attack
colors through six phases (shades 0, 1, 2, 3, 2, 1).

The port reversed this condition, copying normal colors during the active
Screw Attack and cycling only at its ending frames. Corrected the signed
branch condition and the older regression assertions that encoded it backwards.

## Focused reproduction and confirmation

ScrewAttackPaletteAudit drives an actual frontend spin jump with Varia and
Screw Attack equipped. It compares all 15 opaque displayed suit colors with
the installed native Varia Screw Attack palette sequence, independently of
the production phase counter. Before the fix it failed at animation frame 2,
phase 2, color 1: expected 01A8, displayed 0108.

After the fix, 25 actual spin-jump frames matched every displayed color,
including 20 tinted phases. A focused production-handler check also confirms
frame 27 restores all 15 normal Varia colors. Release DebugRunner build passed.
Run through the guarded DebugRunner entry point:

    SuperMetroid.DebugRunner --screw-attack-palette-audit <installation>

Player confirmation remains pending.
