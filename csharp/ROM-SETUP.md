# Install game data from your ROM

Desktop and Android use the same `SuperMetroid.AssetExtraction` library. Application
packages contain no ROM or extracted audio. Most graphics and room data are still read
from the installed ROM; setup extracts room-character and opening-cinematic PNGs, base room palettes, other selected presentation
assets, and the audio catalog/112 PCM WAVs. No upstream disassembly checkout is needed.

Supported image: Super Metroid Japan/USA NTSC v1.0, 3 MiB, SHA-256
`12b77c4bc9c1832cee8881244659065ee1d84c70c3d29e6eaf92e6798cc2ca72`.
A 512-byte copier header is accepted and removed from the installed copy. Other revisions,
patched images, truncated files, and oversized files are rejected before installation.
The selected source file is copied, never moved or deleted.

## Desktop

Run `SuperMetroid.Game` and choose a `.smc` or `.sfc` file when prompted. You can also
pass one ROM path or set `SUPERMETROID_ROM`. Setup installs into
`%LOCALAPPDATA%/SuperMetroid/`; later launches reuse that installation, so the original
file no longer needs to be available. Setup errors leave the picker available to retry.

An existing `SuperMetroid.ini` beside the executable takes precedence over the AppData
copy (the executable directory, not the shell's working directory). This is a complete
configuration override, not a merge. Removing it restores AppData settings; it never
overwrites them. Startup prints the selected source and full path on separate lines.

The Windows ZIP includes `SuperMetroid.defaults.ini` beside the executable, listing
every supported setting and its default. On first use, the game copies this template
to `%LOCALAPPDATA%/SuperMetroid/SuperMetroid.ini` if that active file does not exist.
You may edit the template before first launch. Afterward, edit the active file and
restart the game; the startup console prints its full path. Existing settings always
take precedence over the defaults template. Replacing the application ZIP/template never resets player settings.
Legacy/developer callers without an installation root use the INI beside their ROM.
The template contains no developer cheats or enabled automatic GitHub reporting.

## Android

Install the APK and tap **Choose ROM**. Use Android's document picker to select your
ROM from local storage or a document provider. The app reads the selected stream once,
validates it, and installs a private copy under its `files/` directory. Broad storage
permission and a permanently accessible source URI are unnecessary. Cancelling the
picker returns to setup. Updating the APK with `adb install -r` preserves app data.

Existing installations with a private ROM but no installation receipt automatically
extract and validate their audio on startup. No ROM is copied out of the APK.

## Storage and recovery

Within either platform's application-data root:

- `game/SuperMetroid.smc`: validated, unheadered ROM copy.
- `game/audio/`: extracted audio streams, WAVs, and metadata catalog.
- `game/room-characters/`: stock indexed room-character PNGs and their manifest.
- `game/enemy-tiles/`: 122 indexed ordinary enemy tile sheets, 122 RGB5 palettes,
  Crocomire's two melting images and two BG2 layouts, and their manifest.
- `game/intro-cinematic/`: three stock indexed opening-scene character PNGs and their manifest.
- `game/room-palettes/`: stock RGB5 base room colors and their manifest.
- `game/room-blocks/`: stock JSON for 16x16 visual block compositions and their manifest.
- `game/room-layouts/`: stock BG1/BG2 visual block-reference JSON for every room level source.
- `game/xray-reveals/`: stock visual metatile choices for X-ray block reveals.
- `game/room-backgrounds/`: stock JSON for library-background BG tilemaps and their manifest.
- `game/room-art-index.json`: read-only area/room ID guide to installed art files and state variants.
- `game/installation.json`: extraction format version and ROM identity.
- `SuperMetroid.ini`, `SuperMetroid.save.json`, `debug-states/`, and
  `input-recordings/`: player data, outside the replaceable game directory.

Setup stages and validates all generated content before publishing it. A complete
installation is reused, including valid custom sample replacements. Missing or damaged
audio is regenerated from the installed ROM; this restores stock audio. Player data is
preserved. An interrupted directory swap is recovered on the next launch. Concurrent
installers cannot modify the same installation.

## Command line and verification

From the repository root:

```powershell
# Same installation used by the desktop application; optional final argument overrides its root.
dotnet run --project csharp/src/SuperMetroid.DebugRunner -- assets install "C:\ROMs\Super Metroid.smc"
# Extract just runtime audio into a chosen directory.
dotnet run --project csharp/src/SuperMetroid.DebugRunner -- assets audio-rom "C:\ROMs\Super Metroid.smc" "C:\private\audio"
# Synthetic validation, or full integration when a private ROM is supplied.
dotnet run --project csharp/src/SuperMetroid.IntegrationVerification -c Release -- --asset-import
dotnet run --project csharp/src/SuperMetroid.IntegrationVerification -c Release -- --asset-import "C:\ROMs\Super Metroid.smc"
# Check room-character, palette and visual-block stock import, edits, repair and invalid overrides.
dotnet run --project csharp/src/SuperMetroid.Verification -c Release -- --room-artwork-installation "C:\ROMs\Super Metroid.smc"
# Check all opening character PNGs, live VRAM replacement, and stock repair.
dotnet run --project csharp/src/SuperMetroid.Verification -c Release -- --intro-cinematic-artwork "C:\ROMs\Super Metroid.smc"
```

The integration verifier accepts an optional second argument containing reference audio
to compare every extracted file byte-for-byte. It also tests non-seekable document streams,
header normalization, invalid input, cancellation, asset repair, interrupted publication,
save preservation, and booting the production Android session from the installed layout.
Legacy raw-data/PNG/map extraction commands remain developer tools; normal setup does not
require their input directories. Keep all ROMs and generated game resources out of Git.

## Opening-cinematic character PNG overrides

The opening cinematic installs three indexed 4-bpp sheets under `game/intro-cinematic/`:
`intro-background-characters.png` (256x256), `intro-object-characters.png` (256x64),
and `intro-cinematic-object-characters.png` (256x72). Copy any sheet to the same filename
under `overrides/intro-cinematic/` and edit its pixel indexes (0 through 15). The
two object sheets follow cartridge upload order; the cinematic sheet overwrites the
last 1 KiB of the fixed intro sheet in VRAM. Restart to select edits; loading a
debugger state rebinds the current sheets. Palette colors, scene tilemaps, sprite
compositions, scrolling, and timing are not changed by these PNGs. Stock files and
`intro-artwork.json` are validated and repaired from the installed ROM; overrides
survive repair and application updates. Invalid overrides fail with their paths
instead of silently falling back.

The same directory also contains `intro-background-page-0.json` through
`intro-background-page-3.json`. Each file describes one ordered 32x32 BG tilemap page
with tile column/row, palette, priority and flip fields. Copy an individual page
under `overrides/intro-cinematic/` to edit its placement without changing character
pixels or scene timing. These pages are reloaded after debugger-state restoration.
`intro-portrait-tilemap.json` and `intro-initial-narration-tilemap.json` use the same
32x32 schema. The initial narration page applies only before the first illustrated
page begins; restoring a later debugger state preserves its live typewriter text.

The Ceres approach also installs `ceres-flight-mode7-characters.png` (128x128 indexed),
`ceres-flight-object-characters.png` (256x128 indexed), and
`ceres-flight-mode7-maps.json` (32x24 front and rear views). Override these under the
same `overrides/intro-cinematic/` directory. The two Mode-7 maps share one character
sheet; the game's camera, scale, rotation, actor movement and letter timing remain code.

Ceres destruction reuses those character sheets and the approach's front/rear maps.
Its two station views and clear-map slice are in `ceres-destruction-mode7-maps.json`
(three ordered 32x24 views). The following planet reveal uses
`zebes-reveal-tilemap.json` (one 32x32 BG page) and
`zebes-reveal-characters.png` (256x128 indexed). Each can be overridden separately
under `overrides/intro-cinematic/`; restored debugger states rebind these visuals
without restarting the destruction or descent timeline. Explosion, actor and
camera behavior remain compiled cartridge logic.
The ending's ship flyaway also reuses the Ceres Mode-7 character PNG and the front
view of the Ceres flight map, with its original sixteen-frame upload cadence.

The ending's two escape views and planet-explosion backdrop are installed separately
under `game/ending-mode7/`. Each scene has an indexed 128x128 character PNG and a
128x64 ordered tile-index map JSON (`ending-escape-a-*`, `ending-escape-b-*`, and
`ending-planet-explosion-*`). Put a same-named file under `overrides/ending-mode7/`
to replace just that stream. The map describes the native low-byte lane, which is
repeated across the two Mode-7 map halves; the PNG supplies the high-byte character
lane. These edits do not change ship/cloud sprites, palettes, explosion timing,
camera transforms or scene handoffs. A restored debugger state rebinds the current
files, retaining any flyaway chunks the cartridge has already uploaded.
The reward-jump icon has its own `post-credits-icon-map.json` (128x128 tile
references) and `post-credits-icon-characters.png` (128x128 indexed pixels)
under `game/ending-mode7/`. The runtime interleaves these files for the original
sixteen queued uploads; same-named files under `overrides/ending-mode7/` replace
either half without changing the jump or shooting sequence.

Eleven ending/credits character streams are installed under `game/ending-objects/`:
`ending-cloud-characters.png`, `ending-explosion-objects.png`, and four
`ending-explosion-fragment-*.png` sheets, plus `credits-waiting-samus.png`,
`post-credits-shooting.png`, and `post-credits-suitless-samus.png`. The waiting
sheet also supplies the two suited reward variants. The waiting scene's ordered
BG2 tile references are `credits-waiting-tilemap.json` in the same directory.
Two small cartridge tile uploads, `post-credits-tile-fragment-a.png` and
`post-credits-tile-fragment-b.png`, are also installed as indexed PNGs.
Copy any indexed PNG or that JSON file to `overrides/ending-objects/` with the
same filename to replace it without changing scene mechanics.
The planet-explosion upload still overlays the four fragments after its main sheet,
then restores the already-installed ending font over the overlapping font region.
Sprite placement, animation scripts, palettes and scene timing remain compiled code.

## Room-character PNG overrides

Setup extracts the shared CRE characters, each distinct graphics-set character
stream, and the Tourian statue-ghost characters to `game/room-characters/*.png`.
The ghost sheet is `room-characters-87AD64.png`. These are indexed 8x8 tile sheets, not
screenshots: pixel indexes must remain 0 through 15, dimensions must remain fixed,
and the neutral PNG palette is only a preview. The room palette and metatile map
still supply the on-screen colors and arrangement.

To find a room's files, open `game/room-art-index.json` and search for its
`roomId`, such as `00/00` for Landing Site. Each room lists a `default` state and,
when applicable, numbered alternate states selected by in-game conditions. The
entry gives the relative character PNG, visual block JSON, palette JSON, and
possible background-art files. Background lists include door-dependent
alternatives, so a listed file is not necessarily visible on every entrance.
The `sharedCharacters` and `sharedBlocks` fields identify the common CRE art.
`scrollingSkyArtwork` lists the seven shared sky pages used by the streaming path.
Files are shared across rooms: an override can affect every room that cites it.
Use the listed filename in the corresponding `overrides/` subdirectory; do not
edit the index, which is regenerated when stock content is repaired.

Landing Site's dedicated setup path uses these same installed CRE and area
sheets, plus the installed visual block definitions. Its BG1/BTS/BG2 level
allocation remains cartridge-backed because those words also drive collision
and placement; artwork overrides do not edit room geometry.

Copy a sheet to the matching filename under `overrides/room-characters/`, edit it,
and restart. The selected sheet is loaded on desktop and Android; debugger-state
loads rebind it for subsequent room loads, though an already-captured VRAM frame
may retain its saved pixels until the next room load. Do not edit stock
files or `room-characters.json`: stock hashes are validated and repaired from the
installed ROM, while overrides survive repair and updates. Invalid override PNGs
produce a path-specific load error rather than silently falling back to stock.

Ordinary room-enemy tile uploads and OBJ colors have their own files in
`game/enemy-tiles/`. Each `enemy-XXXX-tiles.png` and matching
`enemy-XXXX-colors.json` pair is identified by the enemy definition's stable
four-digit cartridge ID. Copy either file to `overrides/enemy-tiles/` with the
same name. PNG pixels must remain indexed 0..15; its diagnostic PNG palette is
only a preview. The JSON contains exactly sixteen RGB5 colors, each channel
0..31. Stock files and `enemy-tiles.json` are regenerated and hash-checked;
overrides survive repair and updates. Enemy VRAM/CGRAM destinations,
health, and AI are unchanged by these tile/color edits. This covers
the ordinary bank-$B4 room graphics-set uploads, not dynamic boss BG2 art or
enemy-projectile sheets. Restart to load edits; a saved in-room VRAM image may
retain its old pixels until the next room load.
The same directory contains `enemy-compositions.json` with named Boyon,
Cacatac, Boulder, and Atomic visual frames. Copy it to `overrides/enemy-tiles/` to edit a frame's ordered OAM
parts: `offsetX`, `offsetY`, `tileColumn`, `tileRow`, `size`, `priority`, `palette`,
`flipX`, or `flipY`. Frame timing and selection, enemy hitboxes, movement, and
damage remain engine-owned. The stock JSON is hash-checked; malformed overrides
fail with a load error. Other enemy families still use their ROM spritemaps until
their visual frames are extracted.
Crocomire's first and second melting images are the separate indexed files
`crocomire-melt-first.png` and `crocomire-melt-second.png` in the same directory.
Their matching 16×16 BG2 layouts are `crocomire-melt-first-tiles.json` and
`crocomire-melt-second-tiles.json`. Copy any of these four files to
`overrides/enemy-tiles/` and edit the PNG's indexed pixels or a JSON cell's
`tileIndex`, `palette`, `priority`, `flipX`, or `flipY`. The native erase order,
distortion timing, transfer destinations, and collision stay fixed.
Pixels beyond each native image's written byte range are reserved and must
remain zero. Stock melt sheets are hash-checked and repaired with the other
enemy art, while valid overrides survive repair and application updates.
Base room colors are separately extracted into `game/room-palettes/*.json`.
Copy a file to `overrides/room-palettes/` to replace its 128 RGB5 colors, keeping
`version: 1` and each `red`, `green`, and `blue` component in 0..31. Restart to
load the edit. Stock palette hashes and user overrides follow the same repair
rules as the PNG sheets. Palette-FX scripts may animate or replace these base
colors later in the room; this file does not edit those effects or their timing.

Visual 16x16 blocks are installed as `game/room-blocks/*.json`. Each ordered
`blocks` entry names its four 8x8 children (`topLeft`, `topRight`, `bottomLeft`,
`bottomRight`), with `tileColumn`, `tileRow`, `palette`, `priority`, `flipX`, and
`flipY`. Copy the desired file to the same filename under `overrides/room-blocks/`,
edit it, and restart. Preserve block order and count: room level data refers to
these indices. `room-blocks-cre.json` holds the common Zebes blocks; other filenames
encode their graphics-set source. These files control visual tile composition only,
not collision type, BTS behavior, room placement or palette animation. Stock hashes
are verified and repaired; an invalid override fails with its path instead of
silently reverting to stock.

Library-background tilemaps are installed as `game/room-backgrounds/*.json`.
Each file contains one or two ordered 32x32 pages of 8x8 tile references with
`tileColumn`, `tileRow`, `palette`, `priority`, `flipX`, and `flipY`. Copy a file
to `overrides/room-backgrounds/` under the same name to edit it, then restart.
The library-background command sequence, WRAM staging, VRAM transfer order and
door conditions remain engine behavior, not editable data. These files cover
the 58 compressed room BG tilemaps. Seven contiguous scrolling-sky pages are
installed alongside them as `scrolling-sky-*.json`; copies under
`overrides/room-backgrounds/` change both door-selected and per-frame sky rows.
All 68 retail library-background command lists, including Landing Site's six
door-to-sky choices, are compiled engine definitions; only the referenced
tilemaps and character sheets are replaceable artwork.
The bank-$88 sky pointer arithmetic, scrolling rates and VRAM timing remain
engine behavior. Kraid's direct HUD-character upload also uses the existing
`game/maps/hud-tiles.png` and `overrides/maps/hud-tiles.png` artwork instead of
rereading those characters from the ROM. The Tourian statue-ghost library upload
likewise uses the installed room-character sheet instead of the raw ROM source.
Stock hashes and override repair follow the same rules as the room-character
sheets.

Room-level visual arrangements are installed as 246 `game/room-layouts/level-*.json`
files. Find the active file in `game/room-art-index.json` under a room state's
`layout` field, copy it to the same filename under `overrides/room-layouts/`,
edit, and restart. `widthInBlocks` and `heightInBlocks` describe the complete
native allocation, including any authored rows beyond the visible camera.
`foregroundVisualWords` and `backgroundVisualWords` are row-major arrays of
16x16 block references. Each decimal word uses bits 0-9 for the index into the
combined CRE/area block table, bit 10 for horizontal flip, and bit 11 for
vertical flip. Only values 0..4095 are valid; collision bits cannot be placed
in this presentation file. BG2 entries that the ROM leaves uninitialized are
exported as zero visual references. The renderer receives edited references,
while collision, BTS, slopes, hazards and subsequent PLM block writes continue
to use the unmodified native level allocation. Stock files are hash-checked;
overrides survive stock repair and invalid values fail with the offending path.

X-ray reveal art is installed as `game/xray-reveals/reveals.json`. Copy that
file to `overrides/xray-reveals/reveals.json`, edit its `topLeft`, `topRight`,
`bottomLeft`, or `bottomRight` metatile indices, and restart. Each entry names
the collision type and BTS values that select it, plus its read-only copy
shape. The installer groups identical cartridge rules to keep the file short;
changing the rule keys, shape, count, or unused operands is rejected. The
visual indices may be 0..4095, but each selected metatile must exist in the
room's combined CRE/area block definitions when X-ray is used. The compiled
cartridge lookup still determines whether a block is revealed, how many
blocks are copied, extension traversal, and Brinstar-only behavior. The
The same file contains eight `itemMetatiles` (the four rotating item graphics
slots followed by four fixed slots) and `rooms` with special reveal tiles.
Each room tile's `x`, `y`, and `word` are visual-only: the first two are block
coordinates, and the word's low ten bits select a metatile while bit 11 selects
the cartridge's vertical row swap. Keep room pointers and record counts intact;
they identify which compiled room state owns each list. Installed hosts use
these entries without reading the item draw table or room overlay records from
the ROM during X-ray setup. Stock hashes are checked and user overrides survive
stock repair.

These visual-layout files do not yet replace the runtime ROM source for the
native collision/BTS allocation. The room-ID guide makes the files discoverable,
but shared sheet and palette filenames still encode source identities; stable
semantic filenames and ROM-free mechanics remain broader migration work.

## Projectile composition and beam PNG overrides

Installed hosts extract `game/projectiles/projectile-compositions.json` under the
player-data root. To edit it, copy the complete file to
`overrides/projectiles/projectile-compositions.json` under that same root. Do not
edit the stock file or its manifest: stock hashes are checked and damaged stock
is repaired during setup. External overrides survive stock repair/reinstallation.

Each `sprite_XXXX` entry exposes part offsets, tile row/column, size, flips, palette
index and priority. Keep every entry and the version field. These are compositions
of graphics. Damage, collision radii and
timing are not editable here. Trails, charge flares and Grapple visuals are not
covered by this composition file.

`projectile-frame-bindings.json` in the same directory selects which existing
`sprite_XXXX` composition each of the 805 timed beam, missile, Super Missile and
bomb instruction frames draws. Copy it to `overrides/projectiles/` to change a
visual frame choice. Keep all `frame_XXXX` entries and select only sprite IDs
present in `projectile-compositions.json`; duration, trail cadence, collision
radii, damage and instruction flow remain compiled gameplay data. The stock
file is hash-checked, and edits are included in recorded content identity.

Trail appearance uses `projectile-trails.json` in the same stock/override
directories. Copy the complete file before editing tile row/column, palette,
priority or flips. Timing and trail movement are intentionally not editable.
This selects existing trail graphics; separate trail PNG replacement is not yet
supported. The Mother Brain intro flashback shares the selected projectile,
explosion and trail compositions, including after loading a debugger state.

Beam sheets `beam-00-tiles.png` through `beam-0B-tiles.png` are also installed in
`game/projectiles`. Copy any sheet to `overrides/projectiles` to replace that beam
combination's artwork. Keep its 64x8 indexed format and pixel indices 0 through 15.
PNG palette colors are diagnostic: gameplay uses the separately selected beam palette.
These sheets do not replace missile, bomb, trail, flare or Grapple artwork.

Copy `game/projectiles/beam-palettes.json` to `overrides/projectiles/beam-palettes.json`
to edit ordinary beam colors. Keep all twelve selections and sixteen colors per
selection; RGB components range from 0 to 31. Charge/Hyper animation colors are
not controlled by this file. Loading a state refreshes ordinary colors at the
next accepted display update, but preserves an active Crystal Flash or Hyper
palette. Crystal Flash completion restores the selected override normally.

Restart to load edits. Loading a debugger state retains the current session's
selected composition and beam catalogs rather than restoring old artwork from the state.
Startup logs stock and selected hashes. Invalid overrides fail loudly; removing
the override restores stock on restart. Explicit developer-ROM sessions outside
the installed-content workflow retain ROM-backed rendering.

## Verified build

On September 10, 2026, the Release AOT APK was installed as the isolated package
`org.supermetroid.csharp.romimporttest` on a Retroid Pocket Classic. First-run setup
accepted the supplied ROM and reached the game menus; player testing confirmed the
game runs with audio. The existing game package and its saves were left intact.
The import integration suite and full Core verification suite passed using private
fixtures. The APK and desktop publish output were inspected and contained no ROM,
SPC upload streams, audio manifest, or WAV samples.
