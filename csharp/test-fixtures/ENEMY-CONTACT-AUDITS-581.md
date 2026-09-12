# Enemy encounter audit repairs (#581)

All six audits failed at contact checks on clean baseline 1647ba14 and again on
e15fa3c5 before these diagnostic changes. No production code changes were needed.

## Native boundaries

Pinned `upstream-sm/src/sm_a0.c` and the corresponding disassembly establish:

- $A0:A4A1 body contact and $A0:9923 projectile contact publish damage,
  invincibility 96, knockback timer 5 and source side. They do not install a hurt
  pose or move Samus. $90:DDE9/$91:ED4E admit that request later.
- $A0:A3AF creates a separate death projectile, then clears the enemy record.
  Respawning actors get the $DAFF placeholder; a Deleted property is not retained.
  The Power Bomb outer dispatcher subsequently adds ProcessOffScreen.
- $A0:957E requires Ice to remain equipped. Removing Ice takes the thaw branch;
  an injected Ice projectile does not equip Samus automatically.
- Single-box shot dispatch marks the projectile collision before invoking the
  enemy callback. Kzan's $A6:804C RTS does not undo that mark.
- A zero Power Bomb callback selects normal Power Bomb AI, not immunity.
  GRipper's $B4:EF4A vulnerability record has multiplier two for Power Bombs,
  so the default handler deals 200 and kills its 200-health actor. Kzan's
  corresponding $B4:EEC6 byte is zero, preserving its immunity.

## Changes and verification by encounter

| Audit | Incorrect assumption / fixture | Corrected exact checks |
| --- | --- | --- |
| Stoke | Contact immediately activates knockback; death leaves Deleted | Five-damage projectile and 40-damage body publication/admission; cleared enemy and one correctly identified/positioned death effect |
| Cacatac | Same immediate knockback and Deleted assumptions | Five-damage spike and 20-damage body handoff; ordinary beam and later Grapple death effects/kill counts |
| Owtch | Same contact assumption; plasma and diagnostic normal-bomb deaths require Deleted | 100-damage handoff; exact death records/effects; retain moving-right immunity and moving-left callback gate |
| Ripper | Same contact/death assumptions; injected Ice shot without equipped Ice | Five-damage handoff; native beam/missile/Super/Power Bomb outcomes; all 400 frozen calls stationary, followed by stationary thaw call with handler cleared |
| Kzan | Contact immediately activates knockback; no-op shot AI implies no collision | 200-damage handoff; one accepted shot collision with direction $12, unchanged health/list/activity; zero-damage Power Bomb remains immune |
| GRipper/Ripper II | Same contact/death assumptions; GRipper assumed Power Bomb-immune | Ten-damage handoff; default callback/multiplier-two check and exact Power Bomb death effect; Ripper II Super death; drawing uses an actor belonging to the reloaded fixture |

`EnemyContactAuditAssertions` captures pre-hit health, pose and whole/fractional
position. It verifies exact pending state, frozen-time rejection, one later
standing-air admission, no extra damage or position movement, hurt flash 1,
vertical velocity 5.0000 and rejection of a second admission. Its fixture domain
is explicitly right-facing standing Samus, not an assertion that every movement
type accepts knockback.

`EnemyDeathAuditAssertions` captures pre-death identity, position, physical slot,
respawn property and kill count. It checks the resulting cleared/placeholder
record and one matching death effect, including respawn-qualified slot, graphics
index and initial instruction timer. Existing nonlethal damage and collision
assertions are retained; no health expectations are weakened to pass these tests.

All six complete Release DebugRunner audits pass, including their movement,
animation, terrain, combat and rendering branches beyond the repaired assertions.
DebugRunner and Windows Release builds pass with zero warnings/errors. The full
Verification suite was not rerun for these diagnostic-only changes; it passed on
the unchanged production code in e15fa3c5.

No private captures or ROM data files are committed. Closing #581 does not claim
to resolve any unrelated player-reported movement, damage or rendering issue.
