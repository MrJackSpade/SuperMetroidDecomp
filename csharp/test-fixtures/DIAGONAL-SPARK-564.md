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
