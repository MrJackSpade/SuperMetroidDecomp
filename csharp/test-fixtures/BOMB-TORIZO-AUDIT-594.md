# Bomb Torizo diagnostic contact timing (#594)

Reproduced the complete audit failure: boundary probes passed, then body contact
changed health from 989 to 981 while KnockbackActive remained false. The audit
incorrectly required immediate activation. Native $AA:C977 delegates to $A0:A4A1,
which publishes the timer and source side; $90:DDE9/$91:ED4E admit movement later.

Replaced the stale check with EnemyContactAuditAssertions.VerifyStandingAirHit:
exact eight damage, timer five, invincibility 96, pending side one at equal X,
unchanged position/pose, frozen-time rejection and exactly one later knockback
admission. No production behavior changed for this diagnostic fix.

The complete --bomb-torizo-audit now passes, including 2,736 boundary probes,
awakening, attacks, contact/projectile damage, low-health breakup, death lifecycle,
item drop, music and boss-bit publication. The separately staged #547 migration
changes only initializer table storage, not this contact path.
