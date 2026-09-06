# Blue door investigation (#299)

These source-only diagnostics enter retail room `$01/$1D` through its actual door
header and test the blue cap at BTS `$40`. They do not replay a player save state
and **have not reproduced the reported Super-only opening failure**.

Run the Release DebugRunner with a local ROM:

- `--blue-brinstar-door-audit ROM`: six beam/ammo loadouts, settled closing actor.
- `--blue-door-contact-audit ROM`: Power Beam, Missile, and Super Missile at seven
  contact distances (including flush contact) and three shot heights. All 63 cases
  open the cap. The fixture refreshes collision radii after changing Samus's pose.
- `--blue-door-timing-audit ROM`: the same three weapons after 0 through 32
  closing-actor updates. Of 99 cases, 24 initial shots leave the cap closed. All
  open after a later retry with the **same weapon**, with no persistent failures.

The early window affects all three weapon families, not just ordinary shots.
Its exact timing has not been compared with the cartridge, so this is not proof
that the closing animation is correct or defective. It also does not prove that
the player-facing report is resolved. Keep #299 open without a validation label
until the failure is reproduced and fixed, or the player withdraws/confirms it.

For the missing reproduction, retain the player's failing state before another
save overwrites its slot. Preserve equipped beams, selected HUD item, location,
pose, contact distance, and live door/PLM state rather than substituting a fresh
room that happens to have the same header.
