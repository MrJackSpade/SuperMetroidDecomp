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

## Real encounter population and visible patterns

`--phantoon-attack-population-audit ROM LOCAL_DIRECTORY` runs 3600 no-input
frames in the actual room, without assigning AI phases or manually spawning
flames. Eight spiral records appear at frame 1495; eight rain records appear at
frame 1603. Each has a live sprite map. The fixture asserts both populations and
writes per-frame slot/position/angle/radius/delay/sprite/collision-property traces.

The spawn-frame display still reflects the preceding NMI. Captures 16, 32 and 48
frames later avoid mistaking that normal display delay for missing rendering.
Visually inspected frame +32 shows eight blue flames distributed around the boss
for spiral, and eight blue flames forming a staggered falling row for rain.
All captures remain local; none are published with this source-only fixture.

This rules out absent spawn dispatch for these two patterns in this tested
sequence. It does not establish original-CPU trajectory parity, every conditional
rage-wave count, or contact/shot collision parity. Those remain open requirements
before #556 can move to awaiting player validation.

## Original CPU flame-coordinate comparison

The headless native Phantoon probe now executes original `$86:9BA2` for all
256 byte angles and 256 byte radii. It records DP `$14/$16` results rather than
calling the upstream C port. The complete local capture has SHA256
`2B3ECC25B05166235C9B5D3EE88A309D93F10F6A1E97E75652386175C0E37464`.
The temporary upstream entrypoint patch was reversed after capture.

`--phantoon-flame-coordinate-audit ROM CSV` reproduced 1020 mismatches among
65536 combinations. At angle zero/radius one, managed flame offset Y was zero
but original CPU returned -1. The shared enemy byte-sine routine used by the port
tops out at 255; Phantoon's `$86:9BF3` instead multiplies both bytes of a word sine
sample, preserving 256 at cardinal angles. A Phantoon-specific component helper
now follows that exact multiplication and whole-result negation. The common
helper remains unchanged for other enemies that genuinely use the byte routine.

After the fix, all 65536 original-CPU coordinate pairs match. Core verification,
real-room rain/spiral population and no-input transparency audits pass. The CSV
remains local; the committed probe regenerates it from the user's ROM. This
proves component-coordinate parity, not every spawn/lifetime/collision path.

## Rage-wave population and sound

The native `$A7:D8E0` DEY followed by CPY #8 / BPL still spawns direction eight.
The translation stopped at nine, losing one flame in each odd-numbered wave.
It also omitted the per-wave library-three sound `$29` at `$A7:D8E6`.
`--phantoon-rage-wave-audit ROM` reproduced all four seven-versus-eight count
failures and all eight missing sound publications before the production fix.

The corrected loop includes direction eight and publishes the native sound using
the existing lossless queue. The audit reads loop bounds, delays, count and sound
ID from actual ROM operands, checks the full initialized angle population, then
triggers a production 600-damage Super Missile eye hit in the actual room. Only
projectile contact is constructed; the following AI sequence is unmodified.
It produces waves at frames 1709, 1837, 1965, 2093, 2221, 2349, 2477 and 2605:
alternating seven/eight live flames, 128 frames apart, one sound request per wave,
followed by the normal fade-out. The first wave is four frames after rage entry.

Core verification and both real-room missile/Super Missile hit-fade branches
pass. This verifies queued sound, not a separate audible playback capture.
Projectile lifetime and collision parity remain outstanding; #556 is not yet
ready for player validation.

## Collision and lifetime completion

`--phantoon-flame-collision-audit ROM` uses production flame initialization,
constructed shot/touch positions, the common collision dispatchers and the real
enemy-projectile instruction pass. All sixteen cases pass without another
production change:

- Casual falling flames are neither shootable nor harmful at this stage.
- Rain/rage/spiral flames damage Samus for 40 without suits, request 96 frames of
  invincibility and five of knockback, and delete on contact. Exact touch edges
  miss; one pixel of overlap hits.
- Shots use the native 32-pixel-cell comparison, not ordinary hitbox overlap.
  A distant corner of the same cell hits; a neighboring pixel across its edge
  misses. Hits install the definition's shot list and clear shootability.
- Shot deaths display all four ROM sprite maps for five frames each without
  moving, then request exactly one drop and delete.
- Forty-eight rage/spiral trajectories (all direction indices, two body heights)
  delete on the first boundary crossing, at 26..96 frames, without drops.
  This checks lifecycle using the separately CPU-verified coordinate helper as
  its boundary predictor; it is not an independent second coordinate oracle.
- Real-room rain impacts at frame 76 for the chosen initial delay, displays its
  eight-frame impact image and four five-frame dying images, then deletes with
  no drop. The prior timing audit independently covers delay expiry.

Together with the real-room population/render captures, exhaustive original-CPU
coordinate comparison, ROM regional operands and production Super Missile rage
branch, this completes the reported rain/outward-pattern implementation checks.
The tests construct projectile contacts rather than claiming a controller-driven
whole fight; sound assertions verify queue handoff rather than audible output.
The issue is ready for player validation and must remain open until confirmed.
