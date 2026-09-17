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
typewriter commands or change the delay. Both production owners use their
installed programs without reading the native bank-$A6 text streams. The Ceres
Ridley state owns a saveable compiled-program cursor and rebinds it to the active
catalog after debugger-state restore just like the Mother Brain/Zebes owner.

Verification compares completion, destination, delay state, visible-glyph count,
click requests and all 64 KiB of VRAM on every accepted call: 271 combined Ceres
and Zebes frames match the pinned cartridge. Installed calls use an address space
that throws on every access. Text editing, active-program rebinding, debugger
save/restore with mandatory host-content rebind, invalid glyphs and full catalog
integrity are also covered. The full Ceres encounter additionally erases both
native text streams after extraction and verifies warning DMA, click cadence and
the timed escape handoff through the real production path. Narration, the other
multi-row notices, the save prompt, general labels and credits remain open under #545.

## Editable large item panels (catalog version 30)

`gameplay-message-panels.json`, schema version 1, contains the seven six-row item
instruction boxes for missiles, Power Bombs, Grapple, X-Ray, Speed Booster and
Bombs. Each entry exposes its UTF-8 title, title column and palette. Its remaining
four-row template is presentation-only tilemap data for the instructional
diagram; the corresponding 2-bpp pixels remain editable in `hud-tiles.png`.

Titles accept space, `A` through `Z`, hyphen and period, and must fit between
columns 3 and 28 at the configured column. There is no automatic wrapping. The
panel template cannot contain callbacks or engine operations: message identity,
window timing, gameplay blocking and the Shoot/Run binding choice remain compiled.
The configured controller glyph is reapplied after every load or active-content
rebind, so editing a template cannot change which binding the instruction teaches.

Verification compares all 1,344 installed words for the seven panels against the
pinned cartridge using remapped Shoot and Run controls while forbidding every
installed bus access. UTF-8 editing, active rebind, stock restoration, strict
glyph validation, deterministic catalog extraction, override identity and corrupt
override failure are covered. The remaining non-title multi-row notices, save
prompt, narration, general labels and credits remain open under #545.
