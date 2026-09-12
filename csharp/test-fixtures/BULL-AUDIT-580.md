# Bull contact and death diagnostic repair (#580)

Developer diagnostic only; no production behavior changed.

## Reproduction

`DebugRunner --sponge-bath-bull-audit "Super Metroid.smc"` failed on clean
baseline 58b22b6f and the family-table migration, then again before this repair:
health 999 -> 989, knockback inactive, invincibility 96. Correcting that assertion
exposed a second obsolete expectation: a killed Bull should have a Deleted flag,
although common cartridge death clears its common enemy record.

## Cartridge evidence

- $A0:A4A1 publishes touch damage, invincibility 96, knockback timer 5 and source
  side, then returns. The later $90:DDE9 interruption handler admits knockback.
- $A8:DB14 invokes common shot AI. Its special shot reaction runs only if health
  is unchanged; it does not replace the ordinary lethal-damage cleanup.
- $A0:A3AF creates the death effect using pre-clear identity/position, clears the
  64-byte enemy record and increments the kill counter.
- $A0:A306 adds ProcessOffScreen after the Power Bomb callback, even after death.
- $A0:9FC4 uses common death for the grapple-kill reaction.

These were cross-checked in the pinned upstream disassembly and local C source.

## Corrected checks and result

The contact assertion now verifies exact damage/timers/direction with unchanged
pose and position and inactive movement. Frozen admission must fail; unfrozen
admission must install exactly one up-right knockback with the expected pose,
vertical speed, direction and hurt-flash state, without moving or damaging again.

Missile, Power Bomb and grapple death checks require the cleared header/health/
position, exact surviving properties, one kill, and one correctly positioned
death effect retaining Bull's header/native index, graphics and initial timer.
No endpoint or lifecycle assertion was replaced with a no-crash check.

The complete Release Bull audit passes: retail Sponge Bath load, four movement
functions, three animation maps, four rendered OBJ pieces, contact, damaging
missiles, all ten immune-shot angles, 48-frame guard, Power Bomb and grapple kills.
The Release diagnostic project builds cleanly. No runtime change required a new
full gameplay-suite run for this audit-only repair.
