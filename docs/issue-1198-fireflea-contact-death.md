# #1198: Fireflea ordinary-contact death effects

Affected version: v0.4.8 (original testing workbook). The player clarified the
reported enemy is the floating bug that lights the room: Fireflea ($D6BF), not
Memu ($D17F). Ordinary contact is the trigger; Speed Booster was never involved.
Earlier Memu/Speed Booster fixtures therefore did not cover this report.

## Cartridge behavior and cause

Pinned NTSC J/U 1.0 disassembly revision
362be646929cf8e483f692b73a6561cfc2dc1d0d, bank_A3.asm $8E6B:
Fireflea calls common normal touch and then EnemyDeath unconditionally. With
ordinary contact, common touch damages Samus and leaves A equal to enemy health
($A0:A480). Fireflea has 20 HP; EnemyDeath clamps animation values above four to
zero, selecting the small explosion ($86:ED69). Its six sprite frames include
the authored library-2 $09 sound request before pickup conversion.

The port implemented the actor removal, kill count, and darkness increment but
omitted the shared death call. This removed both graphics and sound. The fix
calls StartGenericEnemyDeath with the surviving health before clearing the slot.
The pre-existing attacking-contact double-kill accounting remains unchanged.
There is no new Fireflea-specific explosion or sound implementation.

## Reproduction and confirmation

Guarded DebugRunner command:

    --fireflea-contact-death-audit <installed-content-directory>

The fixture loads retail Green Brinstar Fireflea room $9C5E, puts Samus next to
one existing Fireflea within the camera window, and excludes other enemies from
Samus contact. It steps the real frontend with Right, Gravity Suit and no attack
items. Contact index is asserted zero throughout. This is a controlled retail
fixture, not a recording of the player's unidentified room.

Before the fix: ordinary contact reduces Samus health 999 -> 998, marks the
Fireflea deleted, and clears it the next frame. No explosion is allocated and
no death sound is emitted. The reported animation/sound assertion fails.

After the fix: the enemy is cleared at frame zero, all six small-explosion
operands produce OAM sprites at the original contact coordinates, and library-2
$09 reaches the real audio renderer at frame 16. The composed frame-four capture
was visually inspected for the explosion. Kill count remains one, darkness two,
and health 998. The explosion completes and becomes a pickup.

Release DebugRunner build and this focused regression pass. The legacy
--green-brinstar-fireflea-audit could not reach its assertions because its room
loader predates required installed artwork bindings; its obsolete normal-touch
assertion was updated to expect the native cleared slot. The new frontend
regression provides installed-artwork confirmation of this reported path.

Awaiting player confirmation; #1198 remains open.
