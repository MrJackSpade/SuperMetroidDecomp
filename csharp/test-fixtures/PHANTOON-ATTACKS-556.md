# Phantoon attack investigation (#556)

Affected player version: 0.1.1. Player descriptions are falling fire wall and
outward circular burst. Do not assume an attack is absent merely because its
conditional trigger was not exercised. Counts, visible geometry, trajectories,
timing and collision remain part of this issue's scope.

## Confirmed regional motion defect

The translation used PAL increments despite targeting the Japan/USA ROM.
`upstream-disassembly/src/macros.asm` defines `regional(value_ntsc, value_pal)`;
the earlier comment in the managed initializer had those variants reversed.
The project's actual ROM bytes independently confirm the NTSC operands:

| Instruction | ROM bytes | Meaning |
|---|---|---|
| $86:9885 | A9 02 00 | Rage clockwise angle step +2 |
| $86:988D | A9 FE FF | Rage counterclockwise angle step -2 |
| $86:9A49 | 69 04 00 | Rage radius step +4 |
| $86:9ADE | 69 02 00 | Spiral radius step +2 |
| $86:9AE8 | 69 02 00 | Spiral angle step +2 |

Before the fix, `--phantoon-flame-region-audit ROM` fails with initial angle
increment 3 versus ROM 2. The production initializer and motion callbacks now
use the named NTSC definitions in `PhantoonFlameMotionRomData`. The audit checks
32 steps across clockwise/counterclockwise rage and two spiral directions,
reading its expectations from the ROM immediates rather than the new constants.
Core verification and both real-room hit-fade/no-input transparency audits pass.

This fixes rotation/expansion that were too fast; it does not yet prove the
reported attacks have correct spawn counts, display, collision, or lifetime.
The attack issue remains open without awaiting-player-validation.

## Rain timer and sound handoff

Pinned bank-$86 `$9A94` decrements the delay and falls through to acceleration
on the same call when it reaches zero or becomes negative. The translation
returned unconditionally after every nonzero delay, adding an extra frame.
It also omitted both `QueueSound_Lib3_Max6($1D)` calls: delay expiry at `$9AA0`
and terrain impact at `$9AD2`.

`--phantoon-rain-timing-audit ROM` reproduces timer 1 leaving velocity zero
instead of 16. The corrected production callback passes seven timer cases
(0, 1, 2, 8, $8000, $8001, $FFFF), including signed-underflow behavior and exact
sound library/ID/queue cap. A real-room flame continues for 69 more motion calls
to terrain impact and retains both sound requests. Publication uses the existing
lossless per-frame enemy sound queue, not a single last-sound field.

Core verification and the production-hit fade/no-input transparency sequences
pass unchanged. These tests verify sound requests, not a new end-to-end audio
recording. Full attack spawn/trajectory/render/collision coverage remains pending.
