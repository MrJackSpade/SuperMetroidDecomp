using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Enemies.Boulder;

/// <summary>The Boulder species owner: its per-room state, behaviour and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class BoulderBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.Boulder];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemyPlace place, EnemyCommonParticipation common) => new BoulderEntity(this, place, common);

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
    internal static void InitializeBoulder(RoomEnemyPlace place)
    {
        var state = new BoulderEnemyState(BoulderAction.WaitingForSamus);
        place.SetSpeciesState(state);

        state.BounceSpeedAccumulator = QuadraticSpeedCurvePlace.Start;
        state.TravelSpeedAccumulator = QuadraticSpeedCurvePlace.Start;
        state.ImpactsSoFar = 0;
        state.InitialPosition = place.Position.Whole;
        state.PreviousY = place.Position.Y;

        BoulderSpawnSettings settings = place.SpawnSettings<BoulderSpawnSettings>();
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
                state.StartsRollingWithoutBounce = true;
                break;
            default:
                throw new InvalidOperationException($"Undefined {nameof(BoulderStart)} {settings.Start}.");
        }
        place.Position = place.Position.WithWholeY(state.InitialPosition.Y - initialYOffset);

        int horizontalReach = -settings.TriggerReach;

        // The population entry's trigger height is the strict Samus-Y threshold.
        int verticalReach = settings.TriggerHeight;
        state.Roll = settings.Roll;
        switch (settings.Roll)
        {
            case BoulderRoll.Right:
                horizontalReach = -horizontalReach;
                state.Playback = new(BoulderAnimations.StartingWith(BoulderSegment.Right), AnimationPlace.StartOfAnimation);
                break;
            case BoulderRoll.Left:
            case BoulderRoll.LeftBreakingOnImpact:
                state.Playback = new(BoulderAnimations.StartingWith(BoulderSegment.Left), AnimationPlace.StartOfAnimation);
                break;
            default:
                throw new InvalidOperationException($"Undefined {nameof(BoulderRoll)} {settings.Roll}.");
        }
        state.TriggerReach = Offset16<RoomPixels>.Of(horizontalReach, verticalReach);

        state.VerticalCompensation = BoulderMotionDefinitions.RollingGroundReach;
    }

    /// <summary>Runs the wait, fall, rebound, roll or inert phase with required Samus and terrain state.</summary>
    private void RunBoulderMain(RoomEnemyPlace place, BoulderEnemyState state, SamusState samus, RoomLevelData level)
    {
        switch (state.Action)
        {
            case BoulderAction.WaitingForSamus:
                RunBoulderWaiting(place, state, samus);
                break;
            case BoulderAction.InitialFall:
                RunBoulderInitialFall(place, state);
                break;
            case BoulderAction.Rebound:
                RunBoulderRebound(place, state);
                break;
            case BoulderAction.Falling:
                RunBoulderFalling(place, state, level);
                break;
            case BoulderAction.Rolling:
                RunBoulderRolling(place, state, level);
                break;
            case BoulderAction.Inert:
                break;
            default:
                throw new InvalidDataException(
                    $"Boulder has no behaviour for action {state.Action}.");
        }
    }

    /// <summary>Starts the bounce chain or direct roll only when Samus enters the direction-specific X window below the boulder.</summary>
    private static void RunBoulderWaiting(RoomEnemyPlace place, BoulderEnemyState state, SamusState samus)
    {
        int deltaY = CoordinateAxis.Difference(samus.Position.Y.Whole, place.Position.Y.Whole);
        if (deltaY < 0 || deltaY >= state.TriggerReach.Y)
            return;

        int deltaX = CoordinateAxis.Difference(samus.Position.X.Whole, place.Position.X.Whole);
        bool enteredHorizontalWindow = state.Roll != BoulderRoll.Right
            ? deltaX < 0 && deltaX >= state.TriggerReach.X
            : deltaX >= 0 && deltaX < state.TriggerReach.X;
        if (!enteredHorizontalWindow)
            return;

        state.Action = state.StartsRollingWithoutBounce
            ? BoulderAction.Rolling
            : BoulderAction.InitialFall;
    }

    /// <summary>Accelerates down toward the recorded spawn height, then clamps and starts the first rebound.</summary>
    private static void RunBoulderInitialFall(RoomEnemyPlace place, BoulderEnemyState state)
    {
        AddBoulderVerticalVelocity(place, state.BounceSpeedAccumulator, direction: VerticalDirection.Down);
        if (place.Position.Y.Whole < state.InitialPosition.Y)
        {
            state.BounceSpeedAccumulator = AddAndCap(
                state.BounceSpeedAccumulator,
                BoulderMotionDefinitions.BounceSpeedStep,
                BoulderMotionDefinitions.MaximumBouncingSpeed);
            return;
        }

        place.Position = place.Position.WithWholeY(state.InitialPosition.Y);
        state.Action = BoulderAction.Rebound;
        state.BounceSpeedAccumulator = BoulderMotionDefinitions.FirstReboundBounceSpeed;
    }

    /// <summary>Applies upward quadratic motion while reducing bounce speed, adding sideways travel, then returns to falling.</summary>
    private static void RunBoulderRebound(RoomEnemyPlace place, BoulderEnemyState state)
    {
        AddBoulderVerticalVelocity(place, state.BounceSpeedAccumulator, direction: VerticalDirection.Up);

        // The game subtracts one mark and checks whether the result is negative, storing zero in place of a
        // result below zero, so the clock never keeps a value below zero past this update.
        QuadraticSpeedCurvePlace slowed = state.BounceSpeedAccumulator - BoulderMotionDefinitions.BounceSpeedStep;
        if (slowed.IsNegative)
        {
            state.BounceSpeedAccumulator = QuadraticSpeedCurvePlace.Start;
            state.Action = BoulderAction.Falling;
            return;
        }
        state.BounceSpeedAccumulator = slowed;

        AddBoulderHorizontalVelocity(place, state);
        state.TravelSpeedAccumulator = AddAndCap(
            state.TravelSpeedAccumulator,
            BoulderMotionDefinitions.BouncingTravelSpeedStep,
            BoulderMotionDefinitions.MaximumBouncingSpeed);
    }

    /// <summary>Accelerates through terrain contact; ordinary impacts consume bounces while a boulder that breaks on impact breaks at the first.</summary>
    internal void RunBoulderFalling(RoomEnemyPlace place, BoulderEnemyState state, RoomLevelData level)
    {
        SignedSixteenSixteen verticalDisplacement = state.BounceSpeedAccumulator.Speed.Positive;
        if (_terrain.MoveEnemyVertically(level, place, verticalDisplacement))
        {
            _audio.QueueEnemySound(EnemySoundEffectDefinitions.BoulderImpactSound, priority: SoundRequestPriority.Normal);
            if (state.Roll == BoulderRoll.LeftBreakingOnImpact)
            {
                place.Presence = place.Presence.With(EnemyPresence.Deleted);
                _projectiles.SpawnDustOrExplosion(
                    place.Position.Whole,
                    BoulderMotionDefinitions.BreakDust);
                _audio.QueueEnemySound(EnemySoundEffectDefinitions.BoulderBreakSound, priority: SoundRequestPriority.Normal);
                return;
            }

            state.Action = BoulderAction.Rebound;

            // The third impact still restarts the bounce clock, at the curve's start, even though it enters rolling.
            state.BounceSpeedAccumulator = BoulderBounces.RestartPlace(state.ImpactsSoFar);
            if (++state.ImpactsSoFar == BoulderBounces.ImpactCount)
            {
                state.PreviousY = place.Position.Y;
                state.Action = BoulderAction.Rolling;
            }
            return;
        }

        // The falling clock is not capped: its next read selects the following curve mark, and the curve
        // rejects the place past its last mark before the clock could approach the limit of its range.
        state.BounceSpeedAccumulator += BoulderMotionDefinitions.BounceSpeedStep;
        AddBoulderHorizontalVelocity(place, state);
        state.TravelSpeedAccumulator = AddAndCap(
            state.TravelSpeedAccumulator,
            BoulderMotionDefinitions.BouncingTravelSpeedStep,
            BoulderMotionDefinitions.MaximumBouncingSpeed);
    }

    /// <summary>Tracks the floor with vertical compensation and advances horizontally until a wall hides/deletes the actor and spawns dust.</summary>
    private void RunBoulderRolling(RoomEnemyPlace place, BoulderEnemyState state, RoomLevelData level)
    {
        SignedSixteenSixteen verticalDisplacement =
            state.TravelSpeedAccumulator.Speed.Positive +
            SignedSixteenSixteen.FromWholePixels(state.VerticalCompensation);
        _terrain.MoveEnemyVertically(level, place, verticalDisplacement);
        place.Position = place.Position.WithWhole(place.Position.Whole - new Offset16<RoomPixels>(0, state.VerticalCompensation));

        SignedSixteenSixteen horizontalDisplacement = ReadBoulderHorizontalDisplacement(state);
        if (_terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(level, place, horizontalDisplacement))
        {
            // Setting the hidden and deleted properties both hides and deletes the boulder. The inert action is
            // still written before the next update's processing pass clears it.
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
            state.TravelSpeedAccumulator = AddAndCap(
                state.TravelSpeedAccumulator,
                BoulderMotionDefinitions.RollingTravelSpeedStep,
                BoulderMotionDefinitions.MaximumRollingTravelSpeed);
            if (place.Position.Y == state.PreviousY)
                state.VerticalCompensation = 0;
        }

        state.PreviousY = place.Position.Y;
    }

    /// <summary>Moves the actor horizontally by its signed quadratic travel speed, wrapping within the coordinate range.</summary>
    private static void AddBoulderHorizontalVelocity(RoomEnemyPlace place, BoulderEnemyState state) =>
        place.Position = place.Position.WithX(place.Position.X.MovedBy(ReadBoulderHorizontalDisplacement(state)));

    /// <summary>Reads the quadratic speed sample and signs it according to the spawn direction selector.</summary>
    private static SignedSixteenSixteen ReadBoulderHorizontalDisplacement(BoulderEnemyState state) =>
        state.TravelSpeedAccumulator.Speed.Toward(state.Roll != BoulderRoll.Right ? EnemySpeedSign.Negative : EnemySpeedSign.Positive);

    /// <summary>Moves the actor vertically by a speed of the quadratic speed curve toward <paramref name="direction"/>, wrapping within the coordinate range.</summary>
    private static void AddBoulderVerticalVelocity(RoomEnemyPlace place, QuadraticSpeedCurvePlace accumulator, VerticalDirection direction)
    {
        SignedSixteenSixteen displacement = accumulator.Speed.Toward(direction == VerticalDirection.Up ? EnemySpeedSign.Negative : EnemySpeedSign.Positive);
        place.Position = place.Position.WithY(place.Position.Y.MovedBy(displacement));
    }

    /// <summary>
    /// Advances a place on the speed curve by <paramref name="increment"/>, holding it at <paramref name="maximum"/>.
    /// </summary>
    private static QuadraticSpeedCurvePlace AddAndCap(
        QuadraticSpeedCurvePlace value, QuadraticSpeedCurvePlace increment, QuadraticSpeedCurvePlace maximum) =>
        QuadraticSpeedCurvePlace.Min(value + increment, maximum);

    /// <summary>Advances Boulder's typed animation.</summary>
    internal static void AdvanceBoulderTypedAnimation(RoomEnemyPlace place, EnemyAnimationCueContext _) =>
        EnemyAnimationPlayback.Advance(place, BoulderAnimations.Segments,
            static (BoulderEnemyState state) => state.Playback, static (state, playback) => state.Playback = playback,
            static (_, cue) => throw new InvalidOperationException($"A cue-free Boulder animation raised {cue}."));

    /// <summary>Runs Boulder's main AI.</summary>
    internal void RunBoulderMainAi(RoomEnemyPlace place, IEnemyAiFrame frame)
    {
        RunBoulderMain(place, place.RequireSpeciesState<BoulderEnemyState>(), frame.Samus, frame.Level);
    }
}
