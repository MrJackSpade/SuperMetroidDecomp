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

    /// <summary>Each Bull acceleration or deceleration tick changes the 8.8 rate by 24.</summary>
    private const int BullAccelerationDelta = 24;
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
        state.Program = AnimationProgramCursor<BullProgram>.Start(BullProgram.Normal);

        BullSpawnSettings settings = slot.SpawnSettings<BullSpawnSettings>();
        BullSpeedIntervals intervals = settings.Intervals;
        state.AccelerationIntervalTimerReset = intervals.Acceleration;
        state.AccelerationIntervalTimer = state.AccelerationIntervalTimerReset;
        state.DecelerationIntervalTimerReset = intervals.Deceleration;
        state.ActivationTimer = BullMovementDelayFrames;
        state.Function = BullEnemyFunction.MovementDelay;
        state.MaxSpeed = settings.MaximumSpeed;
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
                state.Angle = state.AngleToSamus.TableIndex;
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
                    state.Acceleration = 0;
                    state.AccelerationDelta = 0;
                    state.Speed = 0;
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
        RotationAngle.NormalizeTableIndex(EnemyMath.CalculateBearing(
            new(CoordinateAxis.Difference(samus.Position.X.Whole, slot.Position.X.Whole), CoordinateAxis.Difference(samus.Position.Y.Whole, slot.Position.Y.Whole))).ToMirroredTableAngle().RawValue - EnemyByteAngleDefinitions.QuarterTurn);

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

        // The off-target test measures the turn from the heading to Samus's bearing. A turn of a half turn or more
        // the positive way counts the short way round, but a turn of more than a half turn the negative way counts the
        // long way: the game's sign extension leaves that difference large. A turn of 48 units or more is off target.
        int turn = state.AngleToSamus.TableIndex - state.Angle;
        int measuredTurn = turn >= ByteAngle.HalfTurn.RawValue ? RotationAngle.TableUnitsPerTurn - turn : Math.Abs(turn);
        if (measuredTurn < 48)
            return;

        state.Function = BullEnemyFunction.Decelerating;
        state.AccelerationDelta = BullAccelerationDelta;
    }

    /// <summary>Integrates both 16.16 position components from the committed angle and 8.8 speed.</summary>
    internal static void MoveBull(RoomEnemySlot slot, BullEnemyState state)
    {
        slot.Position = slot.Position.WithX(AddBullComponent(
            slot.Position.X,
            ReadBullSignedSine(ByteAngle.Wrapped(state.Angle + EnemyByteAngleDefinitions.QuarterTurn)),
            state.Speed));
        slot.Position = slot.Position.WithY(AddBullComponent(
            slot.Position.Y,
            ReadBullSignedSine(new ByteAngle(state.Angle)),
            state.Speed));
    }

    /// <summary>Reads the game's signed sine sample for one byte-sized Bull heading.</summary>
    internal static int ReadBullSignedSine(ByteAngle angle) =>
        Trigonometry.SignedSixteenBitSine(angle);

    /// <summary>Applies Bull's 24-bit sine-times-speed displacement and negative-fraction rule.</summary>
    private static FixedCoordinate AddBullComponent(
        FixedCoordinate position,
        int sine,
        int speed)
    {
        // Bull discards the sine table's low byte, multiplies the absolute high byte by its
        // unsigned 8.8 speed, and keeps the 24-bit result as a 16.16 displacement. Negative
        // components use the Bull's own negation, whose zero-fraction path is deliberately one subpixel more
        // negative than a conventional two's-complement negation.
        // The high byte is below 128 and the speed below 65536, so the product fits a 16.16 displacement.
        int sineHighByte = UnsignedEightEight.Whole(Math.Abs(sine));
        SignedSixteenSixteen displacement = SignedSixteenSixteen.FromRaw(sineHighByte * speed);
        if (sine < 0 && displacement != SignedSixteenSixteen.Zero)
        {
            // A displacement with no fraction reverses one subpixel further than its true reversal.
            displacement = SignedSixteenSixteen.Zero - displacement -
                SignedSixteenSixteen.FromRaw(displacement.Fraction == 0 ? 1 : 0);
        }

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
        state.Program = AnimationProgramCursor<BullProgram>.Start(BullProgram.Shot);
        int direction = projectileDirection.ToAimIndex();
        state.Angle = BullMotion.ShotAngle(direction);
        state.Acceleration = 256;
        state.Speed = 1536;
        state.Function = BullEnemyFunction.Decelerating;
        state.ShotReactionDisableTimer = WrappingCountdown.Start(48);
        state.ShotReactionDisabled = true;
    }

    /// <summary>Runs Bull's normal and immune-shot animations as typed programs (#641).</summary>
    internal static void AdvanceBullAnimation(RoomEnemySlot slot, EnemyInstructionContext _) =>
        EnemyProgramAnimation.Advance(slot, BullAnimations.Programs, BullFrameDefinitions.ShownFrames.Of,
            static (BullEnemyState state) => state.Program, static (state, cursor) => state.Program = cursor);

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
