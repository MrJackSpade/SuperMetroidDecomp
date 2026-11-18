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

        state.Playback = EnemyAnimationPlayback.Start(place, BullAnimations.StartingWith(BullSegment.Normal));

        BullSpawnSettings settings = place.SpawnSettings<BullSpawnSettings>();
        state.AccelerationIntervalTimer = settings.Intervals.Acceleration;
        state.ActivationTimer = BullMovementDefinitions.MovementDelay;
    }

    /// <summary>Runs a Bull for one update through all four of its actions.</summary>
    internal static void RunBullMain(RoomEnemyPlace place, BullEnemyState state, SamusState samus)
    {
        // A kick's guard runs out on the update its countdown reaches zero.
        if (state.ShotReactionDisableTimer > 0)
            state.ShotReactionDisableTimer--;

        switch (state.Action)
        {
            case BullAction.MovementDelay:
                if (--state.ActivationTimer == 0)
                {
                    state.ActivationTimer = BullMovementDefinitions.MovementDelay;
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
    /// The displacement is a <see cref="CoordinateAxis.Difference"/>, the short way to Samus. A Bull flies through terrain and carries its speed past Samus
    /// while it brakes, so one charging at Samus near the room's top or left edge passes above or left of the room's origin
    /// and must still turn back toward her the short way.
    /// </remarks>
    private static RotationAngle CalculateBullAngleToSamus(RoomEnemyPlace place, SamusState samus) =>
        (EnemyMath.CalculateBearing(
            new(CoordinateAxis.Difference(samus.Position.X.Whole, place.Position.X.Whole), CoordinateAxis.Difference(samus.Position.Y.Whole, place.Position.Y.Whole)))
            .ToMirroredTrigonometryAngle() - TrigonometryAngle.QuarterTurn).ToAngle();

    /// <summary>Increases acceleration and speed only when the configured interval expires.</summary>
    private static void AccelerateBull(RoomEnemyPlace place, BullEnemyState state)
    {
        if (--state.AccelerationIntervalTimer != 0)
            return;

        state.AccelerationIntervalTimer = place.SpawnSettings<BullSpawnSettings>().Intervals.Acceleration;
        state.Accelerate();
    }

    /// <summary>Reduces acceleration and speed on the configured deceleration cadence.</summary>
    private static void DecelerateBull(RoomEnemyPlace place, BullEnemyState state)
    {
        if (--state.AccelerationIntervalTimer != 0)
            return;

        state.AccelerationIntervalTimer = place.SpawnSettings<BullSpawnSettings>().Intervals.Deceleration;
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
    internal static void MoveBull(RoomEnemyPlace place, BullEnemyState state)
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

    /// <summary>The game's signed fine sine of a Bull's heading, whose sign says which way the Bull steps along an axis.</summary>
    internal static int ReadBullSignedSine(TrigonometryAngle angle) =>
        Trigonometry.FineSignedSine(angle);

    /// <summary>
    /// Moves one axis of the Bull by its speed along the sine of <paramref name="angle"/>: the fine sine's coarse size
    /// (<see cref="Trigonometry.CoarseSizeOfFineSine"/>) taken as 256ths times the speed in 256ths of a pixel, never below
    /// zero (<see cref="BullEnemyState.Speed"/>), a step up or left reversed as <see cref="BullBackwardStep"/> reverses it.
    /// </summary>
    private static FixedCoordinate AddBullComponent(
        FixedCoordinate position,
        TrigonometryAngle angle,
        SignedEightEight speed)
    {
        // The sine's coarse size is below 128 and the speed at most 2880 256ths, so the step is under six pixels.
        SignedSixteenSixteen forward = UnsignedSixteenSixteen.ProductOfTwoFiftySixths(
            Trigonometry.CoarseSizeOfFineSine(angle), speed.InTwoHundredFiftySixthsOfPixel).AsDisplacement();
        SignedSixteenSixteen displacement = ReadBullSignedSine(angle) < 0 ? BullBackwardStep.Of(forward) : forward;

        // A fraction that passes a whole pixel moves the Bull one more whole pixel.
        return position.MovedBy(displacement);
    }

    /// <summary>
    /// Finishes the Bull's shot and bomb reactions when they deal no damage: the hit kicks it away at <paramref name="kickAngle"/>.
    /// </summary>
    internal static void ResolveBullImmuneShot(
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
        state.ShotReactionDisableTimer = BullMovementDefinitions.ShotReactionGuardDuration;
    }

    /// <summary>Runs Bull's normal and immune-shot animations (#641).</summary>
    internal static void AdvanceBullAnimation(RoomEnemyPlace place, EnemyAnimationCueContext _) =>
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
                    ResolveBullImmuneShot(c.Enemy, c.Enemy.RequireSpeciesState<BullEnemyState>(), BullMotion.ShotAngle(c.ProjectileDirection));
            },
        };
}
