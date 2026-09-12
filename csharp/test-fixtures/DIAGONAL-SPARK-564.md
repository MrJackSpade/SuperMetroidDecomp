# #564 diagonal Shinespark input investigation

Affected player version: 0.1.1. Still open, not awaiting validation.

## First complete gameplay-input control

Run DebugRunner `--diagonal-spark-input-audit ROM` against the pinned NTSC ROM.
The fixture uses the shared constructed Landing Site runway: floor row 16,
cleared rows above, dry medium, no gameplay cheats, Speed Booster equipped,
health/max 999, starting center (128,235) right or (1200,235) left. This is a
runtime-input diagnostic, NOT a physical controller or desktop-window test.

No charge, stored-shine, windup or directional pose is injected. Hold forward
and Dash until the real speed counter reaches the charge threshold (90 calls),
press Down for one call, release for 15 calls, then hold Jump + Aim Up. Both
facings enter diagonal travel on call109 and finish on195. Assertions check
storage, diagonal phase, upward/forward displacement, and eventual completion.
The fixture does not yet compare every movement frame against original CPU.

Pinned bank91 transition lists AD6C/AD80 give Up+Jump priority over AimUp+Jump,
then forward+Jump. The paired controls add Up+forward at windup and correctly
launch vertically, even while Aim Up remains held. Adding those directions
before windup instead changes jump admission, so it is not the same control.

An initial test released Down for only one call before pressing Jump+AimUp.
That sequence stayed in stored-shine crouching poses rather than launching.
Waiting for standing produced the successful control above. This is observed
port behavior, not a reproduced defect: cartridge timing has not yet been
compared for that acquisition-to-crouch-to-jump sequence.

## Remaining acceptance work

Capture original-CPU versions of the same acquisition/storage and input timing
cases, including neighboring timing failures and horizontal controls. Record
per-frame movement through termination. Exercise keyboard and physical pad
polling through the desktop frontend, identifying actual device, bindings and
D-pad/stick route. Existing #465 seeds stored charge and therefore cannot
substitute for those tests. No claim that the player's failure was execution
error, no gameplay fix, and no validation label at this checkpoint.

## Original-CPU acquisition and crouch-release timing

The bounded native probe now runs 32 cases: both facings, 0..15 neutral calls
between Down and held Jump+AimUp. It runs 90 forward+Dash calls and stores on
call91 without injected charge. Every pose, boost counter, shine/windup timer
and 16.16 X/Y matches: 3,578 records total. Waits 0..2 remain crouching through
the 150-call observation; waits 3..15 launch diagonally on zero-based frame
93+wait. Thus the initial one-neutral-call failure also occurs on the cartridge.

The first comparator omitted runtime's audio callback and diverged in the low
boost-counter byte at frame89 (native0401, managed0402). Native's echo queue call
clobbers the accumulator used to index its timer table. Connecting the real
CartridgeAudioState.QueueSoundAndGetAccumulator callback removes every mismatch.
This corrects the diagnostic, not production gameplay, which already supplies
its queue callback through the frontend. The isolated comparison begins with an
empty queue and does not claim complete frontend audio scheduling.

Repeat native build with `movement-release/native-diagonal-input-entrypoint.patch`,
then `sm.exe --diagnostic-diagonal-input ROM NEW.csv`; compare via DebugRunner
`--diagonal-spark-native ROM NEW.csv`. Private trace SHA256:
`782C3EAD92A3FD7D20F526FF7BD55A5FE31A522B2466C05135212727711A34CF`.
Native probe geometry is 96x32 blocks with the same runway and no enemies;
the managed fixture uses cleared Landing Site collision geometry. This capture
ends at directional launch (or the failed-input limit), not crash completion.
Desktop physical input, direction-priority native controls and post-launch
per-frame comparisons remain outstanding. No validation label yet.

## Post-launch travel and crash-exit correction

The complete native mode adds a solid ceiling at row zero to both fixtures;
the old acquisition-only probe had no ceiling and was unsuitable for comparing
termination. Its first extended, ceiling-free run is discarded for that purpose.
`DiagnosticDiagonalInputComplete(rom, output)` now captures all 32 cases through
termination (or 260 frames for non-launches), totaling 6,448 records.

This reproduced a defect: at right-facing settle=3/frame181, native standing Y
was 33 while C# was 35. All earlier travel/crash records matched. Native
`$90:D40D` publishes a transitional standing pose, whose `$91:F34E` handler calls
`$90:EC7E Samus_AlignBottomWithPrevPose`. The port skipped that bottom alignment.
Crash exit now applies old-radius minus new-radius to current and previous whole
Y, preserving both fractions. The checkpoint delta is consumed once. Departure
echo tests now correctly use the aligned current body, matching `$90:D4D2`.

All 6,448 records match after the fix, including both directions. Default
regressions exercise the real crash-finishing method and independently verify
the two-pixel center correction, fractional preservation and camera delta.

`movement-release/diagonal-complete-564.zip` contains the numeric CSV only.
Extract locally, then run DebugRunner `--diagonal-spark-native-complete ROM CSV`.
The comparator pins its SHA-256:
`CDA780C3FB58C08A0DF557AB3338608F19E0B5D8AD1A40B5C674409C6B370056`.
To recapture, use the existing headless entrypoint patch pattern but dispatch
`DiagnosticDiagonalInputComplete`; remove temporary hooks afterward.

This scoped gameplay fix does not diagnose the reported physical-controller
launch difficulty. Keyboard/pad path evidence and native direction-priority
controls remain outstanding; #564 stays open and is not awaiting validation yet.

## Desktop shoulder-help correction

The visible footer incorrectly described Q/L as aim-up and W/R as aim-down.
The actual mapper sends Q to SNES L and W to SNES R; default cartridge bindings
use L for aim-down and R for aim-up. Corrected the footer, without changing any
controller mapping. Under defaults the demonstrated diagonal-launch chord is
Space (or X) + W, not Q. Custom cartridge bindings can change those actions.

`DesktopVerification --keyboard-input-audit` now checks the exact footer
definition and synthetic Windows messages through `HostKeyboardInputState`:
Space+W becomes A|R and Space+Q becomes A|L. The audit passes, alongside its
existing release/focus-discontinuity checks. This is not a physical keyboard
test, nor proof that the wrong help caused the player's gamepad failure.
Hardware polling and native direction-priority controls remain open.

## Live zero-axis driver defect

`DesktopVerification --live-gamepad-probe 2` uses the actual production WinMM
adapter, without injecting input or opening a player window. Current observation:
Microsoft PC-joystick driver, joystick0, 32 buttons, **0 axes**, positional layout,
raw X/Y=0/0, advertised ranges0..65535, no POV, no pressed buttons. Before the
fix this became normalized -32768/-32768 and SNES `$0A00` (Up+Left).

The adapter normalized absent axes as though their zero-filled values were real
deflections. It now checks AxisCount before normalizing each axis. The faithful
zero-axis regression failed before the fix and passes after; real two-axis
zero values still map to Up+Left. The keyboard audit includes these controls.
A second live two-second probe returns normalized0/0 and SNES0000 for the same
device. No physical button press, D-pad or stick actuation was performed.

This phantom Up could override aim-based diagonal selection, but it is not
proof of the historical player's cause: their earlier driver reported ten
buttons/two axes, not this current device. Preserve that distinction. #564's
physical acquisition-to-completion and native priority controls remain open.

## Native direction-priority controls completed

Added two original-CPU matrices, retaining charge acquisition/storage and all
0..15 release waits in both facings. Only windup input changes: mode0 holds
Jump+AimUp+Up+forward and selects vertical; mode1 holds Jump+forward without AimUp
and selects horizontal. All 3,578 records per mode match the production runtime,
including short-release failures (7,156 records total). Successful groups also
assert the selected movement phase explicitly. No gameplay change needed.

`movement-release/spark-direction-564.zip` holds both numeric CSVs. Compare with
`DebugRunner --spark-direction-native ROM CSV MODE` (0 vertical,1 horizontal).
Fingerprints are pinned by the comparator:

- Vertical: `891F74968F7697787B286CD07D0145E4AC03CDAC26EFAA449351945D865CBDA6`
- Horizontal: `108E4C1DB58315D1EA70D969B81CAC8A552CA24BB845582371087315B8CF3A9C`

Native recapture uses the prior headless entrypoint pattern, dispatching
`DiagnosticSparkDirection(rom, output, mode)`. Temporary hooks were removed.
These controls end at launch; full post-launch diagonal travel is independently
covered above. Physical user-facing acquisition-to-completion remains unproven;
an idle hardware poll or synthetic mapped messages do not satisfy that requirement.

## Production 0.2.1 recording and explicit input controls

The player supplied `issue-564-release-diagonal-shinespark/` on September 12.
Its 5,587-frame recording reproduces six launches in room `$8F:91F8`, without
changing any input: horizontal-left at 1694, vertical-left at 2162, 3017, 3722,
and 4081, then vertical-right at 4645. Live replay bindings are Jump `$0080`
and AimUp `$0010` (default A/R). The first launch selects horizontal from
`$0280`; Up arrives two frames later. Subsequent launches select vertical from
`$0A80` (Up+Left+Jump). No aim-up shoulder input occurs in these attempts.

`DebugRunner --spark-player-recording-audit RECORDING ROM` pins the recording
hash `A33F0651FE74F5519EE4507674BF75D5FA3975E4B7AAD4DD5DA1F0650AB685D9`
and asserts all six original launch frames/directions. Six independent controls
then replay the exact session up to a selected windup, substituting only
Jump+AimUp (`$0090`) from frames 1688, 2156, 3013, 3722, 4081, or 4645.
After diagonal admission they release input. Each launches diagonally, moves
upward and horizontally in its facing direction, and completes; completion
frames are 1830, 2289, 3143, 3798, 4220, and 4721 respectively. Earlier attempts
in each control retain their original inputs; no position, charge, equipment,
or actor state is injected. These controls all pass.

The input substitutions are **not physical-controller evidence**. This real-room
recording also does not replace the original-CPU comparisons above: its frontend
replay uses audio-port acknowledgements, not an SPC waveform comparison. The
observed launch selection agrees with those native direction-priority controls.
No new production fix is justified by this recording. The player has been given
the native chord to try: store charge, release Down and stand, face the desired
direction, then hold aim-up plus Jump with the D-pad neutral. Physical success
with that chord remains for player confirmation; controller model and D-pad
versus stick were not supplied by this recording format.
