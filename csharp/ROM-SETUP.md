# Install game data from your ROM

Desktop and Android use the same `SuperMetroid.AssetExtraction` library. Application
packages contain no ROM or extracted audio. Initial setup imports the supported ROM
into a private installation and extracts the current presentation catalogs, including
indexed PNGs, JSON, and 112 PCM WAVs. No upstream disassembly checkout is needed.

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

If the installed ROM copy is absent but every extracted asset passes validation,
the desktop host starts using installed presentation and compiled mechanics.
Core has no cartridge reader, payload, decompressor or import dependency;
unresolved definitions cannot fall back to a ROM read. This boundary is not a
claim that every gameplay or presentation behavior has passed parity acceptance.
Setup retains the private ROM copy by default and never removes it automatically. Without that
copy, damaged or missing extracted assets require re-importing the source ROM.

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

Windows automatically captures each session's console output in a timestamped ZIP under
`logs/` beside `SuperMetroid.Game.exe` (not AppData or the current working directory).
After closing the game, send that session's ZIP when reporting a failure. It contains
`session.log` with the version, startup messages and complete exception diagnostics; it
does not include ROMs, artwork, saves or input recordings. Fatal errors publish the ZIP
before the console's Enter prompt. A forcibly killed process leaves its flushed `.log`
instead. The executable folder must be writable. Logs may include personal paths; review
before posting publicly. Logging is always enabled and does not require GitHub reporting.

## Android

Install the APK and tap **Choose ROM**. Use Android's document picker to select your
ROM from local storage or a document provider. The app reads the selected stream once,
validates it, and installs a private copy under its `files/` directory. Broad storage
permission and a permanently accessible source URI are unnecessary. Cancelling the
picker returns to setup. Updating the APK with `adb install -r` preserves app data.

Android also accepts a complete, validated extracted installation when its private
ROM copy is absent. Its ROM-free play scope has the same experimental limitation.

Existing installations with a private ROM but no installation receipt automatically
extract and validate their audio on startup. No ROM is copied out of the APK.

## Storage and recovery

Within either platform's application-data root:

- `game/SuperMetroid.smc`: validated, unheadered ROM copy retained by default;
  optional only while all extracted assets remain complete and valid.
- `game/audio/`: extracted audio streams, WAVs, and metadata catalog.
- `game/maps/gameplay-hud.json`: the HUD's layout, the screen pixel each of its pictures shows at; the HUD's
  tiles, whose cells are the original's packed tile placements, are import data the import pictures the HUD from.
- `game/room-characters/`: stock indexed room-character PNGs and their manifest.
- `game/samus-body/`: what the game binds of Samus's body: each pose's art origin and the body frames her body
  sheet pictures (`samus-body.json`), her death steps and the arm cannon's layout. Her body pages, selectors,
  compositions, atmospheric lists and cover appearances are import data the import pictures her sheets from.
- `game/enemy-tiles/`: the enemies' compositions and extended frames, the bosses' BG2 layouts and
  tile-reference maps, Kraid's eye colours, and their manifest. The enemies' characters and the enemy, boss
  and area colour documents are import data, which only pre-colour the sheets and are not installed.
- `game/intro-cinematic/`: three stock indexed opening-scene character PNGs and their manifest.
- `game/standard-objects/standard-object-characters.png`: indexed 4-bpp common
  gameplay sprite sheet, edited in place.
- `game/room-layouts/`: stock BG1/BG2 visual block-reference JSON for every room level source.
- `game/room-level-streams/`: stock-only native level allocations (collision, BTS and
  streaming baseline) for every room level source. These files are not artwork.
- `game/xray-reveals/`: stock visual metatile choices for X-ray block reveals.
- `game/room-art-index.json`: read-only area/room ID guide to installed art files and state variants.
- `game/installation.json`: extraction format version and ROM identity.
- `SuperMetroid.ini`, `SuperMetroid.save.json`, `debug-states/`, and
  `input-recordings/`: player data, outside the replaceable game directory.

Setup stages and validates all generated content before publishing it. A complete
installation is reused, including valid custom sample replacements. Missing or damaged
audio is regenerated from the installed ROM; this restores stock audio. Player data is
preserved. An interrupted directory swap is recovered on the next launch. Concurrent
installers cannot modify the same installation.

## Normal Ceres checkpoints

The cartridge's normal file-select save includes a startup mode, not just an
area and station. Initial Ceres arrival saves `$1F` (`$8B:C100-$C116`). After
the escape elevator finishes its blackout, the cartridge saves `$22`
(`$82:83D6-$83E3`); loading that checkpoint resumes the destruction cinematic,
not the escaping station. Landing on Zebes replaces it with the ordinary `$05`
checkpoint (`$A2:A9A0-$A9B8`). These three writes and the file-select dispatch
are implemented explicitly. No exception handler creates a normal checkpoint.

Older port builds incorrectly left `$05` on the Ceres checkpoints. Loading does
not guess a replacement mode from boss flags or silently rewrite these saves.
An explicitly approved one-off repair can correct the mode in a separate JSON
copy, leaving the active file and all other saved data untouched.

## Shared palette JSON validation

Room and gameplay base palettes, map palettes and highlight cycles, title/opening/Ceres
palettes, ending backdrops, Mother Brain colors, and room palette effects reject unknown
fields and duplicate names at every object level. This includes repeated world-map
selections and RGB components. An invalid file reports an error; it is not silently
accepted with a field ignored or overwritten.

Room static and gameplay base palettes retain their historical case-insensitive field
names, but two spellings of the same field in one object are ambiguous and rejected.
The other listed formats retain their case-sensitive names. RGB5 components must all
be present and range from 0 to 31; keep the documented row and palette sizes.

## Room and cinematic JSON validation

The installed map/menu/title bundle also rejects duplicate properties recursively.
This covers area maps and labels, pause layouts, HUD and message boxes, file select,
options, narration and ending text/credits, title compositions and scanline gradients.
Repeating even an identical value is an error, not a last-value-wins edit. Existing
schema versions, required fields and field-name casing rules are unchanged. The
stock map manifest and station-reveal document use the same validation boundary;
they are required stock metadata, not editable exploration rules.

Room background/sky and metatile manifests, room visual layouts and X-ray reveal
documents reject unknown fields, duplicate names and missing required fields.
This includes nested entries and X-ray coordinates: omitting `x`, `y` or a visual
operand is an error, not an implicit zero. A supplied zero remains valid where
the format permits it. Errors identify the installed file; an invalid edit is
not replaced or erased.

Metatile compositions, room-FX BG3 pages, intro eye rectangles, the intro divider
and Ceres warning tilemaps also reject unknown and duplicate fields before
compiling tile words. Existing field-name casing rules are unchanged: a format
that previously accepted unambiguous case differences still does, but two
spellings of the same property in one object are rejected. These checks do not
change valid stock files, format versions, drawing or gameplay behavior.

## Command line and verification

From the repository root:

```powershell
# Same installation used by the desktop application; optional final argument overrides its root.
dotnet run --project csharp/src/SuperMetroid.DebugRunner -- assets install "C:\ROMs\Super Metroid.smc"
# Extract just runtime audio into a chosen directory.
dotnet run --project csharp/src/SuperMetroid.DebugRunner -- assets audio-rom "C:\ROMs\Super Metroid.smc" "C:\private\audio"
# Synthetic validation, or full integration from the repository-root Super Metroid.smc.
dotnet run --project csharp/src/SuperMetroid.IntegrationVerification -c Release -- --asset-import
dotnet run --project csharp/src/SuperMetroid.IntegrationVerification -c Release -- --asset-import-rom
# Check room-character, palette and visual-block stock import.
dotnet run --project csharp/src/SuperMetroid.Verification -c Release -- --room-artwork-installation "C:\ROMs\Super Metroid.smc"
# Check all opening character PNGs against the cartridge.
dotnet run --project csharp/src/SuperMetroid.Verification -c Release -- --intro-cinematic-artwork "C:\ROMs\Super Metroid.smc"
# Constructed palette JSON contracts and exact BG/OBJ pixels; no retail ROM needed.
dotnet run --project csharp/src/SuperMetroid.Verification -c Release -- --palette-json-contracts
# Constructed room/environment/intro tile-word and malformed-document checks; no ROM needed.
dotnet run --project csharp/src/SuperMetroid.Verification -c Release -- --tilemap-json-contracts
# Manifest/layout/X-ray admission checks using only an already-extracted installation.
dotnet run --project csharp/src/SuperMetroid.Verification -c Release -- --room-asset-json-contracts "C:\private\SuperMetroid"
```

The integration verifier accepts an optional second argument containing reference audio
to compare every extracted file byte-for-byte. It also tests non-seekable document streams,
header normalization, invalid input, cancellation, asset repair, interrupted publication,
save preservation, and booting the production Android session from the installed layout.
Legacy raw-data/PNG/map extraction commands remain developer tools; normal setup does not
require their input directories. Keep all ROMs and generated game resources out of Git.

## Samus body PNG and frame selectors

Samus's body, atmosphere, overlay and death pictures are installed whole in the sprite sheets. The
body pages, frame selectors, pose-half compositions, the atmospheric lists and the arm cannon's cover
appearances they are composed from are the import's data and are not installed (#1636, #1638).
`game/samus-body/samus-body.json` holds, for each pose, its signed art origin (`graphicsYOffset`) and
each animation frame her body sheet pictures with how many pixels above her drawn point that frame's
body origin sits; the body sheet's frames are named from it. Edit the rises to move a frame's picture
without changing movement or collision.
`samus-death-steps.json` holds what the game works with of her death: the death step
each of the nine explosion frames shows and the 22 colours the room whitens to, each
an RGB colour with channels 0..31. Her suited and suitless death colours only
pre-colour her death shades: they are import data, not installed.
`samus-arm-cannon.json` in the same directory holds each pose's arm-cannon cover placement: its
`drawOrder` (`Hidden`, `BeforeBody` or `AfterBody`), the aim `direction` whose cover picture it shows and
an optional `laterDirection` shown after animation frame zero, and the cover's pixel `displacements` from
Samus at each animation frame the pose shows. Keep a placement for every pose and a displacement for every
frame listed. It does not change the HUD selection, cover opening/closing cadence, projectile origin,
damage, or equipment behavior.

Edited files are validated on load. Run the extractor again to restore the
originals. These files do not change pose timing, movement, collision, or
equipment. The same installed artwork is used by normal gameplay, attract demos,
and the opening flashbacks.

## Opening-cinematic character PNGs

The opening cinematic installs three indexed 4-bpp sheets under `game/intro-cinematic/`:
`intro-background-characters.png` (256x256), `intro-object-characters.png` (256x64),
and `intro-cinematic-object-characters.png` (256x72). Edit a sheet's pixel indexes (0 through 15)
in place. The
two object sheets follow cartridge upload order; the cinematic sheet overwrites the
last 1 KiB of the fixed intro sheet in VRAM. Restart to select edits; loading a
debugger state rebinds the current sheets. Palette colors, scene tilemaps, sprite
compositions, scrolling, and timing are not changed by these PNGs. An invalid file fails
with its path; running the extractor again restores the originals.

The opening's layouts are import data, never installed: the four scene pages
`intro-background-page-0.json` through `intro-background-page-3.json`, the portrait
`intro-portrait-tilemap.json`, the first narration card `intro-initial-narration-tilemap.json`,
the subtitle line `intro-final-text-divider.json`, the portrait's four blinking-eye rectangles
`intro-samus-eye-frames.json` and the Zebes reveal's `zebes-reveal-tilemap.json`. Their cells are
the original's packed tile placements; the import composes the scene, narration and planet
pictures from them, and the installation holds those pictures, which a player edits in place.
The cartridge's blink script, frame durations and portrait placement remain compiled behavior.
`intro-narration-palette.json` in the same directory contains the opening scene's
256 native-precision RGB5 colors, which only the import reads to pre-colour the opening's
sheets; edit it and reinstall to recolor them without changing narration timing or scene
transitions. `intro-backdrops.json` holds the colours the opening shows where nothing is
drawn: the narration's `opening` backdrop and the Ceres scenes' `ceresFlight` backdrop. The
game reads these colours and neither palette document; edit them in place, each channel 0–31.
`intro-caret-sprites.json` contains the illustrated-page text caret's visible
OAM composition. Its blink alternates this one frame with no sprite; the three
nearby ROM spritemaps are not caret frames. Sprite offsets, tile regions,
priority, flips, and an optional fixed palette are editable. A null palette
inherits the caret's live color. Position and blink durations remain code.
`intro-next-page-arrow-sprites.json` contains the three OAM compositions of the
next-page arrow the narration shows below the text at each page's input wait
when Japanese text is selected. Sprite offsets, tile regions, priority, flips,
and an optional fixed palette are editable; its position, frame durations, and
the pause while a second subtitle holds the page remain code.
`intro-mother-brain-sprites.json` contains the three nine-part Mother Brain
frames used in the opening flashback. Their OAM offsets, tile references,
priority, flips, and optional palette are editable; missile damage, hitboxes,
explosion timing, and the page-two handoff remain compiled behavior.

The Ceres approach also installs `ceres-flight-mode7-characters.png` (128x128 indexed),
`ceres-flight-object-characters.png` (256x128 indexed), and
`ceres-flight-mode7-maps.json` (32x24 front and rear views), plus
`ceres-flight-palette.json` (all 256 native-precision RGB5 colors, read only by the import) and
`ceres-flight-sprites.json` (the six star, asteroid, station, and vortex OAM frames).
The sprite JSON exposes offsets, tile references, priority, flips, and optional
palette selections; it does not change actor paths or frame timing. Edit these files in
place. The two Mode-7 maps share one character
sheet and palette; the game's camera, scale, rotation, actor movement and letter timing
remain code. Palette edits take effect when the import recolours the sheets.
`ceres-flight-actors.json` exposes the five rear-view actors' initial 16-bit
screen X/Y positions, in large-asteroid, station, small-asteroid, vortex, and
rear-stars order. Their stock values come from the pinned cartridge's initializer
operands. Keep the IDs and order; edit only `x`/`y`, then restart the scene. Actor speed, wrapping,
instruction timing, and scene handoffs remain compiled.

Ceres destruction reuses those character sheets and the approach's front/rear maps.
Its two station views and clear-map slice are in `ceres-destruction-mode7-maps.json`
(three ordered 32x24 views). The following planet reveal's picture is composed by the import
from its layout and characters, both import data; restored debugger states rebind these visuals
without restarting the destruction or descent timeline. Explosion, actor and
camera behavior remain compiled cartridge logic.
`ceres-destruction-sprites.json` adds the 23 distinct station-blast, explosion,
planet, star-sheet, and title OAM frames. Destruction actors also use shared frames
from `ceres-flight-sprites.json`. Both files support visual part edits without
changing the native actor instruction timing, motion, or scene handoff.
`ceres-destruction-actors.json` holds the initial 16-bit screen X/Y positions
of the large asteroid, small asteroid, and vortex behind the station blast.
Keep those three IDs and their order, and edit only `x`/`y`. The stock values come from the cartridge-checked
actor initializers; horizontal motion, wrapping, animation, and phase timing
remain code-owned. Restart the scene to apply a placement edit.
`ceres-zebes-reveal-actors.json` holds the initial 16-bit screen X/Y coordinates
for the planet, four star sheets, and PLANET ZEBES title. Keep the six IDs and
their order; edit only `x`/`y`, then restart
the scene. These values are read from the pinned cartridge's actor initializer
operands. The slide acceleration, visibility schedule, deletion, and phase
handoff remain compiled. Missing, reordered, or malformed entries fail loudly.
The ending's ship flyaway also reuses the Ceres Mode-7 character PNG and the front
view of the Ceres flight map, with its original sixteen-frame upload cadence.

The ending's two escape views and planet-explosion backdrop are installed separately
under `game/ending-mode7/`. Each scene has an indexed 128x128 character PNG and a
128x64 ordered tile-index map JSON (`ending-escape-a-*`, `ending-escape-b-*`, and
`ending-planet-explosion-*`). Edit a file in place to change just that stream. The map describes the native low-byte lane, which is
repeated across the two Mode-7 map halves; the PNG supplies the high-byte character
lane. These edits do not change ship/cloud sprites, palettes, explosion timing,
camera transforms or scene handoffs. A restored debugger state rebinds the current
files, retaining any flyaway chunks the cartridge has already uploaded.
The reward-jump icon has its own `post-credits-icon-map.json` (128x128 tile
references) and `post-credits-icon-characters.png` (128x128 indexed pixels)
under `game/ending-mode7/`. The runtime interleaves these files for the original
sixteen queued uploads; editing either file in place changes that half without changing the jump
or shooting sequence.

Ending/credits character streams are installed under `game/ending-objects/`:
`ending-cloud-characters.png` and `ending-explosion-objects.png`, plus
`credits-waiting-samus.png`, `post-credits-shooting.png`, and
`post-credits-suitless-samus.png`. The waiting sheet also supplies the two suited
reward variants. The planet explosion's four maps are ordered tile references in
`ending-explosion-fragment-*.json`, and the waiting scene's ordered BG2 tile
references are `credits-waiting-tilemap.json` in the same directory.
`ending-cloud-sprites.json` contains the six editable atmospheric-cloud OAM
compositions, including part offsets, tile references, flips, priority, and
optional palette selections. `ending-explosion-sprites.json` contains the sixteen
editable planet-damage, flash, lava, glow, starfield, silhouette, and afterglow
OAM compositions. Their actor instruction lists, movement, and scene callbacks
remain compiled game logic rather than artwork.
`ending-completion-text-sprites.json` contains the 56 progressive operation,
success, clear-time, digit, and colon OAM compositions. Editing these parts
does not alter the cartridge-matched typewriter pacing or credits handoff.
`ending-reward-sprites.json` contains the 37 suitless, helmetless, and armored
Samus pose compositions used across the reward reveal, gesture, jump, landing,
and shot. Their shared actor instructions and physical handoff remain compiled
game logic.
`ending-logo-sprites.json` contains the eight final assembling-logo OAM
compositions: two S halves and three frames for each circle half. Logo motion,
wrap timing, and palette-crossfade behavior remain compiled game logic.
The small transformation character upload is installed as the indexed PNG
`post-credits-tile-fragment-a.png`, and its map as the ordered tile references in
`post-credits-tile-fragment-b.json`. The
final assembling logo uses `post-credits-logo-tiles.png` and the ordered BG2
references in `post-credits-logo-map.json`; its six upload positions and timing
are fixed game logic. Edit any indexed PNG or JSON file in place to change it without changing
scene mechanics.
The planet-explosion upload still overlays the four fragments after its main sheet,
then restores the already-installed ending font over the overlapping font region.
Other sprite placement, animation scripts, palette-FX mechanics and scene timing remain compiled code.

The ending's colours are pre-coloured into its installed sprite sheets; edit a sheet
in place to change them. Each ending scene's backdrop, the solid colour it shows
behind every layer, is installed as native-precision RGB5 JSON in
`game/ending-backdrops/ending-backdrops.json`: the escape, explosion (which the
flyaway keeps), credits and post-credits scenes', and `spaceBehindExplodingPlanet`,
the backdrop the space behind the exploding planet shows at each frame of its
colour effect. Edit it in place to change a backdrop without changing the scenes'
fade cadence or palette-FX programs. A malformed file fails loudly where it is read.

The ending's animated explosion, cloud, gunship and post-credits glare colors, like
every room and cinematic palette effect's, are pre-coloured into the sheets each
effect frame shows. The import reads them from its import data's
`room-palette-effects.json`, which is not installed. The ending still owns the cartridge
object allocation, cycle timing, color destinations and scene transitions.

Developer dependency boundary: `RoomPaletteFxSystem.Step` requires a non-null
`IPaletteFxColorSource` argument. There is no optional binding API or cached
provider on the interpreter; gameplay, title, Ceres destruction and ending
owners explicitly supply their installed catalog on every execution. Restored
states use the host's current catalog without restarting palette timers. Missing
arguments, wrong types and nullable sources fail compilation (nullable warnings
are errors). This checks the dependency handoff, not the contents of external
JSON: resource loaders still validate those files. Deliberate `null!`/reflection
bypasses fail before the interpreter mutates its slots or CGRAM.
Run the focused compiler contract with
`dotnet run --project csharp/src/SuperMetroid.EnsureVerification -- --palette-fx-contract`.

Mother Brain's rainbow and drained-phase colors are pre-coloured into her sheets; the
import reads them from its import data's `mother-brain-rainbow-palette.json`, which is
not installed. `fakeDeathToGrey` there contains eight three-color frames for the
fake-death descent; the ascent reuses the first three brain colors of each `fromGrey` frame.
Her rainbow beam's backdrop colours, which the game works with, are installed as
`game/maps/mother-brain-beam-colours.json`: `first` is the colour the backdrop shows as the
beam starts, and `cycle` the colours it cycles through while the beam fires. Edit it in
place to change them. The beam window geometry and attack timing remain engine
logic. A debugger-state load rebinds the current color resource.

Mother Brain's separate fake-death room colors are extracted, as import data, into
`mother-brain-room-colors.json`. Its fourteen `flash` rows and
`finalRoom` row each contain two twelve-color source slices; the second slice
is displayed in two CGRAM destinations. `phaseTwoAttack` and
`phaseTwoRearLeg` each contain fifteen nontransparent sprite colors.
`initialGlassShard` and `initialTubeProjectile` contain the two fifteen-color
room-entry sprite palettes. `recoveryLights` contains seven two-slice room-light images shown
after the Baby Metroid cutscene. Edit this file in place to recolor those effects. The cartridge bytecode, flash
cadence, phase handoff, and CGRAM copy destinations remain engine logic.

The `game/maps/room-fx-animated-tiles.png` indexed 2-bpp strip contains the
native lava, acid, rain, and Maridia sand frames, Wrecked Ship's four 32-byte
treadmill frames, the Tourian statue strip, then three 48-byte spores frames.
Edit it in place to change the pixels. Activation, frame order,
cadence, loop control and VRAM destinations remain compiled. Queued NMI
transfers use the installed art rather than reading bank $87. Adding spores does not shift any
existing character or statue transfer alias.

Spores use a ten-tick three-frame loop and camera-anchored quarter-pixel
vertical scroll. Their `room-fx-layer3-tilemaps.json` page is import data, whose cells are the original's packed tile
placements: the import composes the effect pictures from it, and the installation holds the pictures. The
`room-fx-blend-palettes.json` colors can be edited in place. Native source-aware
color math keeps the effect behind BG1 terrain and excludes OBJ palettes
zero through three; it never tints the HUD or enables liquid physics.
Malformed FX artwork/tilemap/palette files fail with the exact path.

`game/maps/room-fx-blend-palettes.json` holds the room-FX blend colors, which the
import pre-colours the effect overlay pictures in; edit it and reinstall to recolor them.

`game/maps/scene-colours.json` holds the colours the game shows that no picture
holds: the title's backdrop, the pause, file-select and each Zebes area's world-map
backdrop, and the Ceres haze's `ceresHazeWhileRidleyLives`/`ceresHazeAfterRidleyDies`
RGB5 tints. The game reads these colours and no palette document. Edit it in place to
change a backdrop or recolor the Ceres haze before or after Ridley's defeat; haze
activation, scanline bands, and fade timing remain compiled. Each channel must be 0–31.
The title, map and menu palette documents are read only by the import, which
pre-colours the screens' sheets from them.

`game/maps/power-bomb-glow-colors.json` contains the 16 pre-explosion and
32 explosion RGB5 glow colours used by Power Bombs, Crystal Flash and the Ceres
explosion. Edit it in place to recolor them. Radius math, window
geometry and cleanup timing remain compiled. Crystal Flash's stock fade
lifetime is separate from the installed colors: black does not end it early,
and brighter colors do not prolong it. Unused color is cleared when its window ends.

`SuperMetroid.RenderVerification --installed-power-bomb-isolation`
checks the installed colours' complete effect lifetimes, state restoration and
software/Direct3D output without importing a cartridge or touching player data.
See the source-access audit for exact coverage and remaining environmental gates.

## Room-character PNGs

Setup extracts the shared CRE characters, each distinct graphics-set character
stream, and the Tourian statue-ghost characters to `game/room-characters/*.png`.
The ghost sheet is `room-characters-87AD64.png`. These are indexed 8x8 tile sheets, not
screenshots: pixel indexes must remain 0 through 15, dimensions must remain fixed,
and the neutral PNG palette is only a preview. The room palette and metatile map
still supply the on-screen colors and arrangement. These sheets are what the room pictures
are composed from. Edit a room's look through its pictures in
the installed `game/room-block-sheets` instead.

To find a room's files, open `game/room-art-index.json` and search for its
`roomId`, such as `00/00` for Landing Site. Each room lists a `default` state and,
when applicable, numbered alternate states selected by in-game conditions. The
entry gives the relative character PNG, visual block JSON, palette JSON, and
possible background-art files. Background lists include door-dependent
alternatives, so a listed file is not necessarily visible on every entrance.
The `sharedCharacters` and `sharedBlocks` fields identify the common CRE art.
`scrollingSkyArtwork` lists the seven shared sky pages used by the streaming path.
Files are shared across rooms: an edit can affect every room that cites it.
Edit the listed file in place; do not edit the index, which is regenerated when
content is extracted again.

Landing Site's dedicated setup path uses these same installed CRE and area
sheets, plus the installed visual block definitions. Its BG1/BTS/BG2 level
allocation comes from the installed stock-only room-level streams because those
words also drive collision and placement; artwork edits do not edit room geometry.

Do not edit stock files or `room-characters.json`: stock hashes are validated and repaired
from the installed ROM.

Ordinary room-enemy tile uploads and OBJ colors are import data, which only the
import composes the enemy sheets from: each `enemy-XXXX-tiles.png` and matching
`enemy-XXXX-colors.json` pair is identified by the enemy definition's stable
four-digit cartridge ID, and neither is installed. PNG pixels must remain indexed 0..15; its diagnostic PNG
palette is only a preview. The JSON contains exactly sixteen RGB5 colors, each
channel 0..31. Enemy VRAM/CGRAM destinations,
health, and AI are unchanged by these tile/color edits. This covers
the ordinary bank-$B4 room graphics-set uploads, not dynamic boss BG2 art or
enemy-projectile sheets. Restart to load edits; a saved in-room VRAM image may
retain its old pixels until the next room load.
Ninja Space Pirates also copy the shared gold-Pirate colors from
`enemy-F413-colors.json` into OBJ palette seven when initialized; the import
pre-colours their gold shade from it.

The ordinary `enemy-ED3F-tiles.png` sheet also supplies Dead Torizo's corpse
and falling-sand rows. `enemy-ED7F-tiles.png` supplies the dead sidehopper,
Zoomer, Ripper, and Skree corpse layouts. Mother Brain's decaying body instead
uses `mother-brain-corpse-tiles.png`, a separate indexed 4bpp source sheet.
The escape typewriter's five character uploads use
`mother-brain-escape-text-tiles.png`, also separate from the ordinary enemy
sheet and editable without changing text timing or placement.
The Baby Metroid, restored Mother Brain attack, and exploded escape-door OBJ
pages are separate `mother-brain-baby-tiles.png`,
`mother-brain-attack-tiles.png`, and `mother-brain-exploded-door-tiles.png`
sheets. Each holds only character pixels; the original page timing and
VRAM destinations remain compiled.
Phase-two leg loading also uses `mother-brain-leg-tiles.png` for its first
eight pages; the following four reuse the attack sheet. Its twelve-record
timing and destination list remain compiled game definitions.
Editing any of these changes those visuals; their row timing, collision, and
decay mechanics remain compiled.

Magdollite's separate four-frame OBJ color cycle is in
the import data's `magdollite-palette-cycle.json`, four RGB5 colors per frame. The cartridge's
eight-tick cadence, frame order, and selected OBJ palette slot stay in code.
The installed game uses this asset for the cycle rather than reading its
colors from the ROM during play.
Work Robot's six four-color records are similarly editable in
the import data's `work-robot-palette-cycle.json`. Its 64/16-tick durations, record wrap, and powered/unpowered state
remain compiled; only the RGB5 words are replaceable.
Crocomire's fight-body restore, initial wall and projectile transfers,
skeleton-arm transfer, and wall-spike transfer are in
the import data's `crocomire-colors.json`. The two initial transfers intentionally contain
seventeen words each, including one word across the next palette boundary.
The white hurt-flash decision, bridge and death phase timing, collision, and
room progression stay in code.
Draygon's opening, normal body/sprite, white-flash and eight health-band colors
are in the import data's `draygon-colors.json`. The
opening transfer includes the first nine colors of the following palette, as
the cartridge's 25-word copy does. Health thresholds, hurt timing and combat
stay in code; edits affect only the displayed RGB5 colors.
Phantoon's eight health palettes, fade-out target and 112-color Wrecked Ship
power-on target are in the import data's `phantoon-colors.json`. Health-band selection, interpolation, hurt
timing and boss/death transitions stay in code.
The Wrecked Ship and Lower Norfair Chozo statue palette pairs, plus the
n00b-tube crack palette pair, are in
the import data's `chozo-and-tube-colors.json`. Statue variant, tube behavior and PLM placement remain compiled.
Spore Spawn's spore palette, four health palettes, and death sprite/level/
background color frames are in the import data's `spore-spawn-colors.json`.
Health thresholds and death-frame timing remain compiled; this file changes
only the RGB5 colors selected at those native moments.
Dachora's default, four speed-boost, and four shine palette frames are in
the import data's `dachora-colors.json`. The timer and frame selection still
follow the cartridge code; the JSON controls the selected RGB5 colors.
The live Shitroid encounter's eight four-color normal-cycle frames and three
sixteen-color initialization targets (sidehopper, Shitroid, and dead sidehopper)
are in the import data's `shitroid-colors.json`; native palette timing and target destinations remain
compiled. The Mother Brain cutscene Baby Metroid's initial fifteen colors and
six fade-to-black frames are separate in
the import data's `baby-metroid-cutscene-colors.json`. The cutscene state machine still selects each fade frame and controls
its timing; this JSON contains only the colors.
Botwoon's eight health-band sprite palettes are in
the import data's `botwoon-colors.json`. The initial enemy palette remains in the ordinary
enemy color sheet; health thresholds and band progression stay compiled.
Mother Brain's sixteen paired body/leg death-fade frames and eight corpse-head
fade frames are in
the import data's `mother-brain-death-colors.json`; death-phase timing, counter selection, and collision
remain compiled behavior.
Zebetite's eight-frame, two-color Tourian pulse is in
the import data's `zebetite-colors.json`. Its physical-slot-zero counter, eight-frame wrap,
and palette destination remain engine behavior.
Lower Norfair Ridley's initial OBJ colors and fifteen arena-reveal BG color
rows are in the import data's `norfair-ridley-colors.json`. The reveal's three-call
cadence, zero-terminator handoff, and liquid-motion trigger remain engine
behavior. Ridley's health-shade rows are shared with Ceres in
the import data's `ceres-ridley-colors.json`.
The Tourian entrance statue's base, boss statue, four eye-glow, and shared
grey-transition colors are in the import data's `tourian-statue-colors.json`. Boss-event gates,
eye positions, unlock timing, and the four grey-palette destinations remain
compiled behavior.

The same directory contains `enemy-compositions.json` with named Boyon,
Cacatac, Boulder, Atomic, Skultera, Waver, Skree, Metaree, Zoa, and the
Brinstar/Norfair/yellow Pipe Bug, Fake Kraid, Kraid fingernail, Owtch, Stoke,
the three Ripper variants, Fireflea, Magdollite, Ceres door visual frames
(including the Ridley-room private overlay), the Ceres Baby's three poses,
Rio's eight idle/swoop compositions, Lower Norfair Rio's eighteen parent/flame
compositions, Norfair Rio's twenty parent/flame compositions, Puyo's eight
ground/airborne compositions, Bull's three idle/immune-shot compositions, and
Alcoon's eighteen left/right walking, firing, and airborne compositions, plus
Beetom's twenty-two crawling, hopping, and draining compositions, and the
Sidehopper/Dessgeega family's twenty-four floor/ceiling compositions (including
the large and Tourian Sidehopper variants), plus Choot's four idle/jump/fall
compositions, plus twenty surface-orientation poses shared by HZoomer, Zeela,
Sova, Zoomer, and Stone Zoomer. It also contains all 276 distinct bank-$B4
sprite-object frames selected by the 62 compiled room-object programs. Four
Draygon opening Evir and nine breath-bubble frames have descriptive names;
the other shared frames use stable `room_sprite_b4_` names followed by their
native spritemap address. A frame can be shared by several object kinds, so an
address-named edit can affect more than one effect. Object allocation, frame
durations, movement, and collision remain engine-owned.
Mother Brain's head, neck joint, and five falling-tube compositions use stable
`mother_brain_a9_` names followed by the native address. The private head/neck
draw hook preserves its world-space clipping and palette logic while using
these installed OAM parts; animation timing and encounter physics are separate.
Dead Torizo's private corpse hook likewise uses the named
`dead_torizo_corpse_a9_d761` composition, while its rotting tile sheet remains
the separate indexed PNG.
The Landing Site gunship uses `gunship_top_hull`, `gunship_bottom_hull`, and
eleven `gunship_entrance_` compositions. Gameplay resolves installed artwork
without ROM access.
Ridley's private wing and articulated-tail hooks use 31 named
`ridley_supplement_a6_` compositions; the fixed wing and tip pointer tables
remain compiled pose selections, separate from editable OAM parts.
Edit it in place to change a frame's ordered OAM
parts: `offsetX`, `offsetY`, `tileColumn`, `tileRow`, `size`, `priority`, `palette`,
`flipX`, or `flipY`. The `displayFrames` table may also bind each named native
frame to a different named frame in the same bank, letting an artist rearrange
the visible sequence without moving its native frame boundaries. For example,
setting `"boyon_idle_0": "boyon_idle_1"` displays the second Boyon composition
whenever the first is selected by gameplay. Native frame duration, collision
selection, enemy hitboxes, movement, and damage remain engine-owned. A malformed
document fails with a load error. Runtime drawing has no ROM-spritemap fallback. The current
document's named `frames` entries are the authoritative replacement inventory;
the examples above are not an exhaustive family list. A required installed
composition that is unavailable fails explicitly. Only the current document
revision loads; a catalog missing a required frame fails with an error instead of
silently rereading cartridge art.
The Evir character sheet is the ordinary extracted enemy PNG; the boss-room
intro reuses that sheet for its VRAM upload rather than reading raw cartridge bytes.
The Ceres Baby's four fifteen-color RGB5 palette rows are in
the import data's `ceres-ridley-colors.json`, which the import pre-colours the Baby's sheets from. Its
`babyShades` rows are separate from the Ridley start/fade/health/alarm/retreat colors. The draw
program still chooses rows using the cartridge's animation cadence; palette
edits do not change movement, collisions, or timing. The sixteen `alarm` rows
are edited the same way; their cadence and wrap remain engine behavior. Only the
current document version loads; re-run the extractor to restore the original file.
The separate `enemy-projectile-compositions.json`, import data whose parts are the original's packed sprite placements
(the import composes the spawned-object sheets from it, and the installation holds their pictures), contains the Ceres elevator
platform/pad, Skree/Metaree debris, and named timed-frame OAM compositions for
translated Mother Brain, dust, Ceres Ridley, Phantoon, Crocomire, Draygon,
Alcoon fireballs, Golden Torizo eggs/missiles/eye beams, and Bomb/Golden Torizo
drool, swipe, sonic-boom, landing-dust, explosion and Chozo-orb effects, plus
generic enemy-death explosions and pickups. It also includes Ceres debris,
save-station electricity, gunship dust, Tourian dust, falling sparks, lava
throws, eye-door sweat, Ki Hunter acid, Fune/Namihe and Dragon fireballs, and
Powamp spikes, Bomb Torizo statue fragments, and Mother Brain glass, hand beam,
top tubes and turrets. Editing its visual parts does not change projectile
timing, damage, motion, or collision. The `programFrames` names identify bank-$86
animation lists and frame numbers; only their drawing is editable. Only the current
document revision loads.
The five `gunship-liftoff-N-tiles.png` files in `game/enemy-tiles/` contain the
gunship's takeoff character uploads, each an indexed 4-bpp image. The five upload
times and VRAM destinations stay fixed; edits are resolved when each queued
upload reaches NMI, including after restoring an older pending debugger state.
Walking, wall, and ninja Space Pirates use the separate, historically named
`game/enemy-tiles/enemy-walking-pirate-compositions.json`, which holds the named Pirate,
Ridley-body, Draygon, Spore Spawn, Torizo and other multipart OAM frames. Each frame contains
ordered visual components with `offsetX`, `offsetY`, and `parts` using the same
OAM fields listed above. Its `displayFrames` table can remap any named visual
frame to another frame of the same enemy family, without changing the native
instruction frame, duration, or hitbox. Component offsets move only the drawing;
native hitbox geometry and touch/shot callbacks remain engine-owned. Walking,
wall, and ninja Pirate collision records are compiled and unaffected by JSON edits.
Display bindings cannot cross between Draygon and Spore Spawn. The 22
`phantoon_bg2_*` and 34 `draygon_bg2_*` display-binding keys have their tilemap
commands in the separate BG2 files; those native BG2-only roots have empty
component arrays in stock, and an edit may add ordinary OAM components to them.
Missing or malformed frames fail loudly. Only the current document revision loads.

Enemy animation remains tied to compiled gameplay frames. `displayFrames` changes
which picture is drawn at a native frame; it does not edit durations, movement,
attack callbacks, projectile origins, collision geometry, or boss progression.
Projectile `programFrames` likewise replaces drawing at fixed instruction-frame
boundaries rather than supplying executable instructions.

`--enemy-animation-stock-parity` compares the explicitly declared Boyon and Torizo
control/selector records with the pinned import source. It is not a whole-game
playthrough or proof of every boss effect's parity.

Phantoon's stock body, eye, tentacles, and mouth use BG2 tilemap writes rather than
ordinary OAM parts: `phantoon-bg2-frames.json` holds its 22 named visual frames. Like Draygon's, Crocomire's and Mother
Brain's BG2 frames below, it is import data in the enemy-tile component's import data: each write's cells are the
original's packed tile placements, the import composes the boss body sheets from them, and the installation holds the
pictures. Its multipart `displayFrames` bindings select the
matching BG2 pose and any authored OAM components together. Physical frame
selection and write timing stay in the engine;
the JSON contains no hitbox, damage, or instruction data. Invalid or missing
frames fail during load.
Draygon's 34 BG2 frames use the same visual-only tilemap format in
`draygon-bg2-frames.json`: the ordered tiles and their 32-by-64
tilemap positions. Its native frame selectors, producer timing, hitboxes, and damage
callbacks remain outside the appearance file. A malformed file fails during load.
Crocomire's fifty fight-body poses use multipart OAM and, for 42 mixed poses,
`crocomire-bg2-frames.json`. One body `displayFrames` binding selects both halves;
the BG2 writer no longer draws the original pose when its limbs are remapped.
Draygon's bindings may select either an OAM or BG2 pose within Draygon's family.
All these writes retain the native new-instruction-frame gate. Selecting an
OAM-only pose writes no BG2 commands; it does not implicitly clear the tilemap.

`--boss-display-bindings` verifies all 154 Crocomire, Phantoon and Draygon poses
with exact packed OAM, ordered BG2 writes, replacement bindings and write gating.
RAM-only fixtures also check twelve exact native sprite-clipping cases,
208,208 physical shot/touch samples and 450 native
duration/goto steps with unchanged state and audio requests. The separate
`--boss-display-stock` imports the declared roots, compares native visual and
collision records. These are focused draw/clock checks, not complete boss-AI playthroughs.
Mother Brain's seventeen body poses use both OAM limbs in the same multipart
composition file and sixteen BG2 frames in `mother-brain-body-bg2-frames.json`.
The initial dummy pose has only OAM. A body `displayFrames` binding selects both halves of
the requested pose; its native physical frame, movement, collision, instruction
clock and BG2 counter-scroll remain compiled. BG2 writes retain the native
new-instruction-frame gate. A malformed file fails explicitly.

`--mother-brain-body-presentation` checks exact authored limb OAM, BG2 runs,
display remaps and write gating with RAM-only fixtures, and compares production
body instruction state through both walks and a crouch. The separate
`--mother-brain-body-stock-presentation` imports all declared body roots and
checks they load. These are focused checks of
the source-identified conversion, not a battle playthrough or a search for reads.
Other unsupported frames fail explicitly; there is no runtime ROM fallback.
Crocomire's first and second melting images, `crocomire-melt-first.png` and
`crocomire-melt-second.png`, and their 16×16 BG2 layouts, `crocomire-melt-first-tiles.json`
and `crocomire-melt-second-tiles.json`, are import data: the import composes the melt
pictures of Crocomire's body sheet from them, and the installation holds those pictures, which
a player edits in place. The layouts' cells are the original's packed tile placements, so no
installed file holds them. The erase order,
distortion timing, the update each band of the image is shown on, and collision
stay fixed; extraction pictures every band at every dissolve step and the death
sequence selects which picture each band shows (`--crocomire-melt-frames`).
Pixels beyond each native image's written byte range are reserved and must
remain zero.
Melt JSON rejects unknown fields and duplicate names, including case aliases,
at the document root and inside cells. Existing case-insensitive field spelling
is still accepted. Invalid or missing melt/corpse resources are rejected before
their initializer changes actor state, phase counters or mutable graphics tables.
The focused `--enemy-effect-mechanics` verifier checks both 54-call dissolves and
the 118-call corpse rot, including exact erase/HDMA, row completion, smoke,
band-selection and audio schedules. Production receives RAM only; these checks
verify identified conversion paths, not the complete battles or every corpse family.
`--enemy-effect-resources` checks failure before mutation, `--crocomire-melt-json`
checks strict fields, and `--mother-brain-corpse-stock-artwork` uses import-only
stock data to check installed pixels, staging and sprite uploads.
Crocomire's six skeleton-character uploads share the indexed 4-bpp
`crocomire-skeleton-tiles.png` sheet in `game/enemy-tiles/`, the pixels used during
his death sequence. The upload sequence and VRAM destinations remain fixed. A
malformed sheet fails when the artwork loads.
Kraid's BG2 body sources, `kraid-upper-bg2.json` and `kraid-lower-bg2.json`, and his
four animated head layouts, `kraid-head-0.json` to `kraid-head-3.json` by mouth stage, are
import data: the import composes Kraid's body sheet from them, and the installation holds its
pictures. Each body source has two ordered 32×32 pages and each head layout 32×11 ordered
cells, every cell one of the original's packed tile placements. Kraid's engine still
owns frame selection and duration, growth, collision, and VRAM transfer timing.
The head and body-map loaders reject unknown fields and duplicate properties,
including differently cased aliases. Historical single-field casing remains
readable. Priority is cleared only in the native copied body regions; the
untouched lower-map tail retains its authored words before the final blank row.
Head uploads target VRAM directly and do not replace the working body map used
by later rise, growth and sink uploads.
`--kraid-installed-presentation` checks installed body/head maps, backdrop pixels
and the four HUD-restoration quarters against import-only stock data. Production paths receive
RAM only. This is focused presentation/control acceptance, not a full boss battle
or GPU-output comparison.
The fixed backdrop characters revealed after growth or defeat are the
`kraid-room-background.png` indexed sheet in the same directory. Its pixel
indices, not the PNG's preview colors, become 4-bpp VRAM characters. Other Kraid
character uploads remain outside this set. An invalid sheet fails during load.
Kraid's colours are import data, which only pre-colour his pictures. The game works with his
eyes' colours alone: `kraid-eye-colours.json` in `game/enemy-tiles/` holds the three eyes'
colours under the hurt flash (`hurtFlash`) and under each of his eight health bands
(`healthBands`), each an RGB colour with channels 0..31, which his eye glow shows, starts
from and fades back to. Damage thresholds, eye-fade steps, and death timing stay in engine code.
Base room colors are import data, which only the import pre-colours the room sheets from; they
are not installed. Palette-FX scripts may animate or replace these base colors later in the room.

Visual 16x16 blocks are composed at installation from the block compositions, which are
import data: only the import reads which characters each block places, and they are not
installed. A room draws each block as one 16x16 picture from its tileset's block sheet in
`game/room-block-sheets`, so a block's appearance is changed by repainting its picture in
that sheet in place.

The library backgrounds' tilemaps and the seven contiguous scrolling-sky pages are import
data as well: the import pictures the room backgrounds and the sky from them, and they are
not installed. A background or the sky is changed through its pictures in the block sheets.
The library-background command sequence, WRAM staging, VRAM transfer order and
door conditions remain engine behavior, not editable data. The shared tilemap
loaders reject unknown fields, duplicate/case-aliased
properties, invalid dimensions and invalid cells explicitly.
`--tilemap-json-contracts` verifies exact one/two-page and Kraid-head word
roundtrips, historical casing and 44 malformed-document rejections.
The import's library tilemaps cover the 58 compressed room BG tilemaps.
All 68 retail library-background command lists, including Landing Site's six
door-to-sky choices, are compiled engine definitions.
The bank-$88 sky pointer arithmetic, scrolling rates and VRAM timing remain
engine behavior. Kraid's direct HUD-character upload also uses the existing
`game/maps/hud-tiles.png` artwork instead of
rereading those characters from the ROM. Kraid's four-frame death restoration
reads the same indexed HUD sheet in four ordered quarters; editing that PNG
changes those BG3 characters too, without changing the native transfer cadence.
The Tourian statue-ghost library upload
likewise uses the installed room-character sheet instead of the raw ROM source.

Room-level visual arrangements are installed as 246 `game/room-layouts/level-*.json`
files, one per level. Each level is named for the room whose state first uses it, for
example `level-LandingSite.json`; a room's states that share a level share its file.
Edit the file in place and restart. `widthInBlocks` and `heightInBlocks` describe the complete
native allocation, including any authored rows beyond the visible camera.
`foregroundVisualWords` and `backgroundVisualWords` are row-major arrays of
16x16 block references. Each decimal word uses bits 0-9 for the index into the
combined CRE/area block table, bit 10 for horizontal flip, and bit 11 for
vertical flip. Only values 0..4095 are valid; collision bits cannot be placed
in this presentation file. BG2 entries that the ROM leaves uninitialized are
exported as zero visual references. The renderer receives edited references,
while collision, BTS, slopes and hazards use the unmodified native level
allocation. Subsequent PLM draws apply their own visual bindings where installed.
Invalid values fail with the offending path.
For all 246 retail level sources, that collision/BTS and streaming baseline is
extracted at installation into `game/room-level-streams/` as one
`level-*.bin` file per level: the complete, unmodified decompressor output,
listed with its length and SHA-256 in `room-level-streams.json`. These files are
not artwork, and a missing or corrupt file fails with its path. Installed desktop and Android room loads do not decompress these
level streams from the ROM. The room-layout JSON changes only rendered
visual words; it cannot modify the installed physical allocation or the native
overread tail. Development verifiers can still read the pinned ROM to compare
every installed byte with its original source.

Every room-object family below installs its frames in one shape: `frames` lists each frame by its `id`, and each
frame's `cells` list every cell it shows by its `x`/`y` place in blocks from the object's origin, with the `block` it
shows and that block's `mirroring`. Edit a cell's `block` or `mirroring` in the installed file and restart. Keep every
frame and every cell: the import checks each frame against the shape and collision the game draws it by, and rejects a
missing, repeated or unknown frame and a missing, repeated or foreign cell. Collision is never part of a picture.

Ordinary shot-block PLM appearances are installed as
`game/room-plm-shot-blocks/shot-blocks.json`. Each of its 19 frames is one
compiled animation or restoration draw. The native instruction
timers, sounds, draw geometry and complete physical level words remain compiled
and immutable. An edited word changes both the immediate PLM redraw and later
camera streaming, without changing shot-block collision or respawn timing.
The visual block still needs a valid definition in the active room's combined
CRE/area block table. Bomb blocks reuse the same 16 breakup-frame
frames, so these edits also change their break/reverse animation.
Their three linked-block restoration lists retain fixed collision words and use
visual block `$058`; repaint that block's pictures in the block sheet to
change its restored appearance. Neither route changes bomb-block collision,
sounds, or respawn timing.
Samus-contact crumble blocks use those same shared breakup frames. Their linked
restoration lists keep compiled type-B parent/type-5 and type-D child words and
display visual block `$0BC`; repaint that block's pictures in the block sheet to change its
restored appearance. Neither edit changes contact collision or crumble timing.

Breakable Grapple-block appearances are installed separately as
`game/room-plm-grapple-blocks/grapple-blocks.json`. Keep all five
frames: they are the compiled Grapple and breakup frames. The full
native level words, including Grapple/air collision, and both instruction
timelines remain fixed. Edited art appears in immediate redraws and later camera
streaming without changing collision or respawn timing. The chosen visual block
must exist in the active room's block-definition table. Other PLM families use
their own resources or remain ROM-backed.

Station PLM appearances are installed as
`game/room-plm-stations/stations.json`. In the installed file, edit only the cells' `block` and `mirroring`,
then restart. Every frame has a semantic `id`, such as
`map-frame-0`, `save-active-a`, or `resource-right-extended`; keep all 20
IDs and every cell. The values select visual blocks from the
active room's common room elements and tileset, whose pictures can be
repainted in the block sheet.
Station timing, access selection, full physical level words, map
rewards, refills, and save behavior stay compiled. Edited station art affects
the immediate redraw and later camera streaming without changing those rules.
Other room-object
families still need separate presentation resources.

Blue-door cap appearances are installed as
`game/room-plm-blue-doors/blue-doors.json`. In the installed file, edit only the cells' `block` and `mirroring`, and restart. Keep all 16 IDs: four animation frames for
each of `left`, `right`, `up`, and `down`. Each frame contains four visual
blocks from the active room's combined CRE/area definitions. The full
physical level words, door collision, animation cadence, and sound stay
cartridge-defined. Edited cap tiles appear in immediate redraws and later
camera streaming. Invalid content fails loudly. Colored, grey, and eye-door
artwork is not covered by this blue-cap resource.

Yellow, green, and red door-cap appearances are installed as
`game/room-plm-colored-doors/colored-doors.json`. In the installed file, edit only the cells'
`block` and `mirroring`, and restart. Keep all 48 IDs: four frames for each
color and orientation. Each frame contains four visual blocks from the active
room's combined CRE/area definitions. The full physical level words, shot
filters, hit counters, opening cadence, sounds, and door-bit persistence stay
cartridge-defined. Edited cap tiles appear immediately and during later camera
streaming. Invalid content fails loudly. Grey-door art uses the separate resource below;
eye-door art uses the separate resource below.

Grey-door caps and the four shared door-clear frames are installed as
`game/room-plm-grey-doors/grey-doors.json`. In the installed file, edit only the cells' `block` and `mirroring`, and restart. Keep all 20 IDs: four frames for each grey
orientation and `clear-left`, `clear-right`, `clear-up`, `clear-down`. The
clear frames are also used by blue and colored doors. Bomb Torizo's special
grey-door program selects the ordinary right-facing grey cap. Physical level
words, collision, condition gates, timing, sound, and door-bit persistence
remain cartridge-defined. Invalid content fails loudly.

Eye-door components are installed as `game/room-plm-eye-doors/eye-doors.json`.
In the installed file, edit only the cells'
`block` and `mirroring`, and restart. Keep all 23 IDs: five animated eye
frames per orientation, three middle and three bottom frames per orientation,
and `left-eye-clear`. Each entry carries one, two or four visual blocks, as
listed in the stock file; replacements use the active room's CRE/area tile
definitions. These edits do not change eye-door collision, attacks, missile
hit counts, opening timing, sounds, or door-bit persistence. Malformed content
fails loudly.

Mother Brain's glass frames are installed as
`game/room-plm-mother-brain-glass/mother-brain-glass.json`. In the installed file, edit only the
cells' `block` and `mirroring`, and restart. Keep all eleven frame IDs and their
cells; the compiled application keeps each run's shape and signed placement.
The active room's CRE/area definitions supply the tiles. Glass collision,
damage thresholds, shatter timing, shards, and progression cannot be changed
by this appearance file. Malformed content fails loudly.

The n00b-tube break-apart frames are installed as
`game/room-plm-noob-tube/noob-tube.json`. In the installed file, edit only the
cells' `block` and `mirroring`, and restart. Keep all seven frame IDs and their cells; run placement,
physical blocks, the power-bomb trigger, shards, and event timing remain
compiled. The tube's two reachable instruction branches are compiled as
engine behavior too. The active room's tile definitions supply the actual art.
Invalid content fails loudly.

Room-object placement records are compiled application definitions, not
editable art. All retail room PLM populations are selected without rereading
their bank-$8F source records; visual files above can replace appearance but
do not change which objects spawn or their collision/progression behavior.
The 70 referenced PLM setup/initial-list header pairs are compiled too;
the 173 retail scroll-trigger mutation programs are compiled as well. Their
room-scroll cell writes remain gameplay behavior, never editable visual
data. Constructed test-room populations can still supply their own
bank-$8F scroll programs. The 17 dynamically uploaded permanent-item kinds
also use compiled character bytes and palette offsets rather than reading
bank $89 and their initial bank-$84 instruction lists during retail loads.
Constructed rooms can still supply their own item graphics on the bus. These
tile uploads are compiled definitions with installed, editable PNGs and palettes;
other PLM instruction-program ROM dependencies remain to be migrated.

Downward-gate PLM block appearances are installed as
`game/room-plm-downward-gates/downward-gates.json`. In the installed file, edit only the cells'
`block` and `mirroring`, then restart. Keep all 14 semantic IDs and each
run/word count: six `column-frame-*` entries and left/right trigger entries
for blue, green, red, and yellow gates. These words select visual blocks from
the active room's combined CRE/area definitions. The compiled draw geometry,
physical level words, collision, shot filters, and animation timing do not
change. The visible moving gate actor uses separate bank-$86 sprite art and is
not replaced by this resource. Invalid data fails loudly.

Elevator-platform PLM frames are installed as
`game/room-plm-elevator-platforms/elevator-platforms.json`. In the installed file, edit only
the cells' `block` and `mirroring`, then restart. Keep all three named frames
and their run lengths. Each word selects a visual block from the active room
tileset; the four-step frame loop, draw offsets, and physical collision words
remain compiled. Invalid edits fail loudly.

Mother Brain's escape-gate appearances are installed as
`game/room-plm-escape-gate/escape-gate.json`. In the installed file, edit only the cells' `block` and `mirroring`, then restart. Keep all three four-block frames: `open`,
`half-closed`, and `closed`. These words select visual blocks from the active
room's combined CRE/area definitions; the cartridge's collision, closing
timing, and door-transition handoff remain compiled mechanics. Malformed
content fails loudly.

Bomb Torizo's resident hand PLM appearances are installed as
`game/room-plm-bomb-torizo-hand/bomb-torizo-hand.json`. In the installed file, edit only
the cells' `block` and `mirroring`, then restart. Keep both `intact` (eight
cells) and `cleared` (sixteen cells). The authored
geometry, collision words, Bombs gate, debris timing, and music are not
editable through this file. Invalid files fail loudly.

Reachable Draygon cannon appearances are installed as
`game/room-plm-draygon-cannons/draygon-cannons.json`. In the installed file, edit only
the cells' `block` and `mirroring`, then restart. Keep all twelve semantic
right/left shield and damaged-frame IDs, four cells per frame. This does not alter cannon collision, projectile thresholds, or boss
control-word writes. The unused diagonal orientations are not part of this
resource. Invalid content fails loudly.

Chozo statue terrain appearances are installed as
`game/room-plm-chozo-statues/chozo-statues.json`. In the installed file, edit only the cells'
`block` and `mirroring`, then restart. Keep the three IDs for the cleared
Lower Norfair hand and the two Wrecked Ship slope-access states. These edits
cannot change draw geometry, slope collision, or the statue's event/acid
behavior. The crumble plug continues to use the shared shot-block frames.
Invalid content fails loudly.
The Chozo crumble, hand, and slope-access PLM instruction lists are also
compiled as bounded mechanics data, while the native statue callbacks remain
separate executable behavior.

Linked bomb-block and Samus-contact crumble-block restoration appearances
are installed as `game/room-plm-linked-restores/linked-restores.json`.
In the installed file, edit
only the cells' `block` and `mirroring`, then restart. Keep the six IDs:
`bomb-horizontal`, `bomb-vertical`, `bomb-square`, `crumble-horizontal`,
`crumble-vertical`, and `crumble-square`, with their existing two or
four cells. These choices change restored appearance, not the
linked parent/child collision, respawn timer, or contact/bomb trigger.
Malformed content fails loudly.

The five reachable Speed Booster contact PLM programs and the bomb-special
reveal program are compiled from the pinned cartridge. Their sound operands,
crumble-frame selections, timers, restore/delete instructions, and the bomb
reveal's physical draw word no longer require bank-$84 runtime reads. The four
crumble appearances remain editable through the shared shot-block resource;
their timing and collision are not appearance settings.
The hardcoded Maridia elevatube PLM likewise uses compiled cartridge delay,
draw, sound, and deletion data, leaving its physical tile and sound behavior
unchanged without reading that bank-$84 source at runtime.
The Tourian entrance access floor also uses compiled clear/crumble programs
and physical draw rows. Its five appearances are installed as
`game/room-plm-tourian-access/tourian-access.json`. In the installed file, edit only the cells'
`block` and `mirroring`, retaining all five IDs and each frame's cells. The six-row removal, collision words, and frame timing remain compiled.
Invalid content fails loudly.

The bomb-revealed Speed Booster block uses
`game/room-plm-speed-booster/speed-booster.json`. In the installed file, edit the sole
`bomb-reveal` frame's one cell. Its type-B collision and
one-frame reveal timing remain compiled. Invalid edits fail validation.

The Maridia elevatube's one-block PLM appearance is installed at
`game/room-plm-maridia-elevatube/maridia-elevatube.json`. In the installed file, edit the
`elevatube-block` frame's one cell. The sixteen-frame hold,
physical block, sound, and deletion remain compiled and unchanged.

Spore Spawn's four 2×2 ceiling frames are installed at
`game/room-plm-spore-spawn-ceiling/spore-spawn-ceiling.json`. In the installed file, edit only
the cells' `block` and `mirroring`. Retain all four frame IDs and their four cells. The crumble/clear sequence,
sound, timing, and physical terrain changes remain compiled.

Permanent collectible PLM appearances are installed as
`game/room-plm-collectibles/collectibles.json`. In the installed file, edit only each frame's cell,
then restart. Keep all 16 semantic IDs: the empty block, Chozo orb
and burst, the four two-frame tank kinds, and the three shot-block reveal
frames. The words select visual blocks from the active room's combined CRE/area
definitions. Every other item shows its own pictures, described below.
Item acquisition, collision, room-item and Chozo persistence, sound, frame
timing, and physical level words remain compiled. An edited cell changes the
immediate redraw and later camera streaming without changing the item reward.
Invalid data fails loudly.

Permanent items that load their own graphics (every item but the four tanks)
are drawn as pictures in the room block sheets: each tileset's sheet in
`game/room-block-sheets` holds, for each such item its rooms place, the item's
two pictures (one per animation frame), named `collectible <item> frame <n>`.
Every such item shows its own pictures, whatever was set up before it. To
restyle an item, repaint those pictures in the installed sheet, then restart. Pickup behavior is
unchanged.

X-ray reveal art is installed as `game/xray-reveals/reveals.json`. In the
installed file, edit its `topLeft`, `topRight`,
`bottomLeft`, or `bottomRight` metatile indices, and restart. Each entry names
the collision type and BTS values that select it, plus its read-only copy
shape. The installer groups identical cartridge rules to keep the file short;
changing the rule keys, shape, count, or unused operands is rejected. The
visual indices may be 0..4095, but each selected metatile must exist in the
room's combined CRE/area block definitions when X-ray is used. The compiled
cartridge lookup still determines whether a block is revealed, how many
blocks are copied, extension traversal, and Brinstar-only behavior.
Its `extensionExitBlock` is the metatile an extension chain shows when its next
step leaves the room backward.
The same file contains four `tankMetatiles` (the Energy, Missile, Super Missile
and Power Bomb tanks' reveal blocks; every other item is revealed as the first
of its own pictures) and `rooms` with special reveal tiles.
Each room tile's `x`, `y`, and `word` are visual-only: the first two are block
coordinates, and the word's low ten bits select a metatile while bit 11 selects
the cartridge's vertical row swap. Keep each room's `overlay` name and the
list of rooms intact; the name identifies which overlay a room state shows. Installed hosts use
these entries without reading the item draw table or room overlay records from
the ROM during X-ray setup.

The room-ID guide makes visual-layout files discoverable, but shared sheet and
palette filenames still encode source identities. Further required-reference,
format compatibility and presentation-parity review remains tracked in
#530 and #549; Core does not retain a runtime ROM fallback for that work.

## Projectile composition and beam PNGs

Installed hosts extract `game/projectiles/projectile-compositions.json` under the
player-data root. Edit it in place; run the extractor again to restore the original.

Each `sprite_XXXX` entry exposes part offsets, tile row/column, size, flips, palette
index and priority. Keep every entry and the version field. These are compositions
of graphics. Damage, collision radii and
timing are not editable here. Trails, charge flares and Grapple visuals are not
covered by this composition file.

`projectile-frame-bindings.json` in the same directory selects which existing
`sprite_XXXX` composition each of the 805 timed beam, missile, Super Missile and
bomb instruction frames draws. Keep all `frame_XXXX` entries and select only sprite
IDs present in `projectile-compositions.json`; duration, trail cadence, collision
radii, damage and instruction flow remain compiled gameplay data. Edits are included
in recorded content identity.

Trail appearance uses `projectile-trails.json` in the same directory: tile
row/column, palette, priority and flips. Timing and trail movement are intentionally
not editable. This selects existing trail graphics; their pixels are in
`game/projectiles/projectile-trails.png`, a 96x8 indexed 4-bpp image. The first eight
tiles are ice/wave trails and the last four are missile/Super Missile trails; their
two VRAM destinations stay fixed. The Mother Brain intro flashback shares the
installed projectile, explosion and trail compositions, including after loading a
debugger state.

Beam sheets `beam-00-tiles.png` through `beam-0B-tiles.png` are also installed in
`game/projectiles`, one per beam combination. Keep each 64x8 indexed format and pixel
indices 0 through 15. PNG palette colors are diagnostic: gameplay uses the separately
installed beam palette. These sheets do not hold missile, bomb, trail, flare or
Grapple artwork. Missile and bomb pixels are part of the shared
`game/standard-objects/standard-object-characters.png` sheet; trail pixels use
the separate PNG above, while Grapple endpoint/segment pixels use
`game/projectiles/grapple-tiles.png`. Charge-flare and Grapple sprite
compositions and their visual placement rules are separately editable JSON in
`game/projectiles/`; these edits do not alter projectile collision or motion.

The ordinary beam colors are import data, which only the import pre-colours the
projectile and frozen-enemy sheets from; they are not installed. The ten-frame Hyper Beam
projectile palette cycle is `game/projectiles/hyper-beam-fx-colors.json`, eight RGB5
colors per frame. This changes appearance only: the frame order, two-tick cadence and
loop remain compiled cartridge behavior. Loading a state refreshes ordinary colors at
the next accepted display update, but preserves an active Crystal Flash or Hyper
palette.

Restart to load edits. Loading a debugger state retains the current session's
composition and beam catalogs rather than restoring old artwork from the state.
Startup logs the projectile content hash. An invalid file fails loudly with its path,
retaining the underlying exception. All required manifest fields must be present.
Core rendering has no ROM fallback; explicit native image decoding remains
import/reference tooling only.

## Verified build

On September 10, 2026, the Release AOT APK was installed as the isolated package
`org.supermetroid.csharp.romimporttest` on a Retroid Pocket Classic. First-run setup
accepted the supplied ROM and reached the game menus; player testing confirmed the
game runs with audio. The existing game package and its saves were left intact.
The import integration suite and full Core verification suite passed using private
fixtures. The APK and desktop publish output were inspected and contained no ROM,
SPC upload streams, audio manifest, or WAV samples.

## Tester inventory and Tourian access

Add these independent options under `[Game]` in the active `SuperMetroid.ini`,
then restart. Both default to `false`; existing INI files are not overwritten.

```ini
GrantAllEquipment=false
UnlockTourian=false
```

`GrantAllEquipment=true` grants every normal collectible upgrade on game start/load:
1499 energy, 400 reserve energy, 230 missiles, 50 super missiles, 50 power bombs,
all equipment and all collectible beams. Plasma is equipped and Spazer is available
in the equipment menu (the two cannot be equipped together). Hyper Beam remains
part of the Mother Brain sequence. Reserve tanks start in Auto mode. Pickups are
marked collected to prevent duplicate capacity upgrades. This is a one-time grant
per initialized player, not invincibility or infinite ammo; equipment can still be
unequipped. Saving retains the granted inventory after the option is turned off.

`UnlockTourian=true` opens the four-statue entrance without defeating the four
bosses or setting their defeat flags. It does not teleport Samus or skip Tourian's
rooms or Mother Brain's encounter. Both settings are preserved in input recordings
and excluded from attract demos.

### Door-transition recovery states

`[Game] DoorTransitionAutosave=true` saves a recovery state after each completed door transition on Windows and Android. It also defaults to true when absent from an existing INI. Set it to false and restart to disable new autosaves.

Choose `auto` in the existing state-slot selector and use **Load State**. The file is `debug-states/SuperMetroid-debug-slot-auto.smstate` under the player-data directory, beside the numbered states; attach it for diagnostic handoff. It is separate from manual slots 0–9. Each completed write atomically replaces the previous auto state; failed writes retain the last valid state and log the exception. Playback replays do not autosave.
