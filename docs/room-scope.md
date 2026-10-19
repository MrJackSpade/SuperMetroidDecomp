# Room scope

Room-scoped state in the C# port has no structural lifetime yet: its owners are built once per gameplay runtime,
cleared field by field during room loading, and late-bound to per-room services. A missed field carries state into
the next room; an over-eager clear drops state the cartridge keeps. #1329 replaces the resets with an explicit room
scope: a `RoomScope` built by `RoomScopeConstruction` in the native load order, holding each owner's room-lifetime
state, with the values a destination reads from the previous room named in `RoomTransitionCarryOver`.

This document records the load order, the deliberate carry-overs and the deliberate resets, with their evidence.
[`room-scope-inventory.json`](room-scope-inventory.json) classifies every step of the four room-load bodies and every
`Reset*`, `Clear*` and `ResetForRoom*` declaration in Core. Both were produced by static source inspection.

## Load order

The door path enters through `RoomLoader.LoadPendingDoorDestination`; station, post-Ceres and debug loads enter at
`RoomLoader.LoadCartridgeRoom`. The construction replaces each reset or load line with "construct this owner's fresh
room state here", so this order is kept line for line.

| Phase | Where | Operation |
| --- | --- | --- |
| Door start | `DoorTransitionState` | Minimap re-enable, Samus input lock, elevator and enemy door gates set |
| Pre-load | `LoadPendingDoorDestination` | X-ray direct G-mode demoted to indirect (carried into the destination setup) |
| Pre-load | `LoadPendingDoorDestination` | Destination assets built before the source door is consumed, so a rejected room stays retryable |
| Pre-load | `LoadPendingDoorDestination` | A departing elevator becomes arriving (status 2) before enemy init |
| Pre-load | `LoadPendingDoorDestination` | Load-station and Ceres-arrival records cleared |
| Pre-load | `LoadPendingDoorDestination` | Samus placed; only door-travel displacement cleared (velocity survives) |
| Header | `LoadCartridgeRoom` | Active door, room and assets |
| Room mains | `LoadCartridgeRoom` | HUD character base; treadmill, elevatube and Ceres shaft restart (the shaft reads `RoomMainScratch`) |
| Display | `LoadCartridgeRoom` | Mode 7 transform: the Ceres elevator door publishes its matrix, other doors clear it |
| Level and camera | `LoadCartridgeRoom` | Level data; a new camera inheriting tracking state and fraction words |
| Scroll | `LoadCartridgeRoom` | Layer-2 scroll; previous blocks primed, or the door-opening destination prepared |
| Streamers | `LoadCartridgeRoom` | Background streamer, scrolling sky, landing-site entry |
| Graphics | `LoadCartridgeRoom` | Room graphics, common gameplay sprite rows (`$82:E139`) |
| FX | `LoadCartridgeRoom` | Layer-3 FX (consumes the session RNG value), Ceres haze, sand/spike/treadmill tiles, palette FX |
| Library BG | `LoadCartridgeRoom` | The room state's library-background program |
| Teardown | `LoadCartridgeRoom` | PLM pool, bombs and Samus projectiles destroyed (`$82:E43A-$E472`) |
| PLMs | `LoadCartridgeRoom` | Runtime services (first instance), PLM population |
| Door ASM | `LoadCartridgeRoom` | Door setup code, then setup-callback PLM spawns |
| Door closer | `LoadCartridgeRoom` | Door-closing PLM after room PLMs, before enemies |
| Beam | `LoadCartridgeRoom` | Beam tiles and palette before enemy init (`$82:E4CC`) |
| Enemies | `LoadCartridgeRoom` | Runtime services (second instance), the room's `RoomEnemySystem` built with the door gates clear, then `Load` |
| Post-enemy | `LoadCartridgeRoom` | Ridley minimap, escape effects, gate projectile requests applied after the pool clear, pre-Phantoon projectile, enemy room effects, Tourian statues, graphics uploads, then the carried door gates re-asserted over the loaded enemies |

Each room builds its `RoomEnemySystem`, with every species owner, at the enemy step. `Load` binds the room's load
inputs (definitions, services, video memory, the Rinka and shutter load cameras) and loads the population once; a
second call fails. The species resets it used to run are gone: a fresh owner holds exactly what each restored (audited
statically, member by member), so none survives into the next room. PLM setup reaches the destination's enemies through
the room under construction and fails if it does so before the enemy step, where the cartridge's enemy load would
overwrite what it wrote.

`RoomPlmSystem.Reset` unbinds the Chozo statue, clears every slot, the sound queue, the gate requests, the bindings, the
tilemap updates and the station events, then resets the Mother Brain glass, collectible and Bomb Torizo hand state.

## Deliberate carry-overs

Each must become a named `RoomTransitionCarryOver` member or stay a field of a session owner.

- **Elevator journey** (`$0E16/$0E18/$0799`): the flags, status and direction survive the source teardown on
  `EnemySession` (`PrepareElevatorArrival` promotes status 1 to 2).
- **Enemy session words** never cleared by enemy loading live on `EnemySession`: the shared fade counter, Ceres's
  escape progress, the critical-energy drop bias (`$0E1E`), the language flag, the random enemy counter, the death
  quota an empty population keeps, the staging surfaces, the Mode 7 transfer residue, the OBJ character table and the
  installed enemy artwork.
- **`RoomMainScratch`** (`$07E1`) is never cleared by room loading (8933bb109). The Ceres hall debris timer and the
  elevatube fraction depend on it.
- **Camera tracking state and fraction words** survive door loading (`ScrollBoundaryCamera.InheritTrackingState`,
  `RetainSubpositions`; 80cc097fc, #1266). `InheritedLayer1Position` supplies them when there is no previous camera.
- **Samus velocity** survives; only `ExtraDisplacement` is cleared.
- **Beam charge, flare counters and the charge palette** survive `ResetProjectileData`: they live on the session
  `BeamChargeState`, which each room's `SamusProjectileSystem` receives at construction.
- **Projectile slot residue**: the projectile-data reset clears a slot's whole position words but not its fraction
  words, and leaves the Spazer special-attack phase (`$0CA4`). `RoomTransitionCarryOver.ProjectileResidue` carries
  both into the next room's slots; the 100% movie diverges at update 23200 without them.
- **Room-FX session state**: the Fireflea darkness state and the COLDATA shadows are never cleared by room loading;
  they live on `RoomFxSession`, which each room's `RoomLayer3FxState` reads, with the host's installed FX catalogs.
- **Palette-FX handler state**: the handler enable bit (`$1E79`) and the Norfair heat palette phases survive room
  loading on `PaletteFxSessionState`, which each room's `RoomPaletteFxSystem` reads.
- **X-ray direct G-mode** is demoted, not reset.
- **Elevatube script movement ownership** survives the door fade (3f9fbfb58, #391).
- **Door and elevator transition gates** (`$0797`, `$0795`) are owned by door-transition code for the whole transition.
  The destination's enemies load with them clear, as the enemy load cleared them, and `RoomTransitionCarryOver.DoorGates`
  re-asserts them once the enemies are loaded, where `DoorTransitionState` used to. The Etecoon fanfare depends on them
  (0188ab9a2, #1194).
- **Enemy residue**: enemy projectile deletion (`$86:8016/8154`) clears a slot's fractions but leaves its whole
  coordinates and velocity for the next family initializer, and an empty population keeps the first-free enemy index.
  `RoomTransitionCarryOver.EnemyResidue` carries both into the next room's enemy system; the 13%, 100%, lowc and Ridley
  movies diverge in enemy projectiles without them.
- **Session RNG and progression** stay session-lifetime (#1330); layer-3 FX reads the RNG value during the load.

## Deliberate resets

- **Room PLMs never survive** a door: each room builds its own `RoomPlmSystem`.
- **Samus projectiles and bombs never survive** a door: each room builds its own `SamusProjectileSystem` and
  `SamusBombProjectileSystem` at the teardown step, before the beam upload, and the installed projectile artwork is
  bound to them there. Samus's speed and shinespark echo storage is cleared at the same step.
- **Room FX, animated tiles, Ceres haze and the Tourian statues never survive**: the FX step builds the layer-3 FX
  (through `RoomLayer3FxState.ForRoom`), the Ceres haze, the sand, spike and treadmill tiles and the palette FX
  (`RoomPaletteFxSystem.ForRoom`); the header step builds the Wrecked Ship treadmill tiles a door's setup code may
  start; the post-enemy step builds the statue sequence. Owners that loaded in place have only construction-time
  loading now, so none can be reloaded. The host tester's Tourian unlock restarts the live room's statue sequence.
- **Room mains never survive**: the header step builds the Maridia elevatube and Ceres elevator shaft owners, active
  only where the room state selects them. Their room-main words other than `RoomMainASMVar1` are written before they
  are read, so a fresh owner is the cartridge's state; door setup code acts on the destination's owners.
- **Enemies, their projectiles and sprite objects never survive**: each room builds its own enemy system, whose
  bank-$86 projectile pool and sprite objects start empty (9d81b919c added the pool's destruction).
- **Stale Mode 7 math is cleared** on ordinary doors.
- **Order-sensitive**: gate requests created during PLM setup are applied after the room's enemy system is built and
  loaded, so the closed gate survives into the first viewport.

## Owner classification

| Owner | State | Classification |
| --- | --- | --- |
| `ActiveRoomState` | door, room, assets, level, camera, streamer, sky, landing site | room (the camera inherits named carry-over) |
| `ActiveRoomState` | `RoomMainScratch`, `InheritedLayer1Position`, `DoorTransitionMainScreenLayers` | session carry-over |
| `RoomEnemySystem` | slots, count, processing window, interactive bodies, graphics set, draw list, room status, earthquake | room (door gates re-asserted from the carry-over after load) |
| `RoomEnemySystem` | species state | room |
| `ElevatorBehavior` | actor states, arrival slot | room |
| `EnemySession` | elevator journey, fade counter, Ceres status, drop bias, language, random counter, death quota, staging, Mode 7 residue, OBJ table, artwork | session carry-over |
| `EnemyProjectileEngine`, `RoomSpriteObjectPool` | pools | room |
| `RoomPlmSystem` | slots, sounds, gate requests, bindings, tilemap updates, station events, Chozo references | room |
| `SamusBombProjectileSystem` | slots, bomb counter, cooldown, power-bomb explosion | room |
| `SamusProjectileSystem` | slots, trails, counter, combo, earthquake | room |
| `BeamChargeState` | charge, flare, charge palette | session carry-over |
| `RoomLayer3FxState`, `RoomPaletteFxSystem`, sand/spike/treadmill tiles | FX state | room |
| `RoomFxSession` | Fireflea state, COLDATA shadows, installed FX catalogs | session carry-over |
| `PaletteFxSessionState` | handler enable bit, heat palette phases | session carry-over |
| Treadmill, elevatube and Ceres shaft room mains | active flag, script state | room, with the elevatube exit carried |
| `CeresHazeState`, `TourianStatueSequence` | presentation and sequence | room |

The earthquake words are reset in two owners, `RoomEnemySystem.Earthquake` and `SamusProjectileSystem.Earthquakes`.
Whether they are one native storage identity is #1330's decision; both are room-lifetime here.
