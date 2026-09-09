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

## Post-launch input matrix and morph alignment

The newest trace contains 6,240 frames with a leading `postInput` column.
Every original case repeats with Morph Ball equipped and four input modes:
0 unchanged; 1 add Up at frame 12 while holding Jump; 2 add Down at frame 12;
3 add Up and release Jump at frame 12. Away remains held. Failed launch windows
remain included as negative controls. The comparer accepts older trace layouts.

The first run found 980 motion/pose/animation mismatches with zero history-word
mismatches. Down-to-morph from walljump first differed by three pixels in Y:
managed 0082.C800 versus native 007F.C800 at frame 12 (rightward, delay two).
The managed morph initializer incorrectly used source radius minus target radius.
Native command seven instead reads a fixed nine-pixel request from $91:ED3A/C,
then uses the new radius in the two-stage block-only $94:96AB probe. The fix
uses that request and collision path, with its value in SamusPostureDefinitions.

Full core verification passes, including open-space compact morph (+9) and a
near-floor case clipped to +3. Post-input modes 0, 1 and 2 now each match all
1,560 frames. Mode 3 still has 476 mismatches: the first is a fractional-X
offset at frame 13, with matching Y/pose/animation. All history words match.
The aggregate audit deliberately remains failing until that separate mismatch
is resolved. Charge-release, repeated-wall, and overhang coverage remain open.
Temporary native integration was removed after capture.

## Released-Jump fallback momentum

The latest trace additionally records base speed, extra speed, acceleration mode,
and divisor (18 columns). This exposed 2,240 register mismatches even where
positions previously matched. Native walljump movement changes mode zero to
two after its base-speed calculation and bypasses the direction-held gate;
the shared managed aerial mover omitted that branch. It now preserves this
write and the no-held-direction movement behavior.

After that correction, 56 speed-word mismatches remained. At frame 12 of the
released-Jump+Up case, the native walljump-to-spin fallback cleared base speed
and mode; C# retained 1.6000 and mode two. Type $14's entry in $91:8304 selects
command six ($91:EC85), which clears horizontal momentum after initializing
the fallback spin pose. The runtime now performs that teardown only on this
definition-fallback path, leaving directional spin transitions unchanged.

All 6,240 frames now match motion, pose, animation, history, and the added speed
words. The full core suite and a focused no-held-direction walljump regression
pass. This completes these four post-input matrix modes, not the remaining
charge-release, same-wall/overhang, or full forced-owner coverage in #473.

## Same-wall return and second launch

Mode four expands the trace to 7,800 frames. Starting from the same first-jump
cases, frames 12..20 hold Jump toward the original wall; frames 21..22 turn
away without Jump; frame 23 onward holds Jump away. No position, velocity,
pose, or animation resets occur between launches. Both directions and both
initial history variants run through the real input/movement/transition paths.

Native and managed traces match all 7,800 samples, including every history and
speed word. Each of the 28 delay-2..8 cases launches twice; the remaining 24
cases miss the first launch and execute one later launch. The comparer asserts
these counts so an unsuccessful return cannot masquerade as same-wall coverage.
The native counts were observed before adding these fixture-validity checks.

This is verified consecutive same-wall jumping, not an arbitrary route test or
proof of overhang clearance. No additional production fix was required. The
native temporary integration was removed. Delayed overhangs, charge release,
and the remaining forced-owner audit are still outstanding.
