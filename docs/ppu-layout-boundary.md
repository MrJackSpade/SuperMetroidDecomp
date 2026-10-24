# PPU layout boundary

## What this boundary is for

The destination has no PPU in the simulation. The simulation publishes a display model (sprites as composed
frames, background layers, palettes as colours, effects as descriptions) and a presentation backend draws it;
the Godot host will be one such backend. VRAM addresses, CGRAM slots, OAM packing and register numbers are then
knowledge of whichever backend emulates the original hardware, and of nothing else.

Presentation quirks are not requirements (sprite caps and overflow, per-part priority, OAM parking, exact
scanline rasterization, stale tilemap strips, VRAM layout effects). What stays exact is anything gameplay can
observe: draw-time side effects that mutate simulation state, and display state that gameplay reads back.

Everything below is the scaffolding that isolates the PPU while the display model replaces it: a boundary
labels a reach-in so it can be removed, a layout catalog is the first step for a VRAM base that used to be
inline, and a rendering view is a step toward the simulation not knowing the backend at all. Nothing new goes
behind this boundary; a new display need is expressed in the display model, and the backend learns to draw it.

## Layers while the PPU is being removed

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
- The backend that emulates the PPU preserves its behaviour exactly (byte/word port behaviour, VMAIN
  increment/remap modes, latch order, OAM low/high packing, CGRAM masking, register side effects, snapshot
  ownership) because that backend is the current renderer. None of it is a requirement on the simulation or
  on any other backend, except where gameplay reads the result back.

## Catalogs, aliases and views in practice

- Catalog files end in `Layout.cs` and
  declare `public static readonly PpuLayoutCell` fields whose `Name` is the member name.
- `PpuMemoryLayout` holds the physical containers: all VRAM, all CGRAM, the background and OBJ
  palette banks and their sixteen 16-colour rows, and the OAM tables. Every screen reuses VRAM,
  so a screen-level VRAM allocation declares `aliasOf: PpuMemoryLayout.Vram`; a sub-use of a
  named plane aliases that plane (an enemy BG2 tilemap aliases `GameplayVramLayout.Bg2Tilemap`).
  CGRAM ranges are declared with `PpuMemoryLayout.Colors(name, first, count, encoding)`, which
  aliases the smallest covering row, bank or the whole CGRAM.
- Gameplay keeps no VRAM (#1406): background artwork binds by character set and character
  (`BgArtworkBindings`, `BgCharacterSet`). The VRAM views are the import pipeline's, in
  `SuperMetroid.AssetExtraction.Video`: `VramRegion` (byte loads, VMDATA word streams, word,
  tilemap and byte reads, slices), `Mode7Vram` (the interleaved map and character planes), the
  gameplay layouts `GameplayVramLayout` and `RoomGraphicsLayout`, `BgCharacterWords` (the codec
  from native words to character sets) and `VramDmaDestination`. `CgramPalette`/`CgramPalettes`
  (`Bgr555` reads and writes, CGDATA port streams, banks and rows) stay in
  `SuperMetroid.Core.Hardware`. Their cell-binding constructors are `[PpuLayoutAccess]`: only a
  domain view binds a cell, and callers use its properties.
- The software rasterizers (`SnesBgTilemapRenderer`, `SnesObjRenderer`, `SnesMode7Renderer`,
  `SnesGameplayFrameRenderer`, …), the GPU dispatch and render-packet producers model BGnSC/NBA
  and OBSEL register values and the snapshot images; they are backend or codec boundaries.
- Data-driven destinations (a native transfer record's VRAM word, a PLM write-table record, a
  library-background command) are decoded by narrow codec members such as
  `PlmVramWriteRequest.Destination`.

## Inventory and checks

`docs/ppu-layout-inventory.json` is generated from the catalogs (owner, name, region, offset,
count, encoding, aliases, consumers) by `SuperMetroid.DebugRunner --generate-ppu-layout-inventory`,
run from the repository root; it refuses to write while a catalog rule is broken. The verifier
(`--ppu-layout-boundary`, part of the full run) requires the committed inventory to list exactly
the catalog cells and checks:

- `(owner, name)` is unique; a cell's alias shares storage with it; two cells share storage only
  when one's alias chain reaches the other or a cell covering it;
- every alias pair round-trips through the typed views on a real backend (VRAM and CGRAM writes
  through one view read back through the other; `gameplay_TM` is TM's NMI-latched copy);
- representative typed round trips (tilemap word, `Bgr555` palette entry, object attributes,
  register value), view and cell bounds, VMAIN one/32-word increments and the 15-bit VMADD wrap,
  DMA mode 1 and Mode 7 port byte order, OAM low/high-table packing and CGRAM bit-15 masking;
- representative view paths (palette writes, tilemap row/column writes and reads, Mode 7 map
  loads) allocate nothing once warm.
