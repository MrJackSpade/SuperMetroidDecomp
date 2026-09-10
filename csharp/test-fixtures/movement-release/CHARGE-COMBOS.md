# #416: Charge Beam Combo implementation evidence (incomplete)

The live C# firing path increments charge to 120 but never invokes the native
FireSBA dispatcher. Its projectile pre-instruction enum also lacks the four combo
families. `--combo-activation-input-audit ROM` reproduces the missing handoff for
Charge plus Wave, Ice, Spazer and Plasma with two Power Bombs and PB selected:
130 held-Fire frames leave PB=2 and charge=120 in every case. This is an explicitly
failing diagnostic, not a passing implementation test or full timing oracle.

## Native dispatcher oracle

`native-combo-activation-probe.h` calls original $90:CCC0, not the C translation.
432 cases cover all twelve normal beam combinations, PB stock 0/1/2, selection
none/PB, both facings, and slot-zero pre-instruction none/Ice/Plasma. Equipment is
Charge plus that low-nibble beam selection. Samus starts at (512,384); remaining
WRAM starts zero except auto-cancel selection three. Existing pre-instructions
are seeded handler-boundary conditions, not proof that those states can be reached
with a particular equipment/input sequence. The zero-ammo selected case likewise
tests the dispatcher, not whether the HUD permits selecting it normally.

The capture records ammo, selection, auto-cancel, projectile count, cooldown,
shared SBA state, carry and four native projectile records. Each slash-separated
record contains fourteen concatenated four-hex-digit words: type, direction,
pre-instruction, X, Y, X speed, Y speed, variable, trail timer, damage, X radius,
Y radius, instruction pointer, instruction timer.

Native observations that implementation must preserve:

- Only indices 1/2/4/8 dispatch a family; other normal combinations return clear.
- Ammo subtraction/clamping precedes family dispatch. A repeated Ice/Plasma
  attempt can debit ammo even when the existing pre-instruction rejects spawning.
- The dispatcher itself can spawn with zero ammo; do not invent a gate here.
  HUD/input eligibility needs its own comparison.
- Depletion clears both selected and auto-cancel item state.
- Wave/Spazer and Ice/Plasma have different initialization, overwrite and shared
  direction-state semantics; do not assume four identical radial projectiles.

## Reproduction

- ROM SHA256: `12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72`.
- Native host: `578f90b3cc49557bb70060ad033bb90b8cf8ac50`.
- Disassembly: `362be646929cf8e483f692b73a6561cfc2dc1d0d`.
- Archive: `combo-activation-416-v1.zip`.
- CSV SHA256: `6A50975CDAEA6A8465118CA7E27DC024BA7CB6A959DD4BDCEB26071921411CD8`.
- Two captures are byte-identical. Temporary source hooks removed; reapply checked.
- Apply `native-combo-activation-entrypoint.patch`, build, and run only the bounded,
  dialog-free `sm.exe --diagnostic-combo-activation ROM NEW.csv` entrypoint.

Remaining: implement shared activation and particle ownership/pre-instructions;
compare real-input timing including turns/spins/release and invalid selections;
test ammo limits, equipment changes and the active-combo armament restrictions.
Family-specific trajectory/damage work belongs to #417–#420, while #416 must not
be considered ready merely because allocation or this endpoint witness passes.
Reference: https://wiki.supermetroid.run/Charge_Beam_Combos (revision 7564).
