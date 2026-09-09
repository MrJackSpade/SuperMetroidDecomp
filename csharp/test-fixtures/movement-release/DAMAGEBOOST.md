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
