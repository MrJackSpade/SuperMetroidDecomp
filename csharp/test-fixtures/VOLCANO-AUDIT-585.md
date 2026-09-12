# Volcano audit contact repair (#585)

On 74398688 the complete audit stopped at Fune fireball contact with health=939,
knockback=False, live=False. Damage and projectile deletion were correct; the
assertion incorrectly demanded movement admission in the projectile producer.
Polyp rock and Fune body checks contained the same assumption.

Pinned sm_a0.c HandleEprojCollWithSamus ($A0:9923) and common body touch
($A0:A4A1) publish invincibility=96 and knockback timer=5. Movement admission
is later. The shared contact helper now verifies exact damage (60/16/10),
source side, unchanged position/pose, pending state, frozen-time rejection,
then one admitted standing-air knockback with correct velocity and direction.
Projectile deletion remains independently asserted.

No production edits. Release DebugRunner builds with zero warnings/errors.
The entire --volcano-enemy-audit passes: six Funes, four Polyps, mouth bytecode,
all five actor maps, spit sound, fireball flight, three-sample RNG rock launch,
rise/apex/fall, cooldown underflow, projectile OBJ, both attack damage values,
body contact, Ice freezing and indestructible vent behavior. This repairs a
developer diagnostic rather than closing an unconfirmed player report.
