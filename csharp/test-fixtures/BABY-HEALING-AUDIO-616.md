# Baby Metroid healing audio (#616)

Reported version: 0.3.4.

The pinned cartridge's $A9:C59F healing routine calls $A9:C546 after both
the incremental and full-health writes. That helper requests library-three
$2D through Max3 when post-write energy is at least 81 (signed comparison)
and the enemy-main counter modulo eight is zero. It does not use the NMI
clock or the Baby's individual animation counter.

The port translated the energy write but omitted the helper. Before the fix,
`--baby-healing-audio-audit ROM` failed at health 80 -> 81, clock zero:
expected one tick, received none. The fixture loads the retail room and Baby
through production allocation, then stages the healing phase and invokes its
real main-AI owner. This intentionally isolates healing from preceding attacks.

The owner now records the exact Max3 request after healing, including the call
which transitions to idle. Existing enemy publication preserves Power Bomb
suppression and carries it to the frontend. No serialized fields were added.

128 cases cover pre-heal energy 79/80/81/98, all clock residues twice, and both
Power Bomb suppression states. Assertions check energy, sound identity/count,
queue cap and captured suppression. The existing long MotherBrainAudit healing
path has the same cadence assertion, but the overall audit currently fails
earlier at onion-ring 4 initialization (angle $19 vs expected $1E); it therefore
does not yet supply end-to-end healing evidence. That assertion was not weakened.
