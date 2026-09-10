# Elevator grab input timelines (#454)

The [contact matrix](ELEVATOR-GRAB.md) establishes exact one-pixel and parity
windows. This follow-up runs the production runtime through approach, landing,
activation-button timing and the previous-frame contact handoff.

## Reproduced defect

Before the fix, accepted case `0,0,0,0,124,8` throws on frame8 with
`Posture transition $00 -> $F3 is outside the crouch/stand ROM-table family`.
It is a down elevator, initial even parity, right-facing stationary Samus with
one pixel of overlap and the first Down press on frame8. The preceding odd scan
has published contact. Alpha samples a crouch request, then the elevator AI
activates and forces the forward-facing pose, but C# retains that stale request.

Cartridge MakeSamusFaceForward ($91:E3F6, writes at $E459-$E46B) clears pending
pose requests and commands. Command seven replaces beta with $90:E8EC, which
does movement/minimap/animation without ordinary pose transitions. The runtime
now clears its pending input/fallback requests when the enemy phase reports
DepartureStarted. This is native owner handoff, not a larger elevator trigger
or an exception suppression.

The failure was rechecked by removing only that fix and running the accepted v3
capture: same case/frame/exception. Restoring it makes all 110,312 frames match.
The full core suite and Release DebugRunner build also pass.

## Matrix

5,376 cases: up/down, two initial NMI parities, both facings, three approaches,
32 start centers (120..151), seven activation delays (7..13).

- Standing on the floor; standing 16 pixels above it and falling through the
  real movement dispatcher; walking while holding R for eight frames then
  releasing horizontal direction for a quick stop.
- Hold R throughout. Pulse the elevator direction every four frames starting
  at the selected delay. Failed Down inputs may crouch and prevent a retry;
  do not invent success or suppress that penalty.
- End each case at its first activation or frame23. This tests entry, not
  subsequent elevator travel or room transitions.
- Up/down success counts by standing/falling/walking scenario are respectively
  448/192/392 and 448/128/224. Adjacent failures are retained in the capture.
- Match each input, X/Y fractions, pose/movement, base/extra velocity, elevator
  status, and the combined native-equivalent contact word. Enforce SHA, all
  dimensions, case/frame order and terminal conditions.

The terrain is a constructed flat row16 with a one-tile pseudo-door pad at
column8, embedded in Green Brinstar's 64x192 allocation and real door metadata.
Unrelated PLMs/enemies are removed; one original $D73F actor is initialized at
(136,256). Both implementations use the same geometry, pose, health99, Morph
Ball equipment, no beams/liquids/cheats, and zero subpixels. No player saves.
C# executes StepFrame, including input sampling, enemy phase, beta and transitions.
Native executes actual ROM alpha/projectile stages, elevator AI, then beta;
on activation it uses command seven's reduced beta path. The native fixture
does not claim to recreate unrelated full-room draw/HDMA/other-actor behavior.

## Capture and reproduction

`elevator-grab-timeline-native-capture.zip` contains
`elevator-grab-timeline-454-v3.csv`, SHA-256:
`45C13B8344FAF161FA34AC3FC8E185235758118EA726EA5A1F6648A02CE3B466`.
Independent recapture is identical. Earlier v1 did not explicitly omit ordinary
beta stages after activation; v2 corrected that, while v3 expands delay coverage
to include successful downward walking approaches. Only v3 is accepted.

```
SuperMetroid.DebugRunner --elevator-grab-timeline-audit "Super Metroid.smc" elevator-grab-timeline-454-v3.csv
```

Apply `native-elevator-grab-timeline-entrypoint.patch` in pinned upstream-sm
using `git apply --unidiff-zero`, build Release x64, then run:

```
sm.exe --diagnostic-elevator-grab-timeline "Super Metroid.smc" elevator-grab-timeline-454-v3.csv
```

Reverse the patch afterward. The probe dispatches before SDL and suppresses
explicit GUI error dialogs while retaining console failures and a CPU budget.
Archive contains numeric diagnostics only. Pins match ELEVATOR-GRAB.md.
No PAL, renderer, final camera alignment or full room-transition claim.
#454 is ready for player confirmation; #11 is not closed or reclassified by this work.
