# Owner questions from the analyzer cleanup

Each entry records a conflict between sources or a red flag found during the cleanup, the evidence checked, and
the option the cleanup continued with. None of them stopped the run.

## 1. SuperMetroid.Android cannot be loaded by the census on this machine

- **File and site:** `csharp/src/SuperMetroid.Android/*` (listed in `csharp/SuperMetroid.Full.slnx`).
- **Conflict:** `docs/analyzer-cleanup.md` says the census "fails if a project has a real compile error", and the
  cleanup's acceptance requires the census to load cleanly. On this workstation every census run fails with
  `Projects that do not compile: SuperMetroid.Android` (CS0246 on `Android.*`, `Activity`, `View`, `Keycode`,
  `AudioTrack`, `Bitmap`, `Paint`, `Context`). This is an environment gap, not a code error: `dotnet workload list`
  shows no workloads installed, and `csharp/ANDROID.md` says the .NET 10 Android workload, SDK and Java tooling are
  required to build that project. As a result the Android project's own SME findings are not visible in the census.
- **Evidence checked:** the baseline census output (all other projects compile and are listed), `dotnet workload
  list`, `csharp/ANDROID.md`, and the solution file.
- **Option taken:** treat the Android CS0246 set as the one known environmental failure. Each batch's census must
  show no compile error outside `SuperMetroid.Android`, and the Android set must not grow. Android's analyzer
  findings remain uncounted until the census runs on a machine with the Android workload. The final full build
  has the same limitation.

## 2. Samus projectile slots: whole-subsystem representation change

- **File and site:** `csharp/src/SuperMetroid.Core/Samus/Projectiles/` — `SamusProjectileSlot.cs` (17, 20, 29, 46, 50,
  52, 56), `SamusBombProjectileSlot.cs` (20–58), `SamusProjectileTypeWord.cs`, `SamusProjectileDirectionWord.cs`,
  `SamusProjectileRecord.cs`, `SamusProjectileTrailSide.cs`, `SamusPowerBombExplosionState.cs` (radii, speed,
  colours) and `PowerBombRenderSnapshot.cs`: 44 of the subsystem's 66 Core SME634x rows.
- **Red flag:** clearing them replaces the subsystem's representation (packed type/direction/lifecycle words,
  address-valued instruction and spritemap pointers, the multipurpose `Variable`, wrapping unsigned 8.8 Power Bomb
  radii) and touches consumers across Enemies, Bosses, Rendering, `AcceptedFrameBoundary`, `SuperMetroidRuntime`
  and many verification suites. Manifesto §8 names a whole subsystem's representation as the owner's decision.
- **Evidence checked:** the batch 1 worker's report and diff; `docs/cartridge-encoding.md` (Botwoon is the reference
  program migration; slot-word countdown views are scaffolding); the analyzer rows for the folder.
- **Option taken:** proceed, because the cleanup cannot finish otherwise and the destination shape is settled by the
  manifesto and `docs/cartridge-encoding.md`. Order: typed per-slot state (family enum with arbitrary values,
  direction enum plus lifecycle flag, `int` damage, parity checks converting through one codec); a named wrapping
  unsigned 8.8 radius type for the Power Bomb (its wrap at `$88:90DF` is load-bearing and stays exact); typed
  projectile programs following Botwoon's `AnimationTimeline`; the trail timeline. Each step is its own batch with
  ownership covering every consumer.

## 3. SpaceTime beam overrun: committed bank-$80 program bytes

- **File and site:** `csharp/src/SuperMetroid.Core/Samus/Projectiles/SpacetimeBeamCopyDefinitions.cs:28` (SME6342),
  about 28 KB of bank-$80 program bytes committed as source so the SpaceTime beam's copy overrun can be reproduced.
  Same shape, smaller: `csharp/src/SuperMetroid.Core/Samus/Abilities/SpeedBoosterPaletteOverrunDefinitions.cs:19`
  (SME6342), the 32 bank-$9B grapple-handler bytes read as sixteen colours by the speed-booster palette overrun;
  its correct move is the extraction (`SamusFullBodyCycleColorExtractor`) reading them from the player's ROM.
- **Conflict:** manifesto §1 ("Nothing extracted from the ROM is ever committed") and AGENTS.md "Emulation scope
  limits" (behaviour that needs unbounded memory corruption is out of scope and is closed, not approximated)
  against the existing tree, which implements the overrun from committed ROM bytes. Removing it changes what the
  SpaceTime glitch does in the port.
- **Evidence checked:** the worker's report; manifesto §1 and §7; AGENTS.md "Emulation scope limits".
- **Option taken:** the manifesto outranks the existing pattern. The committed bytes go: either the overrun's
  observable effect is bounded and reproduced from data the import pipeline reads from the player's ROM, or, if it
  cannot be bounded, the SpaceTime overrun is removed under the emulation scope limit. The worker assigned to it
  decides between those by investigation and records the evidence.

## 4. Enemy family state as views over multipurpose slot words

- **File and site:** every enemy and boss `*EnemyState` that projects typed properties onto `RoomEnemySlot`
  variable words (Crocomire `CrocomireEnemyState.cs:61-99`, Mother Brain, Rinka, Zebetite, Kraid parts, Shutters,
  Fake Kraid), and `EvirBehavior.ReadAliasedEnemyRamByte` / `RoomEnemySlot.ReadNativeWord`, which read arbitrary
  enemy RAM and so pin every slot word to its exact native value.
- **Red flag:** the destination (`docs/cartridge-encoding.md`, manifesto §4 "Shared state is one storage identity…
  with lifetimes that match the thing") is per-family typed state owned by the family, with slot words written only
  by a snapshot/debug codec. Getting there replaces the enemy subsystem's state representation and requires
  modelling the Evir aliasing read (a gameplay-observable read of other slots' RAM) explicitly. That is a
  whole-subsystem representation change.
- **Evidence checked:** Crocomire, Shutters and Mother Brain worker reports; `EvirBehavior.ReadAliasedEnemyRamByte`;
  the documented slot-word-view scaffolding in `docs/cartridge-encoding.md` ("Countdowns").
- **Option taken:** for the cleanup, typed views over slot words stay where the word is aliased, each view exact
  (`NativeCountdown` views, `int` properties projected with `checked((ushort))`, spatial types). That clears the
  narrow-storage findings at the typed members without changing what any other reader of the word observes. The
  move to per-family owned state, and an explicit model of the Evir read, is recorded as the next step for the
  enemy subsystem and is not taken during this run.

## 5. Enemy slot frame identity: `RoomEnemySlot.SpritemapPointer`

- **File and site:** `DraygonFrame.cs:10`, `KraidPartFrame.cs:11`, `CrocomireBodyFrame.cs:11`,
  `PhantoonPartFrame.cs:11`, the Mother Brain/Baby/Zebetite frame records and `BabyMetroidFrame.cs:10`: each
  exposes `ushort Spritemap`, the cartridge address of its extended spritemap, because the shared
  `RoomEnemySlot.SpritemapPointer` stores that word and shared drawing (`EnemySpritemapDraw`,
  `IEnemyBg2Frames.FindBg2Frame`), collision lookups (`ComponentsAt(ushort)`) and `EnemyProgramAnimation.Run(...,
  Func<TFrame, ushort>)` consume it.
- **Red flag:** the fix replaces the shared enemy slot's frame representation for every boss family at once (a
  typed frame identity in the slot, drawing resolving artwork by frame, collision keyed by frame, the address map
  living only in extraction and the oracles). That is a whole-subsystem representation change.
- **Evidence checked:** Kraid, Crocomire, Mother Brain, Phantoon and Draygon worker reports, all of which left this
  row for the same reason; one worker's attempt to convert the property to a method was rejected as rule evasion.
- **Option taken:** proceed with the destination shape as one cross-family batch once the per-family encoding
  batches are committed: the slot holds a typed frame reference, the families' frame records stop exposing an
  address, and the address-to-frame mapping moves to the extraction and ROM oracles.

## 6. Cinematic sprite scratch word and pre-instruction pointer (Intro, Ceres, Ending)

- **File and site:** `csharp/src/SuperMetroid.Core/Frontend/Intro/IntroDiscoverySprite.cs:60` (`GeneralTimer`) and
  `:73` (`PreInstructionPointer`); consumers in `Frontend/Ending/EndingCreditsState.cs` (1043, 1408-1409, 1451,
  1455-1456), `Frontend/Ending/EndingRewardJump.cs:122` and the Ceres cinematics.
- **Red flag:** `GeneralTimer` is one multipurpose word serving as a program loop counter, egg-particle table
  indices, the Rinka spawn parameter, and the stars' and Ending sprites' 8.8 velocities; `PreInstructionPointer` is
  an address identity beside the typed `ListPreInstruction`. Typing them replaces the shared cinematic sprite
  representation across three subsystems.
- **Evidence checked:** the Intro worker's report; `docs/cartridge-encoding.md` (per-family typed state is the
  destination); the typed `ListPreInstruction` already in the tree.
- **Option taken:** one coordinated batch with ownership over Intro, Ceres and Ending: the loop counter becomes a
  private countdown in the sprite, each other use becomes a typed field on its owner, and the pre-instruction
  address becomes the owner's typed pre-instruction.

## 7. Room identity: cartridge-pointer keys for room headers, states and their content

- **File and site:** `csharp/src/SuperMetroid.Core/Rooms/CartridgeRoomHeader.cs` (15, 16, 20-23), `Rooms/CartridgeRoomState.cs`
  (5, 7-12, 14-19), `Rooms/Backgrounds/LibraryBackgroundInstruction.cs` (15, 17), `LibraryBackgroundProgram.cs:11`,
  `Rooms/Scrolling/ScrollPlms.cs:173`; consumers `RoomLoader`, `DoorTransitionState`, `SuperMetroidGame`, about 40
  verification files comparing `LoadedRoom.Pointer`/`State.Pointer`, and the generators
  `tools/generate-library-background-programs.ps1` and `tools/generate-room-plm-population-definitions.ps1`.
- **Red flag:** rooms, room states and everything they reference (FX, populations, tileset, scroll, x-ray, PLM,
  background list, scroll program, callbacks) are keyed by bank-$8F pointers from `door.DestinationRoomPointer` through
  `RoomHeaderDefinitions.Get(ushort)`. Replacing that is a whole-subsystem representation change.
- **Evidence checked:** the Rooms batch worker's report; the existing `RoomIdentity` type; manifesto §3 and §7 (address
  dispatch row).
- **Option taken:** one coordinated batch later in the run with ownership over Rooms, `RoomLoader`,
  `DoorTransitionState`, `SuperMetroidGame` and both generators: rooms keyed by `RoomIdentity`, states by a semantic
  state identity, the runtime state holding resolved content and typed callbacks, scrollers as typed camera offsets,
  CRE as a typed flag, and all pointer translation in the import generators.

## 8. Room sprite-object pool (bank-$B4 instruction interpreter)

- **File and site:** `csharp/src/SuperMetroid.Core/Rooms/SpriteObjects/RoomSpriteObjectSlot.cs` (16, 30, 32, 34, 36, 38);
  consumers `KraidBehavior.cs:836` (copies `NativeIndex` into an enemy scratch word), `MetroidBehavior.cs:444/453`,
  Draygon, Evir burial and escape-room verification.
- **Red flag:** the slot is an instruction interpreter over cartridge pointers; the fix replaces it with typed
  timelines for every user of the pool, and Kraid's scratch-word copy of the native index needs its own decision.
- **Evidence checked:** the Rooms batch worker's report; `docs/cartridge-encoding.md` (Botwoon is the reference
  program migration).
- **Option taken:** migrate the pool to `AnimationTimeline` programs with ROM oracles in one batch owning the pool and
  every consumer; Kraid's copy becomes a typed reference to the sprite object it spawned.
