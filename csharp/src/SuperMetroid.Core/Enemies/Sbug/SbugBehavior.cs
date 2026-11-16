using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Enemies.Sbug;

/// <summary>The Sbug species owner: its per-room state, behaviour and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class SbugBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.Sbug];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemyPlace place, EnemyCommonParticipation common) => new SbugEntity(this, place, common);

    /// <summary>Provides the shared random seed and draws for wandering variants.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Resolves wall and floor collisions for random movement segments.</summary>
    private readonly EnemyTerrain _terrain;

    /// <summary>Binds Sbug AI to the loaded room's RNG and collision geometry.</summary>
    internal SbugBehavior(EnemyRoomBinding room, EnemyTerrain terrain)
    {
        _room = room;
        _terrain = terrain;
    }

    /// <summary>Initializes the Sbug.</summary>
    internal static void InitializeSbug(RoomEnemyPlace place)
    {
        // A new Sbug plays no animation until it selects its first direction.
        var state = new SbugEnemyState(place.SpawnSettings<SbugSpawnSettings>(), SbugAction.WaitForSamus);
        place.SetSpeciesState(state);

        // The forward velocity uses the unsigned quarter-circle magnitudes and lets the shared helper apply signs later
        // from the initial angle. The side velocities instead use the signed short circle product.
        int speed = state.Speed;
        RotationAngle leftAngle = state.InitialAngle.TurnedBySteps(-EnemyTrigonometryAngleDefinitions.EighthTurn);
        RotationAngle rightAngle = state.InitialAngle.TurnedBySteps(EnemyTrigonometryAngleDefinitions.EighthTurn);
        state.ForwardXVelocity = CalculateUnsignedSbugMagnitude(
            state.InitialAngle.ToTrigonometryAngle(),
            speed,
            phase: EnemyTrigonometryAngleDefinitions.QuarterTurn);
        state.ForwardYVelocity = CalculateUnsignedSbugMagnitude(
            state.InitialAngle.ToTrigonometryAngle(),
            speed,
            phase: EnemyTrigonometryAngleDefinitions.HalfTurn);
        CalculateSignedSbugVelocities(
            leftAngle.ToTrigonometryAngle(),
            speed,
            out SignedSixteenSixteen leftX,
            out SignedSixteenSixteen leftY);
        state.LeftXVelocity = leftX;
        state.LeftYVelocity = leftY;
        CalculateSignedSbugVelocities(
            rightAngle.ToTrigonometryAngle(),
            speed,
            out SignedSixteenSixteen rightX,
            out SignedSixteenSixteen rightY);
        state.RightXVelocity = rightX;
        state.RightYVelocity = rightY;

        state.ForwardFacing = SbugMovement.FacingOf(state.InitialAngle);
        state.LeftFacing = SbugMovement.FacingOf(leftAngle);
        state.RightFacing = SbugMovement.FacingOf(rightAngle);
        SetSbugFacing(place, state, state.ForwardFacing);
    }

    /// <summary>Runs an Sbug's per-frame behaviour through all of its actions.</summary>
    internal void RunSbugMain(RoomEnemyPlace place, SamusState samus, RoomLevelData level)
    {
        SbugEnemyState state = place.RequireSpeciesState<SbugEnemyState>();
        switch (state.Action)
        {
            case SbugAction.WaitForSamus:
                RunSbugWait(place, state, samus);
                return;

            // Activation actions deliberately consume a frame merely replacing
            // their own AI action. Movement begins on the following main AI call.
            case SbugAction.ActivateMoveForward:
                state.Action = SbugAction.MoveForward;
                return;
            case SbugAction.ActivateZigZag:
                state.Action = SbugAction.ZigZag;
                return;
            case SbugAction.ActivateRandomUntilCollision:
                SetSbugRandomSeed();
                state.Action = SbugAction.ChooseRandomDirectionUntilCollision;
                return;
            case SbugAction.ActivateRandomAndReverseWhenFar:
                state.StraightMovementTimer = WrappingCountdown.Start(SbugMovementDefinitions.LongWanderDuration);
                SetSbugRandomSeed();
                state.Action = SbugAction.ChooseRandomDirectionAndReverseWhenFar;
                return;
            case SbugAction.ActivateMoveForwardThenWait:
                state.MovementTimer = SbugMovementDefinitions.LegDuration;
                state.Action = SbugAction.MoveForwardThenWait;
                return;
            case SbugAction.ActivateMoveTowardSamus:
                ActivateSbugTowardOrAway(place, state, samus, relativeHeading: SbugHeading.Toward);
                return;
            case SbugAction.ActivateMoveAwayFromSamus:
                ActivateSbugTowardOrAway(place, state, samus, relativeHeading: SbugHeading.Away);
                return;

            case SbugAction.MoveForward:
                MoveSbugForward(place, state);
                return;
            case SbugAction.ZigZag:
                RunSbugZigZag(place, state);
                return;
            case SbugAction.MoveTowardSamus:
            case SbugAction.MoveAwayFromSamus:
                AddSbugVelocity(place, state.CustomXVelocity, state.CustomYVelocity);
                return;
            case SbugAction.MoveForwardThenWait:
                RunSbugForwardThenWait(place, state);
                return;
            case SbugAction.MoveRandomlyUntilCollision:
                RunSbugCollisionAwareSegment(place, state, level);
                return;
            case SbugAction.MoveStraightAndReverseWhenFar:
                RunSbugStraightReverseSegment(place, state, samus);
                return;
            case SbugAction.ChooseRandomDirectionAndReverseWhenFar:
                ChooseSbugRandomDirection(place, state, SbugAction.MoveStraightAndReverseWhenFar);
                return;
            case SbugAction.ChooseRandomDirectionUntilCollision:
                ChooseSbugRandomDirection(place, state, SbugAction.MoveRandomlyUntilCollision);
                return;
            default:
                throw new InvalidDataException(
                    $"Sbug has no behaviour for action {state.Action}.");
        }
    }

    /// <summary>Arms the population-selected behavior only when Samus enters its square trigger region.</summary>
    private static void RunSbugWait(RoomEnemyPlace place, SbugEnemyState state, SamusState samus)
    {
        int threshold = state.ActivationRadius;
        int xDistance = CoordinateAxis.Distance(place.Position.X.Whole, samus.Position.X.Whole);
        int yDistance = CoordinateAxis.Distance(place.Position.Y.Whole, samus.Position.Y.Whole);
        if (xDistance >= threshold || yDistance >= threshold)
            return;

        state.Action = SbugMovement.ActivationAction(state.ActivationBehavior);
    }

    /// <summary>Converts Samus's bearing into Sbug's angular convention and chosen direction.</summary>
    private static void ActivateSbugTowardOrAway(
        RoomEnemyPlace place,
        SbugEnemyState state,
        SamusState samus,
        SbugHeading relativeHeading)
    {
        // The angle of Samus from the enemy uses zero-up/clockwise units. Subtracting it from
        // 64 converts to Sbug's zero-right/counter-clockwise mathematical convention;
        // adding 128 selects the exact opposite direction for moving away from Samus.
        TrigonometryAngle heading = EnemyMath.CalculateBearing(
            new(CoordinateAxis.Difference(samus.Position.X.Whole, place.Position.X.Whole), CoordinateAxis.Difference(samus.Position.Y.Whole, place.Position.Y.Whole))).ToTrigonometryAngle();
        state.FaceCustomHeading((relativeHeading == SbugHeading.Away ? heading.TurnedBy(TrigonometryAngle.HalfTurn) : heading).ToAngle());
        RecalculateSbugCustomDirection(place, state);
        state.Action = relativeHeading == SbugHeading.Away
            ? SbugAction.MoveAwayFromSamus
            : SbugAction.MoveTowardSamus;
    }

    /// <summary>Alternates left and right diagonal legs on the enemy's own update count.</summary>
    private static void RunSbugZigZag(RoomEnemyPlace place, SbugEnemyState state)
    {
        // The global enemy frame counter divides motion into alternating 16-frame diagonal
        // legs. It is sampled before the scheduler increments it for the current frame.
        if (!SbugDefinitions.ZigZagSecondLeg.IsSetIn(place.FrameCounter))
        {
            SetSbugFacing(place, state, state.LeftFacing);
            AddSbugVelocity(place, state.LeftXVelocity, state.LeftYVelocity);
        }
        else
        {
            SetSbugFacing(place, state, state.RightFacing);
            AddSbugVelocity(place, state.RightXVelocity, state.RightYVelocity);
        }
    }

    /// <summary>Moves straight through its signed countdown, then returns to the proximity wait.</summary>
    private static void RunSbugForwardThenWait(RoomEnemyPlace place, SbugEnemyState state)
    {
        if (--state.MovementTimer < 0)
        {
            state.Action = SbugAction.WaitForSamus;
            return;
        }

        MoveSbugForward(place, state);
    }

    /// <summary>Stops a random leg on its first axis collision or chooses a new leg after timeout.</summary>
    private void RunSbugCollisionAwareSegment(
        RoomEnemyPlace place,
        SbugEnemyState state,
        RoomLevelData level)
    {
        if (--state.MovementTimer < 0)
        {
            state.Action = SbugAction.ChooseRandomDirectionUntilCollision;
            return;
        }

        // The movement stops at the first colliding axis. The common movers also perform the
        // game's wall alignment, so a collision frame must not receive a second raw
        // position adjustment here.
        if (_terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(
                level,
                place,
                state.CustomXVelocity))
        {
            state.Action = SbugAction.WaitForSamus;
            return;
        }
        if (_terrain.MoveEnemyVertically(level, place, state.CustomYVelocity))
            state.Action = SbugAction.WaitForSamus;
    }

    /// <summary>Reverses only when both Samus distances reach six blocks during the long leg.</summary>
    private static void RunSbugStraightReverseSegment(
        RoomEnemyPlace place,
        SbugEnemyState state,
        SamusState samus)
    {
        state.StraightMovementTimer = state.StraightMovementTimer.Decrement().Remaining;
        if (!state.StraightMovementTimer.IsNegative)
        {
            if (--state.MovementTimer < 0)
            {
                state.Action = SbugAction.ChooseRandomDirectionAndReverseWhenFar;
                return;
            }

            // The branch reverses only when both coordinate differences are at
            // least six blocks. This is a rectangular test, not Euclidean distance.
            int xDistance = CoordinateAxis.Distance(place.Position.X.Whole, samus.Position.X.Whole);
            int yDistance = CoordinateAxis.Distance(place.Position.Y.Whole, samus.Position.Y.Whole);
            if (xDistance >= SbugMovementDefinitions.ReverseDistance && yDistance >= SbugMovementDefinitions.ReverseDistance)
                ReverseSbugCustomDirection(place, state);
        }

        AddSbugVelocity(place, state.CustomXVelocity, state.CustomYVelocity);
    }

    /// <summary>Offsets the current angle by a random value below 256, then begins a 32-frame movement leg.</summary>
    private void ChooseSbugRandomDirection(
        RoomEnemyPlace place,
        SbugEnemyState state,
        SbugAction movement)
    {
        // The random draw, 0 through 255 steps, less a quarter turn, turns the heading either way around the circle.
        int randomTurn = _room.Random.Next(SbugMovementDefinitions.RandomTurn);
        state.TurnCustomHeading(RotationAngle.FromSteps(randomTurn) - RotationAngle.QuarterTurn);
        RecalculateSbugCustomDirection(place, state);
        state.MovementTimer = SbugMovementDefinitions.LegDuration;
        state.Action = movement;
    }

    /// <summary>Rebuilds signed velocities and facing animation from the chosen custom angle.</summary>
    private static void RecalculateSbugCustomDirection(RoomEnemyPlace place, SbugEnemyState state)
    {
        CalculateSignedSbugVelocities(
            state.CustomHeading.ToTrigonometryAngle(),
            state.Speed,
            out SignedSixteenSixteen customX,
            out SignedSixteenSixteen customY);
        state.CustomXVelocity = customX;
        state.CustomYVelocity = customY;
        state.CustomFacing = SbugMovement.FacingOf(state.CustomHeading);
        SetSbugFacing(place, state, state.CustomFacing);
    }

    /// <summary>Reverses the Sbug's custom velocity and folds its facing on reversal.</summary>
    private static void ReverseSbugCustomDirection(RoomEnemyPlace place, SbugEnemyState state)
    {
        state.CustomXVelocity = -state.CustomXVelocity;
        state.CustomYVelocity = -state.CustomYVelocity;

        // The reversal does not show the opposite facing: as the game does, it turns the facing a quarter turn
        // counter-clockwise and folds it into the four facings from right to up-left.
        state.CustomFacing = SbugMovement.ReversedFacing(state.CustomFacing);
        SetSbugFacing(place, state, state.CustomFacing);
    }

    /// <summary>Applies unsigned quarter-wave magnitudes with signs from the initial angle.</summary>
    private static void MoveSbugForward(RoomEnemyPlace place, SbugEnemyState state)
    {
        // Moving the enemy by its angle and speeds applies signs to the unsigned magnitudes by
        // testing the initial angle independently for X and Y.
        TrigonometryAngle initialAngle = state.InitialAngle.ToTrigonometryAngle();
        bool subtractX = initialAngle.TurnedBySteps(EnemyTrigonometryAngleDefinitions.QuarterTurn).IsInSecondHalf;
        bool subtractY = initialAngle.TurnedBySteps(EnemyTrigonometryAngleDefinitions.HalfTurn).IsInSecondHalf;
        place.Position = place.Position.WithX(place.Position.X.MovedBy(ForwardDisplacement(state.ForwardXVelocity, subtractX)));
        place.Position = place.Position.WithY(place.Position.Y.MovedBy(ForwardDisplacement(state.ForwardYVelocity, subtractY)));
    }

    /// <summary>Adds both signed 1/65536-pixel custom velocities to the actor position.</summary>
    private static void AddSbugVelocity(
        RoomEnemyPlace place,
        SignedSixteenSixteen xVelocity,
        SignedSixteenSixteen yVelocity)
    {
        place.Position = place.Position.WithX(place.Position.X.MovedBy(xVelocity));
        place.Position = place.Position.WithY(place.Position.Y.MovedBy(yVelocity));
    }

    /// <summary>
    /// A forward speed as the displacement it moves the Sbug by, along its axis or, when <paramref name="against"/>,
    /// against it; a speed of 32768 pixels or more reads as the other way, as the game's 1/65536-pixel addition does.
    /// </summary>
    private static SignedSixteenSixteen ForwardDisplacement(UnsignedSixteenSixteen speed, bool against)
    {
        SignedSixteenSixteen along = speed.AsDisplacement();
        return against ? SignedSixteenSixteen.Zero - along : along;
    }

    /// <summary>
    /// The forward speed along one axis: the Sbug's whole-pixel speed, at most 255, scaled by the unsigned half-wave
    /// sample, in 65536ths, of the angle turned by <paramref name="phase"/>.
    /// </summary>
    internal static UnsignedSixteenSixteen CalculateUnsignedSbugMagnitude(
        TrigonometryAngle angle,
        int speed,
        int phase) =>
        UnsignedSixteenSixteen.ShareOfWholePixels(speed, Trigonometry.UnsignedHalfWave(angle.TurnedBySteps(phase).StepsIntoHalfTurn));

    /// <summary>The side speeds along X and Y: the short circle products of the angle turned a quarter and a half turn on.</summary>
    private static void CalculateSignedSbugVelocities(
        TrigonometryAngle angle,
        int speed,
        out SignedSixteenSixteen xVelocity,
        out SignedSixteenSixteen yVelocity)
    {
        xVelocity = CalculateSignedSbugComponent(angle, speed, phase: EnemyTrigonometryAngleDefinitions.QuarterTurn);
        yVelocity = CalculateSignedSbugComponent(angle, speed, phase: EnemyTrigonometryAngleDefinitions.HalfTurn);
    }

    /// <summary>
    /// The side speed along one axis: the short circle product (<see cref="ShortCircleProduct"/>) of the angle turned by
    /// <paramref name="phase"/> at the Sbug's speed, every authored one below five.
    /// </summary>
    internal static SignedSixteenSixteen CalculateSignedSbugComponent(TrigonometryAngle angle, int speed, int phase) =>
        EnemyMath.CircleSine(angle.TurnedBySteps(phase), speed);

    /// <summary>Restarts the selected animation only when its facing animation differs from the one playing (#2145).</summary>
    private static void SetSbugFacing(
        RoomEnemyPlace place,
        SbugEnemyState state,
        CompassDirection facing) =>
        state.Playback = EnemyAnimationPlayback.StartUnlessPlaying(place, state.Playback, SbugAnimations.StartingWith(SbugMovement.FacingAnimation(facing)));

    /// <summary>Resets the shared enemy random seed when random wandering is activated.</summary>
    private void SetSbugRandomSeed()
    {
        _room.Random.Seed(SbugMovementDefinitions.RandomSeed);
    }

    /// <summary>Advances Sbug's typed animation.</summary>
    internal static void AdvanceSbugTypedAnimation(RoomEnemyPlace place, EnemyAnimationCueContext _) =>
        EnemyAnimationPlayback.Advance(place, SbugAnimations.Segments,
            static (SbugEnemyState state) => state.Playback, static (state, playback) => state.Playback = playback,
            static (_, cue) => throw new InvalidOperationException($"Sbug's cue-free animation raised {cue}."));

    /// <summary>Runs Sbug's main AI.</summary>
    internal void RunSbugMainAi(RoomEnemyPlace place, IEnemyAiFrame frame)
    {
        RunSbugMain(place, frame.Samus, frame.Level);
    }
}
