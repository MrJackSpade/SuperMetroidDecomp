# Damage boost parity — #472

Investigation started; not ready for player validation. No production fix is
claimed by this inventory. Parent #394 requires real dispatcher/input timing and
source-specific contacts, not only calling the damage-boost initializer directly.

## Source and current evidence

The [technique reference](https://wiki.supermetroid.run/Damage_boosting) describes
opposite-facing direction with Jump, source-dependent windows, forward-held initial
knockback, continued held-direction distance, morphed rejection and speed variants.
Treat those descriptions as hypotheses for the pinned Japan/USA cartridge documented
in SPINJUMP.md, not as constants to install without CPU reproduction.

Pinned `NormalEnemyTouchAiSkipDeathAnim` writes knockback timer five and chooses
horizontal knockback from Samus/source center comparison. The Mother Brain blue-ring
projectile contact in `sm_86.c` also writes five. This is not evidence that all
projectiles have the same effective input window as enemy contact: contact phase,
initialization and the final timer pass must be reproduced together. Spike handlers
in `sm_94.c` write ten. Do not replace projectile timer five with four solely from
the guide's window description.

The shared native hit-interruption path dispatches on movement type. Humanoid
initialization selects knockback poses; morphed initialization retains ball pose
and has different direction selection. `Samus_Input_0A_KnockbackOrCrystalFlashEnding`
performs input lookup and conditionally reinitializes jump/clears the knockback timer.
The transition slots and timer completion therefore need an exact frame-order probe.

Current C# has `SamusKnockbackMovement.Start`, `Step`, `ApplyDamageBoostTransition`,
and `SamusAerialMovement.StepDamageBoost`. Source timers are supplied separately by
enemy contact, projectile owners and `SamusTerrainHazardCollision`. The existing
`VerifySamusKnockbackAndDamageBoost` checks initialization, movement primitives and
direct transition calls. Its last-hurt-frame case deliberately supplies a captured
knockback source pose after the live pose has become falling, then calls the helper.
That does **not** prove the full runtime/native dispatcher actually accepts that input
at that time. Preserve the test as a helper contract until the sequence is measured;
do not use it as the native success-window oracle.

## Required comparisons

1. Establish equivalent hit initialization: both facings, each horizontal source side,
   neutral/forward-held contact, humanoid/ball bodies, and supported liquid media.
   Record pose, movement handler, direction, velocities, resource loss and timers.
2. Sweep opposite direction plus held/new Jump across the whole hurt lifetime and
   adjacent early/late frames. Execute real alpha/input, beta movement, animation,
   interruption, transition and timer ordering. Compare every frame, not just launch.
3. Repeat with actual enemy, projectile and spike/electric contact owners. A manually
   seeded timer cannot establish source-dependent window differences or damage.
4. Compare hold/release after boost and speedkeep/Speed Booster variants using exact
   base/extra speed words. Assert morph rejection and interruption exclusions.

Start with a flat, bounded synthetic room without unrelated actors or cheats, then
add one contact source at a time. Save deterministic fixtures without touching player
slots. Native and managed geometry must match, including ceiling/floor boundaries.
Keep helper-entry evidence separate from full-runtime/source-contact evidence.

## First original-CPU frame capture

`native-damageboost-probe.h` uses the existing unpatched-ROM loader. Include it after
`native-release-probe.h` in `sm_rtl.c`, dispatch `DiagnosticDamageBoost(rom, output)`
before SDL from a `--damageboost-probe` command, and build as described in WALLJUMP.md.
Output uses exclusive creation. Temporary upstream integration was removed afterward.

```
sm.exe --damageboost-probe "Super Metroid.smc" NEW_TRACE.csv
dotnet run --project csharp/src/SuperMetroid.DebugRunner -c Release --no-launch-profile -- --damageboost-comparison-audit "Super Metroid.smc" NEW_TRACE.csv
```

The bounded room has a floor at row 16, ceiling at row zero and solid side columns
zero/fifteen. Samus starts at X128/Y160 with zero fractions and speed, 99 energy,
Morph Ball equipped and no cheats. The initial hurt handoff is seeded, NOT caused by
a contact actor: timers 5/10, humanoid/ball, both facings, both horizontal knockback
directions, neutral/forward-held initial input, and boost-input delays 0..11. Each of
384 cases records initialization plus 30 frames, totaling 11,904 samples. Native
radius/input/gravity/movement/animation/hit-interruption/collision-transition/pose-
transition/collision-clear/timer routines execute in sequence. The managed comparer
uses the shared hit initializer and then production Runtime.StepFrame.

Current local capture `csharp/test-temp/damageboost-native-472-v2.csv` has all 384
initialized states matching. All 11,520 subsequent samples differ in at least one
category: 10,520 motion/pose/animation, 7,464 timer/direction/speed, 5,140 history.
The comparer deliberately returns failure. The earlier non-v2 capture omitted side
walls and is not the accepted bounded fixture.

First humanoid/right-facing/leftward/neutral/delay-zero frame has matching X/Y/pose,
but native retains timer4, knockback direction1 and Y speed4.E400; managed clears
timer/direction and installs speed4.E000. With delayed input, the first motion frame
instead differs only in older history: native shifts the current hurt pose into it,
managed retains the preceding standing pose. These are reproduced divergence leads;
no production fix is claimed yet. Timer expiry later also needs the real transitional
slot, rather than assuming the special movement handler alone owns cleanup.

This capture does not verify damage amounts, actor-source phase differences, media,
speedkeep, held-direction release variants, or all interruption exclusions. Those
remain required before #472 is ready for player validation.

Build environment note: during this capture the system .NET 10 installation became
unavailable and only SDK5 was registered. A SHA-512-verified Microsoft SDK10.0.401
archive was extracted under `csharp/test-temp/tools/dotnet-10.0.401`; invoking its
dotnet.exe built/ran the comparer without changing project targets or system SDKs.
The SDK/archive are local tooling, not tracked fixture assets.

## Humanoid hurt fallback correction

`--damageboost-hurt-prefix-audit ROM TRACE` selects the 192 humanoid initializations
and 880 frames before either the boost chord or timer expiry. It compares all fourteen
state columns, not only history. Before correction, precisely those 880 frames failed
only in history; motion, animation, timers and speeds already matched. The runtime now
publishes the same-pose fallback slot for ordinary hurt poses, allowing the existing
final transition epilogue to shift history once. Matched self-records still publish
nothing, and locked input does not enter fallback. All 1,072 selected samples now match.
The complete core verification suite also passes after this correction.

This is a partial fix for #472, not completion. The full 11,904-sample comparison still
has 10,640 mismatches: motion/pose/animation 10,520, timers/direction/speeds 7,464,
history 3,900. Do not substitute the prefix gate for the full acceptance scope above.

Further source cross-check: native `Samus_LookupTransitionTable` publishes a prospective
pose without changing live movement type. Thus `$91:8113` does not initialize a fresh
jump simply because that future pose is damage boost; `$91:F8CB` only restores the normal
movement handler. Also `$90:DF38` does not test the timer before moving: normal beta
moves and animates before `$90:DDE9` publishes expiry and `$91:F31D` clears velocity.
The port currently resets velocity/timer on boost entry and finishes active knockback
inside movement instead of at that later interruption seam. Both remain to correct,
including input priority on the expiry frame and the non-humanoid/cinematic callers.

## Boost entry and expiry correction

The initializer now preserves the hurt velocity, direction and timer; it only restores
normal movement. Expiry moved out of the special mover into the post-animation
interruption seam in both runtime and intro. An important refinement of the preceding
source note: `Samus_HandleTransitions` jumps directly to command one, skipping installation
of its proposed falling pose. Therefore expiry retains the current pose/animation,
clears velocity/direction, and consumes the ordinary alpha target. The following normal
type-$0A grounding probe publishes falling when unobstructed. The runtime previously
discarded that probe result. Its shared walk-off handler now admits this native source.
Neutral falling fallback also publishes its same-pose history slot.

After these corrections, all 5,952 humanoid samples match original CPU execution:
both facings/source sides, forward/neutral impact input, timers five/ten, boost delays
zero through eleven and the subsequent 30-frame motion. This includes the expiry-frame
input-priority distinction that the old direct-helper test could not establish.
Core verification passes, including the intro cinematic; its history assertion now
recognizes same-pose expiry as a transition. Direct tests verify retained hurt velocity,
movement on the zero-timer frame, and cleanup without immediate radius/pose substitution.

The full diagnostic still fails: all 5,760 post-initialization ball samples differ
(2,776 timer/direction/speed and 152 history mismatches). The first ball divergence is
horizontal acceleration: native uses the live pose's speed-table row, while the shared
port forces the humanoid knockback row. Ball fallback momentum also requires investigation.
No ball fix is included in this change. Keep #472 open without player-validation status;
source-driven contact windows, liquid media and speed variants remain outstanding too.

## Seeded humanoid and ball sweep now matches

The shared hurt mover now indexes horizontal speed from the live movement type, as
the native speed-table selector does. This fixes the first ball displacement mismatch:
0.C000 versus the wrongly forced humanoid 1.8000 in the retail fixture. A direct core
regression gives each admitted Morph/Spring Ball family a distinct synthetic record
and verifies that hurt movement consumes it rather than the humanoid row.

Stationary Morph Ball fallback now executes command six after movement, clearing
base/extra momentum even while hurt movement is installed. Airborne Morph Ball and
neutral crouch fallback also publish their same-pose history slot. The ten-frame
timer exposed an additional case: when unmorph animation completes on expiry, native
hit interruption changes command three to command eight, preserving the animation
transition while also clearing hurt state. Runtime and intro no longer let animation
completion suppress this cleanup.

The complete bounded capture now matches all 11,904 samples (384 initialized states
plus 30 frames each): zero motion/pose/animation, timer/direction/speed or history
differences. Both humanoid and ball subsets contain 5,952 samples. This supersedes
the preceding historical mismatch counts, but does not complete #472: all hits in
this fixture are still seeded, dry, and without speedkeep. Next acceptance work is
actual source-contact timing, liquid variants, held-direction release, and speed
variants as listed at the beginning of this document. Keep the issue open without
the player-validation label until those requirements are covered.

## Liquid and directional-release sweep

`DiagnosticDamageBoostVariant(rom, output, medium, release)` extends the same bounded
fixture without changing its 384 cases. Medium 0/1/2 means air/water/lava; liquid surface
is Y8 and native FX type is respectively 0/6/2. Release 0 holds the original chord;
release 1 keeps Jump but releases direction after three input frames. The probe appends
`medium,release` to its CSV; the comparer accepts both the original 22-field capture and
the new 24-field format. Temporary native main/include integration was removed after use.

For reproduction, dispatch the variant from a six-argument native command before SDL:
`sm.exe --damageboost-variant ROM NEW_TRACE.csv MEDIUM RELEASE`, parsing the final two
arguments as integers. Build as in WALLJUMP.md and compare with the existing managed
`--damageboost-comparison-audit ROM TRACE` command.

Accepted local captures are `damageboost-472-medium-0-release-{0,1}.csv` and
`damageboost-472-medium-{1,2}-release-{0,1}-v2.csv` under `csharp/test-temp`.
The non-v2 liquid captures are invalid: they set a surface but omitted native FX type,
so their animation differences must not be used as game-bug evidence.

With equivalent FX state, both water variants matched immediately. Dry release exposed
airborne-ball fallback failing to execute alpha-selected momentum command one/two after
movement. The shared speed state now performs the native post-movement speed recheck,
extra-momentum fold, mode selection and boost cancellation. Landing self-fallback also
now shifts history. Those changes remove the dry release's 592 differing samples.

Lava had 24 differing animation frames in each variant. Native command three adds the
FX buffer after target-pose animation initialization; the port omitted that final add.
The correction preserves the buffer from before pose initialization, since native uses
a local value when computing the target delay. A direct regression verifies the lava
turn endpoint timer is 3+2+2=7, not 5.

All six accepted captures now match: **71,424 samples with zero differences** across
all recorded state columns. This is still seeded-contact evidence, without Gravity
Suit, speedkeep, or boundary-crossing liquid entry. Actual enemy/projectile/spike contact
windows and the remaining speed/interruption variants are outstanding for #472.

## Normal enemy contact publication

`native-enemy-contact-probe.h` executes the original `$A0:A4A1` callback using the
retail Ripper header, without invoking the later hurt interruption. Include after the
shared native loader and dispatch `DiagnosticEnemyContact(rom)` before SDL. Temporary
integration used `sm.exe --enemy-contact-probe ROM` and was removed afterward.

Native output for both facings: health94, timer5, invincibility96, knockback direction0,
normal movement handler A337; standing pose remains 01 or 02. The corresponding
production public Ripper-contact fixture originally returned pose53, direction2 and
active hurt movement instead. `--enemy-contact-phase` reproduces this boundary through
the existing synthetic enemy loader/contact path, using equivalent header damage.

The common enemy touch publisher no longer invokes hurt interruption immediately.
It leaves the request for the runtime's existing post-animation handoff. The focused
comparison now matches, and its pose/handler/source-direction assertions also run in
the default Ripper regression. This is a publication-order fix, not a complete input-
window measurement: native GameState_8 orders alpha before EnemyMain, then beta and
projectile processing. The managed runtime's broader alpha/EnemyMain placement still
needs a full contact-frame comparison, along with enemy-projectile and spike sources.

## Generic projectile contact: damage fixed, phase still failing

The native contact probe now also executes `$A0:9923` for a nonpersistent projectile
using the retail fireball definition and damage20. Normal/Varia/Gravity outputs are
health79/89/94, timer5, direction0, pose01, normal handler A337, and deleted projectile.
The managed public `StepEnemyProjectiles` fixture originally produced health79 for
all three suits and immediately installed pose53/direction2/hurt movement.

Generic projectile damage now shares cartridge suit reduction with normal enemy touch.
`--projectile-contact-damage` passes, and runs in the default core suite. The stricter
`--projectile-contact-phase` deliberately remains failing to preserve the timing defect.
It must not be replaced by the damage-only gate when assessing #472 completion.

The runtime currently calls `StepEnemyProjectiles` before its Samus phase, while native
GameState_8 runs projectile instructions after beta and generic projectile collision
after PLMs. The combined managed method also initializes hurt immediately at the end
of collision. Removing only that initializer would still let the current frame's beta
consume a request which native does not publish until afterward. The remaining fix must
address this phase ownership together, with full frame comparisons and affected
projectile/PLM consumers covered. No projectile timing fix is claimed by this commit.

## Full runtime contact-frame reproduction

The native probe now captures three frames on a flat floor with an inert projectile.
Alpha/beta run before contact on frame zero; timers decrement last. Health is 79
throughout. Expected (X fixed, Y fixed, pose, timer, direction):

- Frame 0: (00800000, 00EBFFFF, 01, 4, 0)
- Frame 1: (00800000, 00EBFFFF, 53, 3, 2)
- Frame 2: (00818000, 00E6FFFF, 53, 2, 2)

`--projectile-runtime-phase` uses equivalent constructed terrain in the real runtime,
removes unrelated Ceres actors/arrival state, and compares these exact tuples plus
health. All three frames fail: managed immediately moves to X00818000/Y00E60000/pose53
on the hit frame, then moves again on each following frame. Thus hurt movement begins
two frames early. The native floor snap's FFFF fraction is intentionally asserted.

No production change accompanies this reproduction. Moving the combined projectile
routine alone is insufficient: instructions belong before PLMs, collision after them,
and the current runtime commits Samus transitions below its PLM block. The correction
must separate these owners and preserve projectile-spawn/PLM interactions. This trace
is the first full-frame gate, not a claim of complete source-window coverage.

## Projectile phase correction

The three-frame reproduction above is now green and runs in the default core suite.
Projectile instructions execute after completed Samus movement/pose history, followed
by PLMs, then projectile/Samus and projectile/projectile collision. The ordinary
Power Bomb collision pass follows those projectile passes. Projectile contact only
publishes the timer and source direction; the next Samus phase installs hurt movement.
The standalone combined helper retains that deferred-request behavior rather than
performing an extra Samus phase. All three suit cases now assert this as well as damage.

This fixes the demonstrated two-frame early movement, not all of #472. The seeded
movement sweeps do not cover every source collision, simultaneous transition, or
medium boundary. Native alpha/enemy ordering and other contact-source timing still
need separate examination before the issue can be marked ready for player validation.

## Live projectile input-window sweep

`DiagnosticDamageBoostSource(rom, output, medium, release, contact)` adds a live-contact
mode (`contact=1`) to the native probe. It skips seeded hurt initialization and runs
the original `$A0:9894` overlap scan after the first Samus phase with an inert fireball
at X120 or X136, Y160 and radius8. The managed comparison installs the equivalent actor
and runs the complete production frame. Health is now recorded alongside all existing
movement, timer and history words. Legacy 22/24-column traces remain readable.

192 cases per capture cover both facings, source sides, humanoid/ball bodies, prior
forward/neutral input and twelve opposite-direction-plus-Jump delays. Initial state
plus thirty frames gives 5,952 samples. Air/water/lava, each with held direction or
direction released after three input frames, all match: 35,712 samples, zero mismatches.
This is real overlap/contact publication, not `SamusKnockbackMovement.Start` setup.

To regenerate, use the temporary headless integration described above, dispatching
`DiagnosticDamageBoostSource(argv[2], argv[3], atoi(argv[4]), atoi(argv[5]), 1)` from
`--damageboost-contact ROM NEW.csv MEDIUM RELEASE`. The comparer command is unchanged.
The header exclusively creates output; use new paths rather than overwriting evidence.

Accepted local captures: `damageboost-contact-472-air-held.csv`,
`damageboost-contact-472-m0-r1.csv`, `damageboost-contact-472-m1-r0.csv`,
`damageboost-contact-472-m1-r1.csv`, and `damageboost-contact-472-m2-r{0,1}-v2.csv`.
The first lava capture omitted `$90:E9CE` periodic damage and is not a valid health
oracle. The corrected probe includes that native call; both lava traces then agree.
Temporary native executable hooks were removed after capture.

Remaining scope still includes actual enemy and spike contacts, speedkeep variants,
interruption exclusions and crossings between media; do not label #472 complete
based solely on the seeded and projectile traces.

## Spike-air contacts and Morph Ball release correction

Native `contact=2` installs a spike-air BTS2 block at X8/Y9 or Y10 and calls the real
`$94:9B60` inside-block dispatcher after input/gravity, before movement. Here the
source column selects upper/lower contact rather than projectile side. The managed
fixture installs identical terrain and leaves contact to the production runtime.
Use the same headless command setup as above with `--damageboost-spike` dispatching
`DiagnosticDamageBoostSource(..., 2)`. Both facings/bodies, initial inputs and twelve
boost delays again produce 5,952 samples per medium/release pair.

The first air/released-direction capture reproduced 216 mismatched samples. At frame
three, a rolling Morph Ball with base speed 0.4000 selected stationary pose41 on CPU
but retained rolling pose1F in C#. The shared runtime fallback incorrectly assigned
running's momentum command one to grounded Morph Ball. Native movement type four
selects command six, installs definition fallback immediately, and clears base/extra
momentum after that frame's movement. Spring Ball type eight still uses command one.

The runtime now distinguishes those commands. `VerifyMorphedSpikeRelease` runs seven
exact CPU-derived position/pose/base-speed tuples plus health and hurt timer in the
default core suite. All six `damageboost-spike-472-m{0,1,2}-r{0,1}.csv` comparisons pass
(35,712 samples). The diagnostic integration hooks are removed after capture.
Actual solid spikes, ordinary enemy contacts and the previously listed variants are
not covered by this inside-block fixture; #472 remains open.

## Solid-spike floor contact

Native `contact=3` replaces row11 with solid spikes; source0 selects BTS0 (60 damage),
source1 selects BTS1 (16 damage). Samus starts one collision step above the floor at
Y155 humanoid / Y169 morphed, so the actual movement dispatcher publishes the hit.
No direct hazard callback or seeded knockback is used. The managed fixture installs
the same floor and runs Runtime.StepFrame. The twelve input delays include Jump held
on the collision frame and through the subsequent hurt window.

All six air/water/lava and direction-held/released captures match: 35,712 samples
including exact health, positions, pose/animation, timers, speeds and history.
Local evidence is `damageboost-solid-spike-472-m{0,1,2}-r{0,1}.csv`. Regenerate using
the existing headless integration with `--damageboost-solid-spike` dispatching
`DiagnosticDamageBoostSource(..., 3)`; the comparer is unchanged. Hooks were removed.

No production correction was necessary for this floor-contact slice. Horizontal
spike collision, ordinary enemy contact, medium crossings, speedkeep and interruption
variants are not established by it. The overall #472 issue remains open.

## Ordinary enemy overlap and contact window

Native `contact=4` places a stationary retail Ripper at X120 or X136, Y160 with its
8x4 contact radius and live right-facing spritemap. `$A0:A07A` runs after alpha and
before movement, performing the real radius/invincibility checks and dispatching
the retail touch callback. The managed fixture uses the retail definition in a live
enemy slot and leaves overlap/contact to Runtime.StepFrame/EnemyMain; it does not
call the damage or hurt helper. Animation is held, velocity zero, and other actors
are removed to isolate Samus's timing. The native fixture likewise omits unrelated
actor drawing/animation and does not represent a full ROM gameplay recording.

All six `damageboost-enemy-472-m{0,1,2}-r{0,1}.csv` captures match (35,712 samples),
including real five-energy contact damage in air, combined periodic liquid damage,
both facings/source sides/bodies, twelve delays and held/released direction. No new
production fix was necessary. Regenerate with the headless `--damageboost-enemy`
dispatch to `DiagnosticDamageBoostSource(..., 4)`. Temporary hooks were removed.

This covers the ordinary Ripper contact path, not enemy-specific grabs, contact-damage
attacks, moving-source edge cases, medium crossings or speedkeep variants. The earlier
native alpha/enemy ordering concern remains relevant to those interacting cases even
though these isolated ordinary-contact trajectories match. #472 is not complete.

## Forward input through contact and hurt landing

The prior contact traces used `forward` only for the initial pre-contact input latch;
they did not hold it until the boost delay. New 27-column traces explicitly record
`holdForwardUntilBoost`. The native source modes now hold forward for frames before
the selected boost delay; legacy traces retain their original input validation.
Use `--damageboost-source ROM NEW.csv MEDIUM RELEASE CONTACT` with the temporary
headless dispatch to `DiagnosticDamageBoostSource` to regenerate.

This extension reproduced two landing defects and led to production corrections:

- `damageboost-forward-472-c2-m0-r0.csv` initially had five mismatched samples. After
  hurt expiry above the floor, normal type-$0A's grounding probe reached the floor but
  did not enter the shared landing selector. Runtime now publishes that downward hit
  to the existing landing presentation/pose/command-five path. The trace is green.
- `damageboost-forward-472-c3-m0-r0.csv` then exposed 76 vertical-state mismatches.
  Movement in landing art reset Y words unlike native standing movement. Removing
  those extra resets preserves direction two written by a later hurt expiry. This
  trace is green. Focused default tests check both exact landing position/pose/momentum
  and preservation of post-landing vertical direction.

The complete core suite and all six legacy seeded traces (71,424 samples) pass.
The expanded matrix is NOT all green: `damageboost-forward-472-c3-m0-r1.csv` currently
has 248 pose-history-only mismatches, beginning after landing or a later jump, with
movement/pose/animation, velocities/timers and health matching. This is retained as
the next failing diagnostic; do not mark #472 ready or weaken the history comparison.
The remainder of the forward-contact matrix must finish after that discrepancy is
resolved. Temporary upstream hooks were removed; no player save slot was changed.

## Retained-pose history correction

The 248 mismatches above came from omitted self-transitions for standing and spin-jump
definition fallbacks. `$91:82D9` still writes the current pose when definition byte two
is FF; `$91:EB88` therefore shifts pose history even though the visible pose is unchanged.
Runtime now records those native fallback selections. The continuation of the matrix
then reproduced twelve equivalent mismatches in underwater neutral-jump transition
poses4B/4C; those verified fallback entries are also recorded.

Direct default regressions check both facings of all three families, preserving the
visible pose while requiring history to shift. Both previously failing traces are green.
The full 24-capture `damageboost-forward-472-c{1,2,3,4}-m{0,1,2}-r{0,1}.csv` matrix
now passes: 142,848 samples with exact position, animation/pose, timers/velocities,
history and health. The complete core suite passes as well. Legacy captures remain
supported and their original input semantics are not rewritten.

This completes the currently constructed forward-contact matrix, not #472. In
particular, all these fixtures lack carried running speed and Speed Booster; speedkeep
variants remain an explicit requirement. Enemy-specific interruptions and other
earlier-noted interactions are not inferred from this successful stationary-source gate.

## Carried-speed hurt fallback

Contact modes 5/6 extend the real Ripper overlap fixture with base speed 1.4000,
extra speed 2/7, and a set momentum flag. Humanoid bodies start in spin pose;
balls start moving. Mode 6 equips Speed Booster but leaves the boost counter zero.
These are constructed retained-speed states, not a run-up proving an active blue
Speed Booster entry. Inputs, medium and delay sweeps remain unchanged.

The first air/held mode-5 capture reproduced 2,174 mismatching samples out of
5,952, with no initialization mismatch. Neutral hurt fallback retained pose/history
but omitted command two at $91:ECD0: reset acceleration mode and cancel running
momentum. The numeric extra speed survives that command until the next normal
movement call processes the cleared momentum flag. Runtime now executes both
operations after movement. The default core regression checks the three native
frames spanning fallback, boost selection and normal movement, including exact
X/Y, base/extra speed and momentum cancellation.

All six mode-5 captures and the four air/lava mode-6 captures now match exactly
(59,520 samples). Mode-6 water remains deliberately failing: held has 16 mismatches
and release has 12, first diverging on late aerial-turn completion near the floor.
This is not a completed speedkeep audit and #472 must remain open without the
validation label. Captures are `damageboost-carry-472-c{5,6}-m{0,1,2}-r{0,1}.csv`;
the comparison retains its complete state assertions. Temporary native entry-point
hooks were removed, and no player save slot was changed.

## Underwater turn/floor collision correction

The remaining mode-6 water failures reproduced at frame 26: the body touched
the floor while its falling-turn animation finished. Native retained Y speed
0.A000, but C# zeroed it, changing the following landing frame and pose history.
The old turn mover incorrectly implemented an unconditional collision-owned
velocity reset. The cartridge instead clears the collision-to-pose request at
$90:A7A8/$A7C5; neither turn family dispatches landing commands/presentation.
The shared upward mover still performs its immediate ceiling-stop writes.

Removed the extra reset and suppressed the semantic landing/ceiling-pose flags
on the returned turn result, retaining the physical collision result. A default
runtime regression seeds native frame 25 and asserts the exact contact X/Y,
retained velocity, completed turn pose, absence of a landing publication, and
the following normal falling frame's actual landing position/pose/zero speed.
Both previously failing water traces now match without weakening comparisons.
This finishes the constructed carry matrix, not the full technique contract:
legal run-up/active-boost entry and the source-specific interactions noted above
remain unproven.

## Broken-turret electric block reaction

Contact mode 7 exercises the distinct $94:8F0A routine, identified in pinned
bank_94.asm as Draygon's broken turret (solid spike BTS 3). The constructed floor
uses that behavior across row 11; starting positions and the input/medium sweeps
are identical to mode 3. No enemies or carried-speed seed are installed. The
legacy source column is retained for format compatibility and duplicates this
mode's cases; it does not select a second turret variant.

All six `damageboost-turret-472-m{0,1,2}-r{0,1}.csv` captures match exactly:
35,712 frame samples of position, pose/animation, speed, hurt words, history and
health. For the dry, right-facing, forward-not-held humanoid case, delays 0..9
enter damage boost and delays 10/11 do not; health falls from 99 to 83. This
confirms the collision reaction/window, not the room actor's transformation into
a broken turret or grapple electrocution. No production fix was needed.

## Controller-driven runway and retained boost protection

Modes 8/9 start standing with zero speeds on a constructed 144-block-wide runway
using Landing Site room metadata (native 144x80). Both facings start 128 pixels
from their respective edge. Mode 9 equips Speed Booster, mode 8 does not. Hold
forward+Run for frames 0..123; add Jump at 124; inject one inert projectile at
frame 128 at the prior position +/-16 X, same Y, radii 16. Opposite+Jump starts
at 129+delay (0..11). The projectile contact opportunity is one frame only; a
missed stimulus is removed rather than executing a nonexistent animation list.
Each capture contains 48 sequences of 161 samples including initialization.
No speed/position recentering is done during the run. Cheats remain disabled.

The ordinary run capture `damageboost-runup-472-c8.csv` matches all 7,728 frames.
The Speed Booster capture is `damageboost-runup-472-c9-v2.csv`; do not use the
earlier c9 capture as the accepted probe version because it omitted beta's
contact-damage reset at $90:E725 (the resulting recorded samples happen to match).

Speed Booster exposed two separate discrepancies. The gameplay one was the
missing common $90:9813 contact-damage epilogue on released/airborne/liquid
branches of HandleExtraRunSpeed. Native republishes index one while stage four
remains active; C# returned early, so the projectile damaged Samus. Restoring
that epilogue removes all health, motion-state and history mismatches. Direct
default tests cover airborne/released/liquid paths and the adjacent inactive stage.

The remaining 624 mismatches are animation frames beginning at run frame 100.
Native $90:85A6 calls QueueSound_Lib3_Max6 then uses its returned accumulator's
high byte at $90:85AA as the delay/reset-table index. The pinned C decompilation
explicitly comments out this cartridge bug; the C# port currently uses the
unclobbered stage instead. Queue occupancy and suppression affect that result,
so a hardcoded replacement index would be wrong. The probe currently does not
drain native sound queues through NMI. Keep the animation assertion failing
until queue-state-dependent behavior is translated and the comparison's audio
context is explicitly matched. #472 is not ready for player validation.
