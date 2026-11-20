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
    public EnemyOccupant Create(RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common) => EnemyOccupant.Of(new ChozoStatueEntity(this, occupant, place, common));

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
    /// <summary>The enemy pass's events, among which the statue asks for its room objects as it decides on them.</summary>
    private readonly EnemyPassEvents _passEvents;
    /// <summary>The room's statue, which its hand belongs to, once the statue is placed (#2283).</summary>
    private RoomEnemyPlace? _statue;

    /// <summary>Binds statue behavior to room, movement, FX, sound and projectile owners.</summary>
    internal ChozoStatueBehavior(EnemyRoomBinding room, EnemyTerrain terrain, EnemyProjectileEngine projectiles, EnemyAudioQueue audio, EnemyRoomFx roomFx,
        EnemyPassEvents passEvents)
    {
        _passEvents = passEvents;
        _room = room;
        _terrain = terrain;
        _projectiles = projectiles;
        _audio = audio;
        _roomFx = roomFx;
    }

    /// <summary>Initializes either shipped statue.</summary>
    internal void InitializeChozoStatue(RoomEnemyOccupant occupant, RoomEnemyPlace statue)
    {
        ChozoStatueSite site = occupant.SpawnSettings<ChozoStatueSpawnSettings>().Site;
        if (!Enum.IsDefined(site))
            throw new InvalidOperationException($"Undefined {nameof(ChozoStatueSite)} {site}.");

        // The setup makes the statue solid to Samus and animates it, on screen or off.
        statue.Contact = statue.Contact.With(EnemyContact.SolidToSamus);
        statue.Processing = statue.Processing.With(EnemyProcessing.Animates | EnemyProcessing.OffScreen);
        statue.Frame = EnemyFrameId.PointAtOrigin;
        statue.AnimationClock.Restart();

        // The statue shows its own colours, the shade of its site's statue, which no other enemy shows (#1512).
        statue.Shading = PictureShading.Shaded(site == ChozoStatueSite.WreckedShip ? ChozoAndTubeShades.WreckedShipStatue : ChozoAndTubeShades.LowerNorfairStatue);

        var state = new ChozoStatueState { Phase = ChozoStatuePhase.Placed, Site = site };
        statue.SetSpeciesState(state);
        _statue = statue;

        if (site == ChozoStatueSite.WreckedShip)
        {
            state.Playback = new(ChozoStatueAnimations.StartingWith(ChozoStatueSegment.WreckedShipIdle), AnimationPlace.StartOfAnimation);
            RequestChozoObject(ChozoStatueRoomObject.WreckedShipHand, ChozoStatueRoomDefinitions.WreckedShipHandBlock);
            RequestChozoObject(ChozoStatueRoomObject.SlopeAccessClosing, ChozoStatueRoomDefinitions.WreckedShipSlopeAccessBlock);
        }
        else
        {
            state.Playback = new(ChozoStatueAnimations.StartingWith(ChozoStatueSegment.LowerNorfairIdle), AnimationPlace.StartOfAnimation);
            RequestChozoObject(ChozoStatueRoomObject.LowerNorfairHand, ChozoStatueRoomDefinitions.LowerNorfairHandBlock);
        }
    }

    /// <summary>
    /// Wakes the statue once Samus triggers its hand: the statue's wait moves on
    /// (<see cref="ChozoStatueState.TriggerHand"/>), Samus's controls are taken and the site's room changes are made.
    /// Which touches the hand admits and the hand block's collision are the room object's.
    /// </summary>
    /// <returns>The room objects the trigger asks its room for, in the order it asks, which the room spawns at once.</returns>
    public IReadOnlyList<RoomObjectSpawnRequest<ChozoStatueRoomObject>> ActivateChozoStatueHandTrigger()
    {
        RoomEnemyPlace statue = _statue ?? throw new InvalidOperationException("A Chozo hand triggered in a room whose statue was never placed.");
        ChozoStatueState statueState = statue.RequireSpeciesState<ChozoStatueState>();
        statueState.TriggerHand();
        DisableChozoStatueSamusControls();
        if (statueState.Site == ChozoStatueSite.LowerNorfair)
        {
            // Lower Norfair's hand trigger records the acid-lowered event before waking the actor.
            _room.Services.Events.Set(StoryEvent.LowerNorfairChozoLoweredAcid);
            return [new(ChozoStatueRoomDefinitions.LowerNorfairHandBlock, ChozoStatueRoomObject.PlugCrumbleAtHand)];
        }
        else
        {
            // The Wrecked Ship hand opens two pairs of the room's screens before handing the statue to its carry: two
            // open, then the two below them open with the camera aligned to their top. The slope access's opening then queues its terrain change.
            foreach (Position16<RoomScreens> screen in ChozoStatueRoomDefinitions.WakingOpenScreens)
                _room.Services.RoomScroll.Write(screen, RoomScrollState.Open);
            foreach (Position16<RoomScreens> screen in ChozoStatueRoomDefinitions.WakingTopAlignedScreens)
                _room.Services.RoomScroll.Write(screen, RoomScrollState.OpenAlignedToTop);
            return [new(ChozoStatueRoomDefinitions.WreckedShipSlopeAccessBlock, ChozoStatueRoomObject.SlopeAccessOpening)];
        }
    }

    /// <summary>
    /// Starts the statue's activation once Samus has triggered its hand while it waits for her; the Wrecked Ship's hand
    /// admits her only once Phantoon is defeated. The animation does the rest.
    /// </summary>
    private static void RunChozoStatueMain(RoomEnemyOccupant occupant, RoomEnemyPlace statue, ChozoStatueState state)
    {
        if (state.Phase != ChozoStatuePhase.HandTriggered)
            return;

        state.Playback = new(ChozoStatueAnimations.StartingWith(occupant.SpawnSettings<ChozoStatueSpawnSettings>().Site == ChozoStatueSite.WreckedShip
            ? ChozoStatueSegment.WreckedShipActivated
            : ChozoStatueSegment.LowerNorfairActivated), AnimationPlace.StartOfAnimation);
        statue.AnimationClock.StepOnNextUpdate();
        state.Phase = ChozoStatuePhase.Activated;
    }

    /// <summary>
    /// Carries out one cue of the Chozo statues' animations: every cue
    /// the four statue animations raise. Every cue runs on to the next step, so none redirects.
    /// </summary>
    private void ApplyChozoStatueCue(
        RoomEnemyPlace statue, ChozoStatueState state,
        ChozoStatueCue cue,
        SamusState samus,
        RoomLevelData level)
    {
        switch (cue)
        {
            case ChozoStatueCue.Signal { Kind: ChozoStatueCueKind.AwaitHand }:
                state.AwaitHand();
                return;

            case ChozoStatueCue.Signal { Kind: ChozoStatueCueKind.StartLoweringAcid }:
                // The cue writes the liquid motion values consumed by the liquid's step once, when
                // it runs; the liquid owns them from then on.
                _roomFx.FxOverlay.MoveLiquidAfter(ChozoStatueFxData.LoweringDelay, ChozoStatueFxData.LoweringVelocity);
                return;

            case ChozoStatueCue.Signal { Kind: ChozoStatueCueKind.SetLoweredAcidPosition }:
                _roomFx.FxOverlay.SetLiquidLevel(ChozoStatueRoomDefinitions.LoweredAcidY);
                return;

            case ChozoStatueCue.Signal { Kind: ChozoStatueCueKind.UnlockSamus }:
                EnableChozoStatueSamusControls();
                return;

            case ChozoStatueCue.Signal { Kind: ChozoStatueCueKind.PlayGrabSound }:
                _audio.QueueEnemySound(EnemySoundEffectDefinitions.ChozoGrabsSamusSound, priority: SoundRequestPriority.Normal);
                return;

            case ChozoStatueCue.Signal { Kind: ChozoStatueCueKind.PlayFootstepSound }:
                _audio.QueueEnemySound(EnemySoundEffectDefinitions.ChozoFootstepsSound, priority: SoundRequestPriority.Normal);
                return;

            case ChozoStatueCue.Footstep footstep:
                ProcessChozoStatueFootstep(
                    statue,
                    level,
                    footstep.OffsetX);
                return;

            case ChozoStatueCue.Move move:
                ProcessChozoStatueMovement(
                    statue,
                    samus,
                    level,
                    move.Step);
                return;

            case ChozoStatueCue.Signal { Kind: ChozoStatueCueKind.ReleaseSamus }:
                FinishWreckedShipChozoSequence();
                return;

            default:
                throw new InvalidOperationException($"Undefined {nameof(ChozoStatueCueKind)} {cue}.");
        }
    }

    /// <summary>Moves the statue through terrain, then places Samus at the authored carry offset from its accepted position.</summary>
    private void ProcessChozoStatueMovement(
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
        // Samus is placed at the hold from the statue's position after its terrain movement.
        samus.Position = samus.Position.WithWhole(new(statue.Position.X.Whole + motion.SamusOffset.X, statue.Position.Y.Whole + motion.SamusOffset.Y));
    }

    /// <summary>Spawns the plug crumble and the spike-footstep dust when the block below the footstep is a spike block.</summary>
    private void ProcessChozoStatueFootstep(
        RoomEnemyPlace statue,
        RoomLevelData level,
        int footstepOffsetX)
    {
        Position16<RoomPixels> probe = new(
            statue.Position.X.Whole + footstepOffsetX, statue.Position.Y.Whole + ChozoStatueRoomDefinitions.FootstepDepth);
        Position16<RoomBlocks> footstep = level.GridBlock(probe.ToRoomBlock());
        Position16<RoomBlocks> below = footstep + Offset16<RoomBlocks>.OneDown;
        if (!level.Size.Contains(below) || level.GetCollisionBlock(below).CollisionType != RoomCollisionType.SpikeBlock)
            return;

        RequestChozoObject(ChozoStatueRoomObject.PlugCrumbleUnderFootstep, footstep);
        SpawnWreckedShipChozoFootstep(statue, footstepOffsetX);
    }

    /// <summary>Places the Wrecked Ship footstep projectile beneath the statue at its signed horizontal offset.</summary>
    internal void SpawnWreckedShipChozoFootstep(RoomEnemyPlace statue, int footstepOffsetX)
    {
        EnemyProjectile? projectile = _projectiles.AllocateEnemyProjectile(RoomEnemyProjectileKind.WreckedShipChozoSpikeFootstep, EnemyProjectileBehavior.Still, graphics: SpriteGraphics.SharedStandard);
        if (projectile is null)
            return;

        projectile.StartAnimation(new ChozoTourianDustAnimation(ChozoTourianDustAnimations.StartingWith(ChozoTourianDustSegment.Footsteps)));
        projectile.Position = projectile.Position.WithWhole(new(statue.Position.X.Whole + footstepOffsetX, statue.Position.Y.Whole + ChozoStatueRoomDefinitions.FootstepDepth));
    }

    /// <summary>Restores Samus controls, closes and opens the room's screens behind her and queues the slope access's closing.</summary>
    private void FinishWreckedShipChozoSequence()
    {
        EnableChozoStatueSamusControls();

        // The release leaves five screens of the second row closed, then the third row's second screen open with the
        // camera aligned to its top and its third closed.
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

    /// <summary>
    /// Asks the room to spawn <paramref name="roomObject"/> at <paramref name="block"/>: the room spawns it after the
    /// statue's setup or the enemy pass that asks.
    /// </summary>
    private void RequestChozoObject(ChozoStatueRoomObject roomObject, Position16<RoomBlocks> block) =>
        _passEvents.Publish(new ChozoStatueObjectAsked(new RoomObjectSpawnRequest<ChozoStatueRoomObject>(block, roomObject)));

    /// <summary>Runs ChozoStatue's main AI.</summary>
    internal static void RunChozoStatueMainAi(RoomEnemyOccupant occupant, RoomEnemyPlace place, ChozoStatueState chozoStatueState)
    {
        RunChozoStatueMain(occupant, place, chozoStatueState);
    }

    /// <summary>Advances ChozoStatue's typed animation.</summary>
    internal void AdvanceChozoStatueTypedAnimation(RoomEnemyPlace place, ChozoStatueState chozoStatueState2, ChozoStatueState chozoStatueState, EnemyAnimationCueContext context) =>
        EnemyAnimationPlayback.AdvanceApplyingCues(place, chozoStatueState2, ChozoStatueAnimations.Segments,
            static (ChozoStatueState state) => state.Playback, static (state, playback) => state.Playback = playback,
            (ChozoStatueState _, ChozoStatueCue cue) => ApplyChozoStatueCue(place, chozoStatueState, cue, context.Samus, context.Level));
}
