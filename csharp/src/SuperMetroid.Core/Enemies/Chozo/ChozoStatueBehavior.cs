using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Chozo.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;
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

    /// <summary>Supplies room events, scroll grid and Samus-control service.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Clips walking-statue movement before the carried Samus position is written.</summary>
    private readonly EnemyTerrain _terrain;
    /// <summary>Allocates the Wrecked Ship statue's spike-footstep projectile.</summary>
    private readonly EnemyProjectileEngine _projectiles;
    /// <summary>Queues enemy-library footstep and walking sounds from instructions.</summary>
    private readonly EnemyAudioQueue _audio;
    /// <summary>Receives statue-authored effect overlay motion writes.</summary>
    private readonly EnemyRoomFx _roomFx;
    /// <summary>The room's Chozo statue, which its hand trigger wakes; null until the statue is placed.</summary>
    private RoomEnemySlot? _statue;

    /// <summary>Binds statue behavior to room, movement, FX, sound and projectile owners.</summary>
    internal ChozoStatueBehavior(EnemyRoomBinding room, EnemyTerrain terrain, EnemyProjectileEngine projectiles, EnemyAudioQueue audio, EnemyRoomFx roomFx)
    {
        _room = room;
        _terrain = terrain;
        _projectiles = projectiles;
        _audio = audio;
        _roomFx = roomFx;
    }

    /// <summary>Pending hardcoded and footstep room-object requests consumed by the shared allocator.</summary>
    internal readonly List<ChozoStatueObjectRequest> ChozoStatueObjectRequests = new();

    /// <summary>Consumes each published request once through the shared slot allocator.</summary>
    internal void ApplyPendingChozoStatueObjects(RoomLevelData level, ChozoStatueObjects chozoStatueObjects)
    {
        while (ChozoStatueObjectRequests.Count != 0)
        {
            chozoStatueObjects.TrySpawnChozoStatueObject(level, ChozoStatueObjectRequests[0]);
            ChozoStatueObjectRequests.RemoveAt(0);
        }
    }

    /// <summary>Enemy-library sound queued as the statue grabs Samus.</summary>
    private static readonly SoundEffectId ChozoGrabsSamusSound =
        EnemySoundEffectDefinitions.ChozoGrabsSamusSound;
    /// <summary>Enemy-library sound queued by the walking statue's footsteps.</summary>
    private static readonly SoundEffectId ChozoFootstepsSound =
        EnemySoundEffectDefinitions.ChozoFootstepsSound;

    /// <summary>Whether the statue last enabled Samus's controls rather than disabling them.</summary>
    public bool ChozoStatueSamusControlsEnabled { get; private set; } = true;

    /// <summary>Whether the Lower Norfair statue has started lowering the acid in this room.</summary>
    public bool ChozoStatueAcidLoweringStarted { get; private set; }

    /// <summary>
    /// Initializes the glass tube's cracks, which do nothing the game keeps: the record is deleted before any main-AI
    /// update, and the cracks' colours are the shade every tube projectile draws in
    /// (<see cref="ChozoAndTubeShades.TubeCracks"/>, #1512).
    /// </summary>
    internal static void InitializeN00bTubeCracks()
    {
    }

    /// <summary>Initializes either shipped statue.</summary>
    internal void InitializeChozoStatue(RoomEnemySlot statue)
    {
        ChozoStatueSite site = statue.SpawnSettings<ChozoStatueSpawnSettings>().Site;
        bool isWreckedShip = site switch
        {
            ChozoStatueSite.WreckedShip => true,
            ChozoStatueSite.LowerNorfair => false,
            _ => throw new InvalidOperationException($"Undefined {nameof(ChozoStatueSite)} {site}."),
        };

        // The initializer sets two named properties and the otherwise unnamed high bit. Keep the high bit raw:
        // no translated caller has proved a stable semantic name for that property yet.
        statue.Contact = statue.Contact.With(EnemyContact.SolidToSamus);
        statue.Processing = statue.Processing.With(EnemyProcessing.AnimationScript | EnemyProcessing.OffScreen);
        statue.Frame = EnemyFrameId.Empty;
        statue.FrameHold = WrappingCountdown.Start(1);
        statue.Timer = WrappingCountdown.Start(0);

        // The statue shows its own colours, the shade of its site's statue, which no other enemy shows (#1512).
        statue.Shading = PictureShading.Shaded(isWreckedShip ? ChozoAndTubeShades.WreckedShipStatue : ChozoAndTubeShades.LowerNorfairStatue);

        var state = new ChozoStatueState
        {
            FrameBehavior = ChozoStatueFrameBehavior.Idle,
            IsWreckedShip = isWreckedShip,
            HandTriggered = false,
        };
        statue.SetSpeciesState(state);
        _statue = statue;

        // The statue draws on the lowest layer, as every retail room shows it.
        statue.Layer = EnemyDrawLayer.Of(0);

        if (state.IsWreckedShip)
        {
            state.Playback = new(ChozoStatueAnimations.StartingWith(ChozoStatueProgram.WreckedShipInitial), AnimationPlace.StartOfAnimation);
            PublishHardcodedChozoObject(RoomObjectHeaderId.WreckedShipChozoHand, ChozoStatueRoomDefinitions.WreckedShipHandBlock);
            PublishHardcodedChozoObject(RoomObjectHeaderId.BlockSlopeAccessForWreckedShipChozo, ChozoStatueRoomDefinitions.WreckedShipSlopeAccessBlock);
        }
        else
        {
            state.Playback = new(ChozoStatueAnimations.StartingWith(ChozoStatueProgram.LowerNorfairInitial), AnimationPlace.StartOfAnimation);
            PublishHardcodedChozoObject(RoomObjectHeaderId.LowerNorfairChozoHand, ChozoStatueRoomDefinitions.LowerNorfairHandBlock);
        }
    }

    /// <summary>
    /// Represents the exact enemy-slot side effect produced by the Chozo hand
    /// trigger. Collision/pose/item admission remains the room object's responsibility; once
    /// admitted, both trigger setups write parameter one and disable Samus controls.
    /// </summary>
    public void ActivateChozoStatueHandTrigger(RoomLevelData level, int? collisionBlockIndex = null)
    {
        Ensure.NotNull(level);
        _room.EnsureLoaded();
        RoomEnemySlot statue = _statue ?? throw new InvalidOperationException("Chozo hand trigger requires a placed Chozo statue.");
        if (statue.DefinitionId != EnemyDefinitionId.Chozo)
            throw new InvalidOperationException("Chozo hand trigger requires the Chozo statue in its place.");

        ChozoStatueState state = statue.RequireSpeciesState<ChozoStatueState>();
        int triggerBlockX = state.IsWreckedShip ? ChozoStatueRoomDefinitions.WreckedShipHandBlock.X : ChozoStatueRoomDefinitions.LowerNorfairTriggerX;
        int triggerBlockY = state.IsWreckedShip ? ChozoStatueRoomDefinitions.WreckedShipHandBlock.Y : ChozoStatueRoomDefinitions.LowerNorfairTriggerY;
        int triggerBlockIndex = collisionBlockIndex ?? level.GetBlockIndex(new(CoordinateAxis.Wrap(triggerBlockX), CoordinateAxis.Wrap(triggerBlockY)));
        RoomCollisionBlock triggerBlock = level.GetCollisionBlockByIndex(triggerBlockIndex);

        // Both setup routines clear only the block's collision kind. Its behaviour and
        // appearance survive until their subsequently spawned hardcoded room object runs.
        level.SetTerrain(
            triggerBlockIndex,
            triggerBlock.Terrain.WithCollision(RoomCollisionType.Air));
        state.HandTriggered = true;
        DisableChozoStatueSamusControls();
        if (!state.IsWreckedShip)
        {
            // Lower Norfair's hand trigger records the acid-lowered event before waking the actor.
            _room.Services.Events.Set(StoryEvent.LowerNorfairChozoLoweredAcid);
            PublishHardcodedChozoObject(
                RoomObjectHeaderId.CrumbleLowerNorfairChozoRoomPlug,
                ChozoStatueRoomDefinitions.LowerNorfairHandBlock);
        }
        else
        {
            // The Wrecked Ship hand sets two pairs of scroll cells before handing the statue to
            // its carry: cells 7 and 8 become green and 13 and 14 blue. The clear-slope-access room object then
            // queues the authored slope-access terrain transition.
            _room.Services.RoomScroll.Write(7, RoomScrollState.Green);
            _room.Services.RoomScroll.Write(8, RoomScrollState.Green);
            _room.Services.RoomScroll.Write(13, RoomScrollState.Blue);
            _room.Services.RoomScroll.Write(14, RoomScrollState.Blue);
            PublishHardcodedChozoObject(
                RoomObjectHeaderId.ClearSlopeAccessForWreckedShipChozo,
                ChozoStatueRoomDefinitions.WreckedShipSlopeAccessBlock);
        }
    }

    /// <summary>Dispatches statue phases, including boss-gated movement and authored scene transitions.</summary>
    private void RunChozoStatueMain(RoomEnemySlot statue, ChozoStatueState state)
    {
        switch (state.FrameBehavior)
        {
            case ChozoStatueFrameBehavior.Idle:
            case ChozoStatueFrameBehavior.Cleared:
            case ChozoStatueFrameBehavior.Walking:
                return;

            case ChozoStatueFrameBehavior.WaitForLowerNorfairHandTrigger:
                if (state.HandTriggered)
                {
                    state.Playback = new(ChozoStatueAnimations.StartingWith(ChozoStatueProgram.LowerNorfairActivated), AnimationPlace.StartOfAnimation);
                    statue.FrameHold = WrappingCountdown.Start(1);
                }
                return;

            case ChozoStatueFrameBehavior.WaitForWreckedShipHandTrigger:
                if (_room.Services.Bosses.IsDefeated(BossBits.AreaBoss) && state.HandTriggered)
                {
                    state.Playback = new(ChozoStatueAnimations.StartingWith(ChozoStatueProgram.WreckedShipActivated), AnimationPlace.StartOfAnimation);
                    statue.FrameHold = WrappingCountdown.Start(1);
                }
                return;

            default:
                throw new InvalidDataException(
                    $"Chozo statue frame behaviour {state.FrameBehavior?.ToString() ?? "none"} is not translated.");
        }
    }

    /// <summary>
    /// Carries out one cue of the Chozo statues' animations: every callback reachable from lists
    /// the four statue animations. Every cue runs on to the next step, so none redirects.
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
            case ChozoStatueCue.SetFrameBehavior setFrameBehavior:
                state.FrameBehavior = setFrameBehavior.FrameBehavior;
                return null;

            case ChozoStatueCue.Command { Kind: ChozoStatueCueKind.ClearFrameBehavior }:
                state.FrameBehavior = ChozoStatueFrameBehavior.Cleared;
                return null;

            case ChozoStatueCue.Command { Kind: ChozoStatueCueKind.StartLoweringAcid }:
                ChozoStatueAcidLoweringStarted = true;
                // The cue writes the liquid words consumed by the liquid handler once, when
                // it runs; the liquid owns them from then on.
                _roomFx.FxOverlay?.SetLiquidMotion(
                    timer: ChozoStatueFxData.LoweringDelay, yVelocity: SignedEightEight.FromSignedRaw(ChozoStatueFxData.LoweringVelocity));
                return null;

            case ChozoStatueCue.Command { Kind: ChozoStatueCueKind.SetLoweredAcidPosition }:
                _roomFx.FxOverlay?.SetLiquidMotion(baseYPosition: ChozoStatueRoomDefinitions.LoweredAcidY);
                return null;

            case ChozoStatueCue.Command { Kind: ChozoStatueCueKind.UnlockSamus }:
                EnableChozoStatueSamusControls();
                return null;

            case ChozoStatueCue.Command { Kind: ChozoStatueCueKind.PlayGrabSound }:
                _audio.QueueEnemySound(ChozoGrabsSamusSound, priority: SoundRequestPriority.Normal);
                return null;

            case ChozoStatueCue.Command { Kind: ChozoStatueCueKind.PlayFootstepSound }:
                _audio.QueueEnemySound(ChozoFootstepsSound, priority: SoundRequestPriority.Normal);
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

        // The slope-ignoring horizontal mover receives a signed 8.8 speed promoted to 16.16 by
        // a shift by 256. The vertical mover receives its absolute magnitude in the same format.
        _ = _terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(
            level,
            statue,
            SignedSixteenSixteen.FromTwoHundredFiftySixthsOfPixel(signedVelocity));
        _ = _terrain.MoveEnemyVertically(
            level,
            statue,
            SignedSixteenSixteen.FromTwoHundredFiftySixthsOfPixel(Math.Abs(signedVelocity)));
        EnemyTerrain.AlignEnemyYWithNonSquareSlope(level, statue);

        // The two 32-word tables are joint offsets indexed by the same record.
        // Writing Samus after collision reproduces the cutscene's absolute ownership.
        samus.Position = samus.Position.WithWhole(new(CoordinateAxis.Wrap(statue.Position.X.Whole + motion.SamusOffset.X), CoordinateAxis.Wrap(statue.Position.Y.Whole + motion.SamusOffset.Y)));
    }

    /// <summary>Spawns a plug-crumble room object and spike-footstep only when the offset floor block has the special type.</summary>
    private void ProcessChozoStatueFootstep(
        RoomEnemySlot statue,
        RoomLevelData level,
        Offset16<RoomPixels> footstepOffset)
    {
        Position16<RoomPixels> probe = new(
            CoordinateAxis.Wrap(statue.Position.X.Whole + footstepOffset.X), CoordinateAxis.Wrap(statue.Position.Y.Whole + 28));
        RoomCollisionBlock current = level.GetCollisionBlockAtPixel(probe);
        int belowIndex = current.Index + level.Size.X;
        if (!level.IsLogicalBlockIndex(belowIndex) ||
            level.GetCollisionBlockByIndex(belowIndex).CollisionType != RoomCollisionType.SpikeBlock)
        {
            return;
        }

        ChozoStatueObjectRequests.Add(new ChozoStatueObjectRequest(
            RoomObjectHeaderId.CrumbleLowerNorfairChozoRoomPlug,
            probe.ToRoomBlock(),
            IsHardcoded: false));
        SpawnWreckedShipChozoFootstep(statue, footstepOffset);
    }

    /// <summary>Places the Wrecked Ship footstep projectile beneath the statue at its signed horizontal offset.</summary>
    internal void SpawnWreckedShipChozoFootstep(RoomEnemySlot statue, Offset16<RoomPixels> footstepOffset)
    {
        EnemyProjectile? projectile = _projectiles.AllocateEnemyProjectile();
        if (projectile is null)
            return;

        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            projectile,
            RoomEnemyProjectileKind.WreckedShipChozoSpikeFootstep,
            graphics: default);
        projectile.Position = projectile.Position.WithWhole(new(CoordinateAxis.Wrap(statue.Position.X.Whole + footstepOffset.X), CoordinateAxis.Wrap(statue.Position.Y.Whole + 28)));
    }

    /// <summary>Restores Samus controls, publishes the overlapping scroll writes and queues slope-access terrain.</summary>
    private void FinishWreckedShipChozoSequence()
    {
        EnableChozoStatueSamusControls();

        // The game sets overlapping pairs of scroll cells from cells 6, 8, 9
        // and 13. Publish the resulting cells (6..10 = 0, 13 = 1, 14 = 0) exactly.
        for (int index = 6; index <= 10; index++)
            _room.Services.RoomScroll.Write(index, RoomScrollState.RedBoundary);
        _room.Services.RoomScroll.Write(13, RoomScrollState.Blue);
        _room.Services.RoomScroll.Write(14, RoomScrollState.RedBoundary);

        PublishHardcodedChozoObject(
            RoomObjectHeaderId.BlockSlopeAccessForWreckedShipChozo,
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

    /// <summary>Queues an authored room-object spawn at its fixed room block.</summary>
    private void PublishHardcodedChozoObject(RoomObjectHeaderId header, Position16<RoomBlocks> block) =>
        ChozoStatueObjectRequests.Add(new ChozoStatueObjectRequest(
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
        EnemyProgramAnimation.Advance(slot, ChozoStatueAnimations.Segments, ChozoStatueFrameDefinitions.ShownFrames.Of,
            static (ChozoStatueState state) => state.Playback, static (state, playback) => state.Playback = playback,
            (state, cue) => ApplyChozoStatueCue(slot, cue, context.Samus, context.Level) is { } target
                ? new AnimationContinuation<ChozoStatueProgram>(ChozoStatueAnimations.StartingWith(target), AnimationPlace.StartOfAnimation)
                : null);
}
