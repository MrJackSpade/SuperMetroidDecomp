# Speedball block-family contrasts (#470)

## Reproduced defect

The initial nine-family native comparison failed in both facings for bombable
air (type 7/BTS 0) at active boost stage four. Both implementations passed through
the cell, but C# left 7123 and allocated no actor. Cartridge setup immediately
wrote 0058 and allocated one PLM. This is a destruction/animation omission, not
a blocking collision difference.

Pinned bank 94 routines 92F9/9313 call the same collision bomb PLM table as solid
bomb blocks, but return carry clear independently of setup's result. C# had
classified bombable air with unconditional air stubs and skipped setup altogether.
Horizontal and vertical dispatch now invoke the existing shared setup when its
admission predicate succeeds, ignoring the result for movement. An unavailable
PLM slot or rejected setup cannot turn an air cell into a solid obstruction.
Existing Screw Attack/Shinespark admission is preserved; no Speedball-only
destruction algorithm or speed adjustment is added.

## Native matrix

The same one-call grounded-ball seed as SPEEDBALL-BLOCKS.md is used: both facings,
base speed 3, extra speed 2, stage counters 0/0300/0400, Morph Ball and Speed
Booster, floor row 16. The CSV bts column is a **family index** for this variant,
not the literal behavior byte. Nineteen targets make 114 native cases:

| Index | Type/BTS | Expected contact |
|---|---|---|
| 0 | 0/00 | Air; no actor or visual mutation |
| 1 | 7/00 | Air; active boost writes 0058 and allocates actor |
| 2 | 8/00 | Solid at every stage |
| 3 | B/0E | Only active boost writes 00B6 and passes |
| 4..6 | C/00, C/08, C/09 | Remain solid; no actor/mutation |
| 7 | E/00 | Remains solid; no actor/mutation |
| 8 | F/00 | Only active boost writes 0058 and passes |
| 9..15 | 7/01..07 | Same air/destruction contract as index 1 |
| 16 | 7/80 | High-bit air reaction skipped; always passes unchanged |
| 17 | F/80 | High-bit solid reaction remains solid |
| 18 | B/0F | Only active boost writes 00B6 and passes |

Original visual bits are 0123. Rejected solid contact cancels momentum/boost;
air retains them even at inactive stages. Every sample compares complete X/Y,
base/extra velocity, live level word, boost and active PLM count. Explicit
family assertions independently check mutation and actor eligibility.

Additional focused vertical production scans exercise all eight bomb BTS values,
both air and solid types, all three stages and both vertical directions (96).
They assert exact accepted displacement, collision, live word and actor count;
these are synthetic checks, not native vertical traces. Original 48 solid
horizontal cases remain independently replayable with the old CLI/capture.

## Capture

Include native-release-probe.h and native-speedball-block-probe.h after native
StateRecorder; dispatch DiagnosticSpeedballBlockFamilies before SDL. Temporary
hooks were removed after capture; no GUI or player saves are used.

speedball-family-native-capture.zip contains the accepted v2 CSV; independent
repeat SHA256 matches:
`FADBEB0C7FD5DB32A67B1EC6EFAEA2302583A10F04A63012EBE6A7F1F209AE07`.

```powershell
dotnet run --project csharp/src/SuperMetroid.DebugRunner -c Release -- --speedball-family-comparison-audit "Super Metroid.smc" path/to/speedball-families-470-v2.csv
```

ROM SHA256: 12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72,
Japan/USA NTSC rev0. Native source 578f90b3cc49557bb70060ad033bb90b8cf8ac50;
disassembly 362be646929cf8e483f692b73a6561cfc2dc1d0d.

All 114 native cases and 96 vertical checks pass. The controller-acquired
Speedball matrix still matches all 120,000 frames after the production change.
The complete bank-$80 verification executable also passes.
Temporary Blue Suit conversion is still outstanding; #470 is not yet ready
for player validation. These contact fixtures do not prove that conversion.
