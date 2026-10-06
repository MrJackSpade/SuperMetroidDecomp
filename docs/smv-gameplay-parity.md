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
along with missing RNG advancement through the outer loading dispatches. The
current checked properties match through update 3036, with loading-owner alignment
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

Expanded checked state now matches through update 4125 of 10,717. The next
mismatch is update4126/source4234: the port applies an extra40 damage and hurt
movement to Samus. This remains under investigation.
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
