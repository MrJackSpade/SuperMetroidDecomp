# Ridley stays low after a missed grab

Affected version: v0.4.4+21b505230fff6a3702ea6dc73c05b84405a66672,
carried forward from the player's testing session. Follow-up to #1172:
flight/tail improved, but Ridley sometimes remained low trying to grab Samus.

## Reproduction and cause

The focused production-dispatcher fixture starts a left-facing lunge with
Ridley at (160,400) and standing Samus at (100,420). Before the fix it fails:
expected NorfairHoverSetup, got NorfairGrabApproach. The claw has passed Samus's
height, but the port continued accelerating toward her instead of retreating.
This is a faithful small fixture for the reported behavior; no player recording
was supplied and this is not a claim to reproduce their exact controller sequence.

The cartridge's $A6:BAB7 lunge exits when the previous movement hit a room
boundary, Ridley passes Samus horizontally by at least 32 pixels in his facing
direction, or body Y + 35 reaches Samus Y. The translated approach omitted all
three checks. Movement also discarded the boundary-hit result needed next frame.

## Native implementation

Restored those checks using native signed word comparisons and retained the
nonzero boundary-hit predicate across the existing movement-to-AI handoff.
Movement resets the predicate each call; velocity and position clamping are
unchanged. Native boundary values are only tested for nonzero here, so the host
models the predicate rather than unused individual boundary codes.

$A6:BA85 handles a miss in this order: after ten zero-health lunges start the
death roar immediately; otherwise dodge an armed power bomb; otherwise select
$A6:B3EC recovery and request a tail whip. The existing recovery rises toward
Y=256. Added the missing $A6:BD4E dispatcher implementation needed by the bomb
branch, including its native opposite-side target and return to attack selection
or carrying when the bomb flag clears. Thresholds live in RidleyLungeDefinitions.
This restores authored transitions, with no added timeout or invented movement.

Source: pinned InsaneFirebat disassembly revision
362be646929cf8e483f692b73a6561cfc2dc1d0d, bank A6
$BA85-$BB1D, $B3EC-$B42D, $BD4E-$BD99 and $D86B-$D913.
The project targets J/U NTSC 1.0 (ROM SHA256
12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72).

## Confirmation

`--ridley-missed-lunge` now passes the previously failing assertion and verifies
actual upward movement over 32 production dispatcher/integration calls, no false
grab, exact height and horizontal pass thresholds for both facings, boundary
publication/consumption/reset, ninth/tenth zero-health misses, and immediate
power-bomb dodge/recovery. The fixture rejects gameplay cartridge reads.

Verification builds; the PLM, queued-VRAM-DMA and enemy-visual static build gates
pass. The audit build reports existing SME6201 argument-validation warnings
(1,383 warnings, zero errors); no warnings originate in the new lunge code.
These resource gates cannot establish semantic boss-AI parity; the focused
regression verifies this specific restored contract. Player confirmation remains
pending. No whole-fight replay or exploratory bug search was performed.
