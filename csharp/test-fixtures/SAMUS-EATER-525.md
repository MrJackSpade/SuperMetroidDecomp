# Samus Eater capture (#525)

Affected version: 0.1.1. Player confirmed Samus Eater, not Yapping Maw.
The earlier Maw diagnostics concern a different actor and do not resolve this report.

## Reproduced omission and implementation

Real Beta Power Bomb room $8F:A37C has floor-plant trigger $35A1/BTS80 at block
(23,14). Standing Samus at (368,219), radius21, satisfies native floor setup's
feet alignment. Before the fix, one full-runtime step leaves the special-air trigger
unchanged; `--samus-eater-audit ROM` fails that assertion. The inside-block area
dispatcher omitted both plant setup routines, and the PLM owner lacked their
position/damage instructions. This—not Maw enemy targeting—is the missing path.

Brinstar BTS80/81 now dispatch $84:B0DC/B113 and allocate the normal descending PLM
slot. Setup applies the exact feet/head alignment gate, deactivates the trigger,
and saves Samus coordinates with native -1/+1 Y. The existing interpreter executes
the ROM's floor/ceiling lists. Added AC89 pre-instruction restores saved position
and ORs immunity with $10; AC9D accumulates two periodic damage points; ACB1 sets
release immunity to $30. No artificial InputLocked or pose change is introduced.
Normal movement still executes before the plant writes position, as in the cart.

## Current verification

The real-room floor fixture now checks every frame through the first 257 calls:

- trigger deactivation on entry;
- mouth-owned position (368,218) through frame160;
- eight five-frame draw steps repeated four times, followed by the 96-frame
  released-open delay and original trigger restoration at frame256;
- eight two-damage events at frames15/35 plus each successive forty-frame cycle,
  reducing health999 to983 with no suit or host cheats;
- four library2/$31/max6 sound requests at frames15,55,95,135;
- right input after release moves Samus away without a forced input unlock.

The draw assertion currently checks the authored trigger tile word, not the full
composed mouth pixels. Ceiling orientation, admission boundaries, captured-state
round-trip and visual capture remain to verify before the overall ticket is ready.
The ROM list/source drives this implementation; no independent native-CPU replay
of the complete plant sequence has been performed. Keep open, not awaiting validation.
