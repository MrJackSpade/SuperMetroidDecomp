using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Rooms;
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

    /// <summary>Each Bull acceleration or deceleration tick changes the 8.8 rate by $0018.</summary>
    private const ushort BullAccelerationDelta = 24;
    /// <summary>Bulls wait sixteen enemy frames before retargeting Samus.</summary>
    private const ushort BullMovementDelayFrames = 16;

    /// <summary>The words each physical slot keeps across Bull occupants for the current room.</summary>
    private readonly BullExtensionWords[] _extensions = NewExtensions();

    /// <summary>Fresh, zeroed extension words for every physical slot.</summary>
    private static BullExtensionWords[] NewExtensions() =>
        [.. Enumerable.Range(0, EnemySlotOrder.Capacity).Select(_ => new BullExtensionWords())];

    /// <summary>Ports <c>InitAI_Bull</c> at <c>$A8:D8C9</c>.</summary>
    internal void InitializeBull(RoomEnemySlot slot)
    {
        var state = new BullEnemyState(_extensions[slot.SlotIndex]);
        slot.SetSpeciesState(state, state.ClearWithRecord);

        slot.InstructionTimer = WrappingCountdown.Start(1);
        slot.Timer = WrappingCountdown.Start(0);
        state.Program = AnimationProgramCursor<BullProgram>.Start(BullProgram.Normal);

        // These are definition selectors, not live timers or editable presentation data.
        // Unsupported debug-edited selectors fail explicitly instead of reading adjacent code.
        var intervals = BullMotion.Intervals(checked((ushort)slot.Spawn.Population.Parameter1));
        state.AccelerationIntervalTimerReset = intervals.Acceleration;
        state.AccelerationIntervalTimer = WrappingCountdown.Start(state.AccelerationIntervalTimerReset);
        state.DecelerationIntervalTimerReset = intervals.Deceleration;
        state.ActivationTimer = WrappingCountdown.Start(BullMovementDelayFrames);
        state.Function = BullEnemyFunction.MovementDelay;
        state.MaxSpeed = BullMotion.MaximumSpeed(checked((ushort)slot.Spawn.Population.Parameter2));
    }

    /// <summary>Ports <c>MainAI_Bull</c> and all four indirect function targets.</summary>
    internal static void RunBullMain(RoomEnemySlot slot, BullEnemyState state, SamusState? samus)
    {
        state.ShotReactionDisableTimer = state.ShotReactionDisableTimer.Decrement().Remaining;
        if (state.ShotReactionDisableTimer.IsZero)
        {
            // Once the timer reaches zero the cartridge pins it at one, clearing the guard
            // every subsequent frame. A zero-initialized timer first wraps to $FFFF instead.
            state.ShotReactionDisableTimer = WrappingCountdown.Start(1);
            state.ShotReactionDisabled = false;
        }

        switch (state.Function)
        {
            case BullEnemyFunction.MovementDelay:
                state.ActivationTimer = state.ActivationTimer.Decrement().Remaining;
                if (state.ActivationTimer.IsZero)
                {
                    state.ActivationTimer = WrappingCountdown.Start(BullMovementDelayFrames);
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

    /// <summary>Converts the signed displacement to Samus into Bull's bearing, a quarter turn behind the cartridge's zero-up angle.</summary>
    private static RotationAngle CalculateBullAngleToSamus(RoomEnemySlot slot, SamusState samus) =>
        RotationAngle.NormalizeTableIndex(EnemyMath.CalculateBearing(
            new(unchecked((short)(samus.Position.X.Whole - slot.Position.X.Whole)), unchecked((short)(samus.Position.Y.Whole - slot.Position.Y.Whole)))).ReinterpretAsTableAngle().RawValue - EnemyByteAngleDefinitions.QuarterTurn);

    /// <summary>Increases acceleration and speed only when the configured interval expires.</summary>
    private static void AccelerateBull(BullEnemyState state)
    {
        state.AccelerationIntervalTimer = state.AccelerationIntervalTimer.Decrement().Remaining;
        if (!state.AccelerationIntervalTimer.IsZero)
            return;

        state.AccelerationIntervalTimer = WrappingCountdown.Start(state.AccelerationIntervalTimerReset);
        state.Accelerate();
    }

    /// <summary>Reduces acceleration and speed on the configured deceleration cadence.</summary>
    private static void DecelerateBull(BullEnemyState state)
    {
        state.AccelerationIntervalTimer = state.AccelerationIntervalTimer.Decrement().Remaining;
        if (!state.AccelerationIntervalTimer.IsZero)
            return;

        state.AccelerationIntervalTimer = WrappingCountdown.Start(state.DecelerationIntervalTimerReset);
        state.Decelerate();
    }

    /// <summary>Starts braking when the native signed angle difference reaches $30.</summary>
    private static void TriggerBullDecelerationIfOffTarget(
        RoomEnemySlot slot,
        BullEnemyState state,
        SamusState samus)
    {
        state.AngleToSamus = CalculateBullAngleToSamus(slot, samus);

        // $A8:D9BC subtracts the two byte angles as 16-bit words. Sign_Extend_A ($A0:AFEA)
        // only ORs $FF00 when bit seven is set and never clears a borrowed high byte, so a
        // negative difference with bit seven clear (e.g. $FF10) stays large. NegateA then
        // takes the absolute value and CMP/BMI tests it against $30.
        int difference = unchecked((ushort)(state.AngleToSamus.TableIndex - state.Angle));
        difference = EnemyMath.SignExtendLowByte(difference);
        if (EnemyMath.IsNegative16(difference))
            difference = unchecked((ushort)-difference);
        if (unchecked((short)(difference - 48)) < 0)
            return;

        state.Function = BullEnemyFunction.Decelerating;
        state.AccelerationDelta = BullAccelerationDelta;
    }

    /// <summary>Integrates both 16.16 position components from the committed angle and 8.8 speed.</summary>
    internal static void MoveBull(RoomEnemySlot slot, BullEnemyState state)
    {
        slot.Position = slot.Position.WithX(AddBullComponent(
            slot.Position.X,
            ReadBullSignedSine(unchecked(new ByteAngle((byte)(state.Angle + EnemyByteAngleDefinitions.QuarterTurn)))),
            state.Speed));
        slot.Position = slot.Position.WithY(AddBullComponent(
            slot.Position.Y,
            ReadBullSignedSine(unchecked(new ByteAngle((byte)state.Angle))),
            state.Speed));
    }

    /// <summary>Reads the cartridge's signed sine sample for one byte-sized Bull heading.</summary>
    internal static short ReadBullSignedSine(ByteAngle angle) =>
        Trigonometry.SignedSixteenBitSine(angle);

    /// <summary>Applies Bull's 24-bit sine-times-speed displacement and native negative-fraction rule.</summary>
    private static FixedCoordinate AddBullComponent(
        FixedCoordinate position,
        int sine,
        int speed)
    {
        // Bull discards the sine table's low byte, multiplies the absolute high byte by its
        // unsigned 8.8 speed, and keeps the 24-bit result as a 16.16 displacement. Negative
        // components use $A8:DAF6, whose zero-fraction path is deliberately one subpixel more
        // negative than a conventional two's-complement negation.
        int magnitude = unchecked((ushort)Math.Abs(sine));
        int sineHighByte = UnsignedEightEight.Whole(magnitude);
        uint product = (uint)sineHighByte * (uint)speed;
        int fraction = unchecked((ushort)product);
        int whole = FixedCoordinate.WholeOf(unchecked((int)product));
        if (sine < 0 && (whole != 0 || fraction != 0))
        {
            if (fraction == 0)
                fraction = RoomPixelMasks.LastFraction;
            else
                fraction = unchecked((ushort)-fraction);
            whole = unchecked((ushort)~whole);
        }

        // Both fractions are words, so their sum fits an int and its carry is the bit above the word.
        int fractionalSum = position.Fraction + fraction;
        return new(
            unchecked((ushort)(position.Whole + whole + (WordCarry.OutOfWord(fractionalSum)))),
            unchecked((ushort)fractionalSum));
    }

    /// <summary>Ports the no-damage tail of <c>EnemyShot_Bull</c> at $A8:DB2C.</summary>
    internal static void ResolveBullImmuneShot(
        RoomEnemySlot bull,
        BullEnemyState state,
        SamusProjectileDirection projectileDirection)
    {
        if (state.ShotReactionDisabled)
            return;

        bull.InstructionTimer = WrappingCountdown.Start(1);
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

    /// <summary>Applies Bull's immune-hit recoil after common bomb damage handling.</summary>
    internal EnemyBombReaction BombReaction { get; } =
        new(c => c.Enemy.DefinitionId == EnemyDefinitionId.Bull && c.SelectedShotAi == EnemyInteractionCallback.BullShot)
        {
            AfterCommon = c =>
            {
                BullEnemyState bullState = c.Enemy.RequireSpeciesState<BullEnemyState>();
                bullState.PreviousHealth = c.HealthBefore;
                if (c.Enemy.Health == c.HealthBefore)
                    ResolveBullImmuneShot(c.Enemy, bullState, SamusBombProjectileSlot.Direction);
            },
        };

    /// <summary>Applies recoil after a frozen or undamaging accepted projectile hit.</summary>
    internal EnemyShotReaction ShotReaction { get; } =
        new(enemy => enemy.DefinitionId == EnemyDefinitionId.Bull &&
                     EnemyCollision.ShotAi(enemy) == EnemyInteractionCallback.BullShot)
        {
            AfterCommon = c =>
            {
                BullEnemyState bullState = c.Enemy.RequireSpeciesState<BullEnemyState>();
                bullState.PreviousHealth = c.HealthBefore;
                if (c.Froze || c.Enemy.Health == c.HealthBefore)
                    ResolveBullImmuneShot(c.Enemy, bullState, c.ProjectileDirection);
            },
        };
}
