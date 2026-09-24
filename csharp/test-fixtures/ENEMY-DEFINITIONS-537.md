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

This is a partial #537 migration. Drop and vulnerability tables, enemy-name
words, and final presentation bindings still need separation. The catalog's
artwork addresses and palette/spritemap IDs are stable native references at
this stage, not editable gameplay data. No claim of ROM-free general room
loading or completed enemy visual modding is made.

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

This remains partial #537 work: drop/vulnerability probabilities and tables,
enemy-name words, presentation bindings, and a broader ROM-free runtime audit
are not finished by compiling room lists.
