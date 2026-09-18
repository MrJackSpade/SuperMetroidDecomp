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
override failure are covered. Completion/save notices, narration, general labels
and credits remain open under #545.

## Editable completion and save notices (catalog version 31)

`gameplay-message-notices.json`, schema version 1, contains the map/energy/missile
completion notices and both owners of the shared save-confirmation prompt. Text is
stored as bounded UTF-8 regions with row, column, width, alignment and palette.
The stock regions are deliberately fixed-width and do not wrap; replacement text
must fit its declared width. Space, `A` through `Z`, hyphen, period and question
mark use the editable `hud-tiles.png` font.

The JSON also carries presentation-only tilemap templates and the two visual
YES/NO cursor rows. Cursor input, confirmation/cancellation, minimum display time,
gunship saving delay and save behavior remain compiled. The active cursor row is
reapplied after a content or debugger-state rebind, and neither opening nor moving
the installed save prompt reads its cartridge tilemap.

Verification compares 1,248 installed words covering all five message identities
and both save selections against the pinned cartridge with all installed bus reads
forbidden. UTF-8 editing, active rebind, stock restoration, strict glyph/layout
validation, deterministic extraction, override identity and corrupt override
failure are covered. All translated gameplay messages are now installed-content
driven; narration, general cinematic labels and credits remain open under #545.

## Editable opening narration (catalog version 32)

`intro-narration.json`, schema version 1, exposes the six English opening pages as
ordered UTF-8 lines with explicit tile rows. Copy it to
`overrides/maps/intro-narration.json` to replace narration without modifying the
ROM or recompiling C#. Lines begin at column one, do not wrap, and are limited to
29 cells. Rows must remain even-numbered rows 4 through 16. The safe glyph map
accepts space, `A` through `Z`, digits, period, comma, apostrophe and exclamation
mark; those characters compile to the existing opening-cinematic font tiles.

The initial marker, five-frame-per-character cadence, typewriter sound alternation,
caret tracking/blink, final 128-frame hold, input waits and scene transitions remain
compiled behavior. JSON cannot inject cinematic opcodes or change timing. The six
production page owners consume the installed catalog and no longer read their text
records or glyph tile words from bank `$8C`.

Verification parses the six retail streams mechanically and compares 3,990 frames
of tilemap output, caret placement and page-completion boundaries against the native
interpreter while forbidding installed narration reads. UTF-8 editing, deterministic
extraction, override identity, corrupt-resource failure and strict layout/glyph
validation are covered. General cinematic labels, credits, and editable pixels for
the opening font remain open under #545.

## Editable post-credit text (catalog version 33)

`ending-text.json`, schema version 1, exposes the producer-panel `PRODUCED BY`
label, the `1994 NINTENDO` copyright, both item-percentage lines, the alternate
language subtitle tile template, and `SEE YOU NEXT MISSION`. Text is uppercase,
bounded to the stock rectangles, and never wraps. The remaining stylized producer
logo stays in the result-panel template so its non-font tile composition is retained.

The initial 64-frame delays, four-frame typewriter cadence, percentage calculation,
alternate-language selection, 128-frame percentage hold, scroll request, and final
hold remain compiled behavior. Installed playback no longer interprets these text
streams or glyph payloads from bank `$8C`; debugger restoration requires explicit
rebinding to the current host catalog.

Verification compares both literal panels and every item-percentage/final-message
frame with the native interpreter while forbidding the installed bank-$8C reads.
UTF-8 edits, deterministic extraction, override identity, current-content state
restore, corrupt resources, dimensions, and glyph restrictions are covered. The
scrolling staff credits and other cinematic labels remain open under #545.

## Editable ending font (catalog version 34)

`ending-font.png` exposes the complete 160-tile, four-bit font sheet shared by the
scrolling credits, post-credit result screens, and post-shot subtitle. It is a
128x80 indexed PNG with sixteen palette indexes. The loader compiles it back to
the exact 5120-byte SNES planar transfer and uses that installed transfer at all
three native consumers, including the post-shot subtitle upload. The
ROM-compressed font is only the stock extraction source.

Verification compares every compiled stock byte with the decompressed cartridge,
proves a pixel edit changes installed VRAM data, and covers deterministic extraction,
override identity, and corrupt PNG failure. Other cinematic labels and
remaining cinematic font sheets remain open under #545.

## Editable scrolling staff credits (catalog version 35)

`ending-credits.json`, schema version 1, exposes all 67 retail staff-credit lines
as ordered UTF-8 text with explicit columns and palettes. Small headings support
spaces and `A` through `Z`; large names additionally support ampersand and period.
Each line must fit the fixed 32-column tilemap and never wraps. Stable line IDs and
ordering are required so an edit cannot change cinematic control flow.

Extraction mechanically interprets the retail `$8C:D91B` row program and validates
its compressed `$97:EEFF` source, timer loops, row offsets, blank gaps, glyph halves,
palettes, and terminating opcode. The resulting installed asset always compiles to
the native 520-row schedule: initial, inter-section, inter-line and trailing waits,
one-row headings, and two-row names remain application behavior rather than editable
instructions.

Playback preserves the cartridge's half-pixel scroll, one-row-per-16-frame cadence,
32-row circular tilemap, and post-final-row completion boundary. It consumes only the
installed compiled rows; it no longer reads compressed credit text or bank `$8C`
instructions at runtime. Debugger restoration explicitly rebinds the current catalog.

Verification compares all 520 stock rows with the native interpreter, then exercises
all 8,336 playback frames through completion. It covers text/column/palette edits,
deterministic extraction, override precedence and identity, stock restoration, corrupt
JSON, missing or reordered IDs, unsupported glyphs, overflow, and palette bounds.

## Editable opening font (catalog version 36)

`intro-font.png` exposes the complete 144-tile, two-bit font sheet used by the
English opening narration. It is a 128x72 indexed PNG with four palette indexes.
The loader compiles it back to the exact 2,304-byte SNES planar transfer, including
the blank glyph reused to clear the optional lower text margin.

The production opening owner receives this atlas with the installed catalog and no
longer decompresses the `$95:D089` font stream. Rebinding current host content updates
both live VRAM and the retained blank glyph, so debugger restoration cannot conceal a
new override. Focused legacy tests may still construct the intro without a catalog and
use the cartridge-backed fallback.

Verification compares every compiled stock byte with the decompressed cartridge,
forbids installed reads across the native compressed-font range, proves a pixel edit
changes live opening VRAM, and covers deterministic extraction, override identity and
corrupt PNG failure. The unused Japanese font-two staging stream is not presented as
editable content until the corresponding Japanese narration owner is translated.
