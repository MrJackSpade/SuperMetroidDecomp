# Alcoon diagnostic repair (#589)

The complete retail Alcoon audit reproduced two obsolete assertions. Production
code was not changed.

- Body contact removed the correct 50 energy but did not immediately activate
  knockback. The shared domain helper now verifies exact pending-hit publication,
  unchanged pose/position, source side, frozen-time rejection and single later
  player-scheduler admission.
- A lethal Super Missile cleared the actor's properties, so requiring the
  `Deleted` bit failed. The native common-death helper now checks the complete
  cleared record, one kill increment and exactly one correctly positioned,
  separately owned death explosion retaining the original actor identity.

Verification: DebugRunner Release build passes with zero warnings/errors.
`--crateria-power-bombs-alcoon-audit "Super Metroid.smc"` passes completely:
three retail actors; floor finding; seven AI states; thirteen actor maps; four
fireball maps; all three launch velocities in both directions; definition
loading; drag; beam pass-through; wall deletion; real runtime scheduling;
20-damage projectile contact; 50-damage body contact; nonlethal/lethal shots;
and OBJ rendering.

This is diagnostic repair, not a new gameplay behavior change. Shared helpers
retain the native contact/death boundaries documented by earlier audit repairs.
