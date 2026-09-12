# Exhaustive combat audit repair (#586)

Both failures reproduce on clean pre-shot-classifier ac700f53: projectile audit
28 failing room states, normal-bomb audit 44. The temporary baseline worktree
and generated binaries were removed after comparison. No production edits.

## Causes and checks

- Freeze lifecycle: the old deadline was frozen timer + two frames, ignoring
  the native no-op hurt handler's precedence. Flash 12 takes five calls to fall
  below eight; only then may frozen AI decrement its timer. The audit now derives
  the hold from the installed state and asserts every held flash/frozen value,
  exact hurt-bit retirement, each ordinary frozen countdown value, and final
  zero timer plus cleared frozen handler. Rinka's private frozen callback has
  the same preceding no-op hurt hold; it keeps its private tail behavior.
- Normal-bomb death: expected Deleted conflicted with native common death's
  cleared record. Lethal callbacks now assert the exact cleared/respawn header,
  properties, position, kill count and separately owned explosion identity/XY.
  Surviving callbacks still reject unexpected deletion and verify exact HP.
- Inventory: names.txt includes unused debug room $8F:E82C/$E839, explicitly
  identified in pinned bank_8F.asm as area seven. A dedicated diagnostic catalog
  identifies only that record, asserts its single expected state, and logs the
  exclusion. Invalid arbitrary rooms are not silently skipped. All execution
  audits share this corrected playable-retail inventory.

Native references: pinned sm_a0.c common flash housekeeping $A0:9128,
NormalEnemyFrozenAI and EnemyDeathAnimation $A0:A3AF; pinned bank_8F.asm debug
room definition. Existing lifecycle assertions were independently checked in
#581/#583/#584/#585. This changes diagnostics, not cartridge behavior.

## Verification

Release DebugRunner: zero warnings/errors. Both complete audits pass:

- Projectiles: 657 variants, 2,604 live weapon callbacks, 225,818 lifecycle frames,
  111 definitions; six variants correctly rejected by canonical multibox gates.
  44 definitions remain naturally deleted/empty/intangible after the existing
  2,048-frame fresh-load activation probes; they are not claimed as combat coverage.
- Normal bombs: 596 supported variants, 398 physical callbacks, 92 definitions
  (42 common, 44 private, six literal RTL), 29 focused private callback identities,
  zero private-callback residuals. 32 naturally unavailable definitions are reported
  separately rather than counted as successful collisions.

This closes a developer diagnostic issue, not an unconfirmed player report.
