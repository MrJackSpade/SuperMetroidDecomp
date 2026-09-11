# Rio diagnostic repair (#577)

The untouched-room audit failed on clean 71f1c01f and after the signed-math
migration with health 984, knockback inactive, Rio health 45. Its immediate
knockback assertion contradicted the split between $A0:A4A1 contact publication
and $90:DDE9 hit admission. This is an audit repair, not a gameplay change.

The corrected check asserts 15 damage, invincibility 96, knockback timer five,
source-side direction one, unchanged pose/position and inactive movement at
contact. Frozen time must retain the pending request. Unfrozen admission must
install exactly one right-facing/up-right hurt pose, flash counter one and
vertical speed 5.0000, without dealing damage twice or moving Samus immediately.

Passing that check exposed two more obsolete death assertions. $A0:A3AF spawns
an explosion, clears the common enemy slot and increments the room kill count;
it does not retain the Deleted bit. $A0:A306 additionally sets ProcessOffScreen
after the Power Bomb callback returns, even when that callback cleared the slot.
The shared beam/Power Bomb death assertion now checks those exact slot results
and a single effect retaining Rio's header, native slot index, pre-death position,
zero graphics selector and instruction timer one.

Verified with the complete `--rio-audit` against the pinned ROM: load, animation,
both dive directions, collision, perch return, contact/admission, beam death and
Power Bomb death all pass. Release DebugRunner builds cleanly. No production
code, private captures, ROM data or save fields changed.

References: upstream-sm sm_a0.c ($A0:A306, $A0:A3AF, $A0:A4A1), sm_90.c
($90:DDE9), pinned revision 578f90b3cc49557bb70060ad033bb90b8cf8ac50.
