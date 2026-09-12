# Ceiling wrap-around investigation (#410)

Status: native address generation reproduced; PLM overload and traversal remain
unverified. This is not a completed gameplay fix.

## Original CPU experiment

`native-ceiling-wrap-probe.h` runs the original `$94:A352` routine with all-air
synthetic rooms, width 112/128/144 blocks and height 32. Projectile slot 0 has
X = 63/64/65 tiles, Y = 4 pixels, radii 1/8, zero velocities and subpixels.
Fresh zeroed WRAM per case. No gameplay cheats or instruction substitutions.
It logs CPU X at `$94:A3D0`, before each horizontal block reaction. The loader
restores the original ROM bytes after the comparison harness initializes.

The pinned NTSC ROM and upstream revisions are the same as WRAP-SHOTS-409.md.
Two independent runs produced identical 18-record CSVs. These are synthetic
register/address observations, not exported cartridge assets or player saves.

| Width | Leading tile X | First byte offset | Second byte offset |
| --- | --- | --- | --- |
| 128 | 63 | FF7E | 007E |
| 128 | 64 | FF80 | 0081 |
| 128 | 65 | FF82 | 0083 |

The 112- and 144-block width controls retain even offsets. At width 128, the
first width addition overflows for X >= 64; the second ADC consumes that carry
and adds an extra byte. Pinned bank-94 `$A3D4..A3DB` confirms this ordering.
`$A1B5` bounds the byte offset, then `$A1BB..A1C1` separately floors the block
index and reads the level word at the original, potentially odd byte address.
Therefore merely wrapping a normal block index loses observable behavior.

The C# horizontal Wave path currently calls `ScanHorizontalShotReactions`,
which rejects the underflowed top row before dispatching any block reaction.
This differs from the observed original CPU path. No production edit has yet
been made: the next experiment must establish odd-word interpretation, actual
PLM allocation/slot exhaustion, and the resulting Frog Speedway traversal state.
The address trace alone cannot establish the issue's full acceptance criteria.

## Reproduction wiring

Temporarily include `native-release-probe.h` and this probe after `state_recorder`
in upstream-sm/src/sm_rtl.c. Dispatch `DiagnosticCeilingWrap(rom, output)` before
SDL initialization and suppress SDL warning/error dialogs for that headless
entry only. Rebuild Release x64, then run:

```
sm.exe --ceiling-wrap-probe "Super Metroid.smc" NEW_OUTPUT.csv
```

The output uses exclusive creation. Remove only these temporary hooks afterward;
do not reset unrelated upstream changes. CPU execution has a 100,000-instruction
budget per case and returns a nonzero exit status on exhaustion.
