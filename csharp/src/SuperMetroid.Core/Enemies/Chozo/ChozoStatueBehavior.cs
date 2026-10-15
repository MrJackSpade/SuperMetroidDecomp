using SuperMetroid.Core.Enemies.Drawing;
using SuperMetroid.Core.Enemies.Chozo.Artwork;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Rooms.Scrolling;
using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.Enemies.Chozo;

/// <summary>The Chozo statue species owner: its per-room state, AI routines and the interactions it registers.</summary>
internal sealed class ChozoStatueBehavior : IEnemySpecies
{
    /// <summary>Supplies installed palette art, room events, scroll grid and Samus-control service.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Clips walking-statue movement before the carried Samus position is written.</summary>
    private readonly EnemyTerrain _terrain;
    /// <summary>Allocates the Wrecked Ship statue's spike-footstep projectile.</summary>
    private readonly EnemyProjectileEngine _projectiles;
    /// <summary>Queues library-two footstep and walking sounds from native instructions.</summary>
    private readonly EnemyAudioQueue _audio;
    /// <summary>Exposes native slot zero for the singleton statue's cross-slot writes.</summary>
    private readonly EnemySlotTable _slotTable;
    /// <summary>Receives tube-crack and statue palette changes in live CGRAM.</summary>
    private readonly EnemyPresentation _presentation;
    /// <summary>Receives statue-authored layer-three FX motion writes.</summary>
    private readonly EnemyRoomFx _roomFx;

    /// <summary>Binds bank-$AA statue behavior to room, movement, palette, FX, sound and projectile owners.</summary>
    internal ChozoStatueBehavior(EnemyRoomBinding room, EnemyTerrain terrain, EnemyProjectileEngine projectiles, EnemyAudioQueue audio, EnemySlotTable slotTable, EnemyPresentation presentation, EnemyRoomFx roomFx)
    {
        _room = room;
        _terrain = terrain;
        _projectiles = projectiles;
        _audio = audio;
        _slotTable = slotTable;
        _presentation = presentation;
        _roomFx = roomFx;
    }


    /// <summary>Per-slot statue phase and carry-motion state cleared on room load.</summary>
    internal readonly ChozoStatueState?[] _chozoStatueStates =
        new ChozoStatueState?[RoomEnemySystem.MaximumEnemyCount];
    /// <summary>Pending hardcoded and footstep PLM requests consumed by the shared allocator.</summary>
    internal readonly List<ChozoStatuePlmRequest> _chozoStatuePlmRequests = new();

    /// <summary>Consumes each published request once through the shared bank-$84 slot allocator.</summary>
    internal void ApplyPendingChozoStatuePlms(RoomLevelData level, ChozoStatuePlms chozoStatuePlms)
    {
        while (_chozoStatuePlmRequests.Count != 0)
        {
            chozoStatuePlms.TrySpawnChozoStatuePlm(level, _chozoStatuePlmRequests[0]);
            _chozoStatuePlmRequests.RemoveAt(0);
        }
    }

    /// <summary>Last library-two sound requested by either walking sequence.</summary>
    public ushort? LastChozoStatueSoundEffect { get; private set; }

    /// <summary>Last value sent through native <c>CallSomeSamusCode(0/1)</c>.</summary>
    public bool ChozoStatueSamusControlsEnabled { get; private set; } = true;

    /// <summary>FX timer written by instruction $AA:E429.</summary>
    public ushort ChozoStatueFxTimer { get; private set; }

    /// <summary>FX Y velocity written by instruction $AA:E429.</summary>
    public ushort ChozoStatueFxYVelocity { get; private set; }

    /// <summary>FX base Y written by instruction $AA:E436.</summary>
    public ushort ChozoStatueFxBaseYPosition { get; private set; }

    /// <summary>Clears prior-room statue state, pending PLMs, sound, Samus-control and FX publications.</summary>
    internal void ResetChozoStatueRoomState(IChozoStatueRoomServices services)
    {
        Ensure.NotNull(services);
        Array.Clear(_chozoStatueStates);
        _chozoStatuePlmRequests.Clear();
        LastChozoStatueSoundEffect = null;
        ChozoStatueSamusControlsEnabled = true;
        ChozoStatueFxTimer = 0;
        ChozoStatueFxYVelocity = 0;
        ChozoStatueFxBaseYPosition = 0;
    }

    /// <summary>
    /// Ports the complete palette-only initializer at $AA:E716. The retail record begins
    /// with property $0200, so it exists solely long enough to replace target palette rows
    /// nine and ten; the scheduler removes it before any main-AI frame.
    /// </summary>
    internal void InitializeN00bTubeCracks()
    {
        (_room.Artwork?.ChozoAndTubeColors ?? throw new InvalidDataException(
            "Tube crack colors require installed artwork.")).ApplyTubeCracks(_presentation.Cgram);
    }

    /// <summary>Ports $AA:E725-$E7A1 for both shipped parameter-two variants.</summary>
    internal void InitializeChozoStatue(RoomEnemySlot statue)
    {
        if ((statue.Parameter2 & 1) != 0 || statue.Parameter2 > 2)
        {
            throw new InvalidDataException(
                $"Chozo statue parameter two ${statue.Parameter2:X4} is outside its two-entry ROM table.");
        }

        // The original ORs $2000, $0800, and the otherwise unnamed high bit. Keep $8000 raw:
        // no translated caller has proved a stable semantic name for that property yet.
        statue.Properties = statue.Properties.With(
            EnemyProperties.SolidToSamus |
            EnemyProperties.ProcessInstructions |
            EnemyProperties.ProcessOffScreen);
        statue.SpritemapPointer = CommonEnemyEmptyExtendedFrameDefinitions.EmptySpritemap;
        statue.InstructionTimer = NativeCountdown.Start(1);
        statue.Timer = NativeCountdown.Start(0);
        statue.Parameter1 = 0;
        statue.PaletteIndex = 0;

        var state = new ChozoStatueState(statue)
        {
            PreInstruction = ChozoStatuePreInstruction.Idle,
        };
        _chozoStatueStates[statue.SlotIndex] = state;

        // The native initializer writes enemy_data[0].layer regardless of cur_enemy_index.
        // Every retail population places this singleton in slot zero, but preserve the alias
        // explicitly so malformed/debug populations expose the same cross-slot behavior.
        _slotTable._slots[0].Layer = 0;

        if (statue.Parameter2 == 0)
        {
            state.Program = AnimationProgramCursor<ChozoStatueProgram>.Start(ChozoStatueProgram.WreckedShipInitial);
            LoadChozoStatuePalette(wreckedShip: true);
            PublishHardcodedChozoPlm(PlmHeaderId.WreckedShipChozoHand, ChozoStatuePlmRomData.WreckedShipHandBlock);
            PublishHardcodedChozoPlm(PlmHeaderId.BlockSlopeAccessForWreckedShipChozo, ChozoStatuePlmRomData.WreckedShipSlopeAccessBlock);
        }
        else
        {
            state.Program = AnimationProgramCursor<ChozoStatueProgram>.Start(ChozoStatueProgram.LowerNorfairInitial);
            LoadChozoStatuePalette(wreckedShip: false);
            PublishHardcodedChozoPlm(PlmHeaderId.LowerNorfairChozoHand, ChozoStatuePlmRomData.LowerNorfairHandBlock);
        }
    }

    /// <summary>Applies the installed Wrecked Ship or Lower Norfair statue palette to live CGRAM.</summary>
    private void LoadChozoStatuePalette(bool wreckedShip)
    {
        var colors = _room.Artwork?.ChozoAndTubeColors ?? throw new InvalidDataException(
            "Chozo statue colors require installed artwork.");
        if (wreckedShip) colors.ApplyWreckedShip(_presentation.Cgram);
        else colors.ApplyLowerNorfair(_presentation.Cgram);
    }

    /// <summary>
    /// Represents the exact enemy-slot side effect produced by the bank-$84 Chozo hand
    /// trigger. Collision/pose/item admission remains the PLM owner's responsibility; once
    /// admitted, both native trigger setups write parameter one and disable Samus controls.
    /// </summary>
    public void ActivateChozoStatueHandTrigger(RoomLevelData level, int? collisionBlockIndex = null)
    {
        Ensure.NotNull(level);
        _room.EnsureLoaded();
        RoomEnemySlot statue = _slotTable._slots[0];
        if (statue.EnemyDefinitionPointer != EnemyDefinitionId.Chozo)
            throw new InvalidOperationException("Chozo hand trigger requires statue $F0FF in enemy slot zero.");

        int triggerBlockX = statue.Parameter2 == 0 ? ChozoStatuePlmRomData.WreckedShipHandBlock.X : ChozoStatuePlmRomData.LowerNorfairTriggerX;
        int triggerBlockY = statue.Parameter2 == 0 ? ChozoStatuePlmRomData.WreckedShipHandBlock.Y : ChozoStatuePlmRomData.LowerNorfairTriggerY;
        int triggerBlockIndex = collisionBlockIndex ?? level.GetBlockIndex(new(unchecked((ushort)triggerBlockX), unchecked((ushort)triggerBlockY)));
        RoomCollisionBlock triggerBlock = level.GetCollisionBlockByIndex(triggerBlockIndex);

        // Both setup routines clear only the block-type nibble. The parallel BTS byte and
        // visual block number survive until their subsequently spawned hardcoded PLM runs.
        level.SetForegroundEntry(
            triggerBlockIndex,
            triggerBlock.PackedWord.VisualWord);
        statue.Parameter1 = 1;
        SetChozoStatueSamusControls(enabled: false);
        if (statue.Parameter2 != 0)
        {
            // Lower Norfair's $84:D18F trigger records event $0C before waking the actor.
            _room.Services.Events.Set(EventNumber.LowerNorfairChozoLoweredAcid);
            PublishHardcodedChozoPlm(
                PlmHeaderId.CrumbleLowerNorfairChozoRoomPlug,
                ChozoStatuePlmRomData.LowerNorfairHandBlock);
        }
        else
        {
            // $84:D620 performs two little-endian word stores before handing the statue to
            // $E7AE: scroll bytes 7/8 become green and 13/14 become blue. PLM $D6F8 then
            // queues the authored slope-access terrain transition in bank $84.
            _room.Services.RoomScroll.Write(7, RoomScrollState.Green);
            _room.Services.RoomScroll.Write(8, RoomScrollState.Green);
            _room.Services.RoomScroll.Write(13, RoomScrollState.Blue);
            _room.Services.RoomScroll.Write(14, RoomScrollState.Blue);
            PublishHardcodedChozoPlm(
                PlmHeaderId.ClearSlopeAccessForWreckedShipChozo,
                ChozoStatuePlmRomData.WreckedShipSlopeAccessBlock);
        }
    }

    /// <summary>Dispatches statue phases, including boss-gated movement and authored scene transitions.</summary>
    private void RunChozoStatueMain(RoomEnemySlot statue, ChozoStatueState state)
    {
        switch (state.PreInstruction)
        {
            case ChozoStatuePreInstruction.Idle:
            case ChozoStatuePreInstruction.Cleared:
            case ChozoStatuePreInstruction.Walking:
                return;

            case ChozoStatuePreInstruction.WaitForLowerNorfairHandTrigger:
                if (statue.Parameter1 != 0)
                {
                    state.Program = AnimationProgramCursor<ChozoStatueProgram>.Start(ChozoStatueProgram.LowerNorfairActivated);
                    statue.InstructionTimer = NativeCountdown.Start(1);
                }
                return;

            case ChozoStatuePreInstruction.WaitForWreckedShipHandTrigger:
                if (_room.Services.Bosses.IsDefeated(BossBits.AreaBoss) && statue.Parameter1 != 0)
                {
                    state.Program = AnimationProgramCursor<ChozoStatueProgram>.Start(ChozoStatueProgram.WreckedShipActivated);
                    statue.InstructionTimer = NativeCountdown.Start(1);
                    state.VariableA = unchecked((ushort)-256);
                    state.VariableB = 256;
                }
                return;

            default:
                throw new InvalidDataException(
                    $"Chozo statue pre-instruction $AA:{(ushort)state.PreInstruction:X4} is not translated.");
        }
    }

    /// <summary>
    /// Carries out one cue of the Chozo statues' animations: every callback reachable from lists
    /// $E39D/$E3A7/$E457/$E461. Every cue runs on to the next step, so none redirects.
    /// </summary>
    private ChozoStatueProgram? ApplyChozoStatueCue(
        RoomEnemySlot statue,
        ChozoStatueCue cue,
        SamusState? samus,
        RoomLevelData? level)
    {
        ChozoStatueState state = RequireChozoStatueState(statue);
        switch (cue.Kind)
        {
            case ChozoStatueCueKind.SetPreInstruction:
                state.PreInstruction = cue.PreInstruction;
                return null;

            case ChozoStatueCueKind.ClearPreInstruction:
                state.PreInstruction = ChozoStatuePreInstruction.Cleared;
                return null;

            case ChozoStatueCueKind.StartLoweringAcid:
                ChozoStatueFxTimer = ChozoStatueFxData.LoweringDelay;
                ChozoStatueFxYVelocity = ChozoStatueFxData.LoweringVelocity;
                // Native AI writes the same FX words consumed by the liquid handler.
                // Apply once at instruction execution, not every frame from the retained
                // diagnostic values, which would repeatedly reset the delay.
                _roomFx.Layer3Fx?.ApplyCartridgeMotionWrites(
                    timer: ChozoStatueFxTimer, packedYVelocity: ChozoStatueFxYVelocity);
                return null;

            case ChozoStatueCueKind.SetLoweredAcidPosition:
                ChozoStatueFxBaseYPosition = ChozoStatuePlmRomData.LoweredAcidY;
                _roomFx.Layer3Fx?.ApplyCartridgeMotionWrites(baseYPosition: ChozoStatueFxBaseYPosition);
                return null;

            case ChozoStatueCueKind.UnlockSamus:
                SetChozoStatueSamusControls(enabled: true);
                return null;

            case ChozoStatueCueKind.PlayGrabSound:
                LastChozoStatueSoundEffect = EnemySoundEffectDefinitions.ChozoGrabsSamusSound;
                _audio.QueueEnemySound(SoundEffectId.FromCartridge(SoundEffectLibrary.Library2,
                    LastChozoStatueSoundEffect.Value), maximumQueued: 6);
                return null;

            case ChozoStatueCueKind.PlayFootstepSound:
                LastChozoStatueSoundEffect = EnemySoundEffectDefinitions.ChozoFootstepsSound;
                _audio.QueueEnemySound(SoundEffectId.FromCartridge(SoundEffectLibrary.Library2,
                    LastChozoStatueSoundEffect.Value), maximumQueued: 6);
                return null;

            case ChozoStatueCueKind.Footstep:
                ProcessChozoStatueFootstep(
                    statue,
                    RequireChozoLevel(level),
                    cue.Value);
                return null;

            case ChozoStatueCueKind.Move:
                ProcessChozoStatueMovement(
                    statue,
                    state,
                    RequireChozoSamus(samus),
                    RequireChozoLevel(level),
                    cue.Value);
                return null;

            case ChozoStatueCueKind.ReleaseSamus:
                FinishWreckedShipChozoSequence();
                return null;

            default:
                throw new InvalidOperationException($"Undefined {nameof(ChozoStatueCueKind)} {cue.Kind}.");
        }
    }

    /// <summary>Moves the statue through terrain, then places Samus at the authored carry offset from its accepted position.</summary>
    internal void ProcessChozoStatueMovement(
        RoomEnemySlot statue,
        ChozoStatueState state,
        SamusState samus,
        RoomLevelData level,
        ushort tableOffset)
    {
        var motion = ChozoCarryMotionDefinitions.Read(tableOffset);
        state.MovementTableOffset = tableOffset;
        short signedVelocity = motion.Velocity;

        // Enemy_MoveRight_IgnoreSlopes receives a signed 8.8 word promoted to 16.16 by
        // INT16_SHL8. Enemy_MoveDown receives its absolute magnitude in the same format.
        _ = _terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(
            level,
            statue,
            SnesSixteenSixteen.FromEightEight(signedVelocity));
        _ = _terrain.MoveEnemyVertically(
            level,
            statue,
            SnesSixteenSixteen.FromEightEight(Math.Abs((int)signedVelocity)));
        EnemyTerrain.AlignEnemyYWithNonSquareSlope(level, statue);

        // The two 32-word tables are joint offsets indexed by the exact byte offset above.
        // Writing Samus after collision reproduces the native cutscene's absolute ownership.
        samus.Position = samus.Position.WithWhole(new(unchecked((ushort)(statue.Position.X.Whole + motion.SamusX)), unchecked((ushort)(statue.Position.Y.Whole + motion.SamusY))));
    }

    /// <summary>Spawns a plug-crumble PLM and spike-footstep only when the offset floor block has the native special type.</summary>
    private void ProcessChozoStatueFootstep(
        RoomEnemySlot statue,
        RoomLevelData level,
        ushort rawXOffset)
    {
        short xOffset = unchecked((short)rawXOffset);
        ushort probeX = unchecked((ushort)(statue.Position.X.Whole + xOffset));
        ushort probeY = unchecked((ushort)(statue.Position.Y.Whole + 28));
        RoomCollisionBlock current = level.GetCollisionBlockAtPixel(new Position16<RoomPixels>(probeX, probeY));
        int belowIndex = current.Index + level.Size.X;
        if ((uint)belowIndex >= (uint)level.ForegroundEntries.Length ||
            level.GetCollisionBlockByIndex(belowIndex).CollisionType != RoomCollisionType.SpikeBlock)
        {
            return;
        }

        _chozoStatuePlmRequests.Add(new ChozoStatuePlmRequest(
            PlmHeaderId.CrumbleLowerNorfairChozoRoomPlug,
            new Position16<RoomPixels>(probeX, probeY).ToRoomBlock(),
            IsHardcoded: false));
        SpawnWreckedShipChozoFootstep(statue, rawXOffset);
    }

    /// <summary>Places the Wrecked Ship footstep projectile beneath the statue at its signed horizontal offset.</summary>
    internal void SpawnWreckedShipChozoFootstep(RoomEnemySlot statue, ushort xOffset)
    {
        RoomEnemyProjectileSlot? projectile = _projectiles.AllocateEnemyProjectile();
        if (projectile is null)
            return;

        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            projectile,
            RoomEnemyProjectileKind.WreckedShipChozoSpikeFootstep,
            graphicsIndex: 0);
        projectile.Position = projectile.Position.WithWhole(new(unchecked((ushort)(statue.Position.X.Whole + unchecked((short)xOffset))), unchecked((ushort)(statue.Position.Y.Whole + 28))));
    }

    /// <summary>Restores Samus controls, publishes the native overlapping scroll writes and queues slope-access terrain.</summary>
    private void FinishWreckedShipChozoSequence()
    {
        SetChozoStatueSamusControls(enabled: true);

        // The assembly performs overlapping 16-bit stores at raw scroll indexes 6, 8, 9,
        // and 13. Publish the resulting bytes (6..10 = 0, 13 = 1, 14 = 0) exactly.
        for (int index = 6; index <= 10; index++)
            _room.Services.RoomScroll.Write(index, RoomScrollState.RedBoundary);
        _room.Services.RoomScroll.Write(13, RoomScrollState.Blue);
        _room.Services.RoomScroll.Write(14, RoomScrollState.RedBoundary);

        PublishHardcodedChozoPlm(
            PlmHeaderId.BlockSlopeAccessForWreckedShipChozo,
            ChozoStatuePlmRomData.WreckedShipSlopeAccessBlock);
    }

    /// <summary>Publishes the statue's control lock to both the room service and observable state.</summary>
    private void SetChozoStatueSamusControls(bool enabled)
    {
        ChozoStatueSamusControlsEnabled = enabled;
        _room.Services.SamusControls.SetControlsEnabled(enabled);
    }

    /// <summary>Queues an authored bank-$84 PLM spawn at its fixed room block.</summary>
    private void PublishHardcodedChozoPlm(PlmHeaderId header, Position16<RoomBlocks> block) =>
        _chozoStatuePlmRequests.Add(new ChozoStatuePlmRequest(
            header,
            block,
            IsHardcoded: true));

    /// <summary>Requires initialized bank-$AA state before statue AI dispatch.</summary>
    private ChozoStatueState RequireChozoStatueState(RoomEnemySlot statue) =>
        _chozoStatueStates[statue.SlotIndex] ?? throw new InvalidOperationException(
            $"Chozo statue slot {statue.SlotIndex} has no initialized bank-$AA state.");

    /// <summary>Requires the live Samus actor for cutscene carry movement.</summary>
    private static SamusState RequireChozoSamus(SamusState? samus) =>
        samus ?? throw new InvalidOperationException(
            "Chozo statue movement requires the active Samus actor.");

    /// <summary>Requires loaded terrain before statue movement or footstep collision probes.</summary>
    private static RoomLevelData RequireChozoLevel(RoomLevelData? level) =>
        level ?? throw new InvalidOperationException(
            "Chozo statue movement requires the active room level.");

    /// <summary>Registers separate tube-crack palette and statue initialization entry points.</summary>
    internal void RegisterChozoStatueInitializationAi(EnemyAiTable table)
    {

        table.RegisterInitialization(EnemyAiRoutine.InitAI_NoobTubeCrack, slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.NoobTubeCrack, (slot, level, samus, controllerInput, cameraX) =>
        {
            InitializeN00bTubeCracks();
        });

        table.RegisterInitialization(EnemyAiRoutine.InitAI_Chozo, slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.Chozo, (slot, level, samus, controllerInput, cameraX) =>
        {
            InitializeChozoStatue(slot);
        });
    }

    /// <summary>Registers the common statue phase dispatcher for the Chozo enemy identity.</summary>
    internal void RegisterChozoStatueMainAi(EnemyAiTable table)
    {

        table.RegisterMain(EnemyAiRoutine.MainAI_Chozo, slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.Chozo, (slot, frame, nmiFrameCounter8) =>
        {
            RunChozoStatueMain(slot, RequireChozoStatueState(slot));
        });
    }

    /// <summary>Runs the Chozo statues' animations as typed programs (#641).</summary>
    internal void RegisterChozoStatueTypedAnimations(EnemyAiTable table) =>
        table.AddTypedAnimation(slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.Chozo, (slot, context) =>
        {
            if (!EnemyProgramAnimation.Expire(slot))
                return;
            var state = RequireChozoStatueState(slot);
            AnimationProgramCursor<ChozoStatueProgram> cursor = state.Program;
            EnemyProgramAnimation.Run(slot, ChozoStatueAnimations.Programs, ref cursor, ChozoStatueFrameDefinitions.Spritemap,
                    (cue, _) => ApplyChozoStatueCue(slot, cue, context.Samus, context.Level));
            state.Program = cursor;
        });


    /// <inheritdoc/>
    public void ResetForRoomLoad(IRoomEnemyLoad request)
    {
        ResetChozoStatueRoomState(request.Services);
    }

    /// <inheritdoc/>
    public void BeginEnemyFrame(RoomEnemyFrame frame)
    {
    }
}
