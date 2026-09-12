# Puyo, Hopper and Boulder audit repair (#583)

All three contact failures were reproduced on clean baseline 5c3ec7a7 before
the #547 quadratic migration and again on the migrated code. Health was already
correct (939, 879 and 959), but the tests incorrectly required immediate knockback.

Native common touch ($A0:A4A1) publishes a five-frame knockback request and
96-frame invincibility; later $90:DDE9 / $91:ED4E admits the movement. The shared
EnemyContactAuditAssertions now checks exact damage (60/120/40), source side,
timer, unchanged pose/position, frozen-time rejection and one later admission
with the correct pose, upward velocity and direction. Existing right-facing
standing-air fixtures satisfy the helper's explicit preconditions.

After contact was repaired, Puyo failed its death assertion with health=0,
properties=0000 and kills=1. Native EnemyDeathAnimation ($A0:A3AF) clears the
slot and owns the explosion separately; it does not retain Deleted on that
record. Beam and Grapple deaths now use EnemyDeathAuditAssertions to check
the exact cleared/respawn header, properties, coordinates, kill count and one
correctly positioned explosion with the original enemy identity.

Reference: pinned upstream-sm 578f90b3cc49557bb70060ad033bb90b8cf8ac50,
sm_a0.c, and the existing #581 native lifecycle evidence. No production changes.

Release DebugRunner build: zero warnings/errors. Complete audits passed:

- Waterway Puyo: three actors, seven hop records, six functions, eight maps,
  245 airborne frames, dropping/landing, OBJ, contact, beam death and Grapple kill.
- Blue Hopper: retail Tourian cycles and sounds, exact contact handoff, plus
  floor/ceiling Dessgeega art and Red Brinstar ceiling movement checks.
- Boulder: all 13 records, eight maps, proximity, fall/rebound/roll, collision
  dust/sounds, zero-height shortcut, contact, vulnerabilities and Grapple cancel.

This resolves developer diagnostic defects, not unconfirmed player reports.
