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
| Generic OAM / enemy OAM | Required installed spritemap/frame catalogs. Generic bus-backed sprite decoding and enemy sprite fallback were removed. Import/reference spritemap oracles remain outside Core. |
| VRAM DMA / queued writes | `ExecuteQueuedMemoryWrite`, `ExecuteHardwareMemoryDmaWrite` and queue drainage require `ISnesMutableMemory`. Installed asset transfers accept validated payloads or `VramAssetId`; legacy source-address identities resolve only bounded named art transfers. Unresolved cartridge windows reject, not read. |
| Background command lists | Core executes decoded background programs. Native command-list byte decoding is AssetExtraction's `LibraryBackgroundProgramImporter`, not a diagnostic-only reader left in Core. |
| CPU open-bus / indirect operand helpers | Explicit WRAM/SRAM/peripheral/latch behavior. A cartridge classification rejects a missing compiled definition; it has no cartridge read capability. The operand helper has no production Core callers; the indirect helper's sole Core wrapper is currently uncalled. |
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
  with no installed ROM. The Android platform project also builds; this is not yet
  a fresh on-device cold-start check.
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
- Extended and projectile override compatibility: all 26 accepted extended-frame
  schemas and all 11 enemy-projectile schemas preserve exact edited art and
  inherited newer stock. The extended fixtures cover all ordered components and
  OAM parts, 23 binding-capable schemas, schema-six Spore Spawn aliases, malformed
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
These fixtures do not newly verify every BG2 boss,
melting/rotting effect, GPU output or historical color-document schema. Those
remaining acceptance checks must not be represented as completed by this fixture.

## Remaining #549 acceptance

1. Expand `GameContentIdentity`: it currently fingerprints source provenance,
   compiled Core build, selected audio, maps, projectiles, six room-art domains,
   Samus, all 27 room-actor domains, shared gameplay/X-ray presentation and three
   ending domains, plus the opening-cinematic/Ceres and complete enemy bundles.
   Audit any further resources and compiled/default presentation against that
   inventory rather than assuming a passed subset is exhaustive.
2. Complete Android on-device cold-start acceptance with the cartridge unavailable.
   Windows real-host and portable Android-host acceptance passed as recorded above;
   neither substitutes for running the APK on the device. This is final platform
   acceptance, not a way to hunt remaining reads.
3. Complete required-resource/reference validation across every installed domain
   and review compatibility of older override document formats. Current-format
   override persistence through startup and the real repair/regeneration
   transaction passes as recorded above. Keep missing-resource errors explicit.
4. Review remaining cached/reference diagnostic assumptions separately. Historical
   successful frame probes do not authorize restoring Core cartridge capability.

Do not close #549 or describe the whole game as validated until its remaining
acceptance criteria are satisfied.
