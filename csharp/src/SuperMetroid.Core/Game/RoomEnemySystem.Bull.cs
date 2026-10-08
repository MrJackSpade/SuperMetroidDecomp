namespace SuperMetroid.Core.Game;

/// <summary>The literal bank-$A8 function word stored by Bull in common enemy variable A.</summary>
public enum BullEnemyFunction : ushort
{
    /// <summary>$A8:D92B, Function_Bull_MovementDelay: waits for the sixteen-call activation countdown to reach zero, reloads it, and selects Samus targeting.</summary>
    MovementDelay = 0xd92b,
    /// <summary>$A8:D940, Function_Bull_TargetSamus: captures the byte angle toward Samus, sets acceleration delta $18, and starts accelerating without moving that call.</summary>
    TargetSamus = 0xd940,
    /// <summary>$A8:D963, Function_Bull_Accelerating: increases speed on configured interval expiries while below the maximum threshold, moves along the captured angle, and checks native $30-unit off-target deceleration.</summary>
    Accelerating = 0xd963,
    /// <summary>$A8:D97C, Function_Bull_Decelerating: moves before reducing speed on interval expiries, then clears speed/acceleration and returns to the movement delay when either magnitude is zero or signed-negative.</summary>
    Decelerating = 0xd97c,
}

/// <summary>
/// Debugger-visible view of Bull's six common words and its bank-$7E extension words. Raw
/// 16-bit values are intentional: speed and acceleration are unsigned 8.8 magnitudes, while
/// angle arithmetic and the native independently-negated 16.16 movement components wrap.
/// </summary>
public sealed class BullEnemyState
{
    private readonly RoomEnemySlot _slot;
    private readonly ushort[] _maxSpeeds;
    private readonly ushort[] _anglesToSamus;
    private readonly ushort[] _angles;
    private readonly ushort[] _shotReactionDisableFlags;
    private readonly ushort[] _accelerationTimerResets;
    private readonly ushort[] _decelerationTimerResets;
    private readonly ushort[] _shotReactionDisableTimers;
    private readonly ushort[] _previousHealth;

    internal BullEnemyState(
        RoomEnemySlot slot,
        ushort[] maxSpeeds,
        ushort[] anglesToSamus,
        ushort[] angles,
        ushort[] shotReactionDisableFlags,
        ushort[] accelerationTimerResets,
        ushort[] decelerationTimerResets,
        ushort[] shotReactionDisableTimers,
        ushort[] previousHealth)
    {
        _slot = slot;
        _maxSpeeds = maxSpeeds;
        _anglesToSamus = anglesToSamus;
        _angles = angles;
        _shotReactionDisableFlags = shotReactionDisableFlags;
        _accelerationTimerResets = accelerationTimerResets;
        _decelerationTimerResets = decelerationTimerResets;
        _shotReactionDisableTimers = shotReactionDisableTimers;
        _previousHealth = previousHealth;
    }

    /// <summary>Native variable A ($0FA8 plus slot byte index): the current bank-$A8 delay, target, acceleration, or deceleration dispatcher pointer.</summary>
    public BullEnemyFunction Function
    {
        get => (BullEnemyFunction)_slot.VariableA;
        internal set => _slot.VariableA = (ushort)value;
    }

    /// <summary>Native variable B: wrapping unsigned 8.8 speed-change magnitude, increased or decreased by AccelerationDelta and then added to or subtracted from Speed at interval expiry.</summary>
    public ushort Acceleration
    {
        get => _slot.VariableB;
        internal set => _slot.VariableB = value;
    }

    /// <summary>Native variable C: second-order increment $0018 applied to the 8.8 acceleration magnitude at each configured interval expiry; cleared when the flight cycle stops.</summary>
    public ushort AccelerationDelta
    {
        get => _slot.VariableC;
        internal set => _slot.VariableC = value;
    }

    /// <summary>Native variable D: unsigned 8.8 flight-speed magnitude in pixels per AI update, projected through the sine table into wrapped 16.16 X/Y displacements.</summary>
    public ushort Speed
    {
        get => _slot.VariableD;
        internal set => _slot.VariableD = value;
    }

    /// <summary>Native variable E: wrapping movement-delay counter initialized/reloaded to $0010, with targeting selected when its decrement reaches exactly zero.</summary>
    public ushort ActivationTimer
    {
        get => _slot.VariableE;
        internal set => _slot.VariableE = value;
    }

    /// <summary>Native variable F: shared acceleration/deceleration interval countdown, decremented on eligible speed-update calls and reloaded from the current phase's reset value on exact-zero expiry.</summary>
    public ushort AccelerationIntervalTimer
    {
        get => _slot.VariableF;
        internal set => _slot.VariableF = value;
    }

    /// <summary>Native extension offset $00: unsigned 8.8 maximum-speed threshold selected by population parameter two; prevents further acceleration when reached but does not clamp an overshooting update.</summary>
    public ushort MaxSpeed
    {
        get => _maxSpeeds[_slot.SlotIndex];
        internal set => _maxSpeeds[_slot.SlotIndex] = value;
    }

    /// <summary>Native extension offset $02: latest byte-wrapped angle toward Samus, 256 units per turn with zero pointing right; compared with the captured flight angle rather than continuously steering it.</summary>
    public ushort AngleToSamus
    {
        get => _anglesToSamus[_slot.SlotIndex];
        internal set => _anglesToSamus[_slot.SlotIndex] = value;
    }

    /// <summary>Native extension offset $04: captured movement angle in 1/256-turn units, zero right and $40 down, set when targeting Samus or replaced from projectile direction by an immune-shot kick.</summary>
    public ushort Angle
    {
        get => _angles[_slot.SlotIndex];
        internal set => _angles[_slot.SlotIndex] = value;
    }

    /// <summary>Native extension offset $06 flag suppressing repeated no-damage projectile kicks during the shot-reaction cooldown; it does not disable ordinary health processing.</summary>
    public bool ShotReactionDisabled
    {
        get => _shotReactionDisableFlags[_slot.SlotIndex] != 0;
        internal set => _shotReactionDisableFlags[_slot.SlotIndex] = value ? (ushort)1 : (ushort)0;
    }

    /// <summary>Native extension offset $0A: acceleration interval in eligible AI calls, selected by population parameter one and also copied into the initial live interval timer.</summary>
    public ushort AccelerationIntervalTimerReset
    {
        get => _accelerationTimerResets[_slot.SlotIndex];
        internal set => _accelerationTimerResets[_slot.SlotIndex] = value;
    }

    /// <summary>Native extension offset $0C: deceleration interval selected alongside the acceleration interval; used to reload the shared timer after a deceleration expiry.</summary>
    public ushort DecelerationIntervalTimerReset
    {
        get => _decelerationTimerResets[_slot.SlotIndex];
        internal set => _decelerationTimerResets[_slot.SlotIndex] = value;
    }

    /// <summary>Native extension offset $0E: wrapping guard timer set to $0030 by an immune-shot kick and decremented each main-AI call; reaching zero clears the flag and pins the timer to one, while an initial zero first wraps to $FFFF.</summary>
    public ushort ShotReactionDisableTimer
    {
        get => _shotReactionDisableTimers[_slot.SlotIndex];
        internal set => _shotReactionDisableTimers[_slot.SlotIndex] = value;
    }

    /// <summary>Native parallel $7E:8800 health snapshot taken before projectile damage, retained so unchanged health can select the actor's immune-shot kick instead of treating it as ordinary damage.</summary>
    public ushort PreviousHealth
    {
        get => _previousHealth[_slot.SlotIndex];
        internal set => _previousHealth[_slot.SlotIndex] = value;
    }
}

/// <summary>
/// Literal translation of Bull enemy <c>$E97F</c> from <c>$A8:D821-$DBC6</c>. Its three-map
/// animation is independent of a seek/accelerate/decelerate flight cycle; immune shots kick
/// the actor along the projectile direction and temporarily suppress repeated reactions.
/// </summary>
public sealed partial class RoomEnemySystem
{
    internal const ushort BullDefinition = 0xe97f;

    private const ushort BullAccelerationDelta = 0x0018;
    private const ushort BullMovementDelayFrames = 0x0010;

    private readonly BullEnemyState?[] _bullStates = new BullEnemyState?[MaximumEnemyCount];
    private readonly ushort[] _bullMaxSpeeds = new ushort[MaximumEnemyCount];
    private readonly ushort[] _bullAnglesToSamus = new ushort[MaximumEnemyCount];
    private readonly ushort[] _bullAngles = new ushort[MaximumEnemyCount];
    private readonly ushort[] _bullShotReactionDisableFlags = new ushort[MaximumEnemyCount];
    private readonly ushort[] _bullAccelerationTimerResets = new ushort[MaximumEnemyCount];
    private readonly ushort[] _bullDecelerationTimerResets = new ushort[MaximumEnemyCount];
    private readonly ushort[] _bullShotReactionDisableTimers = new ushort[MaximumEnemyCount];
    private readonly ushort[] _bullPreviousHealth = new ushort[MaximumEnemyCount];

    /// <summary>Ports <c>InitAI_Bull</c> at <c>$A8:D8C9</c>.</summary>
    private void InitializeBull(RoomEnemySlot slot)
    {
        var state = new BullEnemyState(
            slot,
            _bullMaxSpeeds,
            _bullAnglesToSamus,
            _bullAngles,
            _bullShotReactionDisableFlags,
            _bullAccelerationTimerResets,
            _bullDecelerationTimerResets,
            _bullShotReactionDisableTimers,
            _bullPreviousHealth);
        _bullStates[slot.SlotIndex] = state;

        slot.InstructionTimer = 1;
        slot.Timer = 0;
        slot.CurrentInstruction = BullInstructionProgramDefinitions.Normal;

        // These are definition selectors, not live timers or editable presentation data.
        // Unsupported debug-edited selectors fail explicitly instead of reading adjacent code.
        var intervals = BullMovementDefinitions.Intervals(slot.Parameter1);
        state.AccelerationIntervalTimerReset = intervals.Acceleration;
        state.AccelerationIntervalTimer = state.AccelerationIntervalTimerReset;
        state.DecelerationIntervalTimerReset = intervals.Deceleration;
        state.ActivationTimer = BullMovementDelayFrames;
        state.Function = BullEnemyFunction.MovementDelay;
        state.MaxSpeed = BullMovementDefinitions.MaximumSpeed(slot.Parameter2);
    }

    /// <summary>Ports <c>MainAI_Bull</c> and all four indirect function targets.</summary>
    private static void RunBullMain(RoomEnemySlot slot, BullEnemyState state, SamusState? samus)
    {
        state.ShotReactionDisableTimer = unchecked((ushort)(
            state.ShotReactionDisableTimer - 1));
        if (state.ShotReactionDisableTimer == 0)
        {
            // Once the timer reaches zero the cartridge pins it at one, clearing the guard
            // every subsequent frame. A zero-initialized timer first wraps to $FFFF instead.
            state.ShotReactionDisableTimer = 1;
            state.ShotReactionDisabled = false;
        }

        switch (state.Function)
        {
            case BullEnemyFunction.MovementDelay:
                state.ActivationTimer = unchecked((ushort)(state.ActivationTimer - 1));
                if (state.ActivationTimer == 0)
                {
                    state.ActivationTimer = BullMovementDelayFrames;
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

    private static ushort CalculateBullAngleToSamus(RoomEnemySlot slot, SamusState samus) =>
        unchecked((byte)(CalculateCartridgeAngle(
            unchecked((short)(samus.XPosition - slot.XPosition)),
            unchecked((short)(samus.YPosition - slot.YPosition))) - 0x40));

    private static void AccelerateBull(BullEnemyState state)
    {
        state.AccelerationIntervalTimer = unchecked((ushort)(
            state.AccelerationIntervalTimer - 1));
        if (state.AccelerationIntervalTimer != 0)
            return;

        state.AccelerationIntervalTimer = state.AccelerationIntervalTimerReset;
        state.Acceleration = unchecked((ushort)(
            state.Acceleration + state.AccelerationDelta));
        state.Speed = unchecked((ushort)(state.Speed + state.Acceleration));
    }

    private static void DecelerateBull(BullEnemyState state)
    {
        state.AccelerationIntervalTimer = unchecked((ushort)(
            state.AccelerationIntervalTimer - 1));
        if (state.AccelerationIntervalTimer != 0)
            return;

        state.AccelerationIntervalTimer = state.DecelerationIntervalTimerReset;
        state.Acceleration = unchecked((ushort)(
            state.Acceleration - state.AccelerationDelta));
        state.Speed = unchecked((ushort)(state.Speed - state.Acceleration));
    }

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

    private static void MoveBull(RoomEnemySlot slot, BullEnemyState state)
    {
        (slot.XPosition, slot.XSubposition) = AddBullComponent(
            slot.XPosition,
            slot.XSubposition,
            ReadBullSignedSine(unchecked((byte)(state.Angle + 0x40))),
            state.Speed);
        (slot.YPosition, slot.YSubposition) = AddBullComponent(
            slot.YPosition,
            slot.YSubposition,
            ReadBullSignedSine(unchecked((byte)state.Angle)),
            state.Speed);
    }

    private static short ReadBullSignedSine(byte angle) =>
        EnemyTrigonometryTables.SignedSixteenBitSine(angle);

    private static (ushort Position, ushort Subposition) AddBullComponent(
        ushort position,
        ushort subposition,
        short sine,
        ushort speed)
    {
        // Bull discards the sine table's low byte, multiplies the absolute high byte by its
        // unsigned 8.8 speed, and keeps the 24-bit result as a 16.16 displacement. Negative
        // components use $A8:DAF6, whose zero-fraction path is deliberately one subpixel more
        // negative than a conventional two's-complement negation.
        ushort magnitude = unchecked((ushort)Math.Abs((int)sine));
        byte sineHighByte = unchecked((byte)(magnitude >> 8));
        uint product = (uint)sineHighByte * speed;
        ushort fraction = unchecked((ushort)product);
        ushort whole = unchecked((ushort)(product >> 16));
        if (sine < 0 && (whole != 0 || fraction != 0))
        {
            if (fraction == 0)
                fraction = 0xffff;
            else
                fraction = unchecked((ushort)-fraction);
            whole = unchecked((ushort)~whole);
        }

        uint fractionalSum = (uint)subposition + fraction;
        subposition = unchecked((ushort)fractionalSum);
        position = unchecked((ushort)(position + whole + (fractionalSum >> 16)));
        return (position, subposition);
    }

    /// <summary>Ports the no-damage tail of <c>EnemyShot_Bull</c> at $A8:DB2C.</summary>
    private static void ResolveBullImmuneShot(
        RoomEnemySlot bull,
        BullEnemyState state,
        ushort projectileDirection)
    {
        if (state.ShotReactionDisabled)
            return;

        bull.InstructionTimer = 1;
        bull.Timer = 0;
        bull.CurrentInstruction = BullInstructionProgramDefinitions.Shot;
        int direction = projectileDirection & 0x000f;
        state.Angle = BullMovementDefinitions.ShotAngle(direction);
        state.Acceleration = 0x0100;
        state.Speed = 0x0600;
        state.Function = BullEnemyFunction.Decelerating;
        state.ShotReactionDisableTimer = 0x0030;
        state.ShotReactionDisabled = true;
    }

    private BullEnemyState RequireBullState(RoomEnemySlot slot) =>
        _bullStates[slot.SlotIndex] ?? throw new InvalidOperationException(
            $"Enemy slot {slot.SlotIndex} has no initialized Bull state.");
}
