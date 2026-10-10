using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Input;
using static SuperMetroid.Core.Hardware.SnesAddressMath;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Room-enemy loader, scheduler, instruction interpreter, and draw queues. Retail
/// gameplay definitions are compiled; installed artwork supplies pixels and colors.
/// </summary>
/// <remarks>
/// This deliberately retains the cartridge's 32 fixed slots and its native slot offsets
/// ($0000, $0040, ... $07C0). Those offsets are data: enemy AI uses them for multi-part
/// actors, collision lists publish them, and the gunship bottom initializer even relies on
/// WRAM-array aliasing to copy fields from earlier slots. A compact host list would appear
/// cleaner while destroying behavior that the original code expects.
/// </remarks>
public sealed partial class RoomEnemySystem
{
    /// <summary>Thirty-two fixed physical enemy records in the native bank-$A0 scheduler, including unused and multipart slots.</summary>
    public const int MaximumEnemyCount = 32;
    /// <summary>Native enemy-record stride in bytes ($0040), used to translate host slot numbers into cartridge indexes $0000..$07C0.</summary>
    public const int NativeSlotSize = 0x40;
    /// <summary>Maximum four enemy graphics-set entries accepted by the native room tileset arrays.</summary>
    public const int MaximumGraphicsSetCount = 4;

    private readonly RoomEnemySlot[] _slots = new RoomEnemySlot[MaximumEnemyCount];
    private readonly List<ushort>[] _drawQueues =
        Enumerable.Range(0, 8).Select(_ => new List<ushort>()).ToArray();
    private readonly List<ushort> _activeEnemyIndexes = new();
    private readonly List<ushort> _interactiveEnemyIndexes = new();
    private readonly List<SolidEnemyCollisionBody> _interactiveCollisionBodies = new();
    private readonly List<RoomEnemyGraphicsSetEntry> _graphicsSet = new();
    private ISnesAddressSpace? _bus;
    private Func<ushort>? _nextRandom;
    private Func<ushort>? _readRandomNumber;
    private Func<bool>? _isAreaBossDefeated;
    private Action? _setAreaBossDefeated;
    private Func<bool>? _isAreaMiniBossDefeated;
    private Func<EventNumber, bool>? _hasEvent;
    private Action<EventNumber>? _setEvent;
    private Action<EventNumber>? _clearEvent;
    private Action? _setAreaMiniBossDefeated;
    private Action<ushort>? _setRandomNumber;
    private ushort _randomEnemyCounter;

    /// <summary>
    /// The 16-bit NMI_FrameCounter ($05B6) for the current enemy pass. It is independent of
    /// the 8-bit $05B5 counter; standalone audits seed both from the same enemy clock.
    /// </summary>
    private ushort _enemyFrameNmiFrameCounter;
    private SnesVram? _vram;
    private SnesCgram? _cgram;

    /// <summary>
    /// The shared <c>PaletteChangeNumerator</c>. Enemies and the runtime's door and death
    /// fades advance this one counter, as the cartridge's fades all advance one WRAM word.
    /// </summary>
    public GradualColorChangeCounter GradualColorChange { get; private set; } = new();
    // Live dependency supplied by EnemyMain, shared by ordinary and custom touch callbacks.
    private SamusProjectileSystem? _samusProjectilesForEnemyFrame;
    private SporeSpawnEnemyState? _sporeSpawn;
    private bool _processAllEnemies;
    private GunshipLoadScenario _gunshipLoadScenario;
    private SamusState? _samusAtEnemyInitialization;

    /// <summary>Current host-owned timer artwork; rebound after debugger-state restoration.</summary>
    [field: NonSerialized]
    public EscapeTimerTileAtlas? EscapeTimerArtwork { get; set; }

    /// <summary>Host-bound Ceres Ridley fade and retreat colors; rebound after state restore.</summary>
    [field: NonSerialized]
    public CeresRidleyColorCatalog? CeresRidleyColors { get; set; }

    /// <summary>Host-bound Ceres Mode-7 zoom colors; rebound after state restore.</summary>
    [field: NonSerialized]
    public CeresRidleyMode7ColorCatalog? CeresRidleyMode7Colors { get; set; }

    /// <summary>Current host-owned escape-warning text, rebound after debugger restoration.</summary>
    [field: NonSerialized]
    public EscapeTypewriterPresentation? EscapeTypewriterPresentation { get; set
        {
            field = value;
            EscapeTypewriterState? active = MotherBrain?.EscapeTypewriter;
            if (active?.ProgramId == EscapeTypewriterProgramId.Zebes)
            {
                if (value is null) active.UnbindProgram();
                else active.BindProgram(value.Get(EscapeTypewriterProgramId.Zebes));
            }
            EscapeTypewriterState? ceres = Ridley?.CeresEscapeTypewriter;
            if (ceres?.ProgramId == EscapeTypewriterProgramId.Ceres)
            {
                if (value is null) ceres.UnbindProgram();
                else ceres.BindProgram(value.Get(EscapeTypewriterProgramId.Ceres));
            }
        } }

    /// <summary>Creates all 32 stable physical slot objects without loading a room or binding memory, graphics, random-number, or gameplay services.</summary>
    public RoomEnemySystem()
    {
        for (int slotIndex = 0; slotIndex < _slots.Length; slotIndex++)
            _slots[slotIndex] = new RoomEnemySlot(slotIndex);
    }

    /// <summary>All 32 physical enemy slots, including unused zero-pointer slots.</summary>
    public IReadOnlyList<RoomEnemySlot> Slots => _slots;

    /// <summary>Collision words read from the current slots after their AI has run.</summary>
    public IReadOnlyList<SolidEnemyCollisionBody> InteractiveCollisionBodies =>
        _interactiveCollisionBodies;

    /// <summary>
    /// Installed enemy presentation. Drawing or a named artwork transfer requires its
    /// corresponding catalog; null does not enable a cartridge-backed fallback.
    /// </summary>
    [field: NonSerialized]
    public EnemyTileArtworkCatalog? TileArtwork { get; set; }

    /// <summary>Installed standard BG3 characters shared with Kraid's death restoration.</summary>
    [field: NonSerialized]
    public HudTileAtlas? HudTileArtwork { get; set; }

    /// <summary>Bank-$A1 population identity supplied by the current room state, matching native EnemyPopulationPointer ($07CF).</summary>
    public ushort PopulationPointer { get; private set; }
    /// <summary>Bank-$B4 enemy graphics-set identity supplied by the current room state, matching native EnemySetPointer ($07D1); an empty population does not load that set.</summary>
    public ushort TilesetPointer { get; private set; }
    /// <summary>Native byte offset marking the allocation frontier, not a zero-based slot number; nonempty population load sets it to record count times $40 and an empty load retains its previous value.</summary>
    public ushort FirstFreeEnemyIndex { get; private set; }
    /// <summary>Number of population records installed at room load, including multipart records; zero for an empty population, not a live count of surviving actors.</summary>
    public ushort EnemyCount { get; private set; }
    /// <summary>Wrapping native room kill counter, reset at load and incremented by death paths; counts their publications rather than enumerating remaining or uniquely defeated slots.</summary>
    public ushort EnemiesKilled { get; private set; }
    /// <summary>Required room kill count from the nonempty population's terminator for enemy-clear triggers; native empty-population initialization retains the previous quota.</summary>
    public byte DeathQuota { get; private set; }
    /// <summary>Native BossID ($179C) for the loaded encounter, reset to zero on room load and published by nonzero definition IDs or boss initialization; not an area boss-defeated flag.</summary>
    public ushort BossId { get; private set; }
    /// <summary>Whether an address-space dependency has been bound by <see cref="Load"/>; this reports binding, not successful completion of every initializer or artwork validation.</summary>
    public bool IsLoaded => _bus is not null;
    /// <summary>Most recent gunship lifecycle event in the current enemy pass, cleared to None at the next pass; answering the save prompt also publishes an event immediately.</summary>
    public GunshipFrameEvent LastGunshipEvent { get; private set; }
    /// <summary>Whether gunship restoration has requested message box $1C and is awaiting <see cref="AnswerGunshipSavePrompt"/> before its exit animation can resume.</summary>
    public bool GunshipSavePromptPending { get; private set; }
    /// <summary>Persistent Yes/No result of the latest gunship save answer, reset on room load; true requests outer-runtime SRAM persistence rather than performing a save itself.</summary>
    public bool GunshipSaveRequested { get; private set; }
    /// <summary>Reserved legacy Mochtroid sound publication, cleared at room load and each enemy pass; current Mochtroid processing does not assign a sound to this field.</summary>
    public ushort? LastMochtroidSoundEffect { get; private set; }
    /// <summary>Last library-two sound operand emitted by a Hopper instruction this enemy pass, admitted through the native Max6 queue entry; null means no request.</summary>
    public ushort? LastHopperSoundEffect { get; private set; }
    /// <summary>Last library-two $70 Yard shell kick, launch, or wall-bounce sound this enemy pass, using the Max3 queue entry; cleared before the next pass.</summary>
    public ushort? LastYardSoundEffect { get; private set; }
    /// <summary>Last library-two Metaree dive ($5B) or floor-impact ($5C) sound this enemy pass, using the Max6 queue entry; cleared before the next pass.</summary>
    public ushort? LastMetareeSoundEffect { get; private set; }
    /// <summary>Last library-two Skree dive ($5B) or burrow-impact ($5C) sound this enemy pass, also queued by its AI through Max6; cleared before the next pass.</summary>
    public ushort? LastSkreeSoundEffect { get; private set; }
    /// <summary>Last library-two Alcoon emerge or volley sound this enemy pass, using the Max6 queue entry; null means no current-pass request.</summary>
    public ushort? LastAlcoonSoundEffect { get; private set; }
    /// <summary>Last library-two Kzan landing sound this enemy pass, using the Max6 queue entry; cleared at the next enemy-frame boundary.</summary>
    public ushort? LastKzanSoundEffect { get; private set; }
    /// <summary>Last library-two Hibashi eruption sound this enemy pass, using the Max6 queue entry; cleared at the next enemy-frame boundary.</summary>
    public ushort? LastHibashiSoundEffect { get; private set; }

    /// <summary>
    /// Zero-based index of the bank-$A6 fire-geyser shape command executed this frame.
    /// This mirrors an instruction-dispatch event rather than inventing persistent enemy
    /// state; <see langword="null"/> means no shape command ran during the current frame.
    /// </summary>
    public int? LastHibashiActivityFrameIndex { get; private set; }

    /// <summary>Last library-three sound requested by an attached Beetom this frame.</summary>
    public ushort? LastBeetomSoundEffect { get; private set; }
    /// <summary>Native $177E darkness progression, reset on room load and advanced by two per Fireflea death; the cartridge permits values through 12 and rejects the next step to 14.</summary>
    public ushort FirefleaDarknessLevel { get; private set; }
    /// <summary>Shared wrapping room-shake lifetime word, written by actor/effect producers and decremented by <see cref="HandleRoomShaking"/> for supported types while time is not frozen, not by <see cref="StepFrame"/>.</summary>
    public ushort EarthquakeTimer { get; set; }
    /// <summary>Native room-shake type selecting background/projectile offsets and whether actors shake; types $00..$23 are consumed by <see cref="HandleRoomShaking"/>, while $24 and above are ignored there.</summary>
    public ushort EarthquakeType { get; set; }

    /// <summary>
    /// Ridley's bank-$A6 state extension while either encounter owns slot zero. The native actor
    /// extends far beyond the common $40-byte enemy record, so exposing a deliberately named
    /// object is both more accurate and considerably easier to inspect than aliasing dozens
    /// of unrelated generic slot words.
    /// </summary>
    public RidleyEnemyState? Ridley { get; private set; }

    /// <summary>
    /// Compatibility view used by the existing Ceres debugger. It deliberately becomes
    /// null for Lower Norfair Ridley so callers cannot accidentally apply Baby/Mode-7
    /// encounter assumptions to the real boss fight.
    /// </summary>
    public RidleyEnemyState? CeresRidley =>
        _slots[0].EnemyDefinitionPointer == EnemyDefinitionId.RidleyCeres ? Ridley : null;

    /// <summary>
    /// Native <c>ceres_status</c> word consumed by the Ceres door actor. Fresh station load
    /// begins at zero; Ridley's escape sequence is the later producer of values one/two.
    /// </summary>
    public ushort CeresStatus { get; set; }

    /// <summary>
    /// One-frame publication of $A6:C117. The runtime consumes this after EnemyMain to
    /// start the global escape timer and set Ceres's boss bit in their native owners.
    /// </summary>
    public bool CeresEscapeStartedThisFrame { get; private set; }

    /// <summary>
    /// One-frame publication of a Draygon-owned <c>ReleaseSamusFromDraygon</c> ($90:E2DE)
    /// during EnemyMain. The release writes $FFFF to all three prospective-pose slots, which
    /// the runtime owns and clears after EnemyMain.
    /// </summary>
    public bool SamusReleasedByDraygonThisFrame { get; private set; }

    // A door load initializes enemies while the door IRQ scrolls; init AIs that read
    // layer 1 then wait for the loader's camera (CompleteLoaderTimeCameraReads).
    // TimeIsFrozenFlag as the enemy frame saw it; the draw hooks that follow read it.
    private bool _enemyFrameTimeIsFrozen;

    private bool _deferLoaderTimeCameraReads;

    /// <summary>Language flag sampled by $A6:C0D9 when the warning-text phase begins and by Mother Brain's alternate escape-text selection.</summary>
    public bool JapaneseText { get; set; }

    /// <summary>
    /// Ports the data-producing parts of <c>LoadEnemies</c>,
    /// <c>ProcessEnemyTilesets</c>, and <c>InitializeEnemies</c> at $A0:8A1E-$8C6C.
    /// </summary>
    public void Load(
        ISnesAddressSpace bus,
        ushort populationPointer,
        ushort tilesetPointer,
        SnesVram vram,
        SnesCgram cgram,
        Func<ushort> nextRandom,
        Action<ushort>? setRandomNumber = null,
        Func<ushort>? readRandomNumber = null,
        RoomLevelData? level = null,
        SamusState? samus = null,
        ushort controllerInput = 0,
        Func<bool>? isAreaBossDefeated = null,
        ushort cameraX = 0,
        ushort cameraY = 0,
        Func<EventNumber, bool>? hasEvent = null,
        Action<EventNumber>? setEvent = null,
        Action<EventNumber>? clearEvent = null,
        Func<bool>? isAreaMiniBossDefeated = null,
        Action? setAreaMiniBossDefeated = null,
        Func<bool>? isAreaTorizoDefeated = null,
        Action? setAreaTorizoDefeated = null,
        Func<PlmHeaderId, bool>? isRoomPlmPresent = null,
        Action<bool>? setSamusControlsEnabled = null,
        Action<int, RoomScrollState>? setRoomScrollState = null,
        Action? setAreaBossDefeated = null,
        Action? incrementMotherBrainGlassRoomArgument = null,
        Func<int, RoomScrollState>? readRoomScrollState = null,
        Action<LayerBlendingConfiguration>? setMotherBrainLayerBlendingDefaultConfig = null,
        Action<ushort, ushort>? setMotherBrainBg2Scroll = null,
        GunshipLoadScenario gunshipLoadScenario = GunshipLoadScenario.Ordinary,
        bool deferLoaderTimeCameraReads = false)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(vram);
        ArgumentNullException.ThrowIfNull(cgram);
        ArgumentNullException.ThrowIfNull(nextRandom);

        _bus = bus;
        _gunshipLoadScenario = gunshipLoadScenario;
        _deferLoaderTimeCameraReads = deferLoaderTimeCameraReads;
        _samusAtEnemyInitialization = samus;
        _samusProjectilesForEnemyFrame = null;
        _nextRandom = nextRandom;
        _samusForEnemyDrops = samus;
        // Some enemy routines call GenerateRandomNumber while others, including Alcoon's
        // post-volley bytecode, merely sample the existing WRAM seed. Keep those operations
        // distinct: substituting nextRandom here would silently advance the cartridge RNG.
        _readRandomNumber = readRandomNumber;
        _setRandomNumber = setRandomNumber;
        _isAreaBossDefeated = isAreaBossDefeated;
        _setAreaBossDefeated = setAreaBossDefeated;
        _incrementMotherBrainGlassRoomArgument = incrementMotherBrainGlassRoomArgument;
        _readMotherBrainRoomScrollState = readRoomScrollState;
        _setMotherBrainRoomScrollState = setRoomScrollState;
        _setMotherBrainLayerBlendingDefaultConfig = setMotherBrainLayerBlendingDefaultConfig;
        _setMotherBrainBg2Scroll = setMotherBrainBg2Scroll;
        _isAreaMiniBossDefeated = isAreaMiniBossDefeated;
        _hasEvent = hasEvent;
        _setEvent = setEvent;
        _clearEvent = clearEvent;
        _setAreaMiniBossDefeated = setAreaMiniBossDefeated;
        _isAreaTorizoDefeated = isAreaTorizoDefeated;
        _setAreaTorizoDefeated = setAreaTorizoDefeated;
        _isRoomPlmPresent = isRoomPlmPresent;
        _vram = vram;
        _cgram = cgram;
        PopulationPointer = populationPointer;
        TilesetPointer = tilesetPointer;
        EnemyCount = 0;
        EnemiesKilled = 0;
        BossId = 0;
        LastGunshipEvent = GunshipFrameEvent.None;
        BeginEnemySoundRequestFrame();
        BeginShitroidFrame();
        GunshipSavePromptPending = false;
        GunshipSaveRequested = false;
        LastBoyonSoundEffect = null;
        LastMamaTurtleSoundEffect = null;
        LastMochtroidSoundEffect = null;
        LastMetroidSoundEffectLibrary2 = null;
        LastMetroidSoundEffectLibrary3 = null;
        LastCeresDoorSoundEffectLibrary2 = null;
        CeresEscapeStartedThisFrame = false;
        SamusReleasedByDraygonThisFrame = false;
        LastBoulderSoundEffect = null;
        LastZebetiteSoundEffect = null;
        LastEtecoonSoundEffect = null;
        LastDachoraSoundEffect = null;
        LastEvirSoundEffect = null;
        LastMorphBallEyeSoundEffect = null;
        LastYappingMawSoundEffect = null;
        PaletteChangeNumber = 0;
        CameraDistanceIndex = CameraDistanceMode.NormalTracking;
        _metroidDropRequests.Clear();
        LastHopperSoundEffect = null;
        LastYardSoundEffect = null;
        LastMetareeSoundEffect = null;
        LastAlcoonSoundEffect = null;
        LastFuneNamiheSoundEffect = null;
        LastKagoBugSoundEffect = null;
        LastKagoBugDropRequest = null;
        ResetMagdolliteRoomState();
        ResetBlueBrinstarFaceBlockRoomState();
        ResetKiHunterRoomState();
        ResetPipeBugRoomState();
        ResetBotwoonRoomState();
        ResetBombTorizoRoomState();
        ResetTourianEntranceStatueRoomState();
        ResetShaktoolRoomState();
        ResetChozoStatueRoomState(setSamusControlsEnabled, setRoomScrollState);
        ResetEscapeAnimalRoomState();
        ResetCrocomireRoomState();
        ResetSporeSpawnRoomState();
        ResetRinkaRoomState(cameraX, cameraY);
        ResetRioRoomState();
        ResetNorfairLavaJumpingEnemyRoomState();
        ResetNorfairRioRoomState();
        ResetLowerNorfairRioRoomState();
        ResetMaridiaLargeSnailRoomState();
        ResetRipperVariantRoomState();
        ResetDragonRoomState();
        ResetKraidRoomState();
        ResetPhantoonRoomState();
        ResetDraygonRoomState();
        ResetMotherBrainRoomState();
        ResetDeadTorizoRoomState();
        ResetDeadSidehopperRoomState();
        ResetDeadTourianCorpseRoomState();
        ResetShitroidRoomState();
        ResetShutterRoomState(cameraX, cameraY);
        ResetElevatorRoomActors();
        EnemyDoorTransitionActive = false;
        LastKzanSoundEffect = null;
        LastHibashiSoundEffect = null;
        LastHibashiActivityFrameIndex = null;
        LastSkreeSoundEffect = null;
        LastNuclearWaffleSoundEffect = null;
        LastFakeKraidSoundEffect = null;
        LastFakeKraidDropRequest = null;
        LastKraidSoundEffect = null;
        LastEnemyProjectileDudSoundEffect = null;
        LastBeetomSoundEffect = null;
        LastWorkRobotSoundEffect = null;
        LastBotwoonSoundEffect = null;
        LastBotwoonDropRequest = null;
        FirefleaDarknessLevel = 0;
        EarthquakeTimer = 0;
        EarthquakeType = 0;
        LastRoomShake = default;
        Ridley = null;
        RidleyDeathDropRequested = false;
        _processAllEnemies = false;
        Array.Clear(_boyonStates);
        Array.Clear(_stokeStates);
        Array.Clear(_mamaTurtleStates);
        Array.Clear(_babyTurtleStates);
        Array.Clear(_puyoStates);
        Array.Clear(_cacatacStates);
        Array.Clear(_owtchStates);
        Array.Clear(_multiviolaStates);
        Array.Clear(_polypStates);
        Array.Clear(_funeNamiheStates);
        Array.Clear(_kagoStates);
        Array.Clear(_crawlerStates);
        Array.Clear(_skreeStates);
        Array.Clear(_flyStates);
        Array.Clear(_sbugStates);
        Array.Clear(_mochtroidStates);
        Array.Clear(_metroidStates);
        Array.Clear(_boulderStates);
        Array.Clear(_zebetiteStates);
        Array.Clear(_etecoonStates);
        Array.Clear(_dachoraStates);
        Array.Clear(_evirStates);
        Array.Clear(_morphBallEyeStates);
        Array.Clear(_wreckedShipGhostStates);
        Array.Clear(_yappingMawStates);
        MorphBallEyeBeam.Reset();
        Array.Clear(_hopperStates);
        Array.Clear(_zoaStates);
        Array.Clear(_yardStates);
        Array.Clear(_waverStates);
        Array.Clear(_metareeStates);
        Array.Clear(_firefleaStates);
        Array.Clear(_skulteraStates);
        Array.Clear(_skulteraRadii);
        Array.Clear(_skulteraTurnFinishedFlags);
        Array.Clear(_skulteraAngleDeltas);
        Array.Clear(_skulteraPreviousYOffsets);
        Array.Clear(_skulteraCurrentYOffsets);
        Array.Clear(_chootStates);
        Array.Clear(_chootSpawnXPositions);
        Array.Clear(_chootSpawnYPositions);
        Array.Clear(_chootInitialFallingXPositions);
        Array.Clear(_chootInitialFallingYPositions);
        Array.Clear(_chootFallingXOrigins);
        Array.Clear(_chootFallingYOrigins);
        Array.Clear(_chootInitialYSpeedTableIndexes);
        Array.Clear(_chootJumpDelayTimers);
        Array.Clear(_platformStates);
        Array.Clear(_platformYMovementFunctions);
        Array.Clear(_platformPreviousPositions);
        Array.Clear(_platformXMovementFunctions);
        Array.Clear(_platformVerticallyMovingFlags);
        Array.Clear(_platformVerticallyStillFlags);
        Array.Clear(_platformMaximumYSpeedTableIndexes);
        Array.Clear(_platformPreviousYMovementFunctions);
        Array.Clear(_platformSuspensorPlatformFlags);
        Array.Clear(_alcoonStates);
        Array.Clear(_alcoonYAccelerations);
        Array.Clear(_alcoonYSubaccelerations);
        Array.Clear(_alcoonSpawnXPositions);
        Array.Clear(_alcoonLandingYPositions);
        Array.Clear(_alcoonStepCounters);
        Array.Clear(_beetomStates);
        Array.Clear(_beetomInstalledInstructionLists);
        Array.Clear(_beetomInitialShortLeapYSpeedIndexes);
        Array.Clear(_beetomInitialLongLeapYSpeedIndexes);
        Array.Clear(_beetomInitialLungeYSpeedIndexes);
        Array.Clear(_beetomFallingFlags);
        Array.Clear(_beetomAttachedToSamusFlags);
        Array.Clear(_beetomDirections);
        Array.Clear(_beetomInitialAttachmentXOffsets);
        Array.Clear(_beetomInitialAttachmentYOffsets);
        Array.Clear(_powampStates);
        Array.Clear(_workRobotStates);
        _workRobotPaletteAnimationTimer = 0;
        _workRobotPaletteAnimationTableOffset = 0;
        _workRobotPaletteAnimationPaletteIndex = 0;
        Array.Clear(_bullStates);
        Array.Clear(_bullMaxSpeeds);
        Array.Clear(_bullAnglesToSamus);
        Array.Clear(_bullAngles);
        Array.Clear(_bullShotReactionDisableFlags);
        Array.Clear(_bullAccelerationTimerResets);
        Array.Clear(_bullDecelerationTimerResets);
        Array.Clear(_bullShotReactionDisableTimers);
        Array.Clear(_bullPreviousHealth);
        Array.Clear(_atomicStates);
        Array.Clear(_atomicSpeedFractions);
        Array.Clear(_atomicSpeedWholes);
        Array.Clear(_atomicNegativeSpeedFractions);
        Array.Clear(_atomicNegativeSpeedWholes);
        Array.Clear(_sparkStates);
        Array.Clear(_kzanStates);
        Array.Clear(_hibashiStates);
        Array.Clear(_nuclearWaffleStates);
        Array.Clear(_fakeKraidStates);
        Array.Clear(_walkingSpacePirateStates);
        Array.Clear(_wallSpacePirateStates);
        Array.Clear(_ninjaSpacePirateStates);
        Array.Clear(_kzanFallWaitTimerResetValues);
        Array.Clear(_kzanPreviousYPositions);
        Array.Clear(_kzanFallingYSpeedTableIndexes);
        Array.Clear(_kzanRiseWaitTimers);
        _standaloneEnemyProjectileFrameCounter8 = 0;
        foreach (RoomSpriteObjectSlot spriteObject in _roomSpriteObjects)
            spriteObject.Clear();
        // Enemy projectiles live in a separate native bank-$86 pool, but room loading
        // destroys them just as decisively as it clears bank-$A0 enemy slots. Without
        // this reset, leaving Ridley's room could carry a fireball (and its stale room
        // collision coordinates) into the destination room.
        foreach (RoomEnemyProjectileSlot projectile in _enemyProjectiles)
            projectile.Clear();
        _activeEnemyIndexes.Clear();
        _interactiveEnemyIndexes.Clear();
        _interactiveCollisionBodies.Clear();
        _graphicsSet.Clear();
        foreach (List<ushort> queue in _drawQueues)
            queue.Clear();
        foreach (RoomEnemySlot slot in _slots)
            slot.Clear();

        // $A0:8A6D does not even inspect the room's enemy set when the first population
        // word is the terminator. This is observable: an empty room must not overwrite
        // CGRAM or VRAM merely because its state happens to retain a non-empty set pointer.
        RoomEnemyPopulationDefinition population =
            ResolveRoomEnemyPopulation(bus, populationPointer);
        if (!population.Records.IsEmpty)
            LoadGraphicsSet(bus, tilesetPointer, vram, cgram);
        LoadPopulation(
            bus,
            population,
            level,
            samus,
            controllerInput,
            cameraX);
    }

    /// <summary>
    /// Appends enemy tile DMAs after the standard OBJ transfer queued by room setup.
    /// </summary>
    /// <remarks>
    /// Standard sprite graphics occupy VRAM bytes $C000-$EDFF and therefore overlap the
    /// beginning of the enemy region at $E000. ProcessEnemyTilesets stages enemy art first,
    /// but LoadEnemyTileData schedules its final VRAM copies after standard room graphics.
    /// A direct host load alone would be overwritten by the next accepted NMI.
    /// </remarks>
    /// <param name="queue">Ordered NMI VRAM-write queue to which selected enemy character transfers are appended.</param>
    public void QueueGraphicsUploads(VramWriteQueue queue)
    {
        ArgumentNullException.ThrowIfNull(queue);
        EnsureLoaded();
        foreach (RoomEnemyGraphicsSetEntry entry in _graphicsSet)
        {
            int destinationByteOffset = RoomEnemyRomLayout.VramByteBase + entry.StagingOffset;
            if ((destinationByteOffset & 1) != 0 || entry.TileByteCount > ushort.MaxValue)
                throw new InvalidDataException("Enemy tile DMA is not word-aligned or exceeds one native transfer.");
            queue.Enqueue(
                unchecked((ushort)entry.TileByteCount),
                entry.Definition.TileDataAddress,
                unchecked((ushort)(destinationByteOffset / 2)));
        }
    }

    /// <summary>
    /// Runs the bank-$88 morph-ball eye beam HDMA object, then
    /// <c>Determine_Which_Enemies_to_Process</c> ($A0:8EB6).
    /// </summary>
    /// <remarks>
    /// Game state eight calls $A0:8EB6 first ($82:8B47), before Samus's handlers. The grapple
    /// endpoint scan therefore sees this frame's interactive list, including an actor that
    /// `$86:EF10` respawned at the end of the previous frame. The HDMA objects run earlier
    /// still, in the main-loop prologue: that order is observable on spawn (one-frame pending
    /// initialization) and shutdown (the full beam sees the body's cleared activation word on
    /// the following frame).
    /// </remarks>
    /// <param name="cameraX">Horizontal viewport origin in whole room pixels for native active and interactive selection.</param>
    /// <param name="cameraY">Vertical viewport origin in whole room pixels for native active and interactive selection.</param>
    public void PrepareEnemyProcessingList(ushort cameraX, ushort cameraY)
    {
        EnsureLoaded();
        StepMorphBallEyeBeam();
        DetermineWhichEnemiesToProcess(cameraX, cameraY);
    }

    /// <summary>
    /// Runs one native enemy pass: executes selected AI and instruction lists, publishes
    /// live collision bodies and frame requests, and builds queues for the later draw phase.
    /// </summary>
    /// <param name="cameraX">Horizontal viewport origin in whole room pixels.</param>
    /// <param name="cameraY">Vertical viewport origin in whole room pixels.</param>
    /// <param name="timeIsFrozen">Whether time-freeze gating suppresses actor AI and instructions; this does not suppress every timer, palette hook, or frame publication.</param>
    /// <param name="samus">Active player state required by player-dependent enemy paths.</param>
    /// <param name="newlyPressedControllerInput">Raw SNES button-edge mask for this update, distinct from held buttons.</param>
    /// <param name="level">Room terrain and collision data required by terrain-dependent actors.</param>
    /// <param name="controllerInput">Raw held SNES controller-button mask for this update.</param>
    /// <param name="samusProjectiles">Player shot system used by enemy collision and custom touch paths.</param>
    /// <param name="nmiFrameCounter8">Current native eight-bit NMI clock; null uses the low byte of the standalone enemy-pass clock.</param>
    /// <param name="mode7Transform">Active Mode-7 transform sampled by rotating Ceres steam to derive draw offsets without changing its world collision coordinates.</param>
    /// <param name="sharedProjectiles">Shared bomb and power-bomb state used by native collision order and actor effects.</param>
    /// <param name="vramWriteQueue">Optional destination for corpse-row graphics transfers produced during this pass.</param>
    /// <param name="resolveSamusContactBeforeAi">Whether to perform the configured projectile, bomb, and player-contact pass against each actor's pre-AI state.</param>
    /// <param name="collisionPlms">Optional room PLMs participating in the scoped enemy terrain-collision probes.</param>
    /// <param name="nmiFrameCounter">Current independent sixteen-bit NMI clock; null uses the standalone enemy-pass clock.</param>
    /// <param name="processingListPrepared">True when <see cref="PrepareEnemyProcessingList"/> already ran at the native pre-Samus boundary; false prepares the lists here for standalone callers.</param>
    /// <param name="samusPreviousPositionCheckpoint">The camera's frame-start Samus checkpoint under the live previous-position words that Draygon and Yapping Maw cap; null when no camera owns them.</param>
    public void StepFrame(
        ushort cameraX,
        ushort cameraY,
        bool timeIsFrozen,
        SamusState? samus = null,
        ushort newlyPressedControllerInput = 0,
        RoomLevelData? level = null,
        ushort controllerInput = 0,
        SamusProjectileSystem? samusProjectiles = null,
        byte? nmiFrameCounter8 = null,
        SamusMode7Transform? mode7Transform = null,
        SamusBombProjectileSystem? sharedProjectiles = null,
        VramWriteQueue? vramWriteQueue = null,
        bool resolveSamusContactBeforeAi = false,
        RoomPlmSystem? collisionPlms = null,
        ushort? nmiFrameCounter = null,
        bool processingListPrepared = false,
        SamusCameraPoint? samusPreviousPositionCheckpoint = null)
    {
        using var terrainScope = new EnemyTerrainScope(this, collisionPlms);
        EnsureLoaded();
        _samusForEnemyDrops = samus;
        _samusPreviousPositionCheckpoint = samusPreviousPositionCheckpoint;
        _enemyFrameTimeIsFrozen = timeIsFrozen;
        _samusProjectilesForEnemyFrame = samusProjectiles;
        _audioPowerBomb = sharedProjectiles?.PowerBombExplosion;
        // Standalone audits do not own the runtime NMI clock. In that case the enemy-frame
        // counter begins at zero and advances at the same end-of-frame point, which gives
        // Mama Turtle's even-frame shell jitter the same initial phase as retail room load.
        byte enemyNmiFrameCounter8 = nmiFrameCounter8 ?? unchecked((byte)_randomEnemyCounter);
        _enemyFrameNmiFrameCounter = nmiFrameCounter ?? _randomEnemyCounter;
        LastGunshipEvent = GunshipFrameEvent.None;
        BeginEnemySoundRequestFrame();
        LastBoyonSoundEffect = null;
        LastMamaTurtleSoundEffect = null;
        LastCacatacSoundEffect = null;
        LastMochtroidSoundEffect = null;
        LastMetroidSoundEffectLibrary2 = null;
        LastMetroidSoundEffectLibrary3 = null;
        LastCeresDoorSoundEffectLibrary2 = null;
        CeresEscapeStartedThisFrame = false;
        SamusReleasedByDraygonThisFrame = false;
        LastBoulderSoundEffect = null;
        LastZebetiteSoundEffect = null;
        LastEtecoonSoundEffect = null;
        LastDachoraSoundEffect = null;
        LastEvirSoundEffect = null;
        LastMorphBallEyeSoundEffect = null;
        LastYappingMawSoundEffect = null;
        LastHopperSoundEffect = null;
        LastYardSoundEffect = null;
        LastMetareeSoundEffect = null;
        LastSkreeSoundEffect = null;
        LastAlcoonSoundEffect = null;
        LastFuneNamiheSoundEffect = null;
        LastKagoBugSoundEffect = null;
        LastKagoBugDropRequest = null;
        ResetMagdolliteFrameEvents();
        LastKzanSoundEffect = null;
        LastHibashiSoundEffect = null;
        LastHibashiActivityFrameIndex = null;
        LastNuclearWaffleSoundEffect = null;
        LastFakeKraidSoundEffect = null;
        LastFakeKraidDropRequest = null;
        LastKraidSoundEffect = null;
        _kraidPlmRequests.Clear();
        LastSpacePirateSoundEffect = null;
        LastEnemyProjectileDudSoundEffect = null;
        if (Ridley is not null)
        {
            // These are one-shot publications made by Ridley's current AI call, matching
            // the global QueueMusic/QueueSfx calls in bank $A6. Clear them at the same
            // enemy-frame boundary as every other Last* request above.
            Ridley.LastDeathSoundEffect = null;
            Ridley.MusicRequest = null;
        }
        LastBeetomSoundEffect = null;
        LastWorkRobotSoundEffect = null;
        LastBotwoonSoundEffect = null;
        LastBotwoonDropRequest = null;
        LastBotwoonWallPlm = null;
        LastBotwoonMusicRequest = null;
        LastBombTorizoSoundEffect = null;
        LastBombTorizoMusicRequest = null;
        LastCrocomireSoundEffect = null;
        LastCrocomireMusicRequest = null;
        LastCrocomireDropRequest = null;
        _crocomirePlmRequests.Clear();
        LastDeadTorizoSoundEffect = null;
        _deadTorizoFrameVramTransfers.Clear();
        BeginDeadSidehopperFrame();
        BeginShitroidFrame();
        MotherBrain?.BeginFrame();
        BeginSporeSpawnFrame();
        LastRioSoundEffect = null;
        LastNorfairLavaJumpingEnemySoundEffect = null;
        LastNorfairRioSoundEffect = null;
        LastLowerNorfairRioSoundEffect = null;
        LastMaridiaLargeSnailSoundEffect = null;
        LastDragonSoundEffect = null;
        BeginShutterFrame(cameraX, cameraY);
        BeginElevatorFrame();
        SetRinkaCamera(cameraX, cameraY);
        if (!processingListPrepared)
            PrepareEnemyProcessingList(cameraX, cameraY);
        foreach (List<ushort> queue in _drawQueues)
            queue.Clear();

        foreach (ushort nativeIndex in _activeEnemyIndexes)
        {
            RoomEnemySlot slot = SlotFromNativeIndex(nativeIndex);
            // EnemyMain $A0:9021-$902E decrements tangible actors' invincibility
            // before checking time freeze. X-ray therefore stops their movement but
            // not this clock. Remember the entry value: the call that reaches zero
            // still skips collision, and a new AI-owned timer must not lose a tick.
            bool invincibleAtEntry = slot.InvincibilityTimer != 0;
            if (!slot.Properties.HasAny(EnemyProperties.IgnoreSamusCollision) && invincibleAtEntry)
                slot.InvincibilityTimer = unchecked((ushort)(slot.InvincibilityTimer - 1));
            // EnemyMain checks contact against the pre-movement actor, not the position
            // just moved into by AI. A rising support temporarily overlaps its rider until
            // bank $90 consumes the carry; a late touch pass would retrigger that support.
            if (!timeIsFrozen && !invincibleAtEntry && resolveSamusContactBeforeAi && samus is not null)
            {
                // Native extended collision runs projectiles before bombs/touch and
                // before selecting hurt/main AI. In particular, a retained Plasma
                // hit must publish Phantoon's hurt clock before this frame ticks it.
                if (slot.EnemyDefinitionPointer == EnemyDefinitionId.PhantoonBody &&
                    samusProjectiles is not null && sharedProjectiles is not null)
                    ResolvePhantoonProjectileHits(_bus!, samusProjectiles, sharedProjectiles);
                if (samusProjectiles is not null && sharedProjectiles is not null)
                    ResolveOrdinaryProjectileHits(_bus!, samusProjectiles, sharedProjectiles,
                        samus, nativeIndex);
                // Alpha already updated the bomb slots. Native EnemyMain checks
                // them before this actor's touch and AI, so escape starts now.
                if (sharedProjectiles is not null && samusProjectiles is not null)
                    ResolveOrdinaryBombHits(sharedProjectiles, samusProjectiles, samus, nativeIndex);
                if (slot.EnemyDefinitionPointer == 0)
                    continue;
                ResolveOrdinarySamusContact(samus, controllerInput, level, nativeIndex);
                if (IsRidleyDefinition(slot.EnemyDefinitionPointer))
                    ResolveRidleyBodySamusContact(samus);
                if (slot.EnemyDefinitionPointer == 0)
                    continue;
            }
            if (!timeIsFrozen)
            {
                bool ranActorAi = RunCommonGrappleAi(
                    slot,
                    samus,
                    newlyPressedControllerInput,
                    controllerInput,
                    level,
                    cameraX,
                    cameraY,
                    enemyNmiFrameCounter8);
                if (!ranActorAi &&
                    (slot.AiHandlerBits & 0x0002) != 0 &&
                    slot.EnemyDefinitionPointer == EnemyDefinitionId.Metroid)
                {
                    // The native dispatcher selects the lowest set AI bit. Metroid's
                    // custom hurt entry therefore owns the actor before frozen bit four
                    // when both are present, while instruction bytecode still advances.
                    ApplyMetroidHurt(slot);
                    ranActorAi = true;
                }
                if (!ranActorAi &&
                    (slot.AiHandlerBits & 0x0002) != 0 &&
                    slot.EnemyDefinitionPointer is EnemyDefinitionId.BombTorizo or EnemyDefinitionId.GoldenTorizo)
                {
                    // Torizo_Hurt owns the actor for the selected hurt frame. The common
                    // instruction interpreter still advances afterward, matching $A0:8FF7.
                    ApplyBombTorizoHurt(slot);
                    ranActorAi = true;
                }
                if (!ranActorAi &&
                    (slot.AiHandlerBits & 0x0002) != 0 &&
                    slot.EnemyDefinitionPointer == EnemyDefinitionId.Ridley)
                {
                    // $A6:B297 owns hurt frames instead of falling back to ordinary main
                    // AI. Movement/function work runs only on even actor frames, while tail
                    // projectile armor and hurt palettes remain live on every frame.
                    RunNorfairRidleyHurt(
                        slot,
                        samus,
                        controllerInput,
                        level,
                        samusProjectiles,
                        sharedProjectiles ?? throw new InvalidOperationException(
                            "Ridley's power-bomb check ($A6:BD2C) requires the shared projectile owner."),
                        cameraX, cameraY);
                    ranActorAi = true;
                }
                if (!ranActorAi &&
                    (slot.AiHandlerBits & 0x0002) != 0 &&
                    slot.EnemyDefinitionPointer == EnemyDefinitionId.PhantoonBody)
                {
                    // $A7:DD3F owns every Phantoon hurt frame. It alternates palette seven
                    // between white and the new health band; body movement/main AI resumes
                    // only after common flash timing clears handler bit two.
                    ApplyPhantoonHurt(slot, RequireCompletePhantoonState(slot));
                    ranActorAi = true;
                }
                if (!ranActorAi &&
                    (slot.AiHandlerBits & 0x0002) != 0 &&
                    slot.EnemyDefinitionPointer == EnemyDefinitionId.DraygonBody)
                {
                    // `$A5:954D` owns the entire actor during hurt dispatch. Both the BG2
                    // body palette and sprite-palette eye/tail pieces flash together, while
                    // grapple electrocution may subtract another 256 HP every eighth frame.
                    ApplyDraygonHurt(slot, RequireCompleteDraygonState(slot), samus);
                    ranActorAi = true;
                }
                if (!ranActorAi &&
                    (slot.AiHandlerBits & 0x0002) != 0 &&
                    slot.Definition.HurtAiPointer == EnemyAiCodePointers.BankA0.NoOp)
                {
                    // Native selects the lowest AI bit even when its handler is just
                    // RTL. Hurt therefore delays frozen AI until flash housekeeping
                    // clears bit two; falling through here prematurely clears flash.
                    ranActorAi = true;
                }
                // $A0:9044 selects the handler from the lowest set AI bit alone. A frozen
                // clock without bit four (a Geruta flame sharing its parent's freeze)
                // still runs main AI.
                if (!ranActorAi && (slot.AiHandlerBits & 0x0004) != 0)
                {
                    if (slot.EnemyDefinitionPointer == EnemyDefinitionId.Rinka &&
                        RunRinkaFrozenTail(slot))
                    {
                        // Rinka's termination tail clears/replaces the current actor record.
                        // The active-index array was frozen before AI, so skip the remainder
                        // of this stale entry exactly as the private routine's death call does.
                        continue;
                    }

                    // Test the clock before decrementing: reaching zero retains frozen
                    // dispatch for this call. Ice removal instead takes the thaw branch
                    // immediately. That branch stores the remaining AI bits into BOTH
                    // words; replacing the clock with literal zero loses retail behavior
                    // when hurt and frozen dispatch coexist.
                    slot.FlashTimer = 0;
                    bool thaw = slot.FrozenTimer == 0 ||
                        (samus is not null &&
                            (samus.EquippedBeams & (ushort)SamusBeamFlags.Ice) == 0);
                    if (!thaw)
                        slot.FrozenTimer = unchecked((ushort)(slot.FrozenTimer - 1));
                    else
                    {
                        slot.AiHandlerBits = unchecked((ushort)(slot.AiHandlerBits & ~0x0004));
                        slot.FrozenTimer = slot.AiHandlerBits;
                    }
                    if (slot.EnemyDefinitionPointer == EnemyDefinitionId.Metroid)
                        RunMetroidFrozen(slot);
                    if (slot.EnemyDefinitionPointer == EnemyDefinitionId.YappingMaw)
                        RunYappingMawFrozen(slot, RequireYappingMawState(slot));
                    // The native frame clock advances after every selected AI call,
                    // including frozen AI. Instruction gating below remains separate.
                    ranActorAi = true;
                }
                else if (!ranActorAi)
                {
                    RunMainAi(
                        slot,
                        samus,
                        newlyPressedControllerInput,
                        controllerInput,
                        level,
                        cameraX,
                        cameraY,
                        samusProjectiles,
                        enemyNmiFrameCounter8,
                        mode7Transform,
                        sharedProjectiles,
                        vramWriteQueue);
                    ranActorAi = true;
                }

                if (ranActorAi)
                {
                    slot.FrameCounter = unchecked((ushort)(slot.FrameCounter + 1));
                    // `$A0:8FF7` skips the instruction interpreter whenever frozen bit
                    // four is present, even if lower-priority hurt bit two selected an
                    // enemy-specific hurt callback above. Testing only `ranActorAi` here
                    // incorrectly advanced a Metroid's body animation while its frozen
                    // callback still owned the two composited outer sprite objects.
                    if ((slot.AiHandlerBits & 0x0004) == 0 &&
                        slot.Properties.HasAny(EnemyProperties.ProcessInstructions))
                        ProcessInstructions(
                            slot,
                            samus,
                            level,
                            cameraX,
                            cameraY,
                            controllerInput);
                }
            }

            // EnemyMain queues ordinary-sprite actors only after AI and instruction work.
            // Properties $0100/$0200 suppress drawing, while extra-property bit $0004 can
            // force an otherwise off-screen actor into the queue. Gunship uses the normal
            // radius-aware visibility test.
            bool visible = slot.ExtraProperties.HasAny(EnemyExtraProperties.UsesExtendedSpritemap) ||
                !EnemyWithNormalSpritesIsOffScreen(slot, cameraX, cameraY);
            if (visible && !slot.Properties.HasAny(EnemyProperties.Invisible | EnemyProperties.Deleted))
                _drawQueues[slot.Layer & 7].Add(nativeIndex);

            // $A0:9128 performs this after the enemy has been admitted to its draw queue.
            // Keeping it here also makes the boss's latched draw palettes describe the
            // frame just processed rather than the already-decremented following frame.
            if (!timeIsFrozen && slot.FlashTimer != 0)
            {
                slot.FlashTimer = unchecked((ushort)(slot.FlashTimer - 1));
                // `$A0:9128` clears hurt dispatch once the post-decrement timer is below
                // eight. This is a signed comparison and applies even when another AI bit
                // remains set; omitting it would strand Metroid in custom hurt forever.
                if (unchecked((short)(slot.FlashTimer - 8)) < 0)
                    slot.AiHandlerBits = unchecked((ushort)(slot.AiHandlerBits & ~0x0002));
            }
        }

        // DetermineWhichEnemiesToProcess freezes only the index list. Later bank-$94
        // collision routines dereference the live slot words, so publish bodies after AI
        // has had its chance to move multi-part enemies.
        _interactiveCollisionBodies.Clear();
        foreach (ushort nativeIndex in _interactiveEnemyIndexes)
        {
            RoomEnemySlot slot = SlotFromNativeIndex(nativeIndex);
            _interactiveCollisionBodies.Add(new SolidEnemyCollisionBody(
                nativeIndex,
                slot.XPosition,
                slot.YPosition,
                slot.XRadius,
                slot.YRadius,
                slot.FrozenTimer,
                slot.Properties));
        }
        _randomEnemyCounter = unchecked((ushort)(_randomEnemyCounter + 1));
        QueueDeadTorizoFrameVramTransfers(vramWriteQueue);
        QueueDeadSidehopperFrameVramTransfers(vramWriteQueue);
        samus?.Kinematics.ClearSolidEnemyCollisionIndexes();
        StepWorkRobotPaletteAnimation();
        StepMagdollitePaletteAnimation();
        StepBlueBrinstarFaceBlockPaletteAnimation();
        if (!timeIsFrozen)
            StepRoomSpriteObjects();
        CollectLegacyEnemyAudioRequests();
    }

    /// <summary>
    /// Supplies the choice normally returned by message box $1C after restoration. SRAM
    /// persistence remains an outer-runtime seam; the actor publishes a Yes choice while
    /// continuing the cartridge's identical exit animation for either answer.
    /// </summary>
    /// <param name="save">True to request outer-runtime persistence, or false to resume the exit animation without requesting a save.</param>
    /// <exception cref="InvalidOperationException">The system is not loaded or no gunship save prompt is awaiting a response.</exception>
    public void AnswerGunshipSavePrompt(bool save)
    {
        EnsureLoaded();
        if (!GunshipSavePromptPending)
            throw new InvalidOperationException("The gunship save prompt is not awaiting a response.");

        RoomEnemySlot top = _slots[0];
        RoomEnemySlot pad = _slots[2];
        GunshipSavePromptPending = false;
        GunshipSaveRequested = save;
        top.VariableF = (ushort)GunshipFunction.WaitForExitPadToOpen;
        pad.InstructionTimer = 1;
        pad.CurrentInstruction = GunshipInstructionProgramDefinitions.EntrancePadOpening;
        top.VariableA = 144;
        LastGunshipEvent = GunshipFrameEvent.SavePromptAnswered;
    }

    /// <summary>
    /// Writes queued enemies for an inclusive layer range using native slot order.
    /// </summary>
    /// <remarks>Drawing consumes each emitted actor's shake timer and may apply installed extended-frame BG2 presentation changes; it is a native draw phase, not a side-effect-free snapshot render.</remarks>
    /// <param name="oam">OAM buffer receiving the selected actors' ordinary, extended, and supplemental sprites.</param>
    /// <param name="cameraX">Horizontal viewport origin in whole room pixels, subtracted from world draw positions.</param>
    /// <param name="cameraY">Vertical viewport origin in whole room pixels, subtracted from world draw positions.</param>
    /// <param name="firstLayer">First native draw layer, inclusively, in the range 0..7.</param>
    /// <param name="lastLayer">Last native draw layer, inclusively, in the range 0..7 and not below firstLayer.</param>
    /// <exception cref="ArgumentNullException">The OAM buffer is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The inclusive layer range is invalid.</exception>
    public void DrawLayers(
        OamBuffer oam,
        ushort cameraX,
        ushort cameraY,
        int firstLayer,
        int lastLayer)
    {
        ArgumentNullException.ThrowIfNull(oam);
        EnsureLoaded();
        if ((uint)firstLayer > 7 || (uint)lastLayer > 7 || firstLayer > lastLayer)
            throw new ArgumentOutOfRangeException(nameof(firstLayer));

        foreach (int layer in Enumerable.Range(firstLayer, lastLayer - firstLayer + 1))
        {
            foreach (ushort nativeIndex in _drawQueues[layer])
            {
                RoomEnemySlot slot = SlotFromNativeIndex(nativeIndex);
                // Power-bomb damage runs after queue construction and can clear a
                // queued actor or replace it with the inert respawn reservation.
                // Neither lifecycle state owns a display composition.
                if (slot.EnemyDefinitionPointer is 0 or EnemyDefinitionId.Respawn)
                    continue;

                // Enemy spawn-point offsets are zero for ordinary room-population entries.
                // WriteEnemyOams nevertheless performs the additions before subtracting
                // layer-1 position, so preserve modular 16-bit arithmetic here.
                ushort originX = unchecked((ushort)(slot.SpawnXOffset + slot.XPosition - cameraX));
                ushort originY = unchecked((ushort)(slot.SpawnYOffset + slot.YPosition - cameraY));

                // `WriteEnemyOAM` `$A0:947B-$9494` consumes one shake tick while forming
                // the shared origin for both ordinary and extended spritemaps. Bit one of
                // the enemy's frame counter chooses -1 or +1; this is deliberately not the
                // room's BG displacement. Ceres quake types >=$12 reload ShakeTimer for
                // every active enemy, which is why its ordinary door sprites visibly move
                // with the station instead of remaining pinned to host screen space.
                if (slot.ShakeTimer != 0)
                {
                    originX = unchecked((ushort)(originX +
                        ((slot.FrameCounter & 2) != 0 ? -1 : 1)));
                    slot.ShakeTimer--;
                }
                ushort drawPaletteIndex = SelectCommonEnemyDrawPalette(slot);
                if (IsRidleyDefinition(slot.EnemyDefinitionPointer))
                {
                    // Both Ridley mains call DrawRidleyTail/DrawRidleyWings before the
                    // common WriteEnemyOams pass emits the extended body. Appending these
                    // here preserves both that OAM order and the enemy's normal layer queue.
                    DrawRidleySupplementalSprites(oam, slot, cameraX, cameraY);
                }
                if (!slot.ExtraProperties.HasAny(EnemyExtraProperties.UsesExtendedSpritemap))
                {
                    DrawEnemySpritemap(oam, slot.Definition.Bank,
                        slot.SpritemapPointer,
                        originX,
                        originY,
                        drawPaletteIndex,
                        slot.VramTilesIndex);
                    continue;
                }

                if (TileArtwork?.ExtendedFrames?.TryGetDisplay(slot.Definition.Bank,
                        slot.SpritemapPointer,
                        out ReadOnlyMemory<EnemyExtendedDrawComponent> installed) == true)
                {
                    // Resolve both halves through one presentation binding. Validate
                    // the composition before any BG2 side effects; physical selectors
                    // continue to belong to compiled animation and collision programs.
                    ushort selected = TileArtwork.ExtendedFrames.GetDisplayPointer(
                        slot.Definition.Bank, slot.SpritemapPointer);
                    ApplyInstalledEnemyBg2Frame(slot, selected);
                    // This installed frame owns only the composite's visual offsets
                    // and OAM parts. Collision still uses the original engine-owned
                    // extended hitbox records, never the editable JSON geometry.
                    foreach (EnemyExtendedDrawComponent component in installed.Span)
                    {
                        ushort componentX = unchecked((ushort)(originX + component.OffsetX));
                        ushort componentY = unchecked((ushort)(originY + component.OffsetY));
                        if (((componentX + 128) & 0xfe00) != 0 ||
                            ((componentY + 128) & 0xfe00) != 0)
                            continue;
                        oam.AddEnemySpritemap(component.Parts, componentX,
                            componentY, drawPaletteIndex, slot.VramTilesIndex,
                            clipVerticalWrap: true,
                            originYIsOnScreen: (componentY >> 8) == 0);
                    }
                    continue;
                }

                throw new InvalidDataException(
                    $"Enemy ${(int)slot.EnemyDefinitionPointer:X4} extended frame " +
                    $"${slot.Definition.Bank:X2}:{slot.SpritemapPointer:X4} has no installed presentation.");
            }

            // EnemyGraphicsDrawnHook runs after the complete ordinary enemy queue. Mother
            // Brain installs it from its hidden head record, but the hook itself belongs to
            // the record's authored layer-five pass and therefore must not run in both of
            // the frontend's disjoint low/high-priority layer ranges.
            if (layer == 5)
            {
                DrawMotherBrainHook(oam, cameraX, cameraY);
                DrawDeadTorizoHook(oam, cameraX, cameraY);
            }
        }
    }

    /// <summary>
    /// Ports the palette selection at <c>$A0:9473-$A0:949A</c>, immediately before
    /// <c>WriteEnemyOams</c> emits either an ordinary or extended enemy spritemap.
    /// </summary>
    /// <remarks>
    /// A hit does not replace an enemy's stored graphics-set palette. On alternating
    /// phases of the shared enemy frame counter, the writer temporarily supplies OBJ
    /// palette zero; the following draw restores the actor's assigned palette. Frozen
    /// flashing similarly selects OBJ palette six. Keeping this at the common writer is
    /// important: Space Pirates are merely the first ordinary enemies whose missing flash
    /// was obvious, and a family-specific palette mutation would corrupt every later draw.
    /// Ridley's translated AI already publishes the exact common-writer palette because
    /// its body, wings, and tail must share one latched phase.
    /// </remarks>
    private ushort SelectCommonEnemyDrawPalette(RoomEnemySlot slot)
    {
        if (IsRidleyDefinition(slot.EnemyDefinitionPointer) && Ridley is not null)
            return Ridley.CommonDrawPaletteIndex;

        if (slot.FlashTimer != 0 && (_randomEnemyCounter & 2) != 0)
            return 0;

        if (slot.FrozenTimer != 0 &&
            (slot.FrozenTimer >= 0x005a || (slot.FrozenTimer & 2) != 0))
        {
            return 0x0c00;
        }

        return slot.PaletteIndex;
    }

    private void LoadGraphicsSet(
        ISnesAddressSpace bus,
        ushort tilesetPointer,
        SnesVram vram,
        SnesCgram cgram)
    {
        RoomEnemyGraphicsSetDefinition graphicsSet =
            ResolveRoomEnemyGraphicsSet(bus, tilesetPointer);
        int nextEnemyTileIndex = 0;
        int nextStagingOffset = RoomEnemyRomLayout.OrdinaryStagingOffset;

        foreach (RoomEnemyGraphicsSetHeader source in graphicsSet.Records.Span)
        {
            EnemyDefinitionId definitionPointer = source.DefinitionPointer;
            if (_graphicsSet.Count == MaximumGraphicsSetCount)
            {
                throw new InvalidDataException(
                    $"Enemy graphics set $B4:{tilesetPointer:X4} exceeds the native four-entry arrays.");
            }

            ushort vramDestination = source.VramDestination;
            // Retail enemy headers are immutable gameplay definitions. Graphics uploads
            // use the selected extracted artwork, but
            // the header itself must not be re-read from the cartridge on room entry.
            RoomEnemyDefinition definition = ResolveRoomEnemyDefinition(bus, definitionPointer);

            // ProcessEnemyTilesets copies one complete sixteen-color OBJ palette from the
            // enemy's selected code/data bank. Its assembly masks the entire low byte before
            // adding the native eight-row OBJ bias. Retail records use values zero through
            // seven, but retaining the wider mask makes corrupt data fail visibly.
            int destinationColor = ((vramDestination & 0x00ff) + 8) * 16;
            (TileArtwork ?? throw new InvalidOperationException(
                "Enemy population requires installed palette artwork."))
                .LoadPaletteTo(definitionPointer, cgram, destinationColor);

            int byteCount = definition.TileDataSize & 0x7fff;
            int stagingOffset = (definition.TileDataSize & 0x8000) != 0
                ? (vramDestination & 0x3000) >> 3
                : nextStagingOffset;
            int vramByteOffset = RoomEnemyRomLayout.VramByteBase + stagingOffset;
            if (vramByteOffset < 0 || vramByteOffset + byteCount > SnesVram.ByteCount)
            {
                throw new InvalidDataException(
                    $"Enemy ${(int)definitionPointer:X4} tile DMA would leave VRAM: " +
                    $"offset ${vramByteOffset:X4}, size ${byteCount:X4}.");
            }

            TileArtwork!.LoadTo(definitionPointer, byteCount, vram, vramByteOffset);

            _graphicsSet.Add(new RoomEnemyGraphicsSetEntry(
                definitionPointer,
                vramDestination,
                unchecked((ushort)nextEnemyTileIndex),
                definition,
                stagingOffset,
                byteCount));

            // The original adds the unmasked size word for both counters. Retail ordinary
            // entries, including Landing Site, have bit 15 clear; retaining that arithmetic
            // makes malformed/high-bit records visible instead of silently normalized.
            nextEnemyTileIndex = unchecked((ushort)(nextEnemyTileIndex + (definition.TileDataSize >> 5)));
            nextStagingOffset = unchecked((ushort)(nextStagingOffset + definition.TileDataSize));
        }
    }

    private void LoadPopulation(
        ISnesAddressSpace bus,
        RoomEnemyPopulationDefinition populationDefinition,
        RoomLevelData? level,
        SamusState? samus,
        ushort controllerInput,
        ushort cameraX)
    {
        ReadOnlySpan<RoomEnemyPopulationRecord> records = populationDefinition.Records.Span;
        for (int slotIndex = 0; slotIndex < records.Length; slotIndex++)
        {
            RoomEnemyPopulationRecord population = records[slotIndex];
            EnemyDefinitionId definitionPointer = population.DefinitionPointer;
            if (slotIndex == MaximumEnemyCount)
            {
                throw new InvalidDataException(
                    $"Enemy population $A1:{populationDefinition.Pointer:X4} has more than 32 records.");
            }

            RoomEnemyDefinition definition = ResolveRoomEnemyDefinition(bus, definitionPointer);
            RoomEnemySlot slot = _slots[slotIndex];
            InitializeSlotFromDefinition(slot, population, definition);
            if (definition.BossId != 0)
                BossId = definition.BossId;
            RunInitializationAi(slot, level, samus, controllerInput, cameraX);

            // InitializeEnemies deliberately clears the init routine's immediate map.
            // Disable-Samus-collision actors receive the canonical empty map until their
            // first instruction-list tick replaces it. Gunship properties contain $2000.
            slot.SpritemapPointer = slot.Properties.HasAny(EnemyProperties.ProcessInstructions)
                ? slot.ExtraProperties.HasAny(EnemyExtraProperties.UsesExtendedSpritemap)
                    ? (ushort)0x804f
                    : (ushort)0x804d
                : (ushort)0;

        }

        // InitializeEnemies returns early for an initially empty population. It has
        // already zeroed both enemy counters, but does not rewrite the previous
        // first-free index or death quota. Nonempty populations publish these after
        // all initialization callbacks, exactly as the native terminator path does.
        if (records.IsEmpty)
            return;
        DeathQuota = populationDefinition.DeathQuota;
        EnemyCount = unchecked((ushort)records.Length);
        FirstFreeEnemyIndex = unchecked((ushort)(records.Length * NativeSlotSize));
    }

    private void InitializeSlotFromDefinition(
        RoomEnemySlot slot,
        RoomEnemyPopulationRecord population,
        RoomEnemyDefinition definition)
    {
        (ushort tileIndex, ushort paletteIndex) = FindGraphicsIndexes(population.DefinitionPointer);

        slot.EnemyDefinitionPointer = population.DefinitionPointer;
        slot.Definition = definition;
        slot.XRadius = definition.XRadius;
        slot.YRadius = definition.YRadius;
        slot.Health = definition.Health;
        slot.Layer = definition.Layer;
        slot.XPosition = population.XPosition;
        slot.YPosition = population.YPosition;
        slot.CurrentInstruction = population.InitializationParameter;
        _ = population.Properties.ReadFlagsChecked();
        slot.Properties = population.Properties;
        slot.ExtraProperties = population.ExtraProperties;
        slot.AiHandlerBits = 0;
        slot.Parameter1 = population.Parameter1;
        slot.Parameter2 = population.Parameter2;
        slot.Timer = 0;
        slot.InstructionTimer = 1;
        slot.FrameCounter = 0;
        slot.AiBank = definition.Bank;
        slot.HurtAiTime = definition.HurtAiTime;
        slot.PaletteIndex = paletteIndex;
        slot.VramTilesIndex = tileIndex;
        slot.Spawn = new RoomEnemySpawnSnapshot(
            population,
            tileIndex);
    }

    private (ushort TileIndex, ushort PaletteIndex) FindGraphicsIndexes(EnemyDefinitionId definitionPointer)
    {
        foreach (RoomEnemyGraphicsSetEntry entry in _graphicsSet)
        {
            if (entry.DefinitionPointer == definitionPointer)
            {
                return (
                    entry.VramTilesIndex,
                    unchecked((ushort)((entry.VramDestination & 0x000f) << 9)));
            }
        }

        // LoadEnemyGfxIndexes uses standard sprite tile zero and OBJ palette five when an
        // actor has no room graphics-set record. That fallback supports invisible/control
        // enemies without synthesizing an asset association.
        return (0, 0x0a00);
    }

    private void RunInitializationAi(
        RoomEnemySlot slot,
        RoomLevelData? level = null,
        SamusState? samus = null,
        ushort controllerInput = 0,
        ushort cameraX = 0)
    {
        int address = (slot.Definition.Bank << 16) | slot.Definition.InitializationAiPointer;
        var routine = (EnemyAiRoutine)address;
        if (!Enum.IsDefined(routine))
            throw new InvalidDataException(
                $"Enemy ${(int)slot.EnemyDefinitionPointer:X4} initialization AI ${address:X6} is not translated.");
        switch (routine)
        {
            case EnemyAiRoutine.InitAI_Crocomire when slot.EnemyDefinitionPointer == EnemyDefinitionId.Crocomire:
                InitializeCrocomire(slot);
                return;
            case EnemyAiRoutine.InitAI_SporeSpawn when slot.EnemyDefinitionPointer == EnemyDefinitionId.SporeSpawn:
                InitializeSporeSpawn(slot);
                return;
            case EnemyAiRoutine.InitAI_CrocomireTongue when slot.EnemyDefinitionPointer == EnemyDefinitionId.CrocomireTongue:
                InitializeCrocomireTongue(slot);
                return;
            case EnemyAiRoutine.InitAI_Magdollite when slot.EnemyDefinitionPointer == EnemyDefinitionId.Magdollite:
                InitializeMagdollite(slot, samus);
                return;
            case EnemyAiRoutine.InitAI_ShipTop:
                InitializeGunshipTop(slot);
                return;
            case EnemyAiRoutine.InitAI_Boyon when slot.EnemyDefinitionPointer == EnemyDefinitionId.Boyon:
                InitializeBoyon(slot);
                return;
            case EnemyAiRoutine.InitAI_Stoke when slot.EnemyDefinitionPointer == EnemyDefinitionId.Stoke:
                InitializeStoke(slot);
                return;
            case EnemyAiRoutine.InitAI_MamaTurtle when slot.EnemyDefinitionPointer == EnemyDefinitionId.MamaTurtle:
                InitializeMamaTurtle(slot);
                return;
            case EnemyAiRoutine.InitAI_BabyTurtle when slot.EnemyDefinitionPointer == EnemyDefinitionId.BabyTurtle:
                InitializeBabyTurtle(slot);
                return;
            case EnemyAiRoutine.InitAI_Puyo when slot.EnemyDefinitionPointer == EnemyDefinitionId.Puyo:
                InitializePuyo(slot);
                return;
            case EnemyAiRoutine.InitAI_Cacatac when slot.EnemyDefinitionPointer == EnemyDefinitionId.Cacatac:
                InitializeCacatac(slot);
                return;
            case EnemyAiRoutine.InitAI_Owtch when slot.EnemyDefinitionPointer == EnemyDefinitionId.Owtch:
                InitializeOwtch(slot);
                return;
            case EnemyAiRoutine.InitAI_Multiviola when slot.EnemyDefinitionPointer == EnemyDefinitionId.Multiviola:
                InitializeMultiviola(slot);
                return;
            case EnemyAiRoutine.InitAI_Polyp when slot.EnemyDefinitionPointer == EnemyDefinitionId.LavaRocks:
                InitializePolyp(slot);
                return;
            case EnemyAiRoutine.InitAI_Rinka when slot.EnemyDefinitionPointer == EnemyDefinitionId.Rinka:
                InitializeRinka(slot);
                return;
            case EnemyAiRoutine.InitAI_Rio when slot.EnemyDefinitionPointer == EnemyDefinitionId.Rio:
                InitializeRio(slot);
                return;
            case EnemyAiRoutine.InitAI_Squeept when slot.EnemyDefinitionPointer == EnemyDefinitionId.Squeept:
                InitializeNorfairLavaJumpingEnemy(slot);
                return;
            case EnemyAiRoutine.InitAI_Geruta when slot.EnemyDefinitionPointer == EnemyDefinitionId.Geruta:
                InitializeNorfairRio(slot);
                return;
            case EnemyAiRoutine.InitAI_Holtz when slot.EnemyDefinitionPointer == EnemyDefinitionId.Holtz:
                InitializeLowerNorfairRio(slot);
                return;
            case EnemyAiRoutine.InitAI_Oum when slot.EnemyDefinitionPointer == EnemyDefinitionId.Oum:
                InitializeMaridiaLargeSnail(slot);
                return;
            case EnemyAiRoutine.InitAI_GRipper when slot.EnemyDefinitionPointer == EnemyDefinitionId.GRipper:
                InitializeGRipper(slot);
                return;
            case EnemyAiRoutine.InitAI_Ripper2 when slot.EnemyDefinitionPointer == EnemyDefinitionId.Ripper2:
                InitializeRipper2(slot);
                return;
            case EnemyAiRoutine.InitAI_Dragon when slot.EnemyDefinitionPointer == EnemyDefinitionId.Dragon:
                InitializeDragon(slot);
                return;
            case EnemyAiRoutine.InitAI_ShutterGrowing when slot.EnemyDefinitionPointer == EnemyDefinitionId.ShutterGrowing:
                InitializeGrowingShutter(slot);
                return;
            case EnemyAiRoutine.InitAI_ShutterShootable_ShutterDestroyable
                when slot.EnemyDefinitionPointer is
                EnemyDefinitionId.ShutterShootable or EnemyDefinitionId.ShutterDestroyable:
            case EnemyAiRoutine.InitAI_Kamer when slot.EnemyDefinitionPointer == EnemyDefinitionId.Kamer:
                InitializeVerticalShutter(slot);
                return;
            case EnemyAiRoutine.InitAI_ShutterHorizShootable when slot.EnemyDefinitionPointer == EnemyDefinitionId.ShutterHorizShootable:
                InitializeHorizontalShutter(slot, samus);
                return;
            case EnemyAiRoutine.InitAI_Elevator when slot.EnemyDefinitionPointer == EnemyDefinitionId.Elevator:
                InitializeElevator(slot, samus);
                return;
            case EnemyAiRoutine.InitAI_Fune_Namihe when IsFuneNamiheDefinition(slot.EnemyDefinitionPointer):
                InitializeFuneNamihe(slot);
                return;
            case EnemyAiRoutine.InitAI_ShipBottomEntrance:
                InitializeGunshipBottom(slot);
                return;
            case EnemyAiRoutine.InitAI_CeresSteam:
                InitializeCeresSteam(slot);
                return;
            case EnemyAiRoutine.InitAI_CeresDoor:
                InitializeCeresDoor(slot);
                return;
            case EnemyAiRoutine.InitAI_Ridley when slot.EnemyDefinitionPointer == EnemyDefinitionId.RidleyCeres:
                InitializeCeresRidley(slot);
                return;
            case EnemyAiRoutine.InitAI_Ridley when slot.EnemyDefinitionPointer == EnemyDefinitionId.Ridley:
                InitializeNorfairRidley(slot);
                return;
            case EnemyAiRoutine.InitAI_RidleyExplosion when slot.EnemyDefinitionPointer == EnemyDefinitionId.RidleyExplosion:
                InitializeNorfairRidleyExplosion(
                    slot,
                    _slots[0],
                    Ridley ?? throw new InvalidOperationException(
                        "A Ridley breakup actor has no shared Ridley owner."));
                return;
            case EnemyAiRoutine.InitAI_Boulder when slot.EnemyDefinitionPointer == EnemyDefinitionId.Boulder:
                InitializeBoulder(slot);
                return;
            case EnemyAiRoutine.InitAI_Zebetite when slot.EnemyDefinitionPointer == EnemyDefinitionId.Zebetite:
                InitializeZebetite(slot);
                return;
            case EnemyAiRoutine.InitAI_Etecoon when slot.EnemyDefinitionPointer == EnemyDefinitionId.Etecoon:
                InitializeEtecoon(slot);
                return;
            case EnemyAiRoutine.InitAI_Dachora when slot.EnemyDefinitionPointer == EnemyDefinitionId.Dachora:
                InitializeDachora(slot);
                return;
            case EnemyAiRoutine.InitAI_Evir when slot.EnemyDefinitionPointer == EnemyDefinitionId.Evir:
                InitializeEvir(slot, samus);
                return;
            case EnemyAiRoutine.InitAI_EvirProjectile when slot.EnemyDefinitionPointer == EnemyDefinitionId.EvirProjectile:
                InitializeEvirProjectile(slot);
                return;
            case EnemyAiRoutine.InitAI_Eye when slot.EnemyDefinitionPointer == EnemyDefinitionId.Eye:
                InitializeMorphBallEye(slot);
                return;
            case EnemyAiRoutine.InitAI_Coven when slot.EnemyDefinitionPointer == EnemyDefinitionId.Coven:
                InitializeWreckedShipGhost(slot);
                return;
            case EnemyAiRoutine.InitAI_YappingMaw when slot.EnemyDefinitionPointer == EnemyDefinitionId.YappingMaw:
                InitializeYappingMaw(slot);
                return;
            case EnemyAiRoutine.InitAI_Ripper when slot.EnemyDefinitionPointer == EnemyDefinitionId.Ripper:
                InitializeRipper(slot);
                return;
            case EnemyAiRoutine.InitAI_Choot when slot.EnemyDefinitionPointer == EnemyDefinitionId.Choot:
                InitializeChoot(slot);
                return;
            case EnemyAiRoutine.InitAI_Sciser when slot.EnemyDefinitionPointer == EnemyDefinitionId.Sciser:
                InitializeCrawler(slot, CrawlerAnimationFamily.Sciser, speciesInstructionOffset: 8);
                return;
            case EnemyAiRoutine.InitAI_Zero when slot.EnemyDefinitionPointer == EnemyDefinitionId.Zero:
                InitializeCrawler(slot, CrawlerAnimationFamily.Zero, speciesInstructionOffset: 10);
                return;
            case EnemyAiRoutine.InitAI_Viola when slot.EnemyDefinitionPointer == EnemyDefinitionId.Viola:
                InitializeCrawler(slot, CrawlerAnimationFamily.Viola, speciesInstructionOffset: 6);
                return;
            case EnemyAiRoutine.InitAI_Zeela when slot.EnemyDefinitionPointer == EnemyDefinitionId.Zeela:
            case EnemyAiRoutine.InitAI_Sova when slot.EnemyDefinitionPointer == EnemyDefinitionId.Sova:
            case EnemyAiRoutine.InitAI_Zoomer_MZoomer when slot.EnemyDefinitionPointer is EnemyDefinitionId.Zoomer or EnemyDefinitionId.MZoomer:
                InitializeCrawler(slot, CrawlerAnimationFamily.Shared);
                return;
            case EnemyAiRoutine.InitAI_HZoomer when slot.EnemyDefinitionPointer == EnemyDefinitionId.HZoomer:
                InitializeHZoomer(slot);
                return;
            case EnemyAiRoutine.InitAI_Skree when slot.EnemyDefinitionPointer == EnemyDefinitionId.Skree:
                InitializeSkree(slot);
                return;
            case EnemyAiRoutine.InitAI_Mellow_Mella_Menu
                when slot.EnemyDefinitionPointer is
                EnemyDefinitionId.Mellow or EnemyDefinitionId.Mella or EnemyDefinitionId.Menu:
                InitializeFly(slot);
                return;
            case EnemyAiRoutine.InitAI_Sbug when slot.EnemyDefinitionPointer is EnemyDefinitionId.Sbug or EnemyDefinitionId.Sbug2:
                InitializeSbug(slot);
                return;
            case EnemyAiRoutine.InitAI_Mochtroid when slot.EnemyDefinitionPointer == EnemyDefinitionId.Mochtroid:
                InitializeMochtroid(slot);
                return;
            case EnemyAiRoutine.InitAI_Metroid when slot.EnemyDefinitionPointer == EnemyDefinitionId.Metroid:
                InitializeMetroid(slot);
                return;
            case EnemyAiRoutine.InitAI_Hopper when IsHopperDefinition(slot.EnemyDefinitionPointer):
                InitializeHopper(slot);
                return;
            case EnemyAiRoutine.InitAI_Zoa when slot.EnemyDefinitionPointer == EnemyDefinitionId.Zoa:
                InitializeZoa(slot);
                return;
            case EnemyAiRoutine.InitAI_Yard when slot.EnemyDefinitionPointer == EnemyDefinitionId.Yard:
                InitializeYard(slot);
                return;
            case EnemyAiRoutine.InitAI_Waver when slot.EnemyDefinitionPointer == EnemyDefinitionId.Waver:
                InitializeWaver(slot);
                return;
            case EnemyAiRoutine.InitAI_Metaree when slot.EnemyDefinitionPointer == EnemyDefinitionId.Metaree:
                InitializeMetaree(slot);
                return;
            case EnemyAiRoutine.InitAI_Fireflea when slot.EnemyDefinitionPointer == EnemyDefinitionId.Fireflea:
                InitializeFireflea(slot);
                return;
            case EnemyAiRoutine.InitAI_Skultera when slot.EnemyDefinitionPointer == EnemyDefinitionId.Skultera:
                InitializeSkultera(slot);
                return;
            case EnemyAiRoutine.InitAI_Kamer2 when slot.EnemyDefinitionPointer == EnemyDefinitionId.Kamer2:
            case EnemyAiRoutine.InitAI_Tripper when slot.EnemyDefinitionPointer == EnemyDefinitionId.Tripper:
                InitializePlatform(slot);
                return;
            case EnemyAiRoutine.InitAI_Alcoon when slot.EnemyDefinitionPointer == EnemyDefinitionId.Alcoon:
                InitializeAlcoon(slot, level);
                return;
            case EnemyAiRoutine.InitAI_Kago when slot.EnemyDefinitionPointer == EnemyDefinitionId.Kago:
                InitializeKago(slot);
                return;
            case EnemyAiRoutine.InitAI_Beetom when slot.EnemyDefinitionPointer == EnemyDefinitionId.Beetom:
                InitializeBeetom(slot, samus, controllerInput);
                return;
            case EnemyAiRoutine.InitAI_Powamp when slot.EnemyDefinitionPointer == EnemyDefinitionId.Powamp:
                InitializePowamp(slot);
                return;
            case EnemyAiRoutine.InitAI_Robot when slot.EnemyDefinitionPointer == EnemyDefinitionId.Robot:
            case EnemyAiRoutine.InitAI_RobotNoPower when slot.EnemyDefinitionPointer == EnemyDefinitionId.RobotNoPower:
                InitializeWorkRobot(slot);
                return;
            case EnemyAiRoutine.InitAI_Bull when slot.EnemyDefinitionPointer == EnemyDefinitionId.Bull:
                InitializeBull(slot);
                return;
            case EnemyAiRoutine.InitAI_Atomic when slot.EnemyDefinitionPointer == EnemyDefinitionId.Atomic:
                InitializeAtomic(slot);
                return;
            case EnemyAiRoutine.InitAI_Spark when slot.EnemyDefinitionPointer == EnemyDefinitionId.Spark:
                InitializeSpark(slot);
                return;
            case EnemyAiRoutine.InitAI_FaceBlock when
                slot.EnemyDefinitionPointer == EnemyDefinitionId.FaceBlock:
                InitializeBlueBrinstarFaceBlock(slot, samus);
                return;
            case EnemyAiRoutine.InitAI_Kihunter when IsKiHunterBodyDefinition(slot.EnemyDefinitionPointer):
                InitializeKiHunter(slot);
                return;
            case EnemyAiRoutine.InitAI_KihunterWings when IsKiHunterWingDefinition(slot.EnemyDefinitionPointer):
                InitializeKiHunterWings(slot);
                return;
            case EnemyAiRoutine.InitAI_Zeb_Zebbo when IsBrinstarPipeBugDefinition(slot.EnemyDefinitionPointer):
                InitializeBrinstarPipeBug(slot);
                return;
            case EnemyAiRoutine.InitAI_Gamet when slot.EnemyDefinitionPointer == EnemyDefinitionId.Gamet:
                InitializeNorfairPipeBug(slot);
                return;
            case EnemyAiRoutine.InitAI_Geega when slot.EnemyDefinitionPointer == EnemyDefinitionId.Geega:
                InitializeYellowPipeBug(slot);
                return;
            case EnemyAiRoutine.InitAI_Botwoon when slot.EnemyDefinitionPointer == EnemyDefinitionId.Botwoon:
                InitializeBotwoon(slot);
                return;
            case EnemyAiRoutine.InitAI_EtecoonEscape when slot.EnemyDefinitionPointer == EnemyDefinitionId.EtecoonEscape:
                InitializeEscapeEtecoon(slot);
                return;
            case EnemyAiRoutine.InitAI_DachoraEscape when slot.EnemyDefinitionPointer == EnemyDefinitionId.DachoraEscape:
                InitializeEscapeDachora(slot);
                return;
            case EnemyAiRoutine.InitAI_KzanTop when slot.EnemyDefinitionPointer == EnemyDefinitionId.KzanTop:
                InitializeKzanTop(slot);
                return;
            case EnemyAiRoutine.InitAI_KzanBottom when slot.EnemyDefinitionPointer == EnemyDefinitionId.KzanBottom:
                InitializeKzanBottom(slot);
                return;
            case EnemyAiRoutine.InitAI_Hibashi when slot.EnemyDefinitionPointer == EnemyDefinitionId.Hibashi:
                InitializeHibashi(slot);
                return;
            case EnemyAiRoutine.InitAI_Puromi when slot.EnemyDefinitionPointer == EnemyDefinitionId.Puromi:
                InitializeNuclearWaffle(slot);
                return;
            case EnemyAiRoutine.InitAI_MiniKraid when slot.EnemyDefinitionPointer == EnemyDefinitionId.MiniKraid:
                InitializeFakeKraid(slot, samus);
                return;
            case EnemyAiRoutine.InitAI_PirateWalking when IsWalkingSpacePirateDefinition(slot.EnemyDefinitionPointer):
                InitializeWalkingSpacePirate(slot);
                return;
            case EnemyAiRoutine.InitAI_PirateWall when IsWallSpacePirateDefinition(slot.EnemyDefinitionPointer):
                InitializeWallSpacePirate(slot);
                return;
            case EnemyAiRoutine.InitAI_PirateNinja when IsNinjaSpacePirateDefinition(slot.EnemyDefinitionPointer):
                InitializeNinjaSpacePirate(slot);
                return;
            case EnemyAiRoutine.InitAI_Torizo
                when slot.EnemyDefinitionPointer is
                EnemyDefinitionId.BombTorizo or EnemyDefinitionId.GoldenTorizo:
                InitializeBombTorizo(slot, samus, controllerInput);
                return;
            case EnemyAiRoutine.InitAI_Kraid when slot.EnemyDefinitionPointer == EnemyDefinitionId.Kraid:
                InitializeKraidBody(slot);
                return;
            case EnemyAiRoutine.InitAI_KraidArm when slot.EnemyDefinitionPointer == EnemyDefinitionId.KraidArm:
                InitializeKraidArm(slot);
                return;
            case EnemyAiRoutine.InitAI_KraidLintTop when slot.EnemyDefinitionPointer == EnemyDefinitionId.KraidLintTop:
                InitializeKraidLint(slot, expectedSlot: 2);
                return;
            case EnemyAiRoutine.InitAI_KraidLintMiddle when slot.EnemyDefinitionPointer == EnemyDefinitionId.KraidLintMiddle:
                InitializeKraidLint(slot, expectedSlot: 3);
                return;
            case EnemyAiRoutine.InitAI_KraidLintBottom when slot.EnemyDefinitionPointer == EnemyDefinitionId.KraidLintBottom:
                InitializeKraidLint(slot, expectedSlot: 4);
                return;
            case EnemyAiRoutine.InitAI_KraidFoot when slot.EnemyDefinitionPointer == EnemyDefinitionId.KraidFoot:
                InitializeKraidFoot(slot);
                return;
            case EnemyAiRoutine.InitAI_KraidNail when slot.EnemyDefinitionPointer == EnemyDefinitionId.KraidNail:
                InitializeKraidNail(slot, expectedSlot: 6);
                return;
            case EnemyAiRoutine.InitAI_KraidNailBad when slot.EnemyDefinitionPointer == EnemyDefinitionId.KraidNailBad:
                InitializeKraidNail(slot, expectedSlot: 7);
                return;
            case EnemyAiRoutine.InitAI_PhantoonBody when slot.EnemyDefinitionPointer == EnemyDefinitionId.PhantoonBody:
                InitializePhantoonBody(slot);
                return;
            case EnemyAiRoutine.InitAI_Phantoon_Eye_Tentacles_Mouth
                when slot.EnemyDefinitionPointer is
                EnemyDefinitionId.PhantoonEye or EnemyDefinitionId.PhantoonTentacles or EnemyDefinitionId.PhantoonMouth:
                InitializePhantoonPart(slot);
                return;
            case EnemyAiRoutine.InitAI_DraygonBody when slot.EnemyDefinitionPointer == EnemyDefinitionId.DraygonBody:
                InitializeDraygonBody(slot);
                return;
            case EnemyAiRoutine.InitAI_DraygonEye when slot.EnemyDefinitionPointer == EnemyDefinitionId.DraygonEye:
            case EnemyAiRoutine.InitAI_DraygonTail when slot.EnemyDefinitionPointer == EnemyDefinitionId.DraygonTail:
            case EnemyAiRoutine.InitAI_DraygonArms when slot.EnemyDefinitionPointer == EnemyDefinitionId.DraygonArms:
                InitializeDraygonPart(slot);
                return;
            case EnemyAiRoutine.InitAI_MotherBrainBody when slot.EnemyDefinitionPointer == EnemyDefinitionId.MotherBrainBody:
                InitializeMotherBrainBody(slot);
                return;
            case EnemyAiRoutine.InitAI_MotherBrainHead when slot.EnemyDefinitionPointer == EnemyDefinitionId.MotherBrainHead:
                InitializeMotherBrainHead(slot);
                return;
            case EnemyAiRoutine.InitAI_MotherBrainTubes when slot.EnemyDefinitionPointer == EnemyDefinitionId.MotherBrainTubes:
                InitializeMotherBrainFallingTube(slot);
                return;
            case EnemyAiRoutine.InitAI_BabyMetroidCutscene when slot.EnemyDefinitionPointer == EnemyDefinitionId.BabyMetroidCutscene:
                InitializeMotherBrainBabyMetroid(slot);
                return;
            case EnemyAiRoutine.InitAI_CorpseTorizo when slot.EnemyDefinitionPointer == EnemyDefinitionId.CorpseTorizo:
                InitializeDeadTorizo(slot);
                return;
            case EnemyAiRoutine.InitAI_CorpseSidehopper when slot.EnemyDefinitionPointer == EnemyDefinitionId.CorpseSidehopper:
                InitializeDeadSidehopper(slot);
                return;
            case EnemyAiRoutine.InitAI_CorpseZoomer when slot.EnemyDefinitionPointer == EnemyDefinitionId.CorpseZoomer:
            case EnemyAiRoutine.InitAI_CorpseRipper when slot.EnemyDefinitionPointer == EnemyDefinitionId.CorpseRipper:
            case EnemyAiRoutine.InitAI_CorpseSkree when slot.EnemyDefinitionPointer == EnemyDefinitionId.CorpseSkree:
                InitializeDeadTourianCorpse(slot);
                return;
            case EnemyAiRoutine.InitAI_BabyMetroid when slot.EnemyDefinitionPointer == EnemyDefinitionId.BabyMetroid:
                InitializeShitroid(slot, cameraX);
                return;
            case EnemyAiRoutine.InitAI_TourianStatue when slot.EnemyDefinitionPointer == EnemyDefinitionId.TourianStatue:
                InitializeTourianEntranceStatue(slot);
                return;
            case EnemyAiRoutine.InitAI_Shaktool when slot.EnemyDefinitionPointer == EnemyDefinitionId.Shaktool:
                InitializeShaktool(slot);
                return;
            case EnemyAiRoutine.InitAI_NoobTubeCrack when slot.EnemyDefinitionPointer == EnemyDefinitionId.NoobTubeCrack:
                InitializeN00bTubeCracks();
                return;
            case EnemyAiRoutine.InitAI_Chozo when slot.EnemyDefinitionPointer == EnemyDefinitionId.Chozo:
                InitializeChozoStatue(slot);
                return;
            case EnemyAiRoutine.RTL_A2804C:
                return;
            default:
                throw new InvalidDataException(
                    $"Enemy ${(int)slot.EnemyDefinitionPointer:X4} initialization AI ${address:X6} is not translated.");
        }
    }

    private void InitializeGunshipTop(RoomEnemySlot slot)
    {
        slot.Properties = slot.Properties.With(
            EnemyProperties.ProcessInstructions | EnemyProperties.IgnoreSamusCollision);
        slot.InstructionTimer = 1;
        slot.Timer = 0;
        slot.CurrentInstruction = GunshipInstructionProgramDefinitions.TopHull;
        slot.PaletteIndex = EnemyPaletteBits.Palette7;
        if (_gunshipLoadScenario == GunshipLoadScenario.EscapingCeres)
        {
            SamusState samus = _samusAtEnemyInitialization
                ?? throw new InvalidOperationException(
                    "Post-Ceres gunship initialization requires the loader's Samus actor.");
            // `$A2:A669-$A678` attaches the top hull seventeen pixels above Samus and
            // enters function three, the high-altitude descent used only after Ceres.
            slot.YPosition = unchecked((ushort)(samus.YPosition - 17));
            slot.VariableF = (ushort)GunshipFunction.DescendAfterCeres;
        }
        else
        {
            slot.YPosition = unchecked((ushort)(slot.YPosition - 25));
            slot.VariableE = slot.YPosition;
            slot.VariableF = (ushort)GunshipFunction.Idle;
        }
        slot.VariableD = 1;
        slot.VariableC = 0;
    }

    private void InitializeGunshipBottom(RoomEnemySlot slot)
    {
        slot.Properties = slot.Properties.With(
            EnemyProperties.ProcessInstructions | EnemyProperties.IgnoreSamusCollision);
        slot.InstructionTimer = 1;
        slot.Timer = 0;
        slot.CurrentInstruction = slot.Parameter2 != 0
            ? GunshipInstructionProgramDefinitions.BottomEntrancePad
            : GunshipInstructionProgramDefinitions.BottomHull;

        // $A2:A6F1 reads enemy_drawing_queue[(cur_enemy_index >> 1) + 106].
        // For the two Landing Site bottom slots those WRAM addresses alias the preceding
        // slot's +$20 vram_tiles_index word. Express the alias semantically, but retain the
        // dependency on physical slot order.
        if (slot.SlotIndex == 0)
            throw new InvalidDataException("Gunship bottom cannot occupy enemy slot zero.");
        slot.VramTilesIndex = _slots[slot.SlotIndex - 1].VramTilesIndex;
        slot.PaletteIndex = EnemyPaletteBits.Palette7;

        if (slot.Parameter2 != 0)
        {
            // For slot two, the equally strange +61 indexed read aliases slot zero's +$06
            // Y-position word. The opening pad sits one pixel above the top hull origin.
            if (slot.SlotIndex < 2)
                throw new InvalidDataException("Gunship entrance pad requires two preceding enemy slots.");
            slot.YPosition = unchecked((ushort)(_slots[slot.SlotIndex - 2].YPosition - 1));
        }
        else
        {
            if (_gunshipLoadScenario == GunshipLoadScenario.EscapingCeres)
            {
                SamusState samus = _samusAtEnemyInitialization
                    ?? throw new InvalidOperationException(
                        "Post-Ceres gunship bottom initialization requires Samus.");
                slot.YPosition = unchecked((ushort)(samus.YPosition + 23));
            }
            else
            {
                slot.YPosition = unchecked((ushort)(slot.YPosition + 15));
                slot.VariableD = 71;
            }
        }
        slot.VariableF = (ushort)GunshipFunction.NoOperation;
    }

    private void RunMainAi(
        RoomEnemySlot slot,
        SamusState? samus,
        ushort newlyPressedControllerInput,
        ushort controllerInput,
        RoomLevelData? level,
        ushort cameraX,
        ushort cameraY,
        SamusProjectileSystem? samusProjectiles,
        byte nmiFrameCounter8,
        SamusMode7Transform? mode7Transform = null,
        SamusBombProjectileSystem? sharedProjectiles = null,
        VramWriteQueue? vramWriteQueue = null)
    {
        int address = (slot.Definition.Bank << 16) | slot.Definition.MainAiPointer;
        var routine = (EnemyAiRoutine)address;
        if (!Enum.IsDefined(routine))
            throw new InvalidDataException(
                $"Enemy ${(int)slot.EnemyDefinitionPointer:X4} main AI ${address:X6} is not translated.");
        switch (routine)
        {
            case EnemyAiRoutine.RTL_A3804C:
                return;
            case EnemyAiRoutine.MainAI_DraygonBody when slot.EnemyDefinitionPointer == EnemyDefinitionId.DraygonBody:
                // Bank $A5 times turrets and smoke from NMI_FrameCounter ($05B6), never
                // the separate 8-bit counter at $05B5; only its low bits are tested.
                RunDraygonBodyMain(slot, samus, unchecked((byte)_enemyFrameNmiFrameCounter));
                return;
            case EnemyAiRoutine.MainAI_DraygonEye when slot.EnemyDefinitionPointer == EnemyDefinitionId.DraygonEye:
                RunDraygonPartMain(slot, samus);
                return;
            case EnemyAiRoutine.RTL_A5C5AA when slot.EnemyDefinitionPointer == EnemyDefinitionId.DraygonTail:
            case EnemyAiRoutine.RTL_A5C5C4 when slot.EnemyDefinitionPointer == EnemyDefinitionId.DraygonArms:
                RunDraygonPartMain(slot, samus);
                return;
            case EnemyAiRoutine.MainAI_HurtAI_MotherBrainBody when slot.EnemyDefinitionPointer == EnemyDefinitionId.MotherBrainBody:
                RunMotherBrainBodyMain(slot, samus, sharedProjectiles);
                return;
            case EnemyAiRoutine.MainAI_HurtAI_MotherBrainHead when slot.EnemyDefinitionPointer == EnemyDefinitionId.MotherBrainHead:
                RunMotherBrainHeadMain(slot, samus);
                return;
            case EnemyAiRoutine.MainAI_MotherBrainTubes when slot.EnemyDefinitionPointer == EnemyDefinitionId.MotherBrainTubes:
                RunMotherBrainFallingTubeMain(slot);
                return;
            case EnemyAiRoutine.MainAI_BabyMetroidCutscene when slot.EnemyDefinitionPointer == EnemyDefinitionId.BabyMetroidCutscene:
                RunMotherBrainBabyMetroidMain(slot, samus, cameraX, cameraY);
                return;
            case EnemyAiRoutine.MainAI_CorpseTorizo when slot.EnemyDefinitionPointer == EnemyDefinitionId.CorpseTorizo:
                RunDeadTorizoMain(slot, samus);
                return;
            case EnemyAiRoutine.MainAI_HurtAI_CorpseEnemies when slot.EnemyDefinitionPointer == EnemyDefinitionId.CorpseSidehopper:
                RunDeadSidehopperMain(slot, samus, level, cameraX);
                return;
            case EnemyAiRoutine.MainAI_HurtAI_CorpseEnemies when IsDeadTourianCorpseDefinition(slot.EnemyDefinitionPointer):
                RunDeadTourianCorpseMain(slot, samus);
                return;
            case EnemyAiRoutine.MainAI_BabyMetroid when slot.EnemyDefinitionPointer == EnemyDefinitionId.BabyMetroid:
                RunShitroidMain(slot, samus, cameraX, cameraY, sharedProjectiles);
                return;
            case EnemyAiRoutine.MainAI_Crocomire when slot.EnemyDefinitionPointer == EnemyDefinitionId.Crocomire:
                RunCrocomireMain(slot, samus, cameraX);
                return;
            case EnemyAiRoutine.MainAI_SporeSpawn when slot.EnemyDefinitionPointer == EnemyDefinitionId.SporeSpawn:
                RunSporeSpawnMain(slot, RequireSporeSpawnState(slot));
                return;
            case EnemyAiRoutine.MainAI_CrocomireTongue when slot.EnemyDefinitionPointer == EnemyDefinitionId.CrocomireTongue:
                // $A4:F6BB is a literal RTL. The tongue's bank-$A4 instruction list and
                // extended map position the component relative to Crocomire's body.
                return;
            case EnemyAiRoutine.MainAI_Magdollite when slot.EnemyDefinitionPointer == EnemyDefinitionId.Magdollite:
                RunMagdolliteMain(slot, RequireMagdolliteState(slot), samus);
                return;
            case EnemyAiRoutine.MainAI_ShipTop:
                RunGunshipTopMain(
                    slot,
                    samus,
                    newlyPressedControllerInput,
                    vramWriteQueue);
                return;
            case EnemyAiRoutine.MainAI_Boyon when slot.EnemyDefinitionPointer == EnemyDefinitionId.Boyon:
                RunBoyonMain(slot, RequireBoyonState(slot), samus);
                return;
            case EnemyAiRoutine.MainAI_Stoke when slot.EnemyDefinitionPointer == EnemyDefinitionId.Stoke:
                RunStokeMain(slot, RequireStokeState(slot), level);
                return;
            case EnemyAiRoutine.MainAI_MamaTurtle when slot.EnemyDefinitionPointer == EnemyDefinitionId.MamaTurtle:
                RunMamaTurtleMain(
                    slot,
                    RequireMamaTurtleState(slot),
                    samus,
                    level,
                    nmiFrameCounter8);
                return;
            case EnemyAiRoutine.MainAI_BabyTurtle when slot.EnemyDefinitionPointer == EnemyDefinitionId.BabyTurtle:
                RunBabyTurtleMain(slot, RequireBabyTurtleState(slot), samus, level);
                return;
            case EnemyAiRoutine.MainAI_Puyo when slot.EnemyDefinitionPointer == EnemyDefinitionId.Puyo:
                RunPuyoMain(slot, RequirePuyoState(slot), samus, level);
                return;
            case EnemyAiRoutine.MainAI_Cacatac when slot.EnemyDefinitionPointer == EnemyDefinitionId.Cacatac:
                RunCacatacMain(slot, RequireCacatacState(slot));
                return;
            case EnemyAiRoutine.MainAI_Owtch when slot.EnemyDefinitionPointer == EnemyDefinitionId.Owtch:
                RunOwtchMain(slot, RequireOwtchState(slot));
                return;
            case EnemyAiRoutine.MainAI_Multiviola when slot.EnemyDefinitionPointer == EnemyDefinitionId.Multiviola:
                RunMultiviolaMain(slot, RequireMultiviolaState(slot), level);
                return;
            case EnemyAiRoutine.MainAI_Polyp when slot.EnemyDefinitionPointer == EnemyDefinitionId.LavaRocks:
                RunPolypMain(slot, RequirePolypState(slot), samus);
                return;
            case EnemyAiRoutine.MainAI_Rinka when slot.EnemyDefinitionPointer == EnemyDefinitionId.Rinka:
                RunRinkaMain(slot, RequireRinkaState(slot), samus);
                return;
            case EnemyAiRoutine.MainAI_Rio when slot.EnemyDefinitionPointer == EnemyDefinitionId.Rio:
                RunRioMain(
                    slot,
                    RequireRioState(slot),
                    samus,
                    level,
                    cameraX,
                    cameraY);
                return;
            case EnemyAiRoutine.MainAI_Squeept when slot.EnemyDefinitionPointer == EnemyDefinitionId.Squeept:
                RunNorfairLavaJumpingEnemyMain(
                    slot,
                    RequireNorfairLavaJumpingEnemyState(slot));
                return;
            case EnemyAiRoutine.MainAI_Geruta when slot.EnemyDefinitionPointer == EnemyDefinitionId.Geruta:
                RunNorfairRioMain(
                    slot,
                    RequireNorfairRioState(slot),
                    samus,
                    level);
                return;
            case EnemyAiRoutine.MainAI_Holtz when slot.EnemyDefinitionPointer == EnemyDefinitionId.Holtz:
                RunLowerNorfairRioMain(
                    slot,
                    RequireLowerNorfairRioState(slot),
                    samus,
                    level);
                return;
            case EnemyAiRoutine.MainAI_Oum when slot.EnemyDefinitionPointer == EnemyDefinitionId.Oum:
                RunMaridiaLargeSnailMain(
                    slot,
                    RequireMaridiaLargeSnailState(slot),
                    samus,
                    level,
                    controllerInput);
                return;
            case EnemyAiRoutine.MainAI_GRipper when slot.EnemyDefinitionPointer == EnemyDefinitionId.GRipper:
                RunGRipperMain(slot, RequireRipperVariantState(slot), level);
                return;
            case EnemyAiRoutine.MainAI_Ripper2 when slot.EnemyDefinitionPointer == EnemyDefinitionId.Ripper2:
                RunRipper2Main(slot, level);
                return;
            case EnemyAiRoutine.MainAI_Dragon when slot.EnemyDefinitionPointer == EnemyDefinitionId.Dragon:
                RunDragonMain(slot, RequireDragonState(slot), samus);
                return;
            case EnemyAiRoutine.MainAI_ShutterGrowing when slot.EnemyDefinitionPointer == EnemyDefinitionId.ShutterGrowing:
                RunGrowingShutterMain(
                    slot,
                    RequireGrowingShutterState(slot),
                    samus,
                    cameraX,
                    cameraY);
                return;
            case EnemyAiRoutine.MainAI_ShutterShootable_ShutterDestroyable_Kamer when IsVerticalShutterDefinition(slot.EnemyDefinitionPointer):
                RunVerticalShutterMain(
                    slot,
                    RequireVerticalShutterState(slot),
                    samus,
                    cameraX,
                    cameraY);
                return;
            case EnemyAiRoutine.MainAI_ShutterHorizShootable when slot.EnemyDefinitionPointer == EnemyDefinitionId.ShutterHorizShootable:
                RunHorizontalShutterMain(
                    slot,
                    RequireHorizontalShutterState(slot),
                    samus,
                    controllerInput);
                return;
            case EnemyAiRoutine.MainAI_GrappleAI_FrozenAI_Elevator when slot.EnemyDefinitionPointer == EnemyDefinitionId.Elevator:
                RunElevatorMain(
                    slot,
                    RequireElevatorState(slot),
                    samus,
                    newlyPressedControllerInput,
                    samusProjectiles);
                return;
            case EnemyAiRoutine.MainAI_Fune_Namihe when IsFuneNamiheDefinition(slot.EnemyDefinitionPointer):
                RunFuneNamiheMain(slot, RequireFuneNamiheState(slot), samus);
                return;
            case EnemyAiRoutine.MainAI_Kago when slot.EnemyDefinitionPointer == EnemyDefinitionId.Kago:
                RunKagoMain(RequireKagoState(slot));
                return;
            case EnemyAiRoutine.RTL_A2804C:
                return;
            case EnemyAiRoutine.RTL_A7804C when slot.EnemyDefinitionPointer == EnemyDefinitionId.PhantoonEye:
                // Phantoon's eye is positioned by the body and animated by its own list;
                // the header main is the literal common RTL at the start of bank $A7.
                return;
            case EnemyAiRoutine.MainAI_CeresSteam:
                RunCeresSteamMain(slot, mode7Transform);
                return;
            case EnemyAiRoutine.MainAI_CeresDoor:
                RunCeresDoorMain(slot);
                return;
            case EnemyAiRoutine.MainAI_RidleyCeres
                when slot.EnemyDefinitionPointer == EnemyDefinitionId.RidleyCeres:
                RunCeresRidleyMain(slot, samus, vramWriteQueue);
                return;
            case EnemyAiRoutine.MainAI_Ridley when slot.EnemyDefinitionPointer == EnemyDefinitionId.Ridley:
                RunNorfairRidleyMain(
                    slot,
                    samus,
                    controllerInput,
                    level,
                    samusProjectiles,
                    sharedProjectiles,
                    cameraX, cameraY);
                return;
            case EnemyAiRoutine.MainAI_RidleyExplosion when slot.EnemyDefinitionPointer == EnemyDefinitionId.RidleyExplosion:
                RunNorfairRidleyExplosionMain(slot);
                return;
            case EnemyAiRoutine.MainAI_Boulder when slot.EnemyDefinitionPointer == EnemyDefinitionId.Boulder:
                RunBoulderMain(slot, RequireBoulderState(slot), samus, level);
                return;
            case EnemyAiRoutine.MainAI_Zebetite when slot.EnemyDefinitionPointer == EnemyDefinitionId.Zebetite:
                RunZebetiteMain(slot, RequireZebetiteState(slot));
                return;
            case EnemyAiRoutine.MainAI_Etecoon when slot.EnemyDefinitionPointer == EnemyDefinitionId.Etecoon:
                RunEtecoonMain(slot, RequireEtecoonState(slot), samus, level);
                return;
            case EnemyAiRoutine.MainAI_Dachora when slot.EnemyDefinitionPointer == EnemyDefinitionId.Dachora:
                RunDachoraMain(
                    slot,
                    RequireDachoraState(slot),
                    samus,
                    level,
                    nmiFrameCounter8);
                return;
            case EnemyAiRoutine.MainAI_Evir when slot.EnemyDefinitionPointer == EnemyDefinitionId.Evir:
                RunEvirMain(slot, RequireEvirState(slot), samus);
                return;
            case EnemyAiRoutine.MainAI_EvirProjectile when slot.EnemyDefinitionPointer == EnemyDefinitionId.EvirProjectile:
                RunEvirProjectileMain(
                    slot,
                    RequireEvirState(slot),
                    samus,
                    cameraX,
                    cameraY);
                return;
            case EnemyAiRoutine.MainAI_Eye when slot.EnemyDefinitionPointer == EnemyDefinitionId.Eye:
                RunMorphBallEyeMain(slot, RequireMorphBallEyeState(slot), samus);
                return;
            case EnemyAiRoutine.MainAI_Coven when slot.EnemyDefinitionPointer == EnemyDefinitionId.Coven:
                RunWreckedShipGhostMain(slot, RequireWreckedShipGhostState(slot), samus);
                return;
            case EnemyAiRoutine.MainAI_YappingMaw when slot.EnemyDefinitionPointer == EnemyDefinitionId.YappingMaw:
                RunYappingMawMain(
                    slot,
                    RequireYappingMawState(slot),
                    samus,
                    cameraX,
                    cameraY);
                return;
            case EnemyAiRoutine.MainAI_Ripper when slot.EnemyDefinitionPointer == EnemyDefinitionId.Ripper:
                RunRipperMain(slot, level);
                return;
            case EnemyAiRoutine.MainAI_Choot when slot.EnemyDefinitionPointer == EnemyDefinitionId.Choot:
                RunChootMain(slot, RequireChootState(slot), samus);
                return;
            case EnemyAiRoutine.MainAI_Crawlers when IsSharedCrawlerDefinition(slot.EnemyDefinitionPointer):
                RunCrawlerMain(slot, level);
                return;
            case EnemyAiRoutine.MainAI_HZoomer when slot.EnemyDefinitionPointer == EnemyDefinitionId.HZoomer:
                RunHZoomerMain(slot, samus, level);
                return;
            case EnemyAiRoutine.MainAI_Skree when slot.EnemyDefinitionPointer == EnemyDefinitionId.Skree:
                RunSkreeMain(slot, samus, level);
                return;
            case EnemyAiRoutine.MainAI_Mellow_Mella_Menu
                when slot.EnemyDefinitionPointer is
                EnemyDefinitionId.Mellow or EnemyDefinitionId.Mella or EnemyDefinitionId.Menu:
                RunFlyMain(slot, samus);
                return;
            case EnemyAiRoutine.MainAI_Sbug when slot.EnemyDefinitionPointer is EnemyDefinitionId.Sbug or EnemyDefinitionId.Sbug2:
                RunSbugMain(slot, samus, level);
                return;
            case EnemyAiRoutine.MainAI_Mochtroid when slot.EnemyDefinitionPointer == EnemyDefinitionId.Mochtroid:
                RunMochtroidMain(slot, samus, level);
                return;
            case EnemyAiRoutine.MainAI_Metroid when slot.EnemyDefinitionPointer == EnemyDefinitionId.Metroid:
                RunMetroidMain(slot, samus, level);
                return;
            case EnemyAiRoutine.MainAI_Hopper when IsHopperDefinition(slot.EnemyDefinitionPointer):
                RunHopperMain(slot, samus, level);
                return;
            case EnemyAiRoutine.MainAI_Zoa when slot.EnemyDefinitionPointer == EnemyDefinitionId.Zoa:
                RunZoaMain(slot, RequireZoaState(slot), samus, cameraX, cameraY);
                return;
            case EnemyAiRoutine.MainAI_Yard when slot.EnemyDefinitionPointer == EnemyDefinitionId.Yard:
                RunYardMain(slot, samus, level);
                return;
            case EnemyAiRoutine.MainAI_Waver when slot.EnemyDefinitionPointer == EnemyDefinitionId.Waver:
                RunWaverMain(slot, RequireWaverState(slot), level);
                return;
            case EnemyAiRoutine.MainAI_Metaree when slot.EnemyDefinitionPointer == EnemyDefinitionId.Metaree:
                RunMetareeMain(slot, RequireMetareeState(slot), samus, level);
                return;
            case EnemyAiRoutine.MainAI_Fireflea when slot.EnemyDefinitionPointer == EnemyDefinitionId.Fireflea:
                RunFirefleaMain(slot, RequireFirefleaState(slot));
                return;
            case EnemyAiRoutine.MainAI_Skultera when slot.EnemyDefinitionPointer == EnemyDefinitionId.Skultera:
                RunSkulteraMain(slot, RequireSkulteraState(slot), level);
                return;
            case EnemyAiRoutine.MainAI_Tripper_Kamer2 when IsPlatformDefinition(slot.EnemyDefinitionPointer):
                RunPlatformMain(slot, RequirePlatformState(slot), samus, level);
                return;
            case EnemyAiRoutine.MainAI_Alcoon when slot.EnemyDefinitionPointer == EnemyDefinitionId.Alcoon:
                RunAlcoonMain(slot, RequireAlcoonState(slot), samus, level);
                return;
            case EnemyAiRoutine.MainAI_Beetom when slot.EnemyDefinitionPointer == EnemyDefinitionId.Beetom:
                RunBeetomMain(slot, RequireBeetomState(slot), samus, level, controllerInput);
                return;
            case EnemyAiRoutine.MainAI_Powamp when slot.EnemyDefinitionPointer == EnemyDefinitionId.Powamp:
                RunPowampMain(slot, RequirePowampState(slot), level);
                return;
            case EnemyAiRoutine.MainAI_Robot when slot.EnemyDefinitionPointer == EnemyDefinitionId.Robot:
                RunWorkRobotMain(slot, RequireWorkRobotState(slot), level);
                return;
            case EnemyAiRoutine.RTL_A8CC66 when slot.EnemyDefinitionPointer == EnemyDefinitionId.RobotNoPower:
                return;
            case EnemyAiRoutine.MainAI_Bull when slot.EnemyDefinitionPointer == EnemyDefinitionId.Bull:
                RunBullMain(slot, RequireBullState(slot), samus);
                return;
            case EnemyAiRoutine.MainAI_Atomic when slot.EnemyDefinitionPointer == EnemyDefinitionId.Atomic:
                RunAtomicMain(slot, RequireAtomicState(slot), samus);
                return;
            case EnemyAiRoutine.MainAI_Spark when slot.EnemyDefinitionPointer == EnemyDefinitionId.Spark:
                RunSparkMain(slot, RequireSparkState(slot));
                return;
            case EnemyAiRoutine.MainAI_FaceBlock when
                slot.EnemyDefinitionPointer == EnemyDefinitionId.FaceBlock:
                RunBlueBrinstarFaceBlockMain(
                    slot,
                    RequireBlueBrinstarFaceBlockState(slot),
                    samus);
                return;
            case EnemyAiRoutine.MainAI_Kihunter when IsKiHunterBodyDefinition(slot.EnemyDefinitionPointer):
            case EnemyAiRoutine.MainAI_KihunterWings when IsKiHunterWingDefinition(slot.EnemyDefinitionPointer):
                RunKiHunterMain(slot, RequireKiHunterState(slot), samus, level);
                return;
            case EnemyAiRoutine.MainAI_Zeb_Zebbo when IsBrinstarPipeBugDefinition(slot.EnemyDefinitionPointer):
            case EnemyAiRoutine.MainAI_Gamet when slot.EnemyDefinitionPointer == EnemyDefinitionId.Gamet:
            case EnemyAiRoutine.MainAI_Geega when slot.EnemyDefinitionPointer == EnemyDefinitionId.Geega:
                RunPipeBugMain(
                    slot,
                    RequirePipeBugState(slot),
                    samus,
                    cameraX,
                    cameraY);
                return;
            case EnemyAiRoutine.MainAI_Botwoon when slot.EnemyDefinitionPointer == EnemyDefinitionId.Botwoon:
                RunBotwoonMain(slot, RequireBotwoonState(slot), samus);
                return;
            case EnemyAiRoutine.MainAI_EtecoonEscape when slot.EnemyDefinitionPointer == EnemyDefinitionId.EtecoonEscape:
                RunEscapeEtecoonMain(
                    slot,
                    RequireEscapeEtecoonState(slot),
                    level);
                return;
            case EnemyAiRoutine.RTL_B3EB1A when slot.EnemyDefinitionPointer == EnemyDefinitionId.DachoraEscape:
                // $B3:EB1A is a literal RTL. Dachora's complete movement program lives in
                // its ROM instruction lists and therefore runs later in this same frame.
                return;
            case EnemyAiRoutine.MainAI_KzanTop when slot.EnemyDefinitionPointer == EnemyDefinitionId.KzanTop:
                RunKzanTopMain(slot, RequireKzanState(slot), samus);
                return;
            case EnemyAiRoutine.MainAI_KzanBottom when slot.EnemyDefinitionPointer == EnemyDefinitionId.KzanBottom:
                RunKzanBottomMain(slot);
                return;
            case EnemyAiRoutine.MainAI_Hibashi when slot.EnemyDefinitionPointer == EnemyDefinitionId.Hibashi:
                RunHibashiMain(slot, RequireHibashiState(slot));
                return;
            case EnemyAiRoutine.MainAI_Puromi when slot.EnemyDefinitionPointer == EnemyDefinitionId.Puromi:
                RunNuclearWaffleMain(slot, RequireNuclearWaffleState(slot));
                return;
            case EnemyAiRoutine.MainAI_MiniKraid when slot.EnemyDefinitionPointer == EnemyDefinitionId.MiniKraid:
                RunFakeKraidMain(
                    slot,
                    RequireFakeKraidState(slot),
                    cameraX,
                    cameraY);
                return;
            case EnemyAiRoutine.MainAI_PirateWalking when IsWalkingSpacePirateDefinition(slot.EnemyDefinitionPointer):
                RunWalkingSpacePirateMain(
                    slot,
                    RequireWalkingSpacePirateState(slot),
                    samus,
                    level,
                    samusProjectiles);
                return;
            case EnemyAiRoutine.MainAI_PirateWall when IsWallSpacePirateDefinition(slot.EnemyDefinitionPointer):
                RunWallSpacePirateMain(
                    slot,
                    RequireWallSpacePirateState(slot),
                    samus);
                return;
            case EnemyAiRoutine.MainAI_PirateNinja when IsNinjaSpacePirateDefinition(slot.EnemyDefinitionPointer):
                RunNinjaSpacePirateMain(
                    slot,
                    RequireNinjaSpacePirateState(slot),
                    samus,
                    level,
                    samusProjectiles);
                return;
            case EnemyAiRoutine.MainAI_BombTorizo when slot.EnemyDefinitionPointer == EnemyDefinitionId.BombTorizo:
                RunBombTorizoMain(slot, RequireBombTorizoState(slot), samus, level);
                return;
            case EnemyAiRoutine.MainAI_GoldenTorizo when slot.EnemyDefinitionPointer == EnemyDefinitionId.GoldenTorizo:
                RunGoldenTorizoMain(
                    slot,
                    RequireBombTorizoState(slot),
                    samus,
                    level);
                return;
            case EnemyAiRoutine.MainAI_Kraid when slot.EnemyDefinitionPointer == EnemyDefinitionId.Kraid:
                RunKraidBodyMain(slot, samus, cameraX, cameraY, vramWriteQueue, samusProjectiles, sharedProjectiles);
                return;
            case EnemyAiRoutine.MainAI_KraidArm when slot.EnemyDefinitionPointer == EnemyDefinitionId.KraidArm:
                RunKraidArmMain(slot, cameraY);
                return;
            case EnemyAiRoutine.MainAI_KraidLintTop when slot.EnemyDefinitionPointer == EnemyDefinitionId.KraidLintTop:
            case EnemyAiRoutine.MainAI_KraidLintMiddle when slot.EnemyDefinitionPointer == EnemyDefinitionId.KraidLintMiddle:
            case EnemyAiRoutine.MainAI_KraidLintBottom when slot.EnemyDefinitionPointer == EnemyDefinitionId.KraidLintBottom:
                RunKraidLintMain(slot, samus);
                return;
            case EnemyAiRoutine.MainAI_KraidFoot when slot.EnemyDefinitionPointer == EnemyDefinitionId.KraidFoot:
                RunKraidFootMain(slot, cameraY);
                return;
            case EnemyAiRoutine.MainAI_KraidNail when slot.EnemyDefinitionPointer == EnemyDefinitionId.KraidNail:
            case EnemyAiRoutine.MainAI_KraidNailBad when slot.EnemyDefinitionPointer == EnemyDefinitionId.KraidNailBad:
                RunKraidNailMain(slot, level);
                return;
            case EnemyAiRoutine.MainAI_Phantoon when slot.EnemyDefinitionPointer == EnemyDefinitionId.PhantoonBody:
                RunPhantoonMain(slot, samus, cameraX, cameraY);
                return;
            case EnemyAiRoutine.RTL_A7E011
                when slot.EnemyDefinitionPointer is
                EnemyDefinitionId.PhantoonTentacles or EnemyDefinitionId.PhantoonMouth:
                // $A7:E011 is a literal RTL. These drawing parts animate entirely through
                // their independent bank-$A7 instruction lists after the shared body main.
                return;
            case EnemyAiRoutine.MainAI_TourianStatue when slot.EnemyDefinitionPointer == EnemyDefinitionId.TourianStatue:
                // $AA:D7C7 is the one-byte RTL immediately before the initializer. The
                // three enemy records animate exclusively through their ROM lists.
                return;
            case EnemyAiRoutine.MainAI_HurtAI_Shaktool when slot.EnemyDefinitionPointer == EnemyDefinitionId.Shaktool:
                RunShaktoolMain(slot, RequireShaktoolState(slot), level);
                return;
            case EnemyAiRoutine.MainAI_Chozo when slot.EnemyDefinitionPointer == EnemyDefinitionId.Chozo:
                RunChozoStatueMain(slot, RequireChozoStatueState(slot));
                return;
            default:
                throw new InvalidDataException(
                    $"Enemy ${(int)slot.EnemyDefinitionPointer:X4} main AI ${address:X6} is not translated.");
        }
    }

    private void RunGunshipTopMain(
        RoomEnemySlot top,
        SamusState? samus,
        ushort newlyPressedControllerInput,
        VramWriteQueue? vramWriteQueue)
    {
        if (top.SlotIndex + 2 >= EnemyCount)
            throw new InvalidDataException("Gunship top is missing its two following component slots.");

        RoomEnemySlot bottom = _slots[top.SlotIndex + 1];

        // $A2:A75C decrements the solid bottom's engine-sound timer and reloads 70 on
        // one/underflow. QueueSfx2_Max6($4D) occurs before the reload, exactly as it does in
        // GunshipTop_Main; this periodic producer continues while the ship is idling/bobbing.
        ushort oldBottomTimer = bottom.VariableD;
        bottom.VariableD = unchecked((ushort)(bottom.VariableD - 1));
        if (oldBottomTimer == 1 || (short)bottom.VariableD < 0)
        {
            QueueEnemySound(SoundEffectId.FromCartridge(SoundEffectLibrary.Library2, 0x004d), maximumQueued: 6);
            bottom.VariableD = 70;
        }

        // The native address-range test admits functions $A942-$AC1A. Idle function $A9BD
        // is inside that interval, so the landed ship continuously performs its four-phase
        // bob before dispatching the function itself.
        if (!IsNegative16(top.VariableF + 0x56be) && IsNegative16(top.VariableF + 0x53e5))
            StepGunshipBob(top);

        GunshipFunction function = ClosedNativeWords.Decode<GunshipFunction>(top.VariableF, "gunship function");
        switch (function)
        {
            case GunshipFunction.DescendAfterCeres:
                DescendPostCeresGunship(top, samus);
                return;
            case GunshipFunction.ApplyLandingBrakes:
                BouncePostCeresGunship(top, samus);
                return;
            case GunshipFunction.WaitForLandingEntranceToOpen:
                if (TickGunshipFunctionTimer(top))
                    top.VariableF = (ushort)GunshipFunction.EjectSamus;
                return;
            case GunshipFunction.EjectSamus:
                RaisePostCeresSamus(top, samus);
                return;
            case GunshipFunction.FinishLanding:
                if (TickGunshipFunctionTimer(top))
                {
                    top.VariableF = (ushort)GunshipFunction.Idle;
                    if (samus is not null)
                        samus.InputLocked = false;
                    LastGunshipEvent = GunshipFrameEvent.LandingCompleted;
                }
                return;
            case GunshipFunction.Idle:
                HandleIdleGunshipEntrance(top, samus, newlyPressedControllerInput);
                return;
            case GunshipFunction.WaitForEntranceToOpen:
                if (TickGunshipFunctionTimer(top))
                    top.VariableF = (ushort)GunshipFunction.LowerSamus;
                return;
            case GunshipFunction.LowerSamus:
                LowerSamusIntoGunship(top, samus);
                return;
            case GunshipFunction.WaitForEntranceToClose:
                if (TickGunshipFunctionTimer(top))
                    top.VariableF = (ushort)GunshipFunction.BeginLiftoffOrRestoreSamus;
                return;
            case GunshipFunction.BeginLiftoffOrRestoreSamus:
                RestoreSamusInGunship(top, samus);
                return;
            case GunshipFunction.HandleSaveConfirmation:
                GunshipSavePromptPending = true;
                LastGunshipEvent = GunshipFrameEvent.SavePromptRequested;
                return;
            case GunshipFunction.WaitForExitPadToOpen:
                if (TickGunshipFunctionTimer(top))
                    top.VariableF = (ushort)GunshipFunction.RaiseSamus;
                return;
            case GunshipFunction.RaiseSamus:
                RaiseSamusOutOfGunship(top, samus);
                return;
            case GunshipFunction.FinishSamusExit:
                if (TickGunshipFunctionTimer(top))
                {
                    top.VariableF = (ushort)GunshipFunction.Idle;
                    if (samus is not null)
                        samus.InputLocked = false;
                    LastGunshipEvent = GunshipFrameEvent.ExitCompleted;
                }
                return;
            case GunshipFunction.LoadLiftoffDustTiles:
                QueueGunshipTakeoffTiles(top, vramWriteQueue);
                return;
            case GunshipFunction.FireUpEngines:
                FireUpGunshipEngines(top, samus);
                return;
            case GunshipFunction.SteadyLiftoff:
                LiftGunshipAtConstantSpeed(top, samus);
                return;
            case GunshipFunction.AcceleratingLiftoff:
                AccelerateEscapingGunship(top, samus);
                return;
            case GunshipFunction.MoveAccelerating:
                MoveEscapingGunship(top, samus);
                return;
            case GunshipFunction.NoOperation:
                throw new InvalidDataException(
                    $"Gunship function $A2:{top.VariableF:X4} is not translated.");
            default:
                throw new InvalidOperationException($"Undefined GunshipFunction {function}.");
        }
    }

    private void DescendPostCeresGunship(RoomEnemySlot top, SamusState? samus)
    {
        if (samus is null)
            throw new InvalidOperationException("Post-Ceres gunship descent lost Samus.");

        // Function three carries all four actors as a rigid body. Above Y=$0300 it moves
        // $4.8000 pixels per call; below that threshold it slows to $2.8000 and clamps the
        // top hull to $045F before beginning the cartridge's seventeen-entry bounce table.
        // The clamp ($A2:A8B2) writes only whole positions; subpixels keep this call's sum.
        uint delta = top.YPosition < 0x0300 ? 0x0004_8000u : 0x0002_8000u;
        AddGunshipYFixed(samus, top, delta);
        if (top.YPosition < 0x045f)
            return;

        RoomEnemySlot bottom = _slots[top.SlotIndex + 1];
        RoomEnemySlot pad = _slots[top.SlotIndex + 2];
        top.YPosition = 0x045f;
        bottom.YPosition = 0x0487;
        pad.YPosition = 0x045e;
        top.VariableF = (ushort)GunshipFunction.ApplyLandingBrakes;
        top.VariableE = 0;
    }

    private void BouncePostCeresGunship(RoomEnemySlot top, SamusState? samus)
    {
        if (samus is null)
            throw new InvalidOperationException("Post-Ceres gunship bounce lost Samus.");

        short yDelta = GunshipMotionDefinitions.LandingBrakeYDelta(top.VariableE);
        samus.YPosition = unchecked((ushort)(samus.YPosition + yDelta));
        for (int component = 0; component < 3; component++)
        {
            RoomEnemySlot slot = _slots[top.SlotIndex + component];
            slot.YPosition = unchecked((ushort)(slot.YPosition + yDelta));
        }

        top.VariableE++;
        if (top.VariableE < 17)
            return;

        RoomEnemySlot pad = _slots[top.SlotIndex + 2];
        top.VariableF = (ushort)GunshipFunction.WaitForLandingEntranceToOpen;
        top.VariableE = top.YPosition;
        top.VariableD = 1;
        top.VariableC = 0;
        samus.XPosition = unchecked((ushort)(top.XPosition + 1));
        samus.WritePreviousXPosition(samus.XPosition);
        pad.InstructionTimer = 1;
        pad.CurrentInstruction = GunshipInstructionProgramDefinitions.EntrancePadOpening;
        top.VariableA = 144;
        LastGunshipEvent = GunshipFrameEvent.LandingPadOpened;
        QueueEnemySound(SoundEffectLibrary3Sounds.GunshipEntrancePad, maximumQueued: 6);
    }

    private void RaisePostCeresSamus(RoomEnemySlot top, SamusState? samus)
    {
        if (samus is null)
            throw new InvalidOperationException("Post-Ceres gunship exit lost Samus.");

        samus.YPosition = unchecked((ushort)(samus.YPosition - 1));
        ushort targetY = unchecked((ushort)(top.VariableE - 30));
        if (!IsNegative16(samus.YPosition - targetY))
            return;

        RoomEnemySlot pad = _slots[top.SlotIndex + 2];
        top.VariableF = (ushort)GunshipFunction.FinishLanding;
        pad.InstructionTimer = 1;
        pad.CurrentInstruction = GunshipInstructionProgramDefinitions.EntrancePadClosing;
        top.VariableA = 144;
        LastGunshipEvent = GunshipFrameEvent.LandingPadClosed;
    }

    private void AddGunshipYFixed(SamusState samus, RoomEnemySlot top, uint delta)
    {
        uint samusFixed = ((uint)samus.YPosition << 16) | samus.Kinematics.YSubposition;
        samusFixed = unchecked(samusFixed + delta);
        samus.YPosition = unchecked((ushort)(samusFixed >> 16));
        samus.Kinematics.YSubposition = unchecked((ushort)samusFixed);

        for (int component = 0; component < 3; component++)
        {
            RoomEnemySlot slot = _slots[top.SlotIndex + component];
            uint fixedPosition = ((uint)slot.YPosition << 16) | slot.YSubposition;
            fixedPosition = unchecked(fixedPosition + delta);
            slot.YPosition = unchecked((ushort)(fixedPosition >> 16));
            slot.YSubposition = unchecked((ushort)fixedPosition);
        }
    }

    private void StepGunshipBob(RoomEnemySlot top)
    {
        ushort oldTimer = top.VariableD;
        top.VariableD = unchecked((ushort)(top.VariableD - 1));
        if (oldTimer != 1 && (short)top.VariableD >= 0)
            return;

        GunshipIdleBobDefinition bob = GunshipMotionDefinitions.IdleBob(
            unchecked((ushort)(top.VariableC & 3)));
        top.VariableD = bob.Timer;
        sbyte yDelta = bob.YDelta;
        for (int component = 0; component < 3; component++)
        {
            RoomEnemySlot slot = _slots[top.SlotIndex + component];
            slot.YPosition = unchecked((ushort)(slot.YPosition + yDelta));
        }
        top.VariableC = unchecked((ushort)((top.VariableC + 1) & 3));
    }

    private void HandleIdleGunshipEntrance(
        RoomEnemySlot top,
        SamusState? samus,
        ushort newlyPressedControllerInput)
    {
        SnesButton newlyPressed = SnesButtons.FromRaw(
            newlyPressedControllerInput,
            "gunship entrance AI");
        if (samus is null || !newlyPressed.HasAny(SnesButton.Down))
            return;

        bool insideEntrance =
            (short)unchecked((ushort)(top.XPosition - 8 - samus.XPosition)) < 0 &&
            (short)unchecked((ushort)(top.XPosition + 8 - samus.XPosition)) >= 0 &&
            (short)unchecked((ushort)(top.YPosition - 64 - samus.YPosition)) < 0 &&
            (short)unchecked((ushort)(top.YPosition - samus.YPosition)) >= 0;
        if (insideEntrance && samus.ReadMovementKind(_bus!) == SamusMovementType.Standing)
        {
            RoomEnemySlot pad = _slots[top.SlotIndex + 2];
            top.VariableF = (ushort)GunshipFunction.WaitForEntranceToOpen;
            // $A2:AA17-AA1D stores the ship's X in both Samus X words, so the camera sees
            // no horizontal motion from this alignment.
            if (samus.XPosition != 0x0480)
            {
                samus.XPosition = top.XPosition;
                samus.WritePreviousXPosition(samus.XPosition);
            }
            samus.ApplyForwardFacingPoseSetup(_bus!);
            samus.InputLocked = true;
            samus.PrimeGraphics(_bus!);
            pad.YPosition = unchecked((ushort)(top.YPosition - 1));
            pad.InstructionTimer = 1;
            pad.CurrentInstruction = GunshipInstructionProgramDefinitions.EntrancePadOpening;
            top.VariableA = 144;
            LastGunshipEvent = GunshipFrameEvent.EntryStarted;
            QueueEnemySound(SoundEffectLibrary3Sounds.GunshipEntrancePad, maximumQueued: 6);
        }
    }

    private static bool TickGunshipFunctionTimer(RoomEnemySlot top)
    {
        ushort oldTimer = top.VariableA;
        top.VariableA = unchecked((ushort)(top.VariableA - 1));
        return oldTimer == 1 || (short)top.VariableA < 0;
    }

    private void LowerSamusIntoGunship(RoomEnemySlot top, SamusState? samus)
    {
        if (samus is null)
            throw new InvalidOperationException("Gunship entry lost its Samus actor.");
        samus.YPosition = unchecked((ushort)(samus.YPosition + 2));
        if (IsNegative16(samus.YPosition - unchecked((ushort)(top.VariableE + 18))))
            return;

        RoomEnemySlot pad = _slots[top.SlotIndex + 2];
        top.VariableF = (ushort)GunshipFunction.WaitForEntranceToClose;
        pad.InstructionTimer = 1;
        pad.CurrentInstruction = GunshipInstructionProgramDefinitions.EntrancePadClosing;
        top.VariableA = 144;
        LastGunshipEvent = GunshipFrameEvent.EntryPadClosing;
    }

    private void RestoreSamusInGunship(RoomEnemySlot top, SamusState? samus)
    {
        if (samus is null)
            throw new InvalidOperationException("Gunship restoration lost its Samus actor.");

        // Event $0E is set by Mother Brain's death sequence. Native bypasses every refill
        // and save-prompt branch here, installs the takeoff tile uploader, and keeps the
        // already hidden/input-locked Samus rigidly attached to the ship.
        if (RequireEvent(EventNumber.ZebesTimebombSet))
        {
            RoomEnemySlot bottom = _slots[top.SlotIndex + 1];
            top.VariableF = (ushort)GunshipFunction.LoadLiftoffDustTiles;
            top.VariableB = 0;
            bottom.VariableF = 0;
            bottom.VariableE = 0;
            LastGunshipEvent = GunshipFrameEvent.EscapeTakeoffStarted;
            return;
        }

        SamusEnergyRestoration.Restore(samus, 2);
        samus.Missiles = RestoreTwo(samus.Missiles, samus.MaxMissiles);
        samus.SuperMissiles = RestoreTwo(samus.SuperMissiles, samus.MaxSuperMissiles);
        samus.PowerBombs = RestoreTwo(samus.PowerBombs, samus.MaxPowerBombs);
        if ((short)(samus.ReserveEnergy - samus.MaxReserveEnergy) < 0 ||
            (short)(samus.Health - samus.MaxHealth) < 0 ||
            (short)(samus.Missiles - samus.MaxMissiles) < 0 ||
            (short)(samus.SuperMissiles - samus.MaxSuperMissiles) < 0 ||
            (short)(samus.PowerBombs - samus.MaxPowerBombs) < 0)
            return;

        // The prompt itself opens when $AB1F executes on the following frame.
        top.VariableF = (ushort)GunshipFunction.HandleSaveConfirmation;
    }

    private void RaiseSamusOutOfGunship(RoomEnemySlot top, SamusState? samus)
    {
        if (samus is null)
            throw new InvalidOperationException("Gunship exit lost its Samus actor.");
        samus.YPosition = unchecked((ushort)(samus.YPosition - 2));
        if (!IsNegative16(samus.YPosition - unchecked((ushort)(top.VariableE - 30))))
            return;

        RoomEnemySlot pad = _slots[top.SlotIndex + 2];
        top.VariableF = (ushort)GunshipFunction.FinishSamusExit;
        pad.InstructionTimer = 1;
        pad.CurrentInstruction = GunshipInstructionProgramDefinitions.EntrancePadClosing;
        top.VariableA = 144;
        LastGunshipEvent = GunshipFrameEvent.ExitPadClosing;
        QueueEnemySound(SoundEffectLibrary3Sounds.GunshipEntrancePadClosing, maximumQueued: 6);
    }

    /// <summary>Ports gunship function 17 at <c>$A2:ABC7</c>.</summary>
    private void QueueGunshipTakeoffTiles(
        RoomEnemySlot top,
        VramWriteQueue? vramWriteQueue)
    {
        if (vramWriteQueue is null)
            throw new InvalidOperationException("Gunship takeoff requires the runtime VRAM queue.");

        int transferIndex = top.VariableB;
        if ((uint)transferIndex >= GunshipLiftoffTransferDefinitions.Frames.Length)
            throw new InvalidDataException("Gunship takeoff tile index escaped its five-entry table.");
        GunshipLiftoffTransferDefinition transfer =
            GunshipLiftoffTransferDefinitions.Frames[transferIndex];
        if (TileArtwork?.GunshipLiftoff is not null)
            vramWriteQueue.EnqueueAsset(transfer.Asset,
                GunshipLiftoffTransferDefinitions.ByteCount, transfer.DestinationWord);
        else
            vramWriteQueue.Enqueue(GunshipLiftoffTransferDefinitions.ByteCount,
                transfer.SourceAddress, transfer.DestinationWord);

        top.VariableB++;
        if (top.VariableB >= GunshipLiftoffTransferDefinitions.Frames.Length)
        {
            top.VariableF = (ushort)GunshipFunction.FireUpEngines;
            top.VariableB = 0;
        }
    }

    /// <summary>Ports the 128-frame engine-rumble function at <c>$A2:AC1B</c>.</summary>
    private void FireUpGunshipEngines(RoomEnemySlot top, SamusState? samus)
    {
        if (samus is null)
            throw new InvalidOperationException("Gunship takeoff lost Samus.");
        RoomEnemySlot bottom = _slots[top.SlotIndex + 1];
        RoomEnemySlot pad = _slots[top.SlotIndex + 2];
        ushort rumbleFrame = bottom.VariableE;
        int shake = (rumbleFrame & 1) != 0
            ? (rumbleFrame < 64 ? 1 : 2)
            : (rumbleFrame < 64 ? -1 : -2);
        samus.YPosition = unchecked((ushort)(samus.YPosition + shake));
        top.YPosition = unchecked((ushort)(samus.YPosition - 17));
        pad.YPosition = unchecked((ushort)(top.YPosition - 1));
        bottom.YPosition = unchecked((ushort)(samus.YPosition + 23));

        bottom.VariableE++;
        if (bottom.VariableE == 64)
        {
            // The six calls at `$A2:AC9B-$ACCA` happen after the frame counter reaches
            // exactly 64. Descending projectile allocation means parameter A occupies the
            // highest surviving slot, matching the ordinary bank-$86 scheduler.
            for (ushort parameter = 0; parameter <= 0x000a; parameter += 2)
                SpawnGunshipLiftoffDustCloud(parameter, samus);
        }
        if (bottom.VariableE >= 128)
        {
            top.VariableF = (ushort)GunshipFunction.SteadyLiftoff;
            top.VariableA = 0;
        }
    }

    /// <summary>Ports constant two-pixel liftoff at <c>$A2:ACD7</c>.</summary>
    private void LiftGunshipAtConstantSpeed(RoomEnemySlot top, SamusState? samus)
    {
        if (samus is null)
            throw new InvalidOperationException("Gunship liftoff lost Samus.");
        MoveGunshipAndSamusToY(top, samus, unchecked((ushort)(samus.YPosition - 2)));
        if (IsNegative16(top.YPosition - 896))
        {
            top.VariableF = (ushort)GunshipFunction.AcceleratingLiftoff;
            _slots[top.SlotIndex + 1].VariableF = 0x0200;
        }
    }

    /// <summary>Ports the accelerating takeoff function at <c>$A2:AD0E</c>.</summary>
    private void AccelerateEscapingGunship(RoomEnemySlot top, SamusState? samus)
    {
        MoveEscapingGunship(top, samus);
        if (IsNegative16(top.YPosition - 256))
        {
            top.VariableF = (ushort)GunshipFunction.MoveAccelerating;
            LastGunshipEvent = GunshipFrameEvent.EscapeTakeoffCompleted;
        }
    }

    /// <summary>Ports the shared 8.8 velocity integrator at <c>$A2:AD2D</c>.</summary>
    private void MoveEscapingGunship(RoomEnemySlot top, SamusState? samus)
    {
        if (samus is null)
            throw new InvalidOperationException("Escaping gunship lost Samus.");
        RoomEnemySlot bottom = _slots[top.SlotIndex + 1];
        bottom.VariableF = unchecked((ushort)(bottom.VariableF + 0x0040));
        if ((bottom.VariableF & 0xff00) >= 0x0a00)
            bottom.VariableF = 0x0900;

        uint samusY = samus.Kinematics.YFixed;
        samusY = unchecked(samusY - ((uint)bottom.VariableF << 8));
        samus.Kinematics.SetYFixed(samusY);
        MoveGunshipAndSamusToY(top, samus, samus.YPosition);
    }

    private void MoveGunshipAndSamusToY(
        RoomEnemySlot top,
        SamusState samus,
        ushort samusY)
    {
        samus.YPosition = samusY;
        RoomEnemySlot bottom = _slots[top.SlotIndex + 1];
        RoomEnemySlot pad = _slots[top.SlotIndex + 2];
        top.YPosition = unchecked((ushort)(samusY - 17));
        pad.YPosition = unchecked((ushort)(top.YPosition - 1));
        bottom.YPosition = unchecked((ushort)(samusY + 23));
    }

    private static ushort RestoreTwo(ushort current, ushort maximum)
    {
        if ((short)(current - maximum) >= 0)
            return current;
        return unchecked((ushort)Math.Min(current + 2, maximum));
    }

    private void DrawEnemySpritemap(OamBuffer oam, byte bank, ushort pointer,
        ushort originX, ushort originY, ushort paletteBits, ushort baseTileIndex,
        bool clipVerticalWrap = false, bool originYIsOnScreen = true)
    {
        if (TileArtwork?.Spritemaps?.TryGetDisplay(bank, pointer,
                out EnemySpritemapParts installed) == true)
        {
            oam.AddEnemySpritemap(installed, originX, originY,
                paletteBits, baseTileIndex, clipVerticalWrap, originYIsOnScreen);
            return;
        }
        // The common empty frame is compiled definition data, not missing artwork.
        if (CommonEnemyEmptyExtendedFrameDefinitions.HasEmptySpritemap(bank, pointer))
            return;
        throw new InvalidDataException(
            $"Enemy sprite ${bank:X2}:{pointer:X4} requires an installed display composition.");
    }

    private static ushort ReadEnemyVisualSelector(RoomEnemySlot slot, ushort operandAddress)
    {
        if (slot.EnemyDefinitionPointer is EnemyDefinitionId.MotherBrainBody or EnemyDefinitionId.MotherBrainHead &&
            operandAddress == MotherBrainBodyInstructionProgramDefinitions.InitialDummyVisualOperand)
            return MotherBrainBodyInstructionProgramDefinitions.ReadInitialDummyVisualSelector(
                operandAddress);
        if (slot.EnemyDefinitionPointer == EnemyDefinitionId.MotherBrainBody &&
            MotherBrainHandBeamBodyInstructionDefinitions.ContainsWord(operandAddress))
        {
            return MotherBrainHandBeamBodyInstructionDefinitions.ReadVisualSelector(
                operandAddress);
        }
        if (slot.EnemyDefinitionPointer == EnemyDefinitionId.MotherBrainBody)
            return MotherBrainBodyInstructionProgramDefinitions.ReadVisualSelector(operandAddress);
        if (slot.EnemyDefinitionPointer == EnemyDefinitionId.MotherBrainTubes)
            return MotherBrainFallingTubeInstructionDefinitions.ReadVisualSelector(
                operandAddress);
        if (slot.EnemyDefinitionPointer == EnemyDefinitionId.MotherBrainHead &&
            MotherBrainHeadInstructionProgramDefinitions.ContainsWord(operandAddress))
        {
            return MotherBrainHeadInstructionProgramDefinitions.ReadWord(operandAddress);
        }
        if (EnemySpritemapDefinitions.TryFrameAt(
                slot.EnemyDefinitionPointer, operandAddress, out ushort frame))
            return frame;
        if (CompiledEnemyVisualSelectors.TryGet(slot.Definition.Bank,
                operandAddress, out ushort selected))
            return selected;
        throw new InvalidDataException(
            $"Enemy ${(int)slot.EnemyDefinitionPointer:X4} has no compiled visual selector " +
            $"${slot.Definition.Bank:X2}:${operandAddress:X4}.");
    }

    private void ProcessInstructions(
        RoomEnemySlot slot,
        SamusState? samus,
        RoomLevelData? level,
        ushort cameraX,
        ushort cameraY,
        ushort controllerInput)
    {
        ushort oldTimer = slot.InstructionTimer;
        slot.InstructionTimer = unchecked((ushort)(slot.InstructionTimer - 1));
        if (oldTimer != 1)
        {
            slot.ExtraProperties = slot.ExtraProperties.Without(EnemyExtraProperties.NewInstructionFrame);
            return;
        }

        ushort cursor = slot.CurrentInstruction;
        for (int commandCount = 0; commandCount < 64; commandCount++)
        {
            ushort word = ReadEnemyInstructionMechanicsWord(slot, cursor);
            if ((word & 0x8000) == 0)
            {
                slot.InstructionTimer = word;
                slot.SpritemapPointer = ReadEnemyVisualSelector(slot,
                    unchecked((ushort)(cursor + 2)));
                slot.CurrentInstruction = unchecked((ushort)(cursor + 4));
                slot.ExtraProperties = slot.ExtraProperties.With(EnemyExtraProperties.NewInstructionFrame);
                return;
            }

            // Common bank-$A0 commands behave identically for every owner and no owner's
            // private opcode shares their offsets, so they are decoded first.
            if (Enum.IsDefined((CommonEnemyInstruction)word))
            {
                ProcessCommonEnemyInstruction(
                    slot,
                    (CommonEnemyInstruction)word,
                    ref cursor,
                    out bool yieldAfterCommon);
                if (yieldAfterCommon)
                    return;
                continue;
            }

            if (!TryProcessOwnedEnemyInstruction(
                    slot,
                    samus,
                    level,
                    word,
                    ref cursor,
                    cameraX,
                    cameraY,
                    controllerInput,
                    out bool yieldAfterOwned))
            {
                throw new InvalidDataException(
                    $"Enemy ${(int)slot.EnemyDefinitionPointer:X4} instruction " +
                    $"${slot.Definition.Bank:X2}:{cursor:X4} opcode ${word:X4} is not translated.");
            }
            if (yieldAfterOwned)
                return;
        }

        throw new InvalidDataException(
            $"Enemy ${(int)slot.EnemyDefinitionPointer:X4} instruction list exceeded 64 commands without a frame.");
    }

    /// <summary>
    /// Applies the bank-$A0 common $814B command. Installed Torizo tile art
    /// resolves by its compiled descriptor. Unknown transfers cannot fall back
    /// to an address-based cartridge decoder.
    /// </summary>
    private void ApplyEnemyInstructionVramTransfer(RoomEnemySlot slot,
        ushort instruction)
    {
        if (slot.EnemyDefinitionPointer is not (EnemyDefinitionId.BombTorizo or EnemyDefinitionId.GoldenTorizo) ||
            !TorizoInstructionVramTransferDefinitions.TryGet(
                instruction, out TorizoInstructionVramTransferDefinition transfer))
            throw new InvalidDataException(
                $"Enemy tile transfer ${slot.Definition.Bank:X2}:${instruction:X4} has no compiled descriptor.");
        var installed = TileArtwork?.TorizoInstructionVram ?? throw new InvalidDataException(
            "Torizo instruction-time tile transfers require installed artwork.");
        if (!installed.TryResolve(transfer.SourceAddress, transfer.ByteCount,
                out ReadOnlyMemory<byte> characters))
            throw new InvalidDataException(
                $"Torizo tile transfer $AA:${instruction:X4} has no installed art.");
        _vram!.LoadBytes(transfer.DestinationWord * 2, characters.Span);
    }

    /// <summary>
    /// Resolves simulation-owned enemy instruction words from compiled definitions.
    /// Presentation selectors resolve separately to installed artwork identities.
    /// </summary>
    private static ushort ReadEnemyInstructionMechanicsWord(RoomEnemySlot slot, ushort address)
    {
        if (slot.EnemyDefinitionPointer is EnemyDefinitionId.BombTorizo or EnemyDefinitionId.GoldenTorizo)
            return TorizoInstructionProgramDefinitions.ReadMechanicsWord(address);

        if (slot.EnemyDefinitionPointer is
            EnemyDefinitionId.ShipTop or
            EnemyDefinitionId.ShipBottomEntrance)
        {
            return GunshipInstructionProgramDefinitions.ReadMechanicsWord(address);
        }

        if (slot.EnemyDefinitionPointer ==
            EnemyDefinitionId.BabyMetroidCutscene)
        {
            return MotherBrainBabyInstructionProgramDefinitions.ReadMechanicsWord(address);
        }

        if (slot.EnemyDefinitionPointer == EnemyDefinitionId.Botwoon)
            return BotwoonInstructionProgramDefinitions.ReadMechanicsWord(address);

        if (slot.EnemyDefinitionPointer == EnemyDefinitionId.Shaktool)
            return ShaktoolInstructionProgramDefinitions.ReadMechanicsWord(address);

        if (slot.EnemyDefinitionPointer == EnemyDefinitionId.EtecoonEscape)
            return EscapeEtecoonInstructionProgramDefinitions.ReadMechanicsWord(address);

        if (slot.EnemyDefinitionPointer == EnemyDefinitionId.DachoraEscape)
            return EscapeDachoraInstructionProgramDefinitions.ReadMechanicsWord(address);

        if (slot.EnemyDefinitionPointer is
            EnemyDefinitionId.KraidLintTop or
            EnemyDefinitionId.KraidLintMiddle or
            EnemyDefinitionId.KraidLintBottom)
        {
            return KraidLintInstructionProgramDefinitions.ReadMechanicsWord(address);
        }

        if (slot.EnemyDefinitionPointer == EnemyDefinitionId.CrocomireTongue)
            return CrocomireTongueInstructionProgramDefinitions.ReadMechanicsWord(address);

        if (slot.EnemyDefinitionPointer == EnemyDefinitionId.Crocomire)
            return CrocomireInstructionProgramDefinitions.ReadMechanicsWord(address);

        if (slot.EnemyDefinitionPointer == EnemyDefinitionId.Chozo)
            return ChozoStatueInstructionProgramDefinitions.ReadMechanicsWord(address);

        if (slot.EnemyDefinitionPointer == EnemyDefinitionId.MiniKraid)
            return FakeKraidInstructionProgramDefinitions.ReadMechanicsWord(address);

        if (slot.EnemyDefinitionPointer == EnemyDefinitionId.CeresDoor)
            return CeresDoorInstructionProgramDefinitions.ReadMechanicsWord(address);

        if (slot.EnemyDefinitionPointer == EnemyDefinitionId.Yard)
            return YardInstructionProgramDefinitions.ReadMechanicsWord(address);

        if (IsWalkingSpacePirateDefinition(slot.EnemyDefinitionPointer))
        {
            return WalkingSpacePirateInstructionProgramDefinitions.ReadMechanicsWord(
                address);
        }

        if (IsWallSpacePirateDefinition(slot.EnemyDefinitionPointer))
        {
            return WallSpacePirateInstructionProgramDefinitions.ReadMechanicsWord(
                address);
        }

        if (IsNinjaSpacePirateDefinition(slot.EnemyDefinitionPointer))
        {
            return NinjaSpacePirateInstructionProgramDefinitions.ReadMechanicsWord(
                address);
        }

        if (IsWorkRobotDefinition(slot.EnemyDefinitionPointer))
            return WorkRobotInstructionProgramDefinitions.ReadMechanicsWord(address);

        if (IsPhantoonPartDefinition(slot.EnemyDefinitionPointer))
            return PhantoonInstructionProgramDefinitions.ReadMechanicsWord(address);

        if (slot.EnemyDefinitionPointer == EnemyDefinitionId.KraidArm)
            return KraidArmInstructionProgramDefinitions.ReadMechanicsWord(address);

        if (slot.EnemyDefinitionPointer == EnemyDefinitionId.KraidFoot)
            return KraidFootInstructionProgramDefinitions.ReadMechanicsWord(address);

        if (slot.EnemyDefinitionPointer is
            EnemyDefinitionId.KraidNail or EnemyDefinitionId.KraidNailBad)
        {
            return KraidNailInstructionProgramDefinitions.ReadMechanicsWord(address);
        }

        if (slot.EnemyDefinitionPointer == EnemyDefinitionId.TourianStatue)
        {
            return TourianEntranceStatueInstructionProgramDefinitions.ReadMechanicsWord(
                address);
        }

        if (slot.EnemyDefinitionPointer is
            EnemyDefinitionId.MamaTurtle or
            EnemyDefinitionId.BabyTurtle)
        {
            return MamaTurtleInstructionProgramDefinitions.ReadMechanicsWord(address);
        }

        if (slot.EnemyDefinitionPointer == EnemyDefinitionId.SporeSpawn)
            return SporeSpawnInstructionProgramDefinitions.ReadMechanicsWord(address);

        if (slot.EnemyDefinitionPointer == EnemyDefinitionId.Rinka)
            return RinkaInstructionProgramDefinitions.ReadMechanicsWord(address);

        if (IsFuneNamiheDefinition(slot.EnemyDefinitionPointer))
            return FuneNamiheInstructionProgramDefinitions.ReadMechanicsWord(address);

        if (slot.EnemyDefinitionPointer == EnemyDefinitionId.Atomic)
            return AtomicInstructionProgramDefinitions.ReadMechanicsWord(address);

        if (slot.EnemyDefinitionPointer is EnemyDefinitionId.Sbug or EnemyDefinitionId.Sbug2)
            return SbugInstructionProgramDefinitions.ReadMechanicsWord(address);

        if (slot.EnemyDefinitionPointer == EnemyDefinitionId.Spark)
            return SparkInstructionProgramDefinitions.ReadMechanicsWord(address);

        if (slot.EnemyDefinitionPointer == EnemyDefinitionId.Puromi)
            return NuclearWaffleInstructionProgramDefinitions.ReadMechanicsWord(address);

        if (slot.EnemyDefinitionPointer == EnemyDefinitionId.Hibashi)
            return HibashiInstructionProgramDefinitions.ReadMechanicsWord(address);

        if (slot.EnemyDefinitionPointer == EnemyDefinitionId.FaceBlock)
        {
            return BlueBrinstarFaceBlockInstructionProgramDefinitions.ReadMechanicsWord(
                address);
        }

        if (slot.EnemyDefinitionPointer == EnemyDefinitionId.Boulder)
            return BoulderInstructionProgramDefinitions.ReadMechanicsWord(address);

        if (slot.EnemyDefinitionPointer == EnemyDefinitionId.Boyon)
            return BoyonInstructionProgramDefinitions.ReadMechanicsWord(address);

        if (slot.EnemyDefinitionPointer == EnemyDefinitionId.Skultera)
            return SkulteraInstructionProgramDefinitions.ReadMechanicsWord(address);

        if (slot.EnemyDefinitionPointer == EnemyDefinitionId.Waver)
            return WaverInstructionProgramDefinitions.ReadMechanicsWord(address);

        if (slot.EnemyDefinitionPointer == EnemyDefinitionId.Metaree)
            return SkreeMetareeInstructionProgramDefinitions.ReadMetareeMechanicsWord(address);

        if (slot.EnemyDefinitionPointer == EnemyDefinitionId.Skree)
            return SkreeMetareeInstructionProgramDefinitions.ReadSkreeMechanicsWord(address);

        if (slot.EnemyDefinitionPointer == EnemyDefinitionId.Zoa)
            return ZoaInstructionProgramDefinitions.ReadMechanicsWord(address);

        if (slot.EnemyDefinitionPointer == EnemyDefinitionId.Dragon)
            return DragonInstructionProgramDefinitions.ReadMechanicsWord(address);

        if (IsBrinstarPipeBugDefinition(slot.EnemyDefinitionPointer))
            return BrinstarPipeBugInstructionProgramDefinitions.ReadMechanicsWord(address);

        if (slot.EnemyDefinitionPointer == EnemyDefinitionId.Gamet)
            return NorfairPipeBugInstructionProgramDefinitions.ReadMechanicsWord(address);

        if (slot.EnemyDefinitionPointer == EnemyDefinitionId.Geega)
            return YellowPipeBugInstructionProgramDefinitions.ReadMechanicsWord(address);

        if (slot.EnemyDefinitionPointer == EnemyDefinitionId.Cacatac)
            return CacatacInstructionProgramDefinitions.ReadMechanicsWord(address);

        if (slot.EnemyDefinitionPointer == EnemyDefinitionId.Magdollite)
            return MagdolliteInstructionProgramDefinitions.ReadMechanicsWord(address);

        if (IsKiHunterDefinition(slot.EnemyDefinitionPointer))
            return KiHunterInstructionProgramDefinitions.ReadMechanicsWord(address);

        if (slot.EnemyDefinitionPointer == EnemyDefinitionId.Owtch)
            return OwtchInstructionProgramDefinitions.ReadMechanicsWord(address);

        if (slot.EnemyDefinitionPointer == EnemyDefinitionId.Stoke)
            return StokeInstructionProgramDefinitions.ReadMechanicsWord(address);

        if (slot.EnemyDefinitionPointer is
            EnemyDefinitionId.GRipper or EnemyDefinitionId.Ripper2 or EnemyDefinitionId.Ripper)
        {
            return RipperInstructionProgramDefinitions.ReadMechanicsWord(address);
        }

        if (slot.EnemyDefinitionPointer == EnemyDefinitionId.KzanTop)
            return KzanInstructionProgramDefinitions.ReadMechanicsWord(address);

        if (slot.EnemyDefinitionPointer is
            EnemyDefinitionId.Mellow or EnemyDefinitionId.Mella or EnemyDefinitionId.Menu)
        {
            return FlyInstructionProgramDefinitions.ReadMechanicsWord(address);
        }

        if (slot.EnemyDefinitionPointer == EnemyDefinitionId.Bull)
            return BullInstructionProgramDefinitions.ReadMechanicsWord(address);

        if (slot.EnemyDefinitionPointer == EnemyDefinitionId.Kago)
            return KagoInstructionProgramDefinitions.ReadMechanicsWord(address);

        if (slot.EnemyDefinitionPointer == EnemyDefinitionId.Choot)
            return ChootInstructionProgramDefinitions.ReadMechanicsWord(address);

        if (slot.EnemyDefinitionPointer == EnemyDefinitionId.Squeept)
            return NorfairLavaJumperInstructionProgramDefinitions.ReadMechanicsWord(address);

        if (slot.EnemyDefinitionPointer == EnemyDefinitionId.Beetom)
            return BeetomInstructionProgramDefinitions.ReadMechanicsWord(address);

        if (slot.EnemyDefinitionPointer == EnemyDefinitionId.Alcoon)
            return AlcoonInstructionProgramDefinitions.ReadMechanicsWord(address);

        if (slot.EnemyDefinitionPointer == EnemyDefinitionId.Multiviola)
            return MultiviolaInstructionProgramDefinitions.ReadMechanicsWord(address);

        if (slot.EnemyDefinitionPointer == EnemyDefinitionId.LavaRocks)
            return PolypInstructionProgramDefinitions.ReadMechanicsWord(address);

        if (slot.EnemyDefinitionPointer == EnemyDefinitionId.Powamp)
            return PowampInstructionProgramDefinitions.ReadMechanicsWord(address);

        if (slot.EnemyDefinitionPointer == EnemyDefinitionId.Coven)
        {
            return WreckedShipGhostInstructionProgramDefinitions.ReadMechanicsWord(
                address);
        }

        if (slot.EnemyDefinitionPointer == EnemyDefinitionId.Puyo)
            return PuyoInstructionProgramDefinitions.ReadMechanicsWord(address);

        if (slot.EnemyDefinitionPointer == EnemyDefinitionId.CorpseTorizo)
            return DeadTorizoInstructionProgramDefinitions.ReadMechanicsWord(address);

        if (slot.EnemyDefinitionPointer == EnemyDefinitionId.CorpseSidehopper)
            return DeadSidehopperInstructionProgramDefinitions.ReadMechanicsWord(address);

        if (IsDeadTourianCorpseDefinition(slot.EnemyDefinitionPointer))
            return DeadTourianCorpseInstructionProgramDefinitions.ReadMechanicsWord(address);

        if (slot.EnemyDefinitionPointer == EnemyDefinitionId.BabyMetroid)
            return ShitroidInstructionProgramDefinitions.ReadMechanicsWord(address);

        if (slot.EnemyDefinitionPointer == EnemyDefinitionId.Rio)
            return RioInstructionProgramDefinitions.ReadMechanicsWord(address);

        if (slot.EnemyDefinitionPointer == EnemyDefinitionId.Oum)
            return MaridiaLargeSnailInstructionProgramDefinitions.ReadMechanicsWord(address);

        if (slot.EnemyDefinitionPointer == EnemyDefinitionId.Etecoon)
            return EtecoonInstructionProgramDefinitions.ReadMechanicsWord(address);

        if (slot.EnemyDefinitionPointer == EnemyDefinitionId.Elevator)
            return ElevatorInstructionProgramDefinitions.ReadMechanicsWord(address);

        if (slot.EnemyDefinitionPointer == EnemyDefinitionId.Mochtroid)
            return MochtroidInstructionProgramDefinitions.ReadMechanicsWord(address);

        if (IsPlatformDefinition(slot.EnemyDefinitionPointer))
            return PlatformInstructionProgramDefinitions.ReadMechanicsWord(address);

        if (IsHopperDefinition(slot.EnemyDefinitionPointer))
            return HopperInstructionProgramDefinitions.ReadMechanicsWord(address);

        if (slot.EnemyDefinitionPointer == EnemyDefinitionId.HZoomer)
            return HZoomerInstructionProgramDefinitions.ReadMechanicsWord(address);

        if (slot.EnemyDefinitionPointer == EnemyDefinitionId.Sciser)
            return SciserInstructionProgramDefinitions.ReadMechanicsWord(address);

        if (slot.EnemyDefinitionPointer == EnemyDefinitionId.Zero)
            return ZeroInstructionProgramDefinitions.ReadMechanicsWord(address);

        if (slot.EnemyDefinitionPointer == EnemyDefinitionId.Viola)
            return ViolaInstructionProgramDefinitions.ReadMechanicsWord(address);

        if (slot.EnemyDefinitionPointer is
            EnemyDefinitionId.Zeela or EnemyDefinitionId.Sova or EnemyDefinitionId.Zoomer or EnemyDefinitionId.MZoomer)
        {
            return SharedCrawlerInstructionProgramDefinitions.ReadMechanicsWord(address);
        }

        if (slot.EnemyDefinitionPointer == EnemyDefinitionId.Dachora)
            return DachoraInstructionProgramDefinitions.ReadMechanicsWord(address);

        if (slot.EnemyDefinitionPointer == EnemyDefinitionId.Fireflea)
            return FirefleaInstructionProgramDefinitions.ReadMechanicsWord(address);

        if (slot.EnemyDefinitionPointer == EnemyDefinitionId.Zebetite)
            return ZebetiteInstructionProgramDefinitions.ReadMechanicsWord(address);

        if (slot.EnemyDefinitionPointer is EnemyDefinitionId.Evir or EnemyDefinitionId.EvirProjectile)
            return EvirInstructionProgramDefinitions.ReadMechanicsWord(address);

        if (slot.EnemyDefinitionPointer == EnemyDefinitionId.Eye)
            return MorphBallEyeInstructionProgramDefinitions.ReadMechanicsWord(address);

        if (slot.EnemyDefinitionPointer == EnemyDefinitionId.YappingMaw)
            return YappingMawInstructionProgramDefinitions.ReadMechanicsWord(address);

        if (slot.EnemyDefinitionPointer == EnemyDefinitionId.Metroid)
            return MetroidInstructionProgramDefinitions.ReadMechanicsWord(address);

        if (slot.EnemyDefinitionPointer == EnemyDefinitionId.Geruta)
            return NorfairRioInstructionProgramDefinitions.ReadMechanicsWord(address);

        if (slot.EnemyDefinitionPointer == EnemyDefinitionId.Holtz)
            return LowerNorfairRioInstructionProgramDefinitions.ReadMechanicsWord(address);

        if (slot.EnemyDefinitionPointer == EnemyDefinitionId.ShutterGrowing)
            return GrowingShutterInstructionProgramDefinitions.ReadMechanicsWord(address);

        if (IsVerticalShutterDefinition(slot.EnemyDefinitionPointer))
            return VerticalShutterInstructionProgramDefinitions.ReadMechanicsWord(address);

        if (slot.EnemyDefinitionPointer == EnemyDefinitionId.ShutterHorizShootable)
        {
            return HorizontalShutterInstructionProgramDefinitions.ReadMechanicsWord(address);
        }

        if (slot.EnemyDefinitionPointer == EnemyDefinitionId.Steam)
            return CeresSteamInstructionProgramDefinitions.ReadMechanicsWord(address);

        if (slot.EnemyDefinitionPointer == EnemyDefinitionId.MotherBrainBody)
        {
            if (MotherBrainHandBeamBodyInstructionDefinitions.ContainsWord(address))
                return MotherBrainHandBeamBodyInstructionDefinitions.ReadMechanicsWord(address);
            return MotherBrainBodyInstructionProgramDefinitions.ReadMechanicsWord(address);
        }

        if (slot.EnemyDefinitionPointer == EnemyDefinitionId.MotherBrainTubes)
            return MotherBrainFallingTubeInstructionDefinitions.ReadMechanicsWord(address);

        if (slot.EnemyDefinitionPointer == EnemyDefinitionId.MotherBrainHead &&
            MotherBrainBodyInstructionProgramDefinitions.IsInitialDummyWord(address))
        {
            return MotherBrainBodyInstructionProgramDefinitions.ReadMechanicsWord(address);
        }

        if (slot.EnemyDefinitionPointer == EnemyDefinitionId.MotherBrainHead &&
            MotherBrainHeadInstructionProgramDefinitions.ContainsWord(address))
        {
            return MotherBrainHeadInstructionProgramDefinitions.ReadWord(address);
        }

        if (IsRidleyDefinition(slot.EnemyDefinitionPointer))
            return RidleyInstructionProgramDefinitions.ReadMechanicsWord(address);
        if (slot.EnemyDefinitionPointer == EnemyDefinitionId.RidleyExplosion)
            return RidleyExplosionInstructionProgramDefinitions.ReadMechanicsWord(address);

        if (IsDraygonDefinition(slot.EnemyDefinitionPointer))
            return DraygonInstructionProgramDefinitions.ReadMechanicsWord(address);

        throw new InvalidDataException(
            $"Enemy ${(int)slot.EnemyDefinitionPointer:X4} instruction mechanics pointer " +
            $"${slot.Definition.Bank:X2}:{address:X4} has no compiled owner.");
    }

    private void DetermineWhichEnemiesToProcess(ushort cameraX, ushort cameraY)
    {
        _activeEnemyIndexes.Clear();
        _interactiveEnemyIndexes.Clear();
        foreach (RoomEnemySlot slot in _slots)
        {
            if (slot.EnemyDefinitionPointer is 0 or EnemyDefinitionId.Respawn)
                continue;
            if (slot.Properties.HasAny(EnemyProperties.Deleted))
            {
                slot.EnemyDefinitionPointer = 0;
                continue;
            }

            bool active = _processAllEnemies ||
                slot.Properties.HasAny(EnemyProperties.ProcessOffScreen) ||
                (slot.AiHandlerBits & EnemyAiHandlerMasks.Frozen) != 0 ||
                EnemyIsWithinProcessingWindow(slot, cameraX, cameraY);
            if (!active)
                continue;

            _activeEnemyIndexes.Add(slot.NativeIndex);
            if (!slot.Properties.HasAny(EnemyProperties.IgnoreSamusCollision))
                _interactiveEnemyIndexes.Add(slot.NativeIndex);
        }
    }

    private static bool EnemyIsWithinProcessingWindow(
        RoomEnemySlot slot,
        ushort cameraX,
        ushort cameraY) =>
        !IsNegative16(slot.XRadius + slot.XPosition - cameraX) &&
        !IsNegative16(slot.XRadius + cameraX + 256 - slot.XPosition) &&
        !IsNegative16(slot.YPosition + 8 - cameraY) &&
        !IsNegative16(cameraY + 248 - slot.YPosition);

    /// <summary>
    /// <c>CheckIfEnemyCenterIsOnScreen</c> ($A0:AD70): the center lies within $100 pixels
    /// right of and below the camera on both axes.
    /// </summary>
    private static bool EnemyCenterIsOnScreen(RoomEnemySlot slot, ushort cameraX, ushort cameraY) =>
        !IsNegative16(slot.XPosition - cameraX) &&
        !IsNegative16(cameraX + 0x0100 - slot.XPosition) &&
        !IsNegative16(slot.YPosition - cameraY) &&
        !IsNegative16(cameraY + 0x0100 - slot.YPosition);

    /// <summary>
    /// <c>CheckIfEnemyIsHorizontallyOffScreen</c> ($A0:C18E): a negative X, a right edge
    /// left of the camera, or a position at least $100 right of the camera is off-screen.
    /// </summary>
    private static bool EnemyIsHorizontallyOffScreen(RoomEnemySlot slot, ushort cameraX)
    {
        if (IsNegative16(slot.XPosition))
            return true;
        ushort fromCamera = unchecked((ushort)(slot.XPosition + slot.XRadius - cameraX));
        return IsNegative16(fromCamera) || !IsNegative16(fromCamera - 0x100 - slot.XRadius);
    }

    private static bool EnemyWithNormalSpritesIsOffScreen(
        RoomEnemySlot slot,
        ushort cameraX,
        ushort cameraY) =>
        IsNegative16(slot.XRadius + slot.XPosition - cameraX) ||
        IsNegative16(slot.XRadius + cameraX + 256 - slot.XPosition) ||
        IsNegative16(slot.YPosition + 8 - cameraY) ||
        IsNegative16(cameraY + 248 - slot.YPosition);

    private RoomEnemySlot SlotFromNativeIndex(ushort nativeIndex)
    {
        if ((nativeIndex & (NativeSlotSize - 1)) != 0 || nativeIndex >= MaximumEnemyCount * NativeSlotSize)
            throw new ArgumentOutOfRangeException(nameof(nativeIndex));
        return _slots[nativeIndex / NativeSlotSize];
    }

    private void EnsureLoaded()
    {
        if (_bus is null)
            throw new InvalidOperationException("A room enemy population must be loaded first.");
    }

    /// <summary>Uses compiled retail definitions unless a constructed test bus explicitly supplies fixtures.</summary>
    private static RoomEnemyDefinition ResolveRoomEnemyDefinition(
        ISnesAddressSpace bus, EnemyDefinitionId pointer)
    {
        if (bus is IRoomEnemyFixtureSource fixture)
            return fixture.ReadEnemyDefinition(pointer);
        return RoomEnemyAuxiliaryDefinitionCatalog.TryGet(pointer, out RoomEnemyDefinition auxiliary)
            ? auxiliary
            : RoomEnemyDefinitionCatalog.Get(pointer);
    }

    private static RoomEnemyPopulationDefinition ResolveRoomEnemyPopulation(
        ISnesAddressSpace bus, ushort pointer) =>
        bus is IRoomEnemyFixtureSource fixture
            ? fixture.ReadEnemyPopulation(pointer)
            : RoomEnemyPopulationDefinitions.Get(pointer);

    private static RoomEnemyGraphicsSetDefinition ResolveRoomEnemyGraphicsSet(
        ISnesAddressSpace bus, ushort pointer) =>
        bus is IRoomEnemyFixtureSource fixture
            ? fixture.ReadEnemyGraphicsSet(pointer)
            : RoomEnemyGraphicsSetDefinitions.Get(pointer);

    private static bool IsNegative16(int value) => (short)unchecked((ushort)value) < 0;

    /// <summary>Live enemy staging memory; no cartridge reader can satisfy this dependency.</summary>
    private ISnesMutableMemory EnemyWorkMemory => _bus as ISnesMutableMemory
        ?? throw new InvalidOperationException("Enemy staging requires live WRAM.");
}
