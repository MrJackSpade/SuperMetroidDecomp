# Power-bomb death drawing (#1168)

Affected version: `0.4.0+c1f51a02badbdb9ce80a4c493e97097949cb547b`,
recorded in the supplied October 1 diagnostic log. The report adds that a Power
Bomb damaging an enemy in the long upper Wrecked Ship room triggers the crash.
The description matches the powered Attic ($CA52/$CA7E), whose pinned population
contains yellow KiHunters and Atomics; the ZIP contains no saved state or input
recording to establish the exact room independently.

## Cause and correction

The runtime builds enemy draw queues before `ResolveOrdinaryPowerBombHits`.
A lethal common reaction calls `StartGenericEnemyDeath`, clearing the slot and
its cached definition. The queue still contains the physical slot index.
`DrawLayers` then requested an installed composition for the cleared identity
`$00:0000`, matching the supplied exception and stack.

The draw consumer now skips empty slots and the dedicated respawn placeholder.
This preserves late damage ordering, death explosions, kill counts, and respawn
reservations. It does not accept zero sprite pointers on live actors or suppress
missing installed artwork errors.

Pinned disassembly `362be646929cf8e483f692b73a6561cfc2dc1d0d`:
`bank_A0.asm` has power-bomb interaction at $A0:A306 and common death at
$A0:A3AF. The latter clears the physical slot at $A0:A3EE and optionally writes
the $DAFF reservation and bank $A3 at $A0:A3FE-$A407. `bank_A1.asm` documents
the Attic population. These references establish the lifecycle and ordering;
the correction handles those states explicitly at the installed-artwork boundary.

The cartridge's zero-sprite result is also explicit: $A0:94C2-$94C5 copies
the cleared graphics offset into direct-page $0000; $A0:94EB loads the cleared
spritemap pointer into Y. At $81:8AB9, `LDA $0000,Y` therefore reads that zero
through the low-WRAM mirror (both bank $00 and the respawn bank $A3 map it).
`BEQ` at $81:8ABC immediately returns without emitting OAM. The port's lifecycle
guard preserves this result without interpreting scratch RAM as installed art.
These instruction bytes were checked directly in the supported NTSC J/U v1.0
ROM, including `BD 98 0F 85 00` at $A0:94C2, `BC 8E 0F 22 B8 8A 81` at
$A0:94EB, and `5A B9 00 00 F0 61` at $81:8AB8.

## Focused confirmation

`--power-bomb-death-drawing` builds a real draw queue, applies the production
Power Bomb damage routine, then calls the production layered renderer. AI is
frozen during queue construction to isolate this ordering from movement.
The Atomic case reproduced the exact `$00:0000` exception before the fix.

After the fix, lethal Atomic and Sidehopper cases emit no enemy OAM, preserve
the explosion, and increment the kill count once. A respawning Sidehopper
retains its reservation without drawing. A nonlethal Atomic retains identical
OAM bytes, while an invalid live sprite pointer still fails. These are bounded
fixtures confirming this lifecycle correction, not a room replay or player
validation. The program visual static-analysis build gate also passes.

The hotfix is developed on `fix/v040-hotfix`, for the next release rather than
merging into main. Ticket closure follows the player's explicit instruction;
no `in-progress` label is used.
