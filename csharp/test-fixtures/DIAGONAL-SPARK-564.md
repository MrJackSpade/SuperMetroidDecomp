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
