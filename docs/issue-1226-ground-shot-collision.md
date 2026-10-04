# #1226: Standing beam clips through sloped ground

Affected version: v0.4.13, confirmed by the user as the latest published release.
Input: player-supplied SuperMetroid-debug-slot-0.smstate. Room $9CB3, RoomId
$01/$08; Samus X=1635, Y=171, pose $0001, unupgraded beam $0000. Position,
equipment, collision data, and captured controller bindings are preserved.

## Reproduction and cartridge comparison

Firing in place launches a rightward beam at Y=166, radii 8/4. The slope at
row 10/column 104 has BTS $12; subsequent slope cells use $13. Before the fix,
the shot crosses those cells and dies only at the solid tile: frame 15,
X=1716, fractional X=36864.

The bounded native fixture runs the original NTSC J/U 1.0 CPU routine $94:A23B
with this shot's coordinates, velocities, and relevant terrain. Results:

| Input frame | X | Fraction | Y | Type |
| --- | --- | --- | --- | --- |
| 2 | 1650 | 4096 | 166 | $8000 |
| 3 | 1654 | 12288 | 166 | $8000 |
| 4 | 1666 | 24576 | 166 | $8700 (impact) |

This confirms the player's cartridge comparison. It is an original-routine
comparison with bounded state, not a full cartridge playthrough of the room.
Sources: pinned upstream-sm/src/sm_94.c and InsaneFirebat disassembly revision
362be646929cf8e483f692b73a6561cfc2dc1d0d, bank_94.asm $A147/$A15E and $A543-$A5E2.

## Cause and correction

RunShotReaction never dispatched height-profile slope blocks; its fallback
handled only solid block types $8-$F, making slope type $1 behave as air.
The fix restores the native non-square slope reaction: admit the projectile's
perpendicular center block, mirror within-block coordinates using BTS, compare
against the compiled native height profile, and terminate the span on a hit.
That last step matters: native $94:A5DC/$A5DE clears both counters, overriding
previous air reactions. Wave still discards collision carry while honoring the
scan termination. No room-specific collision rule is introduced. Square-half
slope definitions and missile behavior are outside this reported correction.

## State compatibility needed for this reproduction

The supplied release state predates the two tester flags and three runtime
policy fields. Explicit legacy layouts now omit exactly those new fields;
booleans remain false and the inventory recipient remains null. This is selected
before the older horizontal-spike migration, preventing that migration from
misidentifying the three missing tester fields. Serialized-field set validation
still runs. The supplied state loads and the diagnostic asserts new cheats off.

## Confirmation and repeat

The guarded DebugRunner command loads a private copy, binds installed artwork,
uses the captured Shoot binding, and asserts the exact native collision:

    --captured-ground-shot-audit <state.smstate> <installation-directory> <output-directory>

Before production changes, the frame-four assertion fails with X=1658:24576,
Y=166, type $8000. Afterward it passes with X=1666:24576, Y=166, type $8700.
The resulting impact capture was visually inspected. Release build passes.
The original state, screenshots, and detailed traces stay local.

The native fixture is csharp/test-fixtures/movement-release/native-ground-shot-probe.h.
Use the existing movement-release native loader integration, then apply
native-ground-shot-entrypoint.patch in a private native checkout and run
`sm.exe --diagnostic-ground-shot <retail-rom>`. Its assertion passes independently
of the managed collision implementation.

Player confirmation remains pending on #1226.
