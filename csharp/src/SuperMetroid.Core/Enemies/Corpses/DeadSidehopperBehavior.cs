using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>The dead sidehopper species owner: its per-room state, behaviour and the interactions it registers.</summary>
[RoomLifetime]
internal sealed partial class DeadSidehopperBehavior : IEnemySpecies, IEnemySpawnFactory<DeadSidehopperSpawnSettings>
{

    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.CorpseSidehopper];

    /// <inheritdoc/>
    public EnemyOccupant Create(RoomEnemyOccupant occupant, DeadSidehopperSpawnSettings settings, RoomEnemyPlace place, EnemyCommonParticipation common) => EnemyOccupant.Running(new DeadSidehopperEntity(this, occupant, place, common, settings));

    /// <summary>Provides installed artwork and the room random source for corpse decomposition.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Handles level collision after the scripted opening movement reaches the terrain boundary.</summary>
    private readonly EnemyTerrain _terrain;
    /// <summary>The room's Baby Metroid's feeding: once it has fed, the victim is gone.</summary>
    private readonly IBabyMetroidFeeding _babyMetroidFeeding;
    /// <summary>Supplies the enemy counter used to pace dust sound effects.</summary>
    private readonly EnemyFrameContext _frameContext;
    /// <summary>Spawns dust from each completed corpse row.</summary>
    private readonly EnemyProjectileEngine _projectiles;
    /// <summary>Applies normal touch damage before the corpse finishes transforming.</summary>
    private readonly EnemyCollision _collision;

    /// <summary>Raises each fallen row's dust and its sound request.</summary>
    private readonly CorpseRowDust _rowDust;

    /// <summary>Connects the corpse actor to room state, terrain, presentation, and interaction services.</summary>
    internal DeadSidehopperBehavior(DeadSidehopperBehaviorDependencies dependencies)
    {
        _rowDust = dependencies.RowDust;
        _room = dependencies.Room;
        _terrain = dependencies.Terrain;
        _babyMetroidFeeding = dependencies.BabyMetroidFeeding;
        _frameContext = dependencies.FrameContext;
        _projectiles = dependencies.Projectiles;
        _collision = dependencies.Collision;
    }

    /// <summary>What <see cref="ApplyDeadSidehopperCue"/> does for each <see cref="DeadTourianCueKind"/> member, each member's own arm.</summary>
    private static IdentityTable<DeadTourianCueKind, Action<DeadSidehopperEnemyState>> ApplyDeadSidehopperCueForDeadTourianCueKind => field ??= new(new Dictionary<DeadTourianCueKind, Action<DeadSidehopperEnemyState>>
    {
        [DeadTourianCueKind.EndHop] = static state =>
        {
            // The end-hop cue ends the landing animation and hands ownership back to main AI.
            SelectDeadSidehopperPostAnimationState(state);
        },
    });

    /// <summary>Carries out the dead Sidehopper's animation cue.</summary>
    private static void ApplyDeadSidehopperCue(DeadSidehopperEnemyState deadSidehopperEnemyState, DeadTourianCue cue)
    {
        ApplyDeadSidehopperCueForDeadTourianCueKind[cue.Kind](deadSidehopperEnemyState);
    }

    /// <summary>What <see cref="InitializeDeadSidehopper"/> does for each <see cref="DeadSidehopperLayout"/> member, each member's own arm.</summary>
    [field: NonSerialized]
    private IdentityTable<DeadSidehopperLayout, Func<RoomEnemyPlace, DeadSidehopperEnemyState>> InitializeDeadSidehopperForDeadSidehopperLayout => field ??= new(new Dictionary<DeadSidehopperLayout, Func<RoomEnemyPlace, DeadSidehopperEnemyState>>
    {
        [DeadSidehopperLayout.BabyMetroidVictim] = place => InitializeBabyMetroidVictimSidehopper(place),
        [DeadSidehopperLayout.AlreadyDead] = place => InitializeAlreadyDeadSidehopper(place),
    });

    /// <summary>Initializes the dead Sidehopper.</summary>
    /// <returns>The state the corpse holds from its setup.</returns>
    internal DeadSidehopperEnemyState InitializeDeadSidehopper(DeadSidehopperSpawnSettings deadSidehopperSpawnSettings, RoomEnemyPlace place)
    {
        return InitializeDeadSidehopperForDeadSidehopperLayout[deadSidehopperSpawnSettings.Layout](place);
    }

    /// <summary>Runs the Baby Metroid victim layout.</summary>
    private DeadSidehopperEnemyState InitializeBabyMetroidVictimSidehopper(RoomEnemyPlace place)
    {
        // The victim stops being solid to Samus and keeps updating off screen; everything else about its contact stays
        // as the room gave it.
        place.Contact = place.Contact.Without(EnemyContact.SolidToSamus);
        place.Processing = place.Processing.With(EnemyProcessing.OffScreen);
        // A Baby Metroid that has already fed on this Sidehopper leaves no victim.
        if (_babyMetroidFeeding.HasAlreadyFedOn(place))
            place.Presence = place.Presence.With(EnemyPresence.Deleted);

        place.Position = place.Position.WithWhole(DeadSidehopperVictimDefinitions.StartPosition);
        place.Radius = place.Radius.WithY(DeadSidehopperVictimDefinitions.LivingHalfHeight);

        // The victim shows the colours the Baby Metroid's entry gives it, the target the room's colours fade to (#1512).
        DeadSidehopperEnemyState state = InitializeDeadSidehopperCorpseState(DeadSidehopperAction.AliveWaitForCamera,
            new SidehopperVictim(SidehopperDrainStage.Untouched));
        SetDeadSidehopperAnimation(place, state, DeadTourianSegment.SidehopperIdle);
        place.Shading = state.Shading;
        state.Velocity = DeadSidehopperLaunchDefinitions.FirstHop;
        return state;
    }

    /// <summary>Lays out a Sidehopper corpse placed already dead, drained from the start and waiting for Samus to touch it.</summary>
    private static DeadSidehopperEnemyState InitializeAlreadyDeadSidehopper(RoomEnemyPlace place)
    {
        DeadSidehopperEnemyState state = InitializeDeadSidehopperCorpseState(DeadSidehopperAction.WaitForSamusCollision,
            SidehopperCorpseLayout.PlacedDead);
        SetDeadSidehopperAnimation(place, state, DeadTourianSegment.SidehopperInitiallyDead);
        place.Shading = state.Shading;
        return state;
    }

    /// <summary>A corpse's state with its own rot schedule, its own rows, its first action and its layout.</summary>
    private static DeadSidehopperEnemyState InitializeDeadSidehopperCorpseState(DeadSidehopperAction action,
        SidehopperCorpseLayout layout) =>
        new(new CorpseRotSchedule(DeadSidehopperCorpseDefinitions.RowCount), action, layout);

    /// <summary>Runs the dead Sidehopper's main AI.</summary>
    private void RunDeadSidehopperMain(
        RoomEnemyPlace place, DeadSidehopperEnemyState deadSidehopperEnemyState,
        SamusState samus,
        RoomLevelData level,
        Position16<RoomPixels> camera) =>
        RunDeadSidehopperState(place, deadSidehopperEnemyState, samus, level, camera);

    /// <summary>What <see cref="RunDeadSidehopperState"/> does for each <see cref="DeadSidehopperAction"/> member, each member's own arm.</summary>
    [field: NonSerialized]
    private IdentityTable<DeadSidehopperAction, Action<RoomEnemyPlace, DeadSidehopperEnemyState, SamusState, RoomLevelData, Position16<RoomPixels>>> RunDeadSidehopperStateForDeadSidehopperAction => field ??= new(new Dictionary<DeadSidehopperAction, Action<RoomEnemyPlace, DeadSidehopperEnemyState, SamusState, RoomLevelData, Position16<RoomPixels>>>
    {
        [DeadSidehopperAction.AliveWaitForCamera] = (place, state, samus, level, camera) =>
        {
            // The camera reaching the victim starts its hop on the same update.
            if (camera.X < DeadSidehopperVictimDefinitions.ActivationCameraX)
            {
                state.Action = DeadSidehopperAction.ActivatedMovement;
                HopDeadSidehopper(place, state, level);
            }
        },
        [DeadSidehopperAction.ActivatedMovement] = (place, state, samus, level, camera) => HopDeadSidehopper(place, state, level),
        [DeadSidehopperAction.Landing] = (place, state, samus, level, camera) => { },
        [DeadSidehopperAction.BeginPostLandingDelay] = (place, state, samus, level, camera) =>
        {
            state.Action = DeadSidehopperAction.PostLandingDelay;
            state.PostLandingPause = UpdateCountdown.Lasting(DeadSidehopperVictimDefinitions.PostLandingPause);
        },
        [DeadSidehopperAction.PostLandingDelay] = (place, state, samus, level, camera) =>
        {
            if (!state.PostLandingPause.Step())
                return;

            if (state.HasBegunDraining)
            {
                state.Action = DeadSidehopperAction.Draining;
                return;
            }

            state.Action = DeadSidehopperAction.ActivatedMovement;
            SetDeadSidehopperAnimation(place, state, DeadTourianSegment.SidehopperIdle);
            state.Velocity = state.NextHop.Current;
        },
        [DeadSidehopperAction.Draining] = (place, state, samus, level, camera) => StepDeadSidehopperDrain(place, state),
        [DeadSidehopperAction.WaitForSamusCollision] = (place, state, samus, level, camera) =>
        {
            if (samus.Kinematics.DidCollideWithSolidEnemy(place) == true)
                state.Action = DeadSidehopperAction.PreRotDelay;
        },
        [DeadSidehopperAction.PreRotDelay] = (place, state, samus, level, camera) =>
        {
            if (state.PreRotDelay.Step())
            {
                state.Action = DeadSidehopperAction.Rotting;
                place.Contact = place.Contact.With(EnemyContact.IgnoresSamus);
            }
        },
        [DeadSidehopperAction.Rotting] = (place, state, samus, level, camera) => RunDeadSidehopperRotting(place, state),
    });

    /// <summary>Advances the authored hop, post-landing delay, drain, or collision-triggered decay state.</summary>
    internal void RunDeadSidehopperState(
        RoomEnemyPlace place,
        DeadSidehopperEnemyState state,
        SamusState samus,
        RoomLevelData level,
        Position16<RoomPixels> camera)
    {
        RunDeadSidehopperStateForDeadSidehopperAction[state.Action](place, state, samus, level, camera);
    }

    /// <summary>Moves the victim one update through its hop; on landing it counts the hop and plays its landing.</summary>
    private void HopDeadSidehopper(RoomEnemyPlace place, DeadSidehopperEnemyState state, RoomLevelData level)
    {
        if (!MoveDeadSidehopper(place, state, level))
            return;
        state.NextHop = state.NextHop.Next;
        SetDeadSidehopperAnimation(place, state, DeadTourianSegment.SidehopperHopping);
        state.Action = DeadSidehopperAction.Landing;
    }

    /// <summary>
    /// Moves the victim through its hop. Left of <see cref="DeadSidehopperVictimDefinitions.TerrainFromX"/> it moves
    /// directly and lands on the scripted floor; from there on it collides with the room's terrain.
    /// </summary>
    private bool MoveDeadSidehopper(
        RoomEnemyPlace place,
        DeadSidehopperEnemyState state,
        RoomLevelData level)
    {
        bool beforeTerrainBoundary = place.Position.X.Whole < DeadSidehopperVictimDefinitions.TerrainFromX;
        SignedSixteenSixteen horizontalDisplacement = state.Velocity.X.ToSixteenSixteenDelta();
        if (beforeTerrainBoundary)
        {
            MoveDeadSidehopperAxisDirect(
                place,
                axis: Axis.Horizontal,
                state.Velocity.X);
        }
        else
        {
            _terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(
                level,
                place,
                horizontalDisplacement);
        }

        // Upward motion slows and falling motion speeds up, by whether the corpse was rising before this update's change.
        SignedEightEight acceleration = state.Velocity.Y.IsNegative
            ? DeadSidehopperLaunchDefinitions.RisingGravity
            : DeadSidehopperLaunchDefinitions.FallingGravity;
        state.Velocity = state.Velocity.WithY(state.Velocity.Y + acceleration);
        if (beforeTerrainBoundary)
        {
            MoveDeadSidehopperAxisDirect(
                place,
                axis: Axis.Vertical,
                state.Velocity.Y);
            return place.Position.Y.Whole >= DeadSidehopperVictimDefinitions.FloorY;
        }

        SignedSixteenSixteen verticalDisplacement = state.Velocity.Y.ToSixteenSixteenDelta();
        return _terrain.MoveEnemyVertically(
            level,
            place,
            verticalDisplacement);
    }

    /// <summary>Moves the corpse along one axis by its scripted speed before terrain collision takes over.</summary>
    private static void MoveDeadSidehopperAxisDirect(
        RoomEnemyPlace place,
        Axis axis,
        SignedEightEight velocity)
    {
        FixedCoordinate current = axis == Axis.Horizontal ? place.Position.X : place.Position.Y;
        FixedCoordinate moved = current.MovedBy(velocity);
        if (axis == Axis.Horizontal)
        {
            place.Position = place.Position.WithX(moved);
        }
        else
        {
            place.Position = place.Position.WithY(moved);
        }
    }

    /// <summary>Counts one draining update and, once it has counted a stage's updates, steps the drain, which only the victim shows (#1512).</summary>
    private static void StepDeadSidehopperDrain(
        RoomEnemyPlace place,
        DeadSidehopperEnemyState state)
    {
        if (!state.DrainStageWait.Count())
            return;

        state.DrainStageWait.Restart(DeadSidehopperVictimDefinitions.DrainUpdatesPerStage);

        // Only the victim drains: a corpse placed already dead never reaches its drain.
        state.Layout = state.Layout.DrainedOneStep();
        place.Shading = state.Shading;
        if (!state.IsDrained)
            return;

        SetDeadSidehopperAnimation(place, state, DeadTourianSegment.SidehopperCorpse);
        state.Action = DeadSidehopperAction.WaitForSamusCollision;
        place.Contact = place.Contact.With(EnemyContact.SolidToSamus);
        place.Radius = place.Radius.WithY(DeadSidehopperVictimDefinitions.DrainedHalfHeight);
    }

    /// <summary>
    /// The end of the landing animation selects what follows the landing, from whether the Baby Metroid has begun
    /// draining the victim.
    /// </summary>
    private static void SelectDeadSidehopperPostAnimationState(DeadSidehopperEnemyState state)
    {
        state.Action = state.HasBegunDraining
            ? DeadSidehopperAction.Draining
            : DeadSidehopperAction.BeginPostLandingDelay;
    }

    /// <summary>Begins Baby Metroid's drain of its victim, when the Baby Metroid latches on to feed.</summary>
    internal static void ActivateDeadSidehopperVictim(DeadSidehopperEnemyState deadSidehopperEnemyState) =>
        deadSidehopperEnemyState.Layout = new SidehopperVictim(SidehopperDrainStage.LatchedOn);

    /// <summary>Resolves a shot hitting the dead Sidehopper.</summary>
    internal static void ResolveDeadSidehopperShot(RoomEnemyPlace place, DeadSidehopperEnemyState state)
    {
        if (place.Contact.HasAny(EnemyContact.IgnoresSamus) ||
            !state.IsDrained)
        {
            return;
        }
        TriggerDeadSidehopperRotting(place, state);
    }

    /// <summary>Resolves Samus touching the dead Sidehopper.</summary>
    private void ResolveDeadSidehopperTouch(
        RoomEnemyOccupant corpse,
        DeadSidehopperEnemyState state,
        SamusState samus,
        PseudoScrewCharge pseudoScrew)
    {
        RoomEnemyPlace place = corpse.Place;
        if (!state.IsDrained)
        {
            _collision.ResolveOrdinaryTouch(corpse, samus, pseudoScrew);
            return;
        }
        TriggerDeadSidehopperRotting(place, state);
    }

    /// <summary>Resolves a power bomb reaching the dead Sidehopper.</summary>
    internal void ResolveDeadSidehopperPowerBomb(EnemyPowerBombContext context, DeadSidehopperEnemyState state)
    {
        RoomEnemyPlace place = context.Enemy;
        if (state.IsDrained)
        {
            ResolveDeadSidehopperShot(place, state);
            return;
        }

        // Before the corpse is ready, the private power-bomb reaction runs the actor's main step once, reading the
        // room's terrain and the camera as the power-bomb pass sees them this update.
        RunDeadSidehopperState(place, state, context.Samus, context.Level, context.Camera);
    }

    /// <summary>Starts decay and keeps the corpse updating offscreen without further Samus collision.</summary>
    internal static void TriggerDeadSidehopperRotting(
        RoomEnemyPlace place,
        DeadSidehopperEnemyState state)
    {
        state.Action = DeadSidehopperAction.Rotting;
        place.Processing = place.Processing.With(EnemyProcessing.OffScreen);
        place.Contact = place.Contact.With(EnemyContact.IgnoresSamus);
    }

    /// <summary>Runs one rot update and its dust, moving the corpse's own rows as the update moves them (#1539).</summary>
    private void RunDeadSidehopperRotting(
        RoomEnemyPlace place,
        DeadSidehopperEnemyState state)
    {
        bool stillRotting = state.Rot.Step(
            state.Rows.MoveRow,
            _ => FinishDeadSidehopperCorpseRow(place));
        if (!stillRotting)
            state.Action = DeadSidehopperAction.WaitForSamusCollision;
    }

    /// <summary>The rows <paramref name="place"/>'s frames show: its corpse's own, as its rot has left them (#1539).</summary>
    internal static CorpseRotRows ShownDeadSidehopperRows(DeadSidehopperEnemyState deadSidehopperEnemyState) => deadSidehopperEnemyState.Rows.Shown;

    /// <summary>Spawns a completed row's dust at its randomized X offset, and conditionally requests sound.</summary>
    private void FinishDeadSidehopperCorpseRow(RoomEnemyPlace place) =>
        _rowDust.Spawn(place, _room.Random.Read(), _projectiles, _frameContext.EnemyPassCount);

    /// <summary>Starts a new dead Sidehopper animation, resetting both animation clocks before its first step runs.</summary>
    internal static void SetDeadSidehopperAnimation(RoomEnemyPlace place, DeadSidehopperEnemyState state, DeadTourianSegment segment) =>
        state.Playback = StartDeadTourianAnimation(place, segment);

    /// <summary>Resets <paramref name="place"/>'s animation clocks and returns the playback of <paramref name="segment"/>'s animation from its start.</summary>
    internal static AnimationPlayback<DeadTourianSegment> StartDeadTourianAnimation(RoomEnemyPlace place, DeadTourianSegment segment)
    {
        place.AnimationClock.Restart();
        return new AnimationPlayback<DeadTourianSegment>(DeadTourianAnimations.StartingWith(segment), AnimationPlace.StartOfAnimation);
    }

    /// <summary>Routes a bomb that selects the corpse's shot reaction to its decay check, in place of common bomb damage.</summary>
    internal static void ReplaceDeadSidehopperBombDamage(EnemyBombContext c, DeadSidehopperEnemyState deadSidehopperEnemyState) =>
        ResolveDeadSidehopperShot(c.Enemy, deadSidehopperEnemyState);

    /// <summary>Runs the dead Sidehopper's main AI.</summary>
    internal void RunDeadSidehopperMainAi(RoomEnemyPlace place, DeadSidehopperEnemyState deadSidehopperEnemyState, IEnemyAiFrame frame)
    {
        RunDeadSidehopperMain(place, deadSidehopperEnemyState, frame.Samus, frame.Level, frame.Camera);
    }

    /// <summary>Advances the dead Sidehopper's typed animation.</summary>
    internal static void AdvanceDeadSidehopperTypedAnimation(RoomEnemyPlace place, DeadSidehopperEnemyState state)
    {
        if (!place.AnimationClock.Expire())
            return;
        AnimationPlayback<DeadTourianSegment> playback = state.Playback;
        EnemyAnimationPlayback.PlayApplyingCues(place, DeadTourianAnimations.Segments, ref playback,
            (DeadTourianCue cue) => ApplyDeadSidehopperCue(state, cue));
        state.Playback = playback;
    }

    /// <summary>Uses the private power-bomb reaction in place of common enemy damage handling.</summary>
    /// <param name="replacesCommon">The corpse's own method running its Power Bomb reaction.</param>
    internal static EnemyPowerBombReaction DeadSidehopperPowerBombReaction(EnemyPowerBombStage replacesCommon) => new()
    {
        ReplacesCommon = replacesCommon,
    };

    /// <summary>Marks projectile collision while withholding damage until the transformed corpse can decay.</summary>
    /// <remarks>The shot is marked as having hit, but the victim ignores it until it is fully drained; even then it
    /// starts rotting without taking damage or an impact.</remarks>
    /// <param name="marksShot">The corpse's own method marking the shot and running its reaction.</param>
    internal static EnemyShotReaction DeadSidehopperShotReaction(EnemyShotStage marksShot) => new()
    {
        BeforeSuperMissileQuake = marksShot,
    };

    /// <summary>Uses ordinary touch damage during transformation and starts decomposition afterward.</summary>
    internal void ReactToDeadSidehopperTouch(EnemyTouchContext context, DeadSidehopperEnemyState deadSidehopperEnemyState) =>
        // Before Baby Metroid finishes feeding, the touch reaction uses normal contact damage without a death animation. At
        // the end of the drain it instead starts the corpse decomposition and never enters common touch damage.
        ResolveDeadSidehopperTouch(context.Occupant, deadSidehopperEnemyState, context.Samus, context.PseudoScrew);
}
