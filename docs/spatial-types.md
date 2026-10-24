# Spatial value types

Spatial data — positions, displacements, velocities, sizes and radii — is carried as one typed
value, never as parallel X/Y, width/height or horizontal/vertical scalars. The types live in
`csharp/src/SuperMetroid.Core/Spatial` and the analyzer rules SME6280-SME6283 (errors) enforce them.

Spatial values keep sixteen-bit wrapping because it is load-bearing: room coordinates, scroll positions and
collision probes wrap, and the game's behaviour at room edges, during door transitions and in the glitches the
project reproduces depends on that wrap. This is the documented exception to widening (`manifesto.md`,
"Load-bearing behaviour versus incidental representation"); it is a statement about the game's geometry, not
about the processor.

## Coordinate spaces

Every spatial value is branded with an `ISpatialSpace` marker. Values in different spaces are
different types; moving between spaces is always a named conversion.

| Space | Meaning |
|---|---|
| `RoomPixels` | Room-relative pixels: Samus, enemies, projectiles, PLM blocks, the camera's top-left corner (BG1 scroll). |
| `ScreenPixels` | Viewport-relative pixels: a room position minus the camera position, and the on-screen positions of cinematic and menu objects. |
| `ScreenSubpixels` | Viewport positions in 1/256 pixels: unsigned 8.8 words whose high byte is the screen pixel (the escape timer). |
| `OamPixels` | OAM sprite coordinates (nine-bit signed X, eight-bit Y): every spritemap draw origin and every authored sprite anchor. |
| `Bg2Pixels`, `Bg3Pixels` | BG2/BG3 scroll-register pixels, which follow their own parallax/FX rules. |
| `Bg3Subpixels` | BG3 room-FX texture drift in 1/256 pixels (the 8.8 rain, fog and spores accumulators). |
| `BgPlanePixels` | The scroll words of whichever background plane a generic render layer samples. |
| `RoomBlocks` | The sixteen-pixel collision/level-data grid. |
| `RoomScreens` | The 256-pixel scroll-region grid. |
| `TilemapBlocks` | The sixteen-pixel block grid of a wrapping BG tilemap (bank-$80 `BG1/BG2 X/Y block`), distinct from room blocks. |
| `TilemapCells` | VRAM tilemap cells. |
| `MapTiles` | Area-map tiles (pause, file select, minimap). |
| `MapPixels` | Area-map plane pixels (eight per tile): map scroll offsets, scroll bounds and map marker/arrow positions. |
| `Mode7Pixels` | Mode 7 plane coordinates and matrix centre. |
| `CharacterPixels` | Pixels inside one eight-by-eight character cell. |
| `ImagePixels` | Host raster pixels: decoded images, artwork atlases and other host images. |
| `DisplayPixels` | Host window/swapchain pixels after presentation scaling. |

A host declares its own device spaces beside its adapter, outside Core: the Desktop gamepad's normalized analog
stick is `SuperMetroid.Desktop.Input.GamepadStick`.

Add a space only when values in it must not mix with an existing space; document it here and give it its own
`csharp/src/SuperMetroid.Core/Spatial/<Space>.cs` file beside the others.

Sprite drawing: asset compositions draw at a `Position16<OamPixels>` origin and authored sprite
anchors are OAM coordinates. Game and cinematic state keeps on-screen object positions in
`ScreenPixels` and converts at the draw call with `ScreenConversions.ToOam`.

## Shapes

There is one point/size family. Every coordinate, displacement and size is a native sixteen-bit
value; host-side values (image sizes, grid cells, authored anchors) use the same types.

| Type | Representation | Use |
|---|---|---|
| `Position16<TSpace>` | `ushort X, Y`, wrapping at 16 bits | Whole-pixel positions, camera and scroll positions, block/screen/tile/cell coordinates, image pixels. |
| `Offset16<TSpace>` | `short X, Y`, wrapping at 16 bits | Signed displacements and relative offsets (draw-list steps, part offsets, anchors relative to an origin). |
| `Extent16<TSpace>` | `ushort X, Y` | Sizes and half-extents (radii); the owning member name says which. Grid sizes own `Area`, `Contains`, `RowMajorIndex` and `CellAtRowMajorIndex`. |
| `FixedPosition<TSpace>` | `SnesFixedPosition X, Y` (whole + subpixel word per axis) | Native 16.16 positions such as Samus, enemy, camera and cinematic positions with subpixels. |
| `FixedVector<TSpace>` | `SnesSignedSixteenSixteen X, Y` | Native signed 16.16 displacements and per-frame velocities. |
| `EightEightVector<TSpace>` | `SnesSignedEightEight X, Y` | Native signed 8.8 velocities and accelerations; `ToSixteenSixteen()` promotes them for `FixedPosition.Advance`. |

The native per-axis arithmetic lives once in `SnesSignedSixteenSixteen` and `SnesFixedPosition`
(whole/fraction assembly, carry/borrow, 32-bit wrap, half-word replacement). The two-dimensional
types delegate to it and never re-derive it.

Native 16-bit storage is a rule for spatial values only. Non-spatial gameplay quantities may be widened under
`docs/cartridge-encoding.md`'s "Widened gameplay quantities" rule.

Integer arithmetic that builds a coordinate converts explicitly: `checked((ushort)…)` where a
negative or oversized value is a defect (sizes, image pixels, authored anchors), and
`unchecked((ushort)…)` where native code works in 16-bit words and a later bounds check rejects
the wrapped value (collision and PLM grid probes).

## Operations

- Natural operators only where the meaning is unambiguous and lossless:
  `Position16 ± Offset16 → Position16`, `Position16 - Position16 → Offset16`,
  `Offset16 + Offset16`, unary `-Offset16`, `FixedPosition.Advance(FixedVector)`,
  `EightEightVector + EightEightVector` and `EightEightVector * int` (both wrapping the raw words),
  `FixedVector ± FixedVector` (fraction carry/borrow, 32-bit wrap per axis), and
  `FixedPosition ± Offset16` / `FixedVector ± Offset16` (whole words only, wrapping at sixteen
  bits, fractions unchanged — the platform pushes into Samus's external displacement).
- Single-axis native movement: `FixedPosition.AdvanceX/AdvanceY(SnesSignedSixteenSixteen)`.
- Exact native construction: `FixedPosition.FromWords/FromWhole`, `FixedVector.FromWords`,
  `FixedVector.FromWholePixels(Offset16)`, `SnesFixedPosition.FromRaw/RawValue` for one unsigned
  16.16 axis; `SnesSignedEightEight.SignedRawValue` reads one 8.8 word as signed.
- Axis replacement: `WithX` / `WithY`. Half-word replacement where a cartridge routine stores only
  one word: `SnesFixedPosition.WithWhole/WithFraction`, `SnesSignedSixteenSixteen.WithWholePixels/WithFraction`,
  `FixedPosition.WithWhole/WithFractionsOf/WithWholeX/WithWholeY/WithFractionX/WithFractionY` and
  `FixedVector.WithWhole/WithWholeX/WithWholeY/WithFractionY`.
- Reflection and reversal: `Offset16.MirroredX` (X negated, wrapping; Y kept) for facing-mirrored
  part offsets; `EightEightVector.ReversedX/ReversedY` negate one raw 8.8 word, as crawlers turn.
  Operations exist only where production code uses them; add one with its first consumer.
- Conversions between spaces are named methods/extensions in `SuperMetroid.Core.Spatial` next to
  the spaces they join:
  - `ScreenConversions.RelativeToCamera`: room position minus camera, as a signed screen offset;
    `ScreenConversions.ToScreen`: the same subtraction kept as the wrapped screen position words
    native code stores (Samus's death-sequence screen position);
    `ScreenConversions.ToOam`: an on-screen position as the OAM draw origin;
    `ScreenConversions.WholePixels`: an unsigned 8.8 screen position's high bytes as screen pixels.
  - `RoomGridConversions.ToRoomBlock`: a room pixel to its block (`LSR ×4`);
    `ToRoomPixels`: a scroll-zone screen, or a block (`ASL ×4`), to its top-left room pixel;
    `ToRoomBlocks`: a room size in screens as blocks.
  - `BackgroundBlockConversions.ToTilemapBlocks` / `ToSourceBlocks`: layer pixels to the wrapping
    tilemap block and to the room source block (sign-extended), as the bank-$80 streamer does.
  - `BgPlaneConversions.AsBgPlaneScroll`: a BG1, BG2 or area-map scroll as the generic plane's scroll words.
  - `Bg3FxConversions.AsBg3Scroll` / `WholePixels`: the layer-one anchor of a BG3 effect plane and
    the signed high bytes of its 8.8 drift.
  - `Mode7Conversions.AsMode7Register`: a layer-one room point as signed Mode 7 register words;
    `AsSignedRegister`: a Mode 7 plane position as its signed register words.
  - `MapScreenConversions`: map tile → map pixel, map pixel → screen under the map scroll.
- Never floating-point vectors in cartridge logic: preserve native width, signedness, wrap order,
  origin and truncation exactly.

## Ownership rules

- Objects expose one whole-value property (`Position`, `Velocity`, `Radius`, `ScrollPosition`,
  `ExtraDisplacement`, ...) instead of independent X/Y setters. Axis-specific updates use
  `WithX`/`WithY` or a named domain operation.
- Methods that accept or return one location/vector/size take one typed value, not adjacent
  scalar parameters or tuples.
- Collections store spatial values, not parallel component arrays. Independent per-axis streams
  (BG2's per-line scrolls, `Bg2LineScrolls`) are one type that stores positions and answers per-line values.
- Components are split only at exact boundaries — native WRAM/save-state/snapshot codecs,
  hardware registers, rendering-backend and P/Invoke adapters, and external file formats (the
  JSON document record itself) — through one named codec operation, and those declarations carry
  `[SpatialComponents("reason")]`.
- Enemy family state owns typed storage (#1325). The common enemy slot has no multipurpose words
  (`VariableA`-`F`, `Parameter1`/`2`; SME6408 blocks them): a family stores each value it keeps, spatial values
  included, as a typed field of its own state, and reads its authored spawn parameters from the slot's spawn
  record. The cartridge's words exist only where a native snapshot is translated into that state. Per-slot
  arrays that hold both axes store the typed value (one array, shared by the enemy system and the family).
- Debugger states restore only the current build's layout: changing a field to a spatial type makes older
  states unloadable rather than converting them. Save states are not first-class; never keep primitive pairs
  in owned logic for state compatibility.
