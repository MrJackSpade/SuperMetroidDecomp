# Spatial value types (#628)

Spatial data — positions, displacements, velocities, sizes and radii — is carried as one typed
value, never as parallel X/Y, width/height or horizontal/vertical scalars. The types live in
`csharp/src/SuperMetroid.Core/Spatial` and the analyzer rules SME6280-SME628x enforce them.

## Coordinate spaces

Every spatial value is branded with an `ISpatialSpace` marker. Values in different spaces are
different types; moving between spaces is always a named conversion.

| Space | Meaning |
|---|---|
| `RoomPixels` | Room-relative pixels: Samus, enemies, projectiles, PLM blocks, the camera's top-left corner (BG1 scroll). |
| `ScreenPixels` | Viewport-relative pixels: a room position minus the camera position. |
| `Bg2Pixels`, `Bg3Pixels` | BG2/BG3 scroll-register pixels, which follow their own parallax/FX rules. |
| `RoomBlocks` | The sixteen-pixel collision/level-data grid. |
| `RoomScreens` | The 256-pixel scroll-region grid. |
| `MapTiles` | Area-map tiles (pause, file select, minimap). |
| `MapPixels` | Area-map plane pixels (eight per tile): map scroll offsets, scroll bounds and map marker/arrow anchors. |
| `OamPixels` | OAM sprite coordinates (nine-bit signed X, eight-bit Y). |
| `Mode7Pixels` | Mode 7 plane coordinates and matrix centre. |
| `TilemapCells` | VRAM tilemap cells. |
| `ImagePixels` | Host raster pixels: decoded images, artwork atlases, tile bitmaps and rendered frames. |

Add a space only when values in it must not mix with an existing space; document it here and in
`SpatialSpaces.cs`.

## Shapes

| Type | Representation | Use |
|---|---|---|
| `Position16<TSpace>` | `ushort X, Y`, wrapping at 16 bits | Whole-pixel positions, camera and scroll positions, block/screen/tile coordinates. |
| `Offset16<TSpace>` | `short X, Y`, wrapping at 16 bits | Signed displacements and relative offsets. |
| `Extent16<TSpace>` | `ushort X, Y` | Sizes and half-extents (radii). The owning member name says which. |
| `FixedPosition<TSpace>` | `SnesFixedPosition X, Y` (whole + subpixel word per axis) | Native 16.16 positions such as Samus and enemy positions with subpixels. |
| `FixedVector<TSpace>` | `SnesSignedSixteenSixteen X, Y` | Native signed 16.16 displacements and per-frame velocities. |
| `Point32<TSpace>` | `int X, Y`, ordinary non-wrapping arithmetic | Host-side coordinates and displacements that are not native WRAM words: bounds-checked grid cells (room blocks, room screens, map tiles, tilemap cells), image pixels and authored JSON anchors. |
| `Size32<TSpace>` | `int Width, Height` | Host-side grid, image and rectangle sizes; `Contains`, `RowMajorIndex` and `Area` own the row-major indexing. |

The native per-axis arithmetic lives once in `SnesSignedSixteenSixteen` and `SnesFixedPosition`
(whole/fraction assembly, carry/borrow, 32-bit wrap, half-word replacement). The two-dimensional
types delegate to it and never re-derive it.

## Operations

- Natural operators only where the meaning is unambiguous and lossless:
  `Position16 ± Offset16 → Position16`, `Position16 - Position16 → Offset16`,
  `Offset16 ± Offset16`, `FixedVector ± FixedVector`, `FixedPosition.Advance(FixedVector)`.
- Axis replacement: `WithX` / `WithY`. Half-word replacement where a cartridge routine stores only
  one word: `SnesSignedSixteenSixteen.WithWholePixels/WithFraction`,
  `SnesFixedPosition.WithWhole/WithFraction`.
- Conversions between spaces (room → screen via the camera, pixels → blocks, room → OAM, ...)
  are named methods/extensions in `SuperMetroid.Core.Spatial` next to the spaces they join:
  `RoomGridConversions` (room pixel → block, block/screen → room pixel, screen size → block size)
  and `MapScreenConversions` (map tile → map pixel, map pixel → screen under the map scroll).
- Shape conversions within one space are explicit: `Point32.ToPosition16()` keeps each axis's low
  word, `Point32.ToExactPosition16()` fails outside 0..65535, `Point32.ToOffset16()` keeps the
  signed low word. Choose `Position16`/`Offset16` for native words that wrap and `Point32`/`Size32`
  for host-side values whose arithmetic must not wrap.
- Never floating-point vectors in cartridge logic: preserve native width, signedness, wrap order,
  origin and truncation exactly.

## Ownership rules

- Objects expose one whole-value property (`Position`, `Velocity`, `Radius`, `ScrollPosition`,
  `ExtraDisplacement`, ...) instead of independent X/Y setters. Axis-specific updates use
  `WithX`/`WithY` or a named domain operation.
- Methods that accept or return one location/vector/size take one typed value, not adjacent
  scalar parameters or tuples.
- Collections store spatial values, not parallel component arrays.
- Components are split only at exact boundaries — native WRAM/save-state/snapshot codecs,
  hardware registers, rendering-backend and P/Invoke adapters — through one named codec
  operation, and those declarations carry `[SpatialComponents("reason")]`.
- Debugger/save-state compatibility is preserved by the existing debugger state field migrations,
  never by keeping primitive pairs in owned logic. A field merged from stored components is
  registered in `DebuggerSpatialFieldMerges` (legacy component names and payload types plus the
  exact composition); `VerifyDebuggerSpatialFieldMerges` round-trips every entry.
