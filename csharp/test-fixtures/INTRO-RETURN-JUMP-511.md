# Intro return jump (#511): failing reproduction

Run DebugRunner with the retail ROM:

```
--intro-return-jump-audit "Super Metroid.smc" OUTPUT_DIRECTORY
```

This diagnostic intentionally fails on the current production code. It navigates
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
