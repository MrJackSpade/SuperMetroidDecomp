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

These focused checks exercise known conversions. They are not a room playthrough
or proof of visual/gameplay parity for every event.

## Remaining #549 acceptance

1. Expand `GameContentIdentity`: it currently fingerprints source provenance,
   compiled Core build, selected audio, maps and projectiles. Other selected room,
   enemy, Samus, HUD/cinematic/ending and PLM presentation domains must participate
   before it represents the complete installation.
2. Verify Windows and Android cold-start from complete extracted content with the
   cartridge unavailable, including real host bindings and content validation.
   This is final platform acceptance, not a way to hunt remaining reads.
3. Verify current stock/override persistence and complete required-resource
   validation across every installed domain. Keep missing-resource errors explicit.
4. Review remaining cached/reference diagnostic assumptions separately. Historical
   successful frame probes do not authorize restoring Core cartridge capability.

Do not close #549 or describe the whole game as validated until its remaining
acceptance criteria are satisfied.
