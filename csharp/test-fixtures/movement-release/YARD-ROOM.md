# Aqueduct Yard movement parity (#377)

`yard-room-native-capture.zip` contains `yard-room-377-v1.csv` (30,000 actor
frames), SHA-256:
`997AB174ED8FEF4FA04C76A42217DED6BF5042D77B469C9504BF0C6E0CE830AD`.
An independent recapture and a third run after extracting the shared bounded
CPU helper both produced the same hash.

Run `SuperMetroid.DebugRunner --yard-native-comparison-audit "Super Metroid.smc" yard-room-377-v1.csv`.
All 30,000 actor frames match after the two corrections below. The capture hash,
five focus cases, 1,200 frames per case and five ordered actor rows per frame are
validated, so a shortened or altered capture cannot pass.

## Reproduction

This uses the actual $8F:D5A7 Aqueduct room ($04/$21), state $D5B4, its decompressed
terrain/BTS, and all five $DBBF Yard actors—not an empty-room velocity formula.
Native initialization calls $82:DE6F/$82:DEF2/$82:E7D3 and
$A0:8A1E/$A0:8A9E. Each frame runs the real active-list builder $A0:8EB6 and enemy
dispatcher $A0:8FD4, including interaction, AI, animation instructions and draw
queue production. C# uses `RoomEnemySystem.Load/StepFrame`, including the
before-AI contact phase. No game code or ROM bytes are replaced by an oracle.

Five fresh cases follow each actor in turn with a camera centered 128/112 pixels
behind it (clamped at zero). Samus stays 64 pixels left of that actor's initial
position, at the same Y; health/max health are 999, standing radii 5/21. She faces
right on frame zero and left thereafter. The RNG starts at $0061. At frame 600
the fixture supplies the native super-missile earthquake signal (type $14,
timer $1E), then clears the timer again. This exercises observed hiding, crawl
transitions, detachment, airborne motion and actual terrain landings. It is a
room-local enemy diagnostic, not a controller playthrough or host-frame-rate test.
No player saves are used; no invincibility, speed cap or modified difficulty is
enabled. Existing kick/shot tests remain complementary coverage, not claims that
this capture simulates every moving-player encounter.

Per-frame comparisons include all five actors, even inactive ones: exact fixed
X/Y, properties, instruction pointer/timer, spritemap, all six slot variables,
direction/behavior, airborne X/Y velocity and the shared RNG. The input columns
record camera, Samus position/facing and earthquake state. Thus the test checks
actual trajectory, update timing and animation, not simply that enemies exist.

## Diagnosed causes and solutions

1. **Wrong live direction before the first direction instruction.** C# copied
   the population's initial orientation into Yard's live direction register.
   Native initialization uses that orientation for lists and velocity signs but
   leaves the register zero. A first-frame hide can bypass the direction-setting
   animation instruction, changing which axis is checked and whether the actor
   drops. Initialization now keeps these two concepts separate. The velocity
   initializer receives the population direction explicitly; later changes use
   the instruction-owned live direction.
2. **Attachment checks moved the hidden actor.** C# used ordinary collision
   movement for $A3:CF60's seven-pixel-plus-velocity look-ahead. The native
   $A0:BBBF/$A0:BC76 calls are read-only high-bit probes. C# could therefore
   displace a snail by roughly eight pixels while merely checking attachment.
   With the direction correction isolated, focus 0/slot 0/frame 1 advanced X
   from 1348 to 1356.25 where native remained at 1348 before dropping. Yard now
   calls the read-only probes; the vertical helper is shared with Stoke's
   existing two-pixel floor check. Raw solidity is exposed as
   `RoomLevelWord.HasSolidProbeBit`, without resolving slopes or BTS extensions.

The original full comparison had 25,216 mismatching actor frames. After the
direction correction (with population velocity signs retained), 7,627 remained.
After replacing movement with attachment probes, all 30,000 match. These are
reproduced movement defects related to the reported excessive-speed symptom;
player confirmation remains required before closing #377. Legitimate native
turn-list offsets and kicked horizontal speeds are not capped or slowed.

## Pins and verification

- Japan/USA rev 0 ROM SHA-256:
  `12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72`.
- `upstream-sm`: `578f90b3cc49557bb70060ad033bb90b8cf8ac50`.
- `upstream-disassembly`: `362be646929cf8e483f692b73a6561cfc2dc1d0d`.
- Pinned symbols: `Yard.direction`, `CheckForHorizontalSolidBlockCollision`,
  `CheckForVerticalSolidBlockCollision`, and Yard init/main at $A3:CDE2/$CE64.
- Existing Aqueduct hide/beam/drop/OAM audit passes. Core `--yard-trajectories`
  covers the 16 landing, 36 kick and 2,016 airborne arithmetic cases, plus retail
  crawl observations. Full core verification and the high-bit wrapper assertions
  also pass. No PAL/other-revision equivalence or visual-renderer parity
  is claimed from this movement capture.

Apply `native-yard-room-entrypoint.patch` in the pinned `upstream-sm` checkout
using `git apply --unidiff-zero`, build Release x64, then run
`sm.exe --diagnostic-yard-room "Super Metroid.smc" yard-room-377-v1.csv`.
Reverse the patch with `git apply --reverse --unidiff-zero` after capture.
It dispatches before SDL and redirects explicit errors to the console. The
shared CPU helper enforces an instruction budget. Temporary hooks are removed
after verification; the archive contains numeric diagnostics, not ROM/SRAM.
