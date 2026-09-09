# Ordinary walljump parity — #473

Partially resolved diagnostic; #473 remains open. Include `native-release-probe.h` and
`native-walljump-probe.h` after `struct StateRecorder;` in pinned `sm_rtl.c`.
In `main.c`, before SDL, dispatch `DiagnosticWalljump(argv[2], argv[3])` for
`argc == 4` and `--walljump-probe`. Build Release/x64/v145 with absolute upstream
SolutionDir. Run on the same unpatched cartridge documented in SPINJUMP.md:

```
sm.exe --walljump-probe "Super Metroid.smc" NEW_TRACE.csv
dotnet run --project csharp/src/SuperMetroid.DebugRunner -c Release --no-launch-profile -- --walljump-comparison-audit "Super Metroid.smc" NEW_TRACE.csv
```

The native output is exclusive-create and contains no ROM or save data. Temporary
upstream integration was removed after the run. Both sides use a floor at Y=256,
a full-height solid column (7 or 8), initial spin pose toward the wall at
X=134/122, Y=160 with zero subpixels/speeds, animation frame zero/timer one and
falling direction. No equipment, enemies, grapple or gameplay cheats. Away is
held from frame zero; Jump begins at delays 0..12. Actual input, movement,
animation and pose transitions run for 30 frames in each of 26 cases.

Initial result: 741 of 780 exact X/Y/pose/animation samples differ. First rightward
away case diverges at frame 2: managed X=0086.C000, native=0086.0000, with matching
Y=00A0.5400, spin pose $19 and check animation frame $0B. Native horizontal motion
then advances in whole pixels while the wall-check contact remains active.
This is a lead, not an established cause: verify native/managed initialization
and collision-probe side effects before changing gameplay.

The printed managed success windows are observational, not golden expectations:
rightward away delays 2..6, leftward away 2..5. The comparer must continue to fail
until the exact discrepancy is diagnosed and resolved. Repeated same-wall jumps,
overhangs and post-walljump Up/Down/charge rules are still outstanding.

## Fractional collision write correction

The native solid horizontal dispatcher ($94:8F49) writes live X subposition
even when reached through the observational wall probe ($94:967F): zero on a
leftward hit and FFFF on a rightward hit. The managed copied probe discarded
those writes. Propagating the block-hit fractional word, without committing
integer position, reduces the same trace from 741 mismatches to 13. All 13 are
the first frame of the leftward-away cases; every subsequent frame matches
exact X/Y, pose and animation. Both managed success windows now accept delays
2..8 in this specific geometry. This is not a universal timing claim.

Full core verification passes, including new direct block-probe assertions for
both hit directions and a no-hit case: integer X/Y remain unchanged, block
contact changes only X subposition, and air preserves the original fraction.
The full native comparer intentionally still exits nonzero. The native
last-different-movement gate at $90:9D35 is an investigation lead for the
remaining initial-frame discrepancy, not yet an implemented or verified fix.
