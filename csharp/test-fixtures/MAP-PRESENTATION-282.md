# Map presentation boundary (#282 / #530)

## Completion status

The area-map scope tracked by #282 is complete. Installed gameplay, pause,
file-select, and saved-map lifecycles consume the extracted map layouts, artwork,
palettes, labels, landmarks, station masks, saved-station anchors, sprites, and
compiled exploration/scroll rules. The cartridge-backed constructors remain only
for explicit parity diagnostics.

The focused `--map-presentation` verification covers every one of the 14,336
retail map cells, all 34 saved stations, all six displayed areas, direct and
captured rendering, debugger-state rebinding, editable overrides, replacement
installation, and strict missing/corrupt-resource failures. Its strongest
installed saved-map lifecycle forbids every address-space read and write rather
than maintaining a list of allowed cartridge ranges. Windows and Android consume
the same `GameInstallation.LoadMaps()` catalog path.

Broader audiovisual extraction and the final whole-runtime ROM-unavailable gate
remain tracked by #530/#549. Those shared epics do not leave an area-map runtime
dependency in this completed child scope. The sections below retain the
chronological implementation record; their per-slice "remaining" notes describe
what was outstanding at those points in history.

## Initial presentation boundary

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

## Extracted map-arrow anchors and phase timing

The installed file-select path now consumes `map-arrows.json` from catalog
version 11. Copy the stock file into `overrides/maps/map-arrows.json` to edit
the `Left`, `Right`, `Up`, and `Down` screen anchors and `durationTicks` arrays.
X accepts 0..255, Y 0..223 (the actual draw origin, after the native Y-minus-one
adjustment); each direction requires 1..255 phases of 1..254 menu ticks.
Controller bindings, scroll boundaries and input precedence remain compiled.

Extraction follows `$81:AF32` and the programs selected through `$82:C0E8`.
Cross-check: pinned native `DrawPauseScreenSpriteAnim` at `$82:A881` consumes
the duration and final shape-offset byte of each three-byte record, not its
middle byte. Retail arrows have one fixed shape each and every shape offset is
zero. The importer verifies this; the middle-byte sequence is deliberately not
invented into extra visual frames. The shared highlight palette supplies the
visible glow. Changing phase durations alone changes counters, not arrow art.
Sprite composition/OBJ characters still use the shared cartridge sprite path;
this slice does not claim editable arrow PNG artwork or a ROM-free entire menu.

The catalog validates stock hashes even when overrides exist and includes the
selected arrow bytes in its content identity. Existing override schemas are
unchanged. The nonserialized arrow presentation is rebound after debugger
restoration. Existing serialized arrow fields remain intact; position metadata
is replaced without resetting phase, visibility or remaining delay. A shorter
replacement cycle normalizes the phase modulo its length while retaining the
pending delay. Unbound diagnostic callers continue using the cartridge tables.

Verification:

- 600 ticks compare all four emitted arrow OAM streams and exact phase, timer
  and visibility values against the independent cartridge-fed path, including
  hidden-arrow pauses and repeated loop boundaries.
- Complete menu entry, scrolling, restore, return and reentry preserve exact
  stock pixels while a guard forbids arrow records, program pointers, base
  variants and phase-program ROM reads. Shared sprite-art reads remain allowed.
- A position override changes composed room-map pixels; a two-phase duration
  override obeys the native initial increment and wrap timing. Restore/rebind
  preserves counters and stock OAM; shortened cycles and diagnostic unbinding
  are covered separately.
- Invalid durations, missing directions, corrupt overrides and stock hash
  failures fail loudly. Isolated installer tests preserve the arrow override and
  synthetic player files through cancellation, upgrade and no-op restart.
- Focused map checks, full core verification and Windows Release build pass
  (zero build warnings/errors). Local logs: `282-arrows.log`,
  `282-arrows-suite.log`, `282-arrows-windows.log`,
  `282-arrows-installation.log` in ignored test-temp.

This supersedes the arrow-position/program item in the preceding audit, not
its shared sprite-composition dependency. #282 remains open for the remaining
map/menu resources, combined ROM-read audit and broader integration work.

## World-map layers, character sheets and resolved room frames

Catalog version 12 adds three editable resources:

| Resource | Authored content |
| --- | --- |
| `world-map-foreground.png` | Indexed 128x344 sheet, sixteen 8x8 tiles per row, pixel indexes 0..15 |
| `world-map-background.png` | Indexed 128x48 sheet, sixteen 8x8 tiles per row, pixel indexes 0..3 |
| `map-screens.json` | Thirteen named 32x32 tile grids: `World.Foreground`, `World.<area>` and `Room.<area>` for each of the six Zebes areas |

World foreground pages reference the foreground PNG, area backgrounds reference
the background PNG, and room frames reference the existing `map-tiles.png`.
Cells use `tileColumn`, `tileRow`, `palette`, `priority`, `flipX`, and `flipY`.
They do not contain ROM pointers, PPU commands, save indexes or controller rules.
Copy individual stock files into `overrides/maps/` and edit there. PNG palette
colors are previews; palette indexes are preserved and live colors still come
from `map-palettes.json` and the shared highlight cycle. Dimensions and page
identities are fixed; no arbitrary-resolution artwork is claimed.

Previously, world-map rendering read the foreground page and selected BG3 page
from ROM, and `MenuPpuState` read their character sheets directly. Room-map BG2
frames were assembled from native header, fill, footer and area-name sources.
The importer now resolves those presentation pieces once. Installed world
selection and room frame construction/rebinding use the immutable catalog.
Area selection, additive color math, scroll offsets, transition windows,
palette timing, load selection and map/save semantics remain application-owned.
Shared file-select/options callers without a catalog still use their original
diagnostic path; their broader presentation conversion belongs to #544.

Native evidence includes `$81:A725`'s frame prefix/fill and reverse footer
copy at `$81:A7CA` (words 1..160), and `$82:9628`'s twelve-word area label with
the native palette-bit mask. Named definitions now describe those counts and
offsets. The extractor reproduces the final frame without advertising native
copy instructions as editable content. The two PNG sheets recompile to the
exact original planar bytes, not an RGBA approximation or hidden ROM dump.

Verified:

- All six world selections and room frames match the complete native VRAM
  image; world additive output matches with backdrop math both enabled and
  disabled. Room frame-only transition pixels and four scrolling positions per
  area match. Reselection continues to use installed background pages.
- The full file-select entry/scroll/restore/return/reentry fixture retains exact
  stock pixels while reads of these character sheets, world layouts, frame
  prefix/footer and area-label source tables are forbidden.
- Each of thirteen JSON page edits independently changes its composed pixels.
  Each PNG edit independently changes the world image. Current-content rebinding
  replaces stale edited artwork; a restored complete menu retains phase, load
  handoff timing and exact stock fade pixels. Saved map progression is unchanged.
- Missing/corrupt stock resources, malformed overrides, absent/wrong-size pages,
  out-of-atlas cells, incorrect PNG dimensions and unsupported pixel indexes
  fail loudly without overwriting the override.
- Isolated full installer tests preserve all three new overrides alongside
  existing overrides and synthetic player files through cancellation, version
  replacement and restart. Full core verification and Windows Release build
  pass (zero build warnings/errors). Ignored logs: `282-screens.log`,
  `282-screens-suite.log`, `282-screens-windows.log`,
  `282-screens-installation.log`.

Still required: common sprite composition/OBJ artwork, the shared initial menu
BG2 template, remaining compiled display/load-station metadata, shared HUD/pause
dependencies and the combined integration/ROM-read audit. These tests do not
prove a ROM-free whole menu, historical full gameplay session, or Android device
validation. #282 remains open and is not awaiting player validation.

## Compiled saved-station scroll anchors and area display order

`FileSelectMapLoadAnchors` now supplies the 34 usable station/elevator map
anchors. These are the native player-map positions used to clip the initial
viewport, not the editable marker coordinates. Source derivation follows
`$80:C437`'s load-station arithmetic, `$81:AD17`'s room map origin and
`$82:9028`'s projection: room X plus Samus's high X byte, and room Y plus
Samus's high Y byte plus one, both scaled by eight. The six-area display
sequence from `$81:AAA0` is compiled separately in `FileSelectMapAreaOrder`.
No asset schema or installer version changes are needed for these mechanics.

Installed map construction and reentry no longer read load-station records or
room headers/state selectors to obtain the scroll anchor. They retain area
and station identity from the save and use the current catalog's map view.
Unused indices still fail explicitly. Editable save-marker placement does not
move the initial viewport or change the selected gameplay load location.

The legacy `createScroll` delegate shape remains solely for debugger graph
compatibility. Newly installed menus do not populate its native room/station
baseline and never invoke it. Explicit unbound diagnostics resolve current ROM
metadata in a separate path; switching an installed menu back to diagnostics
does not dereference empty historical captures. Retaining this compatibility
shape does not create an installed runtime ROM fallback.

Verification independently decodes native fourteen-byte records and room
coordinate bytes for every valid index, including matching the room area.
All 34 anchors match. Empty, sparse, fully explored and downloaded map states
retain native initial limits/clipping and subsequent directional trajectories
and sound ticks. Every saved station enters its room-map view with exact pixels,
phase timing and unchanged full SRAM bytes while bank-$8F room reads and the
bank-$80 load-record range are forbidden. The fixture respects each area's
actual expanding-window endpoint instead of assuming one shared duration.

Additional checks cover all six selected areas with every combination of
visible area labels (384 rendered comparisons), ordinary installed
entry/scroll/restore/return/reentry, and a legacy-style graph that actually
contains native room/station captures at nonzero Maridia station three.
After restoration and rebinding, that graph returns/reenters with exact frames
while the metadata gate is closed. Unbinding a newly installed graph is also
covered. This is focused compatibility evidence, not an archived historical
full-gameplay-session validation.

Focused map tests, full core verification and Windows Release build pass.
Local logs: `282-load-anchors.log`, `282-load-anchors-suite.log` and
`282-load-anchors-windows.log`. Shared sprite/initial-menu presentation,
HUD/pause dependencies and combined integration gates remain; #282 stays open.

## Map sprite compositions and shared OBJ artwork (catalog version 13)

`map-sprites.json` contains 26 named frames for map arrows, markers, stations,
elevator labels and world labels. Each ordered visual part specifies signed
pixel offsets, tile column/row in `map-objects.png`, 8- or 16-pixel size,
horizontal/vertical flips, priority and an optional palette. A null palette
inherits the caller's live palette, preserving blinking and defeated markers.
Explicit palette indexes replace only that visual part's palette. Empty frames
are valid invisible artwork; missing names, invalid regions and unsupported
fields fail validation. Navigation, discovery and collision remain compiled.

The indexed PNG is 128 by 128 pixels (16 by 16 character tiles). Its preview
palette is not the live color source. It is shared menu artwork, so changing
characters also affects other pause-page sprites using those same characters.
Only the map compositions are migrated here: equipment-page composition and
other HUD/menu work remain under #544, and general sprite migration under #535.
This is not arbitrary-resolution or editable gameplay data support.

Installed file select and pause maps use compiled visual parts through the
same OAM clipping/packing helper as the cartridge spritemap loader. No synthetic
ROM bus is constructed at runtime. Current-content rebinding refreshes the OBJ
sheet and composition references without resetting menu logic. Catalog hashes
and selected-content identity include both files; importer upgrades preserve
their independent overrides.

Verification covers all 26 native frames across seven X origins, eight Y
origins, eight palettes and three OAM occupancy levels (empty, 127 and 128).
An independent signed-coordinate oracle checks all 65,536 Y origin/offset
pairs, including native partially-above-screen visibility. All 8,192 character
bytes round-trip exactly. File-select entry/scroll/restore/return/reentry and
pause rendering forbid reads of the migrated pointer entries, compositions,
title binding and OBJ sheet. Independent JSON and PNG edits change actual
pause/file-select pixels; current-content restoration returns exact stock
pixels. Authored size, priority, flips, offsets and fixed palette are asserted
in OAM, with invalid and missing frame/resource checks.

The isolated full installer exercises fresh import, cancellation, catalog
upgrade and restart while preserving these overrides and synthetic player
files. Focused map checks, full core verification and Windows Release build
are the verification gates. Generated artwork and logs remain local only.
Shared initial BG2, remaining HUD/pause dependencies, combined ROM-read gates,
historical full-session and Android device verification remain outstanding.

## Saved-map lifecycle without any cartridge bus access

The shared `$8E:DC00` initial BG2 template was the last initialization read in
the installed saved-map lifecycle. It is not a rendered saved-map resource:
world-map captures/direct rendering exclude BG2, and room select installs its
complete area-specific BG2 frame before drawing. Installed maps now omit this
unused transfer instead of importing dead memory as an editable visual.
Other menus and unbound cartridge diagnostics retain their existing template
load. The world owner's unused BG2 page is consequently zero, not a copy of
native unused bytes; all other world VRAM and the complete room VRAM remain
exact. No asset schema or override change is required.

A pre-change test failed at the template read. After removal, an address space
that rejects **every read and write** permits installed saved-map construction,
entry, area/room expansion, scrolling, debugger restore/rebind, return, reentry,
and both load/options fade handoffs. Each frame is compared to the cartridge
path, including captured-layer rendering used by presentation backends as well
as direct rendering. The test stops at the handoffs; it does not claim the
options screen, game-room loader or SPC execution is independent of the ROM.

All 34 valid saved-station entries, four visibility patterns and subsequent
scroll trajectories now run under the all-access prohibition. All 384 world
label selection/visibility renders do too. Native records are read only by the
separate verification oracle before production comparisons; no ROM bytes are
forwarded by the installed test bus. The older legacy-closure test remains a
focused metadata guard, not an archived full-session test.

This supersedes the earlier outstanding initial-template and combined
saved-map-read notes. Shared pause/HUD dependencies, historical full sessions,
Android device coverage and the whole-game ROM-unavailable gate remain open.

## Additional pause interface artwork (#544, catalog version 14)

`pause-ui-tiles.png` exposes the second 256 pause background characters as an
indexed 256x64 PNG. It complements `map-tiles.png`; neither file overrides the
other's characters. The importer decodes `$B6:A000-$B6:BFFF`, and the runtime
loads the compiled 4-bpp result immediately after the shared map atlas. Live
colors still come from pause palette content; the PNG palette is for preview.
Keep the dimensions and indexes 0..15, edit this file under `overrides/maps`,
and restart/rebind content to apply the replacement. Transparency retains
native index-zero behavior. No arbitrary-resolution support is implied.

The pause constructor and current-content rebind no longer read this character
range. Tests prohibit those reads and compare all 8,192 native bytes, then
compare map/equipment transitions in Crateria, Maridia and Tourian. Independent
PNG edits visibly change equipment rendering immediately, including captured
rendering, without changing selection or equipped/collected beams. Restoring
the serialized pause state and rebinding stock returns the original pixels.
Invalid dimensions, out-of-range indexes, corrupt overrides and missing/corrupt
stock fail explicitly; an override does not bypass stock provenance checks.
Full-installer tests preserve the new override through cancellation, upgrade
and restart alongside synthetic player files. Generated content stays ignored.

This is a partial #544 implementation shared with #282. Remaining pause reads
include base/button/equipment tilemaps, inventory-dependent visual patches,
area labels, selector presentation and reserve presentation. Mixed records
also contain equipment masks, suit-selection rules and reserve-transfer amounts;
those mechanics must become compiled definitions, not editable artwork tables.
The pause screen is not yet ROM-free, and Android device validation is pending.

## Compiled pause equipment rules (#544/#282)

`PauseEquipmentRules` separates three application-owned inputs from visual
resources: the fourteen ordered equipment masks at `$82:C04C-$82:C067`, the
four wireframe discriminators at `$82:B257`, and the manual reserve-transfer
amount at `$82:BF04`. The pinned ROM, upstream C and bank-$82 disassembly agree
on their values. Definitions use existing proven equipment/beam flags, not
new inferred flags. No presentation file or schema is added for these rules.

Wireframe selection tests only Varia and Hi-Jump; Gravity is intentionally
ignored, matching the cartridge. Selection maps to the existing visual patch
pointer, which remains a separate presentation dependency. Manual transfer
still moves one energy per selected dispatcher tick and discards remaining
reserves when full health is reached; this migration does not rebalance that
behavior or change transfer ordering. Category control flow and the existing
same-frame category/copy-length quirks remain untouched.

Verification compares all fourteen native masks across every 16-bit inventory
word, all 104 per-category ownership subsets, actual A toggles for every single
upgrade, every 16-bit wireframe selection, and the resulting native wireframe
patch words for all variants with/without Gravity. Manual-transfer fixtures
assert each health/reserve frame for ordinary exhaustion, reaching full health,
and starting already full. The production pause paths reject reads of the
migrated rule ranges. Existing broader menu/glitch tests remain part of the
full suite. Visual layout/patch/selector extraction and final integration are
still outstanding; this is not completion of either parent issue.

## Pause backdrops and button artwork (#544/#282)

Catalog version 15 adds `pause-backdrops.json` (resource schema version 1).
The manifest now hashes seven area maps and seventeen shared resources. Each
of the seven named `areas` entries is a complete 32-column, 32-row BG2 backdrop,
including area lettering. `buttons` is the native 32-column, 16-row mutable
button template. Cells reference `Map` (`map-tiles.png`) or `Interface`
(`pause-ui-tiles.png`), with zero-based `tileColumn` 0..31, `tileRow` 0..7,
`palette` 0..7, `priority`, `flipX` and `flipY`. No addresses, raw tile words,
navigation commands or inventory rules are part of the document.

Import composes `$B6:E000` with the exact unmasked twelve-word area label
selected by `$82:965F`, as `$82:93C3` does at BG2 word `$38AA`. The button
source `$B6:E400` overlaps the second half of that frame transfer, but is a
distinct mutable visual resource at runtime. Native `$82:8EDA` and `$82:93C3`
in the pinned C/disassembly were inspected alongside the ROM. All native
words survive conversion, including unused page cells.

For edits, copy the stock JSON to `overrides/maps/pause-backdrops.json`.
Change cells in the desired area's grid to move or redraw lettering or the
frame; the loader no longer overwrites that lettering on return from equipment.
Button graphics can be replaced in `buttons`. Currently only its rows 9..10
are uploaded, to screen rows 25..26. Highlighted label spans remain compiled:
MAP columns 5..9, EQUIPMENT 12..15, START 22..26 on both rows. Their live
palette overrides authored palette values, preserving native control feedback.
Moving these interactive highlight regions is **not yet supported**; remaining
layout work must expose semantic label placement, not editable navigation.

Content rebinding refreshes BG2 and button artwork without changing page,
selection, scroll, inventory or fade timing. It carries only the live label
palette fields forward, including when the button mode changes before the page
changes. It does not reconstruct the equipment tilemap: native same-frame
Boots-to-Plasma label overruns must survive. No serialized state field was
added; restored states use their existing live button palette fields.

Verification covers exact 7,168 backdrop words and all 512 button words,
532 stock/native transition frames with frame/button/label reads forbidden,
fresh edited sessions versus per-frame rebinding, direct/captured visible
pixels, map/equipment/Start highlights, state restore, and preservation of
the real nine-word Plasma/VAR glitch. Invalid schema, dimensions, atlas names,
coordinates, palettes, missing resources and corrupt stock/overrides fail
explicitly. Full installer fixtures preserve the new override during cancelled
upgrade, successful version replacement and restart, alongside other overrides
and synthetic player files; no actual player data is touched.

Remaining pause dependencies include the equipment base, inventory/wireframe
patches, selector/reserve presentation, and semantic interactive-label placement.
This does not complete #544/#282 or prove the pause screen is ROM-free. Broader
integration, historical full-session coverage and Android validation remain.

## Editable equipment wireframes (#544/#282)

Catalog version 16 adds `pause-wireframes.json` (resource schema version 1),
bringing the shared-resource hash count to eighteen. Existing backdrop/PNG/JSON
override schemas are unchanged. The four named frames are `PowerSuit`,
`PowerSuitHiJump`, `VariaSuit` and `VariaSuitHiJump`. Each contains 136 cells in
eight-column, seventeen-row order, referencing the same `Map`/`Interface` PNG
atlases and visual attributes as pause backdrops. Copy the stock file to
`overrides/maps/pause-wireframes.json` and change the desired named frame;
individual variants can be reskinned independently.

Import resolves `$82:B25F` and the four native artwork patches. Runtime selection
continues to use the compiled Varia/Hi-Jump rule, with Gravity intentionally
ignored. The `$82:B20C` patch footprint remains compiled: eight words per row,
seventeen rows, starting at equipment tile (12,7), with a 32-tile row stride.
This step exposes the artwork, not arbitrary wireframe placement or gameplay
selection. Native disassembly confirms the mask, pointer order, loop dimensions,
start offset and stride; all 544 source words are compared independently.

Content rebind reapplies only the selected wireframe rectangle to the mutable
equipment page. It uploads that page only when equipment currently occupies
BG1; editing while viewing the map cannot overwrite map tiles. Surrounding
inventory labels are not reconstructed. Existing simultaneous-input Plasma/VAR
overrun tests still pass, including the wireframe's native overwrite of the
ninth overrun word. No serialized state fields were added.

Verification compares each patch against native source words with sentinels
outside its rectangle; 576 actual map/equipment transition frames cover all four
variants with/without Gravity and forbid wireframe pointer/art reads. Independent
JSON edits alter only the named variant and reach actual direct/captured pixels.
Real menu navigation and A toggles exercise all four variants. State restore
rebinds current artwork while preserving inventory identity, cursor and animation
phase. Invalid schemas, missing frames, wrong dimensions, atlas/palette errors,
invalid patch destinations, missing stock and corrupt overrides fail explicitly.
Installer tests preserve this override alongside previous schemas across fresh
installation, cancelled upgrade, replacement and restart.

This remains partial #544/#282 work. Equipment base/label patches, semantic
placement, selector/reserve presentation and broader integration remain; the
pause screen is not yet ROM-free, and Android validation remains outstanding.

## Editable equipment selectors (#544/#282/#535)

Catalog version 17 adds `pause-selectors.json`, schema version 1 (nineteen
shared resource hashes). It uses the existing `map-objects.png` character sheet.
The native three sprite shapes, sixteen anchors, palette and fourteen-phase
timing loop are resolved at import. The otherwise-unused middle byte of native
timing entries is not an editable engine instruction. All native sprite offsets
are zero; the importer validates that binding instead of publishing ROM indexes.

`anchors` contains `Reserve.Mode`, `Reserve.Transfer`, `Beam.Charge`, `Beam.Ice`,
`Beam.Wave`, `Beam.Spazer`, `Beam.Plasma`, `Equipment.Varia`, `Equipment.Gravity`,
`Equipment.MorphBall`, `Equipment.Bombs`, `Equipment.SpringBall`,
`Equipment.ScrewAttack`, `Boots.HiJump`, `Boots.SpaceJump`, `Boots.SpeedBooster`.
Coordinates are final screen origins (X 0..255, Y 0..223); import already applies
the native minus-one correction to both axes. `frames` contains named arrays of
the shared sprite-part schema: offsets, indexed tile region, 8/16 size, priority,
flips and optional palette. Null part palette inherits the document's `palette`.
Empty arrays explicitly author a hidden frame. Unknown frame references fail.

`animation` is an ordered array of 1..255 phases, each with `durationTicks`
(1..254) and named `reserve`, `beam`, `equipment` frame references. Suits and
Boots share the equipment visual group, exactly as native. `initialDurationTicks`
is separate from the looping first-phase duration. The shipped shapes do not
change over their fourteen timing phases, but replacements may animate visually.
Changing selector palette does not change the pause-map marker's caller palette.

Copy stock JSON to `overrides/maps/pause-selectors.json` to edit. Navigation,
inventory eligibility, same-frame category dispatch, input bindings and empty-
inventory gating remain compiled. Sprite parts and anchors cannot change those
rules. Rebind preserves serialized timer and phase; drawing a shorter replacement
cycle projects the saved phase modulo its new length, and the next timer expiry
advances in that cycle. The diagnostic spritemap ID reports the original category
binding, not an author-supplied address. No serialized state fields were added.

Verification independently checks native anchors, initial and looping delays,
palette, 672 OAM cases (all anchors/phases with empty/nearly-full/full OAM), and
1,600 real equipment-menu frames with selector ROM reads forbidden. Those frames
compare native timing and pixels and restore a serialized menu midway through.
Custom two-phase hidden/visible artwork verifies exact timing, moved origins,
parts, large size, flips, priority, explicit/inherited palettes, direct/captured
rendering, actual equipment toggling and current-content restore. Shortened-cycle
and empty-inventory controls pass. Strict invalid/missing/corrupt resources and
installer preservation cover the new file; prior override schemas remain intact.

Equipment base/label patches, reserve presentation, coordinated semantic layout
placement and broader integration remain outstanding. Moving only the selector
does not move its equipment label. This partial implementation does not close
#544/#282/#535 or prove the entire pause menu or game is ROM-free. Android device
and historical full-session validation remain pending.

## Editable reserve tank strip (#544/#282/#535)

Catalog version 18 adds `pause-reserve-tanks.json`, schema version 1 (twenty
shared resource hashes). The six ordered screen anchors and ten named sprite
compositions (`Full`, `EndCap`, `Empty`, `Fill1` through `Fill7`) reference the
existing `map-objects.png`. Each anchor has final X/Y coordinates; only Y receives
the native minus-one correction during import. The document palette is inherited
by parts with a null palette; individual parts may override it. Empty composition
arrays deliberately hide a visual. Sprite parts share the selector schema and
strict offset, atlas-region, size, priority, flip and palette validation.

The amount of energy represented by a tank, partial-fill selection, low-fill
flicker, capacity gating and reserve transfer remain compiled. Import validates
both native partial-fill tables at $82:B3D9 against the compiled binding: each
maps to sprites $20..$27. Full tanks use the distinct $1B composition, not the
seven-sevenths partial composition. The native unused palette timer is not exposed
as an author-controlled engine script. Drawing uses the already-sampled NMI byte
and never advances it. No serialized state fields were added. Rebinding restored
menus immediately uses current artwork/anchors without modifying reserve energy.

Copy stock JSON to `overrides/maps/pause-reserve-tanks.json` to replace it. This
moves only the tank strip, not supply digits or reserve-mode labels. The six
positions preserve the native origin table, including trailing-cap support.

Tests compare all ten compositions at six anchors and three OAM capacity levels
(180 cases), then all supplies from zero through each legal capacity 0/100/200/
300/400 at both flicker phases (2,010 actual menu frames). Full OAM and pixels
match the ROM-backed path with native tank positions, fill tables, sprite pointers
and compositions forbidden. Edited position/art/size/flips/priority/palette and
hidden cap reach real menu OAM and direct/captured rendering. Current-content
restore, retained nonzero flicker phase, redraw stability and actual manual reserve
consumption are checked. Invalid JSON, missing/corrupt stock and invalid regions
fail explicitly. Installer preservation includes the new override alongside every
prior presentation resource.

This is partial #544/#282/#535 work. Reserve labels/digits/arrow presentation,
equipment base/inventory patches, coordinated layout and the remaining broader
menu/HUD/sprite integration still prevent closing those tickets. Android and
historical full-session validation are still outstanding.

## Editable reserve labels, digits and arrow (#544/#282)

Catalog version 19 adds `pause-reserve-ui.json`, schema version 1 (twenty-one
shared resource hashes). It contains four named label patches (`Mode`,
`ReserveTank`, `Manual`, `Auto`), ten supply-digit cells, the ten tilemap cells
whose palettes form the energy-transfer arrow, its enabled/disabled palettes,
solid colors and all 32 animated color pairs. Every tile is a named Map/Interface
atlas reference. Anchors use zero-based columns and rows in the 32x32 equipment
page; the file contains no WRAM addresses, energy values or input commands.

Copy the stock file to `overrides/maps/pause-reserve-ui.json` to edit it. Capacity,
current energy, decimal conversion, AUTO/MANUAL behavior, transfer rate, selector
rules, palette destinations and animation phase remain compiled. A zero-capacity
inventory still draws no reserve labels. Mode zero retains the authored `Mode`
baseline exactly as native; nonzero modes replace only the first four characters
while preserving their live tile attributes. Rebinding restored state refreshes
only these bounded reserve footprints, so unrelated equipment labels and the
native same-frame Plasma/VAR overrun remain untouched.

Verification compares installed and ROM-backed pause menus for all 32 arrow phases
with every migrated source blocked, including complete VRAM, CGRAM and rendered
pixels. Independent label, digit and animated-color edits reach the actual menu;
current-content restore and strict malformed/missing/corrupt-resource failures
pass. Full-installer fixtures preserve the override across cancelled upgrade,
replacement and restart alongside prior overrides and synthetic player files.

Equipment base/inventory-label patches, coordinated semantic layout, broader
HUD/menu scope, historical full-session coverage and Android validation remain.
This partial implementation does not complete #544 or #282.

## Editable equipment-page base (#544/#282)

Catalog version 20 adds `pause-equipment-base.json`, schema version 1
(twenty-two shared resource hashes). Its 1,024 cells are the complete 32x32 BG1
equipment-page template expressed as Map/Interface atlas references. Copy it to
`overrides/maps/pause-equipment-base.json` to replace the page frame, separators
and other static artwork. Inventory ownership, navigation and item effects remain
compiled; this resource contains no masks, callbacks or control tables.

The native page is mutable after setup. Current-content rebinding therefore
refreshes only static cells. It preserves all item-label footprints (including
the four extra words exposed by the native Boots-to-Plasma overlong-copy glitch),
the selected wireframe, reserve labels/digits and the live arrow palette. The
arrow's replacement character/flip/priority fields still update while its latched
palette survives. Dedicated visual owners reapply their own bounded resources.
This avoids reconstructing or normalizing cartridge-visible state merely because
the host reloads presentation content.

Verification compares all 1,024 words and 80 native transition frames with the
original `$B6:E800-$B6:EFFF` range forbidden. A static-cell edit reaches actual
equipment pixels. Synthetic live-state sentinels, the real Plasma/VAR regression,
arrow palette ownership, debugger restore/current-content rebind and strict
invalid/missing/corrupt resource failures pass. The full installer preserves the
override through cancellation, replacement and restart.

Inventory-label artwork and semantic label placement remain ROM-backed, along
with broader HUD/menu scope, historical full-session coverage and Android
validation. This is not completion of #544 or #282.

## Editable equipment labels and placement (#544/#282)

Catalog version 21 adds `pause-equipment-labels.json`, schema version 1
(twenty-three shared resource hashes). It contains the fourteen named beam,
suit/misc and boot labels, the Hyper Beam label, the nine-word blank patch,
each label's zero-based equipment-page column/row and the disabled palette.
Copy it to `overrides/maps/pause-equipment-labels.json` to change label artwork,
placement or disabled color. Inventory masks, collection/equip state, category
dispatch, navigation and beam-exclusion behavior remain compiled.

Installed pause setup and equipment toggles no longer read the bank-$82 label,
blank, destination or pointer tables. Hyper Beam retains the cartridge's layout:
the five beam destinations are blanked and its one visible label occupies the
Wave row. The same-frame Boots-to-Plasma path still performs its nine-word copy;
the extra words come from the first four editable Varia cells and the wireframe
owner overwrites the ninth word exactly as native. A content reload with unchanged
label data preserves the mutable live page byte-for-byte. A label edit rebuilds
only that semantic layer on the new authored positions, then reapplies the native
overrun, wireframe and reserve owners.

Verification compares empty, mixed equipped/disabled and Hyper inventories with
the entire `$82:BF32-$82:C0B1` label/table range forbidden. Edited art and placement
reach a live equipment page immediately. The retail Plasma/VAR overrun survives a
changed-content rebind, while missing, overlapping, malformed and corrupt resources
fail explicitly. The full installer preserves this override through replacement.

Broader HUD/menu/timer presentation, historical full-session coverage, Android
validation and the shared ROM-unavailable runtime gate remain. This partial
implementation does not complete #544 or #282.

## Editable escape-timer compositions and layout (#544/#282)

Catalog version 22 adds `escape-timer.json`, schema version 1 (twenty-four shared
resource hashes). It contains named `Label` and `Digit.0` through `Digit.9` OAM
compositions, the four screen-relative anchors for the label/minutes/seconds/
centiseconds groups, digit spacing and the caller palette. Parts use the full
nine-bit gameplay OBJ tile number because the timer lives at tiles 480-504,
rather than pretending it occupies the shared menu atlas.

Copy the stock file to `overrides/maps/escape-timer.json` to change composition
or placement. The BCD values, NTSC decrement table, startup movement, timeout,
escape sequencing and host one-second testing floor remain compiled. With an
installed catalog, normal timer drawing consumes only this resource and performs
no label, digit-pointer or spritemap reads from bank $80. The legacy cartridge
path remains available to diagnostics that deliberately construct no catalog.

Verification compares complete low/high OAM for a mixed-digit timer against the
ROM-backed renderer, with every installed-path bus read forbidden. A moved minute
anchor changes only those four OAM parts. Deterministic extraction, selected
content identity, malformed/corrupt data, missing named digits, invalid palette
and invalid spacing all fail explicitly. Broader timer OBJ artwork extraction,
other HUD/menu presentation, historical full-session coverage and Android
validation remain; this does not complete #544 or #282.

## Editable gameplay HUD presentation (#544/#282)

Catalog version 23 adds `gameplay-hud.json`, schema version 1 (twenty-five
shared resource hashes). It contains the three-row gameplay HUD template,
equipment icons, health/ammunition digits and anchors, energy-tank cells,
reserve AUTO indicator, selected/deselected palettes, and the minimap anchor.
Copy it to `overrides/maps/gameplay-hud.json` to change that presentation while
leaving counters, inventory ownership, exploration state, and selection logic
compiled.

Installed gameplay no longer reads those visual tables from the ROM. Runtime
catalog rebinding rebuilds the presentation-owned cells and carries the live
logical 5x3 minimap to an edited anchor instead of retaining stale layout data.
The cartridge-backed path remains available for focused native comparisons.

Verification compares stock initialization, live counter changes, selection,
reserve state, and minimap output with every migrated ROM range blocked. It also
checks visible icon/digit/anchor edits, state-safe content rebinding, deterministic
extraction, strict malformed/overlapping resources, and full-installer override
preservation. Broader HUD/menu extraction, historical full-session coverage and
Android validation remain; this does not complete #544 or #282.

## Editable game-over screen (#544/#282)

Catalog version 24 adds `game-over.json`, schema version 1 (twenty-six shared
resource hashes). It contains the complete 32x32 text tilemap, Baby Metroid,
egg and four cursor compositions, four Baby palettes, actor anchors/palettes and
cursor animation timing. Compositions reference the existing shared menu OBJ
PNG; background cells reference `map-tiles.png`.

The sixty-record Baby animation sequence, its three named sound callbacks, answer
semantics, music queueing and fades are compiled application behavior. Native
instruction pointers remain debugger/state identities, but installed execution
does not read the text streams, animation records, palette tables, spritemap
pointers or parts from the cartridge. The legacy cartridge path remains for
diagnostics and synthetic dispatcher fixtures.

Verification checks the compiled sequence against `$82:BC27-$82:BD96`, then runs
518 ROM-backed and completely bus-forbidden installed frames side by side through
all three cries, music wait, No selection and fade. Every pixel, animation pointer,
frame identity and audio command matches. Tilemap/composition/palette/layout edits,
live content rebinding, strict invalid resources and full-installer preservation
also pass. File-select/options presentation, broader shared integration, historical
full-session coverage and Android device validation remain; this does not complete
#544 or #282.

## Editable options-menu presentation (#544/#282)

Catalog version 25 adds `options-menu.json`, schema version 1 (twenty-seven shared
resource hashes). It contains the shared background and five complete 32x32 pages,
the seven configurable button-label patches and their destinations, language and
special-toggle highlight regions, three heading compositions, four selector
compositions, page-specific actor anchors, palette choices and selector timing.
Copy it to `overrides/maps/options-menu.json` to edit this presentation.

Navigation, page transitions, controller permutation swaps, language and special
option semantics, menu sounds, fades and persistence remain compiled behavior.
Installed execution no longer reads compressed page streams, controller-label
tiles, selector/heading spritemaps or those presentation tables from the cartridge.
The pages retain the complete ten-bit SNES character references because this menu
uses multiple already-extracted VRAM artwork regions rather than only the area-map
tile atlas. The cartridge-backed construction path remains for parity diagnostics.

Verification runs 196 ROM-backed and completely bus-forbidden installed frames
side by side through language switching, both page dissolves, controller rebinding,
native controller-page scrolling, both special toggles, return transitions and
their audio commands. Every rendered pixel, phase and selected row matches. It also
checks visible page/composition/anchor edits, state-safe live rebinding,
deterministic extraction, strict malformed/missing/overlapping data failures and
full-installer override preservation. File-select-menu presentation, broader
shared integration, historical full-session coverage and Android validation remain;
this does not complete #544 or #282.

## Editable file-select and data-management presentation (#544/#282)

Catalog version 26 adds `file-select.json`, schema version 1 (twenty-eight shared
resource hashes). It contains the shared background; separate complete templates
for populated/empty main screens and every COPY/CLEAR stage; ENERGY, NO DATA and
time-separator patches; digit and slot-letter cells; main/data slot-field layouts;
three borders, four selectors and eight helmet compositions; all actor anchors;
and selector/helmet timing. Copy it to `overrides/maps/file-select.json` to edit
those visuals and placements.

SRAM checksums, slot availability, value formatting, navigation, copy/clear
mutation, confirmation semantics, sounds, helmet state and fades remain compiled.
The runtime overlays live health and time values plus source/destination letters
onto authored templates. Installed execution no longer reads the bank-$81 text
streams, initial background, menu spritemap pointers or compositions. Shared PNG
artwork and palettes continue through the existing catalog resources.

Verification runs 231 ROM-backed and presentation-read-blocked installed frames
side by side through COPY source/destination/confirmation/completion, CLEAR
selection/confirmation/completion, empty-slot rebuilding, return fades, helmet
turning and audio. Every tilemap, rendered pixel, phase, selection and command
matches. It also checks visible template/patch/actor/anchor edits, state-safe live
rebinding, deterministic extraction, strict malformed/missing/duplicate failures
and full-installer preservation. Broader shared integration, historical
full-session coverage and Android validation remain; this does not complete #544
or #282.

## Editable escape-timer OBJ artwork (#544/#282)

Catalog version 27 adds `escape-timer-tiles.png` (twenty-nine shared resource
hashes). It is an indexed 200x8 atlas containing the twenty-five consecutive
4-bpp OBJ characters from `$B0:C000-$B0:C31F`. Copy it to
`overrides/maps/escape-timer-tiles.png` to replace the `TIME` label and digit
pixels independently of `escape-timer.json` composition and placement.

The cartridge intentionally uploads the art as a `$200`-byte page followed by a
`$120`-byte page. Installed Ceres and Mother Brain paths keep those two records,
their order, destinations and one-record-per-owner-call cadence. Typed VRAM asset
references defer byte resolution until NMI for Ceres; Mother Brain's synchronous
owner resolves the same catalog pages directly. A restored legacy queue is rebound
from its two native source addresses without changing queue position or byte count.

Verification compares the deterministic PNG and both compiled pages byte-for-byte
with the cartridge. It exercises typed queue draining with all bus access forbidden,
synchronous Mother Brain publication, restored pending-write rebinding, Ceres's real
self-destruct dispatcher selecting both typed pages, an isolated pixel edit that
changes only the first page, wrong-size rejection and full-installer preservation.
Countdown mechanics, spritemap composition, transfer cadence and later typewriter
pages remain compiled/native owners. Historical full-session coverage and Android
device validation remain shared integration work.

## Editable title color-math gradient (#549)

Catalog version 37 adds `title-gradient.json`, schema version 1 (thirty-nine
shared resource hashes). It expands the title screen's sixteen HDMA variants into
224 explicit scanlines. Each line exposes RGB5 `red`, `green`, and `blue` values
plus the native eight-bit `colorMathControl`; `zoomHighNibble` identifies the
variant selected by title zoom bits four through seven. Copy the stock file to
`overrides/maps/title-gradient.json` to replace the presentation without patching
the ROM.

Title zoom, Mode 7 movement, fades, OBJ palette eligibility, and color-math
semantics remain compiled behavior. Installed title construction, attract-demo
return, debugger restore, and content rebinding select the current catalog
resource. The diagnostic cartridge fallback remains available when no installed
catalog is bound.

Verification compares all sixteen extracted variants and all 256 zoom selections
with the pinned cartridge, then drives the real title owner while every gradient
source byte is forbidden. A valid override changes both the installed-content
identity and the production title frame; removing it restores both exactly.
Malformed dimensions, identities, RGB5 values, controls, and schema versions fail
loudly.

## Editable title palette (#549)

Catalog version 40 uses `title-palette.json` schema version 2 (forty-four shared
resource hashes). Its `colors` array contains all 256 initial CGRAM entries as RGB5
objects. `babyMetroidTubeLight` contains eight four-color frames and
`flickeringDisplays` contains two two-color frames. Copy the stock file to
`overrides/maps/title-palette.json` to replace title colors without modifying the
cartridge.

The resource owns color values only. Console-light instruction streams, animation
cadence, color destinations, title phases, and brightness remain compiled/native
behavior. New title sequences—including attract-demo returns—use the selected
installed palette. A debugger state retains its already-materialized CGRAM so
restoring a mid-animation frame does not reset visual phase, then rebinds the current
installed colors for subsequent animation frames.

Verification compares every initial and ambient extracted word with the cartridge,
constructs the real title owner while all source color bytes are forbidden, and runs
both ambient programs through two complete cycles with exact native CGRAM parity. A
valid override changes content identity, rendered title pixels, and live ambient
output; removing it restores all three exactly. Wrong initial/frame color counts,
unsupported versions, out-of-range RGB5 components, unknown native-address fields,
and malformed JSON fail loudly.

## Editable environmental room palette effects (#536, #549)

Catalog version 59 uses `room-palette-effects.json` schema version 17 (forty-seven
shared resource hashes). The document exposes the four synchronized Norfair environmental
programs as named arrays: `norfairForegroundAndHeatPhase`,
`norfairForegroundPalette4`, `norfairForegroundPalette5`, and
`norfairForegroundPalette6`. Each contains sixteen frames of five RGB5 colors. It
also exposes `maridiaSandPits` (four frames of eight colors), `maridiaSandFalls`
(four frames of four colors), and `maridiaBackgroundWaterfalls` (eight frames of
eight colors). The shared `wreckedShipGreenLights` array contains eight two-color
frames used by both powered Wrecked Ship palette definitions. Red Brinstar's
fourteen eight-color frames live in `redBrinstarBackgroundGlow`; Tourian's eleven
split eight-color frames live in `tourianGlow`. The shared `brinstarBlueSpores`
array contains fourteen three-color frames used by both its standard-room and Spore
Spawn programs. `bombTorizoBelly` and `goldenTorizoBelly` each contain six distinct
three-color frames. `tourianStatueGrey` contains the shared eight frames of eight
colors used by all four boss-statue entries. `crateriaSurfaceLightning` exposes
thirteen eight-color records; `crateriaUnusedDarkLightning` exposes fourteen
seven-color records. `ceresGunshipEngineLights` exposes the two one-color engine
flicker records; `ceresNavigationLights` exposes the shared fourteen two-color records
used by both sprite-Ceres and background-Ceres entry points. The separate
`planetZebesTextFadeIn` and `planetZebesTextFadeOut` arrays each expose eight
three-color records for the opening cinematic's one-shot text fade.
`oldMotherBrainBackgroundLights` exposes fourteen three-color pulse records, while
`cinematicGunshipGlow` exposes fourteen one-color glow records. The zoomed-out
`explodingZebesFade` has seven eight-color records, and `unusedCinematicFade` preserves
the cartridge's eleven sixteen-color records even though retail never installs it.
`titleLogoFade` contains eight fifteen-color records; `nintendoSharedFade` contains
eight two-color records used by both the unused boot logo and live copyright fade.
The Zebes explosion sequence adds `zebesExplosionForeground` (sixteen fifteen-color
records), `zebesExplosionFinale` (forty-five fifteen-color records), and the shared
fifteen-record `zebesExplosionWhiteout` used by both its background and space entries.
The independent `zebesExplosionAfterglow` and `zebesExplosionLava` loops contain six
eight-color and ten one-color records. `zebesExplosionCrust` and
`zebesExplosionGreyClouds` each contain eight fifteen-color fade records, while
`zebesExplosionGunship` contains sixteen sixteen-color reveal records.
The three independently authored `samusLoadingPowerSuit`, `samusLoadingVariaSuit`,
and `samusLoadingGravitySuit` arrays each contain nine sixteen-color records;
`postCreditsIconGlare` contains fourteen sixteen-color one-frame records.
Tourian's escape uses `tourianEscapeShutter` and `tourianEscapeBackground` for its
independent fourteen-record loops, plus `tourianEscapeSharedRedFlash` for the seven
colors shared by the general-level and Arkanoid entries. The old-Tourian shaft uses
`oldTourianEscapeRedFlash` around its two inline CGRAM skips;
`oldTourianEscapeOrangeRailings` and `oldTourianEscapeYellowPanels` expose the paired
fifteen-record accent loops.
`upperCrateriaEscapeRedFlash` exposes its fourteen seven-color red-flash records;
`crateriaEscapeYellowLightning` and `crateriaEscapeCreBlockPixel` expose the
late-Crateria paired eleven-record loops with distinct eleven/five-color widths.
`beaconFlashing` exposes the shared ten-record Crateria/Brinstar beacon colors;
its inline CGRAM skip and mid-cycle sound command are not editable color data.
Copy the stock file to `overrides/maps/room-palette-effects.json` to recolor these
room and cinematic effects without changing the cartridge or engine code.

Only the 4388 authored RGB5 entries are presentation data. Native record layouts,
heat-phase publication, destinations, durations, palette-pointer skips, waits, and loop
targets remain compiled mechanics. The installed catalog binds its
color provider to the existing room palette interpreter, including after debugger-state
content rebinds; an unbound diagnostic interpreter retains its explicit cartridge
fallback.

Verification compares every extracted mapping with the cartridge, forbids color-source
reads for every installed program, and executes all four Norfair programs through two complete 116-frame cycles,
and executes the three Maridia programs through two complete cycles of their 40/40/16
cadences. Both Wrecked Ship definitions execute two complete 80-frame cycles from
their shared color payload; Red Brinstar and both Tourian callers execute two complete
140/110-frame cycles. The two blue-spore callers run two 140-frame cycles from their
shared payload, while both Torizo belly programs run two 52-frame cycles and retain
their enemy/boss deletion callbacks. CGRAM and Norfair heat-phase output match
frame-by-frame. All four Tourian statue entries also run through their shared fade and
terminal deletion. Both Crateria lightning definitions run two complete 503/743-frame
nested-timer cycles. The gunship engine runs two complete two-frame cycles, and both
Ceres navigation-light definitions run two complete 56-frame cycles through their shared
payload. Both PLANET ZEBES text fades execute all 24 frames and their terminal deletion.
Both cinematic-glow definitions run two complete 84/70-frame cycles. Both cinematic fades
run through their complete 56/22-frame one-shot programs and delete after the terminal
hold. The title-logo and both Nintendo callers likewise execute their 24-frame fades
through deletion. All nine explosion entries execute complete one-shot or two-loop
programs with exact native CGRAM parity; the shared whiteout payload is tested at
both destinations. All three Samus loading-suit programs run through their complete
265-frame counted schedules and terminal deletion, and the post-credits icon glare
runs through its fourteen-frame one-shot.
The seven Tourian escape entries each run for two complete loops, retaining both
inline CGRAM skips and the shared entry routing. Upper Crateria's red flash and both
late escape-lightning entries also run for two complete loops.
The beacon loop runs twice with exact palette and audio-request parity, including the
mid-cycle sound command. A valid override changes catalog identity and forty-seven
independent live runtime outputs, while removing it restores both exactly. Wrong frame
or color counts,
unsupported versions, invalid RGB5 values, unknown/native-address fields, corrupt stock,
and malformed overrides fail loudly.

## Editable Mother Brain health colors (#536, #549)

`mother-brain-health-palette.json` contains four fifteen-color `body` and
`backLegs` RGB5 arrays, following the cartridge's bank-$AD pointer tables.
The active room-enemy system copies the selected body array to both body and
brain CGRAM slots and the rear-leg array to its own slot. The three strict
health thresholds and final-battle update cadence remain compiled mechanics.
Copy the stock file to `overrides/maps/mother-brain-health-palette.json` to
recolor the damage states without altering the ROM. Verification compares all
eight threshold-boundary cases to native CGRAM output with palette ROM reads
forbidden, checks body and leg edits through the runtime binding, rejects
malformed arrays, and confirms override removal restores content identity.

## Editable Mother Brain rainbow and grey-drain colors (#536, #549)

`mother-brain-rainbow-palette.json` exposes ten paired rainbow-beam frames,
eight to-grey drain frames, eight from-grey revival frames, and the normal
body/rear-leg restoration pair. The grey frames each include the trailing RGB5
word that the cartridge publishes to WRAM `$017C`; revival still copies only
thirteen body/brain colors, preserving its two existing tail colors. The native
phase transitions, ten-entry loop cursor, CGRAM destinations, and copy lengths
remain compiled behavior. Copy the stock file to
`overrides/maps/mother-brain-rainbow-palette.json` to recolor these frames.
Stock verification checks every frame against the cartridge's CGRAM and trailing
WRAM output with runtime ROM reads forbidden, then checks independent rainbow,
drain, drain-tail, revival, and normal edits, strict schema validation, and
identity restore.

## Editable title artwork (#549)

Catalog version 39 adds four resources (forty-four shared hashes):
`title-mode7-tiles.png`, `title-mode7-map.json`, `title-object-tiles.png`, and
`title-baby-tiles.png`. The indexed PNGs expose all 256 Mode 7 characters, all
512 four-bit OBJ characters, and the sixteen chunky Baby-Metroid animation
characters. The 64x64 JSON map contains only eight-bit tile indexes. Copy any
complete stock file to `overrides/maps` to replace it.

Mode 7 transforms, scene motion, map fill outside the authored 64x64 region,
VRAM destinations, sprite composition, and the Baby animation page order/cadence
remain compiled behavior. The PNG palettes are index previews; live colors come
from `title-palette.json`, so recoloring does not duplicate pixels or alter tile
indexes.

Verification round-trips all four decompressed streams byte-for-byte, compares
140 production title frames with stock while the complete compressed-source read
closure is forbidden, and proves a Mode 7 PNG override changes installed identity
and rendered title output. Dimensions, palette cardinality, map shape/version,
tile-index range, malformed files, and missing manifest resources fail loudly.

## Editable room-FX animated characters (#542, #547, #549)

Catalog version 60 adds `room-fx-animated-tiles.png` to the stock manifest and
`overrides/maps` selection. It is an indexed, four-color strip of 89 two-bit BG
characters. Left to right, the frames are Maridia ceiling sand (four frames,
four characters each), falling sand (four frames, two characters each), lava
(five frames, four characters each), acid (five frames, four characters each),
and rain (five frames, five characters each). The palette in the PNG is only an
index preview; room CGRAM still controls the visible colors.

The original object selection, frame durations, loop commands, transfer sizes,
VRAM destinations, liquid physics, damage, and room-FX activation remain compiled.
The 23 visual-source identities are compiled separately from the 48 existing
control words. Installed liquid/rain transfers and queued sand transfers resolve
their bytes from the currently bound PNG, including after a debugger-state
restore; a diagnostic runtime without installed content retains cartridge art.

`--room-fx-animated-tiles` compares all 23 source operands and all 1,424 art
bytes against the pinned ROM, then executes every native frame through both
synchronous and queued transfers with bank-$87 reads forbidden. The shared
`--map-presentation` verifier changes one PNG pixel and observes the live VRAM
transfer, identity change, restored stock content, and strict corrupt-resource
failure. `--map-installation` verifies upgrade/restart preservation of overrides.

## Editable room-FX BG3 tilemap composition (#542, #547, #549)

Catalog version 61 adds `room-fx-layer3-tilemaps.json` with six named 32x33
tile-reference pages: Lava, Acid, Water, Spores, Rain, and Fog. Each cell exposes
its 8x8 character index as a column/row plus palette, priority, and flip flags.
The extra thirty-third row is part of the native 0x840-byte transfer and must
not be discarded. Copy the complete stock file to `overrides/maps` to change
effect composition without editing the ROM. These tile references select
characters from their existing artwork owners; the JSON does not alter liquid
physics, damage, scroll, effect activation, or animation cadence. Spores are
extracted for completeness, but the general visible Spores renderer is still
outside the translated room-FX set.

The six native source identities are compiled separately from the editable
tilemap words. Installed room loads no longer read the bank-$83 page-pointer
table or bank-$8A tilemap payload for the five translated visible effects.
`--room-fx-layer3-tilemaps` compares every word and pointer against the pinned
ROM and executes all five production room-FX load paths with those ROM ranges
forbidden. The shared map-presentation suite checks a changed Lava cell through
the actual VRAM transfer, strict malformed-resource handling, and stock restore.
The full installer fixture verifies that an edited tilemap survives upgrade and
restart alongside the other player overrides.

## Editable room-FX palette blends (#542, #547, #549)

Catalog version 62 adds `room-fx-blend-palettes.json` with eight named retail
selections, each holding three RGB5 colors. The selectors and their native
bank-$89 addresses remain compiled; the player edits colors, not FX-record
selection, liquid mechanics, or palette-write timing. Selection zero retains
the cartridge's special behavior: clear only CGRAM color 27, preserving 25/26.
The same installed catalog serves both room entry and later `LoadFxEntry` writes.

`--room-fx-palette-blends` checks all 24 words against the pinned ROM, then
exercises both production writes for every selection with bank-$89 reads
forbidden. `--map-presentation` edits one Lava blend component and observes the
changed CGRAM word and content identity; removal restores stock. The installer
fixture confirms this override survives upgrade and restart.

## Editable Power Bomb fixed colors (#542, #549)

Catalog version 63 adds `power-bomb-fixed-colors.json`: sixteen pre-explosion
and thirty-two explosion RGB5 triplets from the native bank-$88 tables. These
are display colors shared by Power Bombs, Crystal Flash, and Ceres destruction.
Copy the stock file to `overrides/maps` to edit the colors. The cartridge-owned
phase, radius, damage, HDMA shape, and timing logic stays compiled and unchanged.

`--power-bomb-fixed-colors` compares all 48 triplets with the pinned ROM and
runs the complete Power Bomb and Crystal Flash state machines with the native
color-table reads forbidden. The map-presentation suite checks that a changed
component reaches the actual explosion state and restoring stock clears the
override. The installer fixture checks upgrade/restart preservation.

## Editable Samus visor colors (#536, #549)

Catalog version 64 adds `samus-visor-colors.json` with the six RGB5 colors in
the native bank-$9B visor palette table. Copy the stock file to `overrides/maps`
to change the displayed colors. The room backdrop cycle and X-ray beam share
this palette source; their cartridge-owned timer, selection order, and beam
behavior remain compiled. Odd or out-of-range table offsets in diagnostic
states retain the native address-space path rather than being approximated.

`--samus-visor-colors` compares all six words with the pinned ROM, then runs
the production room and X-ray color cycles with native color-table reads
forbidden. The map-presentation suite checks edited colors reaching both CGRAM
paths, strict invalid-resource handling, and stock restoration. The full
installation fixture verifies override preservation across upgrade and restart.

## Editable Samus hurt and intro colors (#536, #549)

Catalog version 65 adds `samus-hurt-colors.json` with separate sixteen-color
RGB5 palettes for the ordinary hurt flash and cinematic restoration. Copy the
stock file to `overrides/maps` to edit either set. Native hurt-counter timing,
normal-suit restoration, impact/recovery sounds, and cinematic selection stay
compiled; this asset changes only the two copied color arrays.

`--samus-hurt-colors` compares all 32 words against the pinned ROM and checks
ordinary and cinematic production cycles with native hurt/intro color reads
forbidden. The map-presentation suite checks both edited palettes reaching
CGRAM, content identity, invalid-resource failure, and stock restoration. The
full installer fixture verifies the override survives upgrade and restart.

## Editable Samus Hyper Beam colors (#536, #549)

Catalog version 66 adds `samus-hyper-beam-colors.json` with ten full-body
sixteen-color RGB5 frames. The source pointer list remains a bounded compiled
selector; `frames[0]` through `frames[9]` follow native playback order, not
ascending bank-$9B address order. Copy the stock file to `overrides/maps` to
replace the colors. Rainbow acquisition, phase selection, variable frame delay,
and suit restoration remain cartridge-owned behavior.

`--samus-hyper-beam-colors` checks every pointer and all 160 stock color words
against the pinned ROM, then compares 22 production palette calls with native
pointer and color reads forbidden on the installed path. The map-presentation
suite checks an edited frame reaching CGRAM through a runtime-bound Samus,
invalid-resource failure, content identity, and stock restoration. The installer
fixture verifies that user edits survive upgrade and restart.

## Editable normal Samus suit colors (#536, #541, #549)

Catalog version 67 adds `samus-suit-colors.json` with sixteen RGB5 colors each
for Power, Varia, and Gravity Suit. Copy the stock file to `overrides/maps` to
change the colors; only the displayed OBJ palette is editable. Equipment
selection, palette priority, animation timing, and gameplay state remain in
code. The same installed colors are used when normal suit colors are restored
after hurt flashes, charge effects, Screw Attack frames, X-ray, shinespark,
and drained sequences.

`--map-presentation` compares all 48 stock colors against the pinned ROM,
checks edits for all three live CGRAM palettes and normal-restoration paths,
forbids reads of the original suit palette data during those paths, and
checks malformed colors, content identity, and override removal. The full
verifier covers the existing stock gameplay palette behavior.

## Editable Samus full-body cycle colors (#536, #541, #549)

Catalog version 68 adds `samus-full-body-cycle-colors.json`. It has named
`speedBooster`, `screwAttack`, `storedShine`, and `activeShinespark` families;
each is ordered Power/Varia/Gravity Suit, with four distinct shades per suit
and sixteen RGB5 colors per shade. Copy the stock JSON to `overrides/maps` to
edit it. The compiled cartridge selectors still choose suit, phase, timing,
and pointer. The six-frame Screw Attack and stored-shine cycles revisit shades
two and one; they are not duplicated in the editable file. Metroid attachment
uses the fourth Speed Booster shade.

`--map-presentation` checks all 768 stock color words against the pinned ROM,
then exercises every family, suit, and phase through the real palette handlers
with those source colors forbidden on the runtime bus. It checks four edited
shades, attachment flash, live catalog rebind, invalid RGB5 data, content
identity, and override removal. Non-catalogued restored/debugger phase words
retain their existing native adjacent-data reads rather than being mapped to
an unrelated editable shade.

## Editable Crystal Flash colors (#536, #541, #549)

Catalog version 69 adds `crystal-flash-colors.json` with ten ordered body
frames of ten RGB5 colors and six ordered bubble frames of six colors. Copy the
stock file to `overrides/maps` to edit it. The body record durations, five-call
bubble cadence, independent cursors, beam-palette restoration, and gameplay
state remain compiled. Repeated native body pointers are represented as
separate playback records so a cosmetic edit may distinguish those frames.

`--map-presentation` compares all 136 stock words against the pinned ROM and
runs a 100-call Crystal Flash with the native pointer and source-color reads
forbidden. It asserts each body/bubble CGRAM color, checks both timers and
cursors against an unedited native run, rejects invalid RGB5 data, and verifies
that stock re-extraction preserves the user override. Non-catalogued debugger
cursor values retain the native read path.
