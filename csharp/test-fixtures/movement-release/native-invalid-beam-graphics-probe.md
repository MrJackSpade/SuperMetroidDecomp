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
