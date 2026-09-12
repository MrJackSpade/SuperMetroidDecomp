# Ridley Power Bomb neutral-tail field ownership (#596)

Developer-discovered at 2ee56f306b8ea9ef9b5bc396cc0095ddf5270cee during
#547's claw-offset audit. No player release version is inferred.

Pinned disassembly 362be646929cf8e483f692b73a6561cfc2dc1d0d, bank A6:
$BD2C calls SetNeutralRidleyTail at $B84D. That routine stores one to
tailFunctionIndex and tailAngleDelta. Pinned upstream-sm
578f90b3cc49557bb70060ad033bb90b8cf8ac50 expresses these as tilemap_stuff[0]
and tilemap_stuff[10]. Neither is Ridley's animated foot-displacement index.

The translated Power Bomb reaction instead assigned FeetDistanceIndex = 1,
leaving TailAngleDelta stale and changing the grab geometry. Corrected that
single destination field; no reaction gates, timers or phase behavior changed.

## Reproduction and verification

Program.RidleyNeutralTail.cs invokes the actual private combat-preparation
method. Before the fix, a zero angle delta fails with expected 1, got 0.
After the fix, 65,536 angle/foot-word combinations require tail function one,
angle delta one, unchanged foot displacement, consumed reaction latch and
the lunge phase. Eleven inactive gate combinations preserve both tail words,
the foot index and the original phase.

This is a faithful synthetic reproduction of the incorrect native write,
not a claim of player-observed visual confirmation. The issue remains open
for player validation after the fix.

Full Release Verification passes. The complete Norfair Ridley audit passes
360 reveal frames, 4,096 combat frames across ten states, and 738 death frames.
DebugRunner and Windows Desktop build with zero warnings and errors.
