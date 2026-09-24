# Retail enemy-header migration (#537)

The pinned NTSC J/U v1.0 cartridge and all 323 compiled room states identify
148 distinct bank-$A0 enemy headers through ordered population and graphics-set
records. `RoomEnemyDefinitionCatalog` compiles those complete 64-byte headers as
typed `RoomEnemyDefinition` values. Production room loading uses that catalog
for both graphics-set preparation and enemy-slot initialization; it no longer
reads native header bytes on those paths.

`--enemy-definitions` independently enumerates all referenced pointers from
the ROM's bank-$A1 and bank-$B4 records, compares every field of every compiled
header against the original 64 bytes, rejects unknown IDs, and loads the Ceres
entry room with reads from all 148 source headers forbidden. Constructed verifier
rooms opt into their own authored headers through the explicit
`IRoomEnemyFixtureSource` interface. The public `ReadDefinition` ROM
parser remains available to extraction/debug tools, not production room loading.

This is a partial #537 migration. Fixed drop and vulnerability tables are
already compiled separately; final presentation bindings still need separation.
The catalog's artwork addresses and palette/spritemap IDs are stable native references at
this stage, not editable gameplay data. No claim of ROM-free general room
loading or completed enemy visual modding is made.

## Runtime-created actor headers

Deaths and scripted projectile spawns previously called the public ROM-header
parser after the initial room load. `RoomEnemyAuxiliaryDefinitionCatalog` now
compiles the five additional fixed headers used by those paths: the respawn
placeholder, Mother Brain's Baby Metroid and falling tube, and the two Torizo
orb-drop identities. All ten production callers
use the fixture-aware resolver, so ordinary room headers and auxiliary headers
share one runtime path while synthetic test buses retain their authored bytes.
The public parser remains for debugger/extraction use only.

`--enemy-definitions` compares every field of those five additional headers
against the ROM, confirms they do not duplicate room-selected headers, and
forbids their source bytes during the guarded Ceres room load. This is not a
claim that every later battle phase is exercised by that single guarded room.

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
oracle, rejects unknown IDs, and loads the Ceres entry room with all list-source
bytes forbidden. Synthetic fixtures use `IRoomEnemyFixtureSource`; the Crystal
Flash contact and post-Ceres gunship wrappers forward their authored records.
The full verification suite also exercises those interactions.

This remains partial #537 work: presentation bindings and a broader ROM-free
runtime audit are not finished by compiling room lists.

## Enemy spawn-name snapshot words

The 148 retail headers reference 90 nonzero bank-$B4 name records.
`RoomEnemySpawnNameDefinitions` compiles the six words copied into each spawn
snapshot, retaining the cartridge routine's deliberate omission of source word
five. Production room loading no longer reads these records. Constructed room
fixtures retain their authored bank-$B4 bytes through `IRoomEnemyFixtureSource`.

`--enemy-definitions` independently compares all 90 compiled records against
the pinned ROM and loads Ceres with source-header and source-name reads forbidden.
The catalog does not attempt to reproduce the omitted word or convert these
native snapshot words into editable presentation text.

## Scripted Mother Brain tube placements

The tube-collapse cutscene spawns five enemy records from bank $A9 after normal
room loading. `MotherBrainFallingTubePopulationDefinitions` compiles those
complete 16-byte records and names each cutscene selection; production spawns
no longer reread their cartridge addresses. The fixture-aware path still reads
an explicitly authored constructed record.

`--enemy-definitions` independently compares all forty words, invokes the real
spawn method five times with every source record and enemy header blocked, and
asserts physical slot order, snapshot contents, coordinates and the high-water
count. An altered constructed record separately proves fixture selection.
