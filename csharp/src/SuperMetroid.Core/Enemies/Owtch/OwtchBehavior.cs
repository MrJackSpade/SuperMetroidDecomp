using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Enemies.Owtch;

/// <summary>The Owtch species owner: its per-room state, behaviour and the interactions it registers.</summary>
[RoomLifetime]
internal sealed partial class OwtchBehavior : IEnemySpecies, IEnemySpawnFactory<OwtchSpawnSettings>
{

    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.Owtch];

    /// <inheritdoc/>
    public EnemyOccupant Create(RoomEnemyOccupant occupant, OwtchSpawnSettings settings, RoomEnemyPlace place, EnemyCommonParticipation common) => EnemyOccupant.Running(new OwtchEntity(this, occupant, place, common, settings));

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
    /// <returns>The state the enemy holds from its setup.</returns>
    internal static OwtchEnemyState InitializeOwtch(OwtchSpawnSettings settings, RoomEnemyPlace place)
    {
        var state = new OwtchEnemyState
        {
            Behavior = settings.InitialBehavior,
            UndergroundTimer = UpdateCountdown.For(settings.UndergroundWait),
            MinimumX = place.Position.X.Whole - settings.TravelDistance,
            MaximumX = place.Position.X.Whole + settings.TravelDistance,
        };
        SetOwtchAnimation(place, state, InitialAnimation(settings.InitialBehavior));

        // An Owtch placed underground starts buried: it moves down by whole pixels only, to the same burial depth
        // sinking reaches.
        if (state.Behavior == OwtchBehaviorState.Underground)
            place.Position = place.Position.WithWholeY(place.Position.Y.Whole + OwtchMovementDefinitions.MaximumBurialDepth);
        return state;
    }

    /// <summary>What <see cref="RunOwtchMain"/> does for each <see cref="OwtchBehaviorState"/> member, each member's own arm.</summary>
    [field: NonSerialized]
    private IdentityTable<OwtchBehaviorState, Action<OwtchSpawnSettings, RoomEnemyPlace, OwtchEnemyState>> RunOwtchMainForOwtchBehaviorState => field ??= new(new Dictionary<OwtchBehaviorState, Action<OwtchSpawnSettings, RoomEnemyPlace, OwtchEnemyState>>
    {
        [OwtchBehaviorState.MovingLeft] = (settings, place, state) =>
        {
            MoveOwtch(settings, place, state, direction: HorizontalDirection.Left);
            MaybeMakeOwtchSink(place, state);
        },
        [OwtchBehaviorState.MovingRight] = (settings, place, state) =>
        {
            MoveOwtch(settings, place, state, direction: HorizontalDirection.Right);
            MaybeMakeOwtchSink(place, state);
        },
        [OwtchBehaviorState.Underground] = (_, place, state) =>
        {
            if (state.UndergroundTimer.StepEnds())
            {
                state.Behavior = OwtchBehaviorState.Rising;
                state.Rise = UpdateCountdown.Lasting(OwtchMovementDefinitions.MaximumBurialDepth);
            }
        },
        [OwtchBehaviorState.Sinking] = (settings, place, state) =>
        {
            place.Position = place.Position.WithWholeY(place.Position.Y.Whole + OwtchMovementDefinitions.BurrowSpeed);
            if (state.Sink.Count())
            {
                state.Behavior = OwtchBehaviorState.Underground;
                state.UndergroundTimer = UpdateCountdown.For(settings.UndergroundWait);
            }
        },
        [OwtchBehaviorState.Rising] = (_, place, state) =>
        {
            place.Position = place.Position.WithWholeY(place.Position.Y.Whole - OwtchMovementDefinitions.BurrowSpeed);
            if (state.Rise.Count())
            {
                // Surfacing reads the current random number without advancing it.
                int random = _room.Random.Read();
                state.Behavior = OwtchMovementDefinitions.SurfacesMovingRight.IsTakenBy(random)
                    ? OwtchBehaviorState.MovingRight
                    : OwtchBehaviorState.MovingLeft;
            }
        },
        [OwtchBehaviorState.TurningAtLeftBound] = (_, place, state) =>
        {
            // Owtch does not move this update; it restarts the right-facing animation, whose first cue sets it
            // walking right.
            SetOwtchAnimation(place, state, OwtchSegment.MovingRight);
        },
    });

    /// <summary>Runs the Owtch's current behaviour.</summary>
    private void RunOwtchMain(OwtchSpawnSettings owtchSpawnSettings, RoomEnemyPlace place, OwtchEnemyState state)
    {
        RunOwtchMainForOwtchBehaviorState[state.Behavior](owtchSpawnSettings, place, state);
    }

    /// <summary>
    /// The walking animation an Owtch starts in for its first behaviour: walking left and buried start facing left, and
    /// walking right and sinking start facing right. No placement starts an Owtch rising (#2474): it rises only out of a
    /// burial, and no room places one rising.
    /// </summary>
    /// <exception cref="InvalidDataException"><paramref name="initial"/> is the turn at the left bound or the rise, which no placement starts in.</exception>
    private static OwtchSegment InitialAnimation(OwtchBehaviorState initial)
    {
        if (initial is OwtchBehaviorState.MovingLeft or OwtchBehaviorState.Underground)
            return OwtchSegment.MovingLeft;
        if (initial is OwtchBehaviorState.MovingRight or OwtchBehaviorState.Sinking)
            return OwtchSegment.MovingRight;
        if (initial == OwtchBehaviorState.TurningAtLeftBound)
            throw new InvalidDataException("No Owtch placement starts turning at its left bound.");
        // The remaining member, OwtchBehaviorState.Rising.
        throw new InvalidDataException("No Owtch placement starts rising: an Owtch rises only out of a burial.");
    }

    /// <summary>
    /// Moves the Owtch along its patrol by its spawn settings' speed toward <paramref name="direction"/>; reaching an end of
    /// the patrol turns it.
    /// </summary>
    private static void MoveOwtch(
        OwtchSpawnSettings owtchSpawnSettings, RoomEnemyPlace place,
        OwtchEnemyState state,
        HorizontalDirection direction)
    {
        // A fraction past a whole pixel carries into the whole pixels.
        SignedSixteenSixteen speed = owtchSpawnSettings.Speed;
        place.Position = place.Position.AdvanceX(speed.Toward(direction));

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

    /// <summary>Runs Owtch's burial chance (<see cref="OwtchMovementDefinitions.Sinks"/>) each update.</summary>
    private void MaybeMakeOwtchSink(RoomEnemyPlace place, OwtchEnemyState state)
    {
        if (OwtchMovementDefinitions.Sinks.IsTakenBy(_room.Random.Generate(), advancedBy: place.FrameCounter))
        {
            state.Behavior = OwtchBehaviorState.Sinking;
            state.Sink = UpdateCountdown.Lasting(OwtchMovementDefinitions.MaximumBurialDepth);
        }
    }

    /// <summary>Animation cues for left and right: set the moving behaviour.</summary>
    private static void SetOwtchMoving(OwtchEnemyState state, OwtchBehaviorState moving) =>
        state.Behavior = moving;

    /// <summary>
    /// Owtch's shell takes common damage only while it walks left or turns at the left bound; every other state is
    /// immune.
    /// </summary>
    internal static bool OwtchAcceptsOrdinaryShot(OwtchEnemyState state) =>
        state.Behavior is OwtchBehaviorState.MovingLeft or OwtchBehaviorState.TurningAtLeftBound;

    /// <summary>Restarts the selected facing animation at its first step.</summary>
    private static void SetOwtchAnimation(RoomEnemyPlace place, OwtchEnemyState state, OwtchSegment segment)
    {
        state.Playback = EnemyAnimationPlayback.Start(place, OwtchAnimations.StartingWith(segment));
    }

    /// <summary>Allows common bomb damage only while the Owtch walks left, the one way its shell leaves it open.</summary>
    /// <param name="rejects">The Owtch's own method saying whether its shell turns the bomb away.</param>
    internal static EnemyBombReaction OwtchBombReaction(Func<EnemyBombContext, bool> rejects) => new()
    {
        Rejects = rejects,
    };

    /// <summary>Runs Owtch's main AI.</summary>
    internal void RunOwtchMainAi(OwtchSpawnSettings owtchSpawnSettings, RoomEnemyPlace place, OwtchEnemyState owtchEnemyState)
    {
        RunOwtchMain(owtchSpawnSettings, place, owtchEnemyState);
    }

    /// <summary>Advances Owtch's typed animation.</summary>
    internal static void AdvanceOwtchTypedAnimation(RoomEnemyPlace place, OwtchEnemyState owtchEnemyState) =>
        EnemyAnimationPlayback.Advance(place, owtchEnemyState, OwtchAnimations.Segments,
            static (OwtchEnemyState state) => state.Playback, static (state, playback) => state.Playback = playback,
            (state, cue) =>
            {
                ApplyOwtchCue(state, cue);
                return null;
            });

    /// <summary>Turns ordinary shots away from Owtch's shell unless it is walking left.</summary>
    /// <remarks>A shot the shell turns away does no damage, and a non-Plasma shot still ends where it struck, so the
    /// shell is immune while Owtch moves right, sinks, is buried or rises.</remarks>
    /// <param name="turnsShotAway">The Owtch's own method turning the shot away from its shell.</param>
    internal static EnemyShotReaction OwtchShotReaction(EnemyShotStage turnsShotAway) => new()
    {
        AfterSuperMissileQuake = turnsShotAway,
    };
}
