# Evir arms crash on the body's death frame

Affected version: 0.4.5+ad1701f710a60415b772b68b8f49b2793283178a,
as recorded in the supplied session.log. The diagnostic ZIP contains only that
log, not the referenced input recordings or a saved state.

The fatal exception is `Evir arms body expected definition $E63F at relative
slot -1 from 7`, from PositionEvirArms during the main enemy pass.

## Reproduction and cartridge behavior

The focused `--evir-death-frame` fixture initializes an Evir in physical slots
6/7/8, publishes its ordinary spritemap, and runs production StepFrame with
lethal Screw Attack contact. The body is removed after the scheduler has already
selected all three records. Its contact tail marks the arms and projectile
deleted. Their calls remain in this frame's active list, so the arms run next.
Before the fix this produces the exact reported exception, including slot 7.
The fixture reproduces the crash mechanism; the player's exact attack sequence
was not included in the bundle.

Pinned InsaneFirebat disassembly revision
362be646929cf8e483f692b73a6561cfc2dc1d0d:

- $A0:8FD4 main enemy loop consumes the already-built active-index list.
- $A0:A3AF generic death clears the body's common record.
- $A8:8B16 marks the next two physical records deleted after common damage.
- $A8:8866 arms AI reads the preceding record's common facing, X and Y words
  directly, without checking its definition pointer.
- The projectile likewise retains its physical body alias. Next frame's
  DetermineWhichEnemiesToProcess clears deleted child definitions.

The port's live-header assertion invalidated that legitimate last-frame alias.

## Change and confirmation

Keep strict definition checks during initialization. Once an Evir slot has been
initialized, retain the native physical alias even after the referenced common
record is cleared. Typed facing is backed by the common slot, so the final call
reads zeroed facing/coordinates rather than stale pre-death values. No actor is
silently skipped, respawned or hidden to suppress the exception.

The regression checks the real contact kill, one death count and explosion,
child deletion flags, the arms' final AI call, exact cleared-slot arm position
(-4,10), idle projectile position (-4,18), and removal of both children next
frame. Reinitialization with a missing body still throws inside the harness.
The corrected reproduction passes. Static PLM, queued-VRAM-DMA and enemy-visual
gates pass (zero errors; existing argument-validation analyzer warnings remain).
Player validation remains pending; no exploratory
playthrough or additional defect search was performed.
