# Zebetite skip investigation (#442)

Status: **unfinished**. No production fix or native skip-parity claim yet.

Sources: [14% techniques](https://wiki.supermetroid.run/14%25#Techniques) and
[Mother Brain Room](https://wiki.supermetroid.run/Mother_Brain_Room).
The issue covers Ice-only Rinka placement without Wave/Screw, and an up-left
shinespark followed by escape inputs. Passage, enemy collision and recovered
control must be asserted separately from destroying a Zebetite.

DebugRunner `--zebetite-skip-audit ROM` is a room-local exploratory trace. It loads
the intact first Zebetite in room `$8F:DD58`, with no destroyed-barrier events.
Samus starts at (900,100), zero subpixels, facing left, 399 energy, Morph/Gravity
and Ice only, with cheats off. No state is edited after the first frame begins.
Inputs walk left for 60 frames, then hold Up+Shoot through frame 119. All three
cases freeze the lower Rinka at (823,166) through the real projectile system.

The cases then step right for 0, 8 or 20 frames, followed by Left and 24-frame Jump
holds separated by 12-frame releases. The trace records exact controller words,
Samus coordinates/subpixels, pose, health/invulnerability, and live barrier/Rinka
positions, properties and freeze timers. It asserts the setup's Rinka freeze,
**not successful passage**. The observed wedging could still be invalid alignment;
do not change collision code based on this trace alone.

Next: reproduce the relevant alignment/trajectory using original CPU routines,
find a successful setup and adjacent failure, then compare passage and subsequent
control. The shinespark half remains untested. No player save is loaded or changed;
keep generated logs and any future ROM/state exports private.

## Original-CPU collision interval

`--zebetite-skip-export ROM PRIVATE_DIRECTORY` captures frames 120..159 for each
case, after the lower Rinka has frozen. It runs both the complete room and a
counterfactual omitting enemies other than native slots 128 (Zebetite) and 192
(frozen Rinka), and clearing projectiles. The exported movement/pose/animation/
radii/health/freeze CSVs must remain byte-identical. All three omission checks pass.
This establishes the omission only for these fields and forty frames, not later
damage, respawn, artwork, audio, or successful passage.

The native consumer uses MOV1 room/movement plus ZSK1 supplemental data containing
NMI/RNG, pose history, health, camera, and the two exact 64-byte enemy records.
Include `native-release-probe.h` followed by `native-zebetite-skip-probe.h` in
`sm_rtl.c` and temporarily dispatch before SDL:

```text
--zebetite-skip ROM MOV1 ACTORS OUTPUT_CSV STEP_BACK_FRAMES
```

Pass those five arguments to `DiagnosticZebetiteSkip`; use offsets 0, 8 and 20.
Remove the temporary entrypoint/includes and rebuild the normal executable after
the experiment. Generated seeds contain cartridge data and must remain private.

The original CPU reproduces the wedged trajectory: all 120 frames agree on
positions/subpixels, pose, animation frame/timer, X radius, health and Rinka freeze
timer. The initial comparison found five Y-radius publication differences:

| Step-back frames | Frame | Managed Y radius | Native Y radius |
| --- | --- | --- | --- |
| 0 | 120 | 19 | 21 |
| 0 | 125 | 16 | 19 |
| 8 | 128 | 12 | 21 |
| 8 | 134 | 16 | 12 |
| 20 | 140 | 12 | 21 |

`--zebetite-skip-compare MANAGED_CSV NATIVE_CSV` checks every field and exits with
an error for any differences; they are not silently excluded from a parity pass.
Their effect on a successful skip remains unproven. Do not infer a collision fix
from wedging that the cartridge itself reproduces.

The collision-forced crouch path now matches `$91:FFA7`: read the target radius
for center correction without publishing it as the live radius. This corrects
offset 0/frame 125 and offset 8/frame 134. The same MOV1/ZSK1 seeds are byte-identical,
and comparing all 120 frames now leaves only the three jump-entry differences
listed above. Focused spin-to-crouch and compact-to-crouch tests check retained
radius, immediate center correction, and next-alpha publication separately.

Delaying ordinary jump-entry radius publication initially failed the intro-history
integration test: the scene did not complete its terminal/discovery handoff. The
intro was missing the per-frame radius refresh performed by the cartridge's
intro-demo alpha handler. Both cinematic owners now publish the radius before
movement, and the shared jump initializer leaves it unchanged at pose commit.
The intro test explicitly checks the running-to-spin commit radius, next-alpha
spin radius, and subsequent terminal/discovery handoff. Grounded, aimed, and
firing-landing jump tests distinguish commit from next-alpha publication as well.

With both fixes, all fields in all 120 original-CPU collision frames match. The
MOV1/ZSK1 seeds remain byte-identical to the original comparison. This certifies
only this collision interval, not successful skip execution or all jump routes.

`--zebetite-skip-repeat-jumps ROM` explores repeated step-back/jump cycles with
seven offsets. This remains exploratory: no successful passage assertion or
native comparison for that longer sequence exists yet. The next required work is
the successful alignment/escape setup and
the separate diagonal-shinespark method. #442 remains active.

## Bounded controller timing search

`--zebetite-skip-scan-jumps ROM` runs 633 room-local controller sequences with
the same initial fixture and real lower-Rinka freeze. The first 408 vary the
step-back duration (0..32, every two frames), Jump hold (1/2/4/8/12/24), and
release (1/2/6/12). Another 225 use offsets 0/4/8/12/16/20/24/28/32 and delay
Left for 0..24 frames after the first jump, with subsequent 24-on/1-off Jump.
Every case runs 360 frames, asserts the real freeze, and records minimum X.
No case crossed the candidate threshold X < 800. This is a failed setup search,
not evidence that the native skip fails or that the port's later collision is exact.

`--zebetite-skip-trace-jump ROM OFFSET DELAY` prints the delayed-steering case,
including health/invulnerability and live Rinka/Zebetite positions. The offset-32,
delay-24 trace shows retained rightward jump momentum before the Left input;
delaying steering is not equivalent to a stationary vertical hop. The documented
invulnerability/alignment step and subpixel normalization need a deliberate setup,
with an extended original-CPU comparison beyond the existing forty-frame interval.
No production behavior was changed based on these unsuccessful attempts.

## Constructed aligned escape and one-pixel neighbor

`--zebetite-skip-export-aligned ROM PRIVATE_DIRECTORY` first runs the same real
freeze setup. At frame 120 it deliberately constructs a crouched precondition:
X=836 or 837, Y=142, both subpositions zero, crouching left, invulnerability=120,
with animation/pose history initialized. This is **not** controller-earned alignment
or invulnerability. Other movement fields are retained from the stationary setup.
It exports the same private MOV1/ZSK1 pair and forty-frame CSV, with the existing
Left/Jump schedule (native probe offset argument 0).

The original CPU escapes from X=836 and remains blocked from X=837. At frame 159:

| Initial X | Final X fixed | Final Y fixed | Pose | Zebetite health |
| --- | --- | --- | --- | --- |
| 836 | 52060160 | 8226815 | falling left | 1000 |
| 837 | 54853632 | 8469504 | forward jump left | 1000 |

The complete and two-actor room intervals match each other in both cases. The
initial native comparison found five early Y-radius publications: crouch jump
at frame 120 in both cases, landing at 142/131 respectively, and walk-off at 146
in the successful case. Those three production transition paths now retain the
live radius until alpha. All 80 native frames agree on every CSV field afterward.
The export asserts these timing landmarks, exact endpoints, and intact barrier;
focused core tests distinguish immediate center correction from later radius
publication. The existing 120-frame native interval remains unchanged.

This proves escape from the constructed alignment, including movement after
leaving the barrier, and a one-pixel neighboring failure. It does **not** prove
how to reach that alignment from the preceding controller sequence. That setup
and the separate shinespark method remain required before #442 is ready.
