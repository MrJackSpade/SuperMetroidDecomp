using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Enemies.Sbug;

/// <summary>The Sbug species owner: its per-room state, behaviour and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class SbugBehavior : IEnemySpecies, IEnemySpawnFactory<SbugSpawnSettings>
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.Sbug];

    /// <inheritdoc/>
    public EnemyOccupant Create(RoomEnemyOccupant occupant, SbugSpawnSettings settings, RoomEnemyPlace place, EnemyCommonParticipation common) => EnemyOccupant.Running(new SbugEntity(occupant, place, common, settings));

    /// <summary>Initializes the Sbug.</summary>
    /// <returns>The state the enemy holds from its setup.</returns>
    internal static SbugEnemyState InitializeSbug(RoomEnemyOccupant occupant, SbugSpawnSettings sbugSpawnSettings, RoomEnemyPlace place)
    {
        // A new Sbug starts the loop of its spawn heading's facing as it initializes.
        var state = new SbugEnemyState(SbugAction.WaitForSamus);
        SetSbugFacing(place, state, ForwardLeg(sbugSpawnSettings).Facing);
        return state;
    }

    /// <summary>
    /// The leg along the Sbug's spawn heading, which its forward movement takes: the placement's speed along the heading
    /// (<see cref="EnemyMath.VelocityAlong"/>).
    /// </summary>
    private static SbugLeg ForwardLeg(SbugSpawnSettings settings)
    {
        FixedVector<RoomPixels> velocity = EnemyMath.VelocityAlong(settings.InitialAngle.ToTrigonometryAngle(), settings.Speed);
        return new(velocity.X, velocity.Y, SbugMovement.FacingOf(settings.InitialAngle));
    }

    /// <summary>
    /// The zigzag leg an eighth of a turn from the Sbug's spawn heading, less it for the first leg and more for the
    /// second: its displacement is the signed short circle product of that heading at the placement's speed.
    /// </summary>
    private static SbugLeg ZigZagLeg(SbugSpawnSettings settings, bool secondLeg)
    {
        RotationAngle heading = secondLeg ? settings.InitialAngle + RotationAngle.EighthTurn : settings.InitialAngle - RotationAngle.EighthTurn;
        CalculateSignedSbugVelocities(heading.ToTrigonometryAngle(), settings.Speed, out SignedSixteenSixteen x, out SignedSixteenSixteen y);
        return new(x, y, SbugMovement.FacingOf(heading));
    }

    /// <summary>Runs an Sbug for one update through all of its actions.</summary>
    private static void RunSbugMain(RoomEnemyOccupant occupant, SbugSpawnSettings sbugSpawnSettings, RoomEnemyPlace place, SbugEnemyState state, SamusState samus)
    {
        switch (state.Action)
        {
            case SbugAction.WaitForSamus:
                RunSbugWait(sbugSpawnSettings, place, state, samus);
                return;

            // An activation only changes the bug's action, which takes its update; it moves from the next update.
            case SbugAction.ActivateMoveForward:
                state.Action = SbugAction.MoveForward;
                return;
            case SbugAction.ActivateZigZag:
                state.Action = SbugAction.ZigZag;
                return;
            case SbugAction.ActivateMoveForwardThenWait:
                state.MovementTimer = UpdateCountdown.For(SbugMovementDefinitions.LegDuration);
                state.Action = SbugAction.MoveForwardThenWait;
                return;

            case SbugAction.MoveForward:
                AddSbugVelocity(place, ForwardLeg(sbugSpawnSettings));
                return;
            case SbugAction.ZigZag:
                RunSbugZigZag(occupant, sbugSpawnSettings, place, state);
                return;
            case SbugAction.MoveForwardThenWait:
                RunSbugForwardThenWait(occupant, sbugSpawnSettings, place, state);
                return;
            default:
                throw new InvalidDataException(
                    $"Sbug has no behaviour for action {state.Action}.");
        }
    }

    /// <summary>Arms the population-selected behavior only when Samus enters its square trigger region.</summary>
    private static void RunSbugWait(SbugSpawnSettings settings, RoomEnemyPlace place, SbugEnemyState state, SamusState samus)
    {
        int threshold = settings.ActivationRadius;
        int xDistance = Math.Abs(place.Position.X.Whole - samus.Position.X.Whole);
        int yDistance = Math.Abs(place.Position.Y.Whole - samus.Position.Y.Whole);
        if (xDistance >= threshold || yDistance >= threshold)
            return;

        state.Action = SbugMovement.ActivationAction(settings.Behavior);
    }

    /// <summary>Alternates left and right diagonal legs on the enemy's own update count.</summary>
    private static void RunSbugZigZag(RoomEnemyOccupant occupant, SbugSpawnSettings sbugSpawnSettings, RoomEnemyPlace place, SbugEnemyState state)
    {
        // The enemy's update count alternates the diagonal legs at its zigzag cadence, read before this update is
        // counted.
        SbugLeg leg = ZigZagLeg(sbugSpawnSettings, secondLeg: SbugDefinitions.ZigZagSecondLeg.IsSetIn(place.FrameCounter));
        SetSbugFacing(place, state, leg.Facing);
        AddSbugVelocity(place, leg);
    }

    /// <summary>Moves straight for its leg, then returns to the proximity wait.</summary>
    private static void RunSbugForwardThenWait(RoomEnemyOccupant occupant, SbugSpawnSettings sbugSpawnSettings, RoomEnemyPlace place, SbugEnemyState state)
    {
        if (state.MovementTimer.StepEnds())
        {
            state.Action = SbugAction.WaitForSamus;
            return;
        }

        AddSbugVelocity(place, ForwardLeg(sbugSpawnSettings));
    }

    /// <summary>Moves the Sbug along <paramref name="leg"/>.</summary>
    private static void AddSbugVelocity(RoomEnemyPlace place, SbugLeg leg) =>
        AddSbugVelocity(place, leg.XVelocity, leg.YVelocity);

    /// <summary>Moves the Sbug by both its velocities.</summary>
    private static void AddSbugVelocity(
        RoomEnemyPlace place,
        SignedSixteenSixteen xVelocity,
        SignedSixteenSixteen yVelocity)
    {
        place.Position = place.Position.WithX(place.Position.X.MovedBy(xVelocity));
        place.Position = place.Position.WithY(place.Position.Y.MovedBy(yVelocity));
    }

    /// <summary>The side speeds along X and Y: the short circle products of the angle turned a quarter and a half turn on.</summary>
    private static void CalculateSignedSbugVelocities(
        TrigonometryAngle angle,
        int speed,
        out SignedSixteenSixteen xVelocity,
        out SignedSixteenSixteen yVelocity)
    {
        xVelocity = CalculateSignedSbugComponent(angle, speed, phase: TrigonometryAngle.QuarterTurn);
        yVelocity = CalculateSignedSbugComponent(angle, speed, phase: TrigonometryAngle.HalfTurn);
    }

    /// <summary>
    /// The side speed along one axis: the short circle product (<see cref="ShortCircleProduct"/>) of the angle turned by
    /// <paramref name="phase"/> at the Sbug's speed, every authored one below five.
    /// </summary>
    private static SignedSixteenSixteen CalculateSignedSbugComponent(TrigonometryAngle angle, int speed, TrigonometryAngle phase) =>
        EnemyMath.CircleSine(angle + phase, speed);

    /// <summary>Restarts the selected animation only when its facing animation differs from the one playing (#2145).</summary>
    private static void SetSbugFacing(
        RoomEnemyPlace place,
        SbugEnemyState state,
        CompassDirection facing) =>
        state.Playback = EnemyAnimationPlayback.StartUnlessPlaying(place, state.Playback, SbugAnimations.StartingWith(SbugMovement.FacingAnimation(facing)));

    /// <summary>Advances Sbug's typed animation.</summary>
    internal static void AdvanceSbugTypedAnimation(RoomEnemyPlace place, SbugEnemyState sbugEnemyState) =>
        EnemyAnimationPlayback.Advance(place, sbugEnemyState, SbugAnimations.Segments,
            static (SbugEnemyState state) => state.Playback, static (state, playback) => state.Playback = playback,
            static (_, cue) => throw new InvalidOperationException($"Sbug's cue-free animation raised {cue}."));

    /// <summary>Runs Sbug's main AI.</summary>
    internal static void RunSbugMainAi(RoomEnemyOccupant occupant, SbugSpawnSettings sbugSpawnSettings, RoomEnemyPlace place, SbugEnemyState sbugEnemyState, IEnemyAiFrame frame) => RunSbugMain(occupant, sbugSpawnSettings, place, sbugEnemyState, frame.Samus);
}
