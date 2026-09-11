# Super Missile impact shake (#553)

Affected player version: 0.1.1. This change addresses the shake portion only.

## Cartridge evidence and defect

Pinned `upstream-sm/src/sm_93.c`, `KillProjectileInner` ($93:80CF), writes
earthquake type 20 and duration 30 for Super Missile impacts. The extended enemy
collision prelude also writes these shared words before invoking the enemy callback.
The displacement table at $A0:872D specifies one-pixel diagonal displacement of
BG1, BG2 and enemies, with direction selected by timer bit 1.

Managed collisions wrote projectile-local request words, but the renderer's room
shake handler consumed the enemy system's independent words. An actual Super
Missile wall collision therefore produced zero displayed shake frames.

The fix routes each request synchronously to the shared room owner. It does not
copy stale requests at frame end or restart their timers. The routing reference
is nonserialized and rebound before frame execution after state restoration.

## Reproduction and verification

Run DebugRunner `--missile-impact-shake-audit "Super Metroid.smc"`.
The focused flat-floor fixture uses the production weapon, collision, room-shake,
display-capture and software rendering paths, with a constructed solid wall.

- Before the fix: Super Missile collision at frame 11, projectile request 20/30,
  room request 0/0, zero displayed shake frames (assertion failure).
- After: exactly 30 displayed frames, beginning one accepted NMI after collision,
  with exact alternating diagonal displacements on both background registers.
- Rendered background pixels differ from a neutral-scroll control; HUD pixels
  remain identical. This scene does not independently isolate visible BG1 pixels.
- Regular Missile control: actual wall collision, zero shake frames.
- Direct production enemy-prelude check: request overwrites an earlier quake;
  a later room producer still wins and counts down without stale-request restart.
  This is not an enemy geometry or full target-matrix playthrough test.
- Core Verification passes. Ordinary RenderVerification passes 96 ordinary and
  128 window comparisons on both hardware and WARP.

No screenshots, ROM data, or debugger-state files are published.

## Still open in #553

The native impact sound is library 2, command 7 for both regular and Super
Missiles, not a louder Super-only sound. Managed `KillMissile` currently contains
an explicitly missing sound handoff. Its audio producer/publication path and
actual PCM timing still require implementation and verification, including the
native cinematic suppression. Launch sound and distinct explosion animation also
remain within the issue's investigation scope. Do not mark the whole issue ready
for player validation based solely on this shake test.
