# Charged elevator boarding (#551)

Affected player version: 0.1.1.

Run `SuperMetroid.DebugRunner --elevator-charge-audit ROM` for a bounded,
room-local reproduction in the retail Morph Ball room. The fixture charges
through actual frame inputs, jumps, lands and activates the upward elevator.
No precharged counter or fabricated departure event is injected.

Before the fix, boarding occurred at jump frame 60 and cleared the flare counter,
but all sixteen Samus CGRAM entries still differed from the normal suit palette.
The existing #327 test only checked admission of new charge during travel; it
did not exercise this already-charged palette lifetime.

The pinned cartridge `MakeSamusFaceForward` ends its flare cleanup with
`LoadSamusSuitPalette` at $91:E4A5. The translated actor handled pose/projectile
cleanup but omitted the runtime-owned palette handoff. The departure event now
performs that same suit-palette load, just as the gunship entry event already did.

After the fix, Power, Varia and Gravity cases pass. Assertions compare all sixteen
live palette entries at boarding and both live and displayed packet palettes for
twenty subsequent held-Fire travel frames, while charge remains zero. The Release
build and full Core verification suite pass. These are upward boarding tests;
they do not claim coverage of every room, direction or beam combination.

This is separate from #516's wrapped feet. Leave open awaiting player validation.
