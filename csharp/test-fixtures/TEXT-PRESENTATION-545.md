# Editable text presentation (#545)

## One-row gameplay-message titles (catalog version 28)

`gameplay-message-titles.json`, schema version 1, exposes the fifteen one-row
item/status messages as named UTF-8 strings. The file also records the shared
32-cell border and each title palette. Copy it from `game/maps` to
`overrides/maps/gameplay-message-titles.json` to replace a title without patching
the ROM or rebuilding C#.

The safe glyph mapping currently accepts space, `A` through `Z`, hyphen and
period. Titles are centered in the cartridge's nineteen-cell small-message
content window and may not exceed that width. The font pixels are the already
editable `hud-tiles.png`; this JSON maps text to those characters rather than
duplicating the same artwork in another PNG. Unsupported glyphs, missing or
extra message names, invalid palettes, wrong borders and corrupt JSON fail
explicitly.

Message identity, opening/closing cadence, mandatory display delay, controller
handling, configured-button substitution, save semantics and sounds remain
compiled behavior. The other multi-row messages retain their existing
cartridge-backed presentation until their text, diagrams and safe formatting
tokens are migrated separately.

Verification extracts every supported title from the pinned cartridge, compares
all 1,440 resulting tilemap words through the real message owner, and runs the
installed path against an address space that throws on every read. A UTF-8 edit
is then rebound into an already-active message without changing its coroutine
phase or window radius, and restoring stock content restores the exact tilemap.
The complete map/presentation catalog regression also passes with the version-28
resource and integrity manifest. This is partial #545 implementation, not
completion of narration, multi-row instructions, escape text, labels or credits.

## Editable Ceres/Zebes escape typewriter (catalog version 29)

`escape-typewriter.json`, schema version 1, contains named Ceres and Zebes
programs. Each program exposes UTF-8 uppercase warning text and its visual VRAM
line destinations. Copy the stock file to `overrides/maps/escape-typewriter.json`
to replace the warning or move its lines. The accepted font is `A` through `Z`,
space and exclamation mark; line length and VRAM bounds are validated.

The two-frame character cadence, one-glyph-per-call scheduling, exclamation
remap, every-second-visible-glyph click, escape-sequence blocking and encounter
phase transitions remain compiled. In particular, JSON cannot introduce native
typewriter commands or change the delay. The Mother Brain/Zebes production owner
uses the installed program without reading bank $A6. Ceres data is extracted and
exhaustively checked through the same compiled interpreter, but its older Ridley
state adapter remains cartridge-backed until that saved-state owner is migrated.

Verification compares completion, destination, delay state, visible-glyph count,
click requests and all 64 KiB of VRAM on every accepted call: 271 combined Ceres
and Zebes frames match the pinned cartridge. Installed calls use an address space
that throws on every access. Text editing, active-program rebinding, debugger
save/restore with mandatory host-content rebind, invalid glyphs and full catalog
integrity are also covered. Narration, Ceres production binding, multi-row item
instructions, general labels and credits remain open under #545.
