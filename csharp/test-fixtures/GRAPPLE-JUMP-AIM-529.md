# Grapple moving-jump aim (#529)

Affected player version: **0.1.1**.

## Reproduction and cause

Headless `--grapple-jump-aim-audit "Super Metroid.smc"` drives the production
runtime using a wide synthetic floor, retail pose/animation/weapon data, and normal
move, aim, jump and fire inputs. Grapple is selected through the normal HUD state,
not the diagnostic selection bypass. Samus and the cannon remain in the viewport.

With R held, move right for ten frames, jump, then fire one or more calls later.
Before the fix, the trace was:

| Call | Pose before -> after | Visible cannon direction | Grapple direction |
| --- | --- | --- | --- |
| Fire | spin $19 -> normal jump $13 | right | inactive |
| Next | $13 -> aimed jump $69 | up-right | right (2) |
| Next | $69 -> $69 | up-right | still right (2), incorrectly |

The mirrored left sequence behaved identically. The assertion on the third call
failed before modifying production code. The temporary disagreement on the
second call is not corrected by reordering pose handling or overriding direction
from held keys: native alpha fires before beta commits the next pose.

Pinned `upstream-disassembly/src/bank_9B.asm` ($B861 and $C490) and
`upstream-sm/src/sm_9b.c` identify the missing firing pre-dispatch check:
`GrappleBeam_PoseChangeAutoFireTimer` ($0CF6). Firing seeds ten; subsequent handler
calls decrement first. A changed valid pose direction restarts firing while the
timer remains nonzero. An expired timer, invalid direction, or banned movement
selects immediate cancellation. The restart emits library-1 stop (7, max6), then
the ordinary firing function emits start (5, max1). It resets length and offsets,
uses the new pose's origin/velocity/angle tables, and does not extend that call.

## Change and verification

- Added the missing firing-phase pose check and persisted timer. Constants live
  in the Grapple catalog; no controller-derived aiming special case was added.
- Moving-jump audit covers both directions, aimed and straight controls, and fire
  delays 0/1/2/4/8. Assertions inspect the actual arm-cannon draw witness
  (`SpriteWritten` and direction), direction selected for Grapple, and signed
  endpoint displacement on the following extension call.
- Focused regression covers changed direction on calls 1 through 10: calls 1..9
  restart, call 10 cancels. Checks timer reset, zeroed offsets, current hand origin,
  actual next-call trajectory, sound request order/limits, serialized timer, and
  loss-aware legacy field migration.
- Collision-only fixtures previously seeded forward-facing pose zero with an
  active Grapple. They now seed a valid ordinary pose and matching direction while
  retaining their exact stationary endpoint, so their real collision assertions
  still execute after the new pose guard.

Legacy snapshots lack firing age; they warn and restore that unavailable timer as
expired. Newly fired beams initialize normally. This work covers the reported
extending-beam direction mismatch, not a full audit of every connected-phase
compatibility branch. Cartridge comparison here is instruction/table inspection,
not a newly recorded emulator playthrough. Leave the issue awaiting player
validation after the regression and build checks pass.
