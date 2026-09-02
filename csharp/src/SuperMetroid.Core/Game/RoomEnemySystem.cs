using SuperMetroid.Core.Hardware;
using static SuperMetroid.Core.Hardware.SnesAddressMath;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>
/// ROM-backed room-enemy loader, scheduler, instruction interpreter, and draw queues for
/// the portions of bank $A0 currently exercised by normal Landing Site.
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
    public const int MaximumEnemyCount = 32;
    public const int NativeSlotSize = 0x40;
    public const int MaximumGraphicsSetCount = 4;

    private const int EnemyDefinitionBank = 0xa00000;
    private const int EnemyPopulationBank = 0xa10000;
    private const int EnemyTilesetBank = 0xb40000;
    private const int EnemyVramByteBase = 0xd800;
    private const int OrdinaryEnemyStagingOffset = 0x0800;

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
    private Func<int, bool>? _hasEvent;
    private Action<int>? _setEvent;
    private Action<int>? _clearEvent;
    private Action? _setAreaMiniBossDefeated;
    private Action<ushort>? _setRandomNumber;
    private ushort _randomEnemyCounter;
    private SnesVram? _vram;
    private SnesCgram? _cgram;
    private RidleyEnemyState? _ridleyState;
    private SporeSpawnEnemyState? _sporeSpawn;
    private bool _processAllEnemies;
    private GunshipLoadScenario _gunshipLoadScenario;
    private SamusState? _samusAtEnemyInitialization;

    public RoomEnemySystem()
    {
        for (int slotIndex = 0; slotIndex < _slots.Length; slotIndex++)
            _slots[slotIndex] = new RoomEnemySlot(slotIndex);
    }

    /// <summary>All 32 physical enemy slots, including unused zero-pointer slots.</summary>
    public IReadOnlyList<RoomEnemySlot> Slots => _slots;

    /// <summary>Native $40-byte slot offsets selected by the most recent activity scan.</summary>
    public IReadOnlyList<ushort> ActiveEnemyIndexes => _activeEnemyIndexes;

    /// <summary>Native slot offsets admitted to solid-enemy interaction.</summary>
    public IReadOnlyList<ushort> InteractiveEnemyIndexes => _interactiveEnemyIndexes;

    /// <summary>Collision words read from the current slots after their AI has run.</summary>
    public IReadOnlyList<SolidEnemyCollisionBody> InteractiveCollisionBodies =>
        _interactiveCollisionBodies;

    /// <summary>The room's terminated bank-$B4 graphics-set records.</summary>
    public IReadOnlyList<RoomEnemyGraphicsSetEntry> GraphicsSet => _graphicsSet;

    public ushort PopulationPointer { get; private set; }
    public ushort TilesetPointer { get; private set; }
    public ushort FirstFreeEnemyIndex { get; private set; }
    public ushort EnemyCount { get; private set; }
    public ushort EnemiesKilled { get; private set; }
    public byte DeathQuota { get; private set; }
    public ushort BossId { get; private set; }
    public bool IsLoaded => _bus is not null;
    public GunshipFrameEvent LastGunshipEvent { get; private set; }
    public bool GunshipSavePromptPending { get; private set; }
    public bool GunshipSaveRequested { get; private set; }
    public ushort? LastMochtroidSoundEffect { get; private set; }
    public ushort? LastHopperSoundEffect { get; private set; }
    public ushort? LastYardSoundEffect { get; private set; }
    public ushort? LastMetareeSoundEffect { get; private set; }
    public ushort? LastSkreeSoundEffect { get; private set; }
    public ushort? LastAlcoonSoundEffect { get; private set; }
    public ushort? LastKzanSoundEffect { get; private set; }
    public ushort? LastHibashiSoundEffect { get; private set; }

    /// <summary>
    /// Zero-based index of the bank-$A6 fire-geyser shape command executed this frame.
    /// This mirrors an instruction-dispatch event rather than inventing persistent enemy
    /// state; <see langword="null"/> means no shape command ran during the current frame.
    /// </summary>
    public int? LastHibashiActivityFrameIndex { get; private set; }

    /// <summary>Last library-three sound requested by an attached Beetom this frame.</summary>
    public ushort? LastBeetomSoundEffect { get; private set; }
    public ushort FirefleaDarknessLevel { get; private set; }
    public ushort EarthquakeTimer { get; set; }
    public ushort EarthquakeType { get; set; }

    /// <summary>
    /// Ridley's bank-$A6 state extension while either encounter owns slot zero. The native actor
    /// extends far beyond the common $40-byte enemy record, so exposing a deliberately named
    /// object is both more accurate and considerably easier to inspect than aliasing dozens
    /// of unrelated generic slot words.
    /// </summary>
    public RidleyEnemyState? Ridley => _ridleyState;

    /// <summary>
    /// Compatibility view used by the existing Ceres debugger. It deliberately becomes
    /// null for Lower Norfair Ridley so callers cannot accidentally apply Baby/Mode-7
    /// encounter assumptions to the real boss fight.
    /// </summary>
    public RidleyEnemyState? CeresRidley =>
        _slots[0].EnemyDefinitionPointer == CeresRidleyDefinition ? _ridleyState : null;

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

    /// <summary>Language flag sampled by $A6:C0D9 when the warning-text phase begins.</summary>
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
        Func<int, bool>? hasEvent = null,
        Action<int>? setEvent = null,
        Action<int>? clearEvent = null,
        Func<bool>? isAreaMiniBossDefeated = null,
        Action? setAreaMiniBossDefeated = null,
        Func<bool>? isAreaTorizoDefeated = null,
        Action? setAreaTorizoDefeated = null,
        Func<ushort, bool>? isRoomPlmPresent = null,
        Action<bool>? setSamusControlsEnabled = null,
        Action<int, byte>? setRoomScrollByte = null,
        Action? setAreaBossDefeated = null,
        Action? incrementMotherBrainGlassRoomArgument = null,
        Func<int, byte>? readRoomScrollByte = null,
        Action<ushort>? setMotherBrainLayerBlendingDefaultConfig = null,
        Action<ushort, ushort>? setMotherBrainBg2Scroll = null,
        GunshipLoadScenario gunshipLoadScenario = GunshipLoadScenario.Ordinary)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(vram);
        ArgumentNullException.ThrowIfNull(cgram);
        ArgumentNullException.ThrowIfNull(nextRandom);

        _bus = bus;
        _gunshipLoadScenario = gunshipLoadScenario;
        _samusAtEnemyInitialization = samus;
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
        _readMotherBrainRoomScrollByte = readRoomScrollByte;
        _setMotherBrainRoomScrollByte = setRoomScrollByte;
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
        LastBoulderSoundEffect = null;
        LastZebetiteSoundEffect = null;
        LastEtecoonSoundEffect = null;
        LastDachoraSoundEffect = null;
        LastEvirSoundEffect = null;
        LastMorphBallEyeSoundEffect = null;
        LastYappingMawSoundEffect = null;
        PaletteChangeNumber = 0;
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
        ResetChozoStatueRoomState(setSamusControlsEnabled, setRoomScrollByte);
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
        _ridleyState = null;
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
        if (ReadWord(bus, EnemyPopulationBank | populationPointer) != 0xffff)
            LoadGraphicsSet(bus, tilesetPointer, vram, cgram);
        LoadPopulation(
            bus,
            populationPointer,
            level,
            samus,
            controllerInput,
            cameraX,
            cameraY);
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
    public void QueueGraphicsUploads(VramWriteQueue queue)
    {
        ArgumentNullException.ThrowIfNull(queue);
        EnsureLoaded();
        foreach (RoomEnemyGraphicsSetEntry entry in _graphicsSet)
        {
            int destinationByteOffset = EnemyVramByteBase + entry.StagingOffset;
            if ((destinationByteOffset & 1) != 0 || entry.TileByteCount > ushort.MaxValue)
                throw new InvalidDataException("Enemy tile DMA is not word-aligned or exceeds one native transfer.");
            queue.Enqueue(
                unchecked((ushort)entry.TileByteCount),
                entry.Definition.TileDataAddress,
                unchecked((ushort)(destinationByteOffset / 2)));
        }
    }

    /// <summary>
    /// Rebuilds native active/interactive lists, executes selected enemy AI, advances
    /// instruction lists, and records the layer queues consumed by the later draw phase.
    /// </summary>
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
        VramWriteQueue? vramWriteQueue = null)
    {
        EnsureLoaded();
        _samusForEnemyDrops = samus;
        // Standalone audits do not own the runtime NMI clock. In that case the enemy-frame
        // counter begins at zero and advances at the same end-of-frame point, which gives
        // Mama Turtle's even-frame shell jitter the same initial phase as retail room load.
        byte enemyNmiFrameCounter8 = nmiFrameCounter8 ?? unchecked((byte)_randomEnemyCounter);
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
        LastSpacePirateSoundEffect = null;
        LastEnemyProjectileDudSoundEffect = null;
        if (_ridleyState is not null)
        {
            // These are one-shot publications made by Ridley's current AI call, matching
            // the global QueueMusic/QueueSfx calls in bank $A6. Clear them at the same
            // enemy-frame boundary as every other Last* request above.
            _ridleyState.LastDeathSoundEffect = null;
            _ridleyState.MusicRequest = null;
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
        _motherBrain?.BeginFrame();
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
        // Bank-$88 HDMA objects run before the bank-$A0 enemy dispatcher. This ordering is
        // observable both on spawn (one-frame pending initialization) and shutdown (the
        // full beam sees the body's cleared activation word on the following frame).
        StepMorphBallEyeBeam();
        DetermineWhichEnemiesToProcess(cameraX, cameraY);
        foreach (List<ushort> queue in _drawQueues)
            queue.Clear();

        foreach (ushort nativeIndex in _activeEnemyIndexes)
        {
            RoomEnemySlot slot = SlotFromNativeIndex(nativeIndex);
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
                    slot.EnemyDefinitionPointer == MetroidDefinition)
                {
                    // The native dispatcher selects the lowest set AI bit. Metroid's
                    // custom hurt entry therefore owns the actor before frozen bit four
                    // when both are present, while instruction bytecode still advances.
                    ApplyMetroidHurt(slot);
                    ranActorAi = true;
                }
                if (!ranActorAi &&
                    (slot.AiHandlerBits & 0x0002) != 0 &&
                    slot.EnemyDefinitionPointer is BombTorizoDefinition or GoldenTorizoDefinition)
                {
                    // Torizo_Hurt owns the actor for the selected hurt frame. The common
                    // instruction interpreter still advances afterward, matching $A0:8FF7.
                    ApplyBombTorizoHurt(slot);
                    ranActorAi = true;
                }
                if (!ranActorAi &&
                    (slot.AiHandlerBits & 0x0002) != 0 &&
                    slot.EnemyDefinitionPointer == NorfairRidleyDefinition)
                {
                    // $A6:B297 owns hurt frames instead of falling back to ordinary main
                    // AI. Movement/function work runs only on even actor frames, while tail
                    // projectile armor and hurt palettes remain live on every frame.
                    RunNorfairRidleyHurt(
                        slot,
                        samus,
                        controllerInput,
                        level,
                        samusProjectiles);
                    ranActorAi = true;
                }
                if (!ranActorAi &&
                    (slot.AiHandlerBits & 0x0002) != 0 &&
                    slot.EnemyDefinitionPointer == PhantoonBodyDefinition)
                {
                    // $A7:DD3F owns every Phantoon hurt frame. It alternates palette seven
                    // between white and the new health band; body movement/main AI resumes
                    // only after common flash timing clears handler bit two.
                    ApplyPhantoonHurt(slot, RequireCompletePhantoonState(slot));
                    ranActorAi = true;
                }
                if (!ranActorAi &&
                    (slot.AiHandlerBits & 0x0002) != 0 &&
                    slot.EnemyDefinitionPointer == DraygonBodyDefinition)
                {
                    // `$A5:954D` owns the entire actor during hurt dispatch. Both the BG2
                    // body palette and sprite-palette eye/tail pieces flash together, while
                    // grapple electrocution may subtract another 256 HP every eighth frame.
                    ApplyDraygonHurt(slot, RequireCompleteDraygonState(slot), samus);
                    ranActorAi = true;
                }
                if (!ranActorAi &&
                    (slot.FrozenTimer != 0 || (slot.AiHandlerBits & 0x0004) != 0))
                {
                    if (slot.EnemyDefinitionPointer == RinkaDefinition &&
                        RunRinkaFrozenTail(slot))
                    {
                        // Rinka's termination tail clears/replaces the current actor record.
                        // The active-index array was frozen before AI, so skip the remainder
                        // of this stale entry exactly as the private routine's death call does.
                        continue;
                    }

                    // Common_NormalEnemyFrozenAI owns the actor while its freeze clock is
                    // nonzero. After its decrement, native also thaws immediately when Ice
                    // is no longer equipped; the old host path incorrectly let a synthetic
                    // freeze survive for its full timer after the equipment-menu bit cleared.
                    // Grapple-cancel reactions select this handler with a zero clock; that
                    // call clears bit four without running main AI.
                    slot.FlashTimer = 0;
                    if (slot.FrozenTimer != 0)
                        slot.FrozenTimer = unchecked((ushort)(slot.FrozenTimer - 1));
                    if (samus is not null &&
                        (samus.EquippedBeams & (ushort)SamusBeamFlags.Ice) == 0)
                    {
                        slot.FrozenTimer = 0;
                    }
                    if (slot.FrozenTimer == 0)
                        slot.AiHandlerBits = unchecked((ushort)(slot.AiHandlerBits & ~0x0004));
                    if (slot.EnemyDefinitionPointer == MetroidDefinition)
                        RunMetroidFrozen(slot);
                    if (slot.EnemyDefinitionPointer == YappingMawDefinition)
                        RunYappingMawFrozen(slot, RequireYappingMawState(slot));
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
                            controllerInput,
                            enemyNmiFrameCounter8);
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
            if (!timeIsFrozen && slot.InvincibilityTimer != 0)
                slot.InvincibilityTimer = unchecked((ushort)(slot.InvincibilityTimer - 1));
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
    public void AnswerGunshipSavePrompt(bool save)
    {
        EnsureLoaded();
        if (!GunshipSavePromptPending)
            throw new InvalidOperationException("The gunship save prompt is not awaiting a response.");

        RoomEnemySlot top = _slots[0];
        RoomEnemySlot pad = _slots[2];
        GunshipSavePromptPending = false;
        GunshipSaveRequested = save;
        top.VariableF = 0xab60;
        pad.InstructionTimer = 1;
        pad.CurrentInstruction = 0xa5be;
        top.VariableA = 144;
        LastGunshipEvent = GunshipFrameEvent.SavePromptAnswered;
    }

    /// <summary>
    /// Writes queued enemies for an inclusive layer range using native slot order.
    /// </summary>
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
                    oam.AddEnemySpritemap(
                        _bus!,
                        slot.Definition.Bank,
                        slot.SpritemapPointer,
                        originX,
                        originY,
                        drawPaletteIndex,
                        slot.VramTilesIndex);
                    continue;
                }

                // Extended spritemaps begin with a low-byte component count followed by
                // eight-byte {X,Y,spritemap,hitbox} records. Steam uses one component, but
                // retaining the native list format is necessary for bosses and composite
                // enemies that share this bank-$A0 draw path.
                int extendedAddress = (slot.Definition.Bank << 16) | slot.SpritemapPointer;
                int componentCount = _bus!.ReadByte(extendedAddress);
                ushort componentPointer = unchecked((ushort)(slot.SpritemapPointer + 2));
                for (int component = 0; component < componentCount; component++)
                {
                    int componentAddress = (slot.Definition.Bank << 16) | componentPointer;
                    ushort componentX = unchecked((ushort)(originX + ReadWord(_bus, componentAddress)));
                    ushort componentY = unchecked((ushort)(originY + ReadWord(_bus, AddWithinBank(componentAddress, 2))));
                    ushort ordinarySpritemap = ReadWord(_bus, AddWithinBank(componentAddress, 4));

                    // $FFFE names ProcessExtendedTilemap's command stream. Extra-property
                    // $8000 is the native producer gate: Crocomire clears it while sinking
                    // so the last BG2 image can be erased row-by-row without being restored
                    // by the still-current extended spritemap on every draw pass.
                    ushort componentMarker = ReadWord(
                        _bus,
                        (slot.Definition.Bank << 16) | ordinarySpritemap);
                    if (componentMarker == 0xfffe)
                    {
                        if ((slot.ExtraProperties & 0x8000) != 0)
                            ProcessExtendedEnemyBg2Tilemap(slot.Definition.Bank, ordinarySpritemap);
                    }
                    else if (
                        ((componentX + 128) & 0xfe00) == 0 &&
                        ((componentY + 128) & 0xfe00) == 0)
                    {
                        oam.AddEnemySpritemap(
                            _bus,
                            slot.Definition.Bank,
                            ordinarySpritemap,
                            componentX,
                            componentY,
                            drawPaletteIndex,
                            slot.VramTilesIndex,
                            clipVerticalWrap: true,
                            originYIsOnScreen: (componentY >> 8) == 0);
                    }

                    componentPointer = unchecked((ushort)(componentPointer + 8));
                }
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
        if (IsRidleyDefinition(slot.EnemyDefinitionPointer) && _ridleyState is not null)
            return _ridleyState.CommonDrawPaletteIndex;

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
        int cursor = EnemyTilesetBank | tilesetPointer;
        int nextEnemyTileIndex = 0;
        int nextStagingOffset = OrdinaryEnemyStagingOffset;

        while (true)
        {
            ushort definitionPointer = ReadWord(bus, cursor);
            if (definitionPointer == 0xffff)
                return;
            if (_graphicsSet.Count == MaximumGraphicsSetCount)
            {
                throw new InvalidDataException(
                    $"Enemy graphics set $B4:{tilesetPointer:X4} exceeds the native four-entry arrays.");
            }

            ushort vramDestination = ReadWord(bus, AddWithinBank(cursor, 2));
            RoomEnemyDefinition definition = ReadDefinition(bus, definitionPointer);

            // ProcessEnemyTilesets copies one complete sixteen-color OBJ palette from the
            // enemy's selected code/data bank. Its assembly masks the entire low byte before
            // adding the native eight-row OBJ bias. Retail records use values zero through
            // seven, but retaining the wider mask makes corrupt data fail visibly.
            int destinationColor = ((vramDestination & 0x00ff) + 8) * 16;
            cgram.LoadFromBus(
                bus,
                (definition.Bank << 16) | definition.PalettePointer,
                colorCount: 16,
                destinationIndex: destinationColor);

            int byteCount = definition.TileDataSize & 0x7fff;
            int stagingOffset = (definition.TileDataSize & 0x8000) != 0
                ? (vramDestination & 0x3000) >> 3
                : nextStagingOffset;
            int vramByteOffset = EnemyVramByteBase + stagingOffset;
            if (vramByteOffset < 0 || vramByteOffset + byteCount > SnesVram.ByteCount)
            {
                throw new InvalidDataException(
                    $"Enemy ${definitionPointer:X4} tile DMA would leave VRAM: " +
                    $"offset ${vramByteOffset:X4}, size ${byteCount:X4}.");
            }

            var tileBytes = new byte[byteCount];
            for (int byteIndex = 0; byteIndex < tileBytes.Length; byteIndex++)
                tileBytes[byteIndex] = bus.ReadByte(AddWithinBank(definition.TileDataAddress, byteIndex));
            vram.LoadBytes(vramByteOffset, tileBytes);

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
            cursor = AddWithinBank(cursor, 4);
        }
    }

    private void LoadPopulation(
        ISnesAddressSpace bus,
        ushort populationPointer,
        RoomLevelData? level,
        SamusState? samus,
        ushort controllerInput,
        ushort cameraX,
        ushort cameraY)
    {
        int cursor = EnemyPopulationBank | populationPointer;
        int slotIndex = 0;
        while (true)
        {
            ushort definitionPointer = ReadWord(bus, cursor);
            if (definitionPointer == 0xffff)
            {
                // InitializeEnemies returns early for an initially empty population. It has
                // already zeroed both enemy counters, but it does not rewrite the previous
                // first-free index or death quota. Preserve that retail quirk on reloads.
                if (slotIndex == 0)
                    return;

                DeathQuota = bus.ReadByte(AddWithinBank(cursor, 2));
                EnemyCount = unchecked((ushort)slotIndex);
                FirstFreeEnemyIndex = unchecked((ushort)(slotIndex * NativeSlotSize));
                return;
            }
            if (slotIndex == MaximumEnemyCount)
            {
                throw new InvalidDataException(
                    $"Enemy population $A1:{populationPointer:X4} has more than 32 records.");
            }

            RoomEnemyPopulationRecord population = new(
                definitionPointer,
                ReadWord(bus, AddWithinBank(cursor, 2)),
                ReadWord(bus, AddWithinBank(cursor, 4)),
                ReadWord(bus, AddWithinBank(cursor, 6)),
                ReadWord(bus, AddWithinBank(cursor, 8)),
                ReadWord(bus, AddWithinBank(cursor, 10)),
                ReadWord(bus, AddWithinBank(cursor, 12)),
                ReadWord(bus, AddWithinBank(cursor, 14)));
            RoomEnemyDefinition definition = ReadDefinition(bus, definitionPointer);
            RoomEnemySlot slot = _slots[slotIndex];
            InitializeSlotFromDefinition(slot, population, definition);
            if (definition.BossId != 0)
                BossId = definition.BossId;
            RunInitializationAi(slot, level, samus, controllerInput, cameraX, cameraY);

            // InitializeEnemies deliberately clears the init routine's immediate map.
            // Disable-Samus-collision actors receive the canonical empty map until their
            // first instruction-list tick replaces it. Gunship properties contain $2000.
            slot.SpritemapPointer = slot.Properties.HasAny(EnemyProperties.ProcessInstructions)
                ? slot.ExtraProperties.HasAny(EnemyExtraProperties.UsesExtendedSpritemap)
                    ? (ushort)0x804f
                    : (ushort)0x804d
                : (ushort)0;

            slotIndex++;
            cursor = AddWithinBank(cursor, 16);
        }
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
            definition.XRadius,
            definition.YRadius,
            definition.Health,
            definition.Layer,
            tileIndex,
            paletteIndex,
            ReadSpawnNameWords(definition));
    }

    private (ushort TileIndex, ushort PaletteIndex) FindGraphicsIndexes(ushort definitionPointer)
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
        ushort cameraX = 0,
        ushort cameraY = 0)
    {
        int address = (slot.Definition.Bank << 16) | slot.Definition.InitializationAiPointer;
        switch (address)
        {
            case 0xa48a5a when slot.EnemyDefinitionPointer == CrocomireDefinition:
                InitializeCrocomire(slot);
                return;
            case 0xa5ea2a when slot.EnemyDefinitionPointer == SporeSpawnDefinition:
                InitializeSporeSpawn(slot);
                return;
            case 0xa4f67a when slot.EnemyDefinitionPointer == CrocomireTongueDefinition:
                InitializeCrocomireTongue(slot);
                return;
            case 0xa8af8b when slot.EnemyDefinitionPointer == MagdolliteDefinition:
                InitializeMagdollite(slot, samus);
                return;
            case 0xa2a644:
                InitializeGunshipTop(slot);
                return;
            case 0xa2871c when slot.EnemyDefinitionPointer == BoyonDefinition:
                InitializeBoyon(slot);
                return;
            case 0xa289ad when slot.EnemyDefinitionPointer == StokeDefinition:
                InitializeStoke(slot);
                return;
            case 0xa28d6c when slot.EnemyDefinitionPointer == MamaTurtleDefinition:
                InitializeMamaTurtle(slot);
                return;
            case 0xa28d9d when slot.EnemyDefinitionPointer == BabyTurtleDefinition:
                InitializeBabyTurtle(slot);
                return;
            case 0xa29a3f when slot.EnemyDefinitionPointer == PuyoDefinition:
                InitializePuyo(slot);
                return;
            case 0xa29f48 when slot.EnemyDefinitionPointer == CacatacDefinition:
                InitializeCacatac(slot);
                return;
            case 0xa2a3f9 when slot.EnemyDefinitionPointer == OwtchDefinition:
                InitializeOwtch(slot);
                return;
            case 0xa2b3e0 when slot.EnemyDefinitionPointer == MultiviolaDefinition:
                InitializeMultiviola(slot);
                return;
            case 0xa2b570 when slot.EnemyDefinitionPointer == PolypDefinition:
                InitializePolyp(slot);
                return;
            case 0xa2b602 when slot.EnemyDefinitionPointer == RinkaDefinition:
                InitializeRinka(slot);
                return;
            case 0xa2bbcd when slot.EnemyDefinitionPointer == RioDefinition:
                InitializeRio(slot);
                return;
            case 0xa2be99 when slot.EnemyDefinitionPointer == NorfairLavaJumpingEnemyDefinition:
                InitializeNorfairLavaJumpingEnemy(slot);
                return;
            case 0xa2c242 when slot.EnemyDefinitionPointer == NorfairRioDefinition:
                InitializeNorfairRio(slot);
                return;
            case 0xa2c6f3 when slot.EnemyDefinitionPointer == LowerNorfairRioDefinition:
                InitializeLowerNorfairRio(slot);
                return;
            case 0xa2ccd4 when slot.EnemyDefinitionPointer == MaridiaLargeSnailDefinition:
                InitializeMaridiaLargeSnail(slot);
                return;
            case 0xa2e1d3 when slot.EnemyDefinitionPointer == GRipperDefinition:
                InitializeGRipper(slot);
                return;
            case 0xa2e318 when slot.EnemyDefinitionPointer == Ripper2Definition:
                InitializeRipper2(slot);
                return;
            case 0xa2e606 when slot.EnemyDefinitionPointer == DragonDefinition:
                InitializeDragon(slot);
                return;
            case 0xa2e9da when slot.EnemyDefinitionPointer == GrowingShutterDefinition:
                InitializeGrowingShutter(slot);
                return;
            case 0xa2ee12 when slot.EnemyDefinitionPointer is
                ShootableVerticalShutterDefinition or DestroyableVerticalShutterDefinition:
            case 0xa2ee05 when slot.EnemyDefinitionPointer == KamerVerticalPlatformDefinition:
                InitializeVerticalShutter(slot);
                return;
            case 0xa2f111 when slot.EnemyDefinitionPointer == ShootableHorizontalShutterDefinition:
                InitializeHorizontalShutter(slot, samus);
                return;
            case 0xa394e6 when slot.EnemyDefinitionPointer == ElevatorDefinition:
                InitializeElevator(slot, samus);
                return;
            case 0xa896e3 when IsFuneNamiheDefinition(slot.EnemyDefinitionPointer):
                InitializeFuneNamihe(slot);
                return;
            case 0xa2a6d2:
                InitializeGunshipBottom(slot);
                return;
            case 0xa6efb1:
                InitializeCeresSteam(slot);
                return;
            case 0xa6f6c5:
                InitializeCeresDoor(slot);
                return;
            case 0xa6a0f5 when slot.EnemyDefinitionPointer == CeresRidleyDefinition:
                InitializeCeresRidley(slot);
                return;
            case 0xa6a0f5 when slot.EnemyDefinitionPointer == NorfairRidleyDefinition:
                InitializeNorfairRidley(slot);
                return;
            case 0xa6c696 when slot.EnemyDefinitionPointer == NorfairRidleyExplosionDefinition:
                InitializeNorfairRidleyExplosion(
                    slot,
                    _slots[0],
                    _ridleyState ?? throw new InvalidOperationException(
                        "A Ridley breakup actor has no shared Ridley owner."));
                return;
            case 0xa686f5 when slot.EnemyDefinitionPointer == BoulderDefinition:
                InitializeBoulder(slot);
                return;
            case 0xa6fb72 when slot.EnemyDefinitionPointer == ZebetiteDefinition:
                InitializeZebetite(slot);
                return;
            case 0xa7e912 when slot.EnemyDefinitionPointer == EtecoonDefinition:
                InitializeEtecoon(slot);
                return;
            case 0xa7f4dd when slot.EnemyDefinitionPointer == DachoraDefinition:
                InitializeDachora(slot);
                return;
            case 0xa887e0 when slot.EnemyDefinitionPointer == EvirDefinition:
                InitializeEvir(slot, samus);
                return;
            case 0xa888b0 when slot.EnemyDefinitionPointer == EvirProjectileDefinition:
                InitializeEvirProjectile(slot);
                return;
            case 0xa89058 when slot.EnemyDefinitionPointer == MorphBallEyeDefinition:
                InitializeMorphBallEye(slot);
                return;
            case 0xa89aee when slot.EnemyDefinitionPointer == WreckedShipGhostDefinition:
                InitializeWreckedShipGhost(slot);
                return;
            case 0xa8a148 when slot.EnemyDefinitionPointer == YappingMawDefinition:
                InitializeYappingMaw(slot);
                return;
            case 0xa2e49f when slot.EnemyDefinitionPointer == RipperDefinition:
                InitializeRipper(slot);
                return;
            case 0xa2df76 when slot.EnemyDefinitionPointer == ChootDefinition:
                InitializeChoot(slot);
                return;
            case 0xa396e3 when slot.EnemyDefinitionPointer == SciserDefinition:
                InitializeCrawler(slot, SciserInitialInstructionTable, speciesInstructionOffset: 8);
                return;
            case 0xa3993b when slot.EnemyDefinitionPointer == ZeroDefinition:
                InitializeCrawler(slot, ZeroInitialInstructionTable, speciesInstructionOffset: 10);
                return;
            case 0xa3b66f when slot.EnemyDefinitionPointer == ViolaDefinition:
                InitializeCrawler(slot, ViolaInitialInstructionTable, speciesInstructionOffset: 6);
                return;
            case 0xa3e2d4 when slot.EnemyDefinitionPointer == ZeelaDefinition:
            case 0xa3e59c when slot.EnemyDefinitionPointer == SovaDefinition:
            case 0xa3e669 when slot.EnemyDefinitionPointer is ZoomerDefinition or StoneZoomerDefinition:
                InitializeCrawler(slot, SharedCrawlerInitialInstructionTable);
                return;
            case 0xa3e043 when slot.EnemyDefinitionPointer == HZoomerDefinition:
                InitializeHZoomer(slot);
                return;
            case 0xa3c6ae when slot.EnemyDefinitionPointer == SkreeDefinition:
                InitializeSkree(slot);
                return;
            case 0xa2b06b when slot.EnemyDefinitionPointer is
                MellowDefinition or MellaDefinition or MemuDefinition:
                InitializeFly(slot);
                return;
            case 0xa3a14d when slot.EnemyDefinitionPointer is SbugDefinition or Sbug2Definition:
                InitializeSbug(slot);
                return;
            case 0xa3a77d when slot.EnemyDefinitionPointer == MochtroidDefinition:
                InitializeMochtroid(slot);
                return;
            case 0xa3ea4f when slot.EnemyDefinitionPointer == MetroidDefinition:
                InitializeMetroid(slot);
                return;
            case 0xa3ab09 when IsHopperDefinition(slot.EnemyDefinitionPointer):
                InitializeHopper(slot);
                return;
            case 0xa3b44a when slot.EnemyDefinitionPointer == ZoaDefinition:
                InitializeZoa(slot);
                return;
            case 0xa3cde2 when slot.EnemyDefinitionPointer == YardDefinition:
                InitializeYard(slot);
                return;
            case 0xa386ed when slot.EnemyDefinitionPointer == WaverDefinition:
                InitializeWaver(slot);
                return;
            case 0xa38960 when slot.EnemyDefinitionPointer == MetareeDefinition:
                InitializeMetaree(slot);
                return;
            case 0xa38d2d when slot.EnemyDefinitionPointer == FirefleaDefinition:
                InitializeFireflea(slot);
                return;
            case 0xa390b5 when slot.EnemyDefinitionPointer == SkulteraDefinition:
                InitializeSkultera(slot);
                return;
            case 0xa39c9f when slot.EnemyDefinitionPointer == KamerDefinition:
            case 0xa39cba when slot.EnemyDefinitionPointer == TripperDefinition:
                InitializePlatform(slot);
                return;
            case 0xa8dccd when slot.EnemyDefinitionPointer == AlcoonDefinition:
                InitializeAlcoon(slot, level);
                return;
            case 0xa8ab46 when slot.EnemyDefinitionPointer == KagoDefinition:
                InitializeKago(slot);
                return;
            case 0xa8b776 when slot.EnemyDefinitionPointer == BeetomDefinition:
                InitializeBeetom(slot, samus, controllerInput);
                return;
            case 0xa8c1c9 when slot.EnemyDefinitionPointer == PowampDefinition:
                InitializePowamp(slot);
                return;
            case 0xa8cb77 when slot.EnemyDefinitionPointer == WorkRobotDefinition:
            case 0xa8cbcc when slot.EnemyDefinitionPointer == WorkRobotNoPowerDefinition:
                InitializeWorkRobot(slot);
                return;
            case 0xa8d8c9 when slot.EnemyDefinitionPointer == BullDefinition:
                InitializeBull(slot);
                return;
            case 0xa8e388 when slot.EnemyDefinitionPointer == AtomicDefinition:
                InitializeAtomic(slot);
                return;
            case 0xa8e637 when slot.EnemyDefinitionPointer == SparkDefinition:
                InitializeSpark(slot);
                return;
            case 0xa8e82e when
                slot.EnemyDefinitionPointer == BlueBrinstarFaceBlockDefinition:
                InitializeBlueBrinstarFaceBlock(slot, samus);
                return;
            case 0xa8f188 when IsKiHunterBodyDefinition(slot.EnemyDefinitionPointer):
                InitializeKiHunter(slot);
                return;
            case 0xa8f214 when IsKiHunterWingDefinition(slot.EnemyDefinitionPointer):
                InitializeKiHunterWings(slot);
                return;
            case 0xb3883b when IsBrinstarPipeBugDefinition(slot.EnemyDefinitionPointer):
                InitializeBrinstarPipeBug(slot);
                return;
            case 0xb38b61 when slot.EnemyDefinitionPointer == NorfairPipeBugDefinition:
                InitializeNorfairPipeBug(slot);
                return;
            case 0xb38f4c when slot.EnemyDefinitionPointer == YellowPipeBugDefinition:
                InitializeYellowPipeBug(slot);
                return;
            case 0xb39583 when slot.EnemyDefinitionPointer == BotwoonDefinition:
                InitializeBotwoon(slot);
                return;
            case 0xb3e6cb when slot.EnemyDefinitionPointer == EscapeEtecoonDefinition:
                InitializeEscapeEtecoon(slot);
                return;
            case 0xb3eae5 when slot.EnemyDefinitionPointer == EscapeDachoraDefinition:
                InitializeEscapeDachora(slot);
                return;
            case 0xa68b2f when slot.EnemyDefinitionPointer == KzanTopDefinition:
                InitializeKzanTop(slot);
                return;
            case 0xa68b85 when slot.EnemyDefinitionPointer == KzanBottomDefinition:
                InitializeKzanBottom(slot);
                return;
            case 0xa68ffc when slot.EnemyDefinitionPointer == HibashiDefinition:
                InitializeHibashi(slot);
                return;
            case 0xa694c4 when slot.EnemyDefinitionPointer == NuclearWaffleDefinition:
                InitializeNuclearWaffle(slot);
                return;
            case 0xa69a58 when slot.EnemyDefinitionPointer == FakeKraidDefinition:
                InitializeFakeKraid(slot, samus);
                return;
            case 0xb2fd02 when IsWalkingSpacePirateDefinition(slot.EnemyDefinitionPointer):
                InitializeWalkingSpacePirate(slot);
                return;
            case 0xb2ef9f when IsWallSpacePirateDefinition(slot.EnemyDefinitionPointer):
                InitializeWallSpacePirate(slot);
                return;
            case 0xb2f5de when IsNinjaSpacePirateDefinition(slot.EnemyDefinitionPointer):
                InitializeNinjaSpacePirate(slot);
                return;
            case 0xaac87f when slot.EnemyDefinitionPointer is
                BombTorizoDefinition or GoldenTorizoDefinition:
                InitializeBombTorizo(slot);
                return;
            case 0xa7a959 when slot.EnemyDefinitionPointer == KraidDefinition:
                InitializeKraidBody(slot);
                return;
            case 0xa7ab43 when slot.EnemyDefinitionPointer == KraidArmDefinition:
                InitializeKraidArm(slot);
                return;
            case 0xa7ab68 when slot.EnemyDefinitionPointer == KraidTopLintDefinition:
                InitializeKraidLint(slot, expectedSlot: 2);
                return;
            case 0xa7ab9c when slot.EnemyDefinitionPointer == KraidMiddleLintDefinition:
                InitializeKraidLint(slot, expectedSlot: 3);
                return;
            case 0xa7abca when slot.EnemyDefinitionPointer == KraidBottomLintDefinition:
                InitializeKraidLint(slot, expectedSlot: 4);
                return;
            case 0xa7abf8 when slot.EnemyDefinitionPointer == KraidFootDefinition:
                InitializeKraidFoot(slot);
                return;
            case 0xa7bcef when slot.EnemyDefinitionPointer == KraidGoodNailDefinition:
                InitializeKraidNail(slot, expectedSlot: 6);
                return;
            case 0xa7bd2d when slot.EnemyDefinitionPointer == KraidBadNailDefinition:
                InitializeKraidNail(slot, expectedSlot: 7);
                return;
            case 0xa7cdf3 when slot.EnemyDefinitionPointer == PhantoonBodyDefinition:
                InitializePhantoonBody(slot);
                return;
            case 0xa7ce55 when slot.EnemyDefinitionPointer is
                PhantoonEyeDefinition or PhantoonTentaclesDefinition or PhantoonMouthDefinition:
                InitializePhantoonPart(slot);
                return;
            case 0xa58687 when slot.EnemyDefinitionPointer == DraygonBodyDefinition:
                InitializeDraygonBody(slot);
                return;
            case 0xa5c46b when slot.EnemyDefinitionPointer == DraygonEyeDefinition:
            case 0xa5c599 when slot.EnemyDefinitionPointer == DraygonTailDefinition:
            case 0xa5c5ad when slot.EnemyDefinitionPointer == DraygonArmsDefinition:
                InitializeDraygonPart(slot);
                return;
            case 0xa98687 when slot.EnemyDefinitionPointer == MotherBrainBodyDefinition:
                InitializeMotherBrainBody(slot);
                return;
            case 0xa98705 when slot.EnemyDefinitionPointer == MotherBrainHeadDefinition:
                InitializeMotherBrainHead(slot);
                return;
            case 0xa98b35 when slot.EnemyDefinitionPointer == MotherBrainFallingTubeDefinition:
                InitializeMotherBrainFallingTube(slot);
                return;
            case 0xa9c710 when slot.EnemyDefinitionPointer == MotherBrainBabyMetroidDefinition:
                InitializeMotherBrainBabyMetroid(slot);
                return;
            case 0xa9d308 when slot.EnemyDefinitionPointer == DeadTorizoDefinition:
                InitializeDeadTorizo(slot);
                return;
            case 0xa9d7b6 when slot.EnemyDefinitionPointer == DeadSidehopperDefinition:
                InitializeDeadSidehopper(slot);
                return;
            case 0xa9d849 when slot.EnemyDefinitionPointer == DeadZoomerDefinition:
            case 0xa9d876 when slot.EnemyDefinitionPointer == DeadRipperDefinition:
            case 0xa9d89f when slot.EnemyDefinitionPointer == DeadSkreeDefinition:
                InitializeDeadTourianCorpse(slot);
                return;
            case 0xa9ef37 when slot.EnemyDefinitionPointer == ShitroidDefinition:
                InitializeShitroid(slot, cameraX);
                return;
            case 0xaad7c8 when slot.EnemyDefinitionPointer == TourianEntranceStatueDefinition:
                InitializeTourianEntranceStatue(slot);
                return;
            case 0xaade43 when slot.EnemyDefinitionPointer == ShaktoolDefinition:
                InitializeShaktool(slot);
                return;
            case 0xaae716 when slot.EnemyDefinitionPointer == N00bTubeCracksDefinition:
                InitializeN00bTubeCracks();
                return;
            case 0xaae725 when slot.EnemyDefinitionPointer == ChozoStatueDefinition:
                InitializeChozoStatue(slot);
                return;
            case 0xa2804c:
                return;
            default:
                throw new InvalidDataException(
                    $"Enemy ${slot.EnemyDefinitionPointer:X4} initialization AI ${address:X6} is not translated.");
        }
    }

    private void InitializeGunshipTop(RoomEnemySlot slot)
    {
        slot.Properties = slot.Properties.With(
            EnemyProperties.ProcessInstructions | EnemyProperties.IgnoreSamusCollision);
        slot.InstructionTimer = 1;
        slot.Timer = 0;
        slot.CurrentInstruction = 0xa616;
        slot.PaletteIndex = 0x0e00;
        if (_gunshipLoadScenario == GunshipLoadScenario.EscapingCeres)
        {
            SamusState samus = _samusAtEnemyInitialization
                ?? throw new InvalidOperationException(
                    "Post-Ceres gunship initialization requires the loader's Samus actor.");
            // `$A2:A669-$A678` attaches the top hull seventeen pixels above Samus and
            // enters function three, the high-altitude descent used only after Ceres.
            slot.YPosition = unchecked((ushort)(samus.YPosition - 17));
            slot.VariableF = 0xa80c;
        }
        else
        {
            slot.YPosition = unchecked((ushort)(slot.YPosition - 25));
            slot.VariableE = slot.YPosition;
            slot.VariableF = 0xa9bd;
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
        slot.CurrentInstruction = slot.Parameter2 != 0 ? (ushort)0xa60e : (ushort)0xa61c;

        // $A2:A6F1 reads enemy_drawing_queue[(cur_enemy_index >> 1) + 106].
        // For the two Landing Site bottom slots those WRAM addresses alias the preceding
        // slot's +$20 vram_tiles_index word. Express the alias semantically, but retain the
        // dependency on physical slot order.
        if (slot.SlotIndex == 0)
            throw new InvalidDataException("Gunship bottom cannot occupy enemy slot zero.");
        slot.VramTilesIndex = _slots[slot.SlotIndex - 1].VramTilesIndex;
        slot.PaletteIndex = 0x0e00;

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
        slot.VariableF = 0x804c;
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
        switch (address)
        {
            case 0xa586fc when slot.EnemyDefinitionPointer == DraygonBodyDefinition:
                RunDraygonBodyMain(slot, samus, nmiFrameCounter8);
                return;
            case 0xa5c486 when slot.EnemyDefinitionPointer == DraygonEyeDefinition:
                RunDraygonPartMain(slot, samus);
                return;
            case 0xa5c5aa when slot.EnemyDefinitionPointer == DraygonTailDefinition:
            case 0xa5c5c4 when slot.EnemyDefinitionPointer == DraygonArmsDefinition:
                RunDraygonPartMain(slot, samus);
                return;
            case 0xa9873e when slot.EnemyDefinitionPointer == MotherBrainBodyDefinition:
                RunMotherBrainBodyMain(slot, samus, nmiFrameCounter8, sharedProjectiles);
                return;
            case 0xa9878b when slot.EnemyDefinitionPointer == MotherBrainHeadDefinition:
                RunMotherBrainHeadMain(slot, samus);
                return;
            case 0xa98b85 when slot.EnemyDefinitionPointer == MotherBrainFallingTubeDefinition:
                RunMotherBrainFallingTubeMain(slot);
                return;
            case 0xa9c779 when slot.EnemyDefinitionPointer == MotherBrainBabyMetroidDefinition:
                RunMotherBrainBabyMetroidMain(slot, samus, cameraX, cameraY);
                return;
            case 0xa9d368 when slot.EnemyDefinitionPointer == DeadTorizoDefinition:
                RunDeadTorizoMain(slot, samus);
                return;
            case 0xa9d8db when slot.EnemyDefinitionPointer == DeadSidehopperDefinition:
                RunDeadSidehopperMain(slot, samus, level, cameraX);
                return;
            case 0xa9d8db when IsDeadTourianCorpseDefinition(slot.EnemyDefinitionPointer):
                RunDeadTourianCorpseMain(slot, samus);
                return;
            case 0xa9efc5 when slot.EnemyDefinitionPointer == ShitroidDefinition:
                RunShitroidMain(slot, samus, cameraX, cameraY, sharedProjectiles);
                return;
            case 0xa48c04 when slot.EnemyDefinitionPointer == CrocomireDefinition:
                RunCrocomireMain(slot, samus, controllerInput, level, cameraX);
                return;
            case 0xa5eb13 when slot.EnemyDefinitionPointer == SporeSpawnDefinition:
                RunSporeSpawnMain(slot, RequireSporeSpawnState(slot), nmiFrameCounter8);
                return;
            case 0xa4f6bb when slot.EnemyDefinitionPointer == CrocomireTongueDefinition:
                // $A4:F6BB is a literal RTL. The tongue's bank-$A4 instruction list and
                // extended map position the component relative to Crocomire's body.
                return;
            case 0xa8b10a when slot.EnemyDefinitionPointer == MagdolliteDefinition:
                RunMagdolliteMain(slot, RequireMagdolliteState(slot), samus);
                return;
            case 0xa2a759:
                RunGunshipTopMain(
                    slot,
                    samus,
                    newlyPressedControllerInput,
                    vramWriteQueue);
                return;
            case 0xa2879c when slot.EnemyDefinitionPointer == BoyonDefinition:
                RunBoyonMain(slot, RequireBoyonState(slot), samus);
                return;
            case 0xa289f0 when slot.EnemyDefinitionPointer == StokeDefinition:
                RunStokeMain(slot, RequireStokeState(slot), level);
                return;
            case 0xa28dd2 when slot.EnemyDefinitionPointer == MamaTurtleDefinition:
                RunMamaTurtleMain(
                    slot,
                    RequireMamaTurtleState(slot),
                    samus,
                    level,
                    controllerInput,
                    nmiFrameCounter8);
                return;
            case 0xa2912e when slot.EnemyDefinitionPointer == BabyTurtleDefinition:
                RunBabyTurtleMain(slot, RequireBabyTurtleState(slot), samus, level);
                return;
            case 0xa29a7d when slot.EnemyDefinitionPointer == PuyoDefinition:
                RunPuyoMain(slot, RequirePuyoState(slot), samus, level);
                return;
            case 0xa29fb3 when slot.EnemyDefinitionPointer == CacatacDefinition:
                RunCacatacMain(slot, RequireCacatacState(slot));
                return;
            case 0xa2a47e when slot.EnemyDefinitionPointer == OwtchDefinition:
                RunOwtchMain(slot, RequireOwtchState(slot));
                return;
            case 0xa2b40f when slot.EnemyDefinitionPointer == MultiviolaDefinition:
                RunMultiviolaMain(slot, RequireMultiviolaState(slot), level);
                return;
            case 0xa2b58f when slot.EnemyDefinitionPointer == PolypDefinition:
                RunPolypMain(slot, RequirePolypState(slot), samus);
                return;
            case 0xa2b7c4 when slot.EnemyDefinitionPointer == RinkaDefinition:
                RunRinkaMain(slot, RequireRinkaState(slot), samus);
                return;
            case 0xa2bbe3 when slot.EnemyDefinitionPointer == RioDefinition:
                RunRioMain(
                    slot,
                    RequireRioState(slot),
                    samus,
                    level,
                    cameraX,
                    cameraY);
                return;
            case 0xa2bed2 when slot.EnemyDefinitionPointer == NorfairLavaJumpingEnemyDefinition:
                RunNorfairLavaJumpingEnemyMain(
                    slot,
                    RequireNorfairLavaJumpingEnemyState(slot));
                return;
            case 0xa2c277 when slot.EnemyDefinitionPointer == NorfairRioDefinition:
                RunNorfairRioMain(
                    slot,
                    RequireNorfairRioState(slot),
                    samus,
                    level);
                return;
            case 0xa2c724 when slot.EnemyDefinitionPointer == LowerNorfairRioDefinition:
                RunLowerNorfairRioMain(
                    slot,
                    RequireLowerNorfairRioState(slot),
                    samus,
                    level);
                return;
            case 0xa2cd13 when slot.EnemyDefinitionPointer == MaridiaLargeSnailDefinition:
                RunMaridiaLargeSnailMain(
                    slot,
                    RequireMaridiaLargeSnailState(slot),
                    samus,
                    level,
                    controllerInput);
                return;
            case 0xa2e221 when slot.EnemyDefinitionPointer == GRipperDefinition:
                RunGRipperMain(slot, RequireRipperVariantState(slot), level);
                return;
            case 0xa2e353 when slot.EnemyDefinitionPointer == Ripper2Definition:
                RunRipper2Main(slot, level);
                return;
            case 0xa2e64e when slot.EnemyDefinitionPointer == DragonDefinition:
                RunDragonMain(slot, RequireDragonState(slot), samus);
                return;
            case 0xa2eab6 when slot.EnemyDefinitionPointer == GrowingShutterDefinition:
                RunGrowingShutterMain(
                    slot,
                    RequireGrowingShutterState(slot),
                    samus,
                    cameraX,
                    cameraY);
                return;
            case 0xa2eed1 when IsVerticalShutterDefinition(slot.EnemyDefinitionPointer):
                RunVerticalShutterMain(
                    slot,
                    RequireVerticalShutterState(slot),
                    samus,
                    cameraX,
                    cameraY);
                return;
            case 0xa2f1de when slot.EnemyDefinitionPointer == ShootableHorizontalShutterDefinition:
                RunHorizontalShutterMain(
                    slot,
                    RequireHorizontalShutterState(slot),
                    samus,
                    controllerInput);
                return;
            case 0xa3952a when slot.EnemyDefinitionPointer == ElevatorDefinition:
                RunElevatorMain(
                    slot,
                    RequireElevatorState(slot),
                    samus,
                    newlyPressedControllerInput,
                    samusProjectiles);
                return;
            case 0xa89730 when IsFuneNamiheDefinition(slot.EnemyDefinitionPointer):
                RunFuneNamiheMain(slot, RequireFuneNamiheState(slot), samus);
                return;
            case 0xa8ab75 when slot.EnemyDefinitionPointer == KagoDefinition:
                RunKagoMain(RequireKagoState(slot));
                return;
            case 0xa2804c:
                return;
            case 0xa7804c when slot.EnemyDefinitionPointer == PhantoonEyeDefinition:
                // Phantoon's eye is positioned by the body and animated by its own list;
                // the header main is the literal common RTL at the start of bank $A7.
                return;
            case 0xa6f00d:
                RunCeresSteamMain(slot, mode7Transform);
                return;
            case 0xa6f765:
                RunCeresDoorMain(slot);
                return;
            case 0xa6a288 when slot.EnemyDefinitionPointer == 0xe13f:
                RunCeresRidleyMain(slot, samus, vramWriteQueue);
                return;
            case 0xa6b227 when slot.EnemyDefinitionPointer == NorfairRidleyDefinition:
                RunNorfairRidleyMain(
                    slot,
                    samus,
                    controllerInput,
                    level,
                    samusProjectiles);
                return;
            case 0xa6c8d4 when slot.EnemyDefinitionPointer == NorfairRidleyExplosionDefinition:
                RunNorfairRidleyExplosionMain(slot);
                return;
            case 0xa68793 when slot.EnemyDefinitionPointer == BoulderDefinition:
                RunBoulderMain(slot, RequireBoulderState(slot), samus, level);
                return;
            case 0xa6fc33 when slot.EnemyDefinitionPointer == ZebetiteDefinition:
                RunZebetiteMain(slot, RequireZebetiteState(slot));
                return;
            case 0xa7e940 when slot.EnemyDefinitionPointer == EtecoonDefinition:
                RunEtecoonMain(slot, RequireEtecoonState(slot), samus, level);
                return;
            case 0xa7f52e when slot.EnemyDefinitionPointer == DachoraDefinition:
                RunDachoraMain(
                    slot,
                    RequireDachoraState(slot),
                    samus,
                    level,
                    nmiFrameCounter8);
                return;
            case 0xa8891b when slot.EnemyDefinitionPointer == EvirDefinition:
                RunEvirMain(slot, RequireEvirState(slot), samus);
                return;
            case 0xa8899e when slot.EnemyDefinitionPointer == EvirProjectileDefinition:
                RunEvirProjectileMain(
                    slot,
                    RequireEvirState(slot),
                    samus,
                    cameraX,
                    cameraY);
                return;
            case 0xa890e2 when slot.EnemyDefinitionPointer == MorphBallEyeDefinition:
                RunMorphBallEyeMain(slot, RequireMorphBallEyeState(slot), samus);
                return;
            case 0xa89b3c when slot.EnemyDefinitionPointer == WreckedShipGhostDefinition:
                RunWreckedShipGhostMain(slot, RequireWreckedShipGhostState(slot), samus);
                return;
            case 0xa8a211 when slot.EnemyDefinitionPointer == YappingMawDefinition:
                RunYappingMawMain(
                    slot,
                    RequireYappingMawState(slot),
                    samus,
                    cameraX,
                    cameraY);
                return;
            case 0xa2e4da when slot.EnemyDefinitionPointer == RipperDefinition:
                RunRipperMain(slot, level);
                return;
            case 0xa2e02e when slot.EnemyDefinitionPointer == ChootDefinition:
                RunChootMain(slot, RequireChootState(slot), samus);
                return;
            case 0xa3e6c2 when IsSharedCrawlerDefinition(slot.EnemyDefinitionPointer):
                RunCrawlerMain(slot, level);
                return;
            case 0xa3e08b when slot.EnemyDefinitionPointer == HZoomerDefinition:
                RunHZoomerMain(slot, samus, level);
                return;
            case 0xa3c6c7 when slot.EnemyDefinitionPointer == SkreeDefinition:
                RunSkreeMain(slot, samus, level);
                return;
            case 0xa2b11f when slot.EnemyDefinitionPointer is
                MellowDefinition or MellaDefinition or MemuDefinition:
                RunFlyMain(slot, samus);
                return;
            case 0xa3a2d0 when slot.EnemyDefinitionPointer is SbugDefinition or Sbug2Definition:
                RunSbugMain(slot, samus, level);
                return;
            case 0xa3a790 when slot.EnemyDefinitionPointer == MochtroidDefinition:
                RunMochtroidMain(slot, samus, level);
                return;
            case 0xa3eb98 when slot.EnemyDefinitionPointer == MetroidDefinition:
                RunMetroidMain(slot, samus, level);
                return;
            case 0xa3abcf when IsHopperDefinition(slot.EnemyDefinitionPointer):
                RunHopperMain(slot, samus, level);
                return;
            case 0xa3b47c when slot.EnemyDefinitionPointer == ZoaDefinition:
                RunZoaMain(slot, RequireZoaState(slot), samus, cameraX, cameraY);
                return;
            case 0xa3ce64 when slot.EnemyDefinitionPointer == YardDefinition:
                RunYardMain(slot, samus, level);
                return;
            case 0xa3874c when slot.EnemyDefinitionPointer == WaverDefinition:
                RunWaverMain(slot, RequireWaverState(slot), level);
                return;
            case 0xa38979 when slot.EnemyDefinitionPointer == MetareeDefinition:
                RunMetareeMain(slot, RequireMetareeState(slot), samus, level);
                return;
            case 0xa38dee when slot.EnemyDefinitionPointer == FirefleaDefinition:
                RunFirefleaMain(slot, RequireFirefleaState(slot));
                return;
            case 0xa3912b when slot.EnemyDefinitionPointer == SkulteraDefinition:
                RunSkulteraMain(slot, RequireSkulteraState(slot), level);
                return;
            case 0xa39d16 when IsPlatformDefinition(slot.EnemyDefinitionPointer):
                RunPlatformMain(slot, RequirePlatformState(slot), samus, level);
                return;
            case 0xa8dd6b when slot.EnemyDefinitionPointer == AlcoonDefinition:
                RunAlcoonMain(slot, RequireAlcoonState(slot), samus, level);
                return;
            case 0xa8b80d when slot.EnemyDefinitionPointer == BeetomDefinition:
                RunBeetomMain(slot, RequireBeetomState(slot), samus, level, controllerInput);
                return;
            case 0xa8c21c when slot.EnemyDefinitionPointer == PowampDefinition:
                RunPowampMain(slot, RequirePowampState(slot), level);
                return;
            case 0xa8cc36 when slot.EnemyDefinitionPointer == WorkRobotDefinition:
                RunWorkRobotMain(slot, RequireWorkRobotState(slot), level);
                return;
            case 0xa8cc66 when slot.EnemyDefinitionPointer == WorkRobotNoPowerDefinition:
                return;
            case 0xa8d90b when slot.EnemyDefinitionPointer == BullDefinition:
                RunBullMain(slot, RequireBullState(slot), samus);
                return;
            case 0xa8e3c3 when slot.EnemyDefinitionPointer == AtomicDefinition:
                RunAtomicMain(slot, RequireAtomicState(slot), samus);
                return;
            case 0xa8e68e when slot.EnemyDefinitionPointer == SparkDefinition:
                RunSparkMain(slot, RequireSparkState(slot));
                return;
            case 0xa8e8ae when
                slot.EnemyDefinitionPointer == BlueBrinstarFaceBlockDefinition:
                RunBlueBrinstarFaceBlockMain(
                    slot,
                    RequireBlueBrinstarFaceBlockState(slot),
                    samus);
                return;
            case 0xa8f25c when IsKiHunterBodyDefinition(slot.EnemyDefinitionPointer):
            case 0xa8f262 when IsKiHunterWingDefinition(slot.EnemyDefinitionPointer):
                RunKiHunterMain(slot, RequireKiHunterState(slot), samus, level);
                return;
            case 0xb3887a when IsBrinstarPipeBugDefinition(slot.EnemyDefinitionPointer):
            case 0xb38b9e when slot.EnemyDefinitionPointer == NorfairPipeBugDefinition:
            case 0xb38fae when slot.EnemyDefinitionPointer == YellowPipeBugDefinition:
                RunPipeBugMain(
                    slot,
                    RequirePipeBugState(slot),
                    samus,
                    cameraX,
                    cameraY);
                return;
            case 0xb39668 when slot.EnemyDefinitionPointer == BotwoonDefinition:
                RunBotwoonMain(slot, RequireBotwoonState(slot), samus);
                return;
            case 0xb3e655 when slot.EnemyDefinitionPointer == EscapeEtecoonDefinition:
                RunEscapeEtecoonMain(
                    slot,
                    RequireEscapeEtecoonState(slot),
                    level);
                return;
            case 0xb3eb1a when slot.EnemyDefinitionPointer == EscapeDachoraDefinition:
                // $B3:EB1A is a literal RTL. Dachora's complete movement program lives in
                // its ROM instruction lists and therefore runs later in this same frame.
                return;
            case 0xa68bad when slot.EnemyDefinitionPointer == KzanTopDefinition:
                RunKzanTopMain(slot, RequireKzanState(slot), samus);
                return;
            case 0xa68b99 when slot.EnemyDefinitionPointer == KzanBottomDefinition:
                RunKzanBottomMain(slot);
                return;
            case 0xa69023 when slot.EnemyDefinitionPointer == HibashiDefinition:
                RunHibashiMain(slot, RequireHibashiState(slot));
                return;
            case 0xa6960e when slot.EnemyDefinitionPointer == NuclearWaffleDefinition:
                RunNuclearWaffleMain(slot, RequireNuclearWaffleState(slot));
                return;
            case 0xa69ac2 when slot.EnemyDefinitionPointer == FakeKraidDefinition:
                RunFakeKraidMain(
                    slot,
                    RequireFakeKraidState(slot),
                    cameraX,
                    cameraY);
                return;
            case 0xb2fd32 when IsWalkingSpacePirateDefinition(slot.EnemyDefinitionPointer):
                RunWalkingSpacePirateMain(
                    slot,
                    RequireWalkingSpacePirateState(slot),
                    samus,
                    level,
                    samusProjectiles);
                return;
            case 0xb2f02d when IsWallSpacePirateDefinition(slot.EnemyDefinitionPointer):
                RunWallSpacePirateMain(
                    slot,
                    RequireWallSpacePirateState(slot),
                    samus);
                return;
            case 0xb2f6a2 when IsNinjaSpacePirateDefinition(slot.EnemyDefinitionPointer):
                RunNinjaSpacePirateMain(
                    slot,
                    RequireNinjaSpacePirateState(slot),
                    samus,
                    level,
                    samusProjectiles);
                return;
            case 0xaac6a4 when slot.EnemyDefinitionPointer == BombTorizoDefinition:
                RunBombTorizoMain(slot, RequireBombTorizoState(slot), samus, level);
                return;
            case 0xaad369 when slot.EnemyDefinitionPointer == GoldenTorizoDefinition:
                RunGoldenTorizoMain(
                    slot,
                    RequireBombTorizoState(slot),
                    samus,
                    level);
                return;
            case 0xa7ac21 when slot.EnemyDefinitionPointer == KraidDefinition:
                RunKraidBodyMain(slot, samus);
                return;
            case 0xa7b7bd when slot.EnemyDefinitionPointer == KraidArmDefinition:
                RunKraidArmMain(slot, cameraY);
                return;
            case 0xa7b801 when slot.EnemyDefinitionPointer == KraidTopLintDefinition:
            case 0xa7b80d when slot.EnemyDefinitionPointer == KraidMiddleLintDefinition:
            case 0xa7b819 when slot.EnemyDefinitionPointer == KraidBottomLintDefinition:
                RunKraidLintMain(slot);
                return;
            case 0xa7b9f6 when slot.EnemyDefinitionPointer == KraidFootDefinition:
                RunKraidFootMain(slot, cameraY);
                return;
            case 0xa7bd32 when slot.EnemyDefinitionPointer == KraidGoodNailDefinition:
            case 0xa7bd49 when slot.EnemyDefinitionPointer == KraidBadNailDefinition:
                RunKraidNailMain(slot, level);
                return;
            case 0xa7cea6 when slot.EnemyDefinitionPointer == PhantoonBodyDefinition:
                RunPhantoonMain(slot, samus, cameraX, cameraY, nmiFrameCounter8);
                return;
            case 0xa7e011 when slot.EnemyDefinitionPointer is
                PhantoonTentaclesDefinition or PhantoonMouthDefinition:
                // $A7:E011 is a literal RTL. These drawing parts animate entirely through
                // their independent bank-$A7 instruction lists after the shared body main.
                return;
            case 0xaad7c7 when slot.EnemyDefinitionPointer == TourianEntranceStatueDefinition:
                // $AA:D7C7 is the one-byte RTL immediately before the initializer. The
                // three enemy records animate exclusively through their ROM lists.
                return;
            case 0xaadca3 when slot.EnemyDefinitionPointer == ShaktoolDefinition:
                RunShaktoolMain(slot, RequireShaktoolState(slot), level);
                return;
            case 0xaae7a7 when slot.EnemyDefinitionPointer == ChozoStatueDefinition:
                RunChozoStatueMain(slot, RequireChozoStatueState(slot));
                return;
            default:
                throw new InvalidDataException(
                    $"Enemy ${slot.EnemyDefinitionPointer:X4} main AI ${address:X6} is not translated.");
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
            QueueEnemySound(library: 2, soundId: 0x004d, maximumQueued: 6);
            bottom.VariableD = 70;
        }

        // The native address-range test admits functions $A942-$AC1A. Idle function $A9BD
        // is inside that interval, so the landed ship continuously performs its four-phase
        // bob before dispatching the function itself.
        if (!IsNegative16(top.VariableF + 0x56be) && IsNegative16(top.VariableF + 0x53e5))
            StepGunshipBob(top);

        switch (top.VariableF)
        {
            case 0xa80c:
                DescendPostCeresGunship(top, samus);
                return;
            case 0xa8d0:
                BouncePostCeresGunship(top, samus);
                return;
            case 0xa942:
                if (TickGunshipFunctionTimer(top))
                    top.VariableF = 0xa950;
                return;
            case 0xa950:
                RaisePostCeresSamus(top, samus);
                return;
            case 0xa987:
                if (TickGunshipFunctionTimer(top))
                {
                    top.VariableF = 0xa9bd;
                    if (samus is not null)
                        samus.InputLocked = false;
                    LastGunshipEvent = GunshipFrameEvent.LandingCompleted;
                }
                return;
            case 0xa9bd:
                HandleIdleGunshipEntrance(top, samus, newlyPressedControllerInput);
                return;
            case 0xaa4f:
                if (TickGunshipFunctionTimer(top))
                    top.VariableF = 0xaa5d;
                return;
            case 0xaa5d:
                LowerSamusIntoGunship(top, samus);
                return;
            case 0xaa94:
                if (TickGunshipFunctionTimer(top))
                    top.VariableF = 0xaaa2;
                return;
            case 0xaaa2:
                RestoreSamusInGunship(top, samus);
                return;
            case 0xab1f:
                GunshipSavePromptPending = true;
                return;
            case 0xab60:
                if (TickGunshipFunctionTimer(top))
                    top.VariableF = 0xab6e;
                return;
            case 0xab6e:
                RaiseSamusOutOfGunship(top, samus);
                return;
            case 0xaba5:
                if (TickGunshipFunctionTimer(top))
                {
                    top.VariableF = 0xa9bd;
                    if (samus is not null)
                        samus.InputLocked = false;
                    LastGunshipEvent = GunshipFrameEvent.ExitCompleted;
                }
                return;
            case 0xabc7:
                QueueGunshipTakeoffTiles(top, vramWriteQueue);
                return;
            case 0xac1b:
                FireUpGunshipEngines(top, samus);
                return;
            case 0xacd7:
                LiftGunshipAtConstantSpeed(top, samus);
                return;
            case 0xad0e:
                AccelerateEscapingGunship(top, samus);
                return;
            case 0xad2d:
                MoveEscapingGunship(top, samus);
                return;
            default:
                throw new InvalidDataException(
                    $"Gunship function $A2:{top.VariableF:X4} is not translated.");
        }
    }

    private void DescendPostCeresGunship(RoomEnemySlot top, SamusState? samus)
    {
        if (samus is null)
            throw new InvalidOperationException("Post-Ceres gunship descent lost Samus.");

        // Function three carries all four actors as a rigid body. Above Y=$0300 it moves
        // $4.8000 pixels per call; below that threshold it slows to $2.8000 and clamps the
        // top hull to $045F before beginning the cartridge's seventeen-entry bounce table.
        uint delta = top.YPosition < 0x0300 ? 0x0004_8000u : 0x0002_8000u;
        AddGunshipYFixed(samus, top, delta);
        if (top.YPosition < 0x045f)
            return;

        RoomEnemySlot bottom = _slots[top.SlotIndex + 1];
        RoomEnemySlot pad = _slots[top.SlotIndex + 2];
        top.YPosition = 0x045f;
        top.YSubposition = 0;
        bottom.YPosition = 0x0487;
        bottom.YSubposition = 0;
        pad.YPosition = 0x045e;
        pad.YSubposition = 0;
        top.VariableF = 0xa8d0;
        top.VariableE = 0;
    }

    private void BouncePostCeresGunship(RoomEnemySlot top, SamusState? samus)
    {
        if (samus is null)
            throw new InvalidOperationException("Post-Ceres gunship bounce lost Samus.");

        int tableAddress = 0xa2a622 + top.VariableE * 2;
        short yDelta = unchecked((short)ReadWord(_bus!, tableAddress));
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
        top.VariableF = 0xa942;
        top.VariableE = top.YPosition;
        top.VariableD = 1;
        top.VariableC = 0;
        samus.XPosition = unchecked((ushort)(top.XPosition + 1));
        pad.InstructionTimer = 1;
        pad.CurrentInstruction = 0xa5be;
        top.VariableA = 144;
        LastGunshipEvent = GunshipFrameEvent.LandingPadOpened;
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
        top.VariableF = 0xa987;
        pad.InstructionTimer = 1;
        pad.CurrentInstruction = 0xa5ee;
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

        int tableAddress = 0xa2a7cf + (top.VariableC & 3) * 2;
        top.VariableD = _bus!.ReadByte(tableAddress);
        sbyte yDelta = unchecked((sbyte)_bus.ReadByte(tableAddress + 1));
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
        if (samus is null || (newlyPressedControllerInput & 0x0400) == 0)
            return;

        bool insideEntrance =
            (short)unchecked((ushort)(top.XPosition - 8 - samus.XPosition)) < 0 &&
            (short)unchecked((ushort)(top.XPosition + 8 - samus.XPosition)) >= 0 &&
            (short)unchecked((ushort)(top.YPosition - 64 - samus.YPosition)) < 0 &&
            (short)unchecked((ushort)(top.YPosition - samus.YPosition)) >= 0;
        if (insideEntrance && samus.ReadMovementKind(_bus!) == SamusMovementType.Standing)
        {
            RoomEnemySlot pad = _slots[top.SlotIndex + 2];
            top.VariableF = 0xaa4f;
            if (samus.XPosition != 0x0480)
                samus.XPosition = top.XPosition;
            samus.ApplyForwardFacingPoseSetup(_bus!);
            samus.InputLocked = true;
            samus.PrimeGraphics(_bus!);
            pad.YPosition = unchecked((ushort)(top.YPosition - 1));
            pad.InstructionTimer = 1;
            pad.CurrentInstruction = 0xa5be;
            top.VariableA = 144;
            LastGunshipEvent = GunshipFrameEvent.EntryStarted;
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
        top.VariableF = 0xaa94;
        pad.InstructionTimer = 1;
        pad.CurrentInstruction = 0xa5ee;
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
        if (RequireEvent((int)EventNumber.ZebesTimebombSet))
        {
            RoomEnemySlot bottom = _slots[top.SlotIndex + 1];
            top.VariableF = 0xabc7;
            top.VariableB = 0;
            bottom.VariableF = 0;
            bottom.VariableE = 0;
            LastGunshipEvent = GunshipFrameEvent.EscapeTakeoffStarted;
            return;
        }

        samus.Health = RestoreTwo(samus.Health, samus.MaxHealth);
        samus.Missiles = RestoreTwo(samus.Missiles, samus.MaxMissiles);
        samus.SuperMissiles = RestoreTwo(samus.SuperMissiles, samus.MaxSuperMissiles);
        samus.PowerBombs = RestoreTwo(samus.PowerBombs, samus.MaxPowerBombs);
        if ((short)(samus.ReserveEnergy - samus.MaxReserveEnergy) < 0 ||
            (short)(samus.Health - samus.MaxHealth) < 0 ||
            (short)(samus.Missiles - samus.MaxMissiles) < 0 ||
            (short)(samus.SuperMissiles - samus.MaxSuperMissiles) < 0 ||
            (short)(samus.PowerBombs - samus.MaxPowerBombs) < 0)
            return;

        top.VariableF = 0xab1f;
        GunshipSavePromptPending = true;
        LastGunshipEvent = GunshipFrameEvent.SavePromptRequested;
    }

    private void RaiseSamusOutOfGunship(RoomEnemySlot top, SamusState? samus)
    {
        if (samus is null)
            throw new InvalidOperationException("Gunship exit lost its Samus actor.");
        samus.YPosition = unchecked((ushort)(samus.YPosition - 2));
        if (!IsNegative16(samus.YPosition - unchecked((ushort)(top.VariableE - 30))))
            return;

        RoomEnemySlot pad = _slots[top.SlotIndex + 2];
        top.VariableF = 0xaba5;
        pad.InstructionTimer = 1;
        pad.CurrentInstruction = 0xa5ee;
        top.VariableA = 144;
        LastGunshipEvent = GunshipFrameEvent.ExitPadClosing;
    }

    /// <summary>Ports gunship function 17 at <c>$A2:ABC7</c>.</summary>
    private void QueueGunshipTakeoffTiles(
        RoomEnemySlot top,
        VramWriteQueue? vramWriteQueue)
    {
        if (vramWriteQueue is null)
            throw new InvalidOperationException("Gunship takeoff requires the runtime VRAM queue.");

        int transferIndex = top.VariableB;
        if ((uint)transferIndex >= 5)
            throw new InvalidDataException("Gunship takeoff tile index escaped its five-entry table.");
        ushort source = ReadWord(_bus!, 0xa2ac07 + transferIndex * 2);
        ushort destination = ReadWord(_bus!, 0xa2ac11 + transferIndex * 2);
        vramWriteQueue.Enqueue(
            sizeInBytes: 0x0400,
            sourceAddress: 0x940000 | source,
            encodedVramDestination: destination);

        top.VariableB++;
        if (top.VariableB >= 5)
        {
            top.VariableF = 0xac1b;
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
            top.VariableF = 0xacd7;
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
            top.VariableF = 0xad0e;
            _slots[top.SlotIndex + 1].VariableF = 0x0200;
        }
    }

    /// <summary>Ports the accelerating takeoff function at <c>$A2:AD0E</c>.</summary>
    private void AccelerateEscapingGunship(RoomEnemySlot top, SamusState? samus)
    {
        MoveEscapingGunship(top, samus);
        if (IsNegative16(top.YPosition - 256))
        {
            top.VariableF = 0xad2d;
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

    private void ProcessInstructions(
        RoomEnemySlot slot,
        SamusState? samus,
        RoomLevelData? level,
        ushort cameraX,
        ushort cameraY,
        ushort controllerInput,
        byte nmiFrameCounter8)
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
            ushort word = ReadWord(_bus!, (slot.Definition.Bank << 16) | cursor);
            if ((word & 0x8000) == 0)
            {
                slot.InstructionTimer = word;
                slot.SpritemapPointer = ReadWord(
                    _bus!,
                    (slot.Definition.Bank << 16) | unchecked((ushort)(cursor + 2)));
                slot.CurrentInstruction = unchecked((ushort)(cursor + 4));
                slot.ExtraProperties = slot.ExtraProperties.With(EnemyExtraProperties.NewInstructionFrame);
                return;
            }

            switch (word)
            {
                case 0x807c: // EnemyInstr_StopScript: delete the actor and abort interpretation.
                    slot.Properties = slot.Properties.With(EnemyProperties.Deleted);
                    return;
                case 0x80ed: // EnemyInstr_Goto: next word is a same-bank instruction pointer.
                    cursor = ReadWord(
                        _bus!,
                        (slot.Definition.Bank << 16) | unchecked((ushort)(cursor + 2)));
                    break;
                case 0x812f: // EnemyInstr_Sleep: pin the PC on this command and stop forever.
                    slot.CurrentInstruction = cursor;
                    return;
                case 0x8173: // EnemyInstr_EnableOffScreenProcessing.
                    slot.Properties = slot.Properties.With(EnemyProperties.ProcessOffScreen);
                    cursor = unchecked((ushort)(cursor + 2));
                    break;
                case 0x817d: // EnemyInstr_DisableOffScreenProcessing.
                    slot.Properties = slot.Properties.Without(EnemyProperties.ProcessOffScreen);
                    cursor = unchecked((ushort)(cursor + 2));
                    break;
                case 0xcfb4 when slot.EnemyDefinitionPointer == MotherBrainBabyMetroidDefinition:
                case 0xcfca when slot.EnemyDefinitionPointer == MotherBrainBabyMetroidDefinition:
                    _ = TryRunMotherBrainBabyInstruction(word, ref cursor);
                    break;
                case 0x88c5 when slot.EnemyDefinitionPointer == BoyonDefinition:
                    // `$A2:88C5` is an explicit RTL instruction. It consumes only itself;
                    // keeping it distinct documents the idle-list seam in the ROM.
                    cursor = unchecked((ushort)(cursor + 2));
                    break;
                case 0x88c6 when slot.EnemyDefinitionPointer == BoyonDefinition:
                    StartBoyonBounce(RequireBoyonState(slot));
                    cursor = unchecked((ushort)(cursor + 2));
                    break;
                case 0x897e when slot.EnemyDefinitionPointer == StokeDefinition:
                    SpawnStokeProjectile(
                        slot,
                        ReadWord(
                            _bus!,
                            (slot.Definition.Bank << 16) | unchecked((ushort)(cursor + 2))));
                    cursor = unchecked((ushort)(cursor + 4));
                    break;
                case 0x8990 when slot.EnemyDefinitionPointer == StokeDefinition:
                    SetStokeMovingLeft(RequireStokeState(slot));
                    cursor = unchecked((ushort)(cursor + 2));
                    break;
                case 0x899d when slot.EnemyDefinitionPointer == StokeDefinition:
                    SetStokeMovingRight(RequireStokeState(slot));
                    cursor = unchecked((ushort)(cursor + 2));
                    break;
                case 0x9381 when slot.EnemyDefinitionPointer == BabyTurtleDefinition:
                    ProcessBabyTurtleCrawlInstruction(
                        slot,
                        RequireBabyTurtleState(slot),
                        samus,
                        level);
                    cursor = unchecked((ushort)(cursor + 2));
                    break;
                case 0x9412 when slot.EnemyDefinitionPointer == BabyTurtleDefinition:
                    cursor = SelectBabyTurtleCrawlLoop(slot, RequireBabyTurtleState(slot));
                    break;
                case 0x9447 when slot.EnemyDefinitionPointer == MamaTurtleDefinition:
                    StartMamaTurtleEnteringShell(RequireMamaTurtleState(slot));
                    cursor = unchecked((ushort)(cursor + 2));
                    break;
                case 0x9451 when slot.EnemyDefinitionPointer == MamaTurtleDefinition:
                    StartMamaTurtleRisingToHover(
                        RequireMamaTurtleState(slot),
                        rightward: true);
                    cursor = MamaTurtleSpinningInstruction;
                    break;
                case 0x946b when slot.EnemyDefinitionPointer == MamaTurtleDefinition:
                    StartMamaTurtleRisingToHover(
                        RequireMamaTurtleState(slot),
                        rightward: false);
                    cursor = MamaTurtleSpinningInstruction;
                    break;
                case 0x9485 when slot.EnemyDefinitionPointer == BabyTurtleDefinition:
                    cursor = SelectBabyTurtleLeaveShell(
                        slot,
                        RequireBabyTurtleState(slot),
                        samus,
                        unchecked((ushort)(cursor + 2)));
                    break;
                case 0x94a1 when slot.EnemyDefinitionPointer == BabyTurtleDefinition:
                    cursor = FinishBabyTurtleLeavingShell(
                        slot,
                        RequireBabyTurtleState(slot),
                        samus);
                    break;
                case 0x94c7 when slot.EnemyDefinitionPointer == BabyTurtleDefinition:
                    RequireBabyTurtleState(slot).Function =
                        BabyTurtleAiFunction.SpinningStoppable;
                    cursor = unchecked((ushort)(cursor + 2));
                    break;
                case 0x94d1 when slot.EnemyDefinitionPointer is
                    MamaTurtleDefinition or BabyTurtleDefinition:
                    LastMamaTurtleSoundEffect = MamaTurtleSpinSound;
                    cursor = unchecked((ushort)(cursor + 2));
                    break;
                case DragonAnimationFinishedInstruction
                    when slot.EnemyDefinitionPointer == DragonDefinition:
                    FinishDragonAttackAnimation(slot);
                    cursor = unchecked((ushort)(cursor + 2));
                    break;
                case 0x8a8f when slot.EnemyDefinitionPointer == KraidArmDefinition:
                    cursor = SelectKraidArmSpeedInstruction(cursor);
                    break;
                case 0xb633 when slot.EnemyDefinitionPointer == KraidFootDefinition:
                case 0xb636 when slot.EnemyDefinitionPointer == KraidFootDefinition:
                case 0xb63c when slot.EnemyDefinitionPointer == KraidFootDefinition:
                case 0xb64e when slot.EnemyDefinitionPointer == KraidFootDefinition:
                case 0xb65a when slot.EnemyDefinitionPointer == KraidFootDefinition:
                case 0xb667 when slot.EnemyDefinitionPointer == KraidFootDefinition:
                case 0xb674 when slot.EnemyDefinitionPointer == KraidFootDefinition:
                case 0xb683 when slot.EnemyDefinitionPointer == KraidFootDefinition:
                    ProcessKraidFootInstruction(word, level);
                    cursor = unchecked((ushort)(cursor + 2));
                    break;
                case 0x8108: // EnemyInstr_DecrementTimerAndGoto.
                case 0x8110: // Byte-for-byte duplicate used by Bull's shot animation.
                    slot.Timer = unchecked((ushort)(slot.Timer - 1));
                    cursor = slot.Timer != 0
                        ? ReadWord(
                            _bus!,
                            (slot.Definition.Bank << 16) | unchecked((ushort)(cursor + 2)))
                        : unchecked((ushort)(cursor + 4));
                    break;
                case 0x8123: // EnemyInstr_SetTimer: operand is a literal loop count.
                    slot.Timer = ReadWord(
                        _bus!,
                        (slot.Definition.Bank << 16) | unchecked((ushort)(cursor + 2)));
                    cursor = unchecked((ushort)(cursor + 4));
                    break;
                case 0x808a when IsPhantoonPartDefinition(slot.EnemyDefinitionPointer):
                {
                    ushort function = ReadWord(
                        _bus!,
                        (slot.Definition.Bank << 16) | unchecked((ushort)(cursor + 2)));
                    bool stop = ProcessPhantoonInstructionFunction(
                        slot,
                        function,
                        nmiFrameCounter8);
                    if (stop)
                        return;
                    cursor = unchecked((ushort)(cursor + 4));
                    break;
                }
                case 0x813a: // EnemyInstr_WaitYFrames: delay without changing the map.
                    slot.InstructionTimer = ReadWord(
                        _bus!,
                        (slot.Definition.Bank << 16) | unchecked((ushort)(cursor + 2)));
                    slot.CurrentInstruction = unchecked((ushort)(cursor + 4));
                    return;
                case 0x814b: // EnemyInstr_CopyToVram: packed seven-byte DMA descriptor.
                {
                    int descriptor = (slot.Definition.Bank << 16) |
                        unchecked((ushort)(cursor + 2));
                    ushort byteCount = ReadWord(_bus!, descriptor);
                    int sourceAddress = _bus!.ReadByte(descriptor + 2) |
                        (_bus.ReadByte(descriptor + 3) << 8) |
                        (_bus.ReadByte(descriptor + 4) << 16);
                    ushort vramDestination = unchecked((ushort)(
                        _bus.ReadByte(descriptor + 5) |
                        (_bus.ReadByte(descriptor + 6) << 8)));
                    var bytes = new byte[byteCount];
                    for (int byteIndex = 0; byteIndex < bytes.Length; byteIndex++)
                        bytes[byteIndex] = _bus.ReadByte(sourceAddress + byteIndex);
                    _vram!.LoadBytes(vramDestination * 2, bytes);

                    // The descriptor is seven bytes rather than words. The next command is
                    // therefore at opcode+2+7, an odd bank address used intentionally by
                    // the Torizo crumbling-statue stream.
                    cursor = unchecked((ushort)(cursor + 9));
                    break;
                }
                case >= 0x8000 when TryProcessMotherBrainInstruction(
                    slot,
                    samus,
                    word,
                    ref cursor):
                    break;
                case >= 0x8000 when TryProcessDraygonInstruction(
                    slot,
                    samus,
                    word,
                    ref cursor):
                    break;
                case >= 0x8000 when TryProcessWallSpacePirateInstruction(
                    slot,
                    level,
                    word,
                    ref cursor):
                    break;
                case >= 0x8000 when TryProcessNinjaSpacePirateInstruction(
                    slot,
                    samus,
                    word,
                    ref cursor):
                    break;
                case >= 0x8000 when TryProcessWorkRobotInstruction(
                    slot,
                    samus,
                    level,
                    word,
                    ref cursor,
                    cameraX,
                    cameraY):
                    break;
                case 0xb75e when slot.EnemyDefinitionPointer == BeetomDefinition:
                    // Beetom's initial drain animation calls a literal RTS stub before it
                    // falls through into the looping blood-spray frames. It consumes no
                    // operand and changes no state beyond advancing the instruction cursor.
                    cursor = unchecked((ushort)(cursor + 2));
                    break;
                case 0xa0c7 when slot.EnemyDefinitionPointer == YappingMawDefinition:
                    SetYappingMawHeldOffset(RequireYappingMawState(slot), directionIndex: 1);
                    cursor = unchecked((ushort)(cursor + 2));
                    break;
                case 0xa0d9 when slot.EnemyDefinitionPointer == YappingMawDefinition:
                    SetYappingMawHeldOffset(RequireYappingMawState(slot), directionIndex: 7);
                    cursor = unchecked((ushort)(cursor + 2));
                    break;
                case 0xa0eb when slot.EnemyDefinitionPointer == YappingMawDefinition:
                    SetYappingMawHeldOffset(RequireYappingMawState(slot), directionIndex: 3);
                    cursor = unchecked((ushort)(cursor + 2));
                    break;
                case 0xa0fd when slot.EnemyDefinitionPointer == YappingMawDefinition:
                    SetYappingMawHeldOffset(RequireYappingMawState(slot), directionIndex: 5);
                    cursor = unchecked((ushort)(cursor + 2));
                    break;
                case 0xa10f when slot.EnemyDefinitionPointer == YappingMawDefinition:
                    SetYappingMawHeldOffset(RequireYappingMawState(slot), directionIndex: 0);
                    cursor = unchecked((ushort)(cursor + 2));
                    break;
                case 0xa121 when slot.EnemyDefinitionPointer == YappingMawDefinition:
                    SetYappingMawHeldOffset(RequireYappingMawState(slot), directionIndex: 4);
                    cursor = unchecked((ushort)(cursor + 2));
                    break;
                case 0xa133 when slot.EnemyDefinitionPointer == YappingMawDefinition:
                    PlayYappingMawAttackSound(RequireYappingMawState(slot));
                    cursor = unchecked((ushort)(cursor + 2));
                    break;
                case 0xa095 when slot.EnemyDefinitionPointer == CacatacDefinition:
                    RestoreCacatacPatrol(RequireCacatacState(slot));
                    cursor = unchecked((ushort)(cursor + 2));
                    break;
                case 0x9f2a when slot.EnemyDefinitionPointer == CacatacDefinition:
                    PlayCacatacSpikeSound();
                    cursor = unchecked((ushort)(cursor + 2));
                    break;
                case 0xa0a7 when slot.EnemyDefinitionPointer == CacatacDefinition:
                    SpawnCacatacSpike(
                        slot,
                        ReadWord(
                            _bus!,
                            (slot.Definition.Bank << 16) |
                            unchecked((ushort)(cursor + 2))));
                    cursor = unchecked((ushort)(cursor + 4));
                    break;
                case 0xa56d when slot.EnemyDefinitionPointer == OwtchDefinition:
                    SetOwtchMovingLeft(RequireOwtchState(slot));
                    cursor = unchecked((ushort)(cursor + 2));
                    break;
                case 0xa571 when slot.EnemyDefinitionPointer == OwtchDefinition:
                    SetOwtchMovingRight(RequireOwtchState(slot));
                    cursor = unchecked((ushort)(cursor + 2));
                    break;
                case 0x9625 when IsFuneNamiheDefinition(slot.EnemyDefinitionPointer):
                    // Shared mouth animation queues library-two sound $1F.
                    QueueFuneNamiheSpitSound();
                    cursor = unchecked((ushort)(cursor + 2));
                    break;
                case 0x878f when slot.EnemyDefinitionPointer == EvirProjectileDefinition:
                    QueueEvirSpitSound();
                    cursor = unchecked((ushort)(cursor + 2));
                    break;
                case 0x879b when slot.EnemyDefinitionPointer == EvirProjectileDefinition:
                    SetInitialEvirRegenerationOffset(slot, RequireEvirState(slot));
                    cursor = unchecked((ushort)(cursor + 2));
                    break;
                case 0x87b6 when slot.EnemyDefinitionPointer == EvirProjectileDefinition:
                    AdvanceEvirRegenerationOffset(slot, RequireEvirState(slot));
                    cursor = unchecked((ushort)(cursor + 2));
                    break;
                case 0x87cb when slot.EnemyDefinitionPointer == EvirProjectileDefinition:
                    FinishEvirRegeneration(RequireEvirState(slot));
                    cursor = unchecked((ushort)(cursor + 2));
                    break;
                case 0x9631 when IsFuneNamiheDefinition(slot.EnemyDefinitionPointer):
                    SpawnFuneNamiheFireball(
                        slot,
                        movingRight: false,
                        RoomEnemyProjectileKind.NamiheFireball);
                    cursor = unchecked((ushort)(cursor + 2));
                    break;
                case 0x964a when IsFuneNamiheDefinition(slot.EnemyDefinitionPointer):
                    SpawnFuneNamiheFireball(
                        slot,
                        movingRight: true,
                        RoomEnemyProjectileKind.NamiheFireball);
                    cursor = unchecked((ushort)(cursor + 2));
                    break;
                case 0x9663 when IsFuneNamiheDefinition(slot.EnemyDefinitionPointer):
                    SpawnFuneNamiheFireball(
                        slot,
                        movingRight: false,
                        RoomEnemyProjectileKind.FuneFireball);
                    cursor = unchecked((ushort)(cursor + 2));
                    break;
                case 0x967c when IsFuneNamiheDefinition(slot.EnemyDefinitionPointer):
                    SpawnFuneNamiheFireball(
                        slot,
                        movingRight: true,
                        RoomEnemyProjectileKind.FuneFireball);
                    cursor = unchecked((ushort)(cursor + 2));
                    break;
                case 0x9695 when IsFuneNamiheDefinition(slot.EnemyDefinitionPointer):
                case 0x96b4 when IsFuneNamiheDefinition(slot.EnemyDefinitionPointer):
                    // Left/right lists use duplicate opcodes with byte-for-byte state effects.
                    FinishFuneNamiheActivity(RequireFuneNamiheState(slot));
                    cursor = unchecked((ushort)(cursor + 2));
                    break;
                case 0xcc36 when slot.EnemyDefinitionPointer == YardDefinition:
                    // Yard animation bytecode owns movement dispatch. The word after the
                    // opcode is a same-bank function pointer, not a branch destination.
                    RequireYardState(slot).MovementFunction = (YardMovementFunction)ReadWord(
                        _bus!,
                        (slot.Definition.Bank << 16) | unchecked((ushort)(cursor + 2)));
                    cursor = unchecked((ushort)(cursor + 4));
                    break;
                case 0xcc3f when slot.EnemyDefinitionPointer == YardDefinition:
                    RequireYardState(slot).HidingInstructionList = ReadWord(
                        _bus!,
                        (slot.Definition.Bank << 16) | unchecked((ushort)(cursor + 2)));
                    cursor = unchecked((ushort)(cursor + 4));
                    break;
                case 0xcc48 when slot.EnemyDefinitionPointer == YardDefinition:
                {
                    YardEnemyState yard = RequireYardState(slot);
                    yard.Direction = ReadWord(
                        _bus!,
                        (slot.Definition.Bank << 16) | unchecked((ushort)(cursor + 2)));
                    if (yard.Direction >= 8)
                    {
                        throw new InvalidDataException(
                            $"Yard instruction selected invalid direction {yard.Direction}.");
                    }
                    yard.AirborneFacingDirection = ReadWord(
                        _bus!,
                        YardDirectionData + yard.Direction * 8 + 6);
                    cursor = unchecked((ushort)(cursor + 4));
                    break;
                }
                case 0xcc5f when slot.EnemyDefinitionPointer == YardDefinition:
                    slot.XPosition = unchecked((ushort)(slot.XPosition + ReadWord(
                        _bus!,
                        (slot.Definition.Bank << 16) | unchecked((ushort)(cursor + 2)))));
                    slot.YPosition = unchecked((ushort)(slot.YPosition + ReadWord(
                        _bus!,
                        (slot.Definition.Bank << 16) | unchecked((ushort)(cursor + 4)))));
                    cursor = unchecked((ushort)(cursor + 6));
                    break;
                case 0xcc78 when slot.EnemyDefinitionPointer == YardDefinition:
                    // The native instruction receives Y already advanced past the opcode.
                    // Subtracting six therefore resumes four bytes before the opcode.
                    cursor = RequireYardState(slot).Behavior == 2 || (_nextRandom!() & 1) != 0
                        ? unchecked((ushort)(cursor - 4))
                        : unchecked((ushort)(cursor + 2));
                    break;
                case 0xaa68 when IsHopperDefinition(slot.EnemyDefinitionPointer):
                    // Sidehopper's list passes a library-two sound operand, then the native
                    // instruction returns the cursor after that operand. Audio playback is
                    // an outer concern; publishing the exact word keeps the event observable.
                    LastHopperSoundEffect = ReadWord(
                        _bus!,
                        (slot.Definition.Bank << 16) | unchecked((ushort)(cursor + 2)));
                    cursor = unchecked((ushort)(cursor + 4));
                    break;
                case 0xaafe when IsHopperDefinition(slot.EnemyDefinitionPointer):
                    RequireHopperState(slot).ReadyToHop = true;
                    cursor = unchecked((ushort)(cursor + 2));
                    break;
                case 0xb429 when slot.EnemyDefinitionPointer == ZoaDefinition:
                    RequireZoaState(slot).XSpeedTableIndex = 4;
                    cursor = unchecked((ushort)(cursor + 2));
                    break;
                case 0xb434 when slot.EnemyDefinitionPointer == ZoaDefinition:
                    RequireZoaState(slot).XSpeedTableIndex = 8;
                    cursor = unchecked((ushort)(cursor + 2));
                    break;
                case 0xb43f when slot.EnemyDefinitionPointer == ZoaDefinition:
                    RequireZoaState(slot).XSpeedTableIndex = 12;
                    cursor = unchecked((ushort)(cursor + 2));
                    break;
                case 0xeaa5 when slot.EnemyDefinitionPointer == MetroidDefinition:
                    // The attached loop emits library-two sound $50 without consuming
                    // an operand; animation parsing resumes at the following word.
                    LastMetroidSoundEffectLibrary2 = MetroidAnimationSoundEffect;
                    cursor = unchecked((ushort)(cursor + 2));
                    break;
                case 0xeab1 when slot.EnemyDefinitionPointer == MetroidDefinition:
                    // Eight ROM words supply the idle cry. GenerateRandomNumber advances
                    // exactly once and the low three result bits choose the table entry.
                    int soundIndex = _nextRandom!() & 7;
                    LastMetroidSoundEffectLibrary2 = ReadWord(
                        _bus!,
                        MetroidRandomSoundTable + soundIndex * 2);
                    cursor = unchecked((ushort)(cursor + 2));
                    break;
                case 0xe660: // Shared crawler: install the function pointer operand.
                    RequireCrawlerState(slot).Function = (CrawlerEnemyFunction)ReadWord(
                        _bus!,
                        (slot.Definition.Bank << 16) | unchecked((ushort)(cursor + 2)));
                    cursor = unchecked((ushort)(cursor + 4));
                    break;
                case 0xdfc2 when slot.EnemyDefinitionPointer == HZoomerDefinition:
                    RequireCrawlerState(slot).Function = (CrawlerEnemyFunction)ReadWord(
                        _bus!,
                        (slot.Definition.Bank << 16) | unchecked((ushort)(cursor + 2)));
                    cursor = unchecked((ushort)(cursor + 4));
                    break;
                case 0xc6a4: // Skree: the preparation animation releases the dive AI.
                    RequireSkreeState(slot).AttackReady = true;
                    cursor = unchecked((ushort)(cursor + 2));
                    break;
                case 0x86e3 when slot.EnemyDefinitionPointer == WaverDefinition:
                    // The four-frame spin list hands its completion back to main AI rather
                    // than branching directly to steady art.
                    RequireWaverState(slot).SpinFinished = true;
                    cursor = unchecked((ushort)(cursor + 2));
                    break;
                case 0x8956 when slot.EnemyDefinitionPointer == MetareeDefinition:
                    // The preparation list sleeps immediately after publishing this flag.
                    // Main AI consumes it on the following enemy frame and installs the
                    // launched list, exactly matching the native instruction/AI hand-off.
                    RequireMetareeState(slot).AttackReady = true;
                    cursor = unchecked((ushort)(cursor + 2));
                    break;
                case 0x9096 when slot.EnemyDefinitionPointer == SkulteraDefinition:
                    // The right-facing steady list promotes the fish above foreground
                    // scenery only after its first instruction tick, exactly like the ROM.
                    slot.Layer = 6;
                    cursor = unchecked((ushort)(cursor + 2));
                    break;
                case 0x90a0 when slot.EnemyDefinitionPointer == SkulteraDefinition:
                    // The left-facing steady list draws below the matching scenery layer.
                    slot.Layer = 2;
                    cursor = unchecked((ushort)(cursor + 2));
                    break;
                case 0x90aa when slot.EnemyDefinitionPointer == SkulteraDefinition:
                    // Turning lists sleep immediately after publishing this flag. Main AI
                    // consumes it on the following frame and installs steady facing art.
                    RequireSkulteraState(slot).TurnFinished = true;
                    cursor = unchecked((ushort)(cursor + 2));
                    break;
                case 0x9c6b when IsPlatformDefinition(slot.EnemyDefinitionPointer):
                case 0x9c81 when IsPlatformDefinition(slot.EnemyDefinitionPointer):
                    // The "ordinary" and duplicate commands are byte-for-byte equivalent:
                    // both publish horizontal dispatcher index zero before the next frame.
                    SetPlatformHorizontalMovementFromInstruction(
                        slot,
                        PlatformHorizontalMovement.Left);
                    cursor = unchecked((ushort)(cursor + 2));
                    break;
                case 0x9c76 when IsPlatformDefinition(slot.EnemyDefinitionPointer):
                case 0x9c8c when IsPlatformDefinition(slot.EnemyDefinitionPointer):
                    SetPlatformHorizontalMovementFromInstruction(
                        slot,
                        PlatformHorizontalMovement.Right);
                    cursor = unchecked((ushort)(cursor + 2));
                    break;
                case 0xdf1c when slot.EnemyDefinitionPointer == AlcoonDefinition:
                    SpawnAlcoonFireball(slot, yVelocityTableByteOffset: 0);
                    LastAlcoonSoundEffect = AlcoonFireSound;
                    cursor = unchecked((ushort)(cursor + 2));
                    break;
                case 0xdf33 when slot.EnemyDefinitionPointer == AlcoonDefinition:
                    SpawnAlcoonFireball(slot, yVelocityTableByteOffset: 2);
                    LastAlcoonSoundEffect = AlcoonFireSound;
                    cursor = unchecked((ushort)(cursor + 2));
                    break;
                case 0xdf39 when slot.EnemyDefinitionPointer == AlcoonDefinition:
                    SpawnAlcoonFireball(slot, yVelocityTableByteOffset: 4);
                    LastAlcoonSoundEffect = AlcoonFireSound;
                    cursor = unchecked((ushort)(cursor + 2));
                    break;
                case 0xdf3f when slot.EnemyDefinitionPointer == AlcoonDefinition:
                    cursor = StartAlcoonWalking(slot, RequireAlcoonState(slot));
                    break;
                case 0xdf63 when slot.EnemyDefinitionPointer == AlcoonDefinition:
                    cursor = MoveAlcoonHorizontally(
                        slot,
                        RequireAlcoonState(slot),
                        level,
                        unchecked((ushort)(cursor + 2)),
                        decrementStepCounter: true);
                    break;
                case 0xdf71 when slot.EnemyDefinitionPointer == AlcoonDefinition:
                    cursor = MoveAlcoonHorizontally(
                        slot,
                        RequireAlcoonState(slot),
                        level,
                        unchecked((ushort)(cursor + 2)),
                        decrementStepCounter: false);
                    break;
                case 0xf526 when IsKiHunterBodyDefinition(slot.EnemyDefinitionPointer):
                    // The callback returns a direct body-list pointer while separately
                    // restarting the following attached wing list.
                    cursor = ReturnKiHunterToSteadyInstruction(slot);
                    break;
                case 0xf5e4 when IsKiHunterBodyDefinition(slot.EnemyDefinitionPointer):
                    StartKiHunterGroundJumpFromInstruction(RequireKiHunterState(slot));
                    cursor = unchecked((ushort)(cursor + 2));
                    break;
                case 0xf67f when IsKiHunterBodyDefinition(slot.EnemyDefinitionPointer):
                    StartKiHunterGroundWaitFromInstruction(RequireKiHunterState(slot));
                    cursor = unchecked((ushort)(cursor + 2));
                    break;
                case 0xf6d2 when IsKiHunterBodyDefinition(slot.EnemyDefinitionPointer):
                    SpawnKiHunterAcidFromInstruction(slot, movingRight: false);
                    cursor = unchecked((ushort)(cursor + 2));
                    break;
                case 0xf6d8 when IsKiHunterBodyDefinition(slot.EnemyDefinitionPointer):
                    SpawnKiHunterAcidFromInstruction(slot, movingRight: true);
                    cursor = unchecked((ushort)(cursor + 2));
                    break;
                case 0xe4be: // Ridley_Instr_5: set roaring flag and QueueSfx2_Max6($59).
                    RequireRidley(slot).Roaring = true;
                    QueueEnemySound(library: 2, soundId: 0x0059, maximumQueued: 6);
                    cursor = unchecked((ushort)(cursor + 2));
                    break;
                case 0xe61d when slot.EnemyDefinitionPointer == SparkDefinition:
                    // Spark flicker-out command: property bit $0400 removes the actor from
                    // every ordinary Samus, beam, and grapple collision pass.
                    slot.Properties = slot.Properties.With(EnemyProperties.IgnoreSamusCollision);
                    cursor = unchecked((ushort)(cursor + 2));
                    break;
                case 0xe62a when slot.EnemyDefinitionPointer == SparkDefinition:
                    // Spark flicker-on command executes before the first visible activation
                    // frame, so collision and art become live together.
                    slot.Properties = slot.Properties.Without(EnemyProperties.IgnoreSamusCollision);
                    cursor = unchecked((ushort)(cursor + 2));
                    break;
                case 0x8daf when slot.EnemyDefinitionPointer == HibashiDefinition:
                    PlayHibashiEruptionSound();
                    cursor = unchecked((ushort)(cursor + 2));
                    break;
                case 0x8e13 when slot.EnemyDefinitionPointer == HibashiDefinition:
                case 0x8e2d when slot.EnemyDefinitionPointer == HibashiDefinition:
                case 0x8e41 when slot.EnemyDefinitionPointer == HibashiDefinition:
                case 0x8e55 when slot.EnemyDefinitionPointer == HibashiDefinition:
                case 0x8e69 when slot.EnemyDefinitionPointer == HibashiDefinition:
                case 0x8e7d when slot.EnemyDefinitionPointer == HibashiDefinition:
                case 0x8e91 when slot.EnemyDefinitionPointer == HibashiDefinition:
                case 0x8ea5 when slot.EnemyDefinitionPointer == HibashiDefinition:
                case 0x8eb9 when slot.EnemyDefinitionPointer == HibashiDefinition:
                case 0x8ecd when slot.EnemyDefinitionPointer == HibashiDefinition:
                case 0x8ee1 when slot.EnemyDefinitionPointer == HibashiDefinition:
                case 0x8ef5 when slot.EnemyDefinitionPointer == HibashiDefinition:
                case 0x8f09 when slot.EnemyDefinitionPointer == HibashiDefinition:
                case 0x8f1d when slot.EnemyDefinitionPointer == HibashiDefinition:
                case 0x8f31 when slot.EnemyDefinitionPointer == HibashiDefinition:
                case 0x8f45 when slot.EnemyDefinitionPointer == HibashiDefinition:
                case 0x8f59 when slot.EnemyDefinitionPointer == HibashiDefinition:
                case 0x8f6d when slot.EnemyDefinitionPointer == HibashiDefinition:
                case 0x8f81 when slot.EnemyDefinitionPointer == HibashiDefinition:
                case 0x8f95 when slot.EnemyDefinitionPointer == HibashiDefinition:
                case 0x8fa9 when slot.EnemyDefinitionPointer == HibashiDefinition:
                case 0x8fbd when slot.EnemyDefinitionPointer == HibashiDefinition:
                    // These 22 bank-$A6 routines are laid out at an exact $14-byte stride.
                    // Each routine selects the correspondingly indexed Y-offset/radius pair;
                    // deriving that index from the executed ROM address keeps the dispatcher
                    // visibly tied to native layout and avoids another 22-arm magic mapping.
                    ApplyHibashiActivityFrame(slot, (word - 0x8e13) / 0x14);
                    cursor = unchecked((ushort)(cursor + 2));
                    break;
                case 0x8fd1 when slot.EnemyDefinitionPointer == HibashiDefinition:
                    FinishHibashiActivity(slot);
                    cursor = unchecked((ushort)(cursor + 2));
                    break;
                case 0x9b26 when slot.EnemyDefinitionPointer == FakeKraidDefinition:
                    // Walk opcode: advance one four-pixel step unless its random reversal
                    // clock expires, then refresh the live facing marker from Samus.
                    ProcessFakeKraidWalkInstruction(
                        slot,
                        RequireFakeKraidState(slot),
                        samus,
                        level);
                    cursor = unchecked((ushort)(cursor + 2));
                    break;
                case 0x9b74 when slot.EnemyDefinitionPointer == FakeKraidDefinition:
                    // Decision opcode returns a direct same-bank address. Several targets
                    // deliberately begin two bytes inside a named list to skip this opcode.
                    cursor = SelectFakeKraidInstruction(RequireFakeKraidState(slot));
                    break;
                case 0x9bb2 when slot.EnemyDefinitionPointer == FakeKraidDefinition:
                    // `$A6:9BB2` queues sound $16 only while the actor origin is inside the
                    // inclusive 256x256 native screen rectangle.
                    if (FakeKraidOriginIsOnScreen(slot, cameraX, cameraY))
                        LastFakeKraidSoundEffect = FakeKraidSpitSound;
                    cursor = unchecked((ushort)(cursor + 2));
                    break;
                case 0x9bc4 when slot.EnemyDefinitionPointer == FakeKraidDefinition:
                    SpawnFakeKraidSpitPair(
                        slot,
                        RequireFakeKraidState(slot),
                        movingRight: false);
                    cursor = unchecked((ushort)(cursor + 2));
                    break;
                case 0x9c02 when slot.EnemyDefinitionPointer == FakeKraidDefinition:
                    SpawnFakeKraidSpitPair(
                        slot,
                        RequireFakeKraidState(slot),
                        movingRight: true);
                    cursor = unchecked((ushort)(cursor + 2));
                    break;
                case 0xfcb8 when IsWalkingSpacePirateDefinition(slot.EnemyDefinitionPointer):
                    // Pirate bytecode does not jump to the operand. It stores that bank-$B2
                    // function address in native variable A for main AI to dispatch next
                    // frame, then resumes immediately after the two-byte operand.
                    RequireWalkingSpacePirateState(slot).Function =
                        (WalkingSpacePirateFunction)ReadWord(
                            _bus!,
                            (slot.Definition.Bank << 16) |
                            unchecked((ushort)(cursor + 2)));
                    cursor = unchecked((ushort)(cursor + 4));
                    break;
                case 0xfc68 when IsWalkingSpacePirateDefinition(slot.EnemyDefinitionPointer):
                    SpawnWalkingSpacePirateLaser(
                        slot,
                        RequireWalkingSpacePirateState(slot),
                        movingRight: false,
                        ReadWord(
                            _bus!,
                            (slot.Definition.Bank << 16) |
                            unchecked((ushort)(cursor + 2))));
                    cursor = unchecked((ushort)(cursor + 4));
                    break;
                case 0xfc90 when IsWalkingSpacePirateDefinition(slot.EnemyDefinitionPointer):
                    SpawnWalkingSpacePirateLaser(
                        slot,
                        RequireWalkingSpacePirateState(slot),
                        movingRight: true,
                        ReadWord(
                            _bus!,
                            (slot.Definition.Bank << 16) |
                            unchecked((ushort)(cursor + 2))));
                    cursor = unchecked((ushort)(cursor + 4));
                    break;
                case 0xfcc8 when IsWalkingSpacePirateDefinition(slot.EnemyDefinitionPointer):
                    // Unlike common goto, this opcode returns a direct instruction-list
                    // pointer chosen from Samus's current side and vertical proximity.
                    cursor = SelectWalkingSpacePirateMovement(slot, samus);
                    break;
                case 0xe4ca: // Ridley: close mouth / clear the roaring presentation flag.
                    RequireRidley(slot).Roaring = false;
                    cursor = unchecked((ushort)(cursor + 2));
                    break;
                case 0xe4d2: // Ridley: Ceres low-energy branch; area two always skips its operand.
                    if (slot.EnemyDefinitionPointer == NorfairRidleyDefinition)
                    {
                        cursor = unchecked((ushort)(cursor + 4));
                        break;
                    }
                    if (samus is null)
                    {
                        throw new InvalidOperationException(
                            "Ceres Ridley fireball branch requires the active Samus actor.");
                    }
                    cursor = unchecked((short)(samus.Health - 30)) < 0
                        ? ReadWord(
                            _bus!,
                            (slot.Definition.Bank << 16) | unchecked((ushort)(cursor + 2)))
                        : unchecked((ushort)(cursor + 4));
                    break;
                case 0xe4ee: // Ridley: choose one of two lists according to grabbed-Samus state.
                    RidleyEnemyState grabbedBranch = RequireRidley(slot);
                    ushort branchOperand = grabbedBranch.GrabState != 0
                        ? (ushort)2
                        : (ushort)4;
                    cursor = ReadWord(
                        _bus!,
                        (slot.Definition.Bank << 16) | unchecked((ushort)(cursor + branchOperand)));
                    break;
                case 0xe4f8: // Ridley: skip an operand while carrying Samus, otherwise branch.
                    RidleyEnemyState carryBranch = RequireRidley(slot);
                    cursor = carryBranch.GrabState != 0
                        ? unchecked((ushort)(cursor + 4))
                        : ReadWord(
                            _bus!,
                            (slot.Definition.Bank << 16) | unchecked((ushort)(cursor + 2)));
                    break;
                case 0xe501: // Ridley: select feet/hand-distance animation index operand.
                    RequireRidley(slot).FeetDistanceIndex = ReadWord(
                        _bus!,
                        (slot.Definition.Bank << 16) | unchecked((ushort)(cursor + 2)));
                    cursor = unchecked((ushort)(cursor + 4));
                    break;
                case 0xe517: // Ridley: branch to operand when he is not facing left.
                    RidleyEnemyState ridley = RequireRidley(slot);
                    cursor = ridley.FacingDirection != 0
                        ? ReadWord(
                            _bus!,
                            (slot.Definition.Bank << 16) | unchecked((ushort)(cursor + 2)))
                        : unchecked((ushort)(cursor + 4));
                    break;
                case 0xe51f: // Ridley: add the two signed pixel operands to his origin.
                    slot.XPosition = unchecked((ushort)(
                        slot.XPosition +
                        ReadWord(_bus!, (slot.Definition.Bank << 16) | unchecked((ushort)(cursor + 2)))));
                    slot.YPosition = unchecked((ushort)(
                        slot.YPosition +
                        ReadWord(_bus!, (slot.Definition.Bank << 16) | unchecked((ushort)(cursor + 4)))));
                    cursor = unchecked((ushort)(cursor + 6));
                    break;
                case 0xe71c: // Ridley: face left and mirror the shared tail workspace.
                    MirrorRidleyTail(RequireRidley(slot));
                    RequireRidley(slot).FacingDirection = 0;
                    cursor = unchecked((ushort)(cursor + 2));
                    break;
                case 0xe727: // Ridley: use the front-facing transition frame.
                    RequireRidley(slot).FacingDirection = 1;
                    cursor = unchecked((ushort)(cursor + 2));
                    break;
                case 0xe72f: // Ridley: face right and mirror the shared tail workspace.
                    MirrorRidleyTail(RequireRidley(slot));
                    RequireRidley(slot).FacingDirection = 2;
                    cursor = unchecked((ushort)(cursor + 2));
                    break;
                case 0xe84d: // Ridley: aim the next fireball from the facing-dependent mouth.
                    if (samus is null)
                    {
                        throw new InvalidOperationException(
                            "Ceres Ridley fireball aim requires the active Samus actor.");
                    }
                    CalculateRidleyFireballVelocity(slot, samus);
                    cursor = unchecked((ushort)(cursor + 2));
                    break;
                case 0xe904: // Ridley: spawn the leading fireball with a wall afterburn.
                    SpawnRidleyFireball(slot, spawnAfterburn: true);
                    cursor = unchecked((ushort)(cursor + 2));
                    break;
                case 0xe909: // Ridley: spawn a following fireball without an afterburn.
                    SpawnRidleyFireball(slot, spawnAfterburn: false);
                    cursor = unchecked((ushort)(cursor + 2));
                    break;
                case 0xe969: // Ceres Ridley: animation hands control to accelerating liftoff.
                    RidleyEnemyState liftoff = RequireCeresRidley(slot);
                    liftoff.Function = RidleyAiFunction.CeresLiftoffAccelerating;
                    liftoff.VerticalVelocity = unchecked((ushort)-352);
                    cursor = unchecked((ushort)(cursor + 2));
                    break;
                case 0xe976 when slot.EnemyDefinitionPointer == NorfairRidleyDefinition:
                    // The shared roar/liftoff list hands control to the real fight at
                    // $B2F3 and supplies the initial upward 8.8 velocity in the same tick.
                    RidleyEnemyState norfairLiftoff = RequireNorfairRidley(slot);
                    norfairLiftoff.Function = RidleyAiFunction.NorfairEnterArena;
                    norfairLiftoff.VerticalVelocity = unchecked((ushort)-352);
                    cursor = unchecked((ushort)(cursor + 2));
                    break;
                case 0xf11d: // Ceres steam: hide and exclude from interaction.
                    slot.Properties = slot.Properties.With(
                        EnemyProperties.Invisible | EnemyProperties.IgnoreSamusCollision);
                    cursor = unchecked((ushort)(cursor + 2));
                    break;
                case 0xf127: // Ceres steam: randomized dormant-loop branch.
                    slot.VariableD = unchecked((ushort)(slot.VariableD - 1));
                    if (slot.VariableD != 0)
                    {
                        cursor = ReadWord(
                            _bus!,
                            (slot.Definition.Bank << 16) | unchecked((ushort)(cursor + 2)));
                    }
                    else
                    {
                        cursor = ReadWord(
                            _bus!,
                            (slot.Definition.Bank << 16) | unchecked((ushort)(cursor + 4)));
                        slot.Properties = slot.Properties.Without(
                            EnemyProperties.Invisible | EnemyProperties.IgnoreSamusCollision);
                    }
                    break;
                case 0xf135: // Ceres steam: show and admit interaction.
                    slot.Properties = slot.Properties.Without(
                        EnemyProperties.Invisible | EnemyProperties.IgnoreSamusCollision);
                    cursor = unchecked((ushort)(cursor + 2));
                    break;
                case 0xf63e: // Ceres door: loop until Samus is within a 48x48-pixel box.
                    if (samus is null)
                    {
                        throw new InvalidOperationException(
                            "Ceres door proximity instruction requires the active Samus actor.");
                    }

                    // `$A6:F63E` subtracts the two unsigned position words, interprets the
                    // wrapped result as signed, then takes its absolute value independently
                    // on each axis. If either distance is at least $30, the operand is a
                    // same-bank loop target; otherwise execution skips that operand.
                    int xDistance = Math.Abs(unchecked((short)(slot.XPosition - samus.XPosition)));
                    int yDistance = Math.Abs(unchecked((short)(slot.YPosition - samus.YPosition)));
                    cursor = xDistance >= 0x30 || yDistance >= 0x30
                        ? ReadWord(
                            _bus!,
                            (slot.Definition.Bank << 16) | unchecked((ushort)(cursor + 2)))
                        : unchecked((ushort)(cursor + 4));
                    break;
                case 0xf66a: // Ceres door: branch while area boss bit one is clear.
                    // `$A6:F66A-$F676` samples bit zero of the current area's SRAM-mirror
                    // boss byte. Ceres begins with that bit clear, so the facing-right door
                    // loops as a tangible actor during Ridley's fight. `$A6:C117` publishes
                    // the boss bit together with status two; the next instruction pass must
                    // then skip the branch operand and reach `$F68B`'s intangible setup.
                    // Hardcoding the early-game branch stranded the invisible 8x32 actor at
                    // X=$0008 and clipped Samus at X=$001D after the getaway cutscene.
                    cursor = RequireAreaBossDefeated()
                        ? unchecked((ushort)(cursor + 4))
                        : ReadWord(
                            _bus!,
                            (slot.Definition.Bank << 16) | unchecked((ushort)(cursor + 2)));
                    break;
                case 0xf678: // Ceres door: branch while ceres_status is zero.
                    cursor = CeresStatus != 0
                        ? unchecked((ushort)(cursor + 4))
                        : ReadWord(
                            _bus!,
                            (slot.Definition.Bank << 16) | unchecked((ushort)(cursor + 2)));
                    break;
                case 0xf68b: // Ceres steam/door: set native property bit $0400.
                    slot.Properties = slot.Properties.With(EnemyProperties.IgnoreSamusCollision);
                    cursor = unchecked((ushort)(cursor + 2));
                    break;
                case 0xf695: // Ceres door: clear native property bit $0400.
                    slot.Properties = slot.Properties.Without(EnemyProperties.IgnoreSamusCollision);
                    cursor = unchecked((ushort)(cursor + 2));
                    break;
                case 0xf69f: // Ceres door: publish animation-state word B=1.
                    slot.VariableB = 1;
                    cursor = unchecked((ushort)(cursor + 2));
                    break;
                case 0xf6a6: // Ceres door/steam: hide the actor.
                    slot.Properties = slot.Properties.With(EnemyProperties.Invisible);
                    cursor = unchecked((ushort)(cursor + 2));
                    break;
                case 0xf6b0: // Ceres door: state B=0, then show the actor.
                    slot.VariableB = 0;
                    slot.Properties = slot.Properties.Without(EnemyProperties.Invisible);
                    cursor = unchecked((ushort)(cursor + 2));
                    break;
                case 0xf6b3: // Ceres door: show the actor.
                    slot.Properties = slot.Properties.Without(EnemyProperties.Invisible);
                    cursor = unchecked((ushort)(cursor + 2));
                    break;
                case 0xf6bd: // CeresDoor_Instr_7: QueueSfx3_Max6($2C).
                    QueueEnemySound(library: 3, soundId: 0x002c, maximumQueued: 6);
                    cursor = unchecked((ushort)(cursor + 2));
                    break;
                case 0xecd0 when slot.EnemyDefinitionPointer == DeadSidehopperDefinition:
                    // `$A9:ECD0` is embedded at the end of the landing animation. It
                    // hands ownership back to main AI without consuming an operand.
                    SelectDeadSidehopperPostAnimationState(slot);
                    cursor = unchecked((ushort)(cursor + 2));
                    break;
                case 0xf920 when slot.EnemyDefinitionPointer == ShitroidDefinition:
                    // Instruction 3 returns the calm animation list directly; it does not
                    // consume an operand from the calling list.
                    cursor = 0xf90e;
                    break;
                case 0xf936 when slot.EnemyDefinitionPointer == ShitroidDefinition:
                    // Instruction 4 restarts the aggressive/draining loop.
                    cursor = 0xf924;
                    break;
                case 0xf990 when slot.EnemyDefinitionPointer == ShitroidDefinition:
                    // Instruction 6 restarts the departure loop.
                    cursor = 0xf93a;
                    break;
                case 0xf994 when slot.EnemyDefinitionPointer == ShitroidDefinition:
                    // Instruction 5 samples the existing RNG word; it does not generate a
                    // new value. Clear high bit takes the same-bank operand branch. Set high
                    // bit plays cry $52 and falls through beyond that operand.
                    if ((RequireRandomNumber() & 0x8000) == 0)
                    {
                        cursor = ReadWord(
                            _bus!,
                            (slot.Definition.Bank << 16) |
                                unchecked((ushort)(cursor + 2)));
                    }
                    else
                    {
                        LastShitroidSoundEffectLibrary2 = 0x0052;
                        cursor = unchecked((ushort)(cursor + 4));
                    }
                    break;
                default:
                    if (TryProcessSporeSpawnInstruction(slot, word, ref cursor))
                        break;
                    if (TryProcessCrocomireInstruction(
                            slot,
                            samus,
                            level,
                            word,
                            ref cursor,
                            cameraX))
                    {
                        break;
                    }
                    if (TryProcessRinkaInstruction(slot, word, ref cursor))
                        break;
                    if (TryProcessRioInstruction(slot, word, ref cursor))
                        break;
                    if (TryProcessNorfairLavaJumpingEnemyInstruction(slot, word, ref cursor))
                        break;
                    if (TryProcessNorfairRioInstruction(slot, word, ref cursor))
                        break;
                    if (TryProcessLowerNorfairRioInstruction(slot, word, ref cursor))
                        break;
                    if (TryProcessMaridiaLargeSnailInstruction(slot, word, ref cursor))
                        break;
                    if (TryProcessMagdolliteInstruction(
                        slot,
                        word,
                        ref cursor,
                        cameraX,
                        cameraY))
                    {
                        break;
                    }
                    if (TryProcessBotwoonInstruction(slot, word, ref cursor))
                        break;
                    if (TryProcessShaktoolInstruction(slot, word, ref cursor))
                        break;
                    if (TryProcessChozoStatueInstruction(
                            slot,
                            samus,
                            level,
                            word,
                            ref cursor))
                    {
                        break;
                    }
                    if (TryProcessEscapeAnimalInstruction(slot, samus, word, ref cursor))
                        break;
                    if (TryProcessBombTorizoInstruction(
                            slot,
                            samus,
                            level,
                            word,
                            ref cursor,
                            controllerInput,
                            nmiFrameCounter8,
                            out bool pauseBombTorizoInterpreter))
                    {
                        if (pauseBombTorizoInterpreter)
                            return;
                        break;
                    }
                    throw new InvalidDataException(
                        $"Enemy ${slot.EnemyDefinitionPointer:X4} instruction " +
                        $"${slot.Definition.Bank:X2}:{cursor:X4} opcode ${word:X4} is not translated.");
            }
        }

        throw new InvalidDataException(
            $"Enemy ${slot.EnemyDefinitionPointer:X4} instruction list exceeded 64 commands without a frame.");
    }

    private void DetermineWhichEnemiesToProcess(ushort cameraX, ushort cameraY)
    {
        _activeEnemyIndexes.Clear();
        _interactiveEnemyIndexes.Clear();
        foreach (RoomEnemySlot slot in _slots)
        {
            if (slot.EnemyDefinitionPointer is 0 or 0xdaff)
                continue;
            if (slot.Properties.HasAny(EnemyProperties.Deleted))
            {
                slot.EnemyDefinitionPointer = 0;
                continue;
            }

            bool active = _processAllEnemies ||
                slot.Properties.HasAny(EnemyProperties.ProcessOffScreen) ||
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

    /// <summary>
    /// Parses one complete 64-byte enemy header from the fixed bank-$A0 definition table.
    /// Keeping this reader public lets debugger tooling inspect unsupported actors without
    /// pretending their initialization or main AI has already been translated.
    /// </summary>
    public static RoomEnemyDefinition ReadDefinition(ISnesAddressSpace bus, ushort pointer)
    {
        ArgumentNullException.ThrowIfNull(bus);
        int address = EnemyDefinitionBank | pointer;
        return new RoomEnemyDefinition(
            TileDataSize: ReadWord(bus, address),
            PalettePointer: ReadWord(bus, AddWithinBank(address, 2)),
            Health: ReadWord(bus, AddWithinBank(address, 4)),
            Damage: ReadWord(bus, AddWithinBank(address, 6)),
            XRadius: ReadWord(bus, AddWithinBank(address, 8)),
            YRadius: ReadWord(bus, AddWithinBank(address, 10)),
            Bank: bus.ReadByte(AddWithinBank(address, 12)),
            HurtAiTime: bus.ReadByte(AddWithinBank(address, 13)),
            HurtSoundEffect: ReadWord(bus, AddWithinBank(address, 14)),
            BossId: ReadWord(bus, AddWithinBank(address, 16)),
            InitializationAiPointer: ReadWord(bus, AddWithinBank(address, 18)),
            PartCount: ReadWord(bus, AddWithinBank(address, 20)),
            Unused16: ReadWord(bus, AddWithinBank(address, 22)),
            MainAiPointer: ReadWord(bus, AddWithinBank(address, 24)),
            GrappleAiPointer: ReadWord(bus, AddWithinBank(address, 26)),
            HurtAiPointer: ReadWord(bus, AddWithinBank(address, 28)),
            FrozenAiPointer: ReadWord(bus, AddWithinBank(address, 30)),
            TimeFrozenAiPointer: ReadWord(bus, AddWithinBank(address, 32)),
            DeathAnimation: ReadWord(bus, AddWithinBank(address, 34)),
            Unused24: ReadWord(bus, AddWithinBank(address, 36)),
            Unused26: ReadWord(bus, AddWithinBank(address, 38)),
            PowerBombReactionPointer: ReadWord(bus, AddWithinBank(address, 40)),
            VariantIndex: ReadWord(bus, AddWithinBank(address, 42)),
            Unused2C: ReadWord(bus, AddWithinBank(address, 44)),
            Unused2E: ReadWord(bus, AddWithinBank(address, 46)),
            TouchAiPointer: ReadWord(bus, AddWithinBank(address, 48)),
            ShotAiPointer: ReadWord(bus, AddWithinBank(address, 50)),
            InitialSpritemapPointer: ReadWord(bus, AddWithinBank(address, 52)),
            TileDataAddress: ReadLong(bus, AddWithinBank(address, 54)),
            Layer: bus.ReadByte(AddWithinBank(address, 57)),
            ItemDropChancesPointer: ReadWord(bus, AddWithinBank(address, 58)),
            VulnerabilityPointer: ReadWord(bus, AddWithinBank(address, 60)),
            NamePointer: ReadWord(bus, AddWithinBank(address, 62)));
    }

    private RoomEnemySpawnNameWords ReadSpawnNameWords(RoomEnemyDefinition definition)
    {
        if (definition.NamePointer == 0)
            return default;

        int address = 0xb40000 | definition.NamePointer;
        return new RoomEnemySpawnNameWords(
            ReadWord(_bus!, address),
            ReadWord(_bus!, AddWithinBank(address, 2)),
            ReadWord(_bus!, AddWithinBank(address, 4)),
            ReadWord(_bus!, AddWithinBank(address, 6)),
            ReadWord(_bus!, AddWithinBank(address, 8)),
            ReadWord(_bus!, AddWithinBank(address, 12)));
    }

    private static ushort ReadWord(ISnesAddressSpace bus, int address) =>
        (ushort)(bus.ReadByte(address) | (bus.ReadByte(AddWithinBank(address, 1)) << 8));

    private static int ReadLong(ISnesAddressSpace bus, int address) =>
        bus.ReadByte(address) |
        (bus.ReadByte(AddWithinBank(address, 1)) << 8) |
        (bus.ReadByte(AddWithinBank(address, 2)) << 16);

    private static bool IsNegative16(int value) => (short)unchecked((ushort)value) < 0;
}
