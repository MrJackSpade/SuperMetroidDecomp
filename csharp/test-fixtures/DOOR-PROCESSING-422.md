# Sound-drain transition ownership (#422)

`--door-sound-wait-audit ROM` reproduces the formerly omitted enemy owner in
DoorTransitionState.WaitForSoundQueues and now guards its restored invocation.
It loads the real awakened Climb population, stages only the sound-wait phase,
and holds an unread sound request for eight calls. Camera alignment, room load,
Samus movement, real-time pacing and ordinary SPC drain duration are excluded.

Before the fix the constructed enemy frozen word remained120 during all eight wait calls.
Calling the existing enemy owner directly changes it to0 in the control.
The diagnostic checks that the control exercises a state change, then fails
because the wait stage omitted it. After the fix both paths agree. The initial assumption of a112 result was
incorrect for this actor/setup and is not used as a cartridge expectation.
This is an owner-invocation fixture, not a natural frozen-pirate technique or
an independent native CPU trace. The later door-entry sound-disable correction
below supersedes this fixture's original assumption that fresh requests are admitted.

Pinned `upstream-sm/src/sm_82.c`, DoorTransitionFunction_WaitForSoundsToFinish
at `$82:E29E`, explicitly runs DetermineWhichEnemiesToProcess, EnemyMain,
DrawSamusEnemiesAndProjectiles and EnsureSamusDrawnEachFrame before testing
the three unread sound-ring counts. The former C# branch ran only NMI and the test.
It therefore could not reproduce enemy-driven sound production during this wait.

The correction reuses extracted actor drawing and the existing enemy phase,
without ordinary movement, terrain preparation, gameplay palette clocks or
room-main updates. It applies the native unconditional non-elevator Samus
draw afterward. Fresh enemy/music and post-draw Samus sounds are collected
before testing the queues; this does not increment a full gameplay publication
or replay old PLM/projectile requests. The diagnostic asserts unchanged Samus
16.16 position and animation timer, body drawing, and one real enemy sound
published exactly once through24 frontend wait calls. Its held acknowledgement
is constructed; it does not establish normal SPC drain duration.

Still required: compare exact cartridge timing and queue acknowledgement phases.
Selecting items, spin/Space Jump checks,
firing, landing, interrupted charge, attached enemies, looping liquids,
Power Bomb suppression and combined actions remain required by the ticket.

Reference: https://wiki.supermetroid.run/Processing (claims are investigation
inputs, not asserted timings). No private cartridge assets are published.

## Door-entry DisableSounds correction

Following the complete entry path exposed an omitted guard: `$82:E279` sets
DisableSounds after the initial stop commands; `$82:E757` clears it only after
the final destination fade. The isolated queue and wait-entry probes below did
not execute that entry owner, so they did not test this flag's lifetime.

The live Climb enemy/post-draw frontend fixture reproduced fresh requests being
admitted during the wait (including an ended spin). Before correction its
native-guard expectation failed at frame zero, library one: expected zero new
requests, observed one. The fixture still runs both owners but now expects zero
admissions throughout 24 wait calls, with and without an active Power Bomb.

CartridgeAudioState now applies the door-owned disable guard after the native
occupancy check, preserving the accumulator return value. SuperMetroidGame sets
it after entry stop commands and clears it after the final fade. Each Step
reconstructs it from the serialized frontend state; the derived field is excluded
from debugger serialization, so older mid-transition states need no new field.
Queued sounds keep draining and music is unaffected. This is not host muting.

Tests cover the actual final-fade release, existing-queue drain, native accumulator,
music admission, re-enable/reset, the full core suite, the Windows build, and an
unchanged production debugger-state load/replay. This does not complete the broader
controller/native-SPC action timing matrix. The guard fix follows v0.3.4 on main;
the published release tag was not moved.

## Original 65816 queue comparison

`native-door-sound-probe.h` executes the original, unpatched `$80:9035`,
`$80:90B7`, `$80:9139` Max3 queue routines and `$82:89EF` sound dispatcher.
Do not substitute the native C sound dispatcher: its acknowledgement handlers
are explicitly modified and use a different clear delay.

The72 cases cover all three libraries,0..3 requests, starting ring positions0
and14, and acknowledgement lags0..2. Each emits32 frames. All2304 rows match
the production C# queue exactly: read/write indexes, state, current request,
clear delay, sparse APU write and unread-ring predicate. With two requests,
the ring first empties at frames4/6/8 for the three lags; with three it empties
at8/12/16. The final sound is still in state1 at that point. These are fixture
dispatcher frames, not measured full-door delays or wiki maximums.

### Reproduce privately

Include `native-release-probe.h` and then `native-door-sound-probe.h` at the
end of `upstream-sm/src/sm_rtl.c`. Before SDL initialization in `main`, route
`--door-sound-probe ROM NEW_CSV` to `DiagnosticDoorSounds(argv[2], argv[3])`.
At the APU-read branch of `snes_readBBus`, before its unsupported-read assertion,
temporarily add:

```c
extern bool g_diagnostic_door_sound_ports;
if (g_diagnostic_door_sound_ports) return snes->apu->outPorts[adr & 3];
```

This switch applies only to the explicit diagnostic, supplies constructed
acknowledgements and does not run the SPC. The cartridge's CPU instructions
and queue state remain original. Build Release/x64 with the documented v145
native toolchain, run the headless command, then compare using
`--door-sound-queue-compare ROM NEW_CSV`. The probe refuses to overwrite a CSV.
Remove all three temporary hooks and rebuild the ordinary executable afterward.
That cleanup was completed after this comparison. No emulated controller
action, full door coroutine, music downtime, Power Bomb cancellation or
simultaneous-library arbitration is certified by these cases.

### Simultaneous-library comparison

`DiagnosticCombinedDoorSounds` extends the original-CPU probe to all 64
combinations of 0..3 requests across the three libraries, ring starts 0/14,
and acknowledgement lag assignments (0,1,2)/(2,1,0). All 256 cases run 32
dispatcher frames and publish each library plus the shared unread predicate.
The resulting 24,576 rows match C# exactly, including sparse writes, current
sound, ring positions, state, and clear delay. This closes the prior isolated-
library evidence gap without changing production code.

Committed trace `movement-release/combined-sound-422.csv` contains only constructed
queue state, no ROM/SRAM or player recording. Verify with DebugRunner
`--door-sound-combined-compare ROM TRACE`. To regenerate, use the temporary hooks
described above, dispatch `DiagnosticCombinedDoorSounds`, and include its header
after `g_apu_write` is declared in sm_rtl.c (the release-probe helper must precede
it). Pass a new output path: the probe refuses overwrite. Temporary SDL-free
dispatch/APU hooks were removed and the ordinary executable rebuilt afterward.

These are controlled acknowledgements, not real SPC timing or the complete
controller-to-door transition matrix. Action alignment, music downtime and
combined gameplay producers still require their own original-CPU comparisons.

### Actual wait-to-fade handoff

`DiagnosticDoorWaitHandoff` runs original `$82:E29E` (including EnemyMain and
the complete draw pass), then `$82:89EF`, once per fixture frame. It covers each
library independently with 0..3 queued requests and acknowledgement lags 0..2.
All 225 rows across 36 cases match `DoorWaitNativeComparison`: coroutine phase,
read/write positions, sound state, sparse APU write, and fixed Samus X/Y.
An empty ring advances on frame zero; one request advances on frame one even
when its acknowledgement is still pending. This verifies the production wait
branch's ordering before audio advancement, not just its unread-ring predicate.

The empty native population still requires LoadEnemies' default post-draw hook
`$A0:804C`, installed by `$A0:8A31-$8A3A`. The initial diagnostic omitted it and
jumped through zero after drawing; the CPU trace identified this fixture error.
Restoring the actual initialization value allowed the full draw path to execute
without bypassing its owners. No production behavior was changed for this audit.

Trace: `movement-release/door-wait-handoff-422.csv`. Verify with DebugRunner
`--door-wait-native-compare ROM TRACE`. Regenerate using the same temporary
headless/native-APU hooks, dispatching `DiagnosticDoorWaitHandoff` with a new CSV
path. The native program's error/warning dialogs must be disabled for this
explicit headless command only. Remove the hooks and rebuild after capture.

These cases seed queues and have no live enemy population or new sound producers.
They do not establish full controller-driven door durations, real SPC timing,
music downtime, interrupted charging, landing, or combined gameplay actions.
Those broader #422 requirements remain open.

## Reproduced active Power Bomb publication gap

`--power-bomb-sound-suppression-audit ROM` originally failed before the HUD fix.
It uses the real frontend Select input with five missiles and no selected item.
The baseline selects missiles and emits library1/$39. A paired fixture arms and
spawns the production Power Bomb owner before the same input; its status is
$8000 and selection still emits$39, violating the native queue guard. No fake
sound list or invalid explosion phase is injected. Explosion entry is constructed,
not earned by waiting through a planted projectile fuse.

The queue implementation already exposes the native suppression argument, used
by echo/low-health calls; general deferred sound publication omits it. Do not
filter all requests by final frame status: `$88:8AA4` queues its own library1
sound before setting the active bit, and earlier producers may also precede
that change. The correction needs producer-time suppression semantics, retaining
the Power Bomb cue and legitimately earlier sounds. Other suppressed producers
and start/cleanup boundaries require coverage. No production fix accompanies
the original failing diagnostic.

The HUD producer now captures suppression at its update and the frontend passes
that captured value to the existing native queue guard. The paired real Select
fixture now emits $39 only without an active explosion; both still select missiles.
Four additional producer/publication combinations verify that deferred publication
does not retroactively change admission when explosion state changes. This is a
scoped correction for item selection, not certification of all sound producers:
the Power Bomb startup cue, earlier producers, and cleanup ordering still need
their own coverage before the broader suppression work is complete.

### Bomb-owned requests and activation ordering

The same audit now also plants a normal bomb through the production placement
helper, shortens its fuse to one, and expires it through the frontend. Before
the bomb-producer fix, both the inactive and active Power Bomb cases emitted
the normal explosion sound. Afterward only the inactive case emits it.
`SamusSoundRequest` carries the producer-time guard for bomb-owned requests;
publication uses the existing native queue implementation. Charge cancellation
and ordinary explosion requests sample current status, while the Power Bomb
startup cue samples status before its pre-instruction activates the explosion.

Two real allocated slots, with constructed simultaneous fuse expiration, cover
both descending-slot orders through the frontend: normal slot 0 / Power Bomb
slot 1 suppresses the normal sound, whereas normal slot 1 / Power Bomb slot 0
admits it. Both emit the Power Bomb startup cue. A singly planted Power Bomb
also emits its startup cue. These assertions cover actual APU port commands,
not just request-list contents. This does not yet certify all other producers,
cleanup timing, full native gameplay execution, or the complete #422 matrix.

### Weapon production and cleanup boundary

`PowerBombProjectileSoundAudit` is run by the same command. The original active
Power Bomb beam-fire case failed with an emitted shot sound. Weapon production
now retains the sound guard from after the HDMA update and before bomb handling:
the cartridge dispatches the HUD weapon producer before descending projectile
pre-instructions (`$90:DCF9-$DD00`). Combo pre-instruction requests separately
capture their later status. The frontend passes both through the native guard.

Fifteen frontend checks cover power beam, missile and super missile across five
states: inactive, active, starting this frame, last active frame, and cleanup
this frame. Every case actually fires a shot and checks its APU command. Starting
this frame preserves the earlier shot sound even though final status is active;
the last active frame suppresses it; cleanup immediately admits it. Cleanup
fixtures advance the real HDMA owner until its last two afterglow steps rather
than injecting phase/counter values. The tests are source-checked constructed
boundaries, not full original-CPU controller traces. Missile impact audio, other
owners, and the broader technique matrix remain unverified.

Debugger layout migrations explicitly cover old sound-request, HUD, bomb-owner
and projectile-result field sets (including the older five-field result). Older
captures cannot supply a missing producer-time guard: migration warns and retains
their historical unsuppressed publication behavior until the next producer runs.

### Missile impact producer

`PowerBombImpactSoundAudit` also runs under the same command. A real missile fired
into a constructed solid column reproduced the missing guard: its impact emitted
library2/$07 during an active Power Bomb. The common impact conversion now takes
the guard from its already-supplied shared bomb owner and stores it on the sound
request. Frontend publication passes that captured guard to the native queue.
No additional runtime binding or serialized field is needed.

Four frontend cases cover missile/super missile impacts with and without an active
explosion; all still collide, but only the inactive cases emit the sound. Eight
additional checks fire a real projectile, invoke the enemy-impact conversion seam,
and reverse Power Bomb status after impact. Requests retain their original guard,
while cinematic impacts still generate no request. Those seam checks do not drive
an enemy AI/overlap calculation. The cartridge source is `$93:80F8-$8100`, which
checks cinematic state before the ordinary library-two queue call. The complete
Power Bomb audit and core suite pass; other owners and full #422 timing remain open.

### Enemy audio and door sound waits

`PowerBombEnemySoundAudit` uses the real awakened Climb population and missile
producer. Before the fix its lethal hurt cry was delivered both with and without
an active Power Bomb. Both cases still kill the pirate afterward, but only the
inactive case delivers the cry. Echoed acknowledgements isolate CPU sound-queue
delivery; this is not a new SPC/PCM comparison. EnemyMain binds the live shared
explosion owner, and the common request helper captures suppression. Escape
explosions now use that same helper instead of bypassing it.

The separate door-wait audit runs both inactive and active cases. Each produces
one fresh enemy request in 24 sound-wait calls; the inactive case admits one,
the active case admits zero. It verifies actual queue-write positions, not merely
the request flag. An intentionally unacknowledged second request keeps this
fixture in the wait phase; no native full-transition duration is claimed.
Both frontend publication routes honor the captured guard. An explicit legacy
EnemySoundRequest migration and field-order regression preserve old captures.
The full core suite passes. Initialization-only producers, other owner families,
and the complete processing-technique matrix remain outside this result.

### Samus liquid, movement and post-draw audio

`PowerBombLiquidSoundAudit` reproduces the frontend water-entry gap: both cases
entered water, but the active Power Bomb incorrectly emitted the splash sound.
The shared liquid/movement helper now captures the live explosion guard on each
request. Runtime binds the owner for ordinary movement and door-wait drawing;
both publication paths honor the captured value. Two post-draw spin-stop checks
reverse status before and after the real draw-audio routine, proving it samples
neither a stale frame-start value nor the later publication-time value.

The door-wait fixture now also supplies a previous spin movement snapshot for its
first draw. Both cases produce two requests (one enemy, one post-draw movement)
over 24 calls. The inactive case admits both; the active case admits neither.
An initially observed duplicate was a fixture error: injected door entry lacked
the already-published gameplay marker that normal door entry carries. The fixture
now supplies that marker; no production change was made for that mismatch.
Focused audits and full core verification pass. This still does not cover the
remaining room-FX/palette/PLM owners or certify the complete #422 technique matrix.

### Room FX and palette-effect requests

`PowerBombRoomEffectSoundAudit` reproduces two further omissions through the
frontend: room $02/$28's real rising-lava entry and retail beacon definition
$8D:F781 both admitted sound during an active Power Bomb. The corrected room-FX
and palette request records capture the live guard; the prefix publisher uses
the native queue guard. Legacy record layouts have explicit migration coverage.

Four frontend cases verify the rising-lava request on frame 1 and beacon request
on frame 60, with sound delivered only without the active explosion. The lava
case also asserts its quake remains applied. Queue acknowledgements are echoed;
32 audio-only ticks after the request let earlier queue entries finish without
advancing FX/AI. This prevents a busy port from being misidentified as suppression.
Two standalone retail heat-definition cases both generate three sound requests
and accumulate seven whole damage points across 32 calls; only the active case
marks those requests suppressed. Heat damage is not disabled by the sound fix.
Focused audits and full core verification pass. PLM/one-off publishers, any
remaining initialization or ordering gaps, and full technique timing still need
coverage before #422 can be considered ready for validation.

## PLM producer-time suppression

The retail collision-bomb instruction list reproduced another missing guard:
its block entered the break animation with and without a Power Bomb, but library
two sound $06 was delivered in both cases. PLM sound requests now capture the live
explosion guard when created. The frontend retains that decision, including gate
rejection requests deferred from collision setup into the PLM handler. Queue caps,
PLM execution, music and fanfare requests are unchanged. Older request records
receive an explicit compatibility migration rather than invalidating snapshots.

`PowerBombPlmSoundAudit` verifies both frontend cases against the real break
animation word, plus two rejected green-gate cases with opposite explosion states
before binding and after request creation. The latter verify producer-time capture,
not frame-start or publication-time filtering. This does not establish every
room-initialization path or the remaining full #422 technique matrix.

## Stored-shine and shinespark sound guards

The real frontend reproduced a stored-shine warning at timer 170 with and without
an active Power Bomb; both emitted library-three $0C before correction. Native
$91:DAC7 calls QueueSfx3_Max9, whose $80:9155 guard rejects active Power Bombs.
Directional launch ($91:F80F) and crash ($90:D2BA) similarly call the guarded
queues. Their legacy one-shot publishers omitted producer-time suppression.

The three request sites now retain that guard; deferred publication uses the
existing queue routine without changing caps, sound identities, shine timers,
or movement. Routing references are nonserialized and rebound at gameplay entry.
New suppression fields have an explicit legacy migration; the older nineteen-field
crash-alias migration remains covered as well.

PowerBombShinesparkSoundAudit covers the paired frontend warning reproduction
(both reach timer 169), plus warning/launch/crash production with opposite guard
states before binding and after production. All six boundary cases verify actual
frontend port commands, including both crash libraries. Launch and low-energy
crash use the real state-machine entry points, not an input-earned shinespark.
The complete Power Bomb audit, old player-state load/resource test, full core
suite, and Windows build pass. Other one-off publishers and the complete native
door-processing timing matrix remain open; this is not full #422 completion.

## Suit transformation sound admission

The real Varia setup and frontend publisher reproduced another missing guard:
library-two $56 played with an active Power Bomb. Native $91:D66B (Gravity) and
the corresponding Varia setup call QueueSound_Lib2_Max6, retaining bank $80's
Power Bomb suppression. Suit setup now records that admission decision and the
runtime supplies the live explosion status; the frontend consumes it once even
on frames without a new gameplay publication. Animation and item behavior are
unchanged.

PowerBombSuitSoundAudit covers both suit kinds, active/inactive explosion status
at setup, and unchanged/reversed status at publication (eight cases). Both real
setup owners remain active and actual frontend port commands match admission.
This invokes post-message setup directly; it is not a natural PLM pickup or full
native controller timing reproduction. Existing nine- and ten-field debugger
layouts have explicit migrations, preserving phase and any captured entry latch.
The production slot-1 recovery still deserializes and advances past its saved
boundary. The complete sound-suppression audit and Windows build pass.

## Final-fade spin sound restart

Pinned bank $82:E757 clears DisableSounds, then $82:E75A invokes the spin
restart wrapper. Command $1C at $90:F41E chooses ordinary spin, Space Jump,
or Screw Attack from movement/pose; wall jumping uses animation boundaries
13 and 23. It does not restart charging sounds for a non-spinning pose.

The frontend omitted this call. DoorSoundWaitAudit reproduced zero queued
commands on the final fade where ordinary spin requires library-one $31.
The frontend now calls the shared SamusSpinSoundCommand selector after clearing
door suppression. Hurt-flash recovery uses the same selector instead of a
duplicate implementation. Power Bomb and demo suppression still apply.

The audit checks all four fade calls for both-facing spin variants, both
wall-jump boundaries, and a non-spin negative, each with/without an active
Power Bomb (22 cases). Only the completed fade admits exactly one expected
command, and active Power Bombs admit none. This is a focused frontend
reproduction checked against pinned assembly, not a new native controller
trace; the broader #422 processing matrix remains unfinished.

## X-ray admission versus deferred sound suppression

`PowerBombXrayAdmissionAudit` exercises the actual selected-HUD-item/Dash path
and frontend audio publication, in both facings and with an inactive, armed,
or exploding Power Bomb. All six cases pass: inactive and merely armed bombs
allow X-ray and its library-one activation command; an active explosion admits
neither. The first fixture expectation incorrectly treated Arm as explosion
status. Inspection of `$91:E16D` and the separate native words corrected that
expectation without changing production code. Arm owns `power_bomb_flag`;
X-ray admission tests `power_bomb_explosion_status`.

This rules out a missing active-explosion guard at the ordinary X-ray activation
publisher. It does not cover forced/glitched state transitions, X-ray release,
or a natural controller-to-door timing matrix. The complete Power Bomb sound
suppression audit passes with these added cases. #422 remains incomplete.

## Shinespark charge cancellation

Original CPU execution of `Projectile_Func7_Shinespark` at `$90:CFFA` exposes a
missing action in the port: entering windup clears the live beam-charge counter
and flare animation. When the counter is at least sixteen, it first calls
`QueueSfx1_Max9($02)` to stop the sustained charge sound. An active Power Bomb
suppresses that call but does not prevent the teardown. The committed native
action trace at `movement-release/processing-actions-422.csv` records the same
result at charge sixty: library-one `$02` without an active explosion and no
command with one.

The translated windup now binds the live projectile owner, clears both its charge
and Samus's movement-visible mirror, and publishes `$02` through the existing
producer-time Power Bomb guard. It deliberately leaves the previous-frame charge
sample alone, matching the cartridge routine rather than reusing HUD selection's
broader reset. The pending publication and routing reference are nonserialized:
they exist only inside one completed gameplay frame, so old and new debugger-state
layouts remain identical.

`PowerBombShinesparkSoundAudit` covers charges 1, 15, 16, and 60 with both Power
Bomb states. Every case verifies charge teardown; only counters 16 and 60 emit
the cancellation command, and only when the explosion was inactive at the native
call. Reversing explosion status before frontend publication proves the guard is
captured at production. This closes the shinespark-activation producer mismatch,
not the rest of #422's controller-action timing matrix.

## Processing action producer matrix

`processing-actions-422.csv` records original-CPU queue contents immediately after
eleven cartridge action routines, each with and without an active Power Bomb. The
cases cover HUD selection, ordinary spin entry, Space Jump checking, Screw Attack's
silent direction-change control, ending spin, ending spin while charging, Power and
Wave beam fire, charged-beam release, normal-bomb expiry, and shinespark activation.
The two calls in the charging spin-exit case deliberately retain their library-one
order `$32,$41`; shinespark retains library-one `$02` before library-three `$0F`.

Run the managed comparison with:

```text
dotnet run --no-build --no-restore --project csharp/src/SuperMetroid.DebugRunner/SuperMetroid.DebugRunner.csproj -c Release -- --processing-action-compare "Super Metroid.smc" csharp/test-fixtures/movement-release/processing-actions-422.csv
```

All 22 snapshots match. Managed cases exercise the production HUD, movement,
post-draw, projectile, bomb, and shinespark producers rather than fabricating sound
lists. Beam charge is earned through repeated production alpha passes; bomb expiry
starts from a normally allocated bomb with only its fuse shortened. This establishes
queue production and Power Bomb admission, not the eventual logical door delay.
Attached-enemy, looping-liquid, combined action alignment, and acknowledgement-to-
transition measurements remain outstanding.
