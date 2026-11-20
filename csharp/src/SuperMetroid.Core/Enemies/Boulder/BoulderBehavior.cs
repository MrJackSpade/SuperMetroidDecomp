using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Enemies.Boulder;

/// <summary>The Boulder species owner: its per-room state, behaviour and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class BoulderBehavior : IEnemySpecies, IEnemySpawnFactory<BoulderSpawnSettings>
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.Boulder];

    /// <inheritdoc/>
    public EnemyOccupant Create(RoomEnemyOccupant occupant, BoulderSpawnSettings settings, RoomEnemyPlace place, EnemyCommonParticipation common) => EnemyOccupant.Running(new BoulderEntity(this, occupant, place, common, settings));

    /// <summary>Moves the actor through room collision during fall and roll phases.</summary>
    private readonly EnemyTerrain _terrain;
    /// <summary>Creates the dust explosion when a rolling or brittle boulder breaks.</summary>
    private readonly EnemyProjectileEngine _projectiles;

    /// <summary>Receives the species' sound requests, each queued as the species makes it.</summary>
    private readonly EnemyAudioQueue _audio;

    /// <summary>Connects terrain collision and the shared projectile engine for Boulder AI.</summary>
    internal BoulderBehavior(EnemyTerrain terrain, EnemyProjectileEngine projectiles, EnemyAudioQueue audio)
    {
        _audio = audio;
        _terrain = terrain;
        _projectiles = projectiles;
    }

    /// <summary>Reads the spawn settings as direction, trigger windows and how it starts.</summary>
    /// <returns>The state the enemy holds from its setup.</returns>
    internal static BoulderEnemyState InitializeBoulder(RoomEnemyOccupant occupant, BoulderSpawnSettings settings, RoomEnemyPlace place)
    {
        var state = new BoulderEnemyState(BoulderAction.WaitingForSamus);

        state.RestingY = place.Position.Whole.Y;
        state.PreviousY = place.Position.Y;
        int initialYOffset;
        switch (settings.Start)
        {
            case BoulderStart.FallsFrom fall:
                initialYOffset = fall.Drop;
                break;
            case BoulderStart.RollsAtOnce:
                // A Boulder that rolls at once still hangs a pixel above its resting place, and its trigger starts it
                // rolling instead of falling.
                initialYOffset = BoulderMotionDefinitions.RollingStartLift;
                break;
            default:
                throw new InvalidOperationException($"Undefined {nameof(BoulderStart)} {settings.Start}.");
        }
        place.Position = place.Position.WithWholeY(state.RestingY - initialYOffset);

        switch (settings.Roll)
        {
            case BoulderRoll.Right:
                state.Playback = new(BoulderAnimations.StartingWith(BoulderSegment.Right), AnimationPlace.StartOfAnimation);
                break;
            case BoulderRoll.Left:
            case BoulderRoll.LeftBreakingOnImpact:
                state.Playback = new(BoulderAnimations.StartingWith(BoulderSegment.Left), AnimationPlace.StartOfAnimation);
                break;
            default:
                throw new InvalidOperationException($"Undefined {nameof(BoulderRoll)} {settings.Roll}.");
        }
        state.VerticalCompensation = BoulderMotionDefinitions.RollingGroundReach;
        return state;
    }

    /// <summary>Runs the wait, fall, rebound, roll or inert phase with required Samus and terrain state.</summary>
    private void RunBoulderMain(RoomEnemyOccupant occupant, BoulderSpawnSettings boulderSpawnSettings, RoomEnemyPlace place, BoulderEnemyState state, SamusState samus, RoomLevelData level)
    {
        switch (state.Action)
        {
            case BoulderAction.WaitingForSamus:
                RunBoulderWaiting(occupant, boulderSpawnSettings, place, state, samus);
                break;
            case BoulderAction.InitialFall:
                RunBoulderInitialFall(place, state);
                break;
            case BoulderAction.Rebound:
                RunBoulderRebound(occupant, boulderSpawnSettings, place, state);
                break;
            case BoulderAction.Falling:
                RunBoulderFalling(occupant, boulderSpawnSettings, place, state, level);
                break;
            case BoulderAction.Rolling:
                RunBoulderRolling(occupant, boulderSpawnSettings, place, state, level);
                break;
            case BoulderAction.Inert:
                break;
            default:
                throw new InvalidDataException(
                    $"Boulder has no behaviour for action {state.Action}.");
        }
    }

    /// <summary>
    /// Starts the bounce chain or direct roll only when Samus stands nearer below the boulder than its spawn settings'
    /// trigger height and within their reach toward its roll (#2432): up to the reach to the left of one that rolls left,
    /// nearer than the reach to the right of one that rolls right. One that rolls at once, which hangs a pixel above its
    /// resting place, bypasses the falling and bounce sequence.
    /// </summary>
    private static void RunBoulderWaiting(RoomEnemyOccupant occupant, BoulderSpawnSettings settings, RoomEnemyPlace place, BoulderEnemyState state, SamusState samus)
    {
        int deltaY = samus.Position.Y.Whole - place.Position.Y.Whole;
        if (deltaY < 0 || deltaY >= settings.TriggerHeight)
            return;

        int deltaX = samus.Position.X.Whole - place.Position.X.Whole;
        bool enteredHorizontalWindow = settings.Roll != BoulderRoll.Right
            ? deltaX < 0 && -deltaX <= settings.TriggerReach
            : deltaX >= 0 && deltaX < settings.TriggerReach;
        if (!enteredHorizontalWindow)
            return;

        state.Action = settings.Start is BoulderStart.RollsAtOnce
            ? BoulderAction.Rolling
            : BoulderAction.InitialFall;
    }

    /// <summary>Accelerates down toward the recorded spawn height, then clamps and starts the first rebound.</summary>
    private static void RunBoulderInitialFall(RoomEnemyPlace place, BoulderEnemyState state)
    {
        AddBoulderVerticalVelocity(place, state.BounceCurvePlace, direction: VerticalDirection.Down);
        if (place.Position.Y.Whole < state.RestingY)
        {
            state.BounceCurvePlace = AddAndCap(
                state.BounceCurvePlace,
                BoulderMotionDefinitions.BounceSpeedStep,
                BoulderMotionDefinitions.MaximumBouncingSpeed);
            return;
        }

        place.Position = place.Position.WithWholeY(state.RestingY);
        state.Action = BoulderAction.Rebound;
        state.BounceCurvePlace = BoulderMotionDefinitions.FirstReboundBounceSpeed;
    }

    /// <summary>Applies upward quadratic motion while reducing bounce speed, adding sideways travel, then returns to falling.</summary>
    private static void RunBoulderRebound(RoomEnemyOccupant occupant, BoulderSpawnSettings boulderSpawnSettings, RoomEnemyPlace place, BoulderEnemyState state)
    {
        AddBoulderVerticalVelocity(place, state.BounceCurvePlace, direction: VerticalDirection.Up);

        // Slowing the rebound by one mark past the curve's start ends it: the boulder falls from the curve's start.
        QuadraticSpeedCurvePlace slowed = state.BounceCurvePlace - BoulderMotionDefinitions.BounceSpeedStep;
        if (slowed.IsNegative)
        {
            state.BounceCurvePlace = QuadraticSpeedCurvePlace.Start;
            state.Action = BoulderAction.Falling;
            return;
        }
        state.BounceCurvePlace = slowed;

        AddBoulderHorizontalVelocity(occupant, boulderSpawnSettings, place, state);
        state.TravelCurvePlace = AddAndCap(
            state.TravelCurvePlace,
            BoulderMotionDefinitions.BouncingTravelSpeedStep,
            BoulderMotionDefinitions.MaximumBouncingSpeed);
    }

    /// <summary>Accelerates through terrain contact; ordinary impacts consume bounces while a boulder that breaks on impact breaks at the first.</summary>
    private void RunBoulderFalling(RoomEnemyOccupant occupant, BoulderSpawnSettings boulderSpawnSettings, RoomEnemyPlace place, BoulderEnemyState state, RoomLevelData level)
    {
        SignedSixteenSixteen verticalDisplacement = state.BounceCurvePlace.Speed.Positive;
        if (_terrain.MoveEnemyVertically(level, place, verticalDisplacement))
        {
            _audio.QueueEnemySound(EnemySoundEffectDefinitions.BoulderImpactSound, priority: SoundRequestPriority.Normal);
            if (boulderSpawnSettings.Roll == BoulderRoll.LeftBreakingOnImpact)
            {
                place.Presence = place.Presence.With(EnemyPresence.Deleted);
                _projectiles.SpawnDustOrExplosion(
                    place.Position.Whole,
                    BoulderMotionDefinitions.BreakDust);
                _audio.QueueEnemySound(EnemySoundEffectDefinitions.BoulderBreakSound, priority: SoundRequestPriority.Normal);
                return;
            }

            state.Action = BoulderAction.Rebound;

            // The last impact still restarts the bounce clock, at the curve's start, even though it enters rolling.
            state.BounceCurvePlace = BoulderBounces.RestartPlace(state.ImpactsSoFar);
            if (++state.ImpactsSoFar == BoulderBounces.ImpactCount)
            {
                state.PreviousY = place.Position.Y;
                state.Action = BoulderAction.Rolling;
            }
            return;
        }

        // The fall's place on the curve is not capped: its next update reads the following mark, and the curve
        // rejects a place past its last mark.
        state.BounceCurvePlace += BoulderMotionDefinitions.BounceSpeedStep;
        AddBoulderHorizontalVelocity(occupant, boulderSpawnSettings, place, state);
        state.TravelCurvePlace = AddAndCap(
            state.TravelCurvePlace,
            BoulderMotionDefinitions.BouncingTravelSpeedStep,
            BoulderMotionDefinitions.MaximumBouncingSpeed);
    }

    /// <summary>Tracks the floor with vertical compensation and advances horizontally until a wall hides/deletes the actor and spawns dust.</summary>
    private void RunBoulderRolling(RoomEnemyOccupant occupant, BoulderSpawnSettings boulderSpawnSettings, RoomEnemyPlace place, BoulderEnemyState state, RoomLevelData level)
    {
        SignedSixteenSixteen verticalDisplacement =
            state.TravelCurvePlace.Speed.Positive +
            SignedSixteenSixteen.FromWholePixels(state.VerticalCompensation);
        _terrain.MoveEnemyVertically(level, place, verticalDisplacement);
        place.Position = place.Position.WithWhole(place.Position.Whole - new Offset16<RoomPixels>(0, state.VerticalCompensation));

        SignedSixteenSixteen horizontalDisplacement = ReadBoulderHorizontalDisplacement(occupant, boulderSpawnSettings, state);
        if (_terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(level, place, horizontalDisplacement))
        {
            // The boulder breaks: hidden and gone, it does nothing more.
            place.Presence = place.Presence.With(EnemyPresence.Hidden | EnemyPresence.Deleted);
            state.Action = BoulderAction.Inert;
            _audio.QueueEnemySound(EnemySoundEffectDefinitions.BoulderImpactSound, priority: SoundRequestPriority.Normal);
            _projectiles.SpawnDustOrExplosion(
                place.Position.Whole,
                BoulderMotionDefinitions.BreakDust);
            _audio.QueueEnemySound(EnemySoundEffectDefinitions.BoulderBreakSound, priority: SoundRequestPriority.Normal);
        }
        else
        {
            state.TravelCurvePlace = AddAndCap(
                state.TravelCurvePlace,
                BoulderMotionDefinitions.RollingTravelSpeedStep,
                BoulderMotionDefinitions.MaximumRollingTravelSpeed);
            if (place.Position.Y == state.PreviousY)
                state.VerticalCompensation = 0;
        }

        state.PreviousY = place.Position.Y;
    }

    /// <summary>Moves the boulder horizontally by its travel speed on the quadratic speed curve, toward its roll.</summary>
    private static void AddBoulderHorizontalVelocity(RoomEnemyOccupant occupant, BoulderSpawnSettings boulderSpawnSettings, RoomEnemyPlace place, BoulderEnemyState state) =>
        place.Position = place.Position.WithX(place.Position.X.MovedBy(ReadBoulderHorizontalDisplacement(occupant, boulderSpawnSettings, state)));

    /// <summary>Reads the quadratic speed sample and signs it toward the roll its spawn settings name.</summary>
    private static SignedSixteenSixteen ReadBoulderHorizontalDisplacement(RoomEnemyOccupant occupant, BoulderSpawnSettings boulderSpawnSettings, BoulderEnemyState state) =>
        state.TravelCurvePlace.Speed.Toward(boulderSpawnSettings.Roll != BoulderRoll.Right ? HorizontalDirection.Left : HorizontalDirection.Right);

    /// <summary>Moves the boulder vertically by its place's speed on the quadratic speed curve, toward <paramref name="direction"/>.</summary>
    private static void AddBoulderVerticalVelocity(RoomEnemyPlace place, QuadraticSpeedCurvePlace accumulator, VerticalDirection direction)
    {
        SignedSixteenSixteen displacement = accumulator.Speed.Toward(direction);
        place.Position = place.Position.WithY(place.Position.Y.MovedBy(displacement));
    }

    /// <summary>
    /// Advances a place on the speed curve by <paramref name="increment"/>, holding it at <paramref name="maximum"/>.
    /// </summary>
    private static QuadraticSpeedCurvePlace AddAndCap(
        QuadraticSpeedCurvePlace value, QuadraticSpeedCurvePlace increment, QuadraticSpeedCurvePlace maximum) =>
        QuadraticSpeedCurvePlace.Min(value + increment, maximum);

    /// <summary>Advances Boulder's typed animation.</summary>
    internal static void AdvanceBoulderTypedAnimation(RoomEnemyPlace place, BoulderEnemyState boulderEnemyState) =>
        EnemyAnimationPlayback.Advance(place, boulderEnemyState, BoulderAnimations.Segments,
            static (BoulderEnemyState state) => state.Playback, static (state, playback) => state.Playback = playback,
            static (_, cue) => throw new InvalidOperationException($"A cue-free Boulder animation raised {cue}."));

    /// <summary>Runs Boulder's main AI.</summary>
    internal void RunBoulderMainAi(RoomEnemyOccupant occupant, BoulderSpawnSettings boulderSpawnSettings, RoomEnemyPlace place, BoulderEnemyState boulderEnemyState, IEnemyAiFrame frame)
    {
        RunBoulderMain(occupant, boulderSpawnSettings, place, boulderEnemyState, frame.Samus, frame.Level);
    }
}
