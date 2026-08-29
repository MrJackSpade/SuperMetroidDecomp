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
    private Func<bool>? _isAreaMiniBossDefeated;
    private Func<int, bool>? _hasEvent;
    private Action<int>? _setEvent;
    private Action<int>? _clearEvent;
    private Action? _setAreaMiniBossDefeated;
    private Action<ushort>? _setRandomNumber;
    private ushort _randomEnemyCounter;
    private SnesVram? _vram;
    private SnesCgram? _cgram;
    private CeresRidleyState? _ceresRidley;

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
    public ushort? LastAlcoonSoundEffect { get; private set; }
    public ushort? LastKzanSoundEffect { get; private set; }
    public ushort? LastHibashiSoundEffect { get; private set; }

    /// <summary>Last library-three sound requested by an attached Beetom this frame.</summary>
    public ushort? LastBeetomSoundEffect { get; private set; }
    public ushort FirefleaDarknessLevel { get; private set; }
    public ushort EarthquakeTimer { get; set; }
    public ushort EarthquakeType { get; set; }

    /// <summary>
    /// Ridley's bank-$A6 state extension while enemy $E13F owns slot zero. The native actor
    /// extends far beyond the common $40-byte enemy record, so exposing a deliberately named
    /// object is both more accurate and considerably easier to inspect than aliasing dozens
    /// of unrelated generic slot words.
    /// </summary>
    public CeresRidleyState? CeresRidley => _ceresRidley;

    /// <summary>
    /// Native <c>ceres_status</c> word consumed by the Ceres door actor. Fresh station load
    /// begins at zero; Ridley's escape sequence is the later producer of values one/two.
    /// </summary>
    public ushort CeresStatus { get; set; }

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
        Func<ushort, bool>? isRoomPlmPresent = null)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(vram);
        ArgumentNullException.ThrowIfNull(cgram);
        ArgumentNullException.ThrowIfNull(nextRandom);

        _bus = bus;
        _nextRandom = nextRandom;
        // Some enemy routines call GenerateRandomNumber while others, including Alcoon's
        // post-volley bytecode, merely sample the existing WRAM seed. Keep those operations
        // distinct: substituting nextRandom here would silently advance the cartridge RNG.
        _readRandomNumber = readRandomNumber;
        _setRandomNumber = setRandomNumber;
        _isAreaBossDefeated = isAreaBossDefeated;
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
        GunshipSavePromptPending = false;
        GunshipSaveRequested = false;
        LastBoyonSoundEffect = null;
        LastMamaTurtleSoundEffect = null;
        LastMochtroidSoundEffect = null;
        LastMetroidSoundEffectLibrary2 = null;
        LastMetroidSoundEffectLibrary3 = null;
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
        ResetRinkaRoomState(cameraX, cameraY);
        ResetRioRoomState();
        ResetNorfairLavaJumpingEnemyRoomState();
        ResetNorfairRioRoomState();
        ResetLowerNorfairRioRoomState();
        ResetMaridiaLargeSnailRoomState();
        ResetRipperVariantRoomState();
        ResetDragonRoomState();
        ResetShutterRoomState(cameraX, cameraY);
        ResetElevatorRoomActors();
        LastKzanSoundEffect = null;
        LastHibashiSoundEffect = null;
        LastNuclearWaffleSoundEffect = null;
        LastFakeKraidSoundEffect = null;
        LastFakeKraidDropRequest = null;
        LastEnemyProjectileDudSoundEffect = null;
        LastBeetomSoundEffect = null;
        LastWorkRobotSoundEffect = null;
        LastBotwoonSoundEffect = null;
        LastBotwoonDropRequest = null;
        FirefleaDarknessLevel = 0;
        EarthquakeTimer = 0;
        EarthquakeType = 0;
        _ceresRidley = null;
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
        LoadPopulation(bus, populationPointer, level, samus, controllerInput);
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
        byte? nmiFrameCounter8 = null)
    {
        EnsureLoaded();
        // Standalone audits do not own the runtime NMI clock. In that case the enemy-frame
        // counter begins at zero and advances at the same end-of-frame point, which gives
        // Mama Turtle's even-frame shell jitter the same initial phase as retail room load.
        byte enemyNmiFrameCounter8 = nmiFrameCounter8 ?? unchecked((byte)_randomEnemyCounter);
        LastGunshipEvent = GunshipFrameEvent.None;
        LastBoyonSoundEffect = null;
        LastMamaTurtleSoundEffect = null;
        LastCacatacSoundEffect = null;
        LastMochtroidSoundEffect = null;
        LastMetroidSoundEffectLibrary2 = null;
        LastMetroidSoundEffectLibrary3 = null;
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
        LastAlcoonSoundEffect = null;
        LastFuneNamiheSoundEffect = null;
        LastKagoBugSoundEffect = null;
        LastKagoBugDropRequest = null;
        ResetMagdolliteFrameEvents();
        LastKzanSoundEffect = null;
        LastHibashiSoundEffect = null;
        LastNuclearWaffleSoundEffect = null;
        LastFakeKraidSoundEffect = null;
        LastFakeKraidDropRequest = null;
        LastSpacePirateSoundEffect = null;
        LastEnemyProjectileDudSoundEffect = null;
        LastBeetomSoundEffect = null;
        LastWorkRobotSoundEffect = null;
        LastBotwoonSoundEffect = null;
        LastBotwoonDropRequest = null;
        LastBotwoonWallPlm = null;
        LastBotwoonMusicRequest = null;
        LastBombTorizoSoundEffect = null;
        LastBombTorizoMusicRequest = null;
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
                    // nonzero. Grapple-cancel reactions also select this handler with a
                    // zero clock; that call clears bit four without running main AI.
                    slot.FlashTimer = 0;
                    if (slot.FrozenTimer != 0)
                        slot.FrozenTimer = unchecked((ushort)(slot.FrozenTimer - 1));
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
                        enemyNmiFrameCounter8);
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
        StepWorkRobotPaletteAnimation();
        StepMagdollitePaletteAnimation();
        StepBlueBrinstarFaceBlockPaletteAnimation();
        if (!timeIsFrozen)
            StepRoomSpriteObjects();
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
                ushort drawPaletteIndex = slot.EnemyDefinitionPointer == CeresRidleyDefinition &&
                    _ceresRidley is not null
                        ? _ceresRidley.CommonDrawPaletteIndex
                        : slot.PaletteIndex;
                if (slot.EnemyDefinitionPointer == CeresRidleyDefinition)
                {
                    // CeresRidley_Main calls DrawRidleyTail/DrawRidleyWings before the
                    // common WriteEnemyOams pass emits the extended body. Appending these
                    // here preserves both that OAM order and the enemy's normal layer queue.
                    DrawCeresRidleySupplementalSprites(oam, slot, cameraX, cameraY);
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

                    // $FFFE names a BG2 tilemap command stream, not OBJ art. Its writer is
                    // a separate modeled-BG seam; invisible steam frames never use it.
                    if (ReadWord(
                            _bus,
                            (slot.Definition.Bank << 16) | ordinarySpritemap) != 0xfffe &&
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
        }
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
        ushort controllerInput)
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
            RunInitializationAi(slot, level, samus, controllerInput);

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
        ushort controllerInput = 0)
    {
        int address = (slot.Definition.Bank << 16) | slot.Definition.InitializationAiPointer;
        switch (address)
        {
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
            case 0xa6a0f5 when slot.EnemyDefinitionPointer == 0xe13f:
                InitializeCeresRidley(slot);
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
            case 0xaad7c8 when slot.EnemyDefinitionPointer == TourianEntranceStatueDefinition:
                InitializeTourianEntranceStatue(slot);
                return;
            case 0xa2804c:
                return;
            default:
                throw new NotSupportedException(
                    $"Enemy ${slot.EnemyDefinitionPointer:X4} initialization AI ${address:X6} is not translated.");
        }
    }

    private void InitializeCeresSteam(RoomEnemySlot slot)
    {
        if (slot.Parameter1 >= 6)
        {
            throw new InvalidDataException(
                $"Ceres steam parameter one ${slot.Parameter1:X4} exceeds its six-entry tables.");
        }

        slot.VramTilesIndex = 0;
        slot.Properties = slot.Properties.With(EnemyProperties.ProcessInstructions);
        slot.ExtraProperties = slot.ExtraProperties.With(EnemyExtraProperties.UsesExtendedSpritemap);
        slot.InstructionTimer = 1;
        slot.Timer = 0;
        slot.PaletteIndex = 0x0a00;
        slot.VariableD = unchecked((ushort)((_nextRandom!() & 0x001f) + 1));
        int tableIndex = slot.Parameter1 * 2;
        slot.CurrentInstruction = ReadWord(_bus!, 0xa6eff5 + tableIndex);
        slot.VariableA = ReadWord(_bus!, 0xa6f001 + tableIndex);
    }

    /// <summary>Ports <c>CeresDoor_Init</c> at $A6:F6C5 for the live Ceres room path.</summary>
    private void InitializeCeresDoor(RoomEnemySlot slot)
    {
        // Both ROM tables contain one word per population parameter. Variants five and six
        // are the left/right OBJ walls spawned at $A6:A9A5 for Ridley's Mode-7 departure;
        // the old four-entry bound made the native spawned records impossible to create.
        if (slot.Parameter1 >= 7)
        {
            throw new InvalidDataException(
                $"Ceres door parameter one ${slot.Parameter1:X4} exceeds its seven variants.");
        }

        slot.SpritemapPointer = 0xfac7;
        slot.InstructionTimer = 1;
        slot.Timer = 0;
        slot.VramTilesIndex = 0;
        slot.PaletteIndex = 0x0400;
        int tableOffset = slot.Parameter1 * 2;
        slot.VariableA = ReadWord(_bus!, 0xa6f72b + tableOffset);
        slot.CurrentInstruction = ReadWord(_bus!, 0xa6f52c + tableOffset);
        slot.VariableB = 0;

        // CeresDoor_Func_1 performs this extra direct transfer only for variant two. The
        // source/destination are the literal reconstructed DMA record at $A6:F739.
        if (slot.Parameter1 == 2)
        {
            // The source register used bank $B0 with a 16-bit address that increments
            // independently of the bank byte. Materialize that exact DMA source slice,
            // then use SnesVram's range-checked consecutive transfer primitive.
            byte[] tileBytes = new byte[0x0400];
            for (int byteIndex = 0; byteIndex < tileBytes.Length; byteIndex++)
                tileBytes[byteIndex] = _bus!.ReadByte(0xb00000 | ((0xc400 + byteIndex) & 0xffff));
            _vram!.LoadBytes(0xe000, tileBytes);
        }

        if (CeresStatus == 0 && slot.Parameter1 == 3)
        {
            // The native destination $142 is a byte offset into target_palettes: colors
            // 161..175. This runtime exposes the final fade target directly in CGRAM.
            _cgram!.LoadFromBus(_bus!, 0xa6f4ee, colorCount: 15, destinationIndex: 0x142 / 2);
            return;
        }

        slot.PaletteIndex = 0x0e00;
        int source = CeresStatus != 0 ? 0xa6f50e : 0xa6f4ee;
        _cgram!.LoadFromBus(_bus!, source, colorCount: 15, destinationIndex: 0x1e2 / 2);
    }

    private static void InitializeGunshipTop(RoomEnemySlot slot)
    {
        // Normal gameplay takes $A2:A67C: the cutscene game-state/loading-state alternatives
        // are different room-entry scenarios and therefore cannot be inferred here.
        slot.Properties = slot.Properties.With(
            EnemyProperties.ProcessInstructions | EnemyProperties.IgnoreSamusCollision);
        slot.InstructionTimer = 1;
        slot.Timer = 0;
        slot.CurrentInstruction = 0xa616;
        slot.PaletteIndex = 0x0e00;
        slot.YPosition = unchecked((ushort)(slot.YPosition - 25));
        slot.VariableE = slot.YPosition;
        slot.VariableF = 0xa9bd;
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
            slot.YPosition = unchecked((ushort)(slot.YPosition + 15));
            slot.VariableD = 71;
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
        byte nmiFrameCounter8)
    {
        int address = (slot.Definition.Bank << 16) | slot.Definition.MainAiPointer;
        switch (address)
        {
            case 0xa8b10a when slot.EnemyDefinitionPointer == MagdolliteDefinition:
                RunMagdolliteMain(slot, RequireMagdolliteState(slot), samus);
                return;
            case 0xa2a759:
                RunGunshipTopMain(slot, samus, newlyPressedControllerInput);
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
            case 0xa6f00d:
                RunCeresSteamMain(slot);
                return;
            case 0xa6f765:
                RunCeresDoorMain(slot);
                return;
            case 0xa6a288 when slot.EnemyDefinitionPointer == 0xe13f:
                RunCeresRidleyMain(slot, samus);
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
            case 0xaad7c7 when slot.EnemyDefinitionPointer == TourianEntranceStatueDefinition:
                // $AA:D7C7 is the one-byte RTL immediately before the initializer. The
                // three enemy records animate exclusively through their ROM lists.
                return;
            default:
                throw new NotSupportedException(
                    $"Enemy ${slot.EnemyDefinitionPointer:X4} main AI ${address:X6} is not translated.");
        }
    }

    private static void RunCeresSteamMain(RoomEnemySlot slot)
    {
        slot.Health = 0x7fff;
        if (slot.VariableA == 0xeff4)
            return;

        // Parameters four/five install $A6:F019 through table words at $F009/$F00B; its
        // graphical offsets depend on the Ceres elevator's Mode-7 matrix. They are retained
        // as a named unsupported AI boundary instead of silently drawing the untransformed
        // actor at its base point.
        throw new NotSupportedException(
            $"Ceres steam Mode-7 function $A6:{slot.VariableA:X4} is not translated.");
    }

    private void RunCeresDoorMain(RoomEnemySlot slot)
    {
        switch (slot.VariableA)
        {
            // Functions two/three only produce escape earthquake state when status >= 2.
            // Earthquake rendering is independent of the door's own initial presentation.
            case 0xf76b:
            case 0xf770:
                return;

            case 0xf7a5:
                slot.Properties = slot.Properties.With(EnemyProperties.Invisible);
                if ((CeresStatus & 1) != 0)
                {
                    slot.PaletteIndex = 0x0e00;
                    slot.Properties = slot.Properties.Without(EnemyProperties.Invisible);
                }
                return;

            case 0xf7bd:
                RunCeresDoorPaletteAnimation();
                if (CeresStatus >= 2)
                {
                    // $A6:F7BD begins a 48-frame destruction sequence. Retaining this as
                    // an explicit later boundary avoids pretending the escape state exists.
                    throw new NotSupportedException("Ceres door destruction sequence $A6:F7DC is not translated.");
                }
                return;

            case 0xf850:
                RunCeresDoorPaletteAnimation();
                return;

            default:
                throw new NotSupportedException(
                    $"Ceres door main function $A6:{slot.VariableA:X4} is not translated.");
        }
    }

    private void RunCeresDoorPaletteAnimation()
    {
        // $A6:F850 selects six colors by NMI counter bits 3..5. Enemy FrameCounter advances
        // at the same accepted-frame cadence in this runtime, so slot zero is the shared
        // timebase for the room-owned palette cycle.
        ushort frame = _slots[0].FrameCounter;

        // AnimateCeresElevatorPlatform at $A6:F8F1 does not belong to either arrival
        // projectile. It survives their touchdown deletion because the rotating-room door
        // actor keeps alternating these four Mode-7 tilemap bytes forever. Omitting this
        // queue made the moving OBJ pad flash correctly, then left the landed tile platform
        // frozen on whichever frame happened to be present at deletion.
        ushort transferPointer = ReadWord(_bus!, 0xa6f900 + (frame & 2));
        ApplyMode7TransferList(transferPointer);

        ushort sourcePointer = unchecked((ushort)(2 * (frame & 0x0038) - 0x078f));
        _cgram!.LoadFromBus(_bus!, 0xa60000 | sourcePointer, colorCount: 6, destinationIndex: 0x52 / 2);
    }

    private void RunGunshipTopMain(
        RoomEnemySlot top,
        SamusState? samus,
        ushort newlyPressedControllerInput)
    {
        if (top.SlotIndex + 2 >= EnemyCount)
            throw new InvalidDataException("Gunship top is missing its two following component slots.");

        RoomEnemySlot bottom = _slots[top.SlotIndex + 1];

        // $A2:A75C decrements the solid bottom's sound timer and reloads 70 on one/underflow.
        // Audio queue two is not yet represented by the runtime, but the actor-owned timer
        // is observable state and must still advance at the original point.
        ushort oldBottomTimer = bottom.VariableD;
        bottom.VariableD = unchecked((ushort)(bottom.VariableD - 1));
        if (oldBottomTimer == 1 || (short)bottom.VariableD < 0)
            bottom.VariableD = 70;

        // The native address-range test admits functions $A942-$AC1A. Idle function $A9BD
        // is inside that interval, so the landed ship continuously performs its four-phase
        // bob before dispatching the function itself.
        if (!IsNegative16(top.VariableF + 0x56be) && IsNegative16(top.VariableF + 0x53e5))
            StepGunshipBob(top);

        switch (top.VariableF)
        {
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
            default:
                throw new NotSupportedException(
                    $"Gunship function $A2:{top.VariableF:X4} is not translated.");
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
                case 0xe4be: // Ridley: begin roar; audio playback is outside this subsystem.
                    RequireCeresRidley(slot).Roaring = true;
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
                    ApplyHibashiActivityFrame(slot, word switch
                    {
                        0x8e13 => 0,
                        0x8e2d => 1,
                        0x8e41 => 2,
                        0x8e55 => 3,
                        0x8e69 => 4,
                        0x8e7d => 5,
                        0x8e91 => 6,
                        0x8ea5 => 7,
                        0x8eb9 => 8,
                        0x8ecd => 9,
                        0x8ee1 => 10,
                        0x8ef5 => 11,
                        0x8f09 => 12,
                        0x8f1d => 13,
                        0x8f31 => 14,
                        _ => 15,
                    });
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
                    RequireCeresRidley(slot).Roaring = false;
                    cursor = unchecked((ushort)(cursor + 2));
                    break;
                case 0xe4d2: // Ceres: low-energy branch embedded in the fireball animation.
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
                case 0xe501: // Ceres Ridley: select feet-distance animation index operand.
                    RequireCeresRidley(slot).FeetDistanceIndex = ReadWord(
                        _bus!,
                        (slot.Definition.Bank << 16) | unchecked((ushort)(cursor + 2)));
                    cursor = unchecked((ushort)(cursor + 4));
                    break;
                case 0xe517: // Ridley: branch to operand when he is not facing left.
                    CeresRidleyState ridley = RequireCeresRidley(slot);
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
                case 0xe84d: // Ridley: aim the next fireball from the facing-dependent mouth.
                    if (samus is null)
                    {
                        throw new InvalidOperationException(
                            "Ceres Ridley fireball aim requires the active Samus actor.");
                    }
                    CalculateCeresRidleyFireballVelocity(slot, samus);
                    cursor = unchecked((ushort)(cursor + 2));
                    break;
                case 0xe904: // Ridley: spawn the leading fireball with a wall afterburn.
                    SpawnCeresRidleyFireball(slot, spawnAfterburn: true);
                    cursor = unchecked((ushort)(cursor + 2));
                    break;
                case 0xe909: // Ridley: spawn a following fireball without an afterburn.
                    SpawnCeresRidleyFireball(slot, spawnAfterburn: false);
                    cursor = unchecked((ushort)(cursor + 2));
                    break;
                case 0xe969: // Ceres Ridley: animation hands control to accelerating liftoff.
                    CeresRidleyState liftoff = RequireCeresRidley(slot);
                    liftoff.Function = CeresRidleyAiFunction.LiftoffAccelerating;
                    liftoff.VerticalVelocity = unchecked((ushort)-352);
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
                    // Fresh Ceres begins with the boss bit clear. CeresStatus becomes the
                    // translated owner of that event later; until then, follow the native
                    // false branch to the same-bank pointer in the next word.
                    cursor = ReadWord(
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
                case 0xf6bd: // Ceres door sound command; audio queue is not yet modeled.
                    cursor = unchecked((ushort)(cursor + 2));
                    break;
                default:
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
                    throw new NotSupportedException(
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

            bool active = slot.Properties.HasAny(EnemyProperties.ProcessOffScreen) ||
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
