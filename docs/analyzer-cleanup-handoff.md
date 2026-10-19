# Analyzer cleanup handoff (single agent, about 2.5 hours)

You are continuing the analyzer cleanup on `main` in `C:\git\SuperMetroidDecomp`. You work alone. Do not spawn
subagents or workflows. Work for about 2.5 hours, then stop at a clean, committed and pushed state and append your
results to the progress log at the end of this file. Do not ask the owner anything during the session.

## 1. Read before touching code

1. `docs/manifesto.md`, in full. Section 8 ("Rules for agents") is binding on you, and section 7 ("Fix versus
   satisfy" and "When an allowance is legitimate") decides every fix.
2. `AGENTS.md`, in full. Pay particular attention to these sections:
   - "Analyzer findings are addressed, never grandfathered"
   - "Agent turnaround time"
   - "Process-boundary error handling"
   - "Identities, definition data and cartridge addresses"
3. `docs/analyzer-cleanup.md` (census mechanics, plus the scope note at the top), `docs/cartridge-encoding.md`,
   `docs/spatial-types.md`, `docs/ppu-layout-boundary.md`, `docs/spc-memory-boundary.md` and
   `docs/godot-architecture.md`.
4. `owner-questions.md`. Every entry has the owner's answer. The standing rule from question 2 is: **whether to
   comply with an analyzer rule is never an owner question. Do what the analyzer asks**, including whole-subsystem
   representation changes. Nothing is deferred to "a later run".

## 2. Scope and current state

- The SME analyzer rules apply **only to `SuperMetroid.Core` and `SuperMetroid.AssetExtraction`**. This is an owner
  decision, and the scoped section at the end of `csharp/.editorconfig` implements it. Every other project
  (verification suites, hosts, audits, tooling, test-support) only has to keep compiling with meaningful assertions.
  Its own SME findings are not counted, and you do not clean them.
- At commit `c77b2c1c9` the census lists **719 sites**:

  | Project | Rule | Sites |
  | --- | --- | --- |
  | Core | SME6347 narrow storage | 502 |
  | Core | SME6292 bus parameters | 12 |
  | Core | SME6341 address dispatch | 7 |
  | Core | SME6342 hex-word arrays | 6 |
  | Core | SME6346 flags casts | 5 |
  | Core | SME6349 wrapping decrements | 4 |
  | AssetExtraction | SME6347 | 175 |
  | AssetExtraction | SME6274 | 6 |
  | AssetExtraction | SME6276 | 2 |

- Largest remaining Core areas:

  | Area | Sites |
  | --- | --- |
  | Enemies (shared and species) | about 150 |
  | Samus/Projectiles | 53 |
  | Rooms/Plms | 35 |
  | Rooms (top level) | 27 |
  | Rooms/LevelData | 23 |
  | Samus/Movement | 20 |
  | Frontend (top level) | 18 |
  | Samus (top level) | 17 |
  | Fx | 15 |
  | Rooms/Doors | 10 |
  | Bosses/Ridley | 10 |

- `SuperMetroid.Android` never compiles on this machine because the Android workload is missing (owner question 1).
  That is the one accepted environmental failure. Every other compile error is real.

## 3. Hard rules

These are condensed from the manifesto and AGENTS.md; the full text wins.

- **Enforcement.** Never touch the analyzer sources, `.editorconfig`, `Directory.Build.*`, `module-map.json`, the
  catalog-suffix list, or the definition of any boundary or owner attribute.
- **No bulk excuses.** Never add baselines, `NoWarn`, assembly-wide suppressions or placeholder justifications.
- **Every allowance or attribute** (`// cartridge-encoding: allow(...)`, `[SnesEncodingOwner]`,
  `[ArchitectureReview]`, boundary attributes) needs a site-specific reason naming the thing. An owner attribute is
  legitimate only on the type that is by construction the single codec of that encoding. **Never put one on a type
  that stores state just to clear that state's storage finding.** That was rejected several times this run.
- **No hiding in catalogs.** Never move hex addresses or logic into a catalog-named file (`*Definitions.cs`,
  `*Catalog.cs`, `*RomData.cs`, …), **including existing Core catalog files**. Cartridge addresses belong in
  `SuperMetroid.AssetExtraction` or in verification oracles.
- **No new findings.** No rule may rise. Common traps this run:
  - SME6276: arithmetic or `++` on an enum. Use an explicit member list or switch.
  - SME6273: formatting an enum with `X2`/`X4`. Format the codec's integral value.
  - SME6280/6281: widening one coordinate of an X/Y pair. Use one spatial value.
  - SME6300: reading `SnesPpuLayout`/`SnesVram` layout constants outside the boundary type.
  - SME6202: passing a manual parameter name to `Ensure`.
- **No behaviour change for any input the old code accepted**, including loaded, imported or corrupt values. A
  GameTime change was rejected for making a loader throw. Widening needs the five conditions in
  `docs/cartridge-encoding.md`, recorded per field. A field kept narrow names its load-bearing wrap and keeps it in a
  named operation (`NativeCountdown`, a named wrapping method, or a spatial type).
- **No forward-looking comments.** Comments describe what the code does now. Never write "destination", "once…",
  "until…", "interim", "to be removed", "step" or "later". This is a global owner rule.
- **Never convert a property into a method** to escape a rule.
- **Dead files.** When a change leaves a file with no reference anywhere in `csharp/` or `tools/` (confirm with
  `git grep -n --untracked -w <TypeName> -- csharp tools`), delete it with `git rm` in the same commit. The owner
  authorized this.
- **Tests.** Every wrap-sensitive or behaviour-changing conversion gets a focused test through the production path,
  and every address-dispatch conversion gets a ROM-oracle check. Never weaken, skip or delete a test to make
  something pass. You cannot run verification commands (projects are red). Name the command that covers each test in
  the commit message.
- **Do not run:**
  - full movie replays
  - `tools/verify-all.ps1`
  - exploratory tests
  - the Discord update drain, unless this machine owns it: one machine runs the drain after every push, and the
    agent's memory on that machine records it
  - `dotnet build` of the solution (the census is the compile check)
- **Never guess APIs.** Confirm every member you call exists with `git grep -n -w <member> -- csharp`. Most compile
  errors this run were guessed members or missed consumers. Before changing any member's type, grep **every**
  consumer across `csharp/` and `tools/` (not only Core) and adapt each one.
- **Genuine conflicts.** A real conflict between two sources (manifesto, docs, AGENTS.md, existing pattern), or a
  gameplay-behaviour red flag, goes into `owner-questions.md` as a new numbered entry: the file and site, the
  conflict, the evidence, and the option you chose. Then continue with that option. "The fix is big" is not a
  conflict.

## 4. The work loop (one batch at a time)

A batch is one subsystem or one coherent family of rows, sized to finish in 30 to 45 minutes.

1. **Pick the batch** from section 5, in order.
2. **Edit everything for the batch**: Core or AssetExtraction, plus every consumer anywhere, plus the focused tests.
3. **Run the census once.** It takes 4 to 9 minutes. Run it in the background and, while it runs, read ahead for the
   next batch:

   ```
   dotnet build csharp/src/SuperMetroid.EnsureAnalyzer -c Release
   dotnet run tools/sme-census/SmeCensus.cs -- csharp/SuperMetroid.Full.slnx census-<n>.tsv
   ```

   Keep the TSVs outside the repository, for example in your temp directory. Never commit them.
4. **Validate the census:**
   - There are no compile errors except `SuperMetroid.Android`.
   - Your batch's rows went down.
   - No rule went up. Compare the per-rule site counts with the previous census:
     `cut -f1,4 census.tsv | sort -u | cut -f1 | sort | uniq -c`.
   - Also check that no new `(rule, file)` pair appeared.
5. **If the census fails, fix and re-run.** Do not commit a red census. If one row's fix turns out to need a large
   unrelated change, revert just that row to `HEAD` (`git diff` it first, then restore the original text with your
   editor; do not run `git checkout` over other files) and record it in the progress log.
6. **Commit and push.** Commit per subsystem. The message follows the existing log style (`git log -20`): a subject
   such as "Clear <subsystem>'s cartridge encodings in Core", then a body that states what each representation
   became, the tests with their command names, and `Census (<project>): <rule> <before> -> <after>; no rule rises.`
   Then `git push origin main`. There is no PR, and you work directly on `main`.
7. **Append a line to the progress log** at the end of this file.

The `post-commit` hook prints how much of your time was spent waiting on tools. The censuses are mandatory. Do not
add any other slow step.

## 5. Work queue, in order

Every file is free: nothing is in flight and no other agent is working. The exact edits below come from the last
workers' reports. Verify each one against the current code before applying it.

### 5.1 Unblock rows whose fix was already designed

These rows were blocked only by file ownership, which no longer applies.

1. **Ridley state rows** (`Core/Bosses/Ridley/RidleyEnemyState.cs`, `RidleyTailSegment.cs`), blocked only by
   `csharp/src/SuperMetroid.Verification/Rooms/RoomEnemiesVerification.cs`:
   - `MovementAnimationEnabled` → `bool`:
     - `RoomEnemiesVerification:961` becomes `AssertTrue(...)`.
     - Writers `CeresRidleyBehavior` (about lines 180 and 1039), `RidleyBehavior` (about 210 and 1364) and
       `LookupStream5Verification` (about 1840) write `true`/`false`.
     - `RidleyFullMovieVerification` (about 1332) projects `? (ushort)1 : (ushort)0`.
   - Whip targets (`:67/69`) → a typed idle-aware whip target that keeps the `$FFFF` idle ordinal comparisons.
     `RoomEnemiesVerification:1068–1075` sets and compares `0xffff`.
   - `RidleyTailSegment.Angle` → `SnesAngle`; for example `RoomEnemiesVerification:1067` becomes
     `SnesAngle.FromRaw(0x4000)`, and `CeresRidleyBehavior` (about 1158) uses the angle's table index.
   - `RidleyTailSegment.MovementDirection` → `bool Clockwise`; the `^=` and `|=` sites in `RidleyTail` change
     accordingly.
   - `CommonDrawPaletteIndex` → `SnesObjAttributeWord`; `RoomEnemiesVerification:1112–1113` compares `.PaletteBits`.
   - `CeresEscapeTextDelayTimer` → `NativeCountdown`:
     - The hand-written wrapping decrement at `CeresRidleyBehavior` (about 1693) becomes `Decrement()`, acting on the
       outcome.
     - `RoomEnemiesVerification:1272` compares `.RawValue`.
   - `CeresStatus` (`RidleyEncounter.cs:40`, `CeresEncounter.cs:14`, `Enemies/ICeresEncounter.cs`): values are
     0/1/2 plus a `$8000` rotation bit. It becomes a closed `CeresStatus` enum plus a separate rotation flag, owned by
     `RidleyEncounter`. Adapt `CeresRidleyBehavior`, `CinematicGamePhases`, `GameplayGamePhases`, `RoomMainRoutines`,
     `SuperMetroidRuntime`, `LookupStream5Verification` (about 156, 181, 1836 and 2050) and `RoomEnemiesVerification`
     (about 923, 938, 1149, 1168 and 1305).
2. **Crocomire hitbox callbacks** (`Core/Bosses/Crocomire/CrocomireBodyCollisionHitbox.cs:5`): typed
   `CrocomireHitboxTouch`/`CrocomireHitboxShot` reactions, plus one `CrocomireHitboxCallbackCodec` that is the only
   `[SnesEncodingOwner]` (follow `RidleyHitboxCallbackCodec`). Consumers:
   - `CrocomireBehavior` (about 2398)
   - `CrocomireCutsceneVerification` (about 56 and 69)
   - `CrocomireBodyCollisionDefinitionsVerification` (about 135 and 137)
   - `Bosses/BossDisplayBindingsOracleVerification.cs:91`, where `box.TouchAi/ShotAi` becomes the codec's words
3. **Rendering leftovers:**
   - `Core/Hardware/OamEntry.cs`: X/Y become one `Position16<OamPixels> Position`, built in `OamBuffer.GetEntry`.
     Consumers: `SnesObjRenderer.DrawSprite`, `LookupStream1Verification` (about 1706 and 2980),
     `CeresDoorQuakeDefinitionsVerification` (about 76 and 109), and every `GetEntry(...).X/.Y`.
   - `Core/Rendering/ColorAddWindow.cs` RGB → range-checked `int`. In
     `test-support/VerificationAccess.Core/Rendering/SnesGameplayFrameRendererConstants.cs` (about 380–382), pass
     `checked((byte)window.Red)` and the same for green and blue.
   - `Core/Hardware/VramWriteEntry.cs` `EncodedVramDestination` → `VramDmaDestination`. The readers become
     `.Destination.Word` / `.WordIncrement`:
     - `CeresEscapeVramTransferDefinitionsVerification` (about 93 and 121)
     - `RoomSkyTilemapsVerification` (about 178 and 200)
     - `RenderingAndRoomsVerification` (about 276)
     - `LookupStream1Verification` (about 598)
     - `LookupStream2Verification` (about 1639 and 1665)
     - `LookupStream5Verification` (about 483)
     - `CrocomirePresentation1193Verification` (about 76, 117 and 121)
     - `EnemyStagingSurfaces` (about 65)
     - `RoomEnemiesVerification` (about 815)
4. **Fx and Rooms leftovers:**
   - `Fx/PaletteFx/GradualColorChangeCounter.Numerator` → `int` with `Advance()`/`Reset()`. Update
     `KraidBehavior` (about 1711, 1722, 1546, 1689 and 1725) and `CartridgePaletteTransition` (about 42 and 61).
   - One queue-limit quantity type (or a validated `int`) replaces `byte MaximumQueued` on every `*SoundRequest`
     (Enemy, Samus, PLM, Fx, PaletteFx) and on `CartridgeAudioState.QueueSound` /
     `QueueSoundAndGetAccumulator(..., byte)`. Consumers: `DoorGamePhases` (about 111–127), `FrontendSession`
     (about 273–299) and `GameplayAudioFramePublication`.
   - `Rooms/Scrolling/Layer2ScrollModes` → a per-axis struct (kind, factor, streams), decoded once. Readers:
     `LookupRoomStateSettingsVerification` (about 107 and 109), `MaridiaPipeTerrainAudit` (about 23), and the
     `RoomStateDefinitions` `new(0x81, 0x01)` constructions.
   - `Rooms/Backgrounds/LibraryBackgroundExecutionResult.Bg3CharacterBaseWord` → a typed BG3 character-base layout
     choice inside the PPU-layout boundary. Adapt `RoomLoader` (about 989) and `LibraryBackgroundVerification`
     (about 156).
   - The liquid-surface family in `Fx/RoomLayer3FxState.cs` and `RoomLayer3FxRenderSnapshot.cs`:
     - The surface base becomes one fixed-point value; surface and target Y become
       `Position16<RoomPixels>` with X = 0 (the existing pattern); the velocity becomes signed 8.8; `tidePhase`
       becomes an `int` with a named wrapping advance.
     - Readers: `LowerNorfairHandVerification` (about 77–112), `NativeSnapshotImportVerification` (about 255–258),
       `RidleyFullMovieVerification` (about 1139), `TourianStatueSequence` (about 242),
       `SamusLiquidPhysicsState.ConfigureWater`, `RetailRoomPublicationTests` (about 54) and `ChozoStatueBehavior`
       (about 266–271).
5. **Audio leftovers:**
   - `SoundEffectId.Value` → `int`; `BeamFiring` (about 175 and 227) carries a `SoundEffectId`.
   - `CartridgeAudioState.MusicDataIndex` → `int`, with `IDoorMusicUploadNmiSource.AcceptedNmisDuringUpload(int)`
     and its implementations.
   - `CartridgeAudioCommand.Value` → `int`; `SpeedBoosterEchoStopVerification` (about 43 and 65) uses an `int` local.
6. **Frame counters:**
   - `Gameplay/FrameClock.Counter`/`Counter8` and `Enemies/FrameCounters.Counter8` become an `int` with named
     wrapping advances.
   - Adapt `PauseGamePhases.AdvanceAnimations(byte)`, `MamaTurtleBehavior` (about 894),
     `DachoraBehavior.RunDachoraMain(byte)`, `EnemyProjectilePreInstructionContext.NmiFrameCounter8` and its readers
     (`BotwoonBehavior` about 881 and 976, `FallingSparksBehavior` about 55 and 148), `CeresSaveStartupVerification`
     (about 161), `RidleyFullMovieVerification` (about 849) and `CoreAccess` (about 67).
7. **Shared enemy leftovers:**
   - `EnemyRoomStatus.EnemiesKilled` → `int` with the named `RecordKill()` wrap. Adapt `PlmFramePhase` (about 120),
     `PlmFrameInputs.EnemyDeaths`, `FirefleaDoubleDeathVerification` (about 31 and 45) and
     `RidleyBreakupProgramsVerification` (about 113).
   - `IElevatorJourney.ElevatorFlags`/`ElevatorDirection` → a typed direction plus flags. Adapt `DoorGamePhases`
     (about 140–144) and `RoomLoader` (about 732–734).
   - `EnemyDeathsAndDrops.LastEnemyPickupSoundEffect` → `SoundEffectId?`; `EnemyDropsVerification` (about 182)
     compares a `SoundEffectId`.
   - `RoomEnemyPopulationRecord`: `Properties` becomes an `EnemyPropertyWord`, `ExtraProperties` an
     `EnemyExtraPropertyWord`, and the parameters become `int`. Adapt `RoomEnemySystem` (about 1033–1039),
     `CeresRidleyBehavior` (about 1337–1346) and the fixture constructors.
   - `RoomEnemySpawnSnapshot.VramTilesIndex` is write-only, so delete it. Remove the parameter, the `RoomEnemySystem`
     argument (about 1048) and the `MetareeBehavior` write (about 213).

### 5.2 AssetExtraction (194 rows; investigated, not started)

The previous worker's plan:

- **SME6274/6276:**
  - `CeresFlightArtworkExtractor` (about 36) and `TitleGraphicsExtractor` (about 39): compare whole
    `Extent16<ImagePixels>` values.
  - `EscapeTypewriterExtractor` (about 53), `FileSelectPresentationExtractor` (about 243) and
    `GameOverPresentationExtractor` (about 107): one enum per stream format, decoded with `Enum.IsDefined` and then
    switched on (the `MusicPortCommand` pattern).
  - `MapArrowExtractor` (about 24): an explicit `[Left, Right, Up, Down]` array.
  - `RoomPlmDynamicCollectibleArtFiles` (about 139): iterate `RoomPlmDynamicCollectibleGraphicsDefinitions.All`.
- **Non-audio SME6347:**
  - Delete `SmCompressionHeader.FirstByte/SecondByte`, which have no consumer.
  - `SamusBodyArtworkFiles.DefinitionEntry` sizes → `int` with validation, using `checked((ushort))` into Core.
  - `FileSelectPresentationExtractor.RawPatchCell.Word` → `SnesBgTilemapWord`.
  - `XrayRevealVisualFiles` document fields → validated `int`.
- **Audio SME6347** (the managed SPC player and S-DSP, about 172 rows):
  - Decide per field. A real hardware register or RAM width whose wrap the emulation depends on is kept through a
    named 8- or 16-bit wrapping operation, or lives inside the documented typed APU/DSP view. A bounded quantity,
    index, count or flag is widened (`int` with `Ensure.BetweenInclusive`, the `SoundEffectClipEntry.Command`
    pattern), or becomes a `bool`/enum.
  - `DspVoiceMask` is typed end to end, including `ISpcDriverObserver.KeysPublished`, `OneShot.Voices` and
    `LoopingLayer.Voice`.
  - The DSP envelope `Gain` floor relies on `ushort` underflow and needs its own focused test.
  - `musicPortToSnes` stays a `byte`, because `SpcCommandPortsVerification` (about 77) needs it; give it a
    site-specific allowance naming the APU output port.
  - `Priority`/`Mode` stay byte-domain `int`s, because the policy tests preserve arbitrary values.
  - `WriteState`/`WriteSequencerState` must write identical bytes, so loop detection is unchanged.
  - Rendered clips and manifests must stay byte-identical.

### 5.3 Per-family owned enemy state (owner question 4, not deferred)

Evir (commit `cb2dc15cc`) is the pattern. For each family, one batch:

- The family's `*EnemyState` stops being a view over `RoomEnemySlot.Variable*` words. It owns typed fields:
  `NativeCountdown`, `SnesSignedSixteenSixteen`/8.8 velocities, `SnesAngle`, spatial positions, bools and enums.
- Values the cartridge keeps in the common record are installed with
  `RoomEnemySlot.SetSpeciesState(state, clearWithRecord)`, so a kill (`Clear()`) reproduces the cleared record.
- Cross-slot reads become species-state queries (see `ResolveEvirCombatAfterCommon`).
- The family's slot words are translated only in `NativeSnapshotImportVerification`, with a per-family translation
  after the generic variable writes.
- Each family's remaining rows and destinations are listed in the species-leftovers commit (`4040e09e5`) and in its
  sibling commits. Examples:
  - Atomic/Crawler/Stoke/Ripper/Waver: 16.16 velocities.
  - Rio/LavaJumper: signed 8.8.
  - Shaktool: `SnesAngle`, with its dual-role word split by role.
  - Spark/Gunship/Kago: `NativeCountdown`.
  - Every `Last…SoundEffect`: `SoundEffectId?` through `QueueEnemySound`.
- Kraid's head cursor is already done (`f7cbab3ce`). Ridley's death-breakup fragments (`VariableB/C/F` in
  `RidleyBehavior` about 1490–1558) are candidates.

### 5.4 Larger representation changes

Do these only if 5.1–5.3 are exhausted; all are approved in `owner-questions.md`. Pick one and do it completely, in
ordered commits:

- Q5: enemy frame identity
- Q7: room identity
- Q2: Samus projectile slots
- Q8: the room sprite-object pool
- Q9: palette-FX colour keys
- Q10: the shared RNG
- Q6: the cinematic sprite word
- Q3: the speed-booster overrun bytes, moved into `SamusFullBodyCycleColorExtractor`

## 6. Stopping

At about 2.5 hours, finish or revert the batch in hand. Do not start a batch you cannot census and commit in time.
Leave the working tree clean, with everything committed and pushed. Then append to the progress log below:

- each commit with its rule counts
- anything you reverted or left unfinished, with the reason
- any new `owner-questions.md` entry
- the final census site count

Commit and push that update too.

## Progress log

- 2026-10-17, coordinator run: stopped at `c77b2c1c9` with 719 census sites (Core 536, AssetExtraction 183).
- 2026-10-17, single-agent run (started at `8d7d6c424`, baseline census 752 sites: Core 558, AssetExtraction 194):
  - `307bdf8de` Ridley state and Ceres status (5.1.1): Core SME6347 521 -> 511.
  - `2ad682c7e` Crocomire hitbox reactions (5.1.2): Core SME6347 511 -> 509.
  - `3341556f8` OAM entry, colour-add window, VRAM queue entry (5.1.3): Core SME6347 509 -> 504.
  - `d66047a73` palette-change step counter (5.1.4): Core SME6347 504 -> 503.
  - `6974ca45b` audio request, queue-limit and music-index storage (5.1.4 queue limit, 5.1.5): Core SME6347 503 -> 493.
  - `1f23a146e` frame counters and shared enemy state (5.1.6, 5.1.7 kill count, elevator, pickup sound, spawn VRAM index):
    Core SME6347 493 -> 475.
  - `0caf741c5` liquid surface, velocity and tide phase (5.1.4): Core SME6347 475 -> 468.
  - `2b7ae32bf` layer-2 scroll axes (5.1.4): Core SME6347 468 -> 466.
  - `bb39597e6` library-background BG3 character choice (5.1.4): Core SME6347 466 -> 465.
  - `497851680` room enemy population record (5.1.7): Core SME6347 465 -> 460.
  - `d79ee9ceb` AssetExtraction closed domains and non-audio storage (5.2): SME6274 7 -> 0, SME6276 2 -> 0,
    AssetExtraction SME6347 185 -> 172.
  - `5de6ef29b` door hit counters and eye-door reach: Core SME6347 460 -> 455.
  - `356388e1e` frontend fade brightness levels: Core SME6347 455 -> 451.
  - `1399f41dd` escape typewriter state and Maridia elevatube motion: Core SME6347 451 -> 444.
  - `752acfb8a` Samus knockback direction and hurt-flash counter: Core SME6347 444 -> 442.
  - `0708472ff` Samus flare mirror and animation frame buffer: Core SME6347 442 -> 440.
  - `2da9420f5` room-state theme and music selectors and the enemy death quota: Core SME6347 440 -> 435.
  - `a74d1456c` station activation index: Core SME6347 435 -> 434.
  - `e6bd20ae5` Ceres steam hitbox reactions: Core SME6347 434 -> 432.
  - `d4a138c6f` escape timer state and running flag: Core SME6347 432 -> 431.
  - `751ffa2b9` solid-enemy collision records and slope alignment result: Core SME6347 431 -> 426.
  - `30cd00191` ending-time override minutes: Core SME6347 426 -> 425.
  - `a5de855a0` Samus knockback direction enum: Core SME6347 425 -> 424.
  - Deviations from the queue's designs, decided against the code: a Ridley tail angle is an int, not SnesAngle (its
    low byte is the sine index; SnesAngle's table index is the high byte), and the whip targets are `int?`.
    `ceres_status` is one four-stage enum (`CeresEscapeStatus`), not an enum plus a rotation flag, because the door's
    $8000 write replaces the status rather than OR-ing a bit.
  - Left for later: beam firing's result tuple still carries a sound request word (changes with the Samus projectile
    slots, owner question 2); `CartridgeDoorHeader.SamusDistance` (a signed distance whose negative values select a
    default) touches every door constructor; the AssetExtraction audio rows (172) and the per-family enemy state
    migrations (5.3) were not started. Also left: `SamusState.BombJumpDirection` and `PoseTransitionShotDirection` pack a
    direction with provenance bits that native imports carry raw, and the power-bomb status mirror belongs with the
    Samus projectile slots; `SuperMetroidGame.FrameNumber` has about 60 consumers.
  - The census was run from a detached worktree at each local commit so editing could continue while it ran; the
    worktree needs one `dotnet restore` of the solution before its first census.
  - No new owner question.
  - Final census: 633 sites (Core 461: SME6347 424, SME6292 12, SME6341 10, SME6342 6, SME6346 5, SME6349 4;
    AssetExtraction 172, all SME6347), down from 752. No project other than Android fails to compile.
- 2026-10-17, single-agent run continued (from `98c61fa0c`, 633 sites):
  - `f3d410bc5` + `a78a2f56c` AssetExtraction audio unit: the managed SPC driver and S-DSP hold their registers as
    ints with every wrap through `SpcDriverWrap`; snapshots go through `SpcDriverSnapshot`; manifest and instrument
    records are validated ints. AssetExtraction SME6347 172 -> 0. `a78a2f56c` repairs a CS8629 build error the first
    commit introduced (the compile-error tool does not report warnings the build treats as errors; build the
    affected projects with `dotnet build -p:RunAnalyzers=false` before committing).
  - `653044bf4` Samus liquid physics: Core SME6347 424 -> 419.
  - `dfc52cc0b` Gunship/Kago countdowns: SME6349 4 -> 0.
  - `90015e05f` Bank80 progression mirror takes `SaveMirrorMemory`: SME6292 12 -> 10.
  - `3ce385084` room block BTS (`RoomBlockBehavior`, `RoomBlockBehaviorCodec`): SME6347 419 -> 413.
  - `762d5882c` enemies' last queued library-two sounds as `SoundEffectId?`: SME6347 413 -> 403.
  - `2010a2e1a` flip flags at their encoding owners: SME6346 5 -> 2.
  - `6ec7d4554` HUD tilemap words, COLDATA writes, timer palette, grapple transfer length: SME6347 403 -> 392.
  - Pre-existing failure, not from this run: `--samus-atmospheric-cadence` fails its null-pointer WRAM check at
    `8d7d6c424` too; the check predates #1318 stage 5 (type two draws nothing).
  - Left: SME6346 Beetom/Shaktool (family slot words, 5.3); SME6341 Fake Kraid / old Tourian shaft (address-keyed
    instruction mechanics); SME6342 overrun bytes (owner question 3; the HUD posture bytes matter only as zero or
    nonzero, so an import-pipeline table is the bounded route); SME6292 bus fields (SpaceTime overrun path);
    `ScreenFade` packed byte counters; `Position16`/`Extent16` X/Y (about 3500 Core uses); Samus speed pairs; room
    and frame identity (Q5, Q7); Samus projectiles (Q2); per-family enemy state (5.3).
  - Final census: 420 sites, all Core (SME6347 392, SME6292 10, SME6341 10, SME6342 6, SME6346 2), down from 633; AssetExtraction 0.
- 2026-10-17, coordinator resumed with three workers (from `6027af33f`, census 404 sites, Core only):
  - `0a6130a15` Samus batch (W2): census clean, Core SME6347 379 -> 356.
  - The commit after it holds two batches that were NOT fully censused, committed at the owner's direction
    at the weekly usage limit:
    - W3 (rooms, frontend, runtime, saves, map, fx, gameplay, spatial): reviewed and accepted, 46 rows.
      - Census b46 was otherwise clean (SME6347 379 -> 310 together with W2), with two exceptions.
      - The first exception, two SME6404 layering findings (Gameplay referencing Rendering's `VramWordRange`),
        is fixed in that commit by calling `SnesPpuLayout.RequireVramWord` instead.
      - The second exception is a compile error: `NativeSnapshotImportVerification.cs:295` calls W1's
        `NativeEnemyFamilyStateImport`.
    - W1 (enemy families to owned state, owner question 4): interrupted mid-run (last step: Metaree state).
      - It is partial and unreviewed.
      - It added `Verification/Enemies/NativeEnemyFamilyStateImport.cs` and `*OwnedStateVerification.cs` files,
        and edited several family `*EnemyState`/`*Behavior` files.
  - **First thing next session:**
    1. Run one census.
    2. Fix every compile error. W1's half-finished families are the likely source; finish each family or revert
       it to its pre-W1 state.
    3. Confirm no rule rose versus 356 Core SME6347 sites.
    4. Then continue section 5.
  - Owner question 13 added (Ceres Mode 7 source address read by the speed-booster residue).
  - Blockers reported by the workers (exact edits are in their reports, summarized here):
    - The largest blocker is `RidleyFullMovieVerification.Check(string, ushort, int)`, which pins most remaining
      Samus fields; it should take `int`.
    - Samus projectile slot consumers in enemy and boss files must take the typed slot API (owner question 2).
    - Remaining Q7 room-identity work needs both generators edited.
    - Q8 sprite objects and Q10 RNG need every consumer in one batch.
- 2026-10-17, single session resumed at `602d5546e`:
  - The compile break was the W1 owned-state checks missing `using SuperMetroid.Core;` for the fixture binder;
    every W1 family otherwise compiled.
  - The Mama Turtle check guessed the wall reversal of -1.0; the routine (unchanged by W1) wraps it to 0 through
    the carry-in, and the assertion now says so.
  - Census 261 sites, all Core (SME6347 235, SME6292 10, SME6341 10, SME6342 6); no (rule, file) pair rose
    against the 420-site census at `6027af33f`.
- 2026-10-17, same session continued (`f39cdb8ca` through the slot-word removal):
  - Samus and Power Bomb typing (`bfdb7a0e3`, `c909c6553`, `886b8726b`, `12dca1a5e`), enemy slot words held as
    word-ranged ints (`49008d85a`, `5b99ce202`, `4250a9010`, `7898c1305`) and the frame phases given operations
    instead of the bus (`d18e6c1e9`, SME6292 10 -> 6).
  - Per-family owned enemy state (5.3) for every family, from the four-family batch `0ab4dcde1` to Phantoon
    (`75bad801f`). Families whose record words carry several meanings in one actor (Dachora, Crocomire, Draygon,
    the gunship, Kraid and Phantoon) keep each record word once with a named view per meaning. Then
    `RoomEnemySlot.VariableA-F` are deleted and the native snapshot import passes `NativeEnemyRecordWords` to the
    family translation.
  - `wrap` follow-up: five `++` on range-checked views wrap at sixteen bits again (Phantoon speed phase, gunship
    counters).
  - Five failing checks repaired (production unchanged): the Ki-Hunter swoop and Fake Kraid walk fixtures had zero
    collision radii, the Owtch fixture lacked room RNG services, and the null atmospheric pointer and standalone
    Samus speed checks still asserted removed live-memory reads.
  - Census 201 sites, all Core (SME6347 179, SME6341 10, SME6342 6, SME6292 6); no (rule, file) pair rose.
  - Not done: the SME6341 rows in the Fake Kraid and Old Tourian projectile definitions need their address
    switches replaced by data-only mechanics tables read outside the catalog (Tooling and tests consume both), plus
    a ROM-oracle check; the SME6342 instruction-byte observations; the SME6292 frontend and runtime bus fields; and
    the remaining SME6347 storage rows (projectile slots, room state, Samus projectile slots).
- 2026-10-17, same session continued (`f8011980b` through `cdfb5d24b`):
  - Word-ranged int storage for the enemy projectile slot (`f8011980b`, SME6347 179 -> 168) and Samus projectile
    damage, frame and variable (`f587de1c7`, 168 -> 164).
  - The full no-argument suite found five failures; all are fixed: the layer-2 scroll default (`07bfd748a`, a
    regression from `2b7ae32bf`), four stale checks and a DebugRunner build break (`cb0333f6c`), and the Core WRAM
    cells #1319 forbids (`0e7de3454`, progression staged in an owned `SaveMirrorImage`). A second full run failed only
    the typed-projectile coverage list, which `fa64b2f2d` fixed.
  - `0e7de3454`'s message says the architecture and module inventories were regenerated; they were not committed.
    The architecture audit reports 46 type findings and the Core module cycle (see below), so its inventories are
    regenerated together with that work.
  - SME6341 10 -> 0 (`d8203133b`): Fake Kraid's projectiles and the old Tourian wall explosion are typed programs,
    with ROM-oracle checks.
  - SME6342 6 -> 0 (`f50384954`): the overread bytes come from the player's cartridge through the new
    `samus-overreads` installed family and `SamusOverreadCatalog`.
  - SME6292 6 -> 0 (`cdfb5d24b`): the VRAM queue drains from installed artwork only; the runtime, session and game
    take no bus. The desktop host's debugger save states still store an address space beside the game (outside
    Core); dropping it from those states is a follow-up.
  - Census 164 sites, all Core SME6347. They are address-valued identities: room, state, door, FX and population
    pointers (owner question 7), frame spritemap pointers (5), the sprite-object pool (8), hitbox callback and
    instruction pointers, and the RNG word (10).
  - Architecture audit: 46 flagged types (20 large, 14 back-reference, 12 context-bag) and the cycle among the eleven
    Simulation modules; `9a5a48ca4` opened them for the cleanup.
- 2026-10-18, same session continued (`74dad1d0e` through the typed hardware words):
  - `74dad1d0e`: at the owner's direction, AGENTS.md and manifesto section 8 no longer pause work on large changes.
  - Word-ranged ints: Samus's speed, gravity, animation-frame and counter words (`d58f05be3`, SME6347 164 -> 152)
    and the PLM loop timer, room-main scratch word, intro-sprite timer, door Samus distance, getaway zoom and PLM
    VRAM write size (`a7566f3b3`, 152 -> 145). Helpers that took those words take range-checked ints
    (`SnesSixteenSixteen.Compose`, `NativeWordCounter.Decrement`, `NativeCountdown.FromRaw`,
    `SnesSignedSixteenSixteen.FromParts`).
  - `562598b60`: the six SMV replays are `FullRunSuites` entries, so the no-argument run is the whole suite (about
    nine minutes here); `tools/verify-all.ps1` no longer launches them separately. Its first run failed lowc, 13%
    and 100%: regressions from this cleanup that no focused check covered.
  - `0c85cba85` fixed both, reproduced against the native captures:
    - Samus's counts are the signed word range; Crystal Flash and firing decrement ammunition unchecked
      (`SamusQuantity.DecrementWord`), since the cartridge drains and fires below zero. #1323 stage 4 had bounded
      them to [0, 32767].
    - The Zebetites' shared palette counter is slot zero's record word C whoever holds the slot
      (`ZebetiteBehavior.SharedPaletteCycle`); in room $E321 that is Mother Brain's body. `6089ea0f4` had tied it to
      a Zebetite in slot zero.
    - All six replays now pass in full.
  - Open: `VerifySamusBodyArtwork`'s installation repair fails with access denied on `Directory.Move` only under
    the 24-worker no-argument run (it passes alone). A handle into the freshly written content directory is still
    open; the fix (retry, a different publish step, or an antivirus exclusion for `csharp/test-temp`) is the owner's
    decision.
  - Typed hardware words for the message tilemap cell, the trail OBJ attributes and the Samus part X word, with
    numeric JSON converters (SME6347 145 -> 142).
  - Typed level words for the collision block and the PLM restore word (`4df9d8727`, 142 -> 139); the shared RNG as a
    typed state with int samples, owner question 10 (`2703356ea`, 139 -> 138); `VramDmaDestination` for the PLM VRAM
    write and Samus death tiles (`9b46e0e8a`, 138 -> 136); word-ranged enemy palette, tile base and Ceres arrival
    words (`94e091d03`, 136 -> 133).
  - Q7, room identity, in steps. Names come from the pinned disassembly's labels, through the symbolic fields of
    bank $8F's room states (`%enemyPop`, `%enemySet`, `%FX`, ...) and the room headers' labels. Each step moves a
    pointer's address-to-identity map into the import side (AssetExtraction or test support):
    - Step 1 (`d4c0fbf15`, 133 -> 131): room states' main and setup callbacks are typed identities.
    - Step 2 (`3d1033bdf`, 131 -> 123): room states hold their enemy population and graphics set as named definitions.
    - Step 3a (this commit, 123 -> 114): rooms are named `RoomHeaderDefinitions`; AssetExtraction's
      `RoomHeaderSourceDefinitions`/`RoomHeaderSources` own the header pointers. Hosts report rooms by name, and
      debugger save states are schema 7.
    - Step 3b (114 -> 105): doors are named `DoorDefinitions`; rooms reach their door lists through
      `RoomDoorListDefinitions` (generated from the disassembly's `RoomDoors_` lists); FX records and
      library-background transfers name their door. AssetExtraction's `DoorHeaderSources` owns the door pointers.
    - Step 3c (`843df62d3`, 105 -> 104): room states hold their scroll data as named `RoomScrollDefinitions`.
    - Step 3d (`e6071bb7a`, 104 -> 102): room states hold their PLM populations as named definitions, generated by
      `tools/generate-room-plm-population-definitions.ps1`.
    - Step 3e (`f07284f80`, 102 -> 100): room states hold their library background as a named
      `LibraryBackgroundProgram`; AssetExtraction's `LibraryBackgroundSources` owns the list pointers.
    - Step 3f (`be1d2a36e`, 100 -> 98): room states hold a named `RoomFxDefinition` (an FX header's records in native
      order, selecting the entry door's record itself); records have no pointer; AssetExtraction's `RoomFxSources`
      owns the header pointers and test support's `RoomFxDefinitionsAccess` derives record pointers from them.
    - Step 3g (`910bd329f`, 98 -> 97): room states hold a named `XrayRoomOverlayDefinition`; the installed overlay
      catalog is keyed by overlay; reveals.json keeps its pointer key, resolved by AssetExtraction's
      `XrayRoomOverlaySources`.
    - Step 3h (`5550c868a`, 97 -> 94): room states are named by the disassembly's `RoomState_` labels; selection
      returns states; the frontend, hosts, error reports and the debugger header (schema 8) name the state;
      AssetExtraction's `RoomStateSources` owns the pointers.
    - Q7 is complete.
  - Standalone rows and Q6 (94 -> 81):
    - `fcea6913e`: sprite-part Y is the typed `SnesSpritemapYByte` from compiled parts through the OAM buffer.
    - `838e9a085`: demo input objects are named `DemoInputObjectDefinitions`; graphics-set headers hold a palette row
      and staging block; eye-door and noob-tube request words are range-checked ints.
    - `fa8fb1a48`: Samus's horizontal speed table is the liquid medium plus a resolved row, not a table base address.
    - `d8a9488c8` (owner question 6): cinematic sprites hold a named `CinematicSpritePreInstruction`; test support's
      `CinematicSpritePreInstructionAccess` maps them to native addresses.
    - `ee522d4e2`: game-over text streams are named by element; AssetExtraction's `GameOverTextSources` owns the
      pointers.
    - `399561a9d`: library-background destinations are range-checked ints.
    - `2d4e7d60b` and `b7922adca` repaired the retail intro render test's music queues and the ending DMA check.
  - Q8 (`04cdb5919`, 81 -> 75): room sprite objects run typed `RoomSpriteObjectAnimations` programs (23 programs, 276
    frames, generated from the disassembly's `InstList_SpriteObject_` lists) instead of interpreting bank-$B4
    instruction words. A slot holds a program cursor, a `NativeCountdown`, the frame shown, typed OBJ attributes and an
    updates-disabled flag; kinds select programs by name. Test support's `RoomSpriteObjectProgramAddresses` and
    `RoomSpriteObjectSlotAccess` give the oracles native record addresses, and `--room-sprite-object-definitions` runs
    every spawned kind beside an independent reading of the cartridge's list.
  - Q9, palette FX:
    - Q9a (`c2b5eb0fc`, 75 -> 73): palette-FX objects are a value-free `PaletteFxObject` named by the disassembly's
      `PaletteFXObjects_` labels; slots, spawns, area bits and every owner name the object. Test support's
      `PaletteFxObjectAccess` owns the object-to-address map.
    - Q9b (`577cbbf72`, 73 -> 71): colours are keyed by `PaletteFxColorKey` (record, written-colour ordinal). Each
      colour family exposes `PaletteFxColorRows` (its first record, rows and colours; a probe confirmed every family
      is consecutive records with ordinal equal to colour), and the heat, loading, gunship, glare, Maridia, lightning,
      PLANET ZEBES, statue and title derivations work in rows and colours. The record table keeps layouts only;
      AssetExtraction's `PaletteFxRecordSources` maps record addresses to frames for extraction, and test support's
      `PaletteFxColorLayoutAccess`/`PaletteFxColorAddressAccess`/`PaletteFxProgramLayoutAccess` keep the ROM oracles
      comparing words.
  - Boss frames (`c186f04a2`, 71 -> 61): Draygon, Kraid, Mother Brain body/head/tube, Baby Metroid, Zebetite, Phantoon
    and Crocomire frame records hold identity only; their spritemaps are looked up at the presentation boundary
    (`*FrameDefinitions`/`*VisualDefinitions`, `PhantoonPartFrameSpritemaps`, `CrocomireBodyFrameSpritemaps`), and
    Crocomire's body frames are a computed list instead of a pointer enumerator.
  - Boss frames is an interim step only: owner question 5 records that converting a frame's address property to a
    method was rejected as evasion, and the shared `RoomEnemySlot.SpritemapPointer` still stores the address. Q5
    proper replaces it.
  - Q5.1 (61 -> 60): `RoomEnemySlot` holds `EnemyFrameId? Frame` instead of the spritemap word. `EnemyFrameId`
    names every installed spritemap and extended frame after its composition (plus the shared `Empty` and
    `EmptyExtended`), and `EnemyFrames.Spritemap` binds identities to installed words by name. Every typed program
    runner takes a generated `Frame(TFrame)` table (81 families, Ridley's body and Crocomire's tongue included);
    named writers install identities. Test support's `RoomEnemySlotFrameAccess` keeps the native word view, mapping
    (bank, word) back to a frame.
  - Q5.1b: the generated tables were switch expressions inside catalog files, which the manifesto forbids. Each is
    now `EnemyFrameTable<TFrame>` data (`ShownFrames`, keyed by the frame value; the lookup that fails loudly lives
    in the table type), Botwoon's head is a table, Tripper's frozen-frame choice moved to `PlatformsBehavior`, and
    Mother Brain's unread head table was deleted.
  - Q5.2a: `EnemySpritemapDraw` names its `EnemyFrameId` instead of a bank and word. `EnemyPresentation` draws
    nothing for `Empty` (the audited renderer no-op) and resolves other frames' installed art through
    `EnemyFrames.Bank`/`Spritemap`; the slot draw path keeps the supported-bank check for the empty frame. Mother
    Brain's brain returns its `MotherBrainHeadFrame` and draws through `HeadFrames`; Ridley's wings, tail tip and
    links, the Ceres Baby, the Ceres door overlay and the Dead Torizo hook draw named frames. ROM oracles take test
    support's `NativeEnemySpritemapDraw`, and native fixtures reach the renderer through
    `EnemyPresentationNativeDrawAccess`.
  - Q5.2b: the Ceres elevator arrival projectile holds its `CeresElevatorArrivalPose?` instead of a bank-$8D word
    and $8000 sentinel; test support keeps the word view.
  - Q5.2c: collision data names its frames. Crocomire's body, the Space Pirates, Ceres steam and Oum key their frame
    records by `EnemyFrameId` in `EnemyFrameCollision<TElement>` tables (`Contains`/`TryGet`/`Of` take the slot's
    nullable frame); steam components hold their hitbox list directly and the steam and Oum lists are plain arrays.
    `CollisionRecordRun`/`CollisionRecordRuns` (run starts and record strides) moved to test support, where the
    `*CollisionDefinitionsAccess` views answer the ROM oracles by native address.
  - Projectile pre-instructions: the enemy-projectile slot and `EnemyProjectileDefinition` hold
    `EnemyProjectilePreInstruction` instead of a bank-$86 word; the catalog names each kind's pre-instruction, and the
    Tourian statue, n00b-tube and Pirate-laser identities joined the enum. The enum still carries its routine
    addresses because the Pirates' A050 and the shared Y-operand install decode list words at the native
    interpreter; the values go when those lists are typed. Test support's `EnemyProjectilePreInstructionAccess` owns
    the word view (`Word`, `FromWord`, slot `PreInstructionWord`).
  - Projectile cue handlers take an `EnemyProjectileCueContext` (projectile, Samus, the pass's camera).
  - The Pirate/Mother Brain laser and the Ninja Pirate claw play `SpacePirateProjectileAnimation` programs; the laser's
    A050 is the `StartMovingNow` cue, which runs the movement once through the cue context's camera. The Core catalog
    keeps only the list entries, its Tooling partner owns the mechanics layout, and the engine no longer reads Pirate
    mechanics words. Native lists still run for the Tourian statue projectiles and Mother Brain's top tubes; when
    those are typed, the engine's native interpreter, the A050/8161 decodes and the pre-instruction values can go.
  - Mother Brain's four ceiling tubes start `MotherBrainTopTubeAnimation` programs (one pose, then hold) through
    `EnemyProjectileTypedStarts`; the instruction layout moved to the Tooling partner.
  - The Tourian statue projectiles (splash, eye glow, particle, tail, soul, base decoration, Ridley and Phantoon statues
    and the private delete list) start `TourianStatueProjectileAnimation` programs through `EnemyProjectileTypedStarts`;
    their family commands are `TourianStatueProjectileCue`s handled by the unlock behaviour, and the native instruction
    interceptor is gone. The particle's counted loop never runs out (its counter wraps from zero and the particle
    leaves the screen first), so its program ends in an unreachable hold where the native list runs into routine
    code; the oracle's `EndsAt` rule names that address. No enemy projectile kind starts a native list now.
  - The bank-$86 interpreter is gone: an active projectile without a typed program fails loudly, the slot has no list
    pointer, and the native instruction handlers, interceptors and mechanics-word reads were deleted. Definition
    initialization starts the instruction timer at one, as `SpawnEprojInner` does. Snapshot import restores the shown
    frame through test support's `EnemyProjectileNativeImportAccess`. Remaining projectile findings: the presentation
    keys and spritemap words (`EnemyProjectileFramePresentation`, the slot's `SpritemapPointer` and
    `PresentationOperandAddress`, `CompiledEnemyVisualSelector`), the drop-chance override, and the
    pre-instruction enum's routine addresses, which the Tooling layouts still emit.
  - Ridley's afterburn chain keeps the next afterburn's `RoomEnemyProjectileKind` instead of its definition word.
  - `EnemyProjectilePreInstruction` is a value-free identity. Its bank-$86 routine addresses live in Tooling's
    `EnemyProjectilePreInstructionWords`, which the Tooling layouts and test support's word view read; oracle cue
    rules compare operands through it.
  - Enemy projectile frames are identities: `RoomEnemyProjectileSlot.Frame` holds an `EnemyProjectileFrameId?`
    (named from the installed program frames, plus the blank map and the Skree/Metaree compositions), and the
    engine draws through `EnemyProjectileFrames`, which binds identities to installed keys by name. Each family's
    key function became a generated `ShownProjectileFrames` table over its full frame domain (record frames take
    the product of their enum fields and phases, aliases included). The presentation registry is generation data
    (`EnemyProjectilePresentationFrameDefinitions` additions, `EnemyProjectilePresentationFrameSets` selection).
    Test support's `EnemyProjectileSlotFrameAccess` gives the native key/spritemap views.
  - The frame lists and counts only the old registry read left Core: unused ones deleted, test-only ones in
    test-support `<Owner>FramesAccess` extension classes, tool-read ones in Tooling `<Owner>FramesTooling`.
  - `CompiledEnemyVisualSelectors` (the 4011-entry operand-to-spritemap map) lives in Tooling: no Core code read
    it; tests, the resource audit and the generator (which now writes to Tooling) do.
  - Enemy collision callbacks: the touch/shot/bomb contexts and `IEnemyPrivateShotCallback` carry only
    `EnemyInteractionCallback?`; Crocomire's private address enum is gone; the dead Tourian corpse profiles name
    their rot-trigger callbacks. Extended-hitbox owners answer with typed callbacks (species codecs map kinds to
    members); the collision walker holds no callback words. The enum still carries address values and Core still
    decodes header words through `EnemyAiCodePointers`; neither is an open census finding.
  - A boss-selected drop keeps its `EnemyDropChances` table on the projectile slot instead of the bank-$B4 word.
  - PLM campaign, finished. Block reactions (`0c42a4c84`) and the scripted room PLMs (`19fdff900`: elevator
    platform, Shaktool, Maridia elevatube, old Tourian wall, escape animals, Crateria passage, Mother Brain escape
    gate and its closing, breakable terrain, Chozo plug/hand/slopes) run typed, oracle-checked programs. `380801b5e`
    deleted the generic bank-$84 interpreter, `RoomPlmProgramDefinitions`, `PlmSlot.InstructionPointer`,
    `PlmProgramStart.NativeList`, the scroll/treadmill marker writes and the static PLM program audit (with its CI
    workflow and release-gate flag); a slot that runs neither a typed program nor a family step throws.
    `a87b8ab92` took pre-instruction routine addresses out of Core (test support's
    `PlmPreInstructionNativeRoutines` maps instances to routines). `5226c7ef1` and `460f01a00` widened room
    arguments, compiled scroll sources and the visual records. `ea724b79a` pruned the compiled-word catalogs the
    interpreter left dead into test-support.
  - Samus projectiles (`045a6e8f1`): slot type/direction/program/spritemap state, the trail pointer and the timed
    record are int quantities; `SamusProjectileTypeWord`/`SamusProjectileDirectionWord` are `[SnesEncodingOwner]`
    views. Storing the typed words in the slots would mean replacing about 80 raw bit-field reads with named
    operations, a separate representation change.
  - **SME census: zero findings** from `045a6e8f1` on.
  - Reachability gate: about 465 findings before this work, 549 after the interpreter removal, then down to under 30
    through `ea724b79a` (PLM prune), `e977f2334` (tool-only code to Tooling, dead Core code pruned outside
    `Hardware/`), `2eaa52ff9` (the audit counts records hashed through repository generics such as
    `EnemyFrameTable<TFrame>` as read; it had reported 53 frame-record fields), `7b6261df9`, `ca2e7a859` (test
    support owns `NativeEnemyProjectileInstruction` and `SnesWindowSelectionPairBits`; the Tooling enum keeps
    what tools emit) and the small moves after it. The prune takes comma-separated `--path-prefix` values. Never
    prune `SuperMetroid.Core/Hardware/`: it deletes the CPU-bus interfaces and the move step cannot repair the
    hierarchies (3291 compile errors). `25924e7c0` and `338c78937` removed the test-only definition fields (corpse
    pointers, the Ceres header pointer, the projectile initial list), two convenience constructors,
    `EnemyFrameCollision.Count` and `CpuBusCell.Require`, and pruned the constants they fed; the library-background
    byte count left Core, its generator and the generated catalog together. What remains (6 findings) is the CPU
    bus write path: `ISnesMemoryBus`, `SnesMemoryBusRequirement`, `SnesWordReads`, `ISnesAddressSpace.WriteByte`,
    `SuperMetroidAddressSpace.WriteByte` and test support's `SyntheticCartridge.WriteByte`. Production keeps
    `SuperMetroidAddressSpace` as the WRAM image for the desktop host and debugger save states, but only tests and
    the import pipeline write through it. About 1700 test call sites pass it as an `ISnesAddressSpace`, so moving
    the write path out of Core means a test-support bus that wraps it, adopted at those sites.
  - Q5.2 remains: about 30 Core readers still resolve a frame to its word for the family collision catalogs
    (`ComponentsAt`/`HasFrame`/`IsFrame`/`TryGetComponents` in 15 catalogs plus Kraid's two), the common empty
    checks, the extended-hitbox test and `EnemySpritemapDraw`. Those catalogs are re-keyed by `EnemyFrameId` one at
    a time (their frame domains are enum roots, fixed strides and switches), then the words move to extraction.
  - Remaining work, assessed (61 sites):
    - Q5, enemy frame identity: `RoomEnemySlot.SpritemapPointer` is written by every family (typed programs through
      `EnemyProgramAnimation.Run(..., Func<TFrame, ushort>)`, named constants such as the common empty frame,
      `(ushort)32847` in `EnemyPopulationLoader`, Crocomire's address-valued tongue enum, Phantoon's collision-only
      selectors) and read by drawing (`EnemySpritemapDraw`, the extended-frame catalog and BG2 frames), collision
      (empty-frame checks, extended hitboxes and family `CollisionRecordRuns` keyed by address) and family logic.
      Design, following Q8: the slot holds an address-free frame identity (an ordinal into the installed frame schema,
      `EnemySpritemapDefinitions.Frames`, plus the extended-frame set); presentation and collision resolve it at their
      boundary catalogs; native snapshot import and the oracles map words to frames in test support. The draw request,
      `CompiledEnemyVisualSelector`, `EnemyProjectileFramePresentation`, the Ceres steam and Maridia snail hitbox
      keys and `CollisionRecordRun.Start` follow from it.
    - Enemy interaction AI words (`Enemy*Context` raw callback words, Dead Tourian functions): every hitbox callback
      word must resolve to a named `EnemyInteractionCallback`, whose values are themselves addresses.
    - Interpreters still running native lists by address: about 68 enemy-projectile kinds (Draygon, Crocomire,
      Ceres Ridley, Phantoon, Kraid, Fake Kraid, pirates, Torizos, Tourian statues, Mother Brain, Spore Spawn,
      Botwoon and others) through `EnemyProjectileEngine`, and the PLMs not yet on `PlmAnimation`. These carry the
      projectile-slot, PLM and `Instruction*` word sites.
    - Q2, Samus projectiles: typed type/direction state, then programs and the trail, per the owner's order.
    - Q13, the Ceres Mode 7 transfer residue.
  - Next: Q5 with the enemy AI pointers, Q2, PLM words, the enemy projectile slot and Q13. The palette-FX setup and
    pre-instruction enums still carry bank-$8D values.
