# Kraid left-hand death investigation (#520)

Affected version: 0.1.1. Still investigating; no production fix or confirmed cause.

Run `--kraid-death-capture ROM OUTPUT_DIRECTORY` through DebugRunner. This extends
the existing full-runtime Kraid defeat audit with rendered checkpoints and a CSV
of body/arm position, camera, properties, instruction, spritemap, and timer.
It uses the existing projectile-hit seam and seeded lethal HP, not a player route.
An input-locked observer is positioned beside the upper body after growth; 120
ordinary frames allow camera streaming to settle before the lethal hit.

The existing endpoint-only fixture was unsuitable for this visual report: its
observer reached camera Y=0 while the arm anchor was at world Y=251, outside the
224-line viewport. A lower-floor observer likewise put the anchor above camera
Y=255. Passing those encounter endpoints does not prove either hand's visibility.

The current capture has camera Y=144 and initially retains the arm visibility bit.
It records the death retraction and sinking phases. However, the composite scene
also has an unexpected background/body alignment in this constructed setup. The
capture must not be treated as a faithful reproduction of the player's hand report
or as evidence of a particular production rendering defect yet.

Next: compare the arm instruction/spritemap sequence with the original CPU, identify
which visible hand the report concerns (independent OBJ arm versus BG2 artwork),
and obtain a representative scene before adding a visual regression assertion.

`kraid-death-520-investigation.zip` preserves the current diagnostic checkpoints
and complete CSV so these observations are not lost. No player save was modified.
Build and full encounter audit pass; issue remains open without validation label.

## Centered capture follow-up

The observer now starts at world X=256 instead of 48 and the capture consumes
the same gameplay render packet as the layered renderer. The complete fixture
settles to camera (117,121), bringing Kraid's head and independent arm into view.
`kraid-death-520-centered.zip` contains the revised images and trace.

The OBJ arm is visible before death and retracts against the body during fade-out.
At death frame 17 it reaches map 92AB; frame 22 starts sinking and selects map
90FD. The native sources select retraction list 8AF0 at C360 and list 8AA4 at C4C8,
matching these managed instruction transitions. This is source cross-checking,
not yet a full original-CPU animation or pixel comparison, and does not identify
the player's left hand conclusively. No deletion-based fix is justified by these
frames.

Inspection also found a concrete separate omission relevant to #519: original
Kraid initialization writes scroll bytes [0,0,1,0], and C0A1 changes them to
[2,2,1,1]. The managed initializer lacks those writes and its growth code only
sets CameraReleasedForSecondPhase, which has no runtime consumer. Reproduce the
jump/camera trajectory for that issue before implementing the missing handoff.
Do not conflate this finding with a demonstrated cause of the #520 hand report.

## Capture after the camera fix

With #519's native camera integration installed, the optional observer consumed
the initial mouth-open cycle and timed out in AEA4 with ThinkingTimer=0. This is
not evidence of a stalled native timer: A7:AEE4 returns to idle without restarting
the timer, and a projectile hit triggers the next eye/mouth reaction. The existing
non-runtime audit already supplied that shot. The optional runtime capture now
does likewise through ResolveKraidProjectileHits when idle, instead of modifying
boss state to force an opening. Timeout errors now report phase, positions and
timers so a setup failure is diagnosable.

`kraid-death-520-after-camera.zip` preserves the resulting full-runtime sequence.
Camera is (168,82), body begins at (288,295), arm at (288,251). The actual visible
arm is present before death, changes through maps 8F59/92A1/92AB at frames 5/11/17,
and changes to 90FD at sink frame 22. Before-death and frame-32 images were visually
inspected: the arm retracts against the body rather than being immediately deleted.
The complete encounter/death audit and clean build pass.

This remains diagnostic work, not a production hand fix. Native CPU animation
comparison and conclusive identification of the player's left-hand component are
still outstanding; leave #520 open without awaiting-player-validation.

## Original-CPU arm comparison

The bounded `native-kraid-arm-probe.h` now executes unmodified cartridge routines
A7:B7BD (arm AI) and A0:C26A (instruction interpreter). It consumes the preserved
body/camera trajectory, starts from the captured pre-death arm state, and installs
native lists 8AF0/8AA4 at the recorded death-phase transitions. All 335 live frames
before population deletion match: X/Y, properties, instruction pointer, map, timer.
This includes the anchor-based visibility cutoff and the looping sink animation.

ROM SHA-256: `12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72`.
Native harness uses upstream-sm `578f90b3cc49557bb70060ad033bb90b8cf8ac50` and
restores the retail ROM bytes after initialization. The temporary entrypoint
suppresses dialogs; execution has an instruction budget. Its first run omitted
the outer enemy dispatcher's callback-bank byte and failed at the list loop.
That harness seed was corrected; the complete run then reported zero mismatches.
The temporary upstream entrypoint patch has been removed after use.

`kraid-arm-native-520-v1.zip` contains the complete original-CPU CSV (its source
filename ends in v2 because v1 was the incomplete harness attempt). The managed
comparison SHA-gates this fixture and compares every live row, including the
body/camera stimulus. After extraction, run:

```text
--kraid-death-capture ROM OUTPUT_DIRECTORY NATIVE_CSV
```

Rebuilding the oracle requires applying `native-kraid-arm-entrypoint.patch` to
the pinned diagnostic upstream worktree, building its Release x64 target, and
running the headless executable with:

```text
--diagnostic-kraid-arm ROM MANAGED_ARM_CSV NEW_NATIVE_CSV
```

Then reverse that exact entrypoint patch. Output creation refuses to overwrite.
The managed capture plus native comparison and clean build pass.

Limits: this verifies the arm AI/interpreter against original instructions, not
whole-encounter phase timing or rendered pixels. Recorded body/camera movement
and list activation frames are inputs, not independently generated native facts.
No production discrepancy was found in that sequence. Next compare the rendered
arm contribution/component visibility; the player report remains unresolved.
