# Portable display fixtures (.smframe), versions 1–10

These are display inputs, not gameplay saves. They contain copied PPU memory and
composition commands, without a ROM path, CLR type name, assembly MVID, object
reference or GPU handle. `RenderFrameSnapshotCodec` serializes/deserializes them;
`SoftwareFrameSnapshotRenderer` renders a restored packet without a game runtime.
Only currently extracted composition shapes are supported. Gameplay effects and
additional scene shapes will require an explicitly versioned extension.

All integers are little-endian. Booleans are exactly one byte, 0 or 1. No padding,
compression, optional trailing data or host-native struct layout is used.

## Header

| Field | Encoding |
| --- | --- |
| Signature | Eight bytes: ASCII `SMFRAME` followed by zero |
| Version | UInt16; writer emits 10, reader supports 1 through 10 |
| Host sequence | Int64, positive |
| Load/reset generation | Int64, positive |
| Cartridge frame | UInt16, may wrap independently |
| Outer brightness count | Int32, 0–1024 |
| Outer brightness passes | Count bytes, each 0–15, applied in stored order |
| Composition kind | Byte: 1 solid, 2 Mode7/OBJ, 3 layered |

Version one always produces 256x224 pixels. A new visible-region policy must not
reinterpret these packets. There is no checksum: arbitrary changes to otherwise
valid pixel data remain valid different fixtures. The parser rejects malformed
structure, not every possible file corruption. Store fixture hashes with evidence.

## Composition bodies

Solid: four bytes R, G, B, A.

Mode7/OBJ: memory image, OBSEL byte, scene brightness byte, background-enabled
boolean. If enabled, eight Int16 values A/B/C/D, center X/Y, horizontal/vertical
offset, followed by the character-zero-outside-map boolean. If disabled there are
no matrix fields. All signed register bits are preserved.

Layered: memory image, OBSEL byte, scene brightness byte, Int32 layer count
(0–1024), then ordered layer records. Rendering begins with opaque CGRAM backdrop.

Both scene shapes apply scene brightness before the header's outer fade passes.
The packet owns every array. Consumers cannot depend on intermediate packets having
been rendered or on producer arrays retaining their contents.

## Memory image

65536 VRAM bytes, 256 UInt16 native CGRAM words, 544 OAM bytes (512 low then 32 high),
and Int32 modeled sprite count (0–128). The count is deliberate host-model metadata:
existing OBJ raster kernels stop there. Physical OAM bytes alone would not preserve
the current model's treatment of unused large sprites.

## Layer records

Every record begins with a byte discriminator:

| Kind | Fields, in order |
| --- | --- |
| 1: whole OBJ | None |
| 2: OBJ priority | Byte priority, 0–3 |
| 3: 4-bpp BG | UInt16 tilemap word, character word, horizontal scroll, vertical scroll; Int32 tilemap width and height (32 or 64); byte priority selector |
| 4: 2-bpp BG | UInt16 tilemap word, character word; Int32 visible row count (28); boolean priority |
| 5: Mode 7 (version 2+) | Eight Int16 values A/B/C/D, center X/Y, horizontal/vertical offset; boolean character-zero-outside-map |
| 6: fixed-color add (version 3+) | Three bytes red/green/blue, each 0–31 |
| 7: 2-bpp viewport (version 4+) | UInt16 tilemap word, character word, vertical scroll; boolean transparent color zero; byte priority selector |
| 8: ordinary gameplay base (version 5+) | Register block and optional HDMA tables described below |
| 9: scanline color add (version 6+) | Exactly 224 rows, each five bytes: left, right, red, green, blue |
| 10: message overlay (version 7+) | Byte row count (3–6), byte radius (0–24), then row count times 32 UInt16 tile words |
| 11: BG color math (version 8+) | UInt16 tilemap/character words; Int32 map height in tiles and first scanline; byte equation; 224 pairs of UInt16 X/Y scroll registers |
| 12: Mode-7 gameplay bands (version 9+) | Mode-7 registers, HUD registers, optional Mode-1 floor band, as below |
| 13: BG subscreen add (version 10+) | UInt16 tilemap/character words; boolean coverage-present; optional kind-3 BG4 descriptor |
| 14: windowed scene (version 10+) | Int32 left/top/right/bottom; memory image; OBSEL and brightness bytes; Int32 layer count; child layers |

4-bpp priority selector: 0 unfiltered, 1 low, 2 high. Whole OBJ uses the winning
OAM pixel irrespective of its BG-relative priority. OBJ-priority insertion uses
the same winner, not an independently filtered sprite list.

Version two adds only layer kind 5, allowing a Mode 7 plane between OBJ priority
insertions. All version-one fields retain their meanings. The reader rejects
kind 5 under a version-one header; old supported packets remain readable and are
upgraded to the current version when serialized again.

Version three adds whole-screen saturating fixed-color addition, inserted before
scene brightness for the Ceres rear view. Its software-reference semantics reduce
each byte component with `(component * 31 + 127) / 255`, add the five-bit operand,
clamp at 31, then expand via `(value << 3) | (value >> 2)` and set alpha to 255.
This is not a replacement for masked main/subscreen color math. Old-version headers
cannot contain this operation. Prior supported packets remain readable unchanged.

Version four adds a 256-by-224 viewport into a 32-by-32 tile 2-bpp map. Source Y is
`(screenY + verticalScroll) & 255`; X is unscrolled. Priority uses the same
all/low/high selector as 4-bpp. Color-zero opacity is explicit. This supports both
the opaque first narration card and the transparent, eight-pixel-scrolled intro
text. Kind 7 is rejected under older headers; existing kind 4 retains its semantics.

Version five adds a fused ordinary gameplay base, which must be the first operation.
Its register block is, in order: UInt16 BG1 X/Y and BG2 X/Y; Int32 BG2 width/height
in tiles (64x32, 32x64 or 64x64); UInt16 BG2 tilemap word, BG1 character word, BG2
character word, HUD character word; byte TM layer mask. Then horizontal and vertical
HDMA tables each have a boolean presence byte followed, when present, by exactly
192 UInt16 register values. They correspond to physical lines 32 through 223;
source Y includes the physical screen line, not a restarted gameplay coordinate.

The base replaces the backdrop, renders the four-row opaque BG3 HUD using its
cartridge tilemap address, and composes BG1/BG2/OBJ below the HUD. BG1 uses the
gameplay 64x32 tilemap. The back-to-front ranks are OBJ0, OBJ1, BG2-low, BG1-low,
OBJ2, BG2-high, BG1-high, OBJ3. BG1/BG2/OBJ enable bits affect the gameplay region
only. This operation does not include subsequent liquid/window/message effects.
No previously supported layer encoding changes; older headers reject kind 8.

Version six adds inclusive color windows for each physical scanline. Left greater
than right is an empty window. Inside the window, add each expanded-byte operand
to the corresponding source component and clamp at 255, preserving source alpha.
There is no implicit HUD exclusion; the producer supplies empty windows for hidden
lines. This preserves the existing suit/haze/Power Bomb reference arithmetic and
must not be replaced with kind 6's five-bit reduction/addition arithmetic.
The fixed row count bounds decoding; truncated rows and old headers are rejected.

Version seven adds the bank-$85 message overlay: a 32-tile-wide, 2-bpp image centered
at physical Y=124, clipped to `[124-radius, 124+radius)`. Character base is word
$4000; tile flips, palette selection and transparent index zero follow the existing
message compositor. Palette indices 25/26 use the temporary BGR555 words $0BB1/$001F
without mutating the packet's CGRAM. This operation is ordered after room effects
and before a suit window by the gameplay producer. Invalid rows/radius, truncated
tile words and use under an old-version header are rejected.

Version eight adds a 32-tile-wide 2-bpp color-math plane. Height is 32 or 64 tiles
and map rows are contiguous. The first scanline is 0–224 (224 is empty). Coordinates
use `(X + screenX) & 255` and `(Y + screenY) & (height*8-1)`. Tilemap and character
addresses wrap to 15-bit VRAM words; flips/palette selection use native tile bits.
Color index zero does nothing. Equation 0 adds expanded palette bytes and clamps
at 255; equation 1 subtracts them and clamps at zero, preserving source alpha.
The producer has resolved liquid visibility/waves and supplies no room identity.
Unknown equations, invalid geometry/scanline, truncated registers and older headers
are rejected. This preserves existing byte-domain FX math, not general SNES math.

The original kind-4 2-bpp operation is an unscrolled full visible plane. Partial-row or
scrolled HUD operations need their own defined semantics, not oversized allocation
or silent clipping by the reader.

## Mode-7 gameplay bands (version 9)

Kind 12 is a base operation and must be first. It contains eight Int16 values
(A/B/C/D, center X/Y, horizontal/vertical offset), a boolean character-zero-outside-map,
UInt16 HUD tilemap and character words, Int32 HUD scanline count (0–224, multiple of 8),
and a boolean floor-present. When present, the floor is Int32 first scanline,
UInt16 tilemap/character/horizontal-scroll/vertical-scroll, and Int32 map width/height
(each 32 or 64). The floor begins at or below the HUD end and before scanline 224.

The HUD is opaque 2-bpp without OBJ. Mode 7 uses physical screen coordinates between
the HUD and floor, with winning OBJ pixels above it. The optional floor starts from
backdrop, not Mode 7, and inserts OBJ0, OBJ1, BG2-low, OBJ2, BG2-high, OBJ3 in that
order. OAM precedence is resolved once before these insertions. Older versions reject
this operation; no existing operation encoding changes.

## Compatibility and error policy

Version ten adds unscrolled 2-bpp subscreen addition in five-bit color space:
`sum=min(31,(main>>3)+(sub>>3))`, expanded by `(sum<<3)|(sum>>2)`.
Transparent sub pixels do not contribute. When coverage is present, only nontransparent
pixels of its 4-bpp plane permit addition. The coverage descriptor must be kind 3;
it cannot contain another operation.

Windowed scenes replace pixels in a half-open rectangle, including the child's
backdrop; they do not alpha-blend or stretch. Bounds must satisfy
`0 <= left <= right <= 256` and `0 <= top <= bottom <= 224`.
Child scenes have independent owned PPU memory and scene brightness. They cannot
contain windowed-scene operations; both construction and decoding enforce that
depth limit. The parent brightness applies after insertion. Empty windows are valid.
The same 4-MiB packet limit applies on serialization and deserialization.

- Never renumber discriminants or change field meaning within version one.
- Reject unknown versions/kinds, invalid booleans/selectors, out-of-range identity,
  counts, geometry and brightness, truncation, and trailing bytes.
- Input is bounded at 4 MiB before allocation; operation counts are separately bounded.
- Do not repair malformed fixtures or load assembly-specific debugger state as fallback.
- Round-trip tests cover solid frames and all extracted real-data title/menu scenes,
  requiring identical serialized bytes and exact rendered pixels.

The comparison CLI, checked-in representative packet corpus and GPU consumer remain
pending in #321. Having a codec alone does not satisfy those deliverables.
