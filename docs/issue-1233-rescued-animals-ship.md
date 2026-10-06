# #1233: Rescued animals escape pod

Affected version: 0.4.15-smoke1231. The player reported that the animals' ship
did not appear leaving the planet after their rescue.

The ending constructor discarded the saved-animals event, and the ending had no
implementation of the conditional escape-pod actor. Forward event $0F from the
live gameplay system and spawn the native actor at the slow-to-accelerating
flyaway transition. Its dedicated definition catalog contains the native slot,
origin, palette, four-frame instruction loop, and small-OBJ composition. The
existing installed explosion character sheet already contains the pod artwork.

Reference: pinned NTSC J/U 1.0 disassembly
362be646929cf8e483f692b73a6561cfc2dc1d0d:
- $8B:DD98-DDA9 tests event $0F and spawns $EF21 in byte slot $04.
- $8B:EF99-EFB1 initializes (128,128), palette seven, and general timer $0104.
  This general timer is not a spawn delay.
- $8B:EFB2-EFE9 advances X by one pixel and the Y fraction by $0080 per call,
  deleting at X=$0110. The initial move occurs on the spawn frame.
- $8B:ECD9-ECED loops four one-frame maps at $8C:BC41/48/4F/56.
- Those maps select small tiles $1E0-$1E3, priority three, palette seven,
  with (-4,-4) offsets. Preserve native OAM slot ordering against the afterglow.

RescuedAnimalsShipAudit reproduces the missing actor through the real frontend
ending handoff. Before the fix: rescued=true, expected pod=true, actual=false.
The final fixture advances the real explosion-to-flyaway sequence with installed
assets and audio acknowledgements. It verifies no early spawn, all 143 active
positions and animation frames, deletion at age 144, and absence without rescue.
Rendered pixel comparison inside the pod's 8x8 bounds confirms visible flight;
its first distinguishable pixels occur at actor age 62 as it leaves the glow.
The native palette/afterglow sequence matters, so the fixture retains it rather
than asserting against an artificially initialized black cinematic frame.

Release DebugRunner build and --rescued-animals-ship-audit passed.
Awaiting player validation.
