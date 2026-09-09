# Invalid beam graphics investigation (#396)

The current C# loader rejects low-twelve-bit combinations >=12. Retail AC8D has
no such bounds check: it indexes the tile/palette pointer tables with the raw
equipped word. The firing path also currently synthesizes an ordinary Wave
preinstruction from bits instead of reading the native out-of-range callback.
Neither mismatch has been fixed by this diagnostic.

## Probe setup and limitation

Temporarily include native-release-probe.h and native-invalid-beam-graphics-probe.h
in sm_rtl.c and dispatch DiagnosticInvalidBeamGraphics before SDL startup.
**Temporarily remove the SDL_ShowSimpleMessageBox calls from Die/Warning:**
SetErrorMode does not suppress those explicit application dialogs. Restore all
temporary edits and rebuild after the diagnostic. No hooks are committed.

The loader restores the unpatched ROM. Seed equipped beams $000D and run the
real $90:AC8D instructions. The comparison harness currently exits with:

```
Error: The game crashed in cart_readLorom
While trying to read from 0x107fff
PC history: 0x90accd ... 0x90acde
```

This is the harness's LoROM bank-masked address for the palette read at $90:7FFF.
The native palette routine constructs bank $90, copies the indexed pointer into
direct-page $00, and reads via [$00],Y. An earlier combination-$C experiment hit
the same harness check at bank-masked $10:3800.

**This is not evidence of a real cartridge crash.** The harness rejects reads
from unmapped cartridge space; real open-bus behavior and the full caller/PPU
state need validation. No palette trace or complete firing comparison is claimed.
Do not delete the C# guard and declare parity based on this failed experiment.

Next: establish a hardware-faithful read reference, then compare palette/DMA,
raw preinstruction targets, short-range collision and Power Bomb interactions.
Preserve charged-shot instability in modeled state without host memory access.

## Bus-latch experiment (2026-09-09)

Inspection found that this comparison fork never updates `Snes.openBus` during
CPU reads/writes. Merely deleting its unmapped-read Die would therefore return a
stale zero, not a useful reference. A temporary diagnostic variant updated the
latch after each `snes_cpuRead` and `snes_cpuWrite`, and let the existing cartridge
unmapped branch return that latch. All changes were removed after the experiment.

[Snes9x's primary read implementation](https://github.com/snes9xgit/snes9x/blob/master/getset.h)
likewise returns OpenBus for MAP_NONE instead of treating it as a cartridge fault.
This supports the mapping distinction, not an assertion of complete emulator parity.

With CPU-latch tracking, the unpatched ROM's long-indirect palette read observes
$90 (the just-fetched pointer bank) at $90:7FFF. It then crosses into mapped ROM.
Actual completed probe output:

```
PROBE unmapped=107FFF latch=90
BEAM D queue-tail=7 bytes=000121C49A0063
palette=0890,30C2,5822,90EC,6EAD,2919,000F,FCAA,8067,1CAD,C90A,004D,19F0,4EC9,F000,AD14
```

The queue requests $100 bytes from $9A:C421 to VRAM word $6300. Palette output
above is the 16-bit WRAM buffer; PPU CGRAM masks bit 15 on upload. This is a
controlled CPU/bus-model result, not a recording of hardware or a complete Snes9x
playthrough. The important implementation constraint is that the palette routine
must supply the actual instruction's bus context; globally returning zero or the
previous high-level C# read from every unmapped address would be incorrect.

Next production work remains: a scoped instruction-aware palette read, a failing
loader/DMA regression using this trace, then native callback and collision probing.
