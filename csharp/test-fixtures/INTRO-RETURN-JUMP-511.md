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
