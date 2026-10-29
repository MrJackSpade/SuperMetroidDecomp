using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies.Drawing;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Chozo.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Rooms.Scrolling;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Enemies.Chozo;

/// <summary>The Chozo statue species owner: its per-room state, AI routines and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class ChozoStatueBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [.. ChozoStatuePartDefinitions.Parts.Keys];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemySlot slot, EnemyCommonParticipation common) => new ChozoStatueEntity(this, slot, common);

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

    /// <summary>Pending hardcoded and footstep PLM requests consumed by the shared allocator.</summary>
    internal readonly List<ChozoStatuePlmRequest> ChozoStatuePlmRequests = new();

    /// <summary>Consumes each published request once through the shared bank-$84 slot allocator.</summary>
    internal void ApplyPendingChozoStatuePlms(RoomLevelData level, ChozoStatuePlms chozoStatuePlms)
    {
        while (ChozoStatuePlmRequests.Count != 0)
        {
            chozoStatuePlms.TrySpawnChozoStatuePlm(level, ChozoStatuePlmRequests[0]);
            ChozoStatuePlmRequests.RemoveAt(0);
        }
    }

    /// <summary>Library-two sound queued as the statue grabs Samus.</summary>
    private static readonly SoundEffectId ChozoGrabsSamusSound =
        EnemySoundEffectDefinitions.ChozoGrabsSamusSound;
    /// <summary>Library-two sound queued by the walking statue's footsteps.</summary>
    private static readonly SoundEffectId ChozoFootstepsSound =
        EnemySoundEffectDefinitions.ChozoFootstepsSound;

    /// <summary>Last library-two sound requested by either walking sequence.</summary>
    public SoundEffectId? LastChozoStatueSoundEffect { get; private set; }

    /// <summary>Last value sent through native <c>CallSomeSamusCode(0/1)</c>.</summary>
    public bool ChozoStatueSamusControlsEnabled { get; private set; } = true;

    /// <summary>Whether the Lower Norfair statue has started lowering the acid in this room (instruction $AA:E429).</summary>
    public bool ChozoStatueAcidLoweringStarted { get; private set; }

    /// <summary>
    /// Ports the complete palette-only initializer at $AA:E716. The retail record begins
    /// with property $0200, so it exists solely long enough to replace target palette rows
    /// nine and ten; the scheduler removes it before any main-AI frame.
    /// </summary>
    internal void InitializeN00bTubeCracks()
    {
        (_room.Artwork?.ChozoAndTubeColors ?? throw new InvalidDataException(
            "Tube crack colors require installed artwork.")).ApplyTubeCracks(ChozoAndTubeColours());
    }

    /// <summary>Ports $AA:E725-$E7A1 for both shipped parameter-two variants.</summary>
    internal void InitializeChozoStatue(RoomEnemySlot statue)
    {
        int variant = statue.Spawn.Population.Parameter2;
        if ((variant & 1) != 0 || variant > 2)
        {
            throw new InvalidDataException(
                $"Chozo statue parameter two ${variant:X4} is outside its two-entry ROM table.");
        }

        // The original ORs $2000, $0800, and the otherwise unnamed high bit. Keep $8000 raw:
        // no translated caller has proved a stable semantic name for that property yet.
        statue.Properties = statue.Properties.With(
            EnemyProperties.SolidToSamus |
            EnemyProperties.ProcessInstructions |
            EnemyProperties.ProcessOffScreen);
        statue.Frame = EnemyFrameId.Empty;
        statue.InstructionTimer = WrappingCountdown.Start(1);
        statue.Timer = WrappingCountdown.Start(0);
        statue.Colouring = SharedSpriteColouring.Common;

        var state = new ChozoStatueState
        {
            PreInstruction = ChozoStatuePreInstruction.Idle,
            IsWreckedShip = variant == 0,
            HandTriggered = false,
        };
        statue.SetSpeciesState(state, state.ClearWithRecord);

        // The native initializer writes enemy_data[0].layer regardless of cur_enemy_index.
        // Every retail population places this singleton in slot zero, but preserve the alias
        // explicitly so malformed/debug populations expose the same cross-slot behavior.
        _slotTable.Slots[0].Layer = 0;

        if (state.IsWreckedShip)
        {
            state.Program = AnimationProgramCursor<ChozoStatueProgram>.Start(ChozoStatueProgram.WreckedShipInitial);
            ChozoStatueColors().ApplyWreckedShip(ChozoAndTubeColours());
            PublishHardcodedChozoPlm(PlmHeaderId.WreckedShipChozoHand, ChozoStatueRoomDefinitions.WreckedShipHandBlock);
            PublishHardcodedChozoPlm(PlmHeaderId.BlockSlopeAccessForWreckedShipChozo, ChozoStatueRoomDefinitions.WreckedShipSlopeAccessBlock);
        }
        else
        {
            state.Program = AnimationProgramCursor<ChozoStatueProgram>.Start(ChozoStatueProgram.LowerNorfairInitial);
            ChozoStatueColors().ApplyLowerNorfair(ChozoAndTubeColours());
            PublishHardcodedChozoPlm(PlmHeaderId.LowerNorfairChozoHand, ChozoStatueRoomDefinitions.LowerNorfairHandBlock);
        }
    }

    /// <summary>Applies the installed Wrecked Ship or Lower Norfair statue palette to live CGRAM.</summary>
    private ChozoAndTubeColorCatalog ChozoStatueColors() =>
        _room.Artwork?.ChozoAndTubeColors ?? throw new InvalidDataException("Chozo statue colors require installed artwork.");

    /// <summary>The two sprite colour sets the Chozo statues and the glass tube colour (#1405).</summary>
    private ColourRun ChozoAndTubeColours() =>
        _presentation.Colours.SpriteSets.Sets(SpriteColourSetDefinitions.ChozoAndTube, SpriteColourSetDefinitions.ChozoAndTubeSets);

    /// <summary>
    /// Represents the exact enemy-slot side effect produced by the bank-$84 Chozo hand
    /// trigger. Collision/pose/item admission remains the PLM owner's responsibility; once
    /// admitted, both native trigger setups write parameter one and disable Samus controls.
    /// </summary>
    public void ActivateChozoStatueHandTrigger(RoomLevelData level, int? collisionBlockIndex = null)
    {
        Ensure.NotNull(level);
        _room.EnsureLoaded();
        RoomEnemySlot statue = _slotTable.Slots[0];
        if (statue.DefinitionId != EnemyDefinitionId.Chozo)
            throw new InvalidOperationException("Chozo hand trigger requires statue $F0FF in enemy slot zero.");

        ChozoStatueState state = statue.RequireSpeciesState<ChozoStatueState>();
        int triggerBlockX = state.IsWreckedShip ? ChozoStatueRoomDefinitions.WreckedShipHandBlock.X : ChozoStatueRoomDefinitions.LowerNorfairTriggerX;
        int triggerBlockY = state.IsWreckedShip ? ChozoStatueRoomDefinitions.WreckedShipHandBlock.Y : ChozoStatueRoomDefinitions.LowerNorfairTriggerY;
        int triggerBlockIndex = collisionBlockIndex ?? level.GetBlockIndex(new(unchecked((ushort)triggerBlockX), unchecked((ushort)triggerBlockY)));
        RoomCollisionBlock triggerBlock = level.GetCollisionBlockByIndex(triggerBlockIndex);

        // Both setup routines clear only the block-type nibble. The parallel BTS byte and
        // visual block number survive until their subsequently spawned hardcoded PLM runs.
        level.SetForegroundEntry(
            triggerBlockIndex,
            triggerBlock.PackedWord.VisualWord);
        state.HandTriggered = true;
        DisableChozoStatueSamusControls();
        if (!state.IsWreckedShip)
        {
            // Lower Norfair's $84:D18F trigger records event $0C before waking the actor.
            _room.Services.Events.Set(EventNumber.LowerNorfairChozoLoweredAcid);
            PublishHardcodedChozoPlm(
                PlmHeaderId.CrumbleLowerNorfairChozoRoomPlug,
                ChozoStatueRoomDefinitions.LowerNorfairHandBlock);
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
                ChozoStatueRoomDefinitions.WreckedShipSlopeAccessBlock);
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
                if (state.HandTriggered)
                {
                    state.Program = AnimationProgramCursor<ChozoStatueProgram>.Start(ChozoStatueProgram.LowerNorfairActivated);
                    statue.InstructionTimer = WrappingCountdown.Start(1);
                }
                return;

            case ChozoStatuePreInstruction.WaitForWreckedShipHandTrigger:
                if (_room.Services.Bosses.IsDefeated(BossBits.AreaBoss) && state.HandTriggered)
                {
                    state.Program = AnimationProgramCursor<ChozoStatueProgram>.Start(ChozoStatueProgram.WreckedShipActivated);
                    statue.InstructionTimer = WrappingCountdown.Start(1);
                }
                return;

            default:
                throw new InvalidDataException(
                    $"Chozo statue pre-instruction {state.PreInstruction?.ToString() ?? "none"} is not translated.");
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
        ChozoStatueState state = statue.RequireSpeciesState<ChozoStatueState>();
        switch (cue)
        {
            case ChozoStatueCue.SetPreInstruction setPreInstruction:
                state.PreInstruction = setPreInstruction.PreInstruction;
                return null;

            case ChozoStatueCue.Command { Kind: ChozoStatueCueKind.ClearPreInstruction }:
                state.PreInstruction = ChozoStatuePreInstruction.Cleared;
                return null;

            case ChozoStatueCue.Command { Kind: ChozoStatueCueKind.StartLoweringAcid }:
                ChozoStatueAcidLoweringStarted = true;
                // Native AI writes the FX words consumed by the liquid handler once, at
                // instruction execution; the FX owns them from then on.
                _roomFx.Layer3Fx?.SetLiquidMotion(
                    timer: ChozoStatueFxData.LoweringDelay, packedYVelocity: ChozoStatueFxData.LoweringVelocity);
                return null;

            case ChozoStatueCue.Command { Kind: ChozoStatueCueKind.SetLoweredAcidPosition }:
                _roomFx.Layer3Fx?.SetLiquidMotion(baseYPosition: ChozoStatueRoomDefinitions.LoweredAcidY);
                return null;

            case ChozoStatueCue.Command { Kind: ChozoStatueCueKind.UnlockSamus }:
                EnableChozoStatueSamusControls();
                return null;

            case ChozoStatueCue.Command { Kind: ChozoStatueCueKind.PlayGrabSound }:
                LastChozoStatueSoundEffect = ChozoGrabsSamusSound;
                _audio.QueueEnemySound(ChozoGrabsSamusSound, maximumQueued: SoundQueueLimit.Max6);
                return null;

            case ChozoStatueCue.Command { Kind: ChozoStatueCueKind.PlayFootstepSound }:
                LastChozoStatueSoundEffect = ChozoFootstepsSound;
                _audio.QueueEnemySound(ChozoFootstepsSound, maximumQueued: SoundQueueLimit.Max6);
                return null;

            case ChozoStatueCue.Footstep footstep:
                ProcessChozoStatueFootstep(
                    statue,
                    RequireChozoLevel(level),
                    footstep.Offset);
                return null;

            case ChozoStatueCue.Move move:
                ProcessChozoStatueMovement(
                    statue,
                    state,
                    RequireChozoSamus(samus),
                    RequireChozoLevel(level),
                    move.CarryRecord);
                return null;

            case ChozoStatueCue.Command { Kind: ChozoStatueCueKind.ReleaseSamus }:
                FinishWreckedShipChozoSequence();
                return null;

            default:
                throw new InvalidOperationException($"Undefined {nameof(ChozoStatueCueKind)} {cue}.");
        }
    }

    /// <summary>Moves the statue through terrain, then places Samus at the authored carry offset from its accepted position.</summary>
    internal void ProcessChozoStatueMovement(
        RoomEnemySlot statue,
        ChozoStatueState state,
        SamusState samus,
        RoomLevelData level,
        int carryRecord)
    {
        var motion = ChozoCarry.Read(carryRecord);
        state.CarryRecord = carryRecord;
        int signedVelocity = motion.Velocity;

        // Enemy_MoveRight_IgnoreSlopes receives a signed 8.8 word promoted to 16.16 by
        // INT16_SHL8. Enemy_MoveDown receives its absolute magnitude in the same format.
        _ = _terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(
            level,
            statue,
            SixteenSixteen.FromEightEight(signedVelocity));
        _ = _terrain.MoveEnemyVertically(
            level,
            statue,
            SixteenSixteen.FromEightEight(Math.Abs(signedVelocity)));
        EnemyTerrain.AlignEnemyYWithNonSquareSlope(level, statue);

        // The two 32-word tables are joint offsets indexed by the same record.
        // Writing Samus after collision reproduces the native cutscene's absolute ownership.
        samus.Position = samus.Position.WithWhole(new(unchecked((ushort)(statue.Position.X.Whole + motion.SamusOffset.X)), unchecked((ushort)(statue.Position.Y.Whole + motion.SamusOffset.Y))));
    }

    /// <summary>Spawns a plug-crumble PLM and spike-footstep only when the offset floor block has the native special type.</summary>
    private void ProcessChozoStatueFootstep(
        RoomEnemySlot statue,
        RoomLevelData level,
        Offset16<RoomPixels> footstepOffset)
    {
        Position16<RoomPixels> probe = new(
            unchecked((ushort)(statue.Position.X.Whole + footstepOffset.X)), unchecked((ushort)(statue.Position.Y.Whole + 28)));
        RoomCollisionBlock current = level.GetCollisionBlockAtPixel(probe);
        int belowIndex = current.Index + level.Size.X;
        if ((uint)belowIndex >= (uint)level.ForegroundEntries.Length ||
            level.GetCollisionBlockByIndex(belowIndex).CollisionType != RoomCollisionType.SpikeBlock)
        {
            return;
        }

        ChozoStatuePlmRequests.Add(new ChozoStatuePlmRequest(
            PlmHeaderId.CrumbleLowerNorfairChozoRoomPlug,
            probe.ToRoomBlock(),
            IsHardcoded: false));
        SpawnWreckedShipChozoFootstep(statue, footstepOffset);
    }

    /// <summary>Places the Wrecked Ship footstep projectile beneath the statue at its signed horizontal offset.</summary>
    internal void SpawnWreckedShipChozoFootstep(RoomEnemySlot statue, Offset16<RoomPixels> footstepOffset)
    {
        RoomEnemyProjectileSlot? projectile = _projectiles.AllocateEnemyProjectile();
        if (projectile is null)
            return;

        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            projectile,
            RoomEnemyProjectileKind.WreckedShipChozoSpikeFootstep,
            graphics: default);
        projectile.Position = projectile.Position.WithWhole(new(unchecked((ushort)(statue.Position.X.Whole + footstepOffset.X)), unchecked((ushort)(statue.Position.Y.Whole + 28))));
    }

    /// <summary>Restores Samus controls, publishes the native overlapping scroll writes and queues slope-access terrain.</summary>
    private void FinishWreckedShipChozoSequence()
    {
        EnableChozoStatueSamusControls();

        // The assembly performs overlapping 16-bit stores at raw scroll indexes 6, 8, 9,
        // and 13. Publish the resulting bytes (6..10 = 0, 13 = 1, 14 = 0) exactly.
        for (int index = 6; index <= 10; index++)
            _room.Services.RoomScroll.Write(index, RoomScrollState.RedBoundary);
        _room.Services.RoomScroll.Write(13, RoomScrollState.Blue);
        _room.Services.RoomScroll.Write(14, RoomScrollState.RedBoundary);

        PublishHardcodedChozoPlm(
            PlmHeaderId.BlockSlopeAccessForWreckedShipChozo,
            ChozoStatueRoomDefinitions.WreckedShipSlopeAccessBlock);
    }

    /// <summary>Gives Samus her controls back, in both the room service and observable state.</summary>
    private void EnableChozoStatueSamusControls()
    {
        ChozoStatueSamusControlsEnabled = true;
        _room.Services.SamusControls.EnableControls();
    }

    /// <summary>Takes Samus's controls away while the statue holds her, in both the room service and observable state.</summary>
    private void DisableChozoStatueSamusControls()
    {
        ChozoStatueSamusControlsEnabled = false;
        _room.Services.SamusControls.DisableControls();
    }

    /// <summary>Queues an authored bank-$84 PLM spawn at its fixed room block.</summary>
    private void PublishHardcodedChozoPlm(PlmHeaderId header, Position16<RoomBlocks> block) =>
        ChozoStatuePlmRequests.Add(new ChozoStatuePlmRequest(
            header,
            block,
            IsHardcoded: true));

    /// <summary>Requires the live Samus actor for cutscene carry movement.</summary>
    private static SamusState RequireChozoSamus(SamusState? samus) =>
        samus ?? throw new InvalidOperationException(
            "Chozo statue movement requires the active Samus actor.");

    /// <summary>Requires loaded terrain before statue movement or footstep collision probes.</summary>
    private static RoomLevelData RequireChozoLevel(RoomLevelData? level) =>
        level ?? throw new InvalidOperationException(
            "Chozo statue movement requires the active room level.");

    /// <summary>Ports ChozoStatue's main AI.</summary>
    internal void RunChozoStatueMainAi(RoomEnemySlot slot, IEnemyAiFrame _)
    {
        RunChozoStatueMain(slot, slot.RequireSpeciesState<ChozoStatueState>());
    }

    /// <summary>Advances ChozoStatue's typed animation.</summary>
    internal void AdvanceChozoStatueTypedAnimation(RoomEnemySlot slot, EnemyInstructionContext context) =>
        EnemyProgramAnimation.Advance(slot, ChozoStatueAnimations.Programs, ChozoStatueFrameDefinitions.ShownFrames.Of,
            static (ChozoStatueState state) => state.Program, static (state, cursor) => state.Program = cursor,
            (state, cue, _) => ApplyChozoStatueCue(slot, cue, context.Samus, context.Level));
}
