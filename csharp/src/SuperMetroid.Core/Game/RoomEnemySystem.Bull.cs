namespace SuperMetroid.Core.Game;

/// <summary>The literal bank-$A8 function word stored by Bull in common enemy variable A.</summary>
public enum BullEnemyFunction : ushort
{
    MovementDelay = 0xd92b,
    TargetSamus = 0xd940,
    Accelerating = 0xd963,
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

    public BullEnemyFunction Function
    {
        get => (BullEnemyFunction)_slot.VariableA;
        internal set => _slot.VariableA = (ushort)value;
    }

    public ushort Acceleration
    {
        get => _slot.VariableB;
        internal set => _slot.VariableB = value;
    }

    public ushort AccelerationDelta
    {
        get => _slot.VariableC;
        internal set => _slot.VariableC = value;
    }

    public ushort Speed
    {
        get => _slot.VariableD;
        internal set => _slot.VariableD = value;
    }

    public ushort ActivationTimer
    {
        get => _slot.VariableE;
        internal set => _slot.VariableE = value;
    }

    public ushort AccelerationIntervalTimer
    {
        get => _slot.VariableF;
        internal set => _slot.VariableF = value;
    }

    public ushort MaxSpeed
    {
        get => _maxSpeeds[_slot.SlotIndex];
        internal set => _maxSpeeds[_slot.SlotIndex] = value;
    }

    public ushort AngleToSamus
    {
        get => _anglesToSamus[_slot.SlotIndex];
        internal set => _anglesToSamus[_slot.SlotIndex] = value;
    }

    public ushort Angle
    {
        get => _angles[_slot.SlotIndex];
        internal set => _angles[_slot.SlotIndex] = value;
    }

    public bool ShotReactionDisabled
    {
        get => _shotReactionDisableFlags[_slot.SlotIndex] != 0;
        internal set => _shotReactionDisableFlags[_slot.SlotIndex] = value ? (ushort)1 : (ushort)0;
    }

    public ushort AccelerationIntervalTimerReset
    {
        get => _accelerationTimerResets[_slot.SlotIndex];
        internal set => _accelerationTimerResets[_slot.SlotIndex] = value;
    }

    public ushort DecelerationIntervalTimerReset
    {
        get => _decelerationTimerResets[_slot.SlotIndex];
        internal set => _decelerationTimerResets[_slot.SlotIndex] = value;
    }

    public ushort ShotReactionDisableTimer
    {
        get => _shotReactionDisableTimers[_slot.SlotIndex];
        internal set => _shotReactionDisableTimers[_slot.SlotIndex] = value;
    }

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

    private const int BullMaxSpeedTable = 0xa8d885;
    private const int BullAccelerationIntervalTable = 0xa8d895;
    // The label spans four contiguous 64-word quadrants at $B1C3-$B3C2. It is the full
    // signed 16-bit sine table, not the similarly named sign-extended table at $B443.
    private const int BullSignedSineTable = 0xa0b1c3;
    private const ushort BullNormalInstruction = 0xd841;
    private const ushort BullShotInstruction = 0xd855;
    private const ushort BullAccelerationDelta = 0x0018;
    private const ushort BullMovementDelayFrames = 0x0010;

    // Directions zero through nine are Samus's native projectile direction nibble. Entries
    // four/five and zero/nine intentionally duplicate straight vertical movement for the
    // two facing-dependent gun poses.
    private static readonly ushort[] BullShotAngles =
    [
        0x00c0, 0x00e0, 0x0000, 0x0020, 0x0040,
        0x0040, 0x0060, 0x0080, 0x00a0, 0x00c0,
    ];

    private readonly BullEnemyState?[] _bullStates = new BullEnemyState?[MaximumEnemyCount];
    private readonly ushort[] _bullMaxSpeeds = new ushort[MaximumEnemyCount];
    private readonly ushort[] _bullAnglesToSamus = new ushort[MaximumEnemyCount];
    private readonly ushort[] _bullAngles = new ushort[MaximumEnemyCount];
    private readonly ushort[] _bullShotReactionDisableFlags = new ushort[MaximumEnemyCount];
    private readonly ushort[] _bullAccelerationTimerResets = new ushort[MaximumEnemyCount];
    private readonly ushort[] _bullDecelerationTimerResets = new ushort[MaximumEnemyCount];
    private readonly ushort[] _bullShotReactionDisableTimers = new ushort[MaximumEnemyCount];
    private readonly ushort[] _bullPreviousHealth = new ushort[MaximumEnemyCount];

    /// <summary>Typed Bull state for all 32 physical enemy slots.</summary>
    public IReadOnlyList<BullEnemyState?> BullStates => _bullStates;

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
        slot.CurrentInstruction = BullNormalInstruction;

        // There are thirteen acceleration/deceleration records and eight max speeds. Native
        // code performs unchecked table reads; using the ROM address directly retains that
        // behavior for debug-edited parameters instead of imposing host-only validation.
        int intervalAddress = BullAccelerationIntervalTable + slot.Parameter1 * 4;
        state.AccelerationIntervalTimerReset = ReadWord(_bus!, intervalAddress);
        state.AccelerationIntervalTimer = state.AccelerationIntervalTimerReset;
        state.DecelerationIntervalTimerReset = ReadWord(_bus!, intervalAddress + 2);
        state.ActivationTimer = BullMovementDelayFrames;
        state.Function = BullEnemyFunction.MovementDelay;
        state.MaxSpeed = ReadWord(_bus!, BullMaxSpeedTable + slot.Parameter2 * 2);
    }

    /// <summary>Ports <c>MainAI_Bull</c> and all four indirect function targets.</summary>
    private void RunBullMain(RoomEnemySlot slot, BullEnemyState state, SamusState? samus)
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

        // Sign_Extend_A uses only bit seven of the wrapped difference, then NegateA makes
        // it positive. This is the shortest circular byte-angle distance, not a 16-bit
        // absolute difference.
        int difference = unchecked((sbyte)(byte)(state.AngleToSamus - state.Angle));
        if (Math.Abs(difference) < 0x30)
            return;

        state.Function = BullEnemyFunction.Decelerating;
        state.AccelerationDelta = BullAccelerationDelta;
    }

    private void MoveBull(RoomEnemySlot slot, BullEnemyState state)
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

    private short ReadBullSignedSine(byte angle) =>
        unchecked((short)ReadWord(_bus!, BullSignedSineTable + angle * 2));

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
        bull.CurrentInstruction = BullShotInstruction;
        int direction = projectileDirection & 0x000f;
        if ((uint)direction >= BullShotAngles.Length)
        {
            // Native code reads beyond the ten-word table for malformed direction nibbles.
            // Live Samus projectiles are constrained to zero through nine, so surface a bad
            // producer instead of inventing a host angle from adjacent animation data.
            throw new InvalidDataException(
                $"Bull immune-shot reaction received invalid projectile direction {direction}.");
        }
        state.Angle = BullShotAngles[direction];
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
