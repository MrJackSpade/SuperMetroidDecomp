# PPU layout boundary (#630)

Rendering, runtime, frontend, tooling and test code never know the PPU's physical layout. Register
numbers, VRAM word/byte addresses, CGRAM slots, OAM low/high-table packing and their backing buffers
live in declared boundaries; every other caller works with typed, domain-named members —
background tilemaps and characters, palettes and `Bgr555` colours, object attributes, Mode 7 data,
windows, screen selection and colour math. This mirrors the CPU-bus boundary (#629,
`docs/cpu-bus-boundary.md`).

## Layers

| Layer | Examples | May know layout? |
|---|---|---|
| PPU backend | `SnesVram`, `SnesCgram`, `OamBuffer`, `VramWriteQueue`, `GameplayWindowRegisterCache` | yes — raw members are `[PpuLayoutAccess]`; mixed types are `[PpuLayoutBoundary("backend: …")]` |
| Layout catalogs | static classes of `PpuLayoutCell` per domain (gameplay BG tilemaps, HUD, menu planes, palette rows, OAM ranges, window/screen registers), `SnesPpuLayout` | yes — the only place a VRAM base, colour slot or register number appears; `[PpuLayoutBoundary("catalog: …")]` |
| Rendering views | typed views over the backend (e.g. a BG tilemap view returning `SnesTilemapWord`, a palette view taking `Bgr555`, an object view taking `SnesObjAttributeWord`, a window-register view) | through cells only — `[PpuLayoutBoundary("rendering view: …")]` |
| Codecs | render-snapshot/packet codecs, VRAM/CGRAM/OAM upload payloads, DMA transfers, asset uploads | through cells only — `[PpuLayoutBoundary("codec: …")]` |
| Exact hardware tests | port/latch order, increment and wrap modes, OAM packing, CGRAM masking, alias coherence | narrowest method — `[PpuLayoutBoundary("exact hardware test: …")]` |
| Everyone else | gameplay, rendering composition, frontend screens, behavioural verification | **no** — typed members only |

## Rules (analyzer)

`PpuLayoutAccessAnalyzer` shares `LayoutBoundaryAnalyzer` with the CPU-bus rules:

- **SME6300** — using a `[PpuLayoutAccess]` member (invocation, property, field/constant, method
  group, construction with arguments, or a concrete implementation of an attributed interface
  member) outside a `[PpuLayoutBoundary]`. Constructing a backend with no arguments is not access.
- **SME6301** — a class/struct implementing a `[PpuLayoutAccess]` interface outside a boundary.

Boundaries are narrow: the backend/view/catalog/codec type or the exact test method, never a whole
project, a test family or the `Program` partial. The analyzer checks the enclosing member, so a
boundary on a local function or lambda has no effect.

## Cells, views and types

- Every VRAM region, CGRAM range, OAM range and register is a `PpuLayoutCell` in a domain catalog,
  with its native identity (VMADD word, CGADD slot, `$21xx` register, native symbol) in XML docs.
  Two meanings that share storage are two cells; the second declares `aliasOf:`.
- Views bind from cells and expose domain types end to end: `SnesTilemapWord`/tile words,
  `SnesObjAttributeWord`, `Bgr555`, bounded indices, typed register values. Masks, packing, byte
  order and raw-word conversion happen inside the view or codec.
- Ranges are bounded typed views and batch/DMA operations — never raw spans handed to callers.
  Hot paths stay allocation-free: views are structs over the backend, batch operations take spans.
- `Bgr555` is the colour type everywhere after the raw CGRAM word crosses the codec boundary.
- Preserve PPU behaviour exactly: byte/word port behaviour, VMAIN increment/remap modes, latch
  order, OAM low/high packing, CGRAM masking, register side effects, snapshot ownership.

## Inventory and checks

`docs/ppu-layout-inventory.json` is generated from the catalogs (owner, name, region, offset,
count, encoding, aliases, consumers). The verifier checks unique identities, overlap only through
declared aliases with alias-coherence round trips, bounds, port/latch order, increment/wrap modes,
and rendering equivalence; a performance check confirms no hot-path allocation.
