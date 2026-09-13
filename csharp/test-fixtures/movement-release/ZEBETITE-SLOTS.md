# Zebetite physical slot reuse — partial #443

Original CPU probe: `native-zebetite-slot-probe.h`, included after
`native-release-probe.h` in the native harness and dispatched before SDL.
ROM/revision provenance and unpatched-ROM loader are the same as XRAY-CHARGE.md.
No player state, cheats, or translated C spawning functions are used.

Four independent zeroed-WRAM setups occupy three of slots 0..3 with a nonzero
Ripper header, leaving one hole. Execute original $A6:FCD9, which invokes
$A0:9275 with the embedded Zebetite population record. Every hole 0, 1, 2, 3
receives $E27F, health 1000, X=824, Y=111. Other occupied headers stay $D47F.
This tests allocation/initialization only, not a full enemy frame or battle.

`--zebetite-audit ROM` now repeats those setups through the production private
spawn path. Before the fix it failed: `Native Zebetite spawn chose hole 0; C#
chose 1.` The old code used the population high-water mark instead of scanning
free physical slots. The correction scans from slot zero, preserving existing
occupied actors and allowing a just-killed primary's slot to be reused. The
four-generation progression assertion now expects that reuse instead of
encoding the incorrect monotonically increasing allocation.

The focused audit passes after the change. The native entrypoint/includes were
removed after capture. No ROM or player-state artifact is published.

This is not completion of #443: surviving linked-half follow-up shots, exact
double-kill timing, camera-gated ten-missile kills, and adjacent failing controls
still require original CPU comparisons. Do not infer those outcomes from the
allocator test alone.

## Linked-half follow-up boundary

The extended original CPU probe initializes generation one (event bit three),
spawns its linked half, sets both health words to zero (the lethal-shot result),
and executes only the primary main callback. The primary slot now contains
generation two at 1000 HP; the old secondary remains at zero HP, linked to slot
zero. Execute the secondary's original shot callback with an ordinary beam,
damage 20. Both health words become zero, and both flash timers remain zero.

The C# audit reproduces that same callback boundary, then uses the public
production projectile-hit resolver to deliver the follow-up beam. It matches
the cartridge's 1000-to-zero health transfer and unchanged flash timers. The
preceding slot-reuse correction suffices; no further production change needed.

This confirms the follow-up mechanism but not its player-accessible timing.
The probe deliberately withholds the secondary main callback, and does not
derive that scheduling from camera visibility. Full camera-gated timing and
adjacent failing input sequences remain required before closing #443.

## Off-screen frozen processing — related #444

The same CPU probe also executes $A0:8EB6 with a single enemy at X=1024/Y=128,
camera zero and no forced-processing property. With AI-handler word zero the
active/interactive lists both begin FFFF; with bit 0004 they both begin 0000.
The C# assertion failed on the frozen control because its visibility predicate
omitted the native frozen-handler exception. Both controls pass after restoring
that exception. This covers processing membership, not crawler dislodging,
freeze duration, or Samus support across a complete stepping-stone sequence.

## Camera-gated regeneration dispatch

`native-zebetite-regen-probe.h` executes original $A0:8EB6 and $A0:8FD4
after one 100-damage missile shot callback against generation one. Twenty
independent cases keep both halves visible for 1..20 frames, then place the
camera at X=0 for the remainder of twenty frames. Samus is outside contact
range; there are no cheats or substituted enemy AI functions. This is controlled
camera input to the dispatcher, not a controller-driven camera trajectory.

All 400 native rows follow these exact formulas, with T = min(frame+1, exposure):
primary health = 900 + max(T-5,0); secondary health = 900 + T; both flash timers
= max(12-T,0); primary handler = 2 for T<5, otherwise zero. The linked shot tail
copies health/flash but does not set the secondary hurt-handler bit. Its main
AI therefore regenerates immediately. The C# audit compares every frame to
these native-verified formulas and passes without a production change.

This confirms the five processed-frame regeneration delay and off-screen timer
freeze. Repeated ten-shot kills and player-accessible camera trajectories,
including the first-barrier exclusion, remain unverified.

## Full enemy-frame double-kill window

Run `SuperMetroid.DebugRunner --zebetite-double-audit ROM zebetite-double-native.csv`.
The accepted numeric trace SHA-256 is
`E6EC8D2C87120784ACAF6EF2AF5C7F1B30663D26507E833D31C8E0CD731ED99F`.
Recapture with `native-zebetite-double-probe.h` and the original CPU loader.

Generation one starts with both halves at 100 HP (the pre-final-missile seed).
A lethal 100-damage missile strikes the lower half. Unlike the earlier isolated
callback test, original EnemyMain runs both halves normally on every active
frame. Sweep a follow-up beam over delays 0..8. At frame eight approach the
replacement primary with the camera, then continue to frame eleven.

Delays 1..5 destroy generation two as well and reach generation three, event
byte 24. Delay zero and delays 6..8 leave generation two alive, event byte 16.
The lower half's hurt dispatch holds its death for five frames while the upper
half respawns immediately. The replacement lies off screen until approached;
zero health copied into it persists until its main routine can execute.

All 108 full enemy frames match C# IDs, health, generation, secondary flash,
and progression events. C# delivers the shots through public projectile-hit
resolution; the CPU probe invokes the original shot callback, not projectile
flight. This verifies scheduling and success/failure windows, not the player's
aiming trajectory or ten-missile setup. No additional production change needed.

## Ten-missile dispatcher matrix

Run `SuperMetroid.DebugRunner --zebetite-ten-audit ROM zebetite-ten-native.csv`.
The numeric trace SHA-256 is
`35963B1F6E0D69B2CC2FC647AE969BEE27D2551A00D3B278E41AEAEF00E44C0E`.
Recapture source: `native-zebetite-ten-probe.h`.

Initialize second/fourth barriers at the native full 1000 HP. Deliver exactly ten
100-damage missiles, twenty frames apart. Each cycle keeps the actor visible
for 1, 2, 3, 4, 5, 6 or 20 frames. The remaining cycle uses camera X just beyond
the actor's right processing bound; camera zero is NOT off-screen for the fourth
barrier. After 200 frames, remain visible for twelve more frames to finish death
processing. All 14 cases / 2,968 enemy frames match C# health, flash, header and
progression events without further gameplay changes.

Exposure 1..5 kills both barriers in ten missiles; six leaves 22 HP and constant
exposure leaves 162 HP. This is a complete ten-hit health sequence, but controlled
camera jumps and injected shot callbacks do not prove the player's movement,
missile flight, legal room camera bounds or first-barrier exclusion. Those
room-local input requirements remain outstanding for #443.

## Retail right-side camera bounds

The untouched Mother Brain header $8F:DD58 specifies 4x1 screens, with four
blue scroll cells at $8F:DDC0. The loaded enemy definitions give X radius 8;
$A6:FC1B positions are 824, 632, 440, 248. Consequently the first camera X
that excludes each barrier on the screen's left is 833, 641, 449, 257.
The room's ordinary maximum camera X is 768. The first barrier cannot be
excluded by scrolling further right, while the others can. The ROM-backed
audit checks these relationships for every generation. This is a geometric
restriction, not proof of all reachable player positions or camera timing;
crossing to the far side of the solid first barrier is outside this check.

## Room-local input search (not an accepted reproduction)

`--zebetite-player-setup-audit ROM` loads the real room and its complete population,
sets generation one, seeds Samus at (696,100) with camera (641,0), then steps
ordinary inputs only. No per-frame camera writes or enemy health edits occur.
The 120-frame probe prints nearby collision types, position, pose, camera,
missiles and barrier health. The standing position settles at Y=139; crouching
reaches Y=144. Its current left/shot/jump/right candidate consumes one missile
but does not damage a barrier. It is exploratory, not a parity pass.

The second half initially does not exist because the primary is off screen.
It appears when input-driven camera motion reveals the primary. This is why
the next search must account for the barrier's initialization history as well
as missile flight through the actual terrain opening. The seeded camera
position itself has not yet been reached through controller input. All earlier
dispatcher-level evidence remains separate from this unsuccessful candidate.

### Revised single-hit candidate

The diagnostic now runs both with/without two preliminary on-screen enemy
frames. It waits for crouch completion before tapping Left once, waits for
the turn, then fires without holding Left (which would stand and run). The
missile travels left at Y145 through the real opening and damages the lower
half in both variants. Holding Left through the turn instead produced a Y131
shot that exploded against the pillar at X638. Pressing Left before crouch
completion left Samus facing right and fired away from the barrier.

Both current variants consume one missile and assert an actual barrier health
decrease. At frame 110 the halves are 919/915 HP: the camera remains on screen,
so this is not regeneration suppression or native parity. The initial camera
seed and the short exposure window around impact still need work. Projectile
positions/types/directions are now logged to make further candidates diagnosable.

### Single-hit suppression search

`--zebetite-player-search ROM` sweeps X=696..736 by four, initial camera
641..655 by two, and Right release frames 87..105 by three: 616 cases.
All use the initialized-history branch and real room movement/projectile
stepping. Twenty-four finish with one missile consumed, lower-half HP exactly
900 and camera beyond X640: X728, every tested camera seed, and release frames
99/102/105. All other sampled cases fail that combined condition.

The ordinary setup diagnostic now also asserts X728/camera641/release99 as a
positive control, X724/X732 and release96 as adjacent negative controls. These
are C# candidate-stability assertions, NOT cartridge parity or the full ten-hit
technique. Initial camera history remains seeded and repeated cycles are not
implemented. The next comparison should preserve the candidate's exact inputs
and subpixels, not replace it with the earlier stepped-camera dispatcher tests.

### Private comparison export

`--zebetite-player-export ROM OUTPUT-DIRECTORY` exports X724/728/732 with
camera641/release99. Each has a pre-input frame-60 MOV1 movement seed, a metadata
JSON describing pose history, camera and live enemy words, and 60 JSONL rows
with exact inputs, Samus fixed-point position, animation, camera/subpixels,
projectile positions/subpixels/velocities/instruction state, and barrier health.

These generated files remain local: the MOV1 seed contains decoded cartridge
room data. The existing MOV1 consumer is collision-only and does NOT import
this metadata or execute the full enemy/FX/PLM state. A dedicated consumer and
an explicit audit of omitted state are still necessary; the export is not a
native save state or a completed parity result.

### Non-Zebetite actor omission control

Export now also runs each candidate with all other enemy slots cleared at the
frame-60 boundary, after the identical initial room history. All 180 JSONL
rows across the three original/isolated pairs match byte-for-byte: inputs,
Samus fixed-point/animation state, camera, missile trajectory and barrier
health/flash/handler bits. This proves those actors do not change the selected
observables during this interval. It does not prove audiovisual equivalence,
later-room behavior, or that live FX/PLM state can also be omitted. The metadata
explicitly identifies isolated exports. Native actor import may preserve the
two Zebetites at physical slots 128/384; it must not renumber linked slots.
