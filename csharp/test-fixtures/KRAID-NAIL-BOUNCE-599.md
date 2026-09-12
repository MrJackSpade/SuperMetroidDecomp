# Kraid horizontal fingernail bounce (#599)

Developer-discovered at 52e31f3a; no player release inferred.

Pinned A7:BE9E..BEAF and BEE2..BEF3 negate the fractional and whole words
independently (EOR FFFF, INC for each). The upstream C translation agrees.
The port used the same mathematical 32-bit negation as its vertical branch,
introducing a fractional borrow that the horizontal cartridge code does not.

Before production changes, an actual flight update in an empty constructed
room reflected X velocity 0001:0001 from the body contour to FFFE:FFFF.
The native result is FFFF:FFFF; the test failed with expected 65535, got 65534.
Ordinary launch records have zero fractions, so this is not evidence that every
normal battle had a visible bounce error.

The correction uses independent-word reflection for both horizontal branches.
Vertical reflection is unchanged and is not covered by this parity claim.
131,072 actual updates cover every fractional word for both contour and solid
wall reflections. Assertions include post-move positions/fractions. The wall
fixture forbids bus reads to prove it bypasses contour processing and reflects
only once. The contour fixture consumes pinned retail contour data.

Full Release Verification, complete Kraid audit and Windows Release build pass.
The fix remains awaiting player confirmation.
