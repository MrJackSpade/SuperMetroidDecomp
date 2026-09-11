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
