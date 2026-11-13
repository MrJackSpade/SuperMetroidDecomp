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

- `import/SuperMetroid.smc`: the import's validated, unheadered ROM copy, retained by default beside the
  content and never inside `game/`; optional only while all extracted assets remain complete and valid. An
  installation from an older release, whose copy lay inside `game/`, is extracted again only from a ROM you
  choose.
- `game/audio/`: the rendered clips of every song and sound (`clips/`) and their index (`audio-clips.json`),
  which names each sound by name, and nothing else; audio is replaced by clip. The SPC upload streams, the
  instrument samples and the catalog manifest the clips are rendered from are import data (#1693).
- `game/maps/gameplay-hud.json`: the HUD's layout, the screen pixel each of its pictures shows at; the HUD's
  tiles, whose cells are the original's packed tile placements, are import data the import pictures the HUD from.
- `game/pictures/`: the picture store, the pre-coloured pictures every sheet below names, edited in place, and
  its index (`pictures.json`): the pages, every picture's place and every map's tag.
- `game/sprite-sheets/`, `game/enemy-sheets/`, `game/boss-body-sheets/` and `game/room-block-sheets/`: the
  sheets the import composes, each naming its frames' pictures in the picture store.
- `game/samus-body/`: what the game binds of Samus's body: each pose's art origin and the body frames her body
  sheet pictures (`samus-body.json`), her death steps and the arm cannon's layout. Her body pages, selectors,
  compositions, atmospheric lists and cover appearances are import data the import pictures her sheets from.
- `game/enemy-tiles/`: the enemies' frame display bindings (`enemy-frame-displays.json`) and Kraid's eye
  colours. The enemies' characters, compositions and extended frames, the bosses' BG2 layouts and
  tile-reference maps, the enemy, boss and area colour documents and their manifest are import data, which
  only compose and pre-colour the sheets and are not installed.
- `game/intro-cinematic/`: the opening's and the Ceres scenes' backdrops and actor placements. Their
  character pages, layouts, Mode 7 maps, compositions, palettes and manifest are import data.
- `game/room-layouts/`: stock BG1/BG2 visual block-reference JSON for every room level source.
- `game/room-level-streams/`: stock-only typed terrain (collision kinds, block behaviours and
  streaming baseline) for every room level source. These files are not artwork.
- `game/xray-reveals/`: stock visual metatile choices for X-ray block reveals.
- `game/installation.json`: extraction format version and ROM identity. No other installed document records the
  cartridge's digest or the hashes of the files beside it (#1693): the manifests that list the import data's
  hashes stay with the import data.
- `SuperMetroid.ini`, `SuperMetroid.save.json`, `debug-states/`, and
  `input-recordings/`: player data, outside the replaceable game directory.

Setup stages and validates all generated content before publishing it. A complete
installation is reused, including edited clips. Missing or damaged
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
station-reveal document uses the same validation boundary; it is required stock
metadata, not an editable exploration rule. The map manifest, which lists the
import data's hashes, is import data and is not installed (#1693).

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
# Extract just the audio: its clips and their index into the chosen directory, and the catalog they are rendered from
# into the import-data directory beside it (here C:\private\import-data\audio), as the installer lays them out.
dotnet run --project csharp/src/SuperMetroid.DebugRunner -- assets audio-rom "C:\ROMs\Super Metroid.smc" "C:\private\audio"
# Synthetic validation, or full integration from the repository-root Super Metroid.smc.
dotnet run --project csharp/src/SuperMetroid.IntegrationVerification -c Release -- --asset-import
dotnet run --project csharp/src/SuperMetroid.IntegrationVerification -c Release -- --asset-import-rom
# Check room-character, palette and visual-block stock import.
dotnet run --project csharp/src/SuperMetroid.Verification -c Release -- --room-artwork-installation "C:\ROMs\Super Metroid.smc"
# Check the opening's, Ceres' and ending's extracted artwork against the cartridge.
dotnet run --project csharp/src/SuperMetroid.Verification -c Release -- --intro-cinematic-artwork "C:\ROMs\Super Metroid.smc"
# Constructed palette JSON contracts and exact BG/OBJ pixels; no retail ROM needed.
dotnet run --project csharp/src/SuperMetroid.Verification -c Release -- --palette-json-contracts
# Constructed room/environment/intro tile-word and malformed-document checks; no ROM needed.
dotnet run --project csharp/src/SuperMetroid.Verification -c Release -- --tilemap-json-contracts
# Manifest/layout/X-ray admission checks using only an already-extracted installation.
dotnet run --project csharp/src/SuperMetroid.Verification -c Release -- --room-asset-json-contracts "C:\private\SuperMetroid"
```

The integration verifier tests non-seekable document streams,
header normalization, invalid input, cancellation, asset repair, interrupted publication,
save preservation, and booting the production Android session from the installed layout.
Legacy raw-data/PNG/map extraction commands remain developer tools; normal setup does not
require their input directories. Keep all ROMs and generated game resources out of Git.

## Samus body pictures and placements

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

## Opening and Ceres cinematics

The opening's pictures are installed whole in the sprite sheets (`intro-scene-pictures`,
`intro-narration-pictures` and `intro-sprites` in `game/sprite-sheets/`), and the Ceres scenes'
likewise (`ceres-floor`, `ceres-cinematic-pictures`, `ceres-flight-sprites`,
`ceres-destruction-sprites` and `zebes-reveal-sprites`). Edit their pictures in the picture store
to change what the scenes show. What the pictures are composed from is import data, never
installed (#1636, #1637, #1638, #1693): the opening's and the Ceres approach's character pages, the
opening's layouts (its four scene pages, the portrait, the first narration card, the subtitle line,
the portrait's four blinking-eye rectangles and the Zebes reveal's map), the Ceres approach's and
destruction's Mode 7 maps, the caret, next-page arrow, Mother Brain, explosion, Rinka, egg-effect,
discovery, scientist, Ceres flight and destruction sprite compositions, the opening's and the Ceres
approach's palettes, and the manifest naming them. Their cells and parts are the original's packed
tile and sprite placements. The cartridge's blink script, caret and arrow cadence, frame durations,
Mother Brain's missile damage and hitboxes, camera, scale, rotation, actor movement, letter timing
and scene handoffs remain compiled behavior.

`game/intro-cinematic/intro-backdrops.json` holds the colours the opening shows where nothing is
drawn: the narration's `opening` backdrop and the Ceres scenes' `ceresFlight` backdrop. The game
reads these colours and neither palette; edit them in place, each channel 0–31.
`ceres-flight-actors.json` in the same directory exposes the five rear-view actors' initial 16-bit
screen X/Y positions, in large-asteroid, station, small-asteroid, vortex, and
rear-stars order. Their stock values come from the pinned cartridge's initializer
operands. Keep the IDs and order; edit only `x`/`y`, then restart the scene. Actor speed, wrapping,
instruction timing, and scene handoffs remain compiled.
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
Restored debugger states rebind these visuals without restarting the destruction or
descent timeline.

The ending's ship flyaway is composed by the import from the Ceres Mode 7 characters and the
front view of the Ceres flight map; its picture is the `ending-flyaway-floor` sheet, shown with
its original sixteen-frame upload cadence.

The ending's two escape views, planet-explosion backdrop and reward-jump icon are
not installed as cell maps or character pages: the import composes their floor
pictures from the cartridge, and the installation holds only those pictures in the
picture store. Edit the floor pictures to change what the scenes show.

The ending's and credits' character pages, its planet-explosion fragments, the waiting scene's
and the final logo's tile references, the transformation fragments and the cloud, explosion,
completion-text, reward and logo sprite compositions are import data as well: the import composes
the ending's sheets in `game/sprite-sheets/` from them, and the installation holds no
`game/ending-objects/` directory. Edit the ending's pictures in the picture store to change
them. The actors' instruction lists, movement, the cartridge-matched typewriter pacing, the
reward's actor instructions and physical handoff, the logo's motion, wrap timing and palette
crossfade, the final logo's six upload positions, the planet-explosion upload order and scene
callbacks remain compiled game logic.

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
after the Baby Metroid cutscene. The import pre-colours those effects' pictures from it; edit the pictures
in the picture store to recolor them. The cartridge bytecode, flash
cadence, phase handoff, and CGRAM copy destinations remain engine logic.

The room effects' animated characters, the `room-fx-animated-tiles.png` strip of the
native lava, acid, rain, and Maridia sand frames, Wrecked Ship's four 32-byte
treadmill frames, the Tourian statue strip and three 48-byte spores frames, are
import data: the import composes the effect pictures from them, and the installation
holds those pictures. Activation, frame order,
cadence, loop control and VRAM destinations remain compiled.

Spores use a ten-tick three-frame loop and camera-anchored quarter-pixel
vertical scroll. Their `room-fx-layer3-tilemaps.json` page is import data, whose cells are the original's packed tile
placements: the import composes the effect pictures from it, and the installation holds the pictures. Native source-aware
color math keeps the effect behind BG1 terrain and excludes OBJ palettes
zero through three; it never tints the HUD or enables liquid physics.

The room-FX blend colors, `room-fx-blend-palettes.json`, are import data too, which only
the import pre-colours the effect overlay pictures in; they are not installed.

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

## Room pictures

The shared CRE characters, each distinct graphics-set character stream and the Tourian
statue-ghost characters, the room character pages and their manifest, are import data, which
only the import composes the room block sheets from; none is installed. A room draws from its
tileset's sheet in `game/room-block-sheets`, whose pictures are in the picture store: edit a
room's look through those pictures. Sheets are shared across rooms, so an edit can affect every
room that uses the tileset.

Landing Site's dedicated setup path uses the same block sheets. Its BG1/BTS/BG2 level
allocation comes from the installed stock-only room-level streams because those
words also drive collision and placement; artwork edits do not edit room geometry.

Ordinary room-enemy tile uploads and OBJ colors are import data, which only the
import composes the enemy sheets from: each `enemy-XXXX-tiles.png` and matching
`enemy-XXXX-colors.json` pair is identified by the enemy definition's stable
four-digit cartridge ID, and neither is installed. PNG pixels must remain indexed 0..15; its diagnostic PNG
palette is only a preview. The JSON contains exactly sixteen RGB5 colors, each
channel 0..31. This covers
the ordinary bank-$B4 room graphics-set uploads, not dynamic boss BG2 art or
enemy-projectile sheets. Edit an enemy's pictures in its sheet in `game/enemy-sheets` instead;
enemy health and AI are unchanged by picture edits.
Ninja Space Pirates also copy the shared gold-Pirate colors from
`enemy-F413-colors.json` into OBJ palette seven when initialized; the import
pre-colours their gold shade from it.

The ordinary `enemy-ED3F-tiles.png` page also supplies Dead Torizo's corpse
and falling-sand rows. `enemy-ED7F-tiles.png` supplies the dead sidehopper,
Zoomer, Ripper, and Skree corpse layouts. Mother Brain's decaying body instead
uses `mother-brain-corpse-tiles.png`, a separate indexed 4bpp source page.
The escape typewriter's five character uploads use
`mother-brain-escape-text-tiles.png`, also separate from the ordinary enemy
page.
The Baby Metroid, restored Mother Brain attack, and exploded escape-door OBJ
pages are separate `mother-brain-baby-tiles.png`,
`mother-brain-attack-tiles.png`, and `mother-brain-exploded-door-tiles.png`
pages.
Phase-two leg loading also uses `mother-brain-leg-tiles.png` for its first
eight pages; the following four reuse the attack page. Its twelve-record
timing and destination list remain compiled game definitions.
All of these pages are import data, which only the import composes the enemy and boss
sheets from; edit those sheets' pictures instead. Text timing and placement, row timing,
collision, and decay mechanics remain compiled.

Magdollite's separate four-frame OBJ color cycle is in
the import data's `magdollite-palette-cycle.json`, four RGB5 colors per frame. The cartridge's
eight-tick cadence, frame order, and selected OBJ palette slot stay in code.
The import pre-colours the cycle's pictures from it; the game reads no colour from the ROM.
Work Robot's six four-color records are similarly in
the import data's `work-robot-palette-cycle.json`. Its 64/16-tick durations, record wrap, and powered/unpowered state
remain compiled.
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

The enemy-tile import data contains `enemy-compositions.json` with named Boyon,
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
draw hook preserves its world-space clipping and palette logic while drawing
these frames' pictures; animation timing and encounter physics are separate.
Dead Torizo's private corpse hook likewise uses the named
`dead_torizo_corpse_a9_d761` composition, while its rotting tiles remain
a separate page.
The Landing Site gunship uses `gunship_top_hull`, `gunship_bottom_hull`, and
eleven `gunship_entrance_` compositions. Gameplay resolves installed artwork
without ROM access.
Ridley's private wing and articulated-tail hooks use 31 named
`ridley_supplement_a6_` compositions; the fixed wing and tip pointer tables
remain compiled pose selections.
Each frame's ordered OAM parts (`offsetX`, `offsetY`, `tileColumn`, `tileRow`, `size`,
`priority`, `palette`, `flipX`, `flipY`) are the original's packed sprite placements, so the
compositions are import data the import composes the enemy sheets from, and none is installed;
edit a frame's picture in the enemy sheets instead.
The installed `game/enemy-tiles/enemy-frame-displays.json` holds what the game reads of them:
its `displayFrames` table binds each named native
frame to a named frame every enemy showing it shows, letting an artist rearrange
the visible sequence without moving its native frame boundaries. For example,
setting `"boyon_idle_loop_first": "boyon_idle_loop_second"` displays the second Boyon composition
whenever the first is selected by gameplay. Native frame duration, collision
selection, enemy hitboxes, movement, and damage remain engine-owned. A malformed
document fails with a load error. Runtime drawing has no ROM-spritemap fallback.
The examples above are not an exhaustive family list. A required
frame that is unavailable fails explicitly instead of silently rereading cartridge art.
The Evir characters are the ordinary extracted enemy page; the boss-room
intro reuses that page for its VRAM upload rather than reading raw cartridge bytes.
The Ceres Baby's four fifteen-color RGB5 palette rows are in
the import data's `ceres-ridley-colors.json`, which the import pre-colours the Baby's sheets from. Its
`babyShades` rows are separate from the Ridley start/fade/health/alarm/retreat colors. The draw
program still chooses rows using the cartridge's animation cadence, and the sixteen
`alarm` rows' cadence and wrap remain engine behavior; recolouring the pictures does not
change movement, collisions, or timing.
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
top tubes and turrets. Their drawing does not change projectile
timing, damage, motion, or collision. The `programFrames` names identify bank-$86
animation lists and frame numbers.
The five `gunship-liftoff-N-tiles.png` pages, the gunship's takeoff character uploads, are
import data in the enemy-tile component's import data, which the import composes the
gunship's pictures from. The five upload times stay fixed.
Walking, wall, and ninja Space Pirates use the separate, historically named
`enemy-walking-pirate-compositions.json`, import data too, which holds the named Pirate,
Ridley-body, Draygon, Spore Spawn, Torizo and other multipart OAM frames. Each frame contains
ordered visual components with `offsetX`, `offsetY`, and `parts` using the same
OAM fields listed above. The installed `multipartDisplayFrames` table in
`game/enemy-tiles/enemy-frame-displays.json` can remap any of its named visual
frames to another frame of the same enemy family, without changing the native
instruction frame, duration, or hitbox; native hitbox geometry and touch/shot callbacks
remain engine-owned. Walking,
wall, and ninja Pirate collision records are compiled and unaffected by display edits.
Display bindings cannot cross between Draygon and Spore Spawn. The 22
`phantoon_bg2_*` and 34 `draygon_bg2_*` display-binding keys have their tilemap
commands in the separate BG2 frames; those native BG2-only roots have empty
component arrays in stock. Missing or malformed bindings fail loudly. Only the current
document revision loads.

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
`crocomire-skeleton-tiles.png` page, the pixels used during his death sequence: it is
import data, which the import composes his death pictures from. The upload sequence remains fixed.
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
`kraid-room-background.png` indexed page, import data beside his body sources, which the
import composes the backdrop's pictures from.
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
engine behavior. The HUD's characters, `hud-tiles.png`, are import data too, which the
import composes the HUD's pictures from, including the quarters Kraid's four-frame death
restoration shows, without changing the native transfer cadence. The Tourian statue-ghost
library upload likewise draws pictures composed from the import's room character pages.

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
For all 246 retail level sources, that terrain and streaming baseline is
extracted at installation into `game/room-level-streams/` as one
`level-*.bin` file per level: each block's collision kind, block and mirroring,
and each foreground block's behaviour as the reading its collision kind takes
(a reacting block's reaction list and entry, a slope's shape and mirroring, an
extension's signed offset, a door's door-list entry; air and solid blocks hold
none), with no behaviour byte (#1709). The collision kinds
`room-level-streams.json` names by the position the files write them at (#1693). These files are
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
`game/room-object-shot-blocks/shot-blocks.json`. Each of its 19 frames is one
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
`game/room-object-grapple-blocks/grapple-blocks.json`. Keep all five
frames: they are the compiled Grapple and breakup frames. The full
native level words, including Grapple/air collision, and both instruction
timelines remain fixed. Edited art appears in immediate redraws and later camera
streaming without changing collision or respawn timing. The chosen visual block
must exist in the active room's block-definition table. Other PLM families use
their own resources or remain ROM-backed.

Station PLM appearances are installed as
`game/room-object-stations/stations.json`. In the installed file, edit only the cells' `block` and `mirroring`,
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
`game/room-object-blue-doors/blue-doors.json`. In the installed file, edit only the cells' `block` and `mirroring`, and restart. Keep all 16 IDs: four animation frames for
each of `left`, `right`, `up`, and `down`. Each frame contains four visual
blocks from the active room's combined CRE/area definitions. The full
physical level words, door collision, animation cadence, and sound stay
cartridge-defined. Edited cap tiles appear in immediate redraws and later
camera streaming. Invalid content fails loudly. Colored, grey, and eye-door
artwork is not covered by this blue-cap resource.

Yellow, green, and red door-cap appearances are installed as
`game/room-object-colored-doors/colored-doors.json`. In the installed file, edit only the cells'
`block` and `mirroring`, and restart. Keep all 48 IDs: four frames for each
color and orientation. Each frame contains four visual blocks from the active
room's combined CRE/area definitions. The full physical level words, shot
filters, hit counters, opening cadence, sounds, and door-bit persistence stay
cartridge-defined. Edited cap tiles appear immediately and during later camera
streaming. Invalid content fails loudly. Grey-door art uses the separate resource below;
eye-door art uses the separate resource below.

Grey-door caps and the four shared door-clear frames are installed as
`game/room-object-grey-doors/grey-doors.json`. In the installed file, edit only the cells' `block` and `mirroring`, and restart. Keep all 20 IDs: four frames for each grey
orientation and `clear-left`, `clear-right`, `clear-up`, `clear-down`. The
clear frames are also used by blue and colored doors. Bomb Torizo's special
grey-door program selects the ordinary right-facing grey cap. Physical level
words, collision, condition gates, timing, sound, and door-bit persistence
remain cartridge-defined. Invalid content fails loudly.

Eye-door components are installed as `game/room-object-eye-doors/eye-doors.json`.
In the installed file, edit only the cells'
`block` and `mirroring`, and restart. Keep all 23 IDs: five animated eye
frames per orientation, three middle and three bottom frames per orientation,
and `left-eye-clear`. Each entry carries one, two or four visual blocks, as
listed in the stock file; replacements use the active room's CRE/area tile
definitions. These edits do not change eye-door collision, attacks, missile
hit counts, opening timing, sounds, or door-bit persistence. Malformed content
fails loudly.

Mother Brain's glass frames are installed as
`game/room-object-mother-brain-glass/mother-brain-glass.json`. In the installed file, edit only the
cells' `block` and `mirroring`, and restart. Keep all eleven frame IDs and their
cells; the compiled application keeps each run's shape and signed placement.
The active room's CRE/area definitions supply the tiles. Glass collision,
damage thresholds, shatter timing, shards, and progression cannot be changed
by this appearance file. Malformed content fails loudly.

The n00b-tube break-apart frames are installed as
`game/room-object-noob-tube/noob-tube.json`. In the installed file, edit only the
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
items are drawn from their pictures in the room block sheets, described below;
other PLM instruction-program ROM dependencies remain to be migrated.

Downward-gate PLM block appearances are installed as
`game/room-object-downward-gates/downward-gates.json`. In the installed file, edit only the cells'
`block` and `mirroring`, then restart. Keep all 14 semantic IDs and each
run/word count: six `column-frame-*` entries and left/right trigger entries
for blue, green, red, and yellow gates. These words select visual blocks from
the active room's combined CRE/area definitions. The compiled draw geometry,
physical level words, collision, shot filters, and animation timing do not
change. The visible moving gate actor uses separate bank-$86 sprite art and is
not replaced by this resource. Invalid data fails loudly.

Elevator-platform PLM frames are installed as
`game/room-object-elevator-platforms/elevator-platforms.json`. In the installed file, edit only
the cells' `block` and `mirroring`, then restart. Keep all three named frames
and their run lengths. Each word selects a visual block from the active room
tileset; the four-step frame loop, draw offsets, and physical collision words
remain compiled. Invalid edits fail loudly.

Mother Brain's escape-gate appearances are installed as
`game/room-object-escape-gate/escape-gate.json`. In the installed file, edit only the cells' `block` and `mirroring`, then restart. Keep all three four-block frames: `open`,
`half-closed`, and `closed`. These words select visual blocks from the active
room's combined CRE/area definitions; the cartridge's collision, closing
timing, and door-transition handoff remain compiled mechanics. Malformed
content fails loudly.

Bomb Torizo's resident hand PLM appearances are installed as
`game/room-object-bomb-torizo-hand/bomb-torizo-hand.json`. In the installed file, edit only
the cells' `block` and `mirroring`, then restart. Keep both `intact` (eight
cells) and `cleared` (sixteen cells). The authored
geometry, collision words, Bombs gate, debris timing, and music are not
editable through this file. Invalid files fail loudly.

Reachable Draygon cannon appearances are installed as
`game/room-object-draygon-cannons/draygon-cannons.json`. In the installed file, edit only
the cells' `block` and `mirroring`, then restart. Keep all twelve semantic
right/left shield and damaged-frame IDs, four cells per frame. This does not alter cannon collision, projectile thresholds, or boss
control-word writes. The unused diagonal orientations are not part of this
resource. Invalid content fails loudly.

Chozo statue terrain appearances are installed as
`game/room-object-chozo-statues/chozo-statues.json`. In the installed file, edit only the cells'
`block` and `mirroring`, then restart. Keep the three IDs for the cleared
Lower Norfair hand and the two Wrecked Ship slope-access states. These edits
cannot change draw geometry, slope collision, or the statue's event/acid
behavior. The crumble plug continues to use the shared shot-block frames.
Invalid content fails loudly.
The Chozo crumble, hand, and slope-access PLM instruction lists are also
compiled as bounded mechanics data, while the native statue callbacks remain
separate executable behavior.

Linked bomb-block and Samus-contact crumble-block restoration appearances
are installed as `game/room-object-linked-restores/linked-restores.json`.
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
`game/room-object-tourian-access/tourian-access.json`. In the installed file, edit only the cells'
`block` and `mirroring`, retaining all five IDs and each frame's cells. The six-row removal, collision words, and frame timing remain compiled.
Invalid content fails loudly.

The bomb-revealed Speed Booster block uses
`game/room-object-speed-booster/speed-booster.json`. In the installed file, edit the sole
`bomb-reveal` frame's one cell. Its type-B collision and
one-frame reveal timing remain compiled. Invalid edits fail validation.

The Maridia elevatube's one-block PLM appearance is installed at
`game/room-object-maridia-elevatube/maridia-elevatube.json`. In the installed file, edit the
`elevatube-block` frame's one cell. The sixteen-frame hold,
physical block, sound, and deletion remain compiled and unchanged.

Spore Spawn's four 2×2 ceiling frames are installed at
`game/room-object-spore-spawn-ceiling/spore-spawn-ceiling.json`. In the installed file, edit only
the cells' `block` and `mirroring`. Retain all four frame IDs and their four cells. The crumble/clear sequence,
sound, timing, and physical terrain changes remain compiled.

Permanent collectible PLM appearances are installed as
`game/room-object-collectibles/collectibles.json`. In the installed file, edit only each frame's cell,
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
the collision type and the `reactions` that select it, each by its list
(`Common` or `AreaSpecific`) and `entry` (0..127), plus its read-only copy
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

Further required-reference,
format compatibility and presentation-parity review remains tracked in
#530 and #549; Core does not retain a runtime ROM fallback for that work.

## Projectile pictures and bindings

The projectiles' pictures are installed whole in the projectile sheets (`projectiles-*` in
`game/sprite-sheets/`, one per beam combination, and `grapple`). What they are composed from is
import data, never installed (#1636, #1637, #1638): the beam character pages, the trail and
Grapple endpoint/segment pages, the shared standard sprite characters that hold missile and bomb
pixels, the projectile, trail, charge-flare and Grapple compositions (`projectile-compositions.json`
and `projectile-trails.json` among them) and the ordinary beam colors. Edit the pictures in the
picture store to change what a projectile shows. Damage, collision radii, trail cadence and motion
are not artwork.

`game/projectiles/projectile-frame-bindings.json` selects which existing
`sprite_XXXX` composition each of the 805 timed beam, missile, Super Missile and
bomb instruction frames draws. Keep all `frame_XXXX` entries and select only sprite
IDs the stock file names; duration, trail cadence, collision
radii, damage and instruction flow remain compiled gameplay data. Edits are included
in recorded content identity.
`charge-flare-placement.json`, `grapple-flare-placement.json` and `grapple-swing-frames.json`
in the same directory hold the charge flare's and the Grapple flare's signed visual offsets and the
orientation each Grapple swing direction displays; these edits do not alter projectile
origins, collision or motion. The Mother Brain intro flashback shares the
installed projectile, explosion and trail pictures, including after loading a
debugger state.

The ten-frame Hyper Beam colour cycle is import data (#1693): the import pre-colours
Samus's installed sheets in each of its frames, and the installation holds no copy of
it. The frame order, two-tick cadence and loop remain compiled game behavior. Loading a
state refreshes ordinary colors at the next accepted display update, but preserves an
active Crystal Flash or Hyper Beam shade.

Restart to load edits. Loading a debugger state retains the current session's
projectile catalogs rather than restoring old artwork from the state.
Startup logs the projectile content hash. An invalid file fails loudly with its path,
retaining the underlying exception. All required document fields must be present;
the projectile directory installs no manifest (#1693).
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
