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
