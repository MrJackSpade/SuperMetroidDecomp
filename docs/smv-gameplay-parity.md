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
| Samus position/fractions, camera position/fractions, pose/history, animation, primary speeds | Compared every retained interval | Control handlers, remaining movement flags and secondary motion state |
| Health, reserves, equipment, collected inventory, ammunition and capacities | Complete replay passes these fields, fractional health, hurt/invincibility/knockback timers and directions, HUD selection/cancellation and X acceleration mode (`tide-owner-replay.log`) | Remaining combat/input ownership |
| Enemy identity/position/health/instructions, Ridley phase/countdown and tail tip | Complete baseline passes | Remaining actor AI variables and seven-segment tail state |
| Ordinary/enemy projectiles | Ordinary active-slot subpixels, velocities, directions, instruction cursors/timers, spritemaps, trail/auxiliary words also pass the full replay | Ordinary pre-instruction identity, enemy-projectile family variables/properties, bombs and explosion owners |
| Persistent world state and room effects | Selected room plus acid damage surface/tide phase checked during gameplay | Boss/event/door/item bits, PLM changes, remaining environmental gameplay state |
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
