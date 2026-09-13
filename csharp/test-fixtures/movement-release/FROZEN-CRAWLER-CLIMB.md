# Frozen crawler controller candidate (#444)

This is a managed connected-sequence fixture, **not completed original-CPU parity**.
It extends the existing Super-impact detachment tests with real Ice firing and
Samus movement. No enemy is manually frozen and no solid terrain is added at the
frozen enemy's position.

Commands:

```text
--frozen-crawler-climb-search ROM
--frozen-crawler-climb-audit ROM PRIVATE_OUTPUT_DIRECTORY
```

The constructed Landing Site collision layer uses the existing flat-floor helper,
a right wall at block X=18, and a ceiling at row 6 / columns 2..10. A retail Zoomer
at X=128 starts attached beneath that ceiling. Its first retail instruction tick
must run before holding its instruction timer: the initial $804D invisible map
correctly suppresses projectile collision and cannot test Ice hits. Falling,
frozen dispatch, projectile collision and solid-enemy support remain live.

Samus starts in the helper's default position with Ice equipped and ten Supers.
Fire a Super on frame 0, cancel selection on 13, turn left on 16, then hold Shoot
for four frames at the selected timing. Jump/Left on frames 60..109 moves onto the
frozen actor; subsequent neutral input observes support and thaw through frame
499. Gameplay cheats are off. All state seeding precedes frame 0.

The 26-case search (Shoot starts 20..45) finds 12 midair freezes (27..38). Focused
exports contain these adjacent boundary cases:

| Shoot start | Detach | Freeze | Frozen Y | Initial timer | Supported frames | Thaw |
|---|---:|---:|---:|---:|---:|---:|
| 26 | 11 | none | — | — | 0 | — |
| 27 | 11 | 41 | 218 | 400 | 315 | 445 |
| 38 | 11 | 47 | 242 | 400 | 315 | 451 |
| 39 | 11 | none | — | — | 0 | — |

Assertions check actual downward collision ownership, Samus's feet exactly at
the enemy top boundary, unchanged frozen 16.16 Y, removal of support after thaw,
and resumed downward travel. These numbers describe current managed evidence;
the connected sequence still needs original-CPU reproduction before #444 can
move to awaiting-player-validation. Existing native detachment/frozen-callback
tests do not substitute for that missing combined comparison.

MOV1 seeds and metadata preserve setup; JSONL records all 500 inputs, Samus pose,
position/animation/resources/camera, enemy position/health/freeze/flash/AI/list
state, crawler velocities/functions, support indices and projectiles. Outputs
contain ROM-derived state and stay private. No player save slot is touched.
