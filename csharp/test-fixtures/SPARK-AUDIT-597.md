# Spark contact diagnostic repair (#597)

The full Spark audit failed before this diagnostic change: health 999 -> 969,
knockback inactive. It required immediate knockback after body-contact resolution.
Native normal touch publishes the five-frame pending request; movement admission
occurs later through $90:DDE9/$91:ED4E. The actor's normal-touch damage is 30.

Replaced the incorrect assertion with EnemyContactAuditAssertions for the actual
right-facing standing, equal-X fixture. It checks damage, 96-frame invincibility,
pending timer/side, unchanged position/pose, frozen-time rejection and exactly
one later knockback admission. No production contact behavior was changed.

The complete --wrecked-ship-spark-audit now passes: seven-actor retail population,
always-active variant, nine body maps, four falling-projectile maps, four trail
maps, floor rebound/deletion, projectile/body damage, beam pass-through,
indestructibility, grapple hurt, random timers, native table overread and the
literal WRAM OR bug. This developer-only diagnostic issue is verified, not
awaiting a player reproduction.
