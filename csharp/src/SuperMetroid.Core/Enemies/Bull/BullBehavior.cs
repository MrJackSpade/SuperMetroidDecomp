using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Enemies.Bull.Artwork;
using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.Enemies.Bull;

/// <summary>The Bull species owner: its per-room state, AI routines and the interactions it registers.</summary>
internal sealed class BullBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.Bull];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemySlot slot, EnemyCommonParticipation common) => new BullEntity(this, slot, common);

    /// <summary>Each Bull acceleration or deceleration tick changes the acceleration by 24 256ths of a pixel an update.</summary>
    private static readonly SignedEightEight BullAccelerationDelta = SignedEightEight.FromTwoHundredFiftySixthsOfPixel(24);

    /// <summary>The acceleration an immune shot's kick starts the Bull's braking with: a pixel an update.</summary>
    private static readonly SignedEightEight ShotKickAcceleration = SignedEightEight.FromPixels(1);

    /// <summary>The speed an immune shot kicks the Bull away at: six pixels an update.</summary>
    private static readonly SignedEightEight ShotKickSpeed = SignedEightEight.FromPixels(6);
    /// <summary>Bulls wait sixteen enemy frames before retargeting Samus.</summary>
    private const int BullMovementDelayFrames = 16;

    /// <summary>Hands each new Bull the previous Bull in its slot, whose shot-reaction guard it inherits.</summary>
    private readonly SlotInheritance<BullEnemyState> _inheritance;

    /// <summary>Binds the slot-inheritance service Bull setup uses.</summary>
    /// <param name="inheritance">Hands each new Bull the previous Bull in its slot.</param>
    internal BullBehavior(SlotInheritance<BullEnemyState> inheritance) => _inheritance = inheritance;

    /// <summary>Initializes the Bull.</summary>
    internal void InitializeBull(RoomEnemySlot slot)
    {
        BullEnemyState state = _inheritance.Inherit(slot.SlotIndex, static previous => new BullEnemyState(previous));
        slot.SetSpeciesState(state);

        slot.FrameHold = WrappingCountdown.Start(1);
        slot.Timer = WrappingCountdown.Start(0);
        state.Playback = new(BullAnimations.StartingWith(BullProgram.Normal), AnimationPlace.StartOfAnimation);

        BullSpawnSettings settings = slot.SpawnSettings<BullSpawnSettings>();
        BullSpeedIntervals intervals = settings.Intervals;
        state.AccelerationIntervalTimerReset = intervals.Acceleration;
        state.AccelerationIntervalTimer = state.AccelerationIntervalTimerReset;
        state.DecelerationIntervalTimerReset = intervals.Deceleration;
        state.ActivationTimer = BullMovementDelayFrames;
        state.Function = BullEnemyFunction.MovementDelay;
        state.MaxSpeed = SignedEightEight.FromTwoHundredFiftySixthsOfPixel(settings.MaximumSpeed);
    }

    /// <summary>Runs a Bull's per-frame behaviour through all four of its functions.</summary>
    internal static void RunBullMain(RoomEnemySlot slot, BullEnemyState state, SamusState? samus)
    {
        state.ShotReactionDisableTimer = state.ShotReactionDisableTimer.Decrement().Remaining;
        if (state.ShotReactionDisableTimer.IsZero)
        {
            // Once the timer reaches zero the game pins it at one, clearing the guard
            // every subsequent frame. A zero-initialized timer first wraps to -1 instead.
            state.ShotReactionDisableTimer = WrappingCountdown.Start(1);
            state.ShotReactionDisabled = false;
        }

        switch (state.Function)
        {
            case BullEnemyFunction.MovementDelay:
                if (--state.ActivationTimer == 0)
                {
                    state.ActivationTimer = BullMovementDelayFrames;
                    state.Function = BullEnemyFunction.TargetSamus;
                }
                return;

            case BullEnemyFunction.TargetSamus:
                if (samus is null)
                    throw new InvalidOperationException("Bull targeting requires the active Samus actor.");
                state.AngleToSamus = CalculateBullAngleToSamus(slot, samus);
                state.Angle = state.AngleToSamus.ToTrigonometryAngle();
                state.Function = BullEnemyFunction.Accelerating;
                state.AccelerationDelta = BullAccelerationDelta;
                return;

            case BullEnemyFunction.Accelerating:
                if (samus is null)
                    throw new InvalidOperationException("Bull steering requires the active Samus actor.");
                if (state.IsBelowMaximumSpeed)
                    AccelerateBull(state);
                MoveBull(slot, state);
                TriggerBullDecelerationIfOffTarget(slot, state, samus);
                return;

            case BullEnemyFunction.Decelerating:
                if (state.HasStopped)
                {
                    state.Function = BullEnemyFunction.MovementDelay;
                    state.Acceleration = SignedEightEight.Zero;
                    state.AccelerationDelta = SignedEightEight.Zero;
                    state.Speed = SignedEightEight.Zero;
                    return;
                }
                MoveBull(slot, state);
                DecelerateBull(state);
                return;

            default:
                throw new InvalidDataException(
                    $"Bull function {state.Function?.ToString() ?? "none"} is not translated.");
        }
    }

    /// <summary>Converts the signed displacement to Samus into Bull's bearing, a quarter turn behind the game's zero-up angle.</summary>
    private static RotationAngle CalculateBullAngleToSamus(RoomEnemySlot slot, SamusState samus) =>
        EnemyMath.CalculateBearing(
            new(CoordinateAxis.Difference(samus.Position.X.Whole, slot.Position.X.Whole), CoordinateAxis.Difference(samus.Position.Y.Whole, slot.Position.Y.Whole)))
            .ToMirroredTableAngle().TurnedBySteps(-EnemyTrigonometryAngleDefinitions.QuarterTurn).ToAngle();

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
        RoomEnemySlot slot,
        BullEnemyState state,
        SamusState samus)
    {
        state.AngleToSamus = CalculateBullAngleToSamus(slot, samus);

        // The off-target test measures the turn from the heading to Samus's bearing as the game reckons it
        // (BullOffTargetTurn). A turn of 48 of the angle's 256ths or more is off target.
        if (BullOffTargetTurn.Of(state.Angle, state.AngleToSamus.ToTrigonometryAngle()) < 48)
            return;

        state.Function = BullEnemyFunction.Decelerating;
        state.AccelerationDelta = BullAccelerationDelta;
    }

    /// <summary>Integrates both 16.16 position components from the committed angle and 8.8 speed.</summary>
    internal static void MoveBull(RoomEnemySlot slot, BullEnemyState state)
    {
        slot.Position = slot.Position.WithX(AddBullComponent(
            slot.Position.X,
            ReadBullSignedSine(state.Angle.TurnedBy(TrigonometryAngle.QuarterTurn)),
            state.Speed));
        slot.Position = slot.Position.WithY(AddBullComponent(
            slot.Position.Y,
            ReadBullSignedSine(state.Angle),
            state.Speed));
    }

    /// <summary>Reads the game's signed sine sample for one byte-sized Bull heading.</summary>
    internal static int ReadBullSignedSine(TrigonometryAngle angle) =>
        Trigonometry.SignedSixteenBitSine(angle);

    /// <summary>
    /// Moves one axis of the Bull by its speed along the sine: the sine's magnitude in whole 256ths (its finer part
    /// dropped) times the speed in 256ths of a pixel, read unsigned as the game's multiply reads it, a step up or left
    /// reversed as <see cref="BullBackwardStep"/> reverses it.
    /// </summary>
    private static FixedCoordinate AddBullComponent(
        FixedCoordinate position,
        int sine,
        SignedEightEight speed)
    {
        // The sine's magnitude is below 128 256ths and the speed below 65536 256ths, so the step fits a 16.16 displacement.
        int sineTwoFiftySixths = UnsignedEightEight.Whole(Math.Abs(sine));
        SignedSixteenSixteen forward = UnsignedSixteenSixteen.ProductOfTwoFiftySixths(
            sineTwoFiftySixths, speed.UnsignedTwoHundredFiftySixthsOfPixel).AsDisplacement();
        SignedSixteenSixteen displacement = sine < 0 ? BullBackwardStep.Of(forward) : forward;

        // The displacement moves the position as one 16.16 value: a fraction that passes a whole pixel moves the whole
        // position by one more, and the whole position wraps at its width.
        return position.MovedBy(displacement);
    }

    /// <summary>Runs the no-damage tail of the Bull's shot reaction.</summary>
    internal static void ResolveBullImmuneShot(
        RoomEnemySlot bull,
        BullEnemyState state,
        SamusProjectileDirection projectileDirection)
    {
        if (state.ShotReactionDisabled)
            return;

        bull.FrameHold = WrappingCountdown.Start(1);
        bull.Timer = WrappingCountdown.Start(0);
        state.Playback = new(BullAnimations.StartingWith(BullProgram.Shot), AnimationPlace.StartOfAnimation);
        state.Angle = BullMotion.ShotAngle(projectileDirection);
        state.Acceleration = ShotKickAcceleration;
        state.Speed = ShotKickSpeed;
        state.Function = BullEnemyFunction.Decelerating;
        state.ShotReactionDisableTimer = WrappingCountdown.Start(48);
        state.ShotReactionDisabled = true;
    }

    /// <summary>Runs Bull's normal and immune-shot animations as typed programs (#641).</summary>
    internal static void AdvanceBullAnimation(RoomEnemySlot slot, EnemyInstructionContext _) =>
        EnemyProgramAnimation.Advance(slot, BullAnimations.Segments, BullFrameDefinitions.ShownFrames.Of,
            static (BullEnemyState state) => state.Playback, static (state, playback) => state.Playback = playback,
            static (_, cue) => throw new InvalidOperationException($"A cue-free Bull animation raised {cue}."));

    /// <summary>
    /// Applies Bull's immune-hit recoil after common bomb damage handling. Only a hit that leaves the Bull's health alone
    /// kicks it, so a killed Bull has no reaction.
    /// </summary>
    internal EnemyBombReaction BombReaction { get; } =
        new(c => c.Enemy.DefinitionId == EnemyDefinitionId.Bull && c.SelectedShotAi == EnemyReaction.BullShot)
        {
            AfterCommon = c =>
            {
                if (c.Enemy.Health == c.HealthBefore)
                    ResolveBullImmuneShot(c.Enemy, c.Enemy.RequireSpeciesState<BullEnemyState>(), SamusBombProjectileSlot.Direction);
            },
        };

    /// <summary>
    /// Applies recoil after a frozen or undamaging accepted projectile hit. Only such a hit kicks the Bull, so a killed
    /// Bull has no reaction.
    /// </summary>
    internal EnemyShotReaction ShotReaction { get; } =
        new(enemy => enemy.DefinitionId == EnemyDefinitionId.Bull &&
                     EnemyCollision.ShotAi(enemy) == EnemyReaction.BullShot)
        {
            AfterCommon = c =>
            {
                if (c.Froze || c.Enemy.Health == c.HealthBefore)
                    ResolveBullImmuneShot(c.Enemy, c.Enemy.RequireSpeciesState<BullEnemyState>(), c.ProjectileDirection);
            },
        };
}
