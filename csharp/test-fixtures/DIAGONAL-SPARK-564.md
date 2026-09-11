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
