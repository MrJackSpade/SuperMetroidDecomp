using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.Enemies.Bull;

/// <summary>The Bull species owner: its per-room state, behaviour and the interactions it registers.</summary>
internal sealed class BullBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.Bull];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemyPlace place, EnemyCommonParticipation common) => new BullEntity(this, place, common);

    /// <summary>Each Bull acceleration or deceleration tick changes the acceleration by 24 256ths of a pixel an update.</summary>
    private static readonly SignedEightEight BullAccelerationDelta = SignedEightEight.FromTwoHundredFiftySixthsOfPixel(24);

    /// <summary>The acceleration an immune shot's kick starts the Bull's braking with: a pixel an update.</summary>
    private static readonly SignedEightEight ShotKickAcceleration = SignedEightEight.FromPixels(1);

    /// <summary>The speed an immune shot kicks the Bull away at: six pixels an update.</summary>
    private static readonly SignedEightEight ShotKickSpeed = SignedEightEight.FromPixels(6);
    /// <summary>Bulls wait sixteen enemy frames before retargeting Samus.</summary>
    private const int BullMovementDelayFrames = 16;

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
        BullSpeedIntervals intervals = settings.Intervals;
        state.AccelerationIntervalTimerReset = intervals.Acceleration;
        state.AccelerationIntervalTimer = state.AccelerationIntervalTimerReset;
        state.DecelerationIntervalTimerReset = intervals.Deceleration;
        state.ActivationTimer = BullMovementDelayFrames;
        state.MaxSpeed = settings.MaximumSpeed;
    }

    /// <summary>Runs a Bull's per-frame behaviour through all four of its actions.</summary>
    internal static void RunBullMain(RoomEnemyPlace place, BullEnemyState state, SamusState samus)
    {
        state.ShotReactionDisableTimer = state.ShotReactionDisableTimer.Decrement().Remaining;
        if (state.ShotReactionDisableTimer.IsZero)
        {
            // Once the timer reaches zero the game pins it at one, clearing the guard
            // every subsequent frame. A zero-initialized timer first wraps to -1 instead.
            state.ShotReactionDisableTimer = WrappingCountdown.Start(1);
            state.ShotReactionDisabled = false;
        }

        switch (state.Action)
        {
            case BullAction.MovementDelay:
                if (--state.ActivationTimer == 0)
                {
                    state.ActivationTimer = BullMovementDelayFrames;
                    state.Action = BullAction.TargetSamus;
                }
                return;

            case BullAction.TargetSamus:
                state.AngleToSamus = CalculateBullAngleToSamus(place, samus);
                state.Angle = state.AngleToSamus.ToTrigonometryAngle();
                state.Action = BullAction.Accelerating;
                state.AccelerationDelta = BullAccelerationDelta;
                return;

            case BullAction.Accelerating:
                if (state.IsBelowMaximumSpeed)
                    AccelerateBull(state);
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
                DecelerateBull(state);
                return;

            default:
                throw new InvalidDataException(
                    $"Bull has no behaviour for action {state.Action}.");
        }
    }

    /// <summary>Converts the signed displacement to Samus into Bull's bearing, a quarter turn behind the game's zero-up angle.</summary>
    private static RotationAngle CalculateBullAngleToSamus(RoomEnemyPlace place, SamusState samus) =>
        EnemyMath.CalculateBearing(
            new(CoordinateAxis.Difference(samus.Position.X.Whole, place.Position.X.Whole), CoordinateAxis.Difference(samus.Position.Y.Whole, place.Position.Y.Whole)))
            .ToMirroredTrigonometryAngle().TurnedBySteps(-EnemyTrigonometryAngleDefinitions.QuarterTurn).ToAngle();

    /// <summary>Increases acceleration and speed only when the configured interval expires.</summary>
    private static void AccelerateBull(BullEnemyState state)
    {
        if (--state.AccelerationIntervalTimer != 0)
            return;

        state.AccelerationIntervalTimer = state.AccelerationIntervalTimerReset;
        state.Accelerate();
    }

    /// <summary>Reduces acceleration and speed on the configured deceleration cadence.</summary>
    private static void DecelerateBull(BullEnemyState state)
    {
        if (--state.AccelerationIntervalTimer != 0)
            return;

        state.AccelerationIntervalTimer = state.DecelerationIntervalTimerReset;
        state.Decelerate();
    }

    /// <summary>Starts braking when the signed angle difference reaches 48.</summary>
    private static void TriggerBullDecelerationIfOffTarget(
        RoomEnemyPlace place,
        BullEnemyState state,
        SamusState samus)
    {
        state.AngleToSamus = CalculateBullAngleToSamus(place, samus);

        // The off-target test measures the turn from the heading to Samus's bearing as the game reckons it
        // (BullOffTargetTurn). A turn of 48 of the angle's 256ths or more is off target.
        if (BullOffTargetTurn.Of(state.Angle, state.AngleToSamus.ToTrigonometryAngle()) < 48)
            return;

        state.Action = BullAction.Decelerating;
        state.AccelerationDelta = BullAccelerationDelta;
    }

    /// <summary>Integrates both 1/65536-pixel position components from the committed angle and 1/256-pixel speed.</summary>
    internal static void MoveBull(RoomEnemyPlace place, BullEnemyState state)
    {
        place.Position = place.Position.WithX(AddBullComponent(
            place.Position.X,
            ReadBullSignedSine(state.Angle.TurnedBy(TrigonometryAngle.QuarterTurn)),
            state.Speed));
        place.Position = place.Position.WithY(AddBullComponent(
            place.Position.Y,
            ReadBullSignedSine(state.Angle),
            state.Speed));
    }

    /// <summary>Reads the game's signed sine sample for one Bull heading, in 256ths of a turn.</summary>
    internal static int ReadBullSignedSine(TrigonometryAngle angle) =>
        Trigonometry.FineSignedSine(angle);

    /// <summary>
    /// Moves one axis of the Bull by its speed along the sine: the sine's magnitude in whole 256ths (its finer part
    /// dropped) times the speed in 256ths of a pixel, never below zero (<see cref="BullEnemyState.Speed"/>), a step up or
    /// left reversed as <see cref="BullBackwardStep"/> reverses it.
    /// </summary>
    private static FixedCoordinate AddBullComponent(
        FixedCoordinate position,
        int sine,
        SignedEightEight speed)
    {
        // The sine's magnitude is below 128 256ths and the speed at most 2880 256ths, so the step fits a 1/65536-pixel displacement.
        int sineTwoFiftySixths = UnsignedEightEight.Whole(Math.Abs(sine));
        SignedSixteenSixteen forward = UnsignedSixteenSixteen.ProductOfTwoFiftySixths(
            sineTwoFiftySixths, speed.InTwoHundredFiftySixthsOfPixel).AsDisplacement();
        SignedSixteenSixteen displacement = sine < 0 ? BullBackwardStep.Of(forward) : forward;

        // The displacement moves the position as one 1/65536-pixel value: a fraction that passes a whole pixel moves the whole
        // position by one more, and the whole position wraps at its width.
        return position.MovedBy(displacement);
    }

    /// <summary>Runs the no-damage tail of the Bull's shot reaction.</summary>
    internal static void ResolveBullImmuneShot(
        RoomEnemyPlace bull,
        BullEnemyState state,
        SamusProjectileDirection projectileDirection)
    {
        if (state.ShotReactionDisabled)
            return;

        state.Playback = EnemyAnimationPlayback.Start(bull, BullAnimations.Shot);
        state.Angle = BullMotion.ShotAngle(projectileDirection);
        state.Acceleration = ShotKickAcceleration;
        state.Speed = ShotKickSpeed;
        state.Action = BullAction.Decelerating;
        state.ShotReactionDisableTimer = WrappingCountdown.Start(48);
        state.ShotReactionDisabled = true;
    }

    /// <summary>Runs Bull's normal and immune-shot animations (#641).</summary>
    internal static void AdvanceBullAnimation(RoomEnemyPlace place, EnemyAnimationCueContext _) =>
        EnemyAnimationPlayback.Advance(place, BullAnimations.Segments,
            static (BullEnemyState state) => state.Playback, static (state, playback) => state.Playback = playback,
            static (_, cue) => throw new InvalidOperationException($"A cue-free Bull animation raised {cue}."));

    /// <summary>
    /// Applies Bull's immune-hit recoil after common bomb damage handling. Only a hit that leaves the Bull's health alone
    /// kicks it, so a killed Bull has no reaction.
    /// </summary>
    internal EnemyBombReaction BombReaction { get; } =
        new()
        {
            AfterCommon = c =>
            {
                if (c.Enemy.Health == c.HealthBefore)
                    ResolveBullImmuneShot(c.Enemy, c.Enemy.RequireSpeciesState<BullEnemyState>(), SamusBomb.Direction);
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
                    ResolveBullImmuneShot(c.Enemy, c.Enemy.RequireSpeciesState<BullEnemyState>(), c.ProjectileDirection);
            },
        };
}
