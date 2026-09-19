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
