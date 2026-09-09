# Spinjump input timing — #474

`native-spinjump-probe.h` uses the existing unpatched-ROM loader from
`native-release-probe.h`. To reproduce, include both headers from `sm_rtl.c`
after `struct StateRecorder;` and dispatch `DiagnosticSpinjump(argv[2], argv[3])`
from `main.c` for `argc == 4 && strcmp(argv[1], "--spinjump-probe") == 0`,
before SDL initialization. These temporary integration edits are not retained.

Build `upstream-sm/src/sm.vcxproj` Release/x64 with installed toolset v145
and `SolutionDir` set to the absolute `upstream-sm` directory (trailing slash).
Run `sm.exe --spinjump-probe "Super Metroid.smc" csharp/test-temp/spinjump-native.csv`.
The upstream CPU runner prints one debug line per call; stdout may be discarded.
The probe itself writes the CSV. It does not open a window or modify saves.

Compare using:

```
dotnet run --project csharp/src/SuperMetroid.DebugRunner -c Release --no-launch-profile -- --spinjump-comparison-audit "Super Metroid.smc" csharp/test-temp/spinjump-native.csv
```

144 cases: both facings, dry/submerged, four input scenarios, delays 0..8,
24 warmup frames and 40 sampled frames. Scenarios are direction-before-jump,
jump-before-opposite-direction, and Jump held for the last input-locked warmup
frame before unlocking and pressing the opposite direction at the chosen delay.
The fourth scenario starts falling at Y=233.FFFF toward a floor at Y=256, with
opposite direction held and Jump first pressed at delays 0..8. This crosses the
actual landing boundary and the preceding aerial-turn animation.
No equipment or gameplay cheats. Native calls include collision-radius refresh,
the installed input handler ($90:E90F), gravity refresh, movement, animation,
pose collision/transition stages, and draw-time input history ($90:EAB3).
The comparer asserts every sampled X/Y fixed-point position, pose, movement type,
auto-jump timer and pending-handler selection.

Initial result: 68 mismatches out of 2,880 samples. Both dry direction-first
cases at delay 6 diverge at frame 6: native accepts spinjump ($19/$1A), managed
finishes the turn into standing ($01/$02) and loses the fresh Jump. The remaining
34 frames of each case diverge. This is an opt-in failing reproduction, not a fix.
Do not weaken the assertion to accept the managed trace.

The production F8 interpreter now receives alpha's prospective input pose and
preserves the four jump targets explicitly tested by native `$90:8370`. It no
longer unconditionally publishes the higher-priority standing transition.
After this change all 2,880 frame positions and poses match. The core suite also
checks four jump targets, absent/non-jump targets, and the locked-input exception.
This fixes the reproduced turnaround-completion input loss, not the remaining
elevator-specific portion of the ticket.

Expanded result: all 4,320 samples match with the installed auto-jump handler and
draw-time Jump history included. In the dry zero-delay input-unlock case, frame
zero enters turn pose $26, frame one enters spin pose $19, and movement starts on
frame two, exactly matching the cartridge. Both facings and water are checked.
This is an input-unlock fixture, not a complete elevator actor/room reproduction.

The near-floor cases initially produced 632 divergent samples: the cartridge
installed $90:E926 at both the F8 endpoint and ordinary landing ($91:F1EC), while
the port had neither the one-shot handler nor its $0AF4 timer. The translated
handler substitutes Jump only for the pose lookup, restores ordinary handling,
and leaves the physical controller edge unchanged. Draw-time $90:EAB3 history
now maintains the exact timer, including 16-bit wrapping and signed comparison.
All 5,760 samples pass after this fix. Unit tests cover timer 0, 1, 8, 9, signed
boundaries, consumption, release and consecutive draw samples. Debugger tests
cover both known older Samus layouts and preservation of a live pending handler.

This does not cover full elevator departure or all landing/animation states.
Those remain part of #474. In particular, Blue Brinstar and Lower Norfair elevator
exceptions cannot be inferred from this synthetic flat floor.

## Destination-room elevator diagnostic

`--elevator-spinjump-audit ROM` runs the actual destination loader, door setup,
terrain and elevator actor in three rooms. It seeds arrival status directly,
not a source-room journey or the complete frontend fade. Samus starts in the
departure front-facing pose with input locked. Jump remains held through arrival;
Left is added 0, 1, 4 or 8 frames after the arrival-completed event.

Observed managed results:

| Destination | Incoming door | Arrival calls | Released Y | First spin after release |
| --- | --- | --- | --- | --- |
| Blue Brinstar / Morph Ball | $8B9E | 455 | 680 | delay + 2 |
| Green Brinstar main shaft | $8C0A | 455 | 680 | delay + 2 |
| Lower Norfair main hall | $96F6 | 434 | 648 | delay + 2 |

The command prints exact X/Y, pose and vertical direction for the first twelve
post-arrival frames, including Lower Norfair's subsequent ceiling collision.
These observations are NOT cartridge golden values, and the command deliberately
does not assert that the player technique is correct merely because spin begins.
The wiki's two elevator exceptions still require native comparison of clearance,
arrival timing and the useful departure trajectory. Pre-completion direction
buffering and frontend handoff are also outside this diagnostic's present scope.

### Cartridge post-release comparison

Pass a third argument (a local output directory) to `--elevator-spinjump-audit`
to export twelve private MOV2 movement seeds and matching 40-frame managed CSVs.
MOV2 retains the MOV1 32-word header and room geometry, then appends live scroll
owner block/trigger records and four auto-jump history words. Generated seeds
contain cartridge-derived data and are not committed.

Apply `elevator-integration.patch` to the pinned upstream checkout and build as
above. This dispatches before SDL and adds a per-call CPU instruction budget, so
bad fixtures fail on the console rather than hang. Execute for each room
`9E9F`, `9AD9`, `B236` and delay `0`, `1`, `4`, `8`:

```
sm.exe --elevator-release-probe ROM DIR/ROOM-DELAY.movement-seed DELAY DIR/ROOM-DELAY.native.csv
dotnet run --project csharp/src/SuperMetroid.DebugRunner -c Release --no-launch-profile -- --elevator-spinjump-compare DIR
```

All 480 ordered position/pose samples match the original CPU. This includes the
Lower Norfair ceiling collision. Earlier the missing scroll-owner seed caused
the cartridge's deliberate $84:B3A6 crash loop; including the actual owner blocks
restores that collision contract. The probe deliberately does not reproduce
native PLM slot ordering, execute PLM programs, move the camera, or run enemies.
Consequently it proves post-release collision/movement in this interval, not
native elevator release timing, pre-release inputs, or full scene parity.
The three temporary upstream edits were removed after the comparison.

### Arrival actor comparison (incomplete handoff investigation)

The same managed audit now emits `.actor-seed.csv` and `.actor.managed.csv`.
Run `sm.exe --elevator-actor-probe ROM PREFIX.actor-seed.csv PREFIX.actor.native.csv`
using the integration patch. This executes the cartridge's $A3:952A dispatcher,
including its real Samus unlock command, from the destination actor's initial
position. Columns are frame, actor Y, Y subposition, elevator status, elevator
flags, and input-locked state. The native side excludes other room actors and
Samus movement, so it isolates elevator AI rather than asserting full-room parity.

For delay zero, all 1,344 ordered actor coordinate/status/flag samples match:
455 frames each in Blue/Green Brinstar, 434 in Lower Norfair. However, the managed
fixture's input lock clears on frame 59 in all three rooms, whereas the isolated
native actor clears it only on arrival completion. This discrepancy is NOT fixed
or dismissed: trace the other managed owner and determine whether the diagnostic
setup or production scheduling is responsible before claiming handoff parity.
The front-facing elevator-status input gate remains a separate mechanism.
Temporary upstream integration was removed after collecting these traces.

The early-unlock discrepancy was traced with a temporary setter stack trace to
`SuperMetroidRuntime`'s Ceres arrival completion, not the ordinary elevator actor.
The fixture had bootstrapped Ceres but bypassed the room-lifecycle reset when
calling the low-level door verification seam. Normal pending-door loading already
clears this owner. The audit now uses `LoadCartridgeRoomForDebug` to leave the
bootstrap room before preparing the actual elevator arrival, and asserts that no
Ceres arrival owner remains. No production behavior was changed.

After that correction all 1,344 complete actor rows, including input lock, match
the retained native traces. The 480 post-release position/pose rows still match.
`--elevator-spinjump-compare` now checks both ordered comparisons and requires a
terminal unlocked actor state. This still excludes frontend fade and direction
inputs before elevator completion; it is not full technique acceptance yet.

### Direction held before release

`--elevator-spinjump-preheld-audit ROM DIR` holds Jump and Left throughout arrival.
The production runtime must retain locked pose zero and publish no prospective
input pose on every frame before actor completion. Those negative assertions pass
in all three rooms. At completion the managed runtime selects turn pose $25,
then spin $1A one frame later (one frame earlier than adding Left after release).
The sampled trajectory hits the Green Brinstar and Lower Norfair ceilings, while
the Blue Brinstar trajectory continues upward. These are recorded observations,
not yet cartridge golden assertions for the pre-held case. Its MOV2 seed now
contains turn pose $25; the older forward-pose-only native release probe must not
be used on it without explicitly extending its initialization contract.

The separate `--elevator-preheld-probe ROM SEED CSV` now explicitly accepts only
the release turn pose $25 and carries Left as held input without inventing a new
press on frame zero. It restores the existing MOV2 animation, velocity, radius,
pose metadata and input history before running the original CPU stages. All 120
ordered X/Y/pose samples match in the three rooms, including both ceiling hits.
Use `--elevator-spinjump-preheld-compare DIR` to verify these traces separately
from the delayed-direction fixtures. This comparison still starts after the
release-frame turn selection; that selection and frontend scheduling must not be
claimed as native-verified by this seeded post-release test.
