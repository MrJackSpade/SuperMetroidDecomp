# Ordinary walljump parity — #473

## Current acceptance status

Ready for player validation of the implemented #473 fixes. This section supersedes
the chronological partial/failing statuses below. Cartridge SHA-256 was rechecked:
`12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72` (unheadered
Japan/USA country 0/revision 0, checksum F8DF). Pinned reference revisions are
recorded in SPINJUMP.md; this is not a PAL or modified-ROM parity claim.

The current probe/comparer covers 17,160 original-CPU frames with exact position,
subpixels, pose, animation, four history words and horizontal speed state. Charge,
contact damage and projectile count are additionally compared in the charged mode.
Both facings, initial Jump delays 0..12, and admitted/rejected older movement seeds
are covered. Modes 0..8 run 30 frames each; mode 9 runs 60. No enemies or RNG affect
these fixtures; Morph Ball is equipped, Charge only in mode 7, and gameplay cheats
are absent. Inputs and all seeds are authored explicitly in the probe header.

- Away/check/Jump success windows and adjacent failures: mode 0.
- Post-walljump Up, one-Down morph while holding Jump, and released-Jump Up: modes 1..3.
- Repeated same-wall jumps: mode 4.
- Early underside collision versus delayed clearance: modes 5..6.
- Charged-spin hold/release without spurious projectile creation: mode 7.
- Second launch beneath the overhang, including body expansion: mode 8.
- Cleared-overhang side jump and follow-through: mode 9. Initial Jump frame 7,
  return on 14, away on 25, second Jump on 29. Frame 28 asserts compact check pose,
  animation 0B, X=150 (mirror 106.FFFF), Y=99.3400. Frame 29 asserts walljump pose,
  animation zero and X=151 (mirror 105.FFFF), same Y. This is beyond the original
  wall's reach and above the lip's lower edge. Both native/managed launches are
  required. Nearby initial delays remain in the full per-frame comparison.

The initial 60-frame probe had an undefined upper-room boundary; its last seven
frames differed after Samus left the constructed geometry. Mode 9 now has the same
explicit row-zero solid ceiling in both engines, preserving the side-jump setup
while keeping follow-through in bounds. The corrected capture is
`csharp/test-temp/walljump-overhang-side-native-473-v2.csv`; the earlier non-v2 file
is not the accepted fixture. Temporary native integration and candidate-search code
were removed after capture. Numeric captures contain no ROM/state payloads.

The production corrections and exact reproductions are documented below, including
probe fractional writes, history gating/owners, post-jump momentum/morph behavior,
charge preservation and overhang body expansion. Focused tests and the full
Verification suite passed with the latest production correction. Legacy field
migration and 90-frame transition-owned history repopulation also pass; missing
mid-air history in old saves remains explicitly unavailable, not guessed.

This completes this ticket's scoped technique investigation, not arbitrary room,
loadout, host-controller or revision coverage. Related player reports retain their
own status; the ticket stays open for player confirmation.

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

## Overhang underside / ceiling-contact transition

Mode five adds a full solid tile adjacent to the wall at block row seven,
overhanging toward Samus. It expands the trace to 9,360 frames. This first
geometry does NOT demonstrate delayed clearance: every admitted native launch
hits the underside at center Y=147. It is retained as a ceiling-contact fixture,
not counted as completing the delayed-overhang technique described by the wiki.

It reproduced 596 history-word mismatches despite exact motion/animation/speed:
native $91:E8E5 publishes the current pose with momentum command five on a
ceiling hit. That same-pose transition still shifts history. The runtime had
already stopped vertical speed but omitted this transition's ownership. It now
consumes ceiling contact after higher-priority animation/hurt interruptions,
suppresses the lower-priority ordinary input target, and shifts history through
the common epilogue without restarting animation.

All 9,360 frames now match position, pose, animation, history and speed words.
Full core verification passes. Temporary native integration was removed.
Delayed overhang clearance still requires geometry where early and late native
launches actually have different clearance outcomes.

## Delayed clearance outcome

Mode six places the one-tile overhang at row six (Y=96..111), keeping mode five
as the closer underside control. The expanded trace has 10,920 frames. The
cartridge shows the required timing distinction in both directions and both
history variants: delays 2..6 launch but hit the underside (minimum center
Y=131), delay seven clears it (minimum Y=85), and delay eight also clears it
(minimum Y=88). Delays 0/1/9..12 never launch. These observed outcomes are now
explicit fixture-validity assertions, alongside exact per-frame comparisons.

All 10,920 samples match C# motion, pose, animation, history and horizontal
speed state. No additional production fix was required. Temporary native
integration was removed. This verifies delayed clearance around the overhang;
the fixture stops before a subsequent jump off the overhang's side and is not
a full retail-room traversal. Charge-release and remaining forced-owner/legacy
entry checks are still pending for #473.

## Charged-spin Shoot release

Mode seven expands the native comparison to 12,480 samples and adds charge,
contact-damage and projectile-count words. It starts with charge sixty, holds
Shoot through the attempted launch frame and releases it on the next frame.
Both directions, both history seeds and all thirteen Jump delays are retained.
The initial hold-through-frame-twelve experiment exited spin in the cartridge;
its incomplete CSV is not accepted as evidence for a spin-release test.

The native probe calls the actual JumpEtc HUD handler for the spin/walljump
types, with inactive grapple, before beta resets contact damage. It does not
model a full native projectile loop: the fixture requires zero projectiles,
and this native HUD handler creates none. Managed charge is built through the
real projectile producer before the movement seed, not just its Samus mirror.

This reproduced 1,560 charge/contact mismatches with matching motion: C# ran
the normal beam producer during spin, incrementing charge while held and
consuming it on release. The production spin/walljump dispatch now preserves
charge when grapple is inactive, while continuing to step existing projectiles.
All 12,480 frames match position, pose, animation, history and speed; the 1,560
charge-mode frames also match charge, contact damage and projectile counts.
Native charge stays sixty, with no projectile; contact damage is four on
1,280 frames and zero on the other 280, following walljump animation timing.

The automatic core regression checks hold/release in all four spin/walljump
poses and ordinary release after leaving spin. Temporary native integration
was removed. This completes the charged-spin release comparison, not the
remaining forced-history-owner/legacy-entry audit or a subsequent overhang-side
jump. Issue #473 remains open until its remaining acceptance work is complete.

## Forced forward-facing owner

The forced-owner audit found that `ApplyForwardFacingPoseSetup` omitted the
four stores at native `MakeSamusFaceForward` ($91:E420-$E435). Both pinned C
and disassembly show this shift immediately after pose/animation initialization,
including calls that leave the pose unchanged. The shared owner is used by
elevator departure, accepted saves, gunship entry and appearance setup.

A focused production-method regression seeds previous spin and older walljump
history. Before the fix the first assertion failed (expected previous spin
pose $19 shifted into older history, got stale walljump $84). The shared setup
now commits history after initialization. The regression checks all four words,
then repeats the same-pose setup and verifies that obsolete spin eligibility
is gone. Full core verification and all 12,480 native walljump samples pass.
This is a history-state fix, not a claim to reproduce elevator camera defects.

The remaining native direct-write owner inventory is below. It is an audit
checklist, not a claim that every listed owner is defective. In addition to the
normal transition epilogue and forward-facing setup already verified, inspect:

- Crystal Flash entry ($90:D5A2).
- Ridley push-out ($90:E12E), Draygon grab/release ($90:E23B/$E2DE).
- Callers of the shared previous-pose helper ($90:F0EE).
- Ceres initial history reset ($90:F1E9) and Samus initialization ($91:E00D).
- Demo pose commands ($91:86FE unused, $91:8739 live).
- X-ray stand-up ($91:E2AD), Mother Brain script dispatcher, and frozen input
  pose handling ($91:FCAF).
- Item fanfare ($92:ED24), death entry ($9B:B3A7), suit pickup completion
  ($88:E320/$E361), and cinematic demo initialization ($8B:AEB8/$AF6C).

Some initialize both samples rather than shifting; some have no gameplay return.
Do not replace these owners with a blanket per-frame or pose-setter update.

## Draygon grab/release history owners

Both forced owners omitted their native history shift: grab at $90:E271-$E286
and release at $90:E30B-$E320. The pinned disassembly and C agree that these
owners publish history themselves and clear pending transition slots, rather
than waiting for normal input dispatch.

The constructed production-owner regression failed before the change: grabbing
retained older walljump $84 instead of shifting previous spin $19. Both facing
directions now test all four words on grab, repeated grab and release, including
removal of pre-grab spin eligibility. Begin and Release call the existing shared
history shift after pose initialization. Ordinary grabbed aiming still belongs
to the runtime transition epilogue and was not given a second commit.

An additional live-runtime check alternates sixty D-pad edges for each facing
direction and verifies that the release frame shifts exactly once, preserving
the actual previous-frame grabbed history. Both cases pass, as do all 12,480
walljump comparison samples. These are history regressions, not new claims about
Draygon's rendering, grapple vulnerability, or complete battle behavior.

## X-ray forced turning and teardown

The frozen input handler has direct shifts after both turn start and turn
completion ($91:FD29-$FD3E and $91:FD94-$FDA9). Teardown at $91:E2AD has
another direct shift after pose initialization. These bypass ordinary input
transition dispatch. All three were absent from the managed X-ray owner.

The regression seeded previous X-ray pose $D5 and older spin $19. Turn start
failed before the fix, retaining $19 where the native stores require $D5.
The shared X-ray turn initializer and teardown now commit history themselves.
Exact four-word assertions cover turn start, completion and release during a
crouched turn; the latter still expands radius 16 to 21 and raises center Y
five pixels, preserving the native stand-up glitch. Initial X-ray activation's
interrupted-slot ownership remains a separate audit item; this change does not
claim to complete that path or unrelated X-ray technique tickets.

## Runtime X-ray activation history handoff

Native setup ($91:E16D) publishes an interrupted pose with command five; the
normal transition handler later commits history. The managed runtime installs
that pose early and clears ordinary pending targets. It omitted the handoff
to the final history epilogue. A live room-local control test failed before the
fix: activation selected $D5 but previous-pose history remained $00.

The runtime now tracks successful activation for this frame and includes it
in the existing final history commit, alongside other consumed transition
owners. It does not commit every frozen frame or move the history shift into
the early admission call. Tests exercise default and swapped Run/Shoot bindings,
assert all four words on activation, and hold scanning for ninety frames to
verify history does not shift again without another transition. Direct diagnostic
admission alone is not a full gameplay frame and is not claimed as one.

## Suit acquisition entry and reveal

Both native entry routines call Samus command $15 after initializing the
front-facing pose. That command calls the shared history updater ($90:F310).
The later Varia/Gravity reveal phases separately shift history at
$88:E340-$E355 / $88:E381-$E396. Both boundaries were missing in C#.

The production-owner regression failed on entry before the fix (previous spin
$19 should replace stale older walljump $84). Begin and RevealSuit now each
commit at their native boundary. Four cases cover both pickup kinds with and
without the other suit already equipped. They verify all four history words
after entry and after the actual stepped reveal, including the same-pose $9B
reveal case. The change restores history only; it does not change suit graphics,
palette selection, transformation timing, or motion.

## Crystal Flash activation

The successful native HDMA activation shifts history at $90:D616-$D62B after
initializing the Crystal Flash pose and confirming movement type $1B. C# omitted
that shift. A production-owner assertion failed before the fix (expected prior
standing pose $01 in older history, found stale spin $1A).

TryBegin now commits after successful pose initialization. Tests verify all four
history words for right and left activation and retain an adjacent rejected-input/
vertical-velocity case that must not shift history. Existing resource consumption,
raising/lowering, palette, bubble and animation tests remain in the same suite.
This completes the activation history owner, not independent Crystal Flash
advanced-technique acceptance or the remaining global owner audit.

## Ceres Ridley ejection initialization

Native gamma initialization shifts history at $90:E14F-$E164 after selecting
the hurt pose. The preceding request only installs the handler; subsequent shove
frames do not repeat this shift. The managed initialization omitted it.

The production-handler test failed before the change: prior standing $01 should
replace older spin $1A. Initialization now commits history after pose/animation
setup. The regression checks all four words and verifies request promotion does
not update history early. Existing assertions still check no motion on the first
gamma call and the ordinary wall-contact termination/feet alignment. This is
only the missing history publication, not a change to the ejection trajectory.

## Fresh initialization and death entry

Fresh Ceres setup allocates a new SamusState. The pinned ROM's pose-zero
definition bytes at $91:B629 are `00 00 FF FF 08 00 18 00`: its packed direction/
movement word is zero. Thus both initial history samples already match the
explicit zeros in $91:E00D and the two current-pose copies in $90:F1E9. No
production change was needed for this fresh-ROM initialization path; this is
not proof of legacy debugger-state history recovery.

Death entry does explicitly shift history ($9B:B3EA-$B3FF), after pose
initialization and before its frame-table override. C# omitted this publication.
The new production-owner assertion failed before the change (expected prior
standing $01, found stale spin $1A). The owner now commits at that boundary.
Tests assert all four words for right-standing and left-Morph entry, alongside
the existing differing initial frames and complete death-sequence checks.
Death has no ordinary gameplay return; the fix is exact state parity, not a
claim that this omission caused an observed post-death walljump failure.

## Saved-game appearance completion

The native `PlaySamusFanfare` owner ($92:ED24) is called by saved-game appearance,
not the ordinary item-message flow. On call 360 it shifts history at
$92:ED57-$ED6C before gameplay handlers are restored. The managed countdown
unlocked input at the right time but omitted that same-pose history shift.

The live saved-game fixture seeds a distinct older sample to make the write
observable. It verifies no shift through call 359, then all four words at call
360. Before the fix the completion assertion retained seeded spin $19 instead
of prior front pose $00. The completion owner now commits immediately before
unlocking. Existing palette-lifetime and no-repeat-save-prompt checks are retained.

## Mother Brain scripted Samus history

The native controller dispatcher shifts history for commands zero, one, two,
and four (fall, stand, release and crouch). Command three enables Hyper Beam
and returns without a shift. Shared rainbow lock setup separately calls the
previous-pose helper at $90:F3A5. The managed owners omitted these publications.

A production-owner command-sequence regression failed before the change on
fall (expected prior crouch $27, found stale older spin $19). The four command
owners and shared rainbow setup now commit history. Two sequences begin in
opposite facing crouches, check all four words after each command, and include
same-pose releases. Hyper activation must retain all four words unchanged.
Full core verification and all 12,480 native walljump samples pass. This fixes
history publication only, not an independently reported Mother Brain visual
or combat issue; remaining audit items are not marked complete by this test.

## Cinematic/demo audit frontier

Inspection of the remaining bank-$8B/$91 demo owners found a coordinated gap,
not just another isolated missing setup store:

- Mother Brain flashback setup creates a new SamusState and omits the native
  $8B:AEB8 history shift. Its $91:8739 terminal demo command likewise omits
  the explicit shift after selecting pose two.
- Baby discovery creates another new SamusState. Native $8B:AF6C reuses the
  existing Samus history and shifts it; a new zero-seeded history is not equivalent.
- IntroSamusDemoMovement applies prospective/fallback transitions independently
  of the gameplay runtime and does not publish their history. Same-pose fallback
  must be covered, not just changed pose IDs. Mother Brain's separate flashback
  coordinator also needs its accepted animation/input transition seam inspected.
- $91:86FE is explicitly unused in the pinned native source; $91:8739 is the
  live terminal command. Do not assume both are exercised by the retail sequence.

No cinematic production change was made from this inspection. The next fixture
must follow history across scene setup, ordinary demo transitions, terminal
command, and discovery setup. Patching just the final setup calls would conceal
the missing intermediate history and cannot establish parity. Existing snapshot
pixel equality does not assert these state words.

Completed gameplay-owner checks are documented above: ordinary transition,
grapple launch, forward setup, Draygon, X-ray (activation/turns/teardown), suits,
Crystal Flash, Ceres ejection, fresh Ceres initialization, death, load appearance,
and drained controllers. Remaining work includes this cinematic/demo frontier,
the unreviewed shared-helper callers, legacy-state entry behavior, and the
overhang-side follow-up jump. This list supersedes the earlier undifferentiated
direct-write inventory; it does not mark #473 ready for player validation.

### SR388 ordinary demo transition history

The focused `VerifyIntroPoseHistory` fixture reproduced a stale-history failure
through `IntroSamusDemoMovement.StepGroundedLeft`: standing-to-running left kept
the seeded older spin pose (25) instead of shifting the previous standing pose (2).
Pinned `sm_91.c` `Samus_HandleTransitions` sends both changed-pose transitions and
same-pose deceleration commands through its four-word history epilogue. The intro
coordinator now uses the shared history commit after an accepted prospective or
fallback slot, including unchanged-pose fallback, and does not commit idle frames.

The synthetic flat room uses retail pose/input tables and real grounded movement.
Assertions cover all four history words for idle, changed-pose, and same-pose
fallback frames. This fixes the ordinary SR388 coordinator only: cinematic scene
setup, Mother Brain flashback transitions, terminal demo command, and discovery
history handoff remain separate work. It does not establish whole-cinematic parity
or finish #473.

### Cinematic setup, hurt sequence, and discovery handoff

`VerifyIntroScenePoseHistory` now plays the retail intro through discovery setup
with normal render/step ordering and periodic narration-advance input. Before the
fix it reported eight failures: flashback setup (tick 1504), pose transitions
02->54 (1670), 54->2A (1681), 2A->A5 (1711), A5->02 (1718), terminal demo command
(1897), and owner replacement plus missing history shift at discovery setup (2820).

Pinned native setup routines `CinematicFunction_Intro_WaitInputSetupMotherBrainFight`
and `CinematicFunction_Intro_WaitInputSetupBabyMetroid`, terminal `DemoInstr_Func3`,
and the shared dispatcher called by `Samus_Func15` explicitly shift the four words.
The corresponding frontend owners now do so. Flashback movement commits once after
its final accepted hurt/animation/landing transition. Discovery reuses the existing
Samus owner instead of allocating a zero-history replacement; standalone discovery
diagnostics can still construct a fresh owner.

The fixture asserts all four words at every event and checks that every intervening
non-transition frame preserves them. It requires all five transition events plus
both scene setups and reference-identical ownership at the handoff. This covers
the previously documented cinematic history gap, not a claim of complete native
cinematic or walljump parity. The remaining #473 checks include unreviewed shared
helper callers, legacy-state entry behavior, and the overhang-side follow-up jump.

### Shared previous-pose helper: Yapping Maw

The remaining `Samus_UpdatePreviousPose` caller in native Samus code 03 is reached
by `YappingMaw_Func_10`. Its translated held-placement path selected standing from
spin/walljump but omitted history. A retail-room/population fixture reproduced the
failure with pose 19 before adding the shared history commit after initialization.
`--yapping-maw-history-audit` covers both facings of spin and walljump plus ordinary
falling, with inactive/active grapple (ten cases). It asserts all four history words,
normalization where required, and no repeat shift on the next normalized held frame.
The grapple branch cancels without normalizing or shifting, matching native code.

The focused audit passes. The broader `--yapping-maw-audit` independently fails its
pre-existing frozen auxiliary-palette assertion, observed before the history fix;
do not describe that broader gate as passing. This audit gap requires separate
diagnosis. The other two callers of the shared previous-pose helper are suit pickup
and drained setup, already covered above. Legacy-state entry and the overhang-side
follow-up remain outstanding for #473.

### Resolution of the broader Maw audit failure (#509)

Both failures were stale fixture contracts, not a production palette defect.
The ice projectile was manufactured without equipping Ice; native
`NormalEnemyFrozenAI` immediately thaws when Ice is unequipped. The audit now
equips Ice, checks the timer decrement to 399 and palette-six attributes in emitted
multipart OAM, then unequips Ice and verifies immediate thaw and restoration of
the four link/root palettes. Its later death assertion incorrectly required the
Deleted property bit: native `EnemyDeathAnimation` clears the common enemy record
with memset. The corrected assertion requires a zero definition and properties,
along with the existing health, auxiliary cleanup and kill-count checks.

The full `--yapping-maw-audit` passes, including all six retail populations, ten
history cases, freeze/unfreeze, OAM output and lethal cleanup. This supersedes the
preceding broader-gate failure note. No production code changed for #509.

### Legacy pose-history entry

The comparison audit now also exercises the migration boundary with two otherwise
identical production runtimes. One has recorded history; the other has the omitted
owner left null exactly as a known older graph layout does. Its lazy history is zero
and cannot invent walljump eligibility. From standing, Right then held Jump creates
running/jump transitions: all four words converge at frame 2 and remain equal for
90 frames. Whole/fractional positions and pose agree throughout. The separate
DiagnosticsVerification legacy-options-migration gate checks supported field layouts
and serialization; this movement fixture is not represented as a legacy file replay.

Compatibility limitation: a legacy save captured mid-spin has no recoverable older
movement word. The native walljump gate reads that word, so an immediate attempt can
be suppressed until accepted transitions repopulate history. Deriving a guessed spin
history from the current pose would fabricate state and break parity for other cases.
The existing load warning explicitly reports zero restoration. Current-format saves
retain all four words. The grounded convergence test does not assert exact replay of
an unknown historical mid-air state. Overhang-side follow-up remains outstanding.

### Overhang return capture: new failing reproduction

Mode eight adds the row-six overhang to the established return-to-wall input
sequence (toward wall on frames 12-20, away on 21-22, fresh Jump on 23). The extended
probe captures 14,040 frames. The original eight modes remain exact. Mode eight
exposes 112 position/pose/animation mismatches and 96 speed-word mismatches, with
history still exact. For rightward-away delay five, frame 23 has matching X=0089
and pose 83, but managed Y=007F.1000 versus native Y=0083.0000. Subsequent managed
frames catch the block while native moves away. Both directions and history seeds
show the failure for delays five through eight.

This is a second launch beneath the overhang, not proof of clearing it and then
jumping from the side. Native minimum Y=124 for these delays shows the compact
body approached the underside before expanding. `ApplyWallJumpTrigger` currently
installs the larger radius without the changed-pose collision handling that native
`SamusFunc_F404` calls before its initializer. This is a source-supported diagnostic
lead, not a production fix: the shared expansion resolver, rejected expansions and
fractional-position side effects require examination before implementing the route.

Local capture: `csharp/test-temp/walljump-overhang-return-native-473.csv`, generated
from the unpatched retail ROM by the updated header. Temporary native integration
was removed after capture. The extended comparison deliberately fails; do not mark
#473 awaiting validation. Both the exposed expansion mismatch and the genuinely
cleared-overhang side-jump sequence remain outstanding.

### Overhang second-launch expansion correction

The accepted walljump now runs the shared larger-pose collision resolver before
installing its radius and launch. This reduced the recorded mismatch to 16 launch
frames, all fractional Y only. Native solid vertical block dispatch writes the live
Y fraction even when reached through the changed-pose observer; the copied managed
probe discarded that write. The shared probe now preserves collision-written Y
fraction while retaining the live whole position. Air probes preserve the fraction.

All 14,040 captured frames now match exact position, pose, animation, history and
speed; the charged-spin mode also retains exact charge/contact/projectile state.
The direct `VerifyWallJumpExpansion` test covers both facings with/without the
ceiling: X unchanged, ceiling center Y131/fraction zero, unobstructed Y127/fraction
1000, and the correct walljump pose/radius. The existing full Verification suite
also passes after the shared-probe change. No room-specific offset was introduced.

This resolves the reproduced second-launch-under-overhang defect. It does not
establish the still-outstanding sequence that clears the overhang and jumps from
its side; #473 remains open without awaiting-player-validation.
