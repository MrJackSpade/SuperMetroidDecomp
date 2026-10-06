# SMV gameplay parity without hardware lag

The project targets functionally exact cartridge gameplay while eliminating SNES
hardware lag. The user explicitly chose this policy: the port's smoothness is a
valued improvement. It applies to future recordings as well as the current
10,890-frame Ridley movie.

## Units of comparison

An SMV video-frame count is not a gameplay-update count. A native update can span
multiple video refreshes, and a coroutine can deliberately wait for another NMI.
Neither repeated inputs nor unchanged positions prove that a frame is disposable.

Compare the complete ordered sequence of gameplay updates and the inputs actually
consumed by those updates. Preserve authored timers, animation progression,
intentional waits, button presses/releases, and state transitions. Do not require
matching hardware-dependent rendering/CPU stalls or elapsed wall-clock time. This
policy does not permit injecting later reference state to repair a diverged replay.

## Converter contract

A reusable SMV converter is the intended diagnostic tool. Its output is a separate
port replay, not a replacement for the original SMV or a promise that a shortened
SMV will replay identically in a SNES emulator.

The converter must:

1. Verify and retain the original SMV and ROM identities and emulator/capture
   revision. Never rewrite the source movie.
2. Obtain an instrumented native trace of completed updates and actual input
   consumption, including input reads during nested transition/pause coroutines.
   Video-boundary WRAM captures alone are insufficient when the CPU is partway
   through a routine.
3. Map each replay update to its original SMV frame or frame interval. Retain
   meaningful NMI waits. Exclude only evidenced hardware stalls; do not delete
   samples simply because their buttons or screen contents repeat.
4. Preserve held inputs and press/release semantics. If a single controller mask
   per port update cannot represent the native consumption sequence, report that
   mismatch and extend the replay format rather than silently dropping events.
5. Emit a reproducible conversion manifest with input hashes, source coverage,
   update count, timing classification, and the source-to-update map. Refuse
   unsupported or ambiguous trace boundaries instead of guessing.
6. Initialize the port once from the movie's starting state, then run production
   gameplay using only the converted input events. Expected native state remains
   read-only comparison data.
7. Compare all relevant gameplay properties through the recording's end. Record
   the first divergence with both source video-frame and port-update identifiers.
   Matching a short interval or a few selected fields is not full-movie parity.

## Current evidence and remaining work

### Coverage audit (current)

The complete 10,717-update input-only replay passes the currently implemented checks.
This is an end-to-end baseline, not proof that every gameplay owner is covered.

| Contract | Current evidence | Remaining work |
| --- | --- | --- |
| Source identity, immutable movie, input edges, lag classification, terminal boundary | Converter v3 and complete replay pass | Retain these checks with each coverage expansion |
| Samus movement and collision | Positions/fractions, camera, pose/history, animation, primary speeds, radii, gravity, external displacement, slope enable, speed divisor, contact mode, bounce/bomb-jump state and momentum/boost pass | Control-handler ownership, remaining movement flags and secondary motion state |
| Inventory and firing | Inventory/capacities, fractional health, hurt/immunity/knockback state, HUD selection, projectile/bomb counts, prior charge, firing immunity, shot-direction publication, charge palette/audio words, retained held/press samples, auto-jump timer and pose-input handler pass | Remaining combat/input ownership |
| Enemies and Ridley | Active enemy collision/properties/timers; complete Ridley tail records, shared tail/body controller, wing/grab/damage/health/facing state pass | Remaining actor AI variables and phase-dependent aliases |
| Ordinary/enemy projectiles | Ordinary active-slot motion/program/art and callback identity; enemy-projectile motion/program/callback state and encountered rendered compositions pass; all bomb slots and explosion activation owners remain inactive throughout this movie | Final render composition/pixels; bombs need live mappings only if a different movie activates them |
| Persistent world and room effects | Complete boss/event/item/door bitsets; gameplay foreground/BTS; movie grey-door execution, condition and hit state; acid surface/tide phase pass | Loading-boundary mutations, remaining persistence allocations, environmental state, and PLM draw/presentation state |
| Pause, presentation and audio behavior | Recorded gameplay transitions pass current fields | Explicit inventory of remaining state and observable output coverage |

The history below records earlier divergence boundaries, not current completion.


The original Ridley movie was played through all 10,890 video frames in Snes9x
1.60. The local native trace contains 10,891 WRAM records, including the initial
state and terminal record. Its source movie SHA-256 is
`7E12861DC56C5ABED12C2BFA2B00D24BFA418F49F2CE4C027D930CE9A3663F66`.

The reusable converter is `tools/convert-smv-updates.py`. Its native capture adapter
and build instructions are in `tools/smv-native-capture/`. The instrumented complete
movie produces 10,758 accepted input intervals: 10,655 outer main-loop dispatches
and 103 NMI continuations. Accepted input is not by itself a gameplay update.
Manifest v3 records native APU-upload and door-scroll evidence: 61 continuations
change the scroll counter, 41 are hardware APU waits, and one completes the
scrolling coroutine without a counter change. Both scrolling groups remain.

The 41 hardware intervals are source frames 294-334. Native `$80:8028` sets WRAM
upload flag `$0617` around `SendAPUData`; the door IRQ keeps requesting NMI at
`$80:9823` even after scrolling finishes. V3 folds these neutral-input intervals
into the preceding outer dispatch and compares its completed checkpoint. It
retains an audit record for every omitted input, plus source/record mappings and
the cumulative excluded NMI count. The verifier subtracts that count only when
comparing the reference's NMI bookkeeping; no runtime state is injected. Any
derived gameplay-state mismatch still fails normally.

Normalization requires the native door music-wait dispatcher, completed scrolling,
no changing scroll counter, no outer dispatch in the omitted interval, and neutral
input throughout. Non-neutral input, overlapping owners, an unfinished terminal
upload, or changed retained input edges fail conversion rather than being guessed.
The original movie now converts to 10,717 replay updates: 10,655 outer dispatches
and 62 continuations. It excludes 132 refreshes without accepted input and 41
proven upload intervals. Every original checkpoint and input edge is validated
through the terminal state; ROM and SMV hashes remain unchanged.

The full-replay diagnostic imports state once and compares Samus position,
subpixels, movement speeds, animation, health, accepted NMI, RNG, dispatcher state,
room identity, and active enemies' identities, positions, health and visual cursors.
That comparison is still under development; conversion success is not port parity.
Door-entry and source-fade HDMA/RNG/actor omissions were reproduced and fixed,
along with missing RNG advancement through the outer loading dispatches. At that stage, the
checked properties matched through update 3036, with loading-owner alignment
and upload normalization as described here. Setup now applies
Samus's first displacement before destination rebasing; the atomic loader retains
the pre-setup source coordinates so it does not count that movement twice. A
focused failing-then-passing regression confirms native `$0013.5800` at setup,
the complete remaining IRQ trajectory, and the unchanged `$00D8.2000` final
position. All four trajectory checks, native camera alignment, and door autosave
continuation checks pass.

Destination placement now rebases both whole position words before the final
nudge, publishes the first moving IRQ while tiles load, and carries that progress
through the atomic room constructor instead of restarting it. The focused native
case matches `$010E.9000` and camera `$00F8` at update 178, all remaining scroll
steps, and the final alignment. Four-direction initial positions and trajectories,
native camera alignment, music timing, and autosave continuation checks pass.

The verifier now aligns destination RNG/enemy owners at native completed loading
(`$82:E659`). During the 62 intervening IRQ intervals, it compares Samus/camera,
input/NMI, movement, animation and health on every interval, and checks that the
port's already-loaded RNG/enemy owners remain unchanged. At completion it compares
them against native state before destination gameplay. This check passes. It does
not compare a half-written cartridge enemy pool against a completed host load,
add decompression delay, or inject any reference state. A pending deferred check
at the movie's end is an error.

The native post-scroll continuation at `$82:E544` now performs horizontal alignment
before the music wait. It accepts one NMI without another main-loop RNG call;
the following animated-tile and music-wait dispatches advance RNG normally. The
focused fixture confirms native positions and RNG through those boundaries.

Music handling now runs before each outer door dispatch; IRQ-only waits preserve
its queued delays. Sound dispatch waits for the suspended coroutine to return.
The corrected real-door test checks post-scroll stop/upload timing and the track's
next-prologue acquisition plus eight-dispatch delay. Audio queue and autosave
continuation checks pass.

The final `$82:E6A2` dispatch now returns after one NMI without moving/animating
Samus or advancing destination actors. Each following `$82:E737` fade dispatch
runs the enemy/draw owners and palette step, still without Samus movement. Native
first-fade instruction `$E546`, sprite `$E9A5`, and durations 12 then 11 are covered
by the focused fixture. The independent replay passes the entire door transition.

The shared Ridley wait now checks the native enemy door flag `$0797`, separately
from the elevator gate `$0795`. Visual instructions still run during the fade, but
the reveal countdown remains zero until the first ordinary gameplay update, where
it becomes 169. This was reproduced with a failing real-door assertion and then
confirmed against the native checkpoints. The replay now also compares the door
flag, Ridley AI function, and function timer. Autosave continuation passes.

The center-facing helper now preserves the current body instruction when Ridley
already faces the room middle. Native `$A6:D955` tests bit 7 of the low X byte;
the port previously turned every actor that was not mid-turn. The original frame
734 trigger and both sides/directions, mid-turn and low-byte boundary cases pass,
and the independent replay passes the corrected interval.

The missing hit at update 772 is fixed by restoring native tail geometry:
function-zero/stagger/stop paths retain offsets, clockwise subtraction restores
its comparison-only decrement, a moving predecessor prevents a child from stopping,
and Mode 7 signed products floor negative fractions. The replay now also compares
tail-tip X/Y during gameplay and confirms Samus takes the native 30 damage.
Focused offset checks and the corrected native pointed-tail stop fixture pass.

The hover timer now follows $A6:B3F8: decrement first, then leave on a negative
result. It wraps from zero to `$FFFF` before selecting an attack. The focused
boundary check and independent replay pass the former update-882 mismatch.

Grounded Spring Ball now uses movement type `$11` momentum command six, matching
$91:8304/$EC85. It previously used the airborne ordinary ball's deceleration rule.
The native source-1616 release fixture confirms final X `$004C.4000`, stationary
pose `$7A`, zero base momentum that update, and no movement on the next update.

Ridley's turn now restores all seven rest distances before mirroring tail angles,
matching $A6:D3F9. The focused fixture confirms reset lengths and preserved targets/
offsets; the independent movie passes the formerly mismatched turn geometry.

Stationary Spring Ball now also selects command six during hurt movement. A
source-1983 fixture confirms the native knockback displacement `$00CB.4000`,
retained stationary pose, and zero residual base speed after the movement owner.

Projectile coverage now includes types, X/Y, radii, damage, charge and shared
cooldown. It caught an omitted cooldown condition at update 2677: Charge equipment
or either current/previous Shoot edge selects the ordinary firing delay. The port
incorrectly used the longer held-fire delay, suppressing a later shot. The focused
producer checks and movie now pass that interval with the expanded coverage.

Ridley's interaction gate now follows $A6:BCB4/$DE7A's signed camera window.
Normal and hurt AI apply the screen test after EnemyMain has consumed the prior
gate. Offscreen AI suspends the release timer; reentry clears the gate for the
following collision pass. Native source3062/3063 and exact boundary fixtures pass,
and the independent replay now matches the disputed damage and vertical movement.
The replay also compares the interaction-disable bit on every Ridley update.

Enemy-projectile comparison now also covers identities, X/Y, radii and damage.
It exposed the omitted dust/sound side effects of Ridley's terrain strike at
source767. Native $A6:B748/B74F now spawns dust variant nine at tip X/Y+12 and
queues library-two $76 Max6. The real-room focused fixture confirms both, and the
replay passes that interval.

The fireball area initializer now applies native $86:932F damage3/60/80 for
default/Norfair/Tourian to fireballs and directional afterburns. Focused production
initializer and Gravity Suit contact checks pass all three rows. The independent
replay also passes the former update3037 missing-15-damage boundary.

Enemy-projectile square slopes now test occupied eight-pixel quadrants rather than
blocking the whole tile, matching $86:85C2/$8676. A real-room source1248 fixture
failed before the fix and now matches native X/Y and subpositions. Occupied-quarter
horizontal/vertical checks also confirm the eight-pixel collision clamp.

The one-time movie importer now preserves the recorded Moonwalk option ($09E4).
Its omission caused a false turn-versus-moonwalk mismatch at source3317. No
production behavior changed for that importer correction.

Standing pose initialization now follows $91:F4DC's frame-one skip when both
source and target aim straight up. Landing entry and animation-command completion
previously restarted frame zero. The focused fixture failed before the change;
both facing directions now match frame1/timer2 on landing and frame1/timer16 on
completion. The input-only replay passes both recorded transition boundaries.

Ridley's grab now enters carry setup and its first movement/countdown immediately,
matching $A6:BB8F's fallthrough to $BBC4. It installs native command-zero stationary
control and release restores command one. Main places carried Samus after body/tail
movement; beta observes actor control changes made during the same update. The
source3850 focused grab fixture failed before the fix and now matches AI, timer,
velocities and lock/release. Input-only replay passes the recorded grab update.

Locked alpha now bypasses the HUD dispatcher's shared cooldown and bomb-placement
producers while preserving existing projectile updates, matching $90:E713.
The focused grab fixture confirms cooldown10 stays10 while locked and resumes to9
after release. The input-only replay passes the formerly mismatched locked interval.

Carry-rise and carry-release now test timer expiry before acceleration, matching
$A6:BBF1/$BC2E. Expiry preserves velocity and installs native tail angle/extension
settings. The source3915 velocity fixture failed before the fix; both expiry
branches now pass their velocity, next-phase and tail-setting checks. Independent
replay confirms the originally mismatched release and subsequent carry phase.

Swoop phases now own a separate $7E:7800 countdown instead of overwriting the
general $0FB2 AI timer. Native $A6:B441-$B594 leaves that general timer intact.
Focused setup/countdown/phase-transition checks pass; replay now compares both
timers during swoops and passes the original source4132 mismatch.

Ridley body contact now runs before AI at EnemyMain's collision seam, while tail
contact runs after the tail is solved ($A6:CAF5) and is disabled while carrying
Samus. The old combined post-AI pass caused body damage one update too early.
The focused real-room boundary fixture fails before the change and passes after:
movement into overlap is harmless until the subsequent body contact pass, which
still deals40 damage. Input-only replay confirms the disputed source4234 hit is gone.

Ridley's spin-jump response now follows $A6:B669-$B684: the current RNG low byte
at least$80 starts the fireball instruction sequence unless roaring or mid-turn.
The port omitted this producer, retaining a sleeping turn instruction. Focused
threshold/roar/facing tests confirm instruction timer/loop reset and unchanged RNG;
independent replay passes the recorded source4322 instruction transition.

Camera coverage now includes both fractional positions, imported once from the movie
snapshot. It exposed door setup/loading clearing retained native fractions at update177.
Door position writes now preserve them, including transfer to the new room scroll grid.
The independent replay confirms the door fraction handoff. Native command seven
($91:ED0E) also replaces Samus's previous whole-Y checkpoint after morph/unmorph
alignment. Restoring that omitted write resolves the source6289 camera fraction
mismatch. Focused checks cover both facings, current/previous fractions, one-time
checkpoint consumption, and the zero-offset unmorph entry.
The source7764 wall-probe discrepancy was caused by stale pose history: a partial
retained-fallback pose list omitted Space Jump, Screw Attack and spin landing.
Fallback now uses the compiled definition ($91:82D9), including both $FF and an
explicit same-pose value. Retained compact aerial poses skip pose reinitialization.
The verifier now compares all four history words at every boundary; focused production
checks cover ordinary spin, Space Jump and Screw Attack in both directions. The
independent replay passes the original wall probe.

Ridley's successful lunge grab now negates vertical velocity before carry setup,
matching $A6:BB56-$BB5D. The separate ground-attack reversal was already present;
its shared carry entry did not supply this lunge-specific write. The source8395
focused fixture confirms velocityFC25, immediate carry and countdown31, and the
independent movie passes the recorded carry positions. All currently compared fields
now pass that grab interval.

Pause-only dispatchers now preserve $82:894F's main-loop RNG advance. Pause entry
also runs the still-enabled lava HDMA callback first (its byte swap is $88:B44A),
while later pause dispatches run with HDMA disabled. Gameplay fade states retain
runtime RNG ownership. Focused checks confirm both pause-entry value7266 and ordinary
paused value5882 from seed117D, with no duplicate advance in gameplay fades.
Independent replay passes the pause-entry RNG boundary.

Reserve mode/capacity/energy are now imported once from the initial movie snapshot
and compared every interval. Native starts with300 reserve energy in Auto mode.
Equipment-page entry now selects the reserve mode control when capacity exists,
matching $82:ABAD-$ABB5; previously it selected the first beam, redirecting the
recorded A press. Each re-entry resets selection through the same native rule.
Focused manual reserve checks pass (selection, toggle, first transfer, suspension,
resumption, sound/clamp and visible digits); the historical-layout assertion now
accounts for subsequently added map fields and excludes nonserialized assets.
Independent replay passes the recorded reserve transfer and all currently compared
fields through the reserve interval.

Successful zero-health grabs now execute death movement immediately after setup,
matching $A6:BB8C's JMP to $C538. Deferring it lost the first death-spot acceleration.
The focused source10132 fixture confirms X velocity01C6, Y velocityFBDE and retained
countdown29; independent replay confirms both position fractions and subsequent
movement. All currently compared fields match through update10161/10717.
Ridley breakup spawning now installs the common empty spritemap after initialization,
matching SpawnEnemy $A0:93D9. The focused twelve-actor fixture confirms this initial
value and its replacement by each first instruction; all29 breakup programs pass.
Fragment expiry now calls shared EnemyDeathAnimation(0), matching $A6:C90B-$C90E,
instead of dust plus deferred deletion. It clears the slot immediately, preserves
header/position in the F345 explosion-to-pickup actor and increments the kill count.
Focused checks assert all these properties. Independent replay confirms recorded
expiry/pickups and all currently compared fields through update10452/10717.
Ridley's death finish now stores terminal RTS $C600 after drops/music/deletion,
matching $A6:C5FA-$C5FD. The focused production fixture checks terminal state,
defeat/drop publication, deletion and inert subsequent dispatch.

The independent input-only replay now reaches the terminal record: **all10,717
updates across the original10,890 source frames match the currently instrumented
fields** (`death-terminal-replay.log`). This is not yet a full-property parity claim.
The remaining audit must cover relevant inventory/ammunition, control/combat timers,
projectile motion/instructions, complete Ridley tail/AI state and other gameplay
owners not currently compared. The original movie remains unchanged and no later
native gameplay state is injected.
Additional gameplay properties still need coverage before any full-match claim.
The old frames 375–744 Ridley-only comparison, which supplies recorded Samus state
and RNG, remains an isolated regression.

Run the converted diagnostic from the hotfix worktree root:

```text
python tools/convert-smv-updates.py MOVIE TRACE_DIRECTORY --rom ROM --output TRACE_DIRECTORY/updates.json
dotnet csharp/src/SuperMetroid.Verification/bin/Release/net10.0/SuperMetroid.Verification.dll --ridley-full-movie TRACE_DIRECTORY
```

The current verifier deliberately requires the original supplied Ridley movie's
identity. The converter accepts supported one-controller SMVs with the matching
instrumented J/U trace; additional verifier starting-state importers are separate
work, not something the converter silently fabricates.

### Acid tide and combat-state coverage

Expanded combat checks reproduced the first fractional-health mismatch at update
3514/source3618. Native acid height was $01BF while the port used $01B7,
charging an extra $6000 fractional energy at exit. $88:B2DF/$B316 sample the
negative-cosine prefix, but the port and its earlier test sampled the sine origin.
Correcting that lookup also exposed missing tidal updates during door fades.
Lava/acid motion now belongs to the HDMA prologue, including callback-install and
frozen-time gates, with no duplicate advancement in the gameplay rendering pass.
The initial import restores the movie's tide phase/offset, base fraction and surface
once; subsequent native checkpoints remain comparison-only.

The corrected ROM-backed tide fixture checks all phases for the existing four
option combinations. The expanded independent replay passes all 10,717 updates,
including health fractions, combat/HUD fields and gameplay acid surface/tide phase.
This fixes that demonstrated discrepancy; the coverage audit above remains open.

### Ordinary projectile state coverage

The comparison now includes all active ordinary projectile subpixels, signed
velocities, directions, instruction cursors/timers, spritemaps, trail timers,
missile variables and auxiliary words. It reproduced a retained-word difference
at update8484/source8598: a Super Missile companion had native trail timer2,
while the port had cleared it. Native $90:ADB7 leaves $0C90 and $0CA4 intact;
$90:BF46 allocation does not initialize those words. Individual deletion/allocation
now preserves them, while full pool initialization retains its separate reset.
The companion does not itself consume the trail timer; this is a verified state
lifetime correction, not evidence of a player-visible missile defect.

Release build and the independent input-only movie pass all10,717 updates with
these additional checks (`projectile-retained-build.log` and
`projectile-retained-replay.log`). The original movie remains unchanged.

### Enemy-projectile motion and composition coverage

All active enemy-projectile subpixels, velocities, graphics indices, general and
instruction timers, instruction cursors and native callbacks now pass the full
movie. Installed program-frame identifiers are compared by rendering their parts
and comparing OAM to the cartridge spritemap; placeholder pointer values are not
mistaken for missing art. Every encountered composition mapping is checked once.

The expansion reproduced retained velocity differences at update666/source767
(dust reusing an inactive physical slot) and update1200/source1301 (afterburn final
animation). $86:8027 leaves whole coordinates/velocities to family initializers;
$86:950D/$9522 stop afterburn movement by replacing its instruction list, whose
callback clear stops integration without zeroing velocity. The port now preserves
these words and explicitly applies the center-afterburn initializer's velocity
clears at $86:9499/$949C. Inactive initial coordinates/velocities are imported once
from the movie snapshot. These are state-lifetime corrections; no new visible
projectile defect is inferred solely from an unused retained word.

The release build and input-only replay pass all10,717 updates with the expanded
checks (`enemy-projectile-final-build.log`, `enemy-projectile-final-replay.log`).
The remaining coverage audit above is still active.

### Enemy collision properties and timers

The complete input-only replay now also checks active enemy X/Y radii, properties,
extra properties, AI-handler bits, general timer, palette/VRAM indices, draw layer,
flash/freeze/invincibility/shake timers and frame counters. These fields were
already imported once at the initial boundary. All10,717 updates pass without a
production change (`enemy-properties-build.log`, `enemy-properties-replay.log`).
Remaining actor-specific AI/tail state and the other audit categories above still
require coverage before full functional parity can be claimed.

### Complete Ridley tail segment coverage

Every retained gameplay boundary with live Ridley now compares all ten words of
all seven tail records: activation, stagger, movement direction, distance, extension
target, angle, X/Y position and X/Y offset. The Boolean activation maps to the
native $8000 word. All10,717 updates pass with no production changes
(`tail-segments-build.log`, `tail-segments-replay.log`). This verifies intermediate
segments as well as the previously checked tip. Shared tail-controller words and
remaining actor AI variables are still tracked separately in the coverage audit.

### Ridley shared tail/body controller coverage

The replay now checks tail function, idle-whip enable, whip request, extension
speed, angular step, both angle limits/targets and ideal segment separation;
body velocities, fight mode, movement enable, wing frame/timers, facing, health
stage, grab offsets, tail damage, feet index and intangibility countdown.

This reproduced two controller discrepancies. At update241/source276, Norfair
initialization prematurely used facing-adjusted bounds3FC0/4010 instead of the
shared initializer's3FF0/4040 ($A6:D2DD-D2E7). Initialization now uses the native
bounds, with subsequent live adjustment unchanged. At update10162/source10329,
death release reset intangibility to10 instead of preserving0: ReleaseSamus skips
that assignment for negative fight mode ($A6:BC8F-BC93). That guard is now present.

The release build, focused grab-entry/release confirmation, and independent full
movie pass (`ridley-controller-final-build.log`, `ridley-controller-grab.log`,
`ridley-controller-final-replay.log`). All10,717 updates agree with the expanded
fields. Phase-dependent aliases and the remaining audit categories stay open.

### Persistent boss, event, item and door state

Every byte of the native boss, event, collected-item and opened-door bitsets now
compares at every retained boundary, including the terminal record. The existing
one-time initial import remains unchanged. All10,717 updates pass, including the
Ridley-defeated bit transition, without a production fix (`world-bits-build.log`,
`world-bits-replay.log`). Other persistence allocations and room/PLM mutations
remain explicit open coverage items.

### Active room collision data

At every MainGameplay boundary, every active foreground block word and BTS byte
now compares with native WRAM. The full movie passes all10,717 retained updates
with these checks enabled; no production change was needed (`room-collision-build.log`,
`room-collision-replay.log`). This covers the collision data consumed during play,
not merely sampled movement outcomes. Partially decompressed loading boundaries
are not compared by this new check; PLM execution state and those transition-owned
mutations remain separate audit items.

### PLM execution and rejected grey-door shots

At MainGameplay boundaries the replay now compares all forty PLM headers and
active slots' block indices, room arguments, shared loop/shot timers, execution
cursors and callbacks. Grey-door semantic phases map to their native program
cursor, condition callback and wake-up link. Executing instruction timers are
compared; locked doors omit the dormant Sleep countdown because the cartridge
resets it before waking the list. Generic retained link words and family-specific
variables remain explicit coverage gaps.

This reproduced update 5126/source 5239: a locked grey door retained shot status
$9019 after rejecting a hit, while native $84:BDD0 cleared it. The port already
consumed the pending-hit flag but omitted the shared word clear. It now clears
both. This confirms state fidelity; no additional visible defect is inferred
solely from the retained word.

The release build has zero errors and the independent input-only replay passes
all 10,717 updates (`plm-clear-build.log`, `plm-clear-replay.log`). No subsequent
native checkpoint state is injected. The remaining coverage audit stays open.

### PLM family-variable and link-consumer audit

The native movie's MainGameplay boundaries contain only grey-door headers $C842
and $C848. The verifier now requires an explicit family mapping for every active
PLM, compares the condition's native byte offset ($1E17) and maps the semantic
one-hit opening state to the native hit counter ($DF0C). Room population spawn
clears the counter ($84:846A); grey-door setup decodes the condition ($84:C794).
All 10,717 updates pass these checks (`plm-family-build.log`,
`plm-family-replay.log`); the release build has zero errors. No production change
was required.

The earlier generic-link gap is now narrowed for this movie: the entering door's
closing list retains $DA54 but never consumes it. Fallthrough into the initial
list overwrites the link before installing the condition callback. Live links
in subsequent locked/flashing phases already compare through the semantic map.
This does not establish coverage of unrelated PLM families or their retained
variables. Draw/presentation state and partial-loading boundaries remain open.

### Samus movement owners and aerial collision-radius timing

Every retained boundary now also compares Samus's collision radii, whole/fractional
gravity, whole/fractional external X/Y displacement, slope-collision enable, speed
divisor, contact-damage selector, ball-bounce state, bomb-jump direction, running
momentum and speed-boost counter. New owners are imported once from the initial
snapshot; later native checkpoints remain comparison-only.

These checks reproduced premature radius updates at three boundaries:
- Update 2676/source 2777: Screw Attack to gun-extended jump, native 12 vs port 19.
- Update 9235/source 9356: down-aim falling to aerial turn, native 10 vs port 19.
- Update 9241/source 9362: turn completion to down-aim falling, native 19 vs port 10.

$91:F404/F433, the normal-jump initializer and aerial-turn initializers preserve
the source radius through these transitions; $90:EC22 publishes the new radius
on the next alpha pass. Removed the early radius refresh from spin exits and
aerial-turn entry, and made aerial-turn animation completion retain its radius.
Other callers of the shared pose helper retain their existing behavior pending
their own source/coverage audit. The focused spin fixture previously asserted the
premature enlargement; it now checks the retained radius and next-alpha update.

Release build, --space-screw-fixture, --samus-aerial-turns-walljump, and all 10,717
updates of the full movie pass (`movement-radius-confirm-build.log`,
`movement-radius-screw.log`, `movement-radius-turns.log`,
`movement-radius-confirm-replay.log`). This verifies the actual radius property,
not just unchanged trajectory. Player validation and the remaining full-parity
coverage audit remain open.

### Firing-state ownership and normal-jump shot publication

MainGameplay boundaries now compare projectile/bomb counts, previous charge,
projectile-interaction immunity, charged-shot glow, charge-palette index,
bomb-spread charge timeout, pose-transition shot direction, Hyper Beam state and
the resume-charge sound latch. These owners receive only their one-time initial
snapshot values. The projectile-immunity XML address was corrected to $18AC.

The comparison reproduced update 8484/source 8598: a neutral-jump to firing-pose
transition left native shot direction $8007 while the port held zero. The early
$90:EB20 clear was correct. The late normal-jump initializer ($91:F5CF-F5E6)
publishes the current pose's direction on a fresh Shoot edge, including aim/firing
changes within an existing jump. The port had that write only on selected jump
entry/spin-exit routes. The shared accepted-pose boundary now publishes it for
changed normal-jump poses before committing history. The tag has a domain-named
catalog member; the native initializer remains the behavioral authority.

All 10,717 updates pass with the expanded checks (`firing-publication-build.log`,
`firing-publication-replay.log`). The final catalog-only cleanup builds and passes
--space-screw-fixture (`firing-final-build.log`, `firing-final-focused.log`). No
new native state is injected after initial import. Player confirmation and the
remaining coverage audit are still pending.


### Bomb and power-bomb ownership coverage

The native recording never activates any of the five bomb projectile slots or
the power-bomb armed/explosion status words. The replay now compares each slot's
type and both activation words at every MainGameplay boundary, independently of
the already-compared bomb counter. Explicit assertions require a live-state
mapping if a future movie activates one of these owners; there is no silent
active-bomb coverage omission.

Release build and all 10,717 input-only updates pass with these checks
(`bomb-ownership-build.log`, `bomb-ownership-replay.log`). No production change
was required. This establishes inactivity for this supplied recording, not
explosion behavior for recordings that use bombs. The summary ledger above now
reflects the prior completed coverage expansions and their remaining gaps.


### Enemy-projectile properties and family variables

The full replay now compares all damage/contact/persistence/shot-blocking/draw
property bits and E/F/G for the movie's fireball, afterburn, dust, death-explosion
and pickup families. Afterburn E/F map to remaining count/next type; pickup E/F
map to type/lifetime. Fireball F is compared as the zero/nonzero afterburn gate
consumed by $86:940E: native callers can leave a noncanonical nonzero parameter
($000E at update 1116/source 1217), while the port retains its Boolean meaning.
Unmapped families fail explicitly rather than silently skipping those words.

This reproduced update 1200/source 1301: final-afterburn properties were native
$503C versus port $703C. Terrain impact prematurely disabled contact damage.
Native $86:950D/$9522 only replace the program and reset its timer; the final
$86:9574-958B program clears movement, draws five frames, and deletes without
disabling contact. Removed the early disable. The focused real-program fixture
now asserts damaging contact on entry and through all five final-animation frames.

Release build, --ceres-ridley-projectile-instruction-mechanics and the independent
10,717-update movie pass (`eproj-family-final-build.log`, `eproj-family-focused.log`,
`eproj-family-final-replay.log`). Player validation remains pending. Parallel
projectile collision/drop metadata and other documented coverage gaps remain open.


### Ordinary projectile callback identity

Every active ordinary projectile now compares its semantic callback with the
native $0C68 word at MainGameplay boundaries. The supplied movie reaches Wave
four-frame-trail flight ($90:B0C3), missile ($AF68), Super Missile ($AFE5), its
companion/link ($B075), and the empty callback used by impact animations ($B169).
The mapping also admits the two other ordinary beam flight callbacks; any other
semantic callback fails explicitly until mapped. This checks dispatch identity
in addition to previously compared motion, timers and program cursors.

The release build and all 10,717 independent input-only updates pass
(`projectile-callback-build.log`, `projectile-callback-replay.log`). No production
change was required. Remaining parallel metadata and presentation/control-state
gaps in the ledger are still open.


### Enemy-projectile collision/drop metadata

All active enemy-projectile collision-option words ($F380) now compare. Death
explosions additionally compare their source enemy header ($F3C8), consumed by
the later drop-selection instruction. Death explosions and pickups both compare
the killed-enemy/respawn word ($F410). Direct chance-table overrides fail explicitly
until mapped if a different movie encounters them.

At update 10453/source 10625, direct Ridley pickups exposed an important native
representation distinction: $86:EF3E writes the supplied source header through
caller X, while pickup allocation uses Y. $86:F118 reads that same caller-X word
during immediate drop selection. The pickup's allocated-slot header retains old
values (zero or E1BF in this recording), and its remaining program never consumes
that header. The port stores the source on the pickup for immediate selection.
This unused per-slot retained word is excluded explicitly; live death-explosion
headers, selected pickup types/lifetimes, respawn state and RNG remain checked.
No production change is justified by that representation difference.

Release build and all 10,717 independent movie updates pass
(`eproj-metadata-final-build.log`, `eproj-metadata-final-replay.log`). The remaining
control, presentation, loading and other documented coverage gaps remain open.


### Projectile trail animation and placement

Both sides of all eighteen trail slots now compare their instruction timers at
every MainGameplay boundary. Live sides additionally compare instruction cursors,
world X/Y and tile-number/attribute words. The original snapshot initializes those
owners once; every subsequent value is produced by the port from recorded inputs.
Left/right streams are checked independently, preserving their separate lifetimes.

Release build and all 10,717 updates pass (`trail-state-build.log`,
`trail-state-replay.log`) with no production change. This establishes trail state,
placement and selected tiles, not final frame pixel equivalence; renderer composition
and the other documented presentation/control/loading gaps remain open.


### Atmospheric effects and liquid animation/damage owners

All four atmospheric slots now compare packed frame/type and, when active, timers
and X/Y. The remembered liquid medium, animation-delay buffer and both periodic
damage accumulator words also compare at MainGameplay boundaries. These fields
are initialized from the starting snapshot once, with no later native injection.
Corrected the atmospheric-state XML address range to $0AD4-$0AF3.

Two discrepancies were reproduced and fixed:
- Update 353/source 454: landing dust Y was $01AF instead of $01AE. Native
  GetBottom_R18 ($90:EC3E) uses current pose radius and subtracts one. Landing
  effects now use that inclusive pixel for placement and liquid suppression.
  Focused checks assert dust/splash Y and equality at the liquid surface.
- Update 3230/source 3334: knockback floor contact created dust absent on the
  cartridge. $90:DF6E clears live vertical speed before $91:F046 checks impact.
  The port restored a diagnostic pre-impact magnitude, inventing landing effects.
  The consumer now sees the cleared live speed; drained movement remains unchanged.

Release build, --samus-atmospheric-effects, and all 10,717 movie updates pass
(`atmosphere-final-build.log`, `atmosphere-final-focused.log`,
`atmosphere-final-replay.log`). Both observed slot properties are verified directly.
These implemented fixes await player validation; remaining presentation/control/
loading coverage still prevents a complete-parity claim.


### Retained input and pause epilogue

Every retained update now compares Samus's previous held and newly pressed input,
auto-jump timer, previous health sample and normal/one-shot auto-jump input-handler
identity. Those owners are imported from the original snapshot once. Special
pose-input locks require an explicit mapping if a future movie encounters them.

Update 9419/source 9544 reproduced a paused held-input discrepancy: native $0010,
port $0000. The cartridge's main-loop tail calls $82:8AB0 after the dispatcher;
$82:8ADD-$8ADF copies current held input to $0DFE even while paused. The port only
published that word from Samus's draw-time epilogue, which pause does not execute.
The five pause-only dispatchers now publish held input on return without updating
the previous press, auto-jump timer or health history. Gameplay continues using
its existing draw-time publication; coroutine-only NMI waits are unaffected.

Release build and all 10,717 updates pass with these comparisons
(`input-epilogue-build.log`, `input-epilogue-replay.log`). The failing pause sample
and preserved gameplay-only words are checked directly. The fix awaits player
validation; remaining handler, presentation and loading coverage remains open.
