using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Enemies.Bull.Artwork;
using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.Enemies.Bull;

/// <summary>The Bull species owner: its per-room state, AI routines and the interactions it registers.</summary>
internal sealed class BullBehavior : IEnemySpecies
{

    /// <summary>Each Bull acceleration or deceleration tick changes the 8.8 rate by $0018.</summary>
    private const ushort BullAccelerationDelta = 0x0018;
    /// <summary>Bulls wait sixteen enemy frames before retargeting Samus.</summary>
    private const ushort BullMovementDelayFrames = 0x0010;

    /// <summary>Per-slot AI state for Bulls in the current room.</summary>
    internal readonly BullEnemyState?[] _bullStates = new BullEnemyState?[RoomEnemySystem.MaximumEnemyCount];
    /// <summary>Population-selected terminal speed for each Bull slot.</summary>
    internal readonly ushort[] _bullMaxSpeeds = new ushort[RoomEnemySystem.MaximumEnemyCount];
    /// <summary>Latest target bearing toward Samus, retained separately from the flight angle.</summary>
    internal readonly ushort[] _bullAnglesToSamus = new ushort[RoomEnemySystem.MaximumEnemyCount];
    /// <summary>Committed travel bearing used while integrating a Bull's position.</summary>
    internal readonly ushort[] _bullAngles = new ushort[RoomEnemySystem.MaximumEnemyCount];
    /// <summary>Suppresses repeated immune-shot knockback during its reaction window.</summary>
    internal readonly ushort[] _bullShotReactionDisableFlags = new ushort[RoomEnemySystem.MaximumEnemyCount];
    /// <summary>Population-selected interval between acceleration steps.</summary>
    internal readonly ushort[] _bullAccelerationTimerResets = new ushort[RoomEnemySystem.MaximumEnemyCount];
    /// <summary>Population-selected interval between deceleration steps.</summary>
    internal readonly ushort[] _bullDecelerationTimerResets = new ushort[RoomEnemySystem.MaximumEnemyCount];
    /// <summary>Counts down the post-hit period before Bull shot reactions may run again.</summary>
    internal readonly ushort[] _bullShotReactionDisableTimers = new ushort[RoomEnemySystem.MaximumEnemyCount];
    /// <summary>Health captured before common hit processing for immune-hit detection.</summary>
    internal readonly ushort[] _bullPreviousHealth = new ushort[RoomEnemySystem.MaximumEnemyCount];

    /// <summary>Ports <c>InitAI_Bull</c> at <c>$A8:D8C9</c>.</summary>
    internal void InitializeBull(RoomEnemySlot slot)
    {
        var state = new BullEnemyState(slot, new BullSharedTables { MaxSpeeds = _bullMaxSpeeds, AnglesToSamus = _bullAnglesToSamus, Angles = _bullAngles, ShotReactionDisableFlags = _bullShotReactionDisableFlags, AccelerationTimerResets = _bullAccelerationTimerResets, DecelerationTimerResets = _bullDecelerationTimerResets, ShotReactionDisableTimers = _bullShotReactionDisableTimers, PreviousHealth = _bullPreviousHealth });
        _bullStates[slot.SlotIndex] = state;

        slot.InstructionTimer = NativeCountdown.Start(1);
        slot.Timer = NativeCountdown.Start(0);
        state.Program = AnimationProgramCursor<BullProgram>.Start(BullProgram.Normal);

        // These are definition selectors, not live timers or editable presentation data.
        // Unsupported debug-edited selectors fail explicitly instead of reading adjacent code.
        var intervals = BullMovementDefinitions.Intervals(slot.Parameter1);
        state.AccelerationIntervalTimerReset = intervals.Acceleration;
        state.AccelerationIntervalTimer = NativeCountdown.FromRaw(state.AccelerationIntervalTimerReset);
        state.DecelerationIntervalTimerReset = intervals.Deceleration;
        state.ActivationTimer = NativeCountdown.Start(BullMovementDelayFrames);
        state.Function = BullEnemyFunction.MovementDelay;
        state.MaxSpeed = BullMovementDefinitions.MaximumSpeed(slot.Parameter2);
    }

    /// <summary>Ports <c>MainAI_Bull</c> and all four indirect function targets.</summary>
    private static void RunBullMain(RoomEnemySlot slot, BullEnemyState state, SamusState? samus)
    {
        state.ShotReactionDisableTimer = state.ShotReactionDisableTimer.Decrement().Remaining;
        if (state.ShotReactionDisableTimer.IsZero)
        {
            // Once the timer reaches zero the cartridge pins it at one, clearing the guard
            // every subsequent frame. A zero-initialized timer first wraps to $FFFF instead.
            state.ShotReactionDisableTimer = NativeCountdown.Start(1);
            state.ShotReactionDisabled = false;
        }

        switch (state.Function)
        {
            case BullEnemyFunction.MovementDelay:
                state.ActivationTimer = state.ActivationTimer.Decrement().Remaining;
                if (state.ActivationTimer.IsZero)
                {
                    state.ActivationTimer = NativeCountdown.Start(BullMovementDelayFrames);
                    state.Function = BullEnemyFunction.TargetSamus;
                }
                return;

            case BullEnemyFunction.TargetSamus:
                if (samus is null)
                    throw new InvalidOperationException("Bull targeting requires the active Samus actor.");
                state.AngleToSamus = CalculateBullAngleToSamus(slot, samus);
                state.Angle = state.AngleToSamus;
                state.Function = BullEnemyFunction.Accelerating;
                state.AccelerationDelta = BullAccelerationDelta;
                return;

            case BullEnemyFunction.Accelerating:
                if (samus is null)
                    throw new InvalidOperationException("Bull steering requires the active Samus actor.");
                if (unchecked((short)(state.Speed - state.MaxSpeed)) < 0)
                    AccelerateBull(state);
                MoveBull(slot, state);
                TriggerBullDecelerationIfOffTarget(slot, state, samus);
                return;

            case BullEnemyFunction.Decelerating:
                if (state.Speed == 0 || unchecked((short)state.Speed) < 0 ||
                    state.Acceleration == 0 || unchecked((short)state.Acceleration) < 0)
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
                    $"Bull function $A8:{(ushort)state.Function:X4} is not translated.");
        }
    }

    /// <summary>Converts the signed displacement to Samus into Bull's quarter-turn-offset byte angle.</summary>
    private static ushort CalculateBullAngleToSamus(RoomEnemySlot slot, SamusState samus) =>
        unchecked((byte)(EnemyMath.CalculateCartridgeAngle(
            new(unchecked((short)(samus.Position.X.Whole - slot.Position.X.Whole)), unchecked((short)(samus.Position.Y.Whole - slot.Position.Y.Whole)))) - 0x40));

    /// <summary>Increases acceleration and speed only when the configured interval expires.</summary>
    private static void AccelerateBull(BullEnemyState state)
    {
        state.AccelerationIntervalTimer = state.AccelerationIntervalTimer.Decrement().Remaining;
        if (!state.AccelerationIntervalTimer.IsZero)
            return;

        state.AccelerationIntervalTimer = NativeCountdown.FromRaw(state.AccelerationIntervalTimerReset);
        state.Acceleration = unchecked((ushort)(
            state.Acceleration + state.AccelerationDelta));
        state.Speed = unchecked((ushort)(state.Speed + state.Acceleration));
    }

    /// <summary>Reduces acceleration and speed on the configured deceleration cadence.</summary>
    private static void DecelerateBull(BullEnemyState state)
    {
        state.AccelerationIntervalTimer = state.AccelerationIntervalTimer.Decrement().Remaining;
        if (!state.AccelerationIntervalTimer.IsZero)
            return;

        state.AccelerationIntervalTimer = NativeCountdown.FromRaw(state.DecelerationIntervalTimerReset);
        state.Acceleration = unchecked((ushort)(
            state.Acceleration - state.AccelerationDelta));
        state.Speed = unchecked((ushort)(state.Speed - state.Acceleration));
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
        ushort difference = unchecked((ushort)(state.AngleToSamus - state.Angle));
        if ((difference & 0x0080) != 0)
            difference |= 0xff00;
        if ((difference & 0x8000) != 0)
            difference = unchecked((ushort)-difference);
        if (unchecked((short)(difference - 0x30)) < 0)
            return;

        state.Function = BullEnemyFunction.Decelerating;
        state.AccelerationDelta = BullAccelerationDelta;
    }

    /// <summary>Integrates both 16.16 position components from the committed angle and 8.8 speed.</summary>
    internal static void MoveBull(RoomEnemySlot slot, BullEnemyState state)
    {
        slot.Position = slot.Position.WithX(AddBullComponent(
            slot.Position.X,
            ReadBullSignedSine(unchecked((byte)(state.Angle + 0x40))),
            state.Speed));
        slot.Position = slot.Position.WithY(AddBullComponent(
            slot.Position.Y,
            ReadBullSignedSine(unchecked((byte)state.Angle)),
            state.Speed));
    }

    /// <summary>Reads the cartridge's signed sine sample for one byte-sized Bull heading.</summary>
    internal static short ReadBullSignedSine(byte angle) =>
        EnemyTrigonometryTables.SignedSixteenBitSine(angle);

    /// <summary>Applies Bull's 24-bit sine-times-speed displacement and native negative-fraction rule.</summary>
    private static SnesFixedPosition AddBullComponent(
        SnesFixedPosition position,
        short sine,
        ushort speed)
    {
        // Bull discards the sine table's low byte, multiplies the absolute high byte by its
        // unsigned 8.8 speed, and keeps the 24-bit result as a 16.16 displacement. Negative
        // components use $A8:DAF6, whose zero-fraction path is deliberately one subpixel more
        // negative than a conventional two's-complement negation.
        ushort magnitude = unchecked((ushort)Math.Abs((int)sine));
        byte sineHighByte = SnesLittleEndian.HighByte(magnitude);
        uint product = (uint)sineHighByte * speed;
        ushort fraction = unchecked((ushort)product);
        ushort whole = SnesFixedPosition.WholeOf(unchecked((int)product));
        if (sine < 0 && (whole != 0 || fraction != 0))
        {
            if (fraction == 0)
                fraction = 0xffff;
            else
                fraction = unchecked((ushort)-fraction);
            whole = unchecked((ushort)~whole);
        }

        uint fractionalSum = (uint)position.Fraction + fraction;
        return new(
            unchecked((ushort)(position.Whole + whole + (SnesCarry.OutOfWord(fractionalSum)))),
            unchecked((ushort)fractionalSum));
    }

    /// <summary>Ports the no-damage tail of <c>EnemyShot_Bull</c> at $A8:DB2C.</summary>
    internal static void ResolveBullImmuneShot(
        RoomEnemySlot bull,
        BullEnemyState state,
        ushort projectileDirection)
    {
        if (state.ShotReactionDisabled)
            return;

        bull.InstructionTimer = NativeCountdown.Start(1);
        bull.Timer = NativeCountdown.Start(0);
        state.Program = AnimationProgramCursor<BullProgram>.Start(BullProgram.Shot);
        int direction = projectileDirection & 0x000f;
        state.Angle = BullMovementDefinitions.ShotAngle(direction);
        state.Acceleration = 0x0100;
        state.Speed = 0x0600;
        state.Function = BullEnemyFunction.Decelerating;
        state.ShotReactionDisableTimer = NativeCountdown.Start(0x0030);
        state.ShotReactionDisabled = true;
    }

    /// <summary>Rejects a Bull callback whose slot has not received species initialization.</summary>
    private BullEnemyState RequireBullState(RoomEnemySlot slot) =>
        _bullStates[slot.SlotIndex] ?? throw new InvalidOperationException(
            $"Enemy slot {slot.SlotIndex} has no initialized Bull state.");

    /// <summary>Runs Bull's normal and immune-shot animations as typed programs (#641).</summary>
    internal void RegisterBullTypedAnimations(EnemyAiTable table) =>
        table.AddTypedAnimation(slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.Bull, (slot, _) =>
        {
            if (!EnemyProgramAnimation.Expire(slot))
                return;
            var state = RequireBullState(slot);
            AnimationProgramCursor<BullProgram> cursor = state.Program;
            EnemyProgramAnimation.Run(slot, BullAnimations.Programs, ref cursor, BullFrameDefinitions.Spritemap);
            state.Program = cursor;
        });

    /// <summary>Applies Bull's immune-hit recoil after common bomb damage handling.</summary>
    internal void RegisterBullBombReactions(EnemyAiTable table)
    {
        table.AddBombCommon(new EnemyBombReaction(c => c.Enemy.EnemyDefinitionPointer == EnemyDefinitionId.Bull &&
                                                       c.SelectedShotAi == EnemyInteractionCallback.BullShot)
        {
            AfterCommon = c =>
            {
                BullEnemyState bullState = RequireBullState(c.Enemy);
                bullState.PreviousHealth = c.HealthBefore;
                if (c.Enemy.Health == c.HealthBefore)
                    ResolveBullImmuneShot(c.Enemy, bullState, c.Bomb.Direction);
            },
        });
    }

    /// <summary>Associates the Bull definition with its native initialization routine.</summary>
    internal void RegisterBullInitializationAi(EnemyAiTable table)
    {

        table.RegisterInitialization(EnemyAiRoutine.InitAI_Bull, slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.Bull, (slot, level, samus, controllerInput, cameraX) =>
        {
            InitializeBull(slot);
        });
    }

    /// <summary>Advances the Bull's delay, targeting, acceleration, and braking state each frame.</summary>
    internal void RegisterBullMainAi(EnemyAiTable table)
    {

        table.RegisterMain(EnemyAiRoutine.MainAI_Bull, slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.Bull, (slot, frame, nmiFrameCounter8) =>
        {
            RunBullMain(slot, RequireBullState(slot), frame.Samus);
        });
    }

    /// <summary>Applies recoil after a frozen or undamaging accepted projectile hit.</summary>
    internal void RegisterBullShotReactions(EnemyAiTable table)
    {
        table.AddShot(new EnemyShotReaction(enemy => enemy.EnemyDefinitionPointer == EnemyDefinitionId.Bull &&
                                                     EnemyCollision.ShotAi(enemy) == EnemyInteractionCallback.BullShot)
        {
            AfterCommon = (c, froze) =>
            {
                BullEnemyState bullState = RequireBullState(c.Enemy);
                bullState.PreviousHealth = c.HealthBefore;
                if (froze || c.Enemy.Health == c.HealthBefore)
                    ResolveBullImmuneShot(c.Enemy, bullState, c.ProjectileDirection);
            },
        });
    }

    /// <inheritdoc/>
    public void ResetForRoomLoad(IRoomEnemyLoad request)
    {
        Array.Clear(_bullStates);
        Array.Clear(_bullMaxSpeeds);
        Array.Clear(_bullAnglesToSamus);
        Array.Clear(_bullAngles);
        Array.Clear(_bullShotReactionDisableFlags);
        Array.Clear(_bullAccelerationTimerResets);
        Array.Clear(_bullDecelerationTimerResets);
        Array.Clear(_bullShotReactionDisableTimers);
        Array.Clear(_bullPreviousHealth);
    }

    /// <inheritdoc/>
    public void BeginEnemyFrame(RoomEnemyFrame frame)
    {
    }
}
