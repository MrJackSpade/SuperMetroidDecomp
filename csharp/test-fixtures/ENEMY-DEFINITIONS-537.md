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
`IRoomEnemyDefinitionFixtureSource` interface. The public `ReadDefinition` ROM
parser remains available to extraction/debug tools, not production room loading.

This is a partial #537 migration. Ordered room populations, drop and
vulnerability tables, enemy-name words, and final presentation bindings still
need separation. The catalog's artwork addresses and palette/spritemap IDs are
stable native references at this stage, not editable gameplay data. No claim of
ROM-free general room loading or completed enemy visual modding is made.
