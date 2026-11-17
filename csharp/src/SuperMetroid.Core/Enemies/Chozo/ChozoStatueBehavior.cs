using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Animation;
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

/// <summary>The Chozo statue species owner: its per-room state, behaviour and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class ChozoStatueBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.Chozo];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemyPlace place, EnemyCommonParticipation common) => new ChozoStatueEntity(this, place, common);

    /// <summary>Supplies room events, scroll grid and Samus-control service.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Clips walking-statue movement before the carried Samus position is written.</summary>
    private readonly EnemyTerrain _terrain;
    /// <summary>Allocates the Wrecked Ship statue's spike-footstep projectile.</summary>
    private readonly EnemyProjectileEngine _projectiles;
    /// <summary>Queues enemy-library footstep and walking sounds from cues.</summary>
    private readonly EnemyAudioQueue _audio;
    /// <summary>Receives statue-authored effect overlay motion writes.</summary>
    private readonly EnemyRoomFx _roomFx;
    /// <summary>The state of the room's statue, which its hand belongs to, once the statue is placed (#2283).</summary>
    private ChozoStatueState? _statue;

    /// <summary>Binds statue behavior to room, movement, FX, sound and projectile owners.</summary>
    internal ChozoStatueBehavior(EnemyRoomBinding room, EnemyTerrain terrain, EnemyProjectileEngine projectiles, EnemyAudioQueue audio, EnemyRoomFx roomFx)
    {
        _room = room;
        _terrain = terrain;
        _projectiles = projectiles;
        _audio = audio;
        _roomFx = roomFx;
    }

    /// <summary>The room objects the statue has asked for and the room has still to spawn, in the order it asked.</summary>
    private readonly List<RoomObjectSpawnRequest<ChozoStatueRoomObject>> _objectRequests = [];

    /// <summary>Spawns each room object the statue asked for, once, in the order it asked.</summary>
    internal void ApplyPendingChozoStatueObjects(RoomLevelData level, ChozoStatueObjects chozoStatueObjects)
    {
        while (_objectRequests.Count != 0)
        {
            chozoStatueObjects.TrySpawnChozoStatueObject(level, _objectRequests[0]);
            _objectRequests.RemoveAt(0);
        }
    }

    /// <summary>Initializes either shipped statue.</summary>
    internal void InitializeChozoStatue(RoomEnemyPlace statue)
    {
        ChozoStatueSite site = statue.SpawnSettings<ChozoStatueSpawnSettings>().Site;
        bool isWreckedShip = site switch
        {
            ChozoStatueSite.WreckedShip => true,
            ChozoStatueSite.LowerNorfair => false,
            _ => throw new InvalidOperationException($"Undefined {nameof(ChozoStatueSite)} {site}."),
        };

        // The initializer makes the statue solid to Samus and animates it, on screen or off.
        statue.Contact = statue.Contact.With(EnemyContact.SolidToSamus);
        statue.Processing = statue.Processing.With(EnemyProcessing.Animates | EnemyProcessing.OffScreen);
        statue.Frame = EnemyFrameId.Empty;
        statue.AnimationClock.Restart();

        // The statue shows its own colours, the shade of its site's statue, which no other enemy shows (#1512).
        statue.Shading = PictureShading.Shaded(isWreckedShip ? ChozoAndTubeShades.WreckedShipStatue : ChozoAndTubeShades.LowerNorfairStatue);

        var state = new ChozoStatueState
        {
            IsWreckedShip = isWreckedShip,
            HandTriggered = false,
        };
        statue.SetSpeciesState(state);
        _statue = state;

        if (state.IsWreckedShip)
        {
            state.Playback = new(ChozoStatueAnimations.StartingWith(ChozoStatueSegment.WreckedShipInitial), AnimationPlace.StartOfAnimation);
            RequestChozoObject(ChozoStatueRoomObject.WreckedShipHand, ChozoStatueRoomDefinitions.WreckedShipHandBlock);
            RequestChozoObject(ChozoStatueRoomObject.SlopeAccessClosing, ChozoStatueRoomDefinitions.WreckedShipSlopeAccessBlock);
        }
        else
        {
            state.Playback = new(ChozoStatueAnimations.StartingWith(ChozoStatueSegment.LowerNorfairInitial), AnimationPlace.StartOfAnimation);
            RequestChozoObject(ChozoStatueRoomObject.LowerNorfairHand, ChozoStatueRoomDefinitions.LowerNorfairHandBlock);
        }
    }

    /// <summary>
    /// Represents the exact enemy-place side effect produced by the Chozo hand
    /// trigger. Collision/pose/item admission remains the room object's responsibility; once
    /// admitted, both trigger setups mark the hand triggered and disable Samus controls.
    /// </summary>
    /// <param name="level">The room's level, whose hand block loses its collision.</param>
    /// <param name="handBlock">The hand block the room object that admitted the trigger stands on.</param>
    public void ActivateChozoStatueHandTrigger(RoomLevelData level, Position16<RoomBlocks> handBlock)
    {
        Ensure.NotNull(level);
        ChozoStatueState state = _statue ?? throw new InvalidOperationException("A Chozo hand triggered in a room whose statue was never placed.");
        Position16<RoomBlocks> triggerPosition = handBlock;
        RoomCollisionBlock triggerBlock = level.GetCollisionBlock(triggerPosition);

        // Both setup steps clear only the block's collision kind. Its behaviour and
        // appearance survive until the room object the statue then spawns runs.
        level.SetTerrain(
            triggerPosition,
            triggerBlock.Terrain.WithCollision(RoomCollisionType.Air));
        state.HandTriggered = true;
        DisableChozoStatueSamusControls();
        if (!state.IsWreckedShip)
        {
            // Lower Norfair's hand trigger records the acid-lowered event before waking the actor.
            _room.Services.Events.Set(StoryEvent.LowerNorfairChozoLoweredAcid);
            RequestChozoObject(ChozoStatueRoomObject.PlugCrumbleAtHand, ChozoStatueRoomDefinitions.LowerNorfairHandBlock);
        }
        else
        {
            // The Wrecked Ship hand opens two pairs of the room's screens before handing the statue to its carry: two
            // open, then the two below them open with the camera aligned to their top. The slope access's opening then queues its terrain change.
            foreach (Position16<RoomScreens> screen in ChozoStatueRoomDefinitions.WakingOpenScreens)
                _room.Services.RoomScroll.Write(screen, RoomScrollState.Open);
            foreach (Position16<RoomScreens> screen in ChozoStatueRoomDefinitions.WakingTopAlignedScreens)
                _room.Services.RoomScroll.Write(screen, RoomScrollState.OpenAlignedToTop);
            RequestChozoObject(ChozoStatueRoomObject.SlopeAccessOpening, ChozoStatueRoomDefinitions.WreckedShipSlopeAccessBlock);
        }
    }

    /// <summary>
    /// Starts the statue's activation once it is awaiting Samus at its hand and she has triggered it: at once in Lower
    /// Norfair, and in the Wrecked Ship only once Phantoon is defeated. The animation does the rest.
    /// </summary>
    private void RunChozoStatueMain(RoomEnemyPlace statue, ChozoStatueState state)
    {
        if (!state.AwaitingHand || !state.HandTriggered)
            return;
        if (state.IsWreckedShip && !_room.Services.Bosses.IsDefeated(DefeatedBosses.AreaBoss))
            return;

        state.Playback = new(ChozoStatueAnimations.StartingWith(state.IsWreckedShip
            ? ChozoStatueSegment.WreckedShipActivated
            : ChozoStatueSegment.LowerNorfairActivated), AnimationPlace.StartOfAnimation);
        statue.AnimationClock.StepOnNextUpdate();
    }

    /// <summary>
    /// Carries out one cue of the Chozo statues' animations: every cue
    /// the four statue animations raise. Every cue runs on to the next step, so none redirects.
    /// </summary>
    private ChozoStatueSegment? ApplyChozoStatueCue(
        RoomEnemyPlace statue,
        ChozoStatueCue cue,
        SamusState samus,
        RoomLevelData level)
    {
        ChozoStatueState state = statue.RequireSpeciesState<ChozoStatueState>();
        switch (cue)
        {
            case ChozoStatueCue.Signal { Kind: ChozoStatueCueKind.AwaitHand }:
                state.AwaitingHand = true;
                return null;

            case ChozoStatueCue.Signal { Kind: ChozoStatueCueKind.StopAwaitingHand }:
                state.AwaitingHand = false;
                return null;

            case ChozoStatueCue.Signal { Kind: ChozoStatueCueKind.StartLoweringAcid }:
                // The cue writes the liquid motion values consumed by the liquid's step once, when
                // it runs; the liquid owns them from then on.
                _roomFx.FxOverlay.MoveLiquidAfter(ChozoStatueFxData.LoweringDelay, ChozoStatueFxData.LoweringVelocity);
                return null;

            case ChozoStatueCue.Signal { Kind: ChozoStatueCueKind.SetLoweredAcidPosition }:
                _roomFx.FxOverlay.SetLiquidLevel(ChozoStatueRoomDefinitions.LoweredAcidY);
                return null;

            case ChozoStatueCue.Signal { Kind: ChozoStatueCueKind.UnlockSamus }:
                EnableChozoStatueSamusControls();
                return null;

            case ChozoStatueCue.Signal { Kind: ChozoStatueCueKind.PlayGrabSound }:
                _audio.QueueEnemySound(EnemySoundEffectDefinitions.ChozoGrabsSamusSound, priority: SoundRequestPriority.Normal);
                return null;

            case ChozoStatueCue.Signal { Kind: ChozoStatueCueKind.PlayFootstepSound }:
                _audio.QueueEnemySound(EnemySoundEffectDefinitions.ChozoFootstepsSound, priority: SoundRequestPriority.Normal);
                return null;

            case ChozoStatueCue.Footstep footstep:
                ProcessChozoStatueFootstep(
                    statue,
                    level,
                    footstep.OffsetX);
                return null;

            case ChozoStatueCue.Move move:
                ProcessChozoStatueMovement(
                    statue,
                    samus,
                    level,
                    move.Step);
                return null;

            case ChozoStatueCue.Signal { Kind: ChozoStatueCueKind.ReleaseSamus }:
                FinishWreckedShipChozoSequence();
                return null;

            default:
                throw new InvalidOperationException($"Undefined {nameof(ChozoStatueCueKind)} {cue}.");
        }
    }

    /// <summary>Moves the statue through terrain, then places Samus at the authored carry offset from its accepted position.</summary>
    internal void ProcessChozoStatueMovement(
        RoomEnemyPlace statue,
        SamusState samus,
        RoomLevelData level,
        ChozoCarryStep carryStep)
    {
        var motion = ChozoCarry.Read(carryStep);

        // The slope-ignoring horizontal mover receives the signed velocity; the vertical mover receives its speed,
        // whichever way the statue walks, so a carrying statue always presses down onto the floor.
        _ = _terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(
            level,
            statue,
            motion.Velocity.ToSixteenSixteenDelta());
        _ = _terrain.MoveEnemyVertically(
            level,
            statue,
            motion.Velocity.Magnitude().ToSixteenSixteenDelta());
        EnemyTerrain.AlignEnemyYWithNonSquareSlope(level, statue);

        // The hand offset belongs to the same carry step as the velocity.
        // Writing Samus after collision reproduces the cutscene's absolute ownership.
        samus.Position = samus.Position.WithWhole(new(CoordinateAxis.Wrap(statue.Position.X.Whole + motion.SamusOffset.X), CoordinateAxis.Wrap(statue.Position.Y.Whole + motion.SamusOffset.Y)));
    }

    /// <summary>Spawns a plug-crumble room object and spike-footstep only when the offset floor block has the special type.</summary>
    private void ProcessChozoStatueFootstep(
        RoomEnemyPlace statue,
        RoomLevelData level,
        int footstepOffsetX)
    {
        Position16<RoomPixels> probe = new(
            CoordinateAxis.Wrap(statue.Position.X.Whole + footstepOffsetX), CoordinateAxis.Wrap(statue.Position.Y.Whole + ChozoStatueRoomDefinitions.FootstepDepth));
        Position16<RoomBlocks> footstep = level.GridBlock(probe.ToRoomBlock());
        Position16<RoomBlocks> below = footstep + new Offset16<RoomBlocks>(0, 1);
        if (!level.Size.Contains(below) || level.GetCollisionBlock(below).CollisionType != RoomCollisionType.SpikeBlock)
            return;

        RequestChozoObject(ChozoStatueRoomObject.PlugCrumbleUnderFootstep, footstep);
        SpawnWreckedShipChozoFootstep(statue, footstepOffsetX);
    }

    /// <summary>Places the Wrecked Ship footstep projectile beneath the statue at its signed horizontal offset.</summary>
    internal void SpawnWreckedShipChozoFootstep(RoomEnemyPlace statue, int footstepOffsetX)
    {
        EnemyProjectile? projectile = _projectiles.AllocateEnemyProjectile();
        if (projectile is null)
            return;

        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            projectile,
            RoomEnemyProjectileKind.WreckedShipChozoSpikeFootstep,
            EnemyProjectileBehavior.Still,
            graphics: default);
        projectile.StartAnimation(new ChozoTourianDustAnimation(ChozoTourianDustAnimations.StartingWith(ChozoTourianDustSegment.Footsteps)));
        projectile.Position = projectile.Position.WithWhole(new(CoordinateAxis.Wrap(statue.Position.X.Whole + footstepOffsetX), CoordinateAxis.Wrap(statue.Position.Y.Whole + ChozoStatueRoomDefinitions.FootstepDepth)));
    }

    /// <summary>Restores Samus controls, closes and opens the room's screens behind her and queues the slope access's closing.</summary>
    private void FinishWreckedShipChozoSequence()
    {
        EnableChozoStatueSamusControls();

        // The release's overlapping writes leave five screens of the second row closed, then the third row's second
        // screen open with the camera aligned to its top and its third closed.
        foreach (Position16<RoomScreens> screen in ChozoStatueRoomDefinitions.ReleaseClosedScreens)
            _room.Services.RoomScroll.Write(screen, RoomScrollState.Closed);
        _room.Services.RoomScroll.Write(ChozoStatueRoomDefinitions.ReleaseTopAlignedScreen, RoomScrollState.OpenAlignedToTop);
        _room.Services.RoomScroll.Write(ChozoStatueRoomDefinitions.ReleaseClosedLastScreen, RoomScrollState.Closed);

        RequestChozoObject(ChozoStatueRoomObject.SlopeAccessClosing, ChozoStatueRoomDefinitions.WreckedShipSlopeAccessBlock);
    }

    /// <summary>Gives Samus her controls back.</summary>
    private void EnableChozoStatueSamusControls() => _room.Services.SamusControls.EnableControls();

    /// <summary>Takes Samus's controls away while the statue holds her.</summary>
    private void DisableChozoStatueSamusControls() => _room.Services.SamusControls.DisableControls();

    /// <summary>Asks the room to spawn <paramref name="roomObject"/> at <paramref name="block"/>.</summary>
    private void RequestChozoObject(ChozoStatueRoomObject roomObject, Position16<RoomBlocks> block) =>
        _objectRequests.Add(new RoomObjectSpawnRequest<ChozoStatueRoomObject>(block, roomObject));

    /// <summary>Runs ChozoStatue's main AI.</summary>
    internal void RunChozoStatueMainAi(RoomEnemyPlace place, IEnemyAiFrame _)
    {
        RunChozoStatueMain(place, place.RequireSpeciesState<ChozoStatueState>());
    }

    /// <summary>Advances ChozoStatue's typed animation.</summary>
    internal void AdvanceChozoStatueTypedAnimation(RoomEnemyPlace place, EnemyAnimationCueContext context) =>
        EnemyAnimationPlayback.Advance(place, ChozoStatueAnimations.Segments,
            static (ChozoStatueState state) => state.Playback, static (state, playback) => state.Playback = playback,
            (state, cue) => ApplyChozoStatueCue(place, cue, context.Samus, context.Level) is { } target
                ? new AnimationContinuation<ChozoStatueSegment>(ChozoStatueAnimations.StartingWith(target), AnimationPlace.StartOfAnimation)
                : null);
}
