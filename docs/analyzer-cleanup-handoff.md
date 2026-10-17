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
  - the Discord update drain (another machine owns it)
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
