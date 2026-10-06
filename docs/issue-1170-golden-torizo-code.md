# Golden Torizo controller code (#1170)

Affected version: `0.4.2+719e01e72669478823123e3fea2926af0691c4f1`, carried
forward from the latest release handed off in this testing session. The tester
reported that the Golden Torizo code did not work. Inspection confirmed that
Torizo initialization stopped after palette setup and omitted the native code.

## Native contract

Pinned disassembly `362be646929cf8e483f692b73a6561cfc2dc1d0d`, bank_AA.asm,
`InitAI_Torizo.GTCode` at $AA:C914-C95D, checks held controller input for exact
equality with $C0C0 (A+B+X+Y). Additional held buttons prevent activation.
The defeated-boss return precedes this check. The Bomb Torizo path returns
without entering the code. The check occurs during enemy initialization on room
entry, not while fighting.

The branch overwrites current and maximum energy with 700, reserves with 300,
missiles with 100, Super Missiles with 20, and Power Bombs with 20. It overwrites
both collected/equipped items with $F337 and both collected/equipped beams with
$100F, preserving simultaneous Spazer and Plasma. It does not normalize those
masks, retain previously collected upgrades, add capacities, set reserve mode,
or change reserve missiles. This is the literal cartridge behavior.

## Implementation and confirmation

The room loader already passes `Controller1.Current` and the live Samus owner
to enemy loading and initialization. The Torizo initializer now receives those
existing arguments and performs the native branch after Golden palette setup.
Named definitions retain the immediate values and their native addresses.

`--golden-torizo-code` invokes the production initialization dispatcher. Before
the change, the exact chord on an undefeated Golden Torizo failed the inventory
assertion. Afterward all 14 written inventory words match the supported ROM's
immediates. Higher initial capacities prove replacement, and the initial Screw
Attack bit proves the native item overwrite. An extra button, a missing button,
an already-defeated encounter, and Bomb Torizo leave inventory unchanged.
Reserve mode and reserve missiles remain unchanged in every case. The fixture
forbids gameplay cartridge reads.

Verification build and all three release static-analysis gates (PLM programs,
queued VRAM DMA, and enemy visual programs) pass. This is a focused initialization
confirmation, not an exploratory playthrough or player validation.
