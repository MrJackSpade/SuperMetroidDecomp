# Intro return jump (#511): failing reproduction

Run DebugRunner with the retail ROM:

```
--intro-return-jump-audit "Super Metroid.smc" OUTPUT_DIRECTORY
```

The initial reproduction commit intentionally failed on production code. It navigates
the first intro page, allows the real Rinka to hit Samus, records every subsequent
demo input/pose/position/animation frame, and captures every eighth post-hit frame.
Outputs stay local/ignored; do not publish ROM assets or captures.

Observed after the page-one confirmation (zero-based audit frame numbering):

- Frame 251: ROM publishes Left with a Left edge; Samus remains pose $02 at (204,115).
- Frame 259: ROM publishes Left+Jump with a Jump edge; same pose and position.
- Frames 260-266: Left+Jump held, still no movement.
- Frame 267: Jump released, Left held; still no movement.
- Frame 271 onward: no input; still (204,115).

The frame-272 capture visibly shows Samus still on the right platform rather than
performing the return arc. The diagnostic reports hit=True, requested=True,
rose=False and throws. This is distinct from the already-tested initial knockback
arc and fall back to the floor.

The ROM stream at $91:8694 falls through into the physically adjacent $86B8 input
records, including eight frames of Left followed by eight frames of Left+Jump.
The disassembly's unused label on that adjacent list does not make those bytes
unreachable by fall-through. Our DemoInputState does publish them correctly.

The missing production behavior is in StepMotherBrainFlashbackSamus: it dispatches
knockback, hurt-ending, and falling movement, but not ordinary ground input/run or
jump movement. The shared IntroSamusDemoMovement coordinator already handles the
other flashback's grounded-left family; any extension must preserve native alpha/
beta ordering and use shared jump transitions/movement rather than hard-coded
coordinates or a replacement cinematic arc.

No production fix is included in this reproduction commit. Before closing the
implementation, expand the assertion beyond this initial missing-rise detector:
verify return trajectory, spin/landing animation, timing, and recovered position
against the cartridge path. Issue remains open, not awaiting player validation.

## Shared-movement integration

The flashback now dispatches the shared grounded coordinator, ordinary jump
initializer, spin/normal-jump movement, and spin landing transition. Grounded
movement already animates, so the outer cinematic does not animate it twice.
The pose-history regression now covers the four newly executed run/jump/landing/
standing transitions as well as the existing hurt and scene-handoff transitions.

The real script now runs at 251, enters spin pose $1A at 259, reaches Y=84,
lands in pose $A7 at frame 294 at (149,115), and stands at 304. These are observed
port results, not asserted native golden coordinates. The expanded audit verifies
rise, descent, landing at the original ground Y, return to standing, and multiple
spin and landing animation frames. It renders every frame so tile-transfer
selection follows the host pipeline, saving only selected captures. Captures of
ascent, descent and landing were inspected locally. Nine spin animation frames and
three landing frames occur. The palette regression and full Verification pass;
Windows Release builds.

Remaining verification: independent original-CPU/emulator trajectory and timing
comparison. Do not claim exact native return coordinates from these observed
results alone. The missing movement is implemented, but #511 remains open without
the awaiting-player-validation label until that comparison is complete.

## Original-CPU comparison probe

`movement-release/native-intro-return-probe.h` executes the original ROM's demo
alpha ($90:E6C9) and intro beta ($90:E833), using a constructed post-hit starting
state and the retail collision data. It restores original ROM bytes after native
harness initialization. The bounded calls and headless entrypoint suppress GUI
error dialogs. No full native cinematic or earlier knockback is claimed.

Apply `native-intro-return-entrypoint.patch` inside upstream-sm with
`git apply --unidiff-zero`, build Release/x64, and run:

```
sm.exe --diagnostic-intro-return ROM NEW_OUTPUT_CSV
```

The output is created exclusively; use a new path for another run. Reverse only
that patch afterward, preserving other native-worktree modifications. The patch
was checked against the current native worktree; the probe built and ran. Its
temporary entrypoint edits were removed after the experiment.

Native frame zero corresponds to port audit frame 250. For the 80-frame bounded
comparison, the jump Y coordinates and pose timeline match. Spin animation and
landing timing match: spin at relative frame 9, landing at 44, standing at 54.
However, native X becomes 203 on frame 1 where the port remains 204; that one-pixel
difference persists through native landing X=148 versus port X=149. Added fractional
position and horizontal-speed columns to the port trace: at frame 250 its X is
204.0000 and base speed is zero, so a guessed initial fractional X is not an
explanation. Native setup/history and first-frame transition behavior still need
comparison before deciding whether this is a fixture or production defect. No
compensating offset was added. Keep #511 open pending that work.

## Native comparison completed

Stage checkpoints localized the missing pixel to native pose commit: X remained
204.0000 after alpha and after movement, then became 203.0000 during UpdateSamusPose.
That routine calls $91:EADE, the prospective-running wall check. Its unobstructed
one-pixel movement is retained by the cartridge. Gameplay already implements it as
`CheckProspectiveRunningPoseForWall`; the shared intro coordinator omitted the call.
The coordinator now runs that same collision-aware probe before installing the
prospective/fallback pose and honors its blocked-pose result. No offset workaround.

Add the native CSV as a fourth argument to the return-jump audit. It asserts exact
input, pose, X, Y, animation frame and animation timer agreement for all 79 native
frames from first run input through final standing. The initial constructed native
standing frame is excluded because its animation timer was seeded rather than
reproduced from the earlier Rinka hit. The first run transition initializes matching
animation state independently. All 79 frames now match; previously X differed on
every one. Native/port landing is (148,115), with spin at relative frame 9, landing
at 44, and standing at 54. Ascent/descent/landing captures remain local.

Full Verification, the real-hit palette audit, and Windows Release build pass.
Temporary native instrumentation and entrypoint edits were removed. This verifies
the return sequence against original CPU execution, not the complete prior Rinka
history or an emulator-video pixel comparison. #511 is ready for player validation.
