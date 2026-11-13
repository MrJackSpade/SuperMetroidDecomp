using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Enemies.Sbug;

/// <summary>The Sbug species owner: its per-room state, behaviour and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class SbugBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.Sbug, EnemyDefinitionId.Sbug2];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemySlot slot, EnemyCommonParticipation common) => new SbugEntity(this, slot, common);

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

    /// <summary>Random-wandering activation resets the shared enemy RNG to 11.</summary>
    private const int SbugRandomSeed = 11;
    /// <summary>Random and forward-then-wait legs last 32 enemy frames before selection changes.</summary>
    private const int SbugMovementSegmentFrames = 32;
    /// <summary>Long wandering mode tracks 512 frames before dropping its distance-based reversal gate.</summary>
    private const int SbugLongRandomLifetime = 512;
    /// <summary>Both axes must be at least six blocks from Samus for the special reversal.</summary>
    private const int SbugReverseDistance = 96;

    /// <summary>Initializes the Sbug.</summary>
    internal static void InitializeSbug(RoomEnemySlot slot)
    {
        var state = new SbugEnemyState(slot)
        {
            // Enemy extension arrays are cleared with the room before initialization: no loop
            // is requested or installed until the first direction is selected.
            RequestedAnimation = null,
            InstalledAnimation = null,
            Action = SbugAction.WaitForSamus,
        };
        slot.SetSpeciesState(state);

        // The forward velocity uses the game-derived unsigned quarter-circle magnitudes and lets
        // the shared helper apply signs later from the initial angle. The side velocities instead use the signed,
        // intentionally imperfect short multiply.
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
        SetSbugFacing(slot, state, state.ForwardFacing);
    }

    /// <summary>Runs an Sbug's per-frame behaviour through all of its actions.</summary>
    internal void RunSbugMain(RoomEnemySlot slot, SamusState? samus, RoomLevelData? level)
    {
        if (samus is null)
            throw new InvalidOperationException("Sbug AI requires the active Samus actor.");

        SbugEnemyState state = slot.RequireSpeciesState<SbugEnemyState>();
        switch (state.Action)
        {
            case SbugAction.WaitForSamus:
                RunSbugWait(slot, state, samus);
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
                state.StraightMovementTimer = WrappingCountdown.Start(SbugLongRandomLifetime);
                SetSbugRandomSeed();
                state.Action = SbugAction.ChooseRandomDirectionAndReverseWhenFar;
                return;
            case SbugAction.ActivateMoveForwardThenWait:
                state.MovementTimer = SbugMovementSegmentFrames;
                state.Action = SbugAction.MoveForwardThenWait;
                return;
            case SbugAction.ActivateMoveTowardSamus:
                ActivateSbugTowardOrAway(slot, state, samus, relativeHeading: SbugHeading.Toward);
                return;
            case SbugAction.ActivateMoveAwayFromSamus:
                ActivateSbugTowardOrAway(slot, state, samus, relativeHeading: SbugHeading.Away);
                return;

            case SbugAction.MoveForward:
                MoveSbugForward(slot, state);
                return;
            case SbugAction.ZigZag:
                RunSbugZigZag(slot, state);
                return;
            case SbugAction.MoveTowardSamus:
            case SbugAction.MoveAwayFromSamus:
                AddSbugVelocity(slot, state.CustomXVelocity, state.CustomYVelocity);
                return;
            case SbugAction.MoveForwardThenWait:
                RunSbugForwardThenWait(slot, state);
                return;
            case SbugAction.MoveRandomlyUntilCollision:
                RunSbugCollisionAwareSegment(slot, state, level);
                return;
            case SbugAction.MoveStraightAndReverseWhenFar:
                RunSbugStraightReverseSegment(slot, state, samus);
                return;
            case SbugAction.ChooseRandomDirectionAndReverseWhenFar:
                ChooseSbugRandomDirection(slot, state, SbugAction.MoveStraightAndReverseWhenFar);
                return;
            case SbugAction.ChooseRandomDirectionUntilCollision:
                ChooseSbugRandomDirection(slot, state, SbugAction.MoveRandomlyUntilCollision);
                return;
            default:
                throw new InvalidDataException(
                    $"Sbug has no behaviour for action {state.Action?.ToString() ?? "none"}.");
        }
    }

    /// <summary>Arms the population-selected behavior only when Samus enters its square trigger region.</summary>
    private static void RunSbugWait(RoomEnemySlot slot, SbugEnemyState state, SamusState samus)
    {
        int threshold = state.ActivationRadius;
        int xDistance = CoordinateAxis.Distance(slot.Position.X.Whole, samus.Position.X.Whole);
        int yDistance = CoordinateAxis.Distance(slot.Position.Y.Whole, samus.Position.Y.Whole);
        if (xDistance >= threshold || yDistance >= threshold)
            return;

        state.Action = SbugMovement.ActivationAction(state.ActivationBehavior);
    }

    /// <summary>Converts Samus's bearing into Sbug's angular convention and chosen direction.</summary>
    private static void ActivateSbugTowardOrAway(
        RoomEnemySlot slot,
        SbugEnemyState state,
        SamusState samus,
        SbugHeading relativeHeading)
    {
        // The angle of Samus from the enemy uses zero-up/clockwise units. Subtracting it from
        // 64 converts to Sbug's zero-right/counter-clockwise mathematical convention;
        // adding 128 selects the exact opposite direction for behavior six.
        TrigonometryAngle heading = EnemyMath.CalculateBearing(
            new(CoordinateAxis.Difference(samus.Position.X.Whole, slot.Position.X.Whole), CoordinateAxis.Difference(samus.Position.Y.Whole, slot.Position.Y.Whole))).ToTableAngle();
        state.FaceCustomHeading((relativeHeading == SbugHeading.Away ? heading.TurnedBy(TrigonometryAngle.HalfTurn) : heading).ToAngle());
        RecalculateSbugCustomDirection(slot, state);
        state.Action = relativeHeading == SbugHeading.Away
            ? SbugAction.MoveAwayFromSamus
            : SbugAction.MoveTowardSamus;
    }

    /// <summary>Alternates left and right diagonal legs on the enemy's own update count.</summary>
    private static void RunSbugZigZag(RoomEnemySlot slot, SbugEnemyState state)
    {
        // The global enemy frame counter divides motion into alternating 16-frame diagonal
        // legs. It is sampled before the scheduler increments it for the current frame.
        if (!SbugDefinitions.ZigZagSecondLeg.IsSetIn(slot.FrameCounter))
        {
            SetSbugFacing(slot, state, state.LeftFacing);
            AddSbugVelocity(slot, state.LeftXVelocity, state.LeftYVelocity);
        }
        else
        {
            SetSbugFacing(slot, state, state.RightFacing);
            AddSbugVelocity(slot, state.RightXVelocity, state.RightYVelocity);
        }
    }

    /// <summary>Moves straight through its signed countdown, then returns to the proximity wait.</summary>
    private static void RunSbugForwardThenWait(RoomEnemySlot slot, SbugEnemyState state)
    {
        if (--state.MovementTimer < 0)
        {
            state.Action = SbugAction.WaitForSamus;
            return;
        }

        MoveSbugForward(slot, state);
    }

    /// <summary>Stops a random leg on its first axis collision or chooses a new leg after timeout.</summary>
    private void RunSbugCollisionAwareSegment(
        RoomEnemySlot slot,
        SbugEnemyState state,
        RoomLevelData? level)
    {
        if (--state.MovementTimer < 0)
        {
            state.Action = SbugAction.ChooseRandomDirectionUntilCollision;
            return;
        }
        if (level is null)
            throw new InvalidOperationException("Collision-aware Sbug movement requires room level data.");

        // The movement stops at the first colliding axis. The common movers also perform the
        // game's wall alignment, so a collision frame must not receive a second raw
        // position adjustment here.
        if (_terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(
                level,
                slot,
                state.CustomXVelocity))
        {
            state.Action = SbugAction.WaitForSamus;
            return;
        }
        if (_terrain.MoveEnemyVertically(level, slot, state.CustomYVelocity))
            state.Action = SbugAction.WaitForSamus;
    }

    /// <summary>Reverses only when both Samus distances reach six blocks during the long leg.</summary>
    private static void RunSbugStraightReverseSegment(
        RoomEnemySlot slot,
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
            int xDistance = CoordinateAxis.Distance(slot.Position.X.Whole, samus.Position.X.Whole);
            int yDistance = CoordinateAxis.Distance(slot.Position.Y.Whole, samus.Position.Y.Whole);
            if (xDistance >= SbugReverseDistance && yDistance >= SbugReverseDistance)
                ReverseSbugCustomDirection(slot, state);
        }

        AddSbugVelocity(slot, state.CustomXVelocity, state.CustomYVelocity);
    }

    /// <summary>Offsets the current angle by a random value below 256, then begins a 32-frame movement leg.</summary>
    private void ChooseSbugRandomDirection(
        RoomEnemySlot slot,
        SbugEnemyState state,
        SbugAction movement)
    {
        // The random draw, 0 through 255 steps, less a quarter turn, turns the heading either way around the circle.
        int randomTurn = _room.Random.Next(SbugMovementDefinitions.RandomTurn);
        state.TurnCustomHeading(RotationAngle.FromSteps(randomTurn) - RotationAngle.QuarterTurn);
        RecalculateSbugCustomDirection(slot, state);
        state.MovementTimer = SbugMovementSegmentFrames;
        state.Action = movement;
    }

    /// <summary>Rebuilds signed velocities and facing animation from the chosen custom angle.</summary>
    private static void RecalculateSbugCustomDirection(RoomEnemySlot slot, SbugEnemyState state)
    {
        CalculateSignedSbugVelocities(
            state.CustomHeading.ToTrigonometryAngle(),
            state.Speed,
            out SignedSixteenSixteen customX,
            out SignedSixteenSixteen customY);
        state.CustomXVelocity = customX;
        state.CustomYVelocity = customY;
        state.CustomFacing = SbugMovement.FacingOf(state.CustomHeading);
        SetSbugFacing(slot, state, state.CustomFacing);
    }

    /// <summary>Reverses the Sbug's custom velocity and folds its facing on reversal.</summary>
    private static void ReverseSbugCustomDirection(RoomEnemySlot slot, SbugEnemyState state)
    {
        state.CustomXVelocity = -state.CustomXVelocity;
        state.CustomYVelocity = -state.CustomYVelocity;

        // The reversal does not show the opposite facing: as the game does, it turns the facing a quarter turn
        // counter-clockwise and folds it into the four facings from right to up-left.
        state.CustomFacing = SbugMovement.ReversedFacing(state.CustomFacing);
        SetSbugFacing(slot, state, state.CustomFacing);
    }

    /// <summary>Applies unsigned quarter-wave magnitudes with signs from the initial angle.</summary>
    private static void MoveSbugForward(RoomEnemySlot slot, SbugEnemyState state)
    {
        // Moving the enemy by its angle and speeds applies signs to the unsigned magnitudes by
        // testing the initial angle independently for X and Y.
        TrigonometryAngle initialAngle = state.InitialAngle.ToTrigonometryAngle();
        bool subtractX = initialAngle.TurnedBySteps(EnemyTrigonometryAngleDefinitions.QuarterTurn).IsInSecondHalf;
        bool subtractY = initialAngle.TurnedBySteps(EnemyTrigonometryAngleDefinitions.HalfTurn).IsInSecondHalf;
        slot.Position = slot.Position.WithX(slot.Position.X.MovedBy(ForwardDisplacement(state.ForwardXVelocity, subtractX)));
        slot.Position = slot.Position.WithY(slot.Position.Y.MovedBy(ForwardDisplacement(state.ForwardYVelocity, subtractY)));
    }

    /// <summary>Adds both signed 16.16 custom velocities to the actor position.</summary>
    private static void AddSbugVelocity(
        RoomEnemySlot slot,
        SignedSixteenSixteen xVelocity,
        SignedSixteenSixteen yVelocity)
    {
        slot.Position = slot.Position.WithX(slot.Position.X.MovedBy(xVelocity));
        slot.Position = slot.Position.WithY(slot.Position.Y.MovedBy(yVelocity));
    }

    /// <summary>
    /// A forward speed as the displacement it moves the Sbug by, along its axis or, when <paramref name="against"/>,
    /// against it; a speed of 32768 pixels or more reads as the other way, as the game's 16.16 addition does.
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
        UnsignedSixteenSixteen.ShareOfWholePixels(speed, Trigonometry.UnsignedHalfWave(angle.TurnedBySteps(phase).HalfWaveIndex));

    /// <summary>Samples the signed short multiply path for X and Y phase offsets.</summary>
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
        EnemyMath.ReadEightBitSineFixedProduct(angle.TurnedBySteps(phase), speed);

    /// <summary>Restarts the selected animation only when its facing animation differs from the installed one.</summary>
    private static void SetSbugFacing(
        RoomEnemySlot slot,
        SbugEnemyState state,
        CompassDirection facing)
    {
        state.RequestedAnimation = SbugMovement.FacingAnimation(facing);
        if (state.RequestedAnimation == state.InstalledAnimation)
            return;

        state.InstalledAnimation = state.RequestedAnimation;
        state.Playback = EnemyAnimationPlayback.Start(slot, SbugAnimations.StartingWith(state.RequestedAnimation.Value));
    }

    /// <summary>Resets the shared enemy random seed when random wandering is activated.</summary>
    private void SetSbugRandomSeed()
    {
        _room.Random.Seed(SbugRandomSeed);
    }

    /// <summary>Advances Sbug's typed animation.</summary>
    internal static void AdvanceSbugTypedAnimation(RoomEnemySlot slot, EnemyAnimationCueContext _) =>
        EnemyAnimationPlayback.Advance(slot, SbugAnimations.Segments,
            static (SbugEnemyState state) => state.Playback, static (state, playback) => state.Playback = playback,
            static (_, cue) => throw new InvalidOperationException($"Sbug's cue-free animation raised {cue}."));

    /// <summary>Runs Sbug's main AI.</summary>
    internal void RunSbugMainAi(RoomEnemySlot slot, IEnemyAiFrame frame)
    {
        RunSbugMain(slot, frame.Samus, frame.Level);
    }
}
