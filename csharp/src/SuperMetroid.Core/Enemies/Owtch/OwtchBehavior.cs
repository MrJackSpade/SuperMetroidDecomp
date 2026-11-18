using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Enemies.Owtch;

/// <summary>The Owtch species owner: its per-room state, behaviour and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class OwtchBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.Owtch];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemyPlace place, EnemyCommonParticipation common) => new OwtchEntity(this, place, common);

    /// <summary>Supplies the shared room random sequence used for burial and emergence.</summary>
    private readonly EnemyRoomBinding _room;

    /// <summary>Binds Owtch AI to the room's shared random source.</summary>
    internal OwtchBehavior(EnemyRoomBinding room)
    {
        _room = room;
    }

    /// <summary>Carries out one cue of Owtch's animations: each sets the walking direction main AI applies.</summary>
    private static void ApplyOwtchCue(OwtchEnemyState state, OwtchCue cue) =>
        SetOwtchMoving(state, cue.Direction == HorizontalDirection.Left ? OwtchBehaviorState.MovingLeft : OwtchBehaviorState.MovingRight);

    /// <summary>Initializes the Owtch.</summary>
    internal static void InitializeOwtch(RoomEnemyPlace place)
    {
        OwtchSpawnSettings settings = place.SpawnSettings<OwtchSpawnSettings>();
        var state = new OwtchEnemyState
        {
            Behavior = settings.InitialBehavior,
            UndergroundTimer = settings.UndergroundWait,
            MinimumX = place.Position.X.Whole - settings.TravelDistance,
            MaximumX = place.Position.X.Whole + settings.TravelDistance,
        };
        place.SetSpeciesState(state);
        SetOwtchAnimation(place, state, InitialAnimation(settings.InitialBehavior));

        // An Owtch placed underground starts buried: it moves down by whole pixels only and records the same
        // sixteen-pixel depth sinking reaches.
        if (state.Behavior == OwtchBehaviorState.Underground)
        {
            state.BurialDepth = OwtchMovementDefinitions.MaximumBurialDepth;
            place.Position = place.Position.WithWholeY(place.Position.Y.Whole + OwtchMovementDefinitions.MaximumBurialDepth);
        }
    }

    /// <summary>Runs the Owtch's current behaviour.</summary>
    private void RunOwtchMain(RoomEnemyPlace place, OwtchEnemyState state)
    {
        switch (state.Behavior)
        {
            case OwtchBehaviorState.MovingLeft:
                MoveOwtch(place, state, direction: HorizontalDirection.Left);
                MaybeMakeOwtchSink(place, state);
                return;

            case OwtchBehaviorState.MovingRight:
                MoveOwtch(place, state, direction: HorizontalDirection.Right);
                MaybeMakeOwtchSink(place, state);
                return;

            case OwtchBehaviorState.Underground:
                if (--state.UndergroundTimer == 0)
                    state.Behavior = OwtchBehaviorState.Rising;
                return;

            case OwtchBehaviorState.Sinking:
                place.Position = place.Position.WithWholeY(place.Position.Y.Whole + OwtchMovementDefinitions.BurrowSpeed);
                // Sinking starts from zero, so the depth stays within one through sixteen.
                state.BurialDepth++;
                if (state.BurialDepth >= OwtchMovementDefinitions.MaximumBurialDepth)
                {
                    state.Behavior = OwtchBehaviorState.Underground;
                    state.UndergroundTimer = place.SpawnSettings<OwtchSpawnSettings>().UndergroundWait;
                }
                return;

            case OwtchBehaviorState.Rising:
                place.Position = place.Position.WithWholeY(place.Position.Y.Whole - OwtchMovementDefinitions.BurrowSpeed);
                if (state.RiseOnePixel())
                {
                    // Surfacing reads the current random number without advancing it.
                    int random = _room.Random.Read();
                    state.Behavior = OwtchMovementDefinitions.SurfacesMovingRight.IsTakenBy(random)
                        ? OwtchBehaviorState.MovingRight
                        : OwtchBehaviorState.MovingLeft;
                }
                return;

            case OwtchBehaviorState.TurningAtLeftBound:
                // Owtch does not move this update; it restarts the right-facing animation, whose first cue sets it
                // walking right.
                SetOwtchAnimation(place, state, OwtchSegment.MovingRight);
                return;

            default:
                throw new InvalidDataException(
                    $"Owtch has no behaviour for state {state.Behavior}.");
        }
    }

    /// <summary>
    /// The walking animation an Owtch starts in for its first behaviour: walking left and buried start facing left, and
    /// walking right and sinking start facing right. No placement starts an Owtch rising (#2474): it rises only out of a
    /// burial, and no room places one rising.
    /// </summary>
    /// <exception cref="InvalidDataException"><paramref name="initial"/> is the turn at the left bound or the rise, which no placement starts in.</exception>
    private static OwtchSegment InitialAnimation(OwtchBehaviorState initial) => initial switch
    {
        OwtchBehaviorState.MovingLeft or OwtchBehaviorState.Underground => OwtchSegment.MovingLeft,
        OwtchBehaviorState.MovingRight or OwtchBehaviorState.Sinking => OwtchSegment.MovingRight,
        OwtchBehaviorState.TurningAtLeftBound => throw new InvalidDataException("No Owtch placement starts turning at its left bound."),
        OwtchBehaviorState.Rising => throw new InvalidDataException("No Owtch placement starts rising: an Owtch rises only out of a burial."),
        _ => throw new ArgumentOutOfRangeException(nameof(initial), initial, "Undefined Owtch behaviour."),
    };

    /// <summary>
    /// Moves the Owtch along its patrol by its spawn settings' speed toward <paramref name="direction"/>; reaching an end of
    /// the patrol turns it.
    /// </summary>
    private static void MoveOwtch(
        RoomEnemyPlace place,
        OwtchEnemyState state,
        HorizontalDirection direction)
    {
        // A fraction past a whole pixel carries into the whole pixels.
        SignedSixteenSixteen speed = place.SpawnSettings<OwtchSpawnSettings>().Speed;
        place.Position = place.Position.AdvanceX(direction == HorizontalDirection.Right ? speed : -speed);

        if (direction != HorizontalDirection.Right)
        {
            if (place.Position.X.Whole >= state.MinimumX)
                return;

            // Unlike the right bound, the left bound does not turn Owtch at once: it spends the next update turning.
            state.Behavior = OwtchBehaviorState.TurningAtLeftBound;
            return;
        }

        if (place.Position.X.Whole >= state.MaximumX)
            state.Behavior = OwtchBehaviorState.MovingLeft;
    }

    /// <summary>Runs Owtch's six-in-256 per-frame burial gate.</summary>
    private void MaybeMakeOwtchSink(RoomEnemyPlace place, OwtchEnemyState state)
    {
        if (OwtchMovementDefinitions.Sinks.IsTakenBy(_room.Random.Generate(), advancedBy: place.FrameCounter))
            state.Behavior = OwtchBehaviorState.Sinking;
    }

    /// <summary>Animation cues for left and right: set the moving behaviour.</summary>
    private static void SetOwtchMoving(OwtchEnemyState state, OwtchBehaviorState moving) =>
        state.Behavior = moving;

    /// <summary>
    /// Owtch's shell takes common damage only while it walks left or turns at the left bound; every other state is
    /// immune.
    /// </summary>
    private static bool OwtchAcceptsOrdinaryShot(OwtchEnemyState state) =>
        state.Behavior is OwtchBehaviorState.MovingLeft or OwtchBehaviorState.TurningAtLeftBound;

    /// <summary>Restarts the selected facing animation at its first step.</summary>
    private static void SetOwtchAnimation(RoomEnemyPlace place, OwtchEnemyState state, OwtchSegment segment)
    {
        state.Playback = EnemyAnimationPlayback.Start(place, OwtchAnimations.StartingWith(segment));
    }

    /// <summary>Allows common bomb damage only while the Owtch walks left, the one way its shell leaves it open.</summary>
    internal static EnemyBombReaction OwtchBombReaction { get; } = new()
    {
        Rejects = c => !OwtchAcceptsOrdinaryShot(c.Enemy.RequireSpeciesState<OwtchEnemyState>()),
    };

    /// <summary>Runs Owtch's main AI.</summary>
    internal void RunOwtchMainAi(RoomEnemyPlace place, IEnemyAiFrame _)
    {
        RunOwtchMain(place, place.RequireSpeciesState<OwtchEnemyState>());
    }

    /// <summary>Advances Owtch's typed animation.</summary>
    internal static void AdvanceOwtchTypedAnimation(RoomEnemyPlace place, EnemyAnimationCueContext _) =>
        EnemyAnimationPlayback.Advance(place, OwtchAnimations.Segments,
            static (OwtchEnemyState state) => state.Playback, static (state, playback) => state.Playback = playback,
            (state, cue) =>
            {
                ApplyOwtchCue(state, cue);
                return null;
            });

    /// <summary>Turns ordinary shots away from Owtch's shell unless it is walking left.</summary>
    /// <remarks>A shot the shell turns away does no damage, and a non-Plasma shot still ends where it struck, so the
    /// shell is immune while Owtch moves right, sinks, is buried or rises.</remarks>
    internal static EnemyShotReaction OwtchShotReaction { get; } = new()
    {
        AfterSuperMissileQuake = (in EnemyShotContext c) =>
        {
            if (OwtchAcceptsOrdinaryShot(c.Enemy.RequireSpeciesState<OwtchEnemyState>()))
                return false;
            if (c.MarksCollision)
                c.Projectile.IsSpent = true;
            return true;
        },
    };
}
