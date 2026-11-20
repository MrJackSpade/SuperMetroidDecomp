using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Enemies.Bull;

/// <summary>The Bull species owner: its per-room state, behaviour and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class BullBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.Bull];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemyPlace place, EnemyCommonParticipation common) => new BullEntity(this, place, common);

    /// <summary>Hands each new Bull the previous Bull in its place, whose shot-reaction guard it inherits.</summary>
    private readonly PlaceInheritance<BullEnemyState> _inheritance;

    /// <summary>Binds the place-inheritance service Bull setup uses.</summary>
    /// <param name="inheritance">Hands each new Bull the previous Bull in its place.</param>
    internal BullBehavior(PlaceInheritance<BullEnemyState> inheritance) => _inheritance = inheritance;

    /// <summary>Initializes the Bull.</summary>
    internal void InitializeBull(RoomEnemyPlace place)
    {
        BullEnemyState state = _inheritance.Inherit(place.Ordinal, static previous => new BullEnemyState(previous, BullAction.MovementDelay));
        place.SetSpeciesState(state);

        state.Playback = EnemyAnimationPlayback.Start(place, BullAnimations.StartingWith(BullSegment.Idle));

        BullSpawnSettings settings = place.SpawnSettings<BullSpawnSettings>();
        state.AccelerationIntervalTimer = UpdateCountdown.For(settings.Intervals.Acceleration);
        state.ActivationTimer = UpdateCountdown.For(BullMovementDefinitions.MovementDelay);
    }

    /// <summary>Runs a Bull for one update through all four of its actions.</summary>
    internal static void RunBullMain(RoomEnemyPlace place, BullEnemyState state, SamusState samus)
    {
        // A kick's guard runs out on the update its countdown reaches zero.
        state.ShotReactionDisableTimer.Step();

        switch (state.Action)
        {
            case BullAction.MovementDelay:
                if (state.ActivationTimer.StepEnds())
                {
                    state.ActivationTimer = UpdateCountdown.For(BullMovementDefinitions.MovementDelay);
                    state.Action = BullAction.TargetSamus;
                }
                return;

            case BullAction.TargetSamus:
                state.AngleToSamus = CalculateBullAngleToSamus(place, samus);
                state.Angle = state.AngleToSamus.ToTrigonometryAngle();
                state.Action = BullAction.Accelerating;
                state.AccelerationDelta = BullMovementDefinitions.AccelerationStep;
                return;

            case BullAction.Accelerating:
                if (state.IsBelow(place.SpawnSettings<BullSpawnSettings>().MaximumSpeed))
                    AccelerateBull(place, state);
                MoveBull(place, state);
                TriggerBullDecelerationIfOffTarget(place, state, samus);
                return;

            case BullAction.Decelerating:
                if (state.HasStopped)
                {
                    state.Action = BullAction.MovementDelay;
                    state.Acceleration = SignedEightEight.Zero;
                    state.AccelerationDelta = SignedEightEight.Zero;
                    state.Speed = SignedEightEight.Zero;
                    return;
                }
                MoveBull(place, state);
                DecelerateBull(place, state);
                return;

            default:
                throw new InvalidDataException(
                    $"Bull has no behaviour for action {state.Action}.");
        }
    }

    /// <summary>Converts the signed displacement to Samus into Bull's bearing, a quarter turn behind the game's zero-up angle.</summary>
    /// <remarks>
    /// The displacement is a <see cref="DistanceAcrossRoomEdge.Apart"/>, the short way to Samus. A Bull flies through terrain and carries its speed past Samus
    /// while it brakes, so one charging at Samus near the room's top or left edge passes above or left of the room's origin
    /// and must still turn back toward her the short way.
    /// </remarks>
    private static RotationAngle CalculateBullAngleToSamus(RoomEnemyPlace place, SamusState samus) =>
        // The Bull measures its heading clockwise from right, the screen's Y pointing down: the aim reversed.
        (TrigonometryAngle.Zero - EnemyMath.CalculateAimAngle(
            new(DistanceAcrossRoomEdge.Apart(samus.Position.X.Whole, place.Position.X.Whole), DistanceAcrossRoomEdge.Apart(samus.Position.Y.Whole, place.Position.Y.Whole)))).ToAngle();

    /// <summary>Increases acceleration and speed only when the configured interval expires.</summary>
    private static void AccelerateBull(RoomEnemyPlace place, BullEnemyState state)
    {
        if (!state.AccelerationIntervalTimer.StepEnds())
            return;

        state.AccelerationIntervalTimer = UpdateCountdown.For(place.SpawnSettings<BullSpawnSettings>().Intervals.Acceleration);
        state.Accelerate();
    }

    /// <summary>Reduces acceleration and speed on the configured deceleration cadence.</summary>
    private static void DecelerateBull(RoomEnemyPlace place, BullEnemyState state)
    {
        if (!state.AccelerationIntervalTimer.StepEnds())
            return;

        state.AccelerationIntervalTimer = UpdateCountdown.For(place.SpawnSettings<BullSpawnSettings>().Intervals.Deceleration);
        state.Decelerate();
    }

    /// <summary>Starts braking when the turn to Samus's bearing reaches <see cref="BullMovementDefinitions.OffTargetTurn"/>.</summary>
    private static void TriggerBullDecelerationIfOffTarget(
        RoomEnemyPlace place,
        BullEnemyState state,
        SamusState samus)
    {
        state.AngleToSamus = CalculateBullAngleToSamus(place, samus);

        // The off-target test measures the turn from the heading to Samus's bearing as the game reckons it
        // (BullOffTargetTurn).
        if (BullOffTargetTurn.Of(state.Angle, state.AngleToSamus.ToTrigonometryAngle()) < BullMovementDefinitions.OffTargetTurn)
            return;

        state.Action = BullAction.Decelerating;
        state.AccelerationDelta = BullMovementDefinitions.AccelerationStep;
    }

    /// <summary>Moves the Bull along both axes by its speed in the direction of its committed angle.</summary>
    private static void MoveBull(RoomEnemyPlace place, BullEnemyState state)
    {
        place.Position = place.Position.WithX(AddBullComponent(
            place.Position.X,
            state.Angle.TurnedBy(TrigonometryAngle.QuarterTurn),
            state.Speed));
        place.Position = place.Position.WithY(AddBullComponent(
            place.Position.Y,
            state.Angle,
            state.Speed));
    }

    /// <summary>Moves one axis of the Bull by its step along the sine of <paramref name="angle"/> (<see cref="BullStep"/>).</summary>
    private static FixedCoordinate AddBullComponent(
        FixedCoordinate position,
        TrigonometryAngle angle,
        SignedEightEight speed) =>
        // A fraction that passes a whole pixel moves the Bull one more whole pixel.
        position.MovedBy(BullStep(UnitComponent.CoarseFineSineOf(angle), speed));

    /// <summary>
    /// How far a Bull flying at <paramref name="speed"/> steps in one update along an axis whose share of its heading is
    /// <paramref name="share"/>, the fine sine read coarsely (<see cref="UnitComponent.CoarseFineSineOf"/>): half the share
    /// of its speed (<see cref="BullMovementDefinitions.StepHalvings"/>) to the 65536th of a pixel, and a step up or
    /// left reversed as <see cref="BullBackwardStep"/> reverses it (#2532).
    /// </summary>
    /// <param name="share">The axis's share of the Bull's heading.</param>
    /// <param name="speed">The Bull's speed, never below zero (<see cref="BullEnemyState.Speed"/>).</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="speed"/> is negative.</exception>
    private static SignedSixteenSixteen BullStep(UnitComponent share, SignedEightEight speed)
    {
        // The share is under one and the speed at most eleven and a quarter pixels, so the step is under six pixels. The
        // step's size is halved before its direction is restored, and a step up or left reverses as BullBackwardStep does.
        if (speed.IsNegative)
            throw new ArgumentOutOfRangeException(nameof(speed), speed, "A Bull's speed is never below zero.");
        SignedSixteenSixteen forward = share.VelocityOf(speed).Magnitude().Halved(BullMovementDefinitions.StepHalvings).AsDisplacement();
        return share.IsNegative ? BullBackwardStep.Of(forward) : forward;
    }

    /// <summary>
    /// Finishes the Bull's shot and bomb reactions when they deal no damage: the hit kicks it away at <paramref name="kickAngle"/>.
    /// </summary>
    private static void ResolveBullImmuneShot(
        RoomEnemyPlace bull,
        BullEnemyState state,
        TrigonometryAngle kickAngle)
    {
        if (state.ShotReactionDisabled)
            return;

        state.Playback = EnemyAnimationPlayback.Start(bull, BullAnimations.Shot);
        state.Angle = kickAngle;
        state.Acceleration = BullMovementDefinitions.ShotKickAcceleration;
        state.Speed = BullMovementDefinitions.ShotKickSpeed;
        state.Action = BullAction.Decelerating;
        state.ShotReactionDisableTimer = UpdateCountdown.For(BullMovementDefinitions.ShotReactionGuardDuration);
    }

    /// <summary>Runs Bull's idle and immune-shot animations (#641).</summary>
    internal static void AdvanceBullAnimation(RoomEnemyPlace place) =>
        EnemyAnimationPlayback.Advance(place, BullAnimations.Segments,
            static (BullEnemyState state) => state.Playback, static (state, playback) => state.Playback = playback,
            static (_, cue) => throw new InvalidOperationException($"A cue-free Bull animation raised {cue}."));

    /// <summary>
    /// Applies Bull's immune-hit recoil after common bomb damage handling: a bomb that leaves the Bull's health alone kicks
    /// it straight up (<see cref="BullMovementDefinitions.BombKickAngle"/>), so a killed Bull has no reaction.
    /// </summary>
    internal EnemyBombReaction BombReaction { get; } =
        new()
        {
            AfterCommon = c =>
            {
                if (c.Enemy.Health == c.HealthBefore)
                    ResolveBullImmuneShot(c.Enemy, c.Enemy.RequireSpeciesState<BullEnemyState>(), BullMovementDefinitions.BombKickAngle);
            },
        };

    /// <summary>
    /// Applies recoil after a frozen or undamaging accepted projectile hit. Only such a hit kicks the Bull, so a killed
    /// Bull has no reaction.
    /// </summary>
    internal EnemyShotReaction ShotReaction { get; } =
        new()
        {
            AfterCommon = c =>
            {
                if (c.Froze || c.Enemy.Health == c.HealthBefore)
                    ResolveBullImmuneShot(c.Enemy, c.Enemy.RequireSpeciesState<BullEnemyState>(), BullMovementDefinitions.ShotAngles.For(c.ProjectileDirection));
            },
        };
}
