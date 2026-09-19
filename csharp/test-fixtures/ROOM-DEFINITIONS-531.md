# Compiled room and transition definitions (#531)

## Load stations

`LoadStationDefinitions` compiles all 134 fourteen-byte records selected through
the seven area lists at `$80:C4B5`. The typed catalog retains unused slots because
station indexes are part of save, elevator, and cinematic behavior. Room and door
pointers remain native identities for the room/door migration that follows; camera
and Samus offsets are application-owned transition mechanics.

Normal Ceres initialization, saved-game loading, and the post-Ceres station-18
handoff use the compiled catalog. `LoadStationEntry.Load` and the addresses in
`LoadStationRomData` remain only as an independent cartridge-parity oracle.

Run `SuperMetroid.Verification --load-station-definitions` from the repository
root. It compares every field in every slot with the pinned retail ROM, rejects
invalid areas/stations, and initializes the production Ceres room through an
address-space guard that throws on any load-station table read.

Remaining #531 work: compiled room headers and state selectors, door connections,
camera/scroll definitions, and setup/main dispatch references. This slice does not
claim that room loading as a whole is ROM-independent.

## Room-state selection

`RoomStateSelectionDefinitions` compiles the ordered condition programs for all
262 retail room headers. Fifty-four rooms have event, boss, Morph Ball/missile, or
Power Bomb branches; the other 208 select their inline state immediately. Normal
runtime loading reads the fixed room area byte, evaluates the compiled program,
then reads the selected state payload. The cartridge interpreter remains available
only for diagnostics and synthetic malformed-program tests.

Run `SuperMetroid.Verification --room-state-definitions` from the repository root.
It compares the native and compiled selections for every room across 75 event,
boss, and inventory contexts, reaches all 323 retail states, rejects non-retail
pointers, and loads production Ceres while its selector bytes are inaccessible.

Remaining #531 work after the selector slice was the fixed header and room-state
payloads, door connections, camera/scroll definitions, and setup/main dispatch
references. Room loading still uses cartridge payloads and visual assets at this
point in the staged migration.

## Fixed room headers

`RoomHeaderDefinitions` compiles the eleven fixed bytes for all 262 retail room
headers: logical room/area identity, map placement, screen dimensions, vertical
scroller values, CRE bitset, and door-list pointer. The production loader now gets
these fields from the typed catalog before evaluating the already-compiled state
program. `CartridgeRoomHeader.Load` remains the independent native parser used by
parity tests and malformed synthetic fixtures.

Run `SuperMetroid.Verification --room-header-definitions` from the repository root.
It compares every compiled field with the pinned retail ROM, rejects the cartridge's
developer area-seven room and arbitrary pointers, and loads production Ceres while
both its fixed header and selector program are inaccessible.

Remaining #531 work after the fixed-header slice was the 323 selected room-state
payloads, door connections, camera/scroll definitions, and setup/main dispatch
references. Visual assets remain cartridge-backed at this point in the migration.

## Room-state payloads

`RoomStateDefinitions` compiles all 323 selected twenty-six-byte room-state records.
That includes stable references to level data, graphics sets, music, FX, enemy
populations/tilesets, layer-two scrolling, scroll data, X-ray data, room main/setup
code, PLMs, and background data. Production room loading now combines the compiled
fixed header, compiled selector, and compiled selected payload without reading any
part of the native header/state record. The referenced visual and level streams
remain cartridge-backed pending their respective #530 child migrations.

Run `SuperMetroid.Verification --room-state-payloads` from the repository root. It
reaches all 323 states through the retail selector graph, compares every payload
field with the pinned ROM, rejects arbitrary pointers, and loads production Ceres
while its complete selected-state record is inaccessible.

Remaining #531 work: compile door connections, camera/scroll definitions, and
setup/main dispatch references, then exercise complete room-entry and camera
trajectories with all migrated definition ranges blocked.

## Door connections

`DoorDefinitions` compiles all 597 physical bank-$83 door records, the shared
overlapping `$83:88FC` elevator pseudo-door, and the exact BTS-index ordering of all
262 room door lists (603 room-owned references). Load
stations, attract demos, Landing Site setup, and type-$9 collision now resolve
doors through that catalog. The latter preserves the cartridge's high-bit BTS
alias before deciding whether to publish an ordinary transition or elevator
contact. The native header reader and physical range catalog remain diagnostic
oracles.

Run `SuperMetroid.Verification --door-definitions` from the repository root. It
compares every physical field and every room-list entry with the pinned ROM,
checks both BTS aliases and every first-out-of-range failure, exercises collision
with an address space that rejects every read, and initializes production Ceres
while both native door-header blocks are inaccessible.

Remaining #531 work: compile camera/scroll definitions and setup/main dispatch
references, then exercise complete room-entry and camera trajectories with every
migrated definition range blocked.
