# Ordinary walljump parity — #473

Partially resolved diagnostic; #473 remains open. Include `native-release-probe.h` and
`native-walljump-probe.h` after `struct StateRecorder;` in pinned `sm_rtl.c`.
In `main.c`, before SDL, dispatch `DiagnosticWalljump(argv[2], argv[3])` for
`argc == 4` and `--walljump-probe`. Build Release/x64/v145 with absolute upstream
SolutionDir. Run on the same unpatched cartridge documented in SPINJUMP.md:

```
sm.exe --walljump-probe "Super Metroid.smc" NEW_TRACE.csv
dotnet run --project csharp/src/SuperMetroid.DebugRunner -c Release --no-launch-profile -- --walljump-comparison-audit "Super Metroid.smc" NEW_TRACE.csv
```

The native output is exclusive-create and contains no ROM or save data. Temporary
upstream integration was removed after the run. Both sides use a floor at Y=256,
a full-height solid column (7 or 8), initial spin pose toward the wall at
X=134/122, Y=160 with zero subpixels/speeds, animation frame zero/timer one and
falling direction. No equipment, enemies, grapple or gameplay cheats. Away is
held from frame zero; Jump begins at delays 0..12. Actual input, movement,
animation and pose transitions run for 30 frames in each of 26 cases.

Initial result: 741 of 780 exact X/Y/pose/animation samples differ. First rightward
away case diverges at frame 2: managed X=0086.C000, native=0086.0000, with matching
Y=00A0.5400, spin pose $19 and check animation frame $0B. Native horizontal motion
then advances in whole pixels while the wall-check contact remains active.
This is a lead, not an established cause: verify native/managed initialization
and collision-probe side effects before changing gameplay.

The printed managed success windows are observational, not golden expectations:
rightward away delays 2..6, leftward away 2..5. The comparer must continue to fail
until the exact discrepancy is diagnosed and resolved. Repeated same-wall jumps,
overhangs and post-walljump Up/Down/charge rules are still outstanding.

## Fractional collision write correction

The native solid horizontal dispatcher ($94:8F49) writes live X subposition
even when reached through the observational wall probe ($94:967F): zero on a
leftward hit and FFFF on a rightward hit. The managed copied probe discarded
those writes. Propagating the block-hit fractional word, without committing
integer position, reduces the same trace from 741 mismatches to 13. All 13 are
the first frame of the leftward-away cases; every subsequent frame matches
exact X/Y, pose and animation. Both managed success windows now accept delays
2..8 in this specific geometry. This is not a universal timing claim.

Full core verification passes, including new direct block-probe assertions for
both hit directions and a no-hit case: integer X/Y remain unchanged, block
contact changes only X subposition, and air preserves the original fraction.
The full native comparer intentionally still exits nonzero. The native
last-different-movement gate at $90:9D35 is an investigation lead for the
remaining initial-frame discrepancy, not yet an implemented or verified fix.

## History isolation

The updated probe emits 1,560 samples: it repeats every case with native
`samus_last_different_pose_movement_type` initialized to zero, then to three.
The new leading `history` column identifies those initializations (0/1).
No other setup or input differs. The comparer still accepts the original
780-sample trace. The managed fixture currently cannot seed an equivalent
history field; it intentionally runs the same managed setup for both halves.

Result: all 780 samples with native history three match; the history-zero
half retains exactly the same 13 initial-frame fractional-X mismatches.
The cartridge's $90:9D35 gate rejects the probe unless last-different movement
is spinjump or walljump. C# currently probes without that gate. This isolates
the missing state distinction, but does not prove its frequency in an actual
playthrough. A proper implementation must preserve the native pose-history
update phase and initialization/save-state behavior, not skip an arbitrary
first frame or modify the expected trace. Native $91:EB88 transitions update
the previous and last-different pose/movement fields; inspect all relevant
forced-transition owners before installing the gate.

The headless probe completed and the expanded comparer exits one as expected.
Temporary native integration was removed afterward. No additional gameplay
fix or player-validation claim is made by this diagnostic expansion.

## History primitive (not yet connected to live Samus)

`SamusPoseHistoryState` implements the four word stores in $91:E719 and the
older-movement admission test in $90:9D35. It deliberately does not sample on
every frame or attach to the Pose setter. Native transitions can commit the
same pose, and intermediate assignments/rollback are not equivalent to a
committed transition. The runtime transition dispatcher must explicitly own
this operation, with forced-transition paths and legacy save restoration
accounted for before enabling the gate in gameplay.

The native probe now first executes $91:E719 twice against the same current
pose and checks all four output words, including preservation of a full
16-bit previous pose. The matching C# fixture passes, as do all 256 movement
byte eligibility cases and full core verification. Native temporary integration
was removed after the successful run. This preparatory state type is not yet
owned by live Samus and does not change gameplay or the debugger-state schema.

## Samus ownership and state compatibility

The next increment adds lazy `SamusState.PoseHistory` ownership and an explicit
`CommitPoseHistory(bus)` operation. Transition call sites and the live walljump
gate remain unconnected; gameplay admission is unchanged in this increment.
Unlike the preceding primitive-only revision, the debugger schema now includes
the nullable owner. Current graph round trips preserve all four history words.
The four supported prior Samus layouts omit this owner with an explicit warning
and retain the existing auto-jump/draw-input migrations. Unavailable historical
words restore neutral, not guessed from the current pose; a future live gate must
account for that information loss. An unknown intermediate layout is rejected.

DiagnosticsVerification's default host/state checks and its explicit
`--legacy-options-migration` command pass. The latter verifies field identity/order,
constructor-bypassed legacy access, unknown-layout rejection, and exact history
plus pending auto-jump round trips. Ordinary/forced transition producer wiring
and native motion comparisons remain required before calling #473 implemented.

## Normal transition integration and gate

The ordinary runtime transition epilogue now commits history when it consumes
an animation, input, or fallback transition, including self-transitions. The
walljump checker tests the older movement byte before either collision probe,
so a rejected probe cannot write fractional X or rewind the animation.

The trace now includes all four history words on every frame. With both sides
seeded to the same previous and older pose metadata, all 1,560 samples match
exact X/Y, pose, animation, and history words (zero mismatches). This fixes the
13 initial-frame discrepancies without a first-frame special case. The full
core verification suite passes. Its mid-spin wall fixtures now explicitly seed
their prior spin transition; a separate rejection fixture verifies no wall
contact, no animation rewind, and no right-probe fractional-X mutation when
history is ineligible. Temporary native integration is removed.

This verifies the normal spin-turn/wall-launch paths exercised by these cases,
not every forced transition owner. Audit forced pose writes outside this
epilogue, legacy-state entry behavior, repeated same-wall/overhang trajectories,
and post-jump input rules before marking the whole issue awaiting validation.

## Grapple forced wall-launch history

The live runtime fixture now also starts at the queued $9B:C9CE function in
each wall-contact direction and advances one real gameplay frame. Before the
fix, $B8 became $84 but history stayed B8/1608/0001/0008: the launch's early
pose application cleared ordinary alpha input and never reached the history
condition. Native C9CE publishes a transitional pose and command six, whose
$91:EB88 epilogue performs the ordinary history shift.

The common epilogue now recognizes `WallJumpStarted` as a consumed transition.
Both directions verify all four words after the live frame, including the
older contact pose so a double commit would fail. The 1,560 ordinary native
samples continue to match exactly. This covers forced grapple wall-launch
history only, not every grapple animation/timing property or other forced
transition owners.
