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

## Controller-earned approach: longer native mismatch

`--zebetite-skip-scan-turns ROM` adds 1,024 sequences with a leftward turn before
jumping (even step-backs 2..32, Left lead 1..16, holds 4/8/16/24, one-frame
releases), plus 800 sequences varying the end of the freeze wait (108..204 every
four frames, offsets 0/4/8/12/16/20/24/28, Left lead four, the same four holds).
All 1,824 valid setups froze the lower Rinka, but none crossed. An earlier wait
ending at frame 80 failed the freeze assertion and is not counted as a skip attempt.
The search logs the first crouched X=836/Y<160 alignment for each case, including
subpixels and invulnerability. Several reach Y=144.FFFF without invulnerability
on that particular frame, rather than the constructed Y=142 precondition.

`--zebetite-skip-export-approach ROM PRIVATE_DIRECTORY` preserves one such actual
controller sequence: the existing freeze setup through 119, Right 120..139,
Left at 140, then Left plus 24-on/1-off Jump from 141. No alignment or health word
is edited. It exports frames 120..219. Complete-room and two-actor CSVs match.
Run the native consumer with offset argument -1 for this hundred-frame schedule;
compare using `--zebetite-skip-compare MANAGED_CSV NATIVE_CSV 100`.

The native comparison fails in four fields, deliberately not masked:

| Frame | Field | C# | Original CPU |
| --- | --- | --- | --- |
| 197 | live Y radius | 21 | 16 |
| 217 | Y fixed | 9175039 | 9109504 |
| 218 | Y fixed | 8855551 | 8790016 |
| 219 | Y fixed | 8535040 | 8469504 |

Both versions enter left knockback at 197 with unchanged health, then recover and
start the next jump at 216. The remaining vertical difference begins on 217,
not at the initial alignment. Trace the knockback radius publication and neutral
jump collision/probe order before making production changes. Invulnerability is
not a CSV field here; matching knockback/health does not certify its exact timer.

The four differences above are now corrected. Humanoid knockback commits its pose
without publishing the new radius before alpha. For the neutral jump, native
`Samus_Move_NoBaseSpeed_X` still calls the solid-enemy probe with a zero magnitude.
The managed mover previously skipped that probe whenever displacement was zero,
losing its tangency write to Samus's fractional Y. The horizontal mover now accepts
an explicit direction for a zero-distance dispatch, and neutral jump movement
supplies its facing. This does not infer a direction for other no-movement callers.

All 100 original-CPU approach frames match afterward; the constructed aligned
80-frame pair remains matched. Focused tests exercise the actual neutral-jump
entrypoint in both directions, solid and frozen enemies, and a one-pixel miss.
They assert unchanged X/whole Y, the exact fractional-Y write, contact reporting,
and absence of vertical movement. A crouch-to-hurt test asserts delayed radius
publication. The turn/freeze search still finds no successful earned skip; this
fix does not complete the alignment or shinespark portions of #442.
# Controller-earned neutral-jump follow-up

## Room-local shinespark exploration

### Recovery interval and compact-pose radius

Follow-up: frame85's X divergence came from knockback omitting the ordinary
horizontal wrapper's collision momentum cleanup (`$90:E5CE`). The first clipped
move retained base speed, making the next clear move too fast. Knockback now
clears horizontal momentum immediately after a collided X move, before Y motion.
A focused two-frame solid-enemy fixture covers left/right clipping, zeroed speed,
and the next clear frame's exact 1.5-pixel displacement. All179 spark-recovery
frames now match the original CPU, and the earlier 240-frame Ice approach still
matches. This does not establish a successful skip; the matched recovery remains
an unsuccessful escape-frame90 attempt.

`--zebetite-spark-export-recovery ROM PRIVATE_DIRECTORY` exports frames0..178
for the escape-frame90 setup. Native probe offset `-4` runs that sequence and
the comparator accepts 179 frames. A preliminary 180-frame export found the
first full/isolated actor difference at frame179 (health/hurt state), so the
two versions are required to match through178 before using the isolated probe.
The native comparison initially differed at frame81: entering compact aerial
pose24 published radius10 immediately, while native retained19 until alpha.
The compact transition now retains the live radius, with direct tests asserting
both shrink and expansion timing and explicitly advancing alpha afterward.

The radius-only correction matched through frame84. Horizontal position still
diverged starting85 during knockback until the separate momentum fix above.
The post-crash 25-case escape scan still did not finish passage after the prior
crash fixes. Complete technique certification remains open.

### Native launch/crash comparison and solid-enemy stop fix

Follow-up: the native spark probe now executes `$91:D6F7` after pose transitions,
matching beta's palette stage. That stage expires the one-frame shine timer at
crash finish; omitting it falsely allowed a second native spark at frame79.
With the harness corrected, the only remaining differences were frame77 timer
and radius. Production crash finish had committed standing before AnimateSamus
and published its radius early. It now queues completion through its movement
result, commits standing at the post-animation transition seam, and uses the
prospective radius only to align the bottom/checkpoint. Next alpha publishes the
live standing radius. All 80 recorded frames now match the corrected native trace.
The exporter asserts the exact standing timer10/live radius19 at frame77, and
direct state fixtures explicitly execute the post-animation pose commit while
retaining their feet and camera-checkpoint assertions. Complete passage remains
unproven; this comparison covers launch and crash handoff, not the escape route.

`--zebetite-spark-export ROM PRIVATE_DIRECTORY` exports the first 80 frames of
the escape-frame-90 setup, with and without actors other than the first Zebetite.
Those managed traces match. Native probe offset `-3` loads the isolated movement
and actor seeds, supplies the same stored-shine words, and executes original CPU
routines. Compare using `--zebetite-skip-compare MANAGED NATIVE 80` (frames 0..79).

The first mismatch was frame 6: managed Y=10151935 versus native Y=10414079.
Native `$90:D1FF` returns on solid-enemy contact without adding the clipped
distance; the shared ordinary vertical mover advanced four pixels to the enemy
boundary. The shinespark path now performs that enemy probe explicitly and only
calls the terrain mover when clear. Focused solid/frozen tests assert exact Y,
zero accepted movement, crash initiation, and unobstructed upward movement.
The full core suite passes. Frames 0..76 now match every recorded field.

At that intermediate stage, four differences remained in the provisional probe:
frame77 animation timer and live Y radius, then frame79 Y and pose. The follow-up
above separates the missing harness palette stage from the production crash-finish
ordering defect and resolves those differences without hiding them.
Temporary native hooks were removed and the ordinary executable rebuilt.

`--zebetite-spark-audit ROM` separately explores the Speed Booster technique.
It constructs a stored shine at X=837.0000/Y=195.FFFF facing left in the intact
room, using the production store initializer rather than earning the charge in
the preceding room. Equipment is Speed Booster, Gravity, and Morph; health is
399 and host cheats are disabled. Holding angle-up plus jump selects diagonal
left launch, followed by down and alternating jump, then left to escape.

Twenty-five left-escape start frames (78..102) completed. The initial trace enters
diagonal motion on frame 2, crash on 6, crash echo circle on 46, finish on 76,
and inactive on 77. The Zebetite retains 1000 health. None of these timings
finishes beyond the barrier. These are exploratory observations, not expected
cartridge values: native launch/crash comparison and a successful passage with
subsequent control are still required. Stored-charge acquisition is explicitly
outside this room-local fixture, not claimed as validated by it.

## Extended original-CPU comparison

`--zebetite-skip-export-long-approach ROM PRIVATE_DIRECTORY` exports frames
120..359 of the unchanged twenty-right/one-left/repeated-jump approach. Native
probe offset `-2` executes the same sequence; the strict comparator accepts 240
frames for this interval. Full-room and isolated C# traces were identical, and
all 240 isolated frames matched the original CPU in every recorded field after
842e0a9f. Both remain blocked, so this failed input sequence is not evidence of
a managed movement defect. This does not certify the adaptive step-back recovery
search, which uses different inputs, nor successful Ice or shinespark passage.
Temporary native entrypoints were removed and the ordinary native build restored
after capturing the trace. Generated seeds and CSVs remain private.

`--zebetite-skip-scan-recovery ROM` additionally tests a fresh step-back and
spinjump after the first hit's knockback ends: four waits, twelve step-back
durations, and three jump holds (144 cases). On this build, 96 cases activated
post-hit recovery while invulnerable; none subsequently activated the neutral
alignment follow-up or crossed. This is not native certification of recovery.
The saved approach trace also shows the lower Rinka already at (823,166) before
becoming tangible, so the frozen whole-pixel position is not evidence that it
drifted away from its spawn. Extend the native recovery interval next; do not
infer a production fix from these unsuccessful input searches.

`--zebetite-skip-scan-neutral ROM` explores 128 controller-only combinations
(eight step-back offsets, four left-turn leads, four jump holds). If Samus reaches
the crouched X=836 alignment above Y=160 while invulnerable, it releases all input
for a frame, presses neutral jump, and then steers left. It never changes position,
pose, or invulnerability to produce that precondition.

On the post-842e0a9f build, all 128 cases completed, with zero crossings and zero
qualifying neutral-jump activations. Thus this experiment does not test successful
final-jump parity: the remaining obstacle is controller-earned alignment with
invulnerability, not a demonstrated failure of the neutral-jump sequence itself.
The earlier constructed successful alignment remains a separate, weaker claim.
