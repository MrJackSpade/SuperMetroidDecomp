# Source-level ROM boundary audit (#549)

## Contract and method

Cartridge images may be opened and decoded only by asset-import/reference tooling.
Executable gameplay consumes compiled mechanics, installed presentation catalogs,
and active console memory. No runtime byte reader can supply cartridge data.

Inventory access from source, types, project references and their callers. Do not
use gameplay, controller replays, room censuses, unit tests or longer frame probes
to discover the next read. Focused tests verify an already identified conversion;
they do not establish that unexecuted paths are ROM-free.

This audit supersedes the strategy in the archived room census. It records the
2026-09-29 compiler-guided migration; #549 remains open for the acceptance work below.

## Access-path inventory

| Source/caller boundary | Current ownership and allowed data |
| --- | --- |
| `ISnesAddressSpace` / `SuperMetroidAddressSpace` | Write-only bus contract plus explicit `ISnesMutableMemory.ReadWorkRamByte` / `ReadSaveRamByte`. Physical allocations are 128 KiB WRAM and 8 KiB SRAM. No ROM property, constructor payload, untyped CPU reader or cartridge reader exists. |
| Native cartridge source / parsers | `IImportCartridgeSource`, `CartridgeImportAddressSpace`, `CartridgeImportSource`, `RomDataReader` and raw `RoomRenderer` belong to AssetExtraction, not Core. Legacy namespaces do not change assembly ownership. |
| Compression | Native `SmCompression` decoder and its command/format definitions belong to AssetExtraction. Core may decode bounded PNG/WAV/installed JSON, never the cartridge's compression stream. |
| Room/state/door selection | Compiled header, state, selection, door and setup catalogs. Pointers are identities/dispatcher selectors, not permission to read native bytes. |
| Level collision and BTS | `RoomLevelStreamDefinitions` indexes independently decompressed allocations by compiled room-state identity. Its embedded `SMLV` corpus validates exact source identities, lengths and trailing extent. It is not an addressable bank/image. Editable visual layouts cannot change collision/BTS. |
| Room PLM population | `LoadRoomPopulation` requires `RoomPlmPopulationDefinition`. Its records carry setup metadata, bounded copied scroll pairs and optional decoded graphics. Raw placements/headers/scroll/item graphics are decoded by `RoomPlmPopulationImporter` in AssetExtraction only. All 71 former pointer/flag callers were compiler-migrated. |
| PLM instruction and draw lookup | Bounded compiled family definitions and installed visual catalogs. `ReadBank84Word` / `ReadNativeBankByte` are deleted. Low-window wrapped instructions/draws consume explicit live WRAM; they cannot reinterpret a missing upper-bank definition as cartridge bytes. Internal authored verification fragments are bounded, nonserialized fixture input, not a generic byte provider. |
| Enemy control/collision/composition | Compiled definition, phase, instruction, collision and visual-selector records. Former generic enemy word/long/source readers are deleted. `IRoomEnemyFixtureSource` is typed internal fixture metadata; OAM composition uses required installed catalogs. |
| Samus horizontal speed | Compiled indexed/standalone mechanics records. Restored low-bank aliases read the six explicit live WRAM words. `ReadMappedByte` is deleted; no SRAM/cartridge/peripheral byte fallback masquerades as physics. |
| Samus body and special sequences | Compiled pose/input/delay programs, physical projectile origins, collision radii, liquid/knockback and special-owner state. Installed body selectors, OAM placement, graphics offsets and normal/cycle/Crystal Flash/death colors are presentation only. `ReadGraphicsYOffset` feeds drawing and visual charge/Grapple flares; physical beam/Grapple origins use the separate compiled correction. No generic cartridge provider exists. |
| Generic OAM / enemy OAM | Required installed spritemap/frame catalogs. Generic bus-backed sprite decoding and enemy sprite fallback were removed. Import/reference spritemap oracles remain outside Core. |
| VRAM DMA / queued writes | `ExecuteQueuedMemoryWrite`, `ExecuteHardwareMemoryDmaWrite` and queue drainage require `ISnesMutableMemory`. Installed asset transfers accept validated payloads or `VramAssetId`; legacy source-address identities resolve only bounded named art transfers. Unresolved cartridge windows reject, not read. |
| Background command lists | Core executes decoded background programs. Native command-list byte decoding is AssetExtraction's `LibraryBackgroundProgramImporter`, not a diagnostic-only reader left in Core. |
| CPU open-bus / indirect operand helpers | Explicit WRAM/SRAM/peripheral/latch behavior. A cartridge classification rejects a missing compiled definition; it has no cartridge read capability. Neither operand helper has a production Core caller; the unused native `SamusBeamPaletteLoader` wrapper was removed after a repository-wide call-site search. |
| APU sequencer / DSP | Installed validated audio catalogs and active APU RAM/DSP state. Managed player `ReadWord` helpers consume their local 64 KiB APU RAM, not SNES CPU ROM. |
| Other `ReadByte` names | VRAM, CGRAM/register caches, bounded local definition arrays, PNG/render-packet/recording codecs and active APU RAM. They are not an untyped CPU address-space API. |
| File/stream access in Core | Installed audio/map/presentation codecs, INI defaults, SRAM/JSON saves, input recordings and render snapshots. No cartridge-image file opener, native library bridge, reflective assembly reader or memory-mapped ROM source was found in the audited source. |
| Embedded Core resources | Exactly `SuperMetroid.defaults.ini` and the bounded room-level corpus. No raw ROM or bank image is embedded. |
| Desktop / Android startup | `GameAssetInstaller.OpenOrRepair` first validates extracted content without opening ROM. Only incomplete installs call `EnsureInstalled`; explicit import uses `Install`. `GameInstallation.OpenRuntimeAddressSpace` validates extracted content and returns mutable-memory-only state. Hosts do not acquire a cartridge reader. |
| Import repair / reference diagnostics | AssetExtraction may open the user's image to create/repair resources. Verifier/debug oracles use that explicit import capability outside Core; it is never reintroduced to make gameplay compile. |
| Legacy debugger cartridge payload | State import recognizes the exact retired address-space `_rom` field and drains its bounded payload through AssetExtraction without allocating or retaining a ROM image. Retired graph aliases reject; restored state contains only WRAM/SRAM. |

The source audit includes direct readers, byte/word/long wrappers, indirect CPU
helpers, DMA/VRAM providers, decompression, embedded resources, file APIs, reflection
and native interop. Source searches are an inventory aid, not a substitute for
checking what each receiver/provider can actually return.

## Compile-time regression boundary

Core imports `tools/RuntimeCartridgeBoundary.targets`. `SMROM001` rejects references
to import/reference assemblies; `SMROM002` rejects linked reference-reader/oracle,
raw-renderer and compression sources. Missing old generic readers therefore break
call sites at compilation rather than silently retaining a ROM-capable fallback.

`test-fixtures/runtime-cartridge-boundary/RuntimeCartridgeBoundary.proj` has a clean
case and nine negative assembly/source-link cases. The normal solution build keeps
analyzers enabled; suppressing analysis or restoring readers is not a migration fix.

## Identified conversion evidence

- Projectile file admission: a static loader inventory found pathless hash/codec
  and manifest failures. A focused extracted-file fixture reproduced 96 omissions.
  The shared loader now retains each file path and original codec exception,
  requires complete manifest fields and validates all stock before overrides.
  `--projectile-file-contracts` passes 121 exact-path cases across all 24 PNG/JSON
  resources and their manifest in Debug and Release, preserving defective edits,
  stock hashes, selected identities and every assembled beam transfer. This is
  file-boundary acceptance, not new gameplay or read-discovery evidence.
- Shared palette contracts: a static loader inventory identified twelve older RGB5
  formats without duplicate-field checks. All now use the recursive shared JSON
  validator; room-static and gameplay-base schemas also reject unknown fields.
  The focused `--palette-json-contracts` fixture first reproduced 45 silently
  accepted documents, then passed 1,667 rejection cases across eighteen selections
  (including all seven ending palettes and every named room-effect family).
  Constructed data verifies required fields, frame/row sizes, RGB bounds, nested
  duplicates, world selections, historical casing and four stock-assisted legacy
  migrations. With mutable-memory-only runtime objects, 160 title-FX ticks and
  90 map-cycle ticks preserve every native slot cursor/timer, heat phase, sound
  and music publication, and map frame/timer. Exact software BG and OBJ pixels
  reflect the replacement colors. This verifies these palette contracts and
  selected visual timing paths, not all palette schemas, combat, GPU rendering
  or on-device Android behavior.
- PLM typed-input check: 284 compiled populations / 941 records, owned immutable
  pairs, native allocation/deletion/reuse order, RAM-only scroll execution, bounded
  input validation and two historical scroll-state layouts.
- Import-side source parity: 70 population headers, 173 scroll programs and 17
  dynamic item graphic uploads; every decoded field is compared independently.
- Door/station conversion checks: all opening/closing/closed blue lists, colored and
  grey family lifecycles, mirrored eyes, glass, tube, stations and save electricity;
  12 fallback and 18 resident closing selections. Existing artwork IDs are retained.
- Explicit WRAM check: live low-window instruction/draw data, exact terrain and
  tilemap updates, next-pass slot deletion, all six horizontal-speed words, and
  rejection of peripheral/uncompiled cartridge offsets.
- Installed-only acceptance: Windows host cold startup, six title frames,
  state save/load and exact captured replay pixels pass with its test-owned ROM
  moved out of the installation. The portable Android host passes 90 title frames,
  state save/load, content validation, repair/recovery and save/config preservation
  with no installed ROM. The actual Release AOT APK also passes two fresh cold
  starts on the Retroid Pocket Classic with extracted resources only, as recorded
  in the Android platform acceptance section below.
- Diagnostic failure boundaries: the integration runner installs the shared Windows
  no-dialog policy before fallible work. The Windows verifier catches initialization
  and message-loop failures as well as its asynchronous body. A deliberate startup
  exception verifies actual Win32 error-mode flags, full stderr stack and exit code 1.
- Room-content identity: six selected, decoded domains (characters and CRE, palettes,
  metatiles and CRE, library backgrounds, scrolling skies and room layouts) participate
  in the aggregate. Focused authored fixtures verify insertion-order independence,
  isolated edits, immutable selection snapshots and domain-specific drift warnings.
- Samus-content identity: body pixels, frame selectors and visual offsets, sprite
  layouts, atmospheric attributes, death art/colors and cannon positioning/transfers
  contribute to the selected bundle. An authored fixture verifies 26 independent
  edits, canonical sprite-map records and specific host compatibility warnings.
- Room-actor identity: all 26 PLM visual catalogs and permanent-item character/palette
  uploads have independent named digests. Focused fixtures match production stock
  selection, verify edits in every domain, preserve native draw-run order/boundaries,
  and cover dynamic item palette changes without altering PLM mechanics.
- Shared gameplay identity: starting/common sprite colors, common OBJ pixels and
  X-ray visual operands/item reveals/room overlays contribute separately. Authored
  fixtures cover all four operands, overlay positions and order, canonical records,
  independent host warnings, and equivalent decoded PNG/JSON content.
- Ending identity: all three Mode-7 backdrops, the complete reward icon, character
  sheets and ordered fragments, both BG maps, five OAM composition families, and
  all seven static/animated palettes participate in three named domains. A ROM-free
  authored fixture verifies 53 independent edits, palette inheritance, part and
  transfer order, canonical frame lookup, and equivalent PNG/JSON encodings.
  Computed identities add no debugger-serialized catalog fields.
- Opening-cinematic identity: the installed bundle includes character sheets, all
  four ordered BG pages, portrait/narration/divider maps, every eye frame, nine
  OAM composition families, colors, Ceres flight/destruction maps and Zebes reveal
  art. All five flight, six reveal and three destruction actors contribute their
  selected coordinates. A ROM-free fixture verifies 64 independent edits,
  canonical frame lookup, legacy caret selection and equivalent PNG/JSON encodings.
  Windows, Android and state import use the same selected bundle digest.
- Enemy identity: the selected enemy bundle includes definition-keyed character
  and palette sheets, bounded DMA aliases, and all 35 boss/enemy subdomains.
  Plain and extended OAM frames retain ordered parts and components, display
  bindings, and instruction-selected projectile compositions. Special uploads,
  BG2 write destinations and tile runs, and every installed color-animation row
  contribute to the digest. A ROM-free authored fixture verifies 365 independent
  edits, equivalent PNG/JSON encodings, canonical dictionary order, and the actual
  merged result of older overrides with newer stock content. Computed identities
  add no serialized fields. Windows, Android and state import include this bundle.
- Enemy gameplay acceptance: all 148 retail and five auxiliary headers, 302 ordered
  populations/1,658 placements, 302 graphics sets/425 members and 90 spawn-name
  records match the import oracle. All 68 vulnerability and 118 drop records match;
  every header reference resolves, and 2,449 production vulnerability selections
  plus area-specific freeze duration checks pass. A six-slot linked body/wing
  population initializes with RAM-only memory. An authored PNG edit changes exactly
  its native VRAM pixel while preserving every slot field, palette and quota.
  Drop/pickup/death fixtures pass with explicit installed-color dependencies.
  This verifies the source-identified conversion, not a gameplay search for reads.
- Enemy animation isolation: constructed ordinary, extended and projectile JSON
  replaces frame bindings, silhouette/offsets, component order, tile selection,
  palette attributes and flips. A RAM-only fixture compares all public value
  state, every physical enemy/projectile slot, full WRAM/SRAM, audio/music calls,
  RNG state/consumption and boss-event publication on each frame. Boyon covers
  240 frames, seven native visual frames, 31 Y positions and two bounce callbacks.
  Golden Torizo covers its fixed 72-frame attack, six exact firing frames, 24,336
  physical shot/touch samples and 374 death frames. Exact authored OAM assertions
  verify the selected display sequence rather than merely a different image.
  Boss bit/drop/music publication remains on death frame 372 (zero-based).
  A separate import-only oracle matches 18 Boyon and 1,761 Torizo control words,
  574 visual selectors, and the right-orb program's additional 23 control words,
  ten selectors, six physical frames and seven hitbox lists. This is acceptance
  of a source-identified separation, not discovery of reads through simulation.
- Artifact headers: recordings retain versions one/two and add a bounded named-component
  table in version three. Debugger states retain older envelopes and add the same table
  in version five. Missing legacy fingerprints warn instead of blocking restoration;
  malformed counts, names, hashes, duplicates and truncated data reject explicitly.
- Legacy state import: an authored three-field address-space fixture preserves both
  physical memory arrays while discarding its retired cartridge payload; references
  to that discarded payload reject. The state audit restores schema-three/four
  envelopes, both preserved #350/#353 captures, and 45 exact continuation frames
  (pixels, PCM and acknowledgements). The save manager's missing typed SRAM alias
  rebinds to its captured bus, without inventing or reopening cartridge state.
- Catalog state compatibility: a pre-fingerprint blue-door fixture reproduced the
  field-count failure introduced by a cached identity. All 39 affected room, Samus
  and PLM catalogs now compute their digests without serialized hash fields.
  Explicit schema rules accept the brief cached-hash layouts only for those named
  types and the exact retired SHA-256 field. Focused fixtures restore all 39
  pre-hash, cached-hash and current layouts with identical selected-content
  identity; seven malformed or unrelated layouts still reject. No generic
  unknown-field tolerance or cartridge restoration is involved.
- Required title references: a source-identified fixture reproduced acceptance of
  an unrelated frame in place of a required title selector. The loader and writer
  now check all 31 identities from compiled title behavior, and extraction checks
  the native selectors against that compiled set. All 31 independent substitutions
  reject before drawing; reordered frames and edited OAM parts still load. Existing
  title/OAM parity checks pass.
- Enemy composition override compatibility: source review identified that schemas
  47 through 59 silently ignored authored display bindings because a second version
  list stopped at 46. The authored regression failed on schema 47 before the fix.
  The loader now uses the validated art-only/binding schema boundary. All 57
  accepted schemas (4 through 60) preserve exact frame art, display selections,
  inherited stock frames and reload identity without mutating stock. All 47
  binding-capable schemas reject missing, incomplete, wrong-key, unknown-target
  and cross-bank bindings. These fixtures use only constructed JSON; they do not
  run gameplay, open a ROM or establish compatibility for other asset formats.
- Extended and projectile override compatibility: all 27 accepted extended-frame
  schemas and all 11 enemy-projectile schemas preserve exact edited art and
  inherited newer stock. The extended fixtures cover all ordered components and
  OAM parts, 24 binding-capable schemas, schema-six Spore Spawn aliases, malformed
  bindings and cross-family rejection. Projectile fixtures cover every ordinary
  and program-selected frame, all nine program-frame schemas, historical sets
  that are not current-catalog prefixes, and incomplete/wrong-key rejection.
  Reload identity and stock immutability pass for every schema. This completes
  compatibility checks for these three enemy composition formats only; palette,
  room and other legacy asset formats still require their own review.
- Installation override lifecycle: all 44 declared presentation directories and
  45 public catalog loaders participate in the acceptance fixture. Its 1,091
  override files remain byte-identical across ROM-unavailable startup, the actual
  stock-repair transaction and regeneration from an outdated installation receipt.
  Real OBJ PNG, narration-font PNG, palette JSON, WAV, authored SFX-duration and
  instrument edits remain selected, with exact decoded payload assertions and
  isolated identity changes. A fresh portable Android session binds the edited
  audio/map identities without a cartridge allocation. Corrupt override data
  reports its path and is retained, rather than repaired away or replaced by stock.
  This is installer acceptance, not gameplay discovery or proof that every older
  asset-document schema remains compatible.

These focused checks exercise known conversions. They are not a room playthrough
or proof of visual/gameplay parity for every event.

## Enemy animation ownership

Native frame identities are retained in simulation state because physical hitboxes
and callbacks depend on them. Editable JSON binds those identities to installed
display frames only when drawing. Artwork cannot replace the compiled control
program or change its instruction clock.

| Source boundary | Simulation ownership | Installed presentation |
| --- | --- | --- |
| Ordinary enemy interpreter | Bounded compiled instruction words, durations, branches and callbacks; compiled native visual selectors | `enemy-compositions.json` OAM parts and same-bank display bindings |
| Multipart enemy drawing and collision | Native frame identity, compiled physical components, hitbox lists and shot/touch dispatch | `enemy-walking-pirate-compositions.json` visual components/parts and family-scoped bindings |
| Enemy projectiles and pickups | Compiled definitions/programs, spawn coordinates, velocities, clocks, damage and drop decisions | `enemy-projectile-compositions.json` direct/program-selected OAM parts |
| Mother Brain body animation | Compiled body commands, physical frame identities, movement, pose/form, instruction clock and quake/footstep events | Seventeen installed multipart OAM poses and sixteen BG2 poses; one display binding selects both halves; character, head/neck and special-sprite artwork remains installed |
| Crocomire, Phantoon and Draygon frames | Compiled physical frame selectors, collision lists, callbacks and new-frame write gate | One shared binding selects both OAM components and the matching BG2 writes; native BG2-only stock roots have no OAM |
| Crocomire melting and skeleton | Compiled pass/transfer/erasure scheduling, phase transitions and completion | Indexed melt/skeleton sheets and installed melt/BG2 maps |
| Corpse rotting | Explicit live-WRAM row scheduler, fixed delays and completion callbacks | Installed corpse character sheets processed by the scheduler |
| Kraid and other boss draw hooks | Compiled pose/phase/hitbox/progression selections | Installed BG2, multipart OAM, special uploads and named color catalogs |

The current focused isolation fixture directly verifies ordinary/extended OAM,
projectile art and Torizo death timing. A separate Mother Brain fixture checks
all seventeen body poses, exact OAM/BG2 output, mixed display remaps, native
new-frame write gating and 300 production instruction steps with unchanged
body movement, clock, pose, BG2 counter-scroll and mutable memory. The installed
frame inventory equals the complete compiled body/hand-beam selector set.
Import-only extraction covers 114 OAM components/423 sprites and 252 BG2 runs/
2,035 tile words, with stock-hash, override, malformed-resource and state-layout
checks. Source inspection identified both missing frame catalogs and a missing
compiled-selector dispatch; the focused instruction check failed on that dispatch
before its correction. There is no runtime cartridge access.
The RAM-only melt/corpse fixtures compare authored replacements through both
54-call Crocomire dissolves and all 118 Mother Brain rot calls. They assert every
native erase-column height, HDMA/phase handoff, smoke cadence, row completion and
dust/audio callback, plus 702 production RAM-to-VRAM corpse uploads. The graphics
change while actor/collision values and fixed control clocks do not. Stock corpse
pixels/staging and six initial sprite uploads are compared against an independent
import-only oracle; overrides survive reload and malformed PNGs fail explicitly.

Static inspection also identified missing-resource validation after mutation in
these initializers. Regression assertions failed before the fixes on Mother Brain
and dead Tourian corpse rot-table writes. Crocomire's phase/actor/buffer mutation
was identified directly in the source; the new checks verify its resource errors
leave those values unchanged. Missing/unknown resources
now fail before those changes; all eight Zoomer/Ripper/Skree variants cover absent
and incomplete catalogs. Crocomire JSON also rejects unknown/duplicate/case-aliased
root/cell fields rather than silently losing edits. Required corpse artwork is no
longer optional in the public initializer; compiler errors exposed and corrected
its nullable callers without restoring a cartridge fallback.

Focused commands: `--enemy-effect-resources`, `--crocomire-melt-json`,
`--enemy-effect-isolation`, `--mother-brain-corpse-stock-artwork`.
The boss display fixtures separately verify all 154 Crocomire/Phantoon/Draygon
poses with exact packed OAM and ordered BG2 output, empty stock BG2-only roots,
authored sprite additions, remaps across OAM/BG2 poses and native write gating.
The Crocomire remap assertion failed before the shared-selection fix: OAM drew
the selected pose while BG2 still drew the original physical pose. Phantoon and
Draygon's BG2-only bypass was identified statically and now uses the same binding.
All 208,208 production shot/touch samples retain the physical callback, including
18,015 positive hits; 450 focused instruction steps retain their exact duration/
goto clock and 50 frame admissions. These steps do not exercise complete boss AI.
Twelve exact carry/sign and component-window cases verify the newly authored
BG2-only sprites use native wrapping/clipping. The import-only oracle compares
2,145 sprites, 1,037 BG2 runs/9,260 tile words and 22 compiled collision lists, while the
production draw path receives only RAM. Installation checks cover stock hashes,
selected bindings/identity/reload, missing files and malformed versions; legacy
schema 26 retains its authored data and inherits the 56 new stock binding roots.
Missing composition or selected BG2 catalogs fail before VRAM writes.

Focused commands: `--boss-display-bindings`, `--boss-display-stock`.
Kraid's installed-presentation fixture verifies all 2,048 working-map words,
including the 224 untouched lower-tail words and final 32-word blank row. The
constructed tail assertion failed before correcting the overly broad priority
clear at word 1,792 (expected 9,984, got 1,792); pinned `$A7:AB19` clears priority
only in the copied region. The import-only oracle independently constructs the
expected map and decodes all four head images and 91 control words. Production
construction, head transfer, backdrop uploads and direct/queued HUD restoration
receive only mutable memory and mandatory installed artwork; the obsolete
missing-art/ROM fallback test baselines and permissive read guards are removed.
The head-transfer assertion also failed before removing a nonnative write into
the body working map: `$A7:AF5D` queues head art directly to VRAM. Head admission
now leaves that source map untouched, so later growth/sink uploads cannot inherit
the head image. This correction applies to installed and live-WRAM head sources.

Four authored head replacements retain all compiled physical selectors,
durations, mouth hitbox pointers and sound callbacks over 599 paired production
ticks, 21 exact admissions, three sound events and 96,360 actual mouth-collision
samples. Every hold/termination tick checks the complete VRAM image for unwanted
uploads and verifies the body map is unchanged. The low-half alias fixture checks
the exact live-WRAM-to-VRAM words and that source RAM also remains unchanged.
All seven body/head/backdrop files reject omission in both stock validation and
installation loading. Installed overrides retain their exact selected identity
after reload; stock hash tampering and malformed overrides fail. This is head-program and
transfer acceptance, not complete Kraid AI/progression or GPU-output parity.

The shared room-background and Kraid-head JSON loaders now reject unknown and
duplicate/case-aliased fields before compiling tile words, while retaining
historical unambiguous casing. The unknown-field rejection assertion failed
before the production fix. Exact bit/page-order roundtrips and all 44 invalid
documents pass through the real loaders.

Focused commands: `--kraid-installed-presentation`, `--tilemap-json-contracts`.
These fixtures do not newly verify every other BG2 boss, other corpse family,
GPU output, full battle progression or historical color-document schema. Those
remaining acceptance checks must not be represented as completed by this fixture.

## Room and cinematic JSON admission

Source inspection identified five other Core tilemap compilers bypassing the
shared recursive JSON validator: room metatiles, the six room-FX BG3 pages,
intro eye rectangles, the intro divider and the five Ceres warning pages.
Five installed room-visual loaders also bypassed it: background, sky and metatile
manifests, room layouts and X-ray reveals. Constructed fixtures first reproduced
31 accepted malformed compiler documents and 39 accepted malformed installed
documents. Missing X-ray coordinates and visual operands could silently become
valid zero values instead of reporting an incomplete edit.

All ten paths now use `JsonAssetDocument` and reject unknown or ambiguous fields,
including nested duplicates and case aliases in case-insensitive formats.
Installed record documents require every nonoptional constructor field. X-ray
file admission uses a private required-field document before converting to the
unchanged runtime value type; valid zero coordinates remain supported.
Historical unambiguous casing remains accepted only where already supported.
Invalid selected files remain untouched and errors identify their exact path.

`--tilemap-json-contracts` passes 70 rejection cases for the five newly covered
compilers plus the existing 44 background/Kraid cases. Independent expected-word
comparisons cover all metatile quadrant bits, all six 32x33 room-FX pages, all
four eye rectangles, the 128-cell divider and all five Ceres pages.
`--room-asset-json-contracts <installation-root>` copies only existing extracted
files, rejects 46 invalid manifests/overrides and checks unchanged stock hashes
and selected-content identities. Both focused commands pass in Debug and Release.
Fresh explicit import before and after the fix produces all 1,094 stock files
byte-for-byte identically, including X-ray JSON and every manifest. No format
version or runtime save-state type changed.

The related room, gameplay and intro identity suites pass. The real installation
lifecycle also passes all 44 override directories, 45 catalog loaders and 1,092
files through ROM-unavailable startup, portable host binding, stock repair and
extraction upgrade. Windows Debug and Android Release builds pass with zero errors.

This is admission and stock-format acceptance for the statically identified
loaders, not new gameplay, GPU or per-scene timing acceptance. Core cartridge
capability remains deleted. These tests did not discover remaining ROM reads.

## Samus disk artwork renderer acceptance

The guarded RenderVerification command `--installed-samus-artwork <installation-root>`
loads existing extracted Samus resources without opening a cartridge or invoking an
importer. Its isolated copy contains 26 real edited PNGs: all 24 body atlases, the arm
cannon atlas and death-explosion atlas. The regular installation loader selects those
overrides before the real body DMA, queued NMI upload and pose drawing owners execute.

Every one of the 435 split body definitions is exercised as a pending debugger-state
transfer rebound to the replacement catalog. The fixture checks the exact whole VRAM
image, split sizes and source identities. All 253 initial pose draws additionally check
OAM placement, physical coordinates/subpositions/speeds, collision radii, health, hurt
timers and animation counters. The twelve cannon and five death uploads keep their
queued source, destination, length and queue-clearing behavior.

The pixel oracle requires every visible nonzero index to receive its precise authored
replacement; zero remains unchanged. Each stock, edited and retained-stock-after-edit
packet also receives a complete software-versus-GPU RGBA comparison. Debug and Release
pass on RTX 3090 hardware and WARP: 2,115 comparisons and 272,007 recolored pixels per
backend/configuration. The fixture deletes only its own temporary extracted copy and
does not modify any player data or publish artwork.

This supplies GPU presentation evidence for those finite, statically identified paths,
not discovery of remaining ROM reads or full gameplay/animation-transition acceptance.
Other enemy/boss/cinematic paths remain separate gates. Actual Android startup is
verified separately below; this renderer fixture does not establish Android pixel parity.

## Samus movement and special sequence isolation

The guarded RenderVerification command `--installed-samus-isolation <installation-root>`
adds paired acceptance for the production owners identified in the Samus source audit.
It loads only already-extracted files. A disposable copy receives the same 26 edited
PNGs as the pixel fixture plus real JSON changes to body DMA selectors, OAM X placement,
graphics/landing/posture/drained Y offsets and death colors/selections. Three additional
edited JSON files pass through the production normal-suit, full-body-cycle and Crystal
Flash color compilers. No importer, player installation, INI, save or ROM is opened by
this command. A fresh source bundle may be prepared separately by the explicit importer.

Debug and Release pass 57 finite scenarios with 12,449 complete mutable-state
comparisons and 6,187 production draws. The field-level snapshot includes private fields,
reference identity and cycles, all motion/subpositions, collision radii, pose/history,
animation buffers/timers, health/ammo, sound publications and special-owner state.
It excludes only nonserialized host catalog bindings and the body's five derived
render outputs (four OAM selector/origin fields and the pending split-DMA owner).
Sensitivity checks require mismatched whole/fractional position, animation frame,
collision radius and private Crystal Flash state to fail. Paired WRAM/SRAM and room
collision words also remain equal. Each draw is checked against its own pre-draw
mechanics, not just the other actor.

The fixtures cover both facings in air, full water and lava: running, authored turn
completion, normal/Hi-Jump launch-to-floor trajectories, crouch/morph/unmorph and
knockback. Four low-ceiling cases assert actual allowed/rejected pose expansion.
Special owners complete both 258-call Crystal Flashes (thirty ammo drains and release),
180-call stored-shine expiration, all six shinespark launches through collision/energy
termination and crash/release, both 163-call suit transformations with identical window
geometry, nine 211-call death sequences (three source postures by three suits), 42-call
reserve refill and both drained falling/standing/crouching/release routes. Assertions
require actual motion, collision, resource changes and terminal states, not only equality
or no-crash. Crystal Flash body/bubble colors are checked against the exact selected
catalog values.

The replacements visibly reach 6,185 differing OAM observations, 6,187 differing VRAM
observations and 12,449 differing CGRAM observations. These are changed-buffer counts,
not additional GPU/pixel comparisons; the independent software/GPU PNG test above stays
separate. Two obsolete comments about neighboring-ROM and missing-catalog fallbacks were
corrected; no production algorithm, format, runtime state field or compiled timing changed.

This is bounded mechanics isolation for the listed owners, not exhaustive pose/input,
wall-jump/grapple/X-ray, every environment boundary or enemy interaction acceptance.
The file-boundary checks below cover selected required-reference and version rejections;
the wider installation contract remains open in #541/#549. No tests were used to discover remaining cartridge reads, and
the deleted Core capability was not restored.

## Samus file admission and diagnostics

`SuperMetroid.RenderVerification --installed-samus-file-contracts <installation-root>`
exercises the statically inventoried body, atmosphere, cannon and death loaders with
copied extracted files. Before the fix, the fixture reproduced pathless hash/codec and
catalog errors, accepted unknown or duplicated properties, and seven missing value-type
frame/OAM fields silently becoming zero. The loaders now retain the exact selected file
in the outer exception message and preserve the original exception. Nested component
errors do not get misattributed to the enclosing body manifest. Cannon JSON and PNG
admission have separate contexts, and JSON transfer metadata is checked before PNG
decoding so invalid sizes are attributed to the manifest.

All 34 required PNG/JSON files are tested in Debug and Release: **301 exact-path
rejections and 30 stock-equivalent overrides**. Cases include missing/hash-corrupt stock,
malformed overrides, hash-valid malformed stock before a valid override, missing/unknown/
duplicate JSON fields (including nested value records), unsupported version numbers,
null records, required spritemap/base/DMA/provenance references, invalid image dimensions
and painting unused body characters. Every failing file is retained, missing files stay
missing, and all fixture stock bytes and the final selected identity are unchanged.

Fresh explicit import produces byte-identical output for all 34 Samus files. Release
Android and the Windows checker build; the existing renderer/isolation checks remain
separate acceptance evidence. The console no-dialog policy now also runs inside the
outer catch before fallible work, so a policy initialization failure is guarded too.
These are presentation-file admission changes, not gameplay or ROM-read discovery.
Unsupported version markers are rejected; authentic older installation schemas, debugger
formats, exhaustive frame references and the wider category/platform gates are not all
established by this finite fixture. No Core cartridge capability was restored.

## Android platform startup acceptance

The Release AOT APK passed two independent cold processes on the Retroid Pocket
Classic on 2026-09-30. Its isolated package contained only 1,093 extracted files
(77,480,674 bytes), no ROM, SRAM or debugger state. Both processes reached the real
`TitleScreen` (host frames 1,567 and 1,569), with 60.0 emulation/paint FPS and zero
audio underruns in each final one-second timing window. Diagnostic package and
staging cleanup succeeded; the player's testing package and saves were untouched.
APK SHA-256: `20892A9C5BC0D502AD32C7C12896EFCA19611FAD39AF4EC6A69CD5ADD1CCFF40`.

The reusable `verify-android-rom-free.ps1` tool requires strict validation of both
the source bundle and its ROM-free staged copy before touching the device. The
initial cached fixture had a current installation receipt but an outdated enemy
manifest (48 instead of 65) and a missing required PLM directory. The strict API
preserves the exact failure rather than reducing it to an ambiguous setup screen.
Fresh content was generated only by the explicit asset importer. A sleeping device
also prevents the production focus gate from running; the tool wakes it without
changing settings and requires actual rendered-title timing, not just Activity launch.

The guarded `--extracted-validation-contract` check verifies valid extracted-only
startup plus missing-root, old-receipt, wrong-provenance, index-hash, stale-nested-
manifest and missing-PLM-domain failures. Each rejection identifies its exact path;
the normal startup query still returns null for repair admission. Hashes of every
stock file remain unchanged by validation, and no cartridge is created or imported.
This checks the statically identified shared startup inventory without gameplay probes.

See [Android diagnostic handoff](../ANDROID-TESTING.md) for commands. These results
complete the on-device cold-start gate, not all Android gameplay, long-run performance,
GPU pixel parity, or the remaining content/reference audit.

## Power Bomb and Crystal Flash color isolation

Static inspection of `SamusPowerBombExplosionState.StepCrystalFlashAfterglow`
identified an editable-color dependency in the cleanup condition. The focused
fixture reproduced a black JSON override ending the effect at frame 19 instead
of the stock frame 36. Pinned `bank_88.asm` at `362be646` and `sm_88.c` at
`578f90b3` confirm that $88:A35D tests the native RGB5 registers for zero.
The four reachable $88:8D85 rows have a maximum component of 14+row. That
operand is now compiled independently of presentation, captured on entry to
afterglow, and decremented with the unchanged signed-timer cadence. Stock
color fades remain exact; editing colors cannot change the effect lifetime.

`--installed-power-bomb-isolation <installation-root>` loads already-extracted
data and writes five isolated on-disk overrides: black, white, red, blue and
a varying gradient. Debug and Release pass 655 whole mutable-state/window
comparisons and 621 changed color observations. Every Power Bomb ends on frame
95 and every Crystal Flash on frame 36. The 645 active edited packets per
configuration pass full software-versus-Direct3D RGBA checks on hardware and
WARP after packet serialization. A black backdrop independently checks the
exact edited RGB at the window center, rather than only a changed image.
Every reachable radius (8,192 values) matches the extracted stock lifetime
operand. All stock file bytes and selected identity survive the override test.

The new serialized control counter has an explicit old-layout migration.
All 131 stock effect checkpoints restore and resume both current and legacy
graphs with identical stock state, colors and cleanup frames. Ninety edited
Crystal Flash afterglow checkpoints retain their current control count after
restoring and rebinding stock presentation. Old-layout projections warn and
preserve their captured historical color-fade remainder once. An old modded
capture cannot reconstruct its counterfactual stock elapsed time; the warning
states this limitation rather than silently resetting or rejecting the state.
Reset clears both control and display state. Windows and Android builds pass.

This fixes one source-identified presentation/mechanics coupling in #542/#549.
It does not establish every liquid/HDMA/environmental effect, native backdrop
interference with Crystal Flash, Android pixel parity or whole-game acceptance.
No tests were used to discover cartridge reads, and no Core ROM capability
was added back.

## Remaining #549 acceptance

1. Expand `GameContentIdentity`: it currently fingerprints source provenance,
   compiled Core build, selected audio, maps, projectiles, six room-art domains,
   Samus, all 27 room-actor domains, shared gameplay/X-ray presentation and three
   ending domains, plus the opening-cinematic/Ceres and complete enemy bundles.
   Audit any further resources and compiled/default presentation against that
   inventory rather than assuming a passed subset is exhaustive.
2. Complete required-resource/reference validation across every installed domain
   and review compatibility of older override document formats. Current-format
   override persistence through startup and the real repair/regeneration
   transaction passes as recorded above. Keep missing-resource errors explicit.
3. Review remaining cached/reference diagnostic assumptions separately. Historical
   successful frame probes do not authorize restoring Core cartridge capability.

Do not close #549 or describe the whole game as validated until its remaining
acceptance criteria are satisfied.
