# Golden Torizo combat audit (#617)

Affected player version: 0.3.4. Report: sustained Super Missile fire felt too
effective, with little damage taken. This does not establish a specific defect.

## Reproduced divergence: subsequent hit after caught Super

Pinned $AA:D667 first rejects flash, then sends animation-locked shots directly
to common damage. Otherwise $D67C tests behavioral bit $1000. If set, $D682
branches to $D69D and ORs $2000 before damage; it does not update the captured
projectile family. $2000 is later consumed by the health-gated stun instruction.

The port's normal C97C -> D667 path skipped that OR. The retail hitbox fixture
reproduced one Super hit at health 13500 with no flash/animation lock and $1000
set: health correctly became 12900 but flags remained $1000 instead of $3000.
This was documented in the issue before changing production code.

The correction adds the missing write only for the normal callback, without
changing damage, catch selection, flash rejection, or animation-lock routing.
Eight cases cover normal and stand-up/sit-down hitboxes, zero/nonzero animation
guard, and zero/nonzero flash. Each asserts hit admission, health-change behavior,
the counterattack bit, and preservation of the captured-family word.

Command: DebugRunner --golden-torizo-audit ROM. Before correction the new first
case fails; after correction all eight and the existing encounter audit pass.

## Coverage limits / remaining audit

### Super capture facing/leg matrix

Eight additional cases check graphical bits 0000/2000/8000/A000 with Samus
in front and behind. Independent $AA:D71D-D741 expectations select CDE1,
CE43, CEA5, CEFF respectively. Front hits retain health and set the catch
latch/list/timer; rear hits apply damage without capture. All cases passed
before cleanup. Two old catalog names incorrectly described the facing
direction; the names now spell out both facing and leading leg, with no
address or behavior change. This confirms selection, not full animation timing.

The existing audit exercises retail loading, animation/movement, five projectile
families, shot reactions, contact damage, death and boss completion. Some branches
are entered explicitly; successful execution does not establish native cadence.
The missing bit alone is not proof of the cause of the difficulty report.

Still required for #617: independent native comparisons of state selection,
attack cadence and movement under sustained firing, full shot/contact damage and
invulnerability timing, projectile trajectories, and any relevant conditional
AI inputs. Do not label the entire audit awaiting validation based on this fix.
