# Map presentation boundary (#282 / #530)

This is an implementation slice, not completed installation integration or removal
of the runtime ROM requirement.

`IAreaMapView` separates displayed cells from discovery, station visibility and
the native slope-corner exploration rule. `AreaMapPresentationAsset` reads and
writes version-one JSON using caller-owned streams. It copies rules from the
application-supplied map definition; no gameplay rules or ROM addresses are
accepted in the editable document. The current stock source remains
`AreaMapCartridgeData` until compiled map definitions are integrated.

The JSON has `version`, `area` (case-sensitive area name), and `cells`: exactly
2048 cells in row-major order, 64 columns by 32 rows. Each cell requires:

```
{ "tileColumn": 17, "tileRow": 2, "palette": 3,
  "priority": true, "flipX": true, "flipY": false }
```

Tile coordinates address a logical 32-by-32 atlas of 8-pixel map characters;
palette is 0 through 7. PNG atlas import/export is not implemented in this slice.
The compiled encoder maps these presentation attributes to the existing renderer.
Unknown fields, missing required fields, unsupported versions, wrong area,
incomplete layouts and invalid coordinates/palettes are rejected with errors.
The loader never writes files, repairs stock assets or erases an override.

`AreaMapTilemapBuilder` (shared pause/file-select projection) accepts this view.
`HudState.UpdateMinimap` accepts an explicit presentation view and performs no map
cartridge reads when one is supplied. Its default legacy path remains unchanged.
Changing a slope's artwork cannot remove its corner exploration rule; painting
an undiscoverable cell cannot create new exploration/reveal state.

## Verification

Run Verification with `--map-presentation` (also included in the full suite).
Tests load and reload JSON edits, check exact live minimap words with a bus that
throws on every read/write, exercise the shared pause/file-select projection,
and verify independent exploration rules and format errors. With the local ROM
available, all 14,336 stock cell words round-trip exactly through JSON, including
palette, priority and flips. This is tilemap output comparison, not PNG screenshot
parity or full interactive pause/file-select testing.

Remaining: installer/exporter manifest integration; stock/override directories;
compiled exploration definitions; runtime injection across normal desktop and
Android startup; map art/palettes/room-placement integration; restart/update/save
and rendered replacement tests. No claim that ordinary game startup consumes
user map files yet. Keep #282 open without awaiting-player-validation.

## Stock installation and override catalog

The shared desktop/Android `GameAssetInstaller` now extracts the seven semantic
map JSON files to staged `game/maps` and checks their manifest hashes before
publishing content. Existing installations without maps are incomplete and use
the installer's existing stock rebuild transaction. Source ROMs and player data
are not moved. Overrides belong in `<installation-root>/overrides/maps`, outside
both `game` and its replacement/rollback directories.

`AreaMapPresentationCatalog.Load` accepts the stock directory, optional override
directory and application rule provider. Filenames are fixed lowercase area
names (`crateria.json`, `wreckedship.json`, etc.), not manifest-supplied paths.
It validates stock hashes, then loads each matching override in preference to
stock. A malformed override throws with area/path context and is never erased or
silently replaced. The complete selected catalog is immutable and receives a
content hash distinct from stock provenance; explicit reload observes edits.
Stock extraction exclusively creates files, refusing an existing target file.

Tests run actual stock extraction twice into separate fresh directories, verify
stable catalog identity, edit/reload an override, switch to re-extracted stock,
and assert override bytes and selected identity survive. Corrupt stock/override
and accidental importer overwrite fail loudly. These are real file operations
in ignored test-temp directories, not a production installation replacement test.
The full installer transaction, Android deployment and host game-constructor
injection still require integration coverage. The normal game is not yet wired
to consume the catalog; this commit advances installation/provider plumbing only.

## Live gameplay and pause binding

Installed desktop sessions now load maps via their `GameInstallation`; ordinary
Android sessions use the same loader. Explicit legacy diagnostic ROM-path sessions
retain their existing fixture path. The frontend passes its bound catalog to new
gameplay and attract runtimes and pause menus. Both gameplay minimap call sites
(including Ceres entry before player control) and pause layout/scroll-bound reads
consume the catalog instead of map ROM tables when installed content is bound.
Stock rule bootstrap still reads the ROM once; compiled rule migration is pending.

The host owns the catalog across debugger-state loads. Game/runtime/pause references
are explicitly nonserialized, and hosts rebind their current catalog after restore.
Pause rebinding refreshes map BG1 only while that plane contains the map, preserving
equipment VRAM, scroll coordinates and transition timing. Restart reloads disk edits;
an ordinary state load reuses the current session catalog rather than secretly
loading files mid-session.

The focused test initializes the actual Ceres runtime and pause menu with every
map pointer/data/reveal-mask ROM read configured to throw, then renders the pause
screen. It verifies the room-entry minimap and preserved pause scroll coordinates.
It also proves binding external content leaves serialized reset-state bytes
unchanged and restored state can be rebound to a newer content identity. This is
not yet a full paused-game state round trip or on-device Android launch test.

The earlier note that no normal gameplay consumed the catalog is superseded for
minimap/pause only. File-select room-map injection, compiled rules, PNG artwork,
other presentation assets and full installation/state compatibility coverage remain.

## File-select room-map presentation binding

The normal frontend now passes the installed catalog into the file-select room
map graphics. Host rebinding also refreshes that menu's BG1 from the current
catalog after debugger restoration. The saved exploration owner is reused from
the existing icon object: no serialized fields or delegate closure shapes were
changed, and no external catalog is captured in the menu graph. Area selection,
scroll position, windows and animation timers are not restarted by rebinding.
This menu uses normal saved exploration, not the diagnostic map-reveal override.

Focused coverage compares exact stock-rendered pixels, rejects map-ROM accesses
while constructing/rebinding room graphics, verifies all edited BG1 words and
round-trips the graphics object through the debugger serializer before rebinding
different content. This is graphics-state coverage, not a full file-select menu
recording or historical binary state migration test.

File-select scroll bounds still use the original ROM rule loader. PNG artwork,
compiled rule definitions and other map presentation resources remain unfinished;
this slice must not be described as complete runtime ROM independence.

## ROM-free installed catalog bootstrap

Catalog manifest version 2 adds `station-reveal.json`, a hash-checked stock-only
dictionary of area names to logical row-major reveal-cell indexes. These are
authored map-station masks, not SRAM offsets, callbacks or a supported gameplay
override surface. Presentation JSON remains version 1. Older installations fail
the existing completeness check and rebuild stock through normal staging; the
separate user override directory is unchanged.

`GameInstallation.LoadMaps()` no longer takes a ROM bus. Catalog loading decodes
the immutable stock layout, attaches bundled station masks, then applies editable
presentation separately. Compiled `AreaMapExplorationRules` determines stock blank
and slope semantics identically to the diagnostic cartridge loader. Exploration
updates, SRAM packing and progression remain application code. The content hash
now includes baseline stock/rule content as well as selected presentation bytes.

All seven areas / 14,336 cells are compared to the cartridge for exact tile words,
discovery, station reveals and corner reveals. The same properties are checked
after applying a visible artwork override. Tests reject absent/corrupt mask files,
duplicate/out-of-range cells, null masks and missing areas, including malformed
semantic data with an otherwise valid file hash. Extraction/stock replacement
continues to preserve override bytes. Full installer replacement and on-device
Android coverage are still pending.

This supersedes the earlier ROM-bootstrap note for installed catalog loading.
File-select scroll initialization, glyph/palette graphics and other presentation
resources still have ROM dependencies and remain work under this issue.

## Installed file-select scrolling and full-menu state round trip

The installed file-select menu now builds scroll limits from the bound catalog's
stock rule view. It no longer calls the cartridge map loader when initializing
scrolling or returning from area selection. The legacy constructor closure is
retained unchanged for debugger compatibility and explicit cartridge diagnostics;
installed sessions bypass it. A nonserialized catalog reference is rebound by the
existing host hook. No scrolling/animation fields are reset during rebind.

The regression constructs the actual menu with all area-map pointers, tile grids
and reveal-mask ROM reads forbidden. It compares every rendered frame and phase
against the cartridge-backed control through entry, confirmation, an asserted
horizontal scroll, full-menu serialization/restoration, cancellation and reentry.
Rebinding a catalog whose changes are in another area leaves serialized menu bytes
unchanged, demonstrating that the external catalog is not captured by the retained
delegate. This is a current-version full-menu state round trip, not an old-binary
fixture migration claim. Load-station/room metadata, artwork and palette ROM reads
remain outside this completed layout/reveal-table removal.

## Indexed PNG prerequisite

The old `WriteIndexedAsRgba` diagnostic helper expands pixels to RGBA and cannot
preserve source palette-index identity for editable atlases. `IndexedPng` adds a
stream-based indexed encoder/decoder without Windows/native imaging dependencies.
It writes 8-bit indexed files with PLTE and optional tRNS, and reads noninterlaced
1/2/4/8-bit indexed images with all five PNG filters and consecutive IDAT chunks.
It preserves RGB/alpha palette entries and indexes separately. Unsupported RGBA,
interlace and animation fail explicitly rather than guessing palette assignments.

Dimensions must match the caller's expected native atlas size (at most 2048 in
either dimension). Encoded data is capped at 16 MiB and inflated rows are exact
length. CRCs, critical chunk ordering, palette index bounds, truncation and extra
image data are checked. Ancillary metadata does not affect tile indexes.
Reference: https://www.w3.org/TR/png-3/ (filter and critical-chunk specifications).

Tests assert the actual indexed IHDR type, a known IEND CRC, index/palette/alpha
round trip, independently hand-calculated filter rows, odd-width packed depths,
split IDAT, and malformed/unsupported resources. This commit provides the codec;
the normal map importer/catalog has not yet been wired to PNG atlases. No claim
that editing PNG artwork already affects the live game is made here.

## Live pause/file-select PNG atlas

Catalog manifest version 3 adds hash-checked `map-tiles.png`. The importer decodes
the shared $B6:8000-$B6:9FFF characters into a native 256x64 indexed atlas: 32 columns
by eight rows of 8x8 tiles. PNG indexes must stay in 0-15; their RGB preview palette
does not set gameplay colors. CGRAM/palette-reference extraction is still pending.
The current preview is grayscale to expose index identity without claiming one
palette for characters reused under multiple runtime palettes. Edit pixel indexes,
not preview RGB values. PNG alpha likewise does not change the SNES transparent
index rule. Color customization is not yet supported by this atlas.

Copy `game/maps/map-tiles.png` into `overrides/maps/map-tiles.png` and edit it as an
indexed image without changing its dimensions. Restart reloads the selected PNG;
state loading rebinds the current session's catalog. Wrong dimensions, corrupt PNG
or out-of-range indexes throw rather than silently selecting stock. Stock repair
continues to preserve the separate override. Layout JSON now rejects atlas rows
outside the eight installed rows rather than referencing unprovided characters.

Pause and file-select graphics now upload the immutable compiled atlas at their
respective native VRAM destinations instead of reading that cartridge range. The
world-map menu's shared initial PPU load is also bound, so entry does not sneak in
the same ROM read. Pause retains the second half of its character sheet from its
existing source; gameplay minimap uses a separate 2-bpp sheet and remains pending.
Map/layout definitions still govern exploration independently of PNG edits.

Tests compare all 8,192 stock planar bytes and exact rendered pause/file-select
stock pixels, with the tile-ROM range blocked. An actual PNG override changes every
decoded pixel as authored and changes pixels inside both map holders. Rebinding
stock after a graphics-state restore produces the original pixels exactly. Tests
also cover replacement preservation, invalid PNG/dimensions/color indexes and
missing stock artwork despite a valid override. These are managed pipeline tests,
not Android on-device or full installation transaction validation.

## Gameplay HUD/minimap PNG and queued asset transfers

Catalog version 4 adds `hud-tiles.png`: the separate native 256x64, four-index
2-bpp HUD/minimap sheet. PNG indexes are compiled with the same shared planar
encoder as the four-bit map sheet. Its 4 KiB of characters are followed by the
native 4 KiB zero-clear payload when uploading; all 8 KiB are compared to retail.
The preview palette still does not configure CGRAM. Existing color/alpha semantics
remain application-owned until the separate palette resources are integrated.

The standard gameplay upload now queues `VramAssetId.StandardHudTiles` when an
installed catalog is bound. The queue keeps its native order, seven-byte logical
record budget and accepted-NMI boundary, but resolves compiled content through
`IVramAssetProvider` only when drained. It never fabricates a ROM address space or
stores a captured artwork blob in the debugger graph. Missing providers, wrong
payload lengths and invalid IDs fail explicitly. The bus-backed path remains for
explicit cartridge diagnostics and non-artwork transfers.

An explicit debugger-field migration retains the original three fields of older
bus entries with AssetId=None. Binding installed maps upgrades an exact pending
legacy HUD source/count match in place, preserving its destination/order. A new
pending asset entry is resolved against the catalog rebound after state load.
Pause's copy of the same standard sheet also uses the installed resource.

Tests cover byte-port parity for odd/even sizes, linear/column increments and VRAM
wrap, interleaved bus/asset ordering, queue capacity and pending queue round trips.
The actual runtime test blocks the complete original HUD ROM range, checks that a
lag NMI writes nothing, and compares all VRAM bytes after an accepted NMI. A PNG edit
changes rendered minimap pixels after restoring a pending runtime state and binding
the edited catalog. The pixel test uses an explicitly constructed distinct-color
BG3 palette: the no-room fixture otherwise renders black and cannot demonstrate an
artwork change. It is not a retail room palette parity claim. Older field selection
is tested separately, not presented as loading a historical binary fixture.

Remaining: editable palette resources, visual room placements and other in-scope
presentation reads; full installer/device checks; historical full-session state
compatibility and reloading changed artwork into already-displayed gameplay state.
