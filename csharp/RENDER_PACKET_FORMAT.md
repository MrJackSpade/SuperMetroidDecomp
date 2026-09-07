# Portable display fixtures (.smframe), versions 1–2

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
| Version | UInt16; writer emits 2, reader supports 1 and 2 |
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

4-bpp priority selector: 0 unfiltered, 1 low, 2 high. Whole OBJ uses the winning
OAM pixel irrespective of its BG-relative priority. OBJ-priority insertion uses
the same winner, not an independently filtered sprite list.

Version two adds only layer kind 5, allowing a Mode 7 plane between OBJ priority
insertions. All version-one fields retain their meanings. The reader rejects
kind 5 under a version-one header; old supported packets remain readable and are
upgraded to version two when serialized again.

The current 2-bpp operation is an unscrolled full visible plane. Partial-row or
scrolled HUD operations need their own defined semantics, not oversized allocation
or silent clipping by the reader.

## Compatibility and error policy

- Never renumber discriminants or change field meaning within version one.
- Reject unknown versions/kinds, invalid booleans/selectors, out-of-range identity,
  counts, geometry and brightness, truncation, and trailing bytes.
- Input is bounded at 4 MiB before allocation; operation counts are separately bounded.
- Do not repair malformed fixtures or load assembly-specific debugger state as fallback.
- Round-trip tests cover solid frames and all extracted real-data title/menu scenes,
  requiring identical serialized bytes and exact rendered pixels.

The comparison CLI, checked-in representative packet corpus and GPU consumer remain
pending in #321. Having a codec alone does not satisfy those deliverables.
