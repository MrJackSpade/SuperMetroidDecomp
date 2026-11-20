using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>The dead sidehopper species owner: its per-room state, behaviour and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class DeadSidehopperBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.CorpseSidehopper];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemyPlace place, EnemyCommonParticipation common, EnemySetup setup) => new DeadSidehopperEntity(this, place, common);

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

    /// <summary>Carries out the dead Sidehopper's animation cue.</summary>
    private static DeadTourianSegment? ApplyDeadSidehopperCue(DeadSidehopperEnemyState state, DeadTourianCue cue)
    {
        switch (cue.Kind)
        {
            case DeadTourianCueKind.EndHop:
                // The end-hop cue ends the landing animation and hands ownership back to main AI.
                SelectDeadSidehopperPostAnimationState(state);
                return null;
            default:
                throw new InvalidOperationException($"Undefined {nameof(DeadTourianCueKind)} {cue.Kind}.");
        }
    }

    /// <summary>Initializes the dead Sidehopper.</summary>
    internal DeadSidehopperEnemyState InitializeDeadSidehopper(RoomEnemyPlace place)
    {
        DeadSidehopperLayout layout = place.SpawnSettings<DeadSidehopperSpawnSettings>().Layout;
        return layout switch
        {
            DeadSidehopperLayout.BabyMetroidVictim => InitializeBabyMetroidVictimSidehopper(place),
            DeadSidehopperLayout.AlreadyDead => InitializeAlreadyDeadSidehopper(place),
            _ => throw new InvalidOperationException($"Undefined {nameof(DeadSidehopperLayout)} {layout}."),
        };
    }

    /// <summary>Runs the Baby Metroid victim layout.</summary>
    private DeadSidehopperEnemyState InitializeBabyMetroidVictimSidehopper(RoomEnemyPlace place)
    {
        // The victim stops being solid to Samus and is processed off screen; the other contact and processing
        // properties the room's population gave it stay as they were, which is observable.
        place.Contact = place.Contact.Without(EnemyContact.SolidToSamus);
        place.Processing = place.Processing.With(EnemyProcessing.OffScreen);
        // A Baby Metroid that has already fed on this Sidehopper leaves no victim.
        if (_babyMetroidFeeding.HasAlreadyFedOn(place))
            place.Presence = place.Presence.With(EnemyPresence.Deleted);

        place.Position = place.Position.WithWhole(DeadSidehopperVictimDefinitions.StartPosition);
        place.Radius = place.Radius.WithY(DeadSidehopperVictimDefinitions.LivingHalfHeight);

        DeadSidehopperEnemyState state = InitializeDeadSidehopperCorpseState(DeadSidehopperAction.AliveWaitForCamera,
            _babyMetroidFeeding.DrainOf(place));
        SetDeadSidehopperAnimation(place, state, DeadTourianSegment.SidehopperIdle);
        // The victim shows the colours the Baby Metroid's entry gives it, the target the room's colours fade to (#1512).
        place.Shading = state.Shading;
        state.Velocity = DeadSidehopperLaunchDefinitions.FirstHop;
        return state;
    }

    /// <summary>Lays out a Sidehopper corpse placed already dead, drained from the start and waiting for Samus to touch it.</summary>
    private static DeadSidehopperEnemyState InitializeAlreadyDeadSidehopper(RoomEnemyPlace place)
    {
        DeadSidehopperEnemyState state = InitializeDeadSidehopperCorpseState(DeadSidehopperAction.WaitForSamusCollision, SidehopperDrain.PlacedDead());
        SetDeadSidehopperAnimation(place, state, DeadTourianSegment.SidehopperInitiallyDead);
        place.Shading = state.Shading;
        return state;
    }

    /// <summary>Allocates place state with the corpse's own rot schedule, its own rows, its first action and its drain.</summary>
    private static DeadSidehopperEnemyState InitializeDeadSidehopperCorpseState(DeadSidehopperAction action, SidehopperDrain drain)
    {
        return new DeadSidehopperEnemyState(new CorpseRotSchedule(DeadSidehopperCorpseDefinitions.RowCount), action, drain);
    }

    /// <summary>Runs the dead Sidehopper's main AI.</summary>
    private void RunDeadSidehopperMain(
        RoomEnemyPlace place, DeadSidehopperEnemyState state,
        SamusState samus,
        RoomLevelData level,
        Position16<RoomPixels> camera) =>
        RunDeadSidehopperState(place, state, samus, level, camera);

    /// <summary>Advances the authored hop, post-landing delay, drain, or collision-triggered decay state.</summary>
    internal void RunDeadSidehopperState(
        RoomEnemyPlace place,
        DeadSidehopperEnemyState state,
        SamusState samus,
        RoomLevelData level,
        Position16<RoomPixels> camera)
    {
        switch (state.Action)
        {
            case DeadSidehopperAction.AliveWaitForCamera:
                // The camera reaching the victim starts its hop on the same update.
                if (camera.X < DeadSidehopperVictimDefinitions.ActivationCameraX)
                {
                    state.Action = DeadSidehopperAction.ActivatedMovement;
                    HopDeadSidehopper(place, state, level);
                }
                return;

            case DeadSidehopperAction.ActivatedMovement:
                HopDeadSidehopper(place, state, level);
                return;

            case DeadSidehopperAction.Landing:
                return;

            case DeadSidehopperAction.BeginPostLandingDelay:
                state.Action = DeadSidehopperAction.PostLandingDelay;
                state.PostLandingPause = UpdateCountdown.Lasting(DeadSidehopperVictimDefinitions.PostLandingPause);
                return;

            case DeadSidehopperAction.PostLandingDelay:
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
                return;

            case DeadSidehopperAction.Draining:
                StepDeadSidehopperDrain(place, state);
                return;

            case DeadSidehopperAction.WaitForSamusCollision:
                if (samus.Kinematics.DidCollideWithSolidEnemy(place) == true)
                    state.Action = DeadSidehopperAction.PreRotDelay;
                return;

            case DeadSidehopperAction.PreRotDelay:
                if (state.PreRotDelay.Step())
                {
                    state.Action = DeadSidehopperAction.Rotting;
                    place.Contact = place.Contact.With(EnemyContact.IgnoresSamus);
                }
                return;

            case DeadSidehopperAction.Rotting:
                RunDeadSidehopperRotting(place, state);
                return;

            default:
                throw new InvalidDataException(
                    $"Dead sidehopper has no behaviour for action {state.Action}.");
        }
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
        state.Drain.Step();
        place.Shading = state.Shading;
        if (!state.IsDrained)
            return;

        SetDeadSidehopperAnimation(place, state, DeadTourianSegment.SidehopperCorpse);
        state.Action = DeadSidehopperAction.WaitForSamusCollision;
        place.Contact = place.Contact.With(EnemyContact.SolidToSamus);
        place.Radius = place.Radius.WithY(DeadSidehopperVictimDefinitions.DrainedHalfHeight);
    }

    /// <summary>
    /// The end-hop cue selects the post-landing state from whether Baby Metroid has begun draining the victim. It is
    /// deliberately cue-driven: replacing it with a main-AI timer would make animation length and movement
    /// cadence diverge.
    /// </summary>
    private static void SelectDeadSidehopperPostAnimationState(DeadSidehopperEnemyState state)
    {
        state.Action = state.HasBegunDraining
            ? DeadSidehopperAction.Draining
            : DeadSidehopperAction.BeginPostLandingDelay;
    }

    /// <summary>Resolves a shot hitting the dead Sidehopper.</summary>
    private static void ResolveDeadSidehopperShot(RoomEnemyPlace place, DeadSidehopperEnemyState state)
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
        RoomEnemyPlace place,
        DeadSidehopperEnemyState state,
        SamusState samus,
        PseudoScrewCharge pseudoScrew)
    {
        if (!state.IsDrained)
        {
            _collision.ResolveOrdinaryTouch(place, samus, pseudoScrew);
            return;
        }
        TriggerDeadSidehopperRotting(place, state);
    }

    /// <summary>Resolves a power bomb reaching the dead Sidehopper.</summary>
    private void ResolveDeadSidehopperPowerBomb(EnemyPowerBombContext context, DeadSidehopperEnemyState state)
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
    internal static CorpseRotRows ShownDeadSidehopperRows(DeadSidehopperEnemyState state) => state.Rows.Shown;

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
    internal static void ReplaceDeadSidehopperBombDamage(EnemyBombContext c, DeadSidehopperEnemyState state) =>
        ResolveDeadSidehopperShot(c.Enemy, state);

    /// <summary>Runs the dead Sidehopper's main AI.</summary>
    internal void RunDeadSidehopperMainAi(RoomEnemyPlace place, DeadSidehopperEnemyState state, IEnemyAiFrame frame)
    {
        RunDeadSidehopperMain(place, state, frame.Samus, frame.Level, frame.Camera);
    }

    /// <summary>Advances the dead Sidehopper's typed animation.</summary>
    internal static void AdvanceDeadSidehopperTypedAnimation(RoomEnemyPlace place, DeadSidehopperEnemyState state, EnemyAnimationCueContext _)
    {
        if (!place.AnimationClock.Expire())
            return;
        AnimationPlayback<DeadTourianSegment> playback = state.Playback;
        EnemyAnimationPlayback.Play(place, DeadTourianAnimations.Segments, ref playback,
            cue => ApplyDeadSidehopperCue(state, cue) is { } target
                ? new AnimationContinuation<DeadTourianSegment>(DeadTourianAnimations.StartingWith(target), AnimationPlace.StartOfAnimation)
                : null);
        state.Playback = playback;
    }

    /// <summary>Uses the private power-bomb reaction in place of common enemy damage handling.</summary>
    /// <param name="state">The corpse's state, which the reaction acts on.</param>
    internal EnemyPowerBombReaction DeadSidehopperPowerBombReactionOf(DeadSidehopperEnemyState state) => new()
    {
        ReplacesCommon = c => ResolveDeadSidehopperPowerBomb(c, state),
    };

    /// <summary>Marks projectile collision while withholding damage until the transformed corpse can decay.</summary>
    /// <remarks>The common projectile prelude always marks the shot, but the private reaction ignores it until the
    /// victim is fully drained. Even then it starts rotting without vulnerability damage or impact.</remarks>
    /// <param name="state">The enemy's state, which the reaction acts on, after a hit has killed it too.</param>
    internal static EnemyShotReaction DeadSidehopperShotReactionOf(DeadSidehopperEnemyState state) => new()
    {
        BeforeSuperMissileQuake = (in EnemyShotContext c) =>
        {
            c.Projectiles.EnemyImpacts.TouchEnemyWithoutImpact(c.Projectile, c.MarksCollision);
            ResolveDeadSidehopperShot(c.Enemy, state);
            return true;
        },
    };

    /// <summary>Uses ordinary touch damage during transformation and starts decomposition afterward.</summary>
    /// <param name="context">The touch.</param>
    /// <param name="state">The enemy's own state, which the touch acts on.</param>
    internal void ReactToDeadSidehopperTouch(EnemyTouchContext context, DeadSidehopperEnemyState state) =>
        // Before Baby Metroid finishes feeding, the touch reaction uses normal contact damage without a death animation. At
        // the end of the drain it instead starts the corpse decomposition and never enters common touch damage.
        ResolveDeadSidehopperTouch(context.Place, state, context.Samus, context.PseudoScrew);
}
