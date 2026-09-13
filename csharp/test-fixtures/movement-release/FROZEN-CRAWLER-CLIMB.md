# Frozen crawler controller parity (#444)

This connected-sequence fixture matches **2,000 original-CPU frames** on the pinned NTSC ROM.
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
and resumed downward travel. All four rows now match the original CPU for every
recorded field, including beam lifetime and camera. This is a focused constructed
room, not a full-route or PAL claim; player confirmation remains outstanding.

## Original CPU replay

Include `native-release-probe.h` and `native-frozen-crawler-climb-probe.h` after
the StateRecorder declaration in the pinned upstream `sm_rtl.c`. Temporarily
dispatch `--frozen-crawler-climb ROM MOV1 OUTPUT_CSV SHOOT_START` from `main`
before SDL startup to `DiagnosticFrozenCrawlerClimb`. Build Release x64, run
each exported seed with its matching Shoot start, then compare using:

```powershell
pwsh -File compare-frozen-crawler-climb.ps1 -ManagedTrace crawler-27.jsonl -NativeTrace crawler-27.native.csv
```

Repeat for 26, 38 and 39. Remove the temporary entrypoint/includes and rebuild
the ordinary executable afterward. The probe executes original instructions,
not the translated C routines. It seeds Landing Site's actual 50-byte scroll
table and room-header vertical offsets; invented scroll cells/zero offsets
produce camera mismatches even when Samus and the crawler match.

The comparison first failed on an ordinary Ice hit: the port converted the beam
to a terrain explosion immediately, offset its X by eight pixels, and retained
the slot for 19 frames. The cartridge instead marks the direction word during
enemy collision and clears the beam on its next projectile pass. The shared
ordinary-enemy beam collision path now preserves that marker and ownership.
The comparator retains projectile and camera assertions rather than excluding
those initially failing fields. Enemy-owned hit graphics are separate from the
projectile slot; this state test does not establish rendered-pixel parity.

MOV1 seeds and metadata preserve setup; JSONL records all 500 inputs, Samus pose,
position/animation/resources/camera, enemy position/health/freeze/flash/AI/list
state, crawler velocities/functions, support indices and projectiles. Outputs
contain ROM-derived state and stay private. No player save slot is touched.
