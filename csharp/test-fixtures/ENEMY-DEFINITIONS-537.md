# Retail enemy-header migration (#537)

The pinned NTSC J/U v1.0 cartridge and all 323 compiled room states identify
148 distinct bank-$A0 enemy headers through ordered population and graphics-set
records. `RoomEnemyDefinitionCatalog` compiles those complete 64-byte headers as
typed `RoomEnemyDefinition` values. Production room loading uses that catalog
for both graphics-set preparation and enemy-slot initialization; it no longer
reads native header bytes on those paths.

`--enemy-definitions` independently enumerates all referenced pointers from
the ROM's bank-$A1 and bank-$B4 records, compares every field of every compiled
header against the original 64 bytes and rejects unknown IDs. Constructed
verifier rooms opt into their own typed authored records through
`IRoomEnemyFixtureSource`. Native header parsing belongs to
`RoomEnemyDefinitionImporter` in AssetExtraction, not Core.

As of 2026-09-30, the enemy gameplay definitions and populations are compiled,
and presentation binds to installed PNG/JSON/audio catalogs. Native artwork
addresses and selectors remain fixed identities for those bounded bindings;
they do not grant permission to read cartridge memory or edit gameplay data.
The focused acceptance below verifies #537's conversion. Shared installation
and remaining cross-domain/platform acceptance are tracked under #549; this is
not a claim that every battle event has been visually validated.

## Runtime-created actor headers

Deaths and scripted projectile spawns previously called the public ROM-header
parser after the initial room load. `RoomEnemyAuxiliaryDefinitionCatalog` now
compiles the five additional fixed headers used by those paths: the respawn
placeholder, Mother Brain's Baby Metroid and falling tube, and the two Torizo
orb-drop identities. All ten production callers
use the fixture-aware resolver, so ordinary room headers and auxiliary headers
share one runtime path while synthetic test buses retain their authored bytes.
The parser belongs to AssetExtraction only.

`--enemy-definitions` compares every field of those five additional headers
against the ROM and confirms they do not duplicate room-selected headers.
Production verification uses RAM-only memory, not a cartridge reader with a
list of forbidden addresses.

## Ordered room populations and graphics sets

All 323 room states select 302 distinct bank-$A1 population lists containing
1,658 ordered placements and 302 bank-$B4 graphics sets containing 425 ordered
members. `RoomEnemyPopulationDefinitions` and
`RoomEnemyGraphicsSetDefinitions` compile those records, including the byte
after each population terminator that becomes the enemy-death quota. Production
room loading no longer reads either source list. The empty-population early
return still preserves the previous first-free index and death quota.

`--enemy-room-lists` enumerates every selected pointer from compiled room states,
compares each ordered record, terminator, and quota against the independent ROM
oracle and rejects unknown IDs. Synthetic fixtures use `IRoomEnemyFixtureSource`; the Crystal
Flash contact and post-Ceres gunship wrappers forward their authored records.
The full verification suite also exercises those interactions.

The production loader uses these ordered records directly. No ROM parsing
fallback remains in Core; see `ROM-FREE-SOURCE-ACCESS-549.md` for the source/type audit.

## Enemy spawn-name snapshot words

The 148 retail headers reference 90 nonzero bank-$B4 name records.
`RoomEnemySpawnNameDefinitions` compiles the six words copied into each spawn
snapshot, retaining the cartridge routine's deliberate omission of source word
five. Production room loading no longer reads these records. Constructed room
fixtures retain their authored bank-$B4 bytes through `IRoomEnemyFixtureSource`.

`--enemy-definitions` independently compares all 90 compiled records against
the pinned ROM. The linked-slot acceptance below checks the actual production
spawn snapshots with a RAM-only provider.
The catalog does not attempt to reproduce the omitted word or convert these
native snapshot words into editable presentation text.

## Scripted Mother Brain tube placements

The tube-collapse cutscene spawns five enemy records from bank $A9 after normal
room loading. `MotherBrainFallingTubePopulationDefinitions` compiles those
complete 16-byte records and names each cutscene selection; production spawns
no longer reread their cartridge addresses. The fixture-aware path still reads
an explicitly authored constructed record.

`--enemy-definitions` independently compares all forty words, invokes the real
spawn method five times with RAM-only memory, and
asserts physical slot order, snapshot contents, coordinates and the high-water
count. An altered constructed record separately proves fixture selection.

## Focused gameplay and presentation acceptance

Run `--enemy-gameplay-acceptance` from the repository root. It combines the complete
header/list oracle comparisons with all 68 vulnerability and 118 drop records.
Every referenced vulnerability, drop and spawn-name identity in all 153 headers
resolves to compiled data. The production shot selector matches 2,449 independent
charged, uncharged, non-beam and default-pointer cases; the freeze handler retains
300 frames in Norfair and 400 in every other area.

The real six-slot Ki-Hunter body/wing population at `$A1:8FC5` initializes through
`RoomEnemySystem.Load` using `SuperMetroidAddressSpace.CreateWithoutCartridge()`.
Assertions cover every ordered spawn word, physical slot index, complete header,
health, body/wing coordinate and tile aliases, first-free offset and death quota.
An actual PNG edit, applied consistently to the shared DMA aliases, changes exactly
the authored planar pixel at its native VRAM destination. Every public slot field,
palette, allocation and quota remains identical. The edit survives a fresh catalog
load without changing stock files.

The same command verifies drop selection, five pickup effects, strict contact and
lifetime boundaries, grapple collection delay, the native slot-zero quirk and
death conversion. Its Pseudo-Screw contact fixture supplies an explicit synthetic
suit-color catalog rather than relying on the removed cartridge fallback.

Presentation persistence through ROM-unavailable startup, stock repair and installer
regeneration is covered by `--override-installation-lifecycle` in
SuperMetroid.IntegrationVerification: all 44 override directories, including enemy
artwork and audio, retain their selected edits. See the source audit for its scope.

For an enemy graphic edit, copy its `enemy-XXXX-tiles.png` from `game/enemy-tiles`
to the corresponding `overrides/enemy-tiles` directory, retain indexed PNG geometry,
and restart. Definitions sharing an equal-size DMA source require matching edited
pixels in each alias file. Colors live in the corresponding RGB5 JSON; combat
stats, populations, hitboxes, callbacks, vulnerabilities and drops are not mod files.
Audio replacement and authored SFX instructions are documented in `csharp/README.md`.
