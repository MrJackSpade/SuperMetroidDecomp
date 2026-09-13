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

## Follow-up: broad audit restored (#618)

The earlier failure was a stale reference calculation, not a ring AI defect.
After the first hit the audit parks Samus at (768,768), outside the shared
angle routine's intended byte range. The production helper correctly retained
native divider truncation from #402, but this independent reference still
divided full host integers. Pinned $A0:C0D2-C0DE/$C0F2-C0FE selects byte writes
to $4204-$4206 after word-sized octant selection; zero divisors yield $FFFF.
The existing original-CPU 225-coordinate matrix in enemy-angle-402.csv verifies
these semantics independently of the Mother Brain implementation.

Correcting only the reference's operand widths restores the entire
--mother-brain-audit run. All four rings retain exact angle/velocity assertions,
and the live Baby drain/healing sequence now reaches the added audio-cadence
assertion on every healing call. No production code changed for #618.
