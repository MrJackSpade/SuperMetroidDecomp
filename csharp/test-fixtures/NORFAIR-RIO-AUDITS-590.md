# Norfair Rio diagnostic repairs (#590)

Both complete encounter audits reproduced stale fixtures/assertions. No production
gameplay was changed.

- Freeze mirroring requires equipped Ice. Native `NormalEnemyFrozenAI` at
  $A0:957E clears freezing without it; both fixtures now equip Ice and verify
  parent/follower timers decrement from 10 to 9 with the follower hidden.
- Contact publishes pending knockback, not immediate movement. The shared helper
  verifies 60/120 damage, exact pending timer/side, unchanged position and pose,
  frozen-time rejection, and exactly one later knockback admission.
- Common beam death clears the parent record rather than setting Deleted. The
  shared death helper verifies the cleared record, kill count and separately
  owned explosion at the original coordinates. The existing subsequent-frame
  follower deletion check remains intact.
- Power Bomb callback zero means the common reaction, not immunity. Native
  $A0:A306 selects that fallback; $A0:A5C1 applies 100 times vulnerability.
  Geruta/Holtz tables at $B4:EE42/$B4:EE58 both contain 2 at byte 15. The tests
  isolate the parent from other slots and assert lethal 200 damage for Geruta,
  or 900 to 700 health, 48 invincibility and 12 flash frames for Holtz. They also
  check the outer loop's ProcessOffScreen property, including after death clear.

References: pinned upstream-sm sm_a0.c and upstream-disassembly banks A0/B4;
the project's Japan/USA retail ROM. No ROM or capture is included here.

Verification: Release DebugRunner builds with zero warnings/errors. Both
`--norfair-rio-audit` and `--lower-norfair-rio-audit` pass completely, including
retail population loading, attack phases, terrain movement, paired animation and
visibility, mirrored launches, freeze, contact, beam death, follower cleanup and
Power Bomb reactions. This repairs developer diagnostics, not player-confirmed
visual parity or the broader unfinished #547 migration.
