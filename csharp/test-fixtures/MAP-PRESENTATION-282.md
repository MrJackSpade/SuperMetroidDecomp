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

## Already-displayed gameplay HUD rebinding

A serialized runtime with no pending HUD upload reproduced stale characters after
binding an edited catalog. Binding now requests a nonserialized, one-shot refresh
at the next accepted NMI, before ordinary queued writes. Lag NMIs and binding itself
do not mutate VRAM or republish the retained display. Only the 4 KiB character range
is refreshed: replaying the initial 8 KiB upload would clear live BG2 room tilemaps.

The regression failed before the change and now verifies edited character bytes,
exact edited HUD pixels, lag gating, and every byte outside that range remaining
unchanged, including a sentinel-filled former clearing range. Pause owns a separate
PPU image; its existing bind updates that image independently and teardown discards
it rather than restoring an old VRAM backup over the gameplay refresh. No immediate
zero-step replacement of an immutable retained render packet is claimed.

This completes the already-displayed runtime HUD refresh gap above. Historical
full-session fixtures, palette/placement resources and installer/device checks
remain separate work; this is not completion of #282.

## Editable map highlight palette cycle

Catalog version 5 adds `map-highlight-cycle.json`. Copy it from `game/maps` to
`overrides/maps` to edit the pause/file-select highlight animation. The document
has `version: 1` and a `frames` array; each frame contains `durationTicks` and
exactly sixteen `colors`, each an object with `red`, `green`, `blue` components
from 0 to 31. Durations are 1-254 menu ticks, with 1-255 frames supported. These
limits reject native sentinel encodings rather than exposing a bytecode editor.
The native stock cycle has fourteen frames. JSON colors, not PNG preview colors,
control this animation. Other static map/menu palettes are not yet editable.

The importer decodes `$82:C10C` timing and `$82:A987` colors. Installed pause and
file-select animation reads the immutable catalog instead of those ROM ranges.
The palette destination, sprite binding, increment-before-read sequence and sound
queue routing stay compiled. Changing cycle length/durations changes cosmetic
timing and when the existing loop sound is requested, not the sound ID or engine
behavior. Binding after state restore preserves the pending timer; a shortened
cycle wraps to zero at the next frame boundary. Content remains nonserialized.

The file-select visible-edit test initially failed: arrows were bound to the
station marker's static OBJ palette seven, so the animated palette never colored
them. `DrawPauseScreenSpriteAnim` in the pinned C and ASM uses
`SpritePalette_IndexValues[3]` (`$82:C100`, value `$0600`). File-select arrows now
use the compiled palette-three identity, independently of station markers. Tests
compare every generated arrow OAM palette against that cartridge word and verify
edited colors visibly reach the actual file-select menu.

Verification covers 600 ticks of exact all-CGRAM parity and native loop-request
timing; exact pause pixels across a complete stock cycle; visible edits after
pause snapshot restore; file-select visible edits with unchanged navigation phase;
stock rebind timing/pixels; shorter cycles and changed durations; reload and stock
re-extraction preservation; malformed schemas, invalid durations/RGB and corrupt
overrides. Existing full file-select navigation tests now forbid the palette ROM
ranges too. This is not native emulator framebuffer comparison, device validation
or completion of static palette and visual placement extraction.

## Static pause, file-select and world-selection palettes

Catalog version 6 adds `map-palettes.json`. Its version-1 document contains `pause`
and `fileSelect` color arrays plus a `world` object with exactly the six Zebes area
names (`Crateria`, `Brinstar`, `Norfair`, `WreckedShip`, `Maridia`, `Tourian`). Each
palette contains 256 RGB5 objects using the same `red`/`green`/`blue` component
schema as the highlight cycle. Copy the file into `overrides/maps`, edit colors,
and restart. The first eight groups of sixteen color slots belong to background
art; the remaining eight groups belong to sprites. Tile JSON palette references
continue to select their existing color groups. No ROM addresses or copy programs
are exposed in the replacement document. World keys select complete visual themes
for each active area, not progression or navigation behavior.

The importer resolves native active/inactive world-map color-copy programs into
six complete palettes. Runtime world selection picks the named result instead of
reading/interpreting those ROM records. Installed pause, shared file-map graphics
and file-map entry also use the extracted base colors. Bound-state replacement
preserves the selected area, scroll and menu phase. Ongoing entry fades receive new
targets without restarting their counters or replacing interpolated current colors.
Pause and room-map rebind deliberately retain colors owned by the highlight cycle;
pause also retains its two reserve-arrow animation colors. Edit those through their
animation assets rather than expecting static colors to override animation owners.
The reserve-arrow cycle is not extracted in this change.

Tests compare every pause/file-select base word and all six world-selection CGRAM
images and rendered backgrounds against the cartridge-backed implementation. Real
palette overrides visibly change pause, room-map and world-map pixels; stock
rebinding restores them, including a serialized pause. An edited palette rebound
mid-entry reaches its new fade target on the unchanged phase schedule. Existing
full file-select navigation/state tests forbid the static palette ROM ranges, in
addition to map layouts, artwork and highlight-cycle reads. Invalid versions,
palette sizes, colors, missing world selections and corrupt overrides fail; stock
re-extraction preserves replacement bytes and content identity.

These are the map-menu base palettes, not all gameplay palettes. The live HUD gets
colors through room/palette systems whose extraction remains in the shared palette
work. Other remaining #282 work includes visual placement resources, historical
full-session compatibility, complete installer replacement and Android validation.

## Full installer transaction verification

Run the opt-in `SuperMetroid.Verification --map-installation <ROM path>` check to
exercise `GameAssetInstaller` itself, including real audio/map extraction and
publication, rather than calling only the map exporter. It creates an isolated,
ignored `csharp/test-temp/map-installation-<guid>` root and never touches the real
player installation. The original input ROM is verified unchanged by SHA-256.

The check installs from scratch, edits a static palette override, marks the map
manifest as the previous format, cancels a complete extraction immediately before
publication, and verifies the old installed manifest and all player sentinels
remain unchanged. It then performs a successful upgrade, verifies current catalog
version and edited content identity, and checks a complete-installation restart
does not extract again. A corrupt override is retained and rejected by the content
loader rather than silently replaced by the installer.

Byte-exact preservation covers the override, `SuperMetroid.ini`,
`SuperMetroid.save.json`, legacy `SuperMetroid.srm`, a named slot in `debug-states`
and an `input-recordings` file. Player files contain synthetic sentinel payloads:
this proves transaction isolation, not save-format deserialization compatibility.
The cancelled transaction also leaves no staging directories. This completed the
previously pending normal installer-upgrade/cancellation gate on Windows. Process
termination recovery, historical full-session compatibility and Android device
validation remain separate checks.
# World-map label coordinates (catalog version 7)

The importer now emits `world-map-labels.json`. Copy that stock file to
`overrides/maps/world-map-labels.json` and edit the named `areas` entries. Each
of Crateria, Brinstar, Norfair, WreckedShip, Maridia and Tourian has an `x` and
`y` pixel anchor; retain all six entries and `version: 1`. Coordinates must be
inside X=1..255, Y=1..223. For example, changing only Crateria's `x` moves its
world-map label and the origin of its opening window. The title is unchanged.

This is cosmetic layout, not navigation data. Area order, used-station masks,
selected save destination, window velocities/duration and exploration remain
application-owned. Large edits can change how the fixed-speed window covers the
screen during its transition; no automatic retiming is implied. Active windows
retain their current geometry during content rebinding rather than restarting.
New windows use current content. Room-map station/boss/elevator placements remain
separate outstanding work under #282.

The shared catalog verifies stock provenance, chooses strict overrides and includes
the chosen bytes in its content identity. Catalog version 7 requires this resource;
older installations are reimported through the existing installer path. Overrides
stay outside stock and survive replacement. Bad layouts fail with a path/context
message rather than reverting to stock. Nonserialized host bindings are reapplied
to menu graphics and navigation after restoration.

Verification (`--map-presentation`) checks all six stock coordinate pairs and
rendered world-map selections with coordinate-table ROM reads blocked, every stock
window edge and completion tick, visible edited label pixels, unchanged availability
with no used stations, actual navigation's edited opening origin, stock rebind,
override identity/preservation and invalid schema/coordinates. Full core suite and
Windows Release build are additional gates. Logs stay in test-temp as
`282-labels.log`, `282-labels-suite.log` and `282-labels-windows.log`.

This implements one placement resource, not full #282 or ROM-free menus: other
artwork, station eligibility and transition definition reads remain in their
respective extraction/compiled-data work items. Android device validation and
historical full-session coverage are not claimed by these tests.

## Compiled window motion definitions

World-map window velocities and timers now come from the dedicated
`FileSelectMapWindowMotions` catalog, not runtime reads of `$81:AA34..AA9F`.
The six signed 16.16 motion records and pre-underflow timers are application
definitions; editable label anchors remain presentation content. No serialized
window fields, fixed-point arithmetic, clamps or return-window behavior changed.

Verification independently reads every cartridge motion word and compares it
with the compiled records. Installed-content tests forbid reads of both the
coordinate and motion tables and compare each edge and completion tick with a
cartridge-fed control. Synthetic fractional-clamp tests explicitly inject their
constructed motion instead of depending on a production ROM lookup. Invalid
area IDs are rejected without truncating them to a byte.

Focused map-presentation checks, the full core verification suite and Windows
Release build pass (`282-window-motion.log`, `282-window-motion-suite.log`,
`282-window-motion-windows.log` in ignored test-temp). This removes only the
transition-definition reads identified above; the broader #282 scope remains open.

## Station drawing positions (catalog version 8)

The fourteen missile-refill, energy-refill and map-station markers now import as
`map-station-labels.json`. To edit their positions, copy the complete stock file
to `overrides/maps/map-station-labels.json`. Keep `version: 1` and all marker IDs;
each marker has `x` (0..511) and `y` (0..255) in area-map pixels. For example,
set `markers["Brinstar.Missile.0"].x` to 80 to move that icon right. These are
map coordinates, not screen coordinates; existing scrolling still applies.

The original records mixed drawing positions with discovery checks. Compiled
`MapStationDiscoveryRules` now own the original explored cells and marker order.
Moving a label onto another explored cell does not discover the station, move
the station in the world, change its refill behavior, or change save data.
Icon artwork/palette bindings remain unchanged. This file covers the normal
file-select room-map station icons, not boss, elevator, gunship or selected
save-point placements, which remain outstanding placement work.

Installed graphics use the selected layout without reading those ROM pointer or
coordinate lists. An unbound cartridge-fed path remains the diagnostic parity
control. Host content rebinding updates the nonserialized layout reference on
the existing icon owner. Invalid overrides fail with file/context information;
catalog v8 requires verified stock station content even when an override exists.

Tests compare all 55 per-area discovery subsets with cartridge-fed OAM and check
full-menu stock pixels, visible position edits, rebind restoration, unchanged
exploration bytes, missing/corrupt stock, invalid overrides and preservation
through stock replacement. The full installer fixture additionally preserves a
station edit through cancelled upgrade, successful replacement and no-op restart,
alongside synthetic INI/save/state/recording files. No player files are used.
Focused and full core verification plus Windows Release build pass; local logs
are `282-stations.log`, `282-stations-suite.log`, `282-stations-windows.log` and
`282-stations-installation.log`. Android device validation is not claimed.

## Boss, elevator and gunship positions (catalog version 9)

`map-landmarks.json` adds 23 named drawing anchors: five boss markers (including
the Ceres list entry), seventeen elevator destination labels, and the Crateria
gunship. Copy the complete stock file to `overrides/maps/map-landmarks.json`.
Retain `version: 1` and every marker ID, editing only `x` (0..511) and `y`
(0..255) in area-map pixels. For example, `Boss.Phantoon` at X=160 moves that
marker eight pixels right. This moves both its living icon and defeated overlay.

`MapLandmarkDefinitions` owns slot order, unused boss entries and elevator
destination identities. Boss-state consumption, map-download eligibility,
palette/art bindings and the gunship/arrows/elevator drawing order remain code.
Moving a boss does not defeat it or reveal an undiscovered living boss. Moving
an elevator label does not move the elevator or alter its destination. Ceres
has no elevator map list; its boss entry is supported separately.

The importer checks record counts, unused slots and elevator spritemap identities
against those definitions. Installed pause/file-select drawing no longer reads
boss/elevator coordinate or identity tables. Direct icon tests also block the
gunship coordinate read. Full file-select rendering still reads selected-save
coordinates, which share the gunship source; selected-save placement is pending.
The catalog validates stock hashes and strict overrides, with host rebinds using
current nonserialized content after restoring a debugger graph.

Verification compares 3,584 combinations (seven areas, every raw saved boss byte,
with/without downloaded map) to cartridge-fed OAM, including overlays and native
unused-slot behavior. Pixel assertions cover stock and edited file-select output,
pause boss output, and restoration/rebinding. Tests also check unchanged boss/map
flags, hidden landmarks staying hidden, invalid content, missing/corrupt stock,
and preserved overrides. Full installation tests preserve an edited Phantoon
anchor through cancellation, replacement and restart alongside synthetic player
files. Logs: `282-landmarks.log`, `282-landmarks-suite.log`,
`282-landmarks-windows.log`, `282-landmarks-installation.log` in ignored test-temp.
No Android device or historical full-session validation is claimed. #282 remains
open for selected-save placement and its other outstanding integration work.

## Selected-save positions and area-label eligibility (catalog version 10)

The 34 usable selected-save/elevator marker anchors now import as
`map-save-markers.json`. Copy the complete file into `overrides/maps/` and edit
the `x`/`y` values of named entries such as `Maridia.Save.0` (X=0..511,
Y=0..255, area-map pixels). Retain `version: 1` and all known IDs. These are
the animated selected-save indicator positions, not actual station locations.
Unused native indices are not editable entries and remain invalid selections.

The original coordinate table also determines whether a world-map area label
has a valid used save station. `MapSaveMarkerDefinitions` now owns the native
index validity and that eligibility check. Neither authored drawing coordinates
nor replacement map tiles change the used-station masks or selected load index.
Initial map scrolling still derives from the load station and room metadata,
not the edited marker anchor; those remaining metadata reads are separate work.

`FileSelectStationMarker` retains the same serialized coordinate and animation
fields. Content rebinding replaces only its coordinates, using area/index from
the existing menu owner, and does not reset frame/timer/backing phase. Ordinary
construction, return/reentry and debugger restore all use current content.
Catalog version 10 adds a new resource without changing older override schemas.

Verification compares every valid coordinate and 128 animation ticks per marker
against the cartridge-fed path, rejects all unused/out-of-range indices, and
checks all 393,216 area/used-mask combinations against native-table eligibility.
The complete existing menu entry/scroll/restore/return/reentry sequence runs with
save-coordinate reads forbidden and exact stock frame parity. An edited marker
changes room-map pixels; restoring and rebinding it preserves animation, scroll,
load selection and load-handoff timing. Invalid/missing/corrupt resources and
override preservation are covered. Full installer tests preserve the new override
alongside other edits and synthetic player files across cancellation/upgrade/restart.
Focused/full core verification and Windows Release build pass. Local logs:
`282-save-markers.log`, `282-save-markers-suite.log`,
`282-save-markers-windows.log`, `282-save-markers-installation.log`.

This completes the selected-save placement portion, not all #282 requirements.
Remaining room/load metadata and other scoped ROM reads still need auditing;
historical full-session and Android device validation are not claimed here.

## Compiled scroll controls and remaining dependency audit

The controller masks/direction order embedded in the four `$81:AF32` arrow
records now belong to `MapScrollControls`. `FileSelectMapScroll` copies those
compiled bindings into its existing serialized field, leaving step timing,
boundary inequalities, precedence and sound requests unchanged. The visual
position/animation portions of the records still require presentation extraction.

Verification independently reads the four cartridge masks and direction IDs,
compares sixteen combinations through sustained movement, boundary stopping and
release, checks unrelated controller bits, and runs the complete existing menu
sequence with the control-word ROM reads forbidden. The injected cartridge
control and compiled path must agree on every scroll position, direction and
sound tick. Focused/full core suite and Windows Release build pass; logs are
`282-scroll-controls.log`, `282-scroll-controls-suite.log` and
`282-scroll-controls-windows.log` in ignored test-temp.

The remaining map-runtime dependencies are not all the same kind:

- `FileSelectMapAnimations`: arrow positions, animation IDs/frames and sprite
  composition remain presentation dependencies; palette-cycle content is bound.
- `FileSelectAreaMapGraphics`: world foreground/background tilemaps and menu
  sprite composition remain presentation reads. Display ordering is a compiled
  definition candidate. Its palette-ROM branch is only the unbound diagnostic
  path; installed palettes, label anchors and eligibility are already supplied.
- `FileSelectRoomMapGraphics`: fixed frame/footer and area-name tile composition
  remain presentation reads, distinct from the extracted room-map cell grid.
- `FileSelectMapMenuState`: load-station/room metadata still computes the initial
  player-map scroll anchor. That is application behavior, not an editable marker
  coordinate. Its old closure remains for historical graph/diagnostic use.
- `FileSelectMapIcons` and `FileSelectStationMarker`: installed position and
  eligibility paths are bound; their common sprite-composition tables remain.
- Gameplay HUD/pause shared dependencies and final combined guards still need
  the broader #282/#544/#549 integration audit. Passing individual blocked-range
  tests is not evidence that the entire menu or game is ROM-free.

This audit directs the remaining work; it does not defer it or close #282.
