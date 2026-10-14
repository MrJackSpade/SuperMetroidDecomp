namespace SuperMetroid.Core.Game;

/// <summary>
/// Debugger-visible view of Bull's six common words and its bank-$7E extension words. Raw
/// 16-bit values are intentional: speed and acceleration are unsigned 8.8 magnitudes, while
/// angle arithmetic and the native independently-negated 16.16 movement components wrap.
/// </summary>
public sealed class BullEnemyState
{
    /// <summary>Common slot words carrying Bull's dispatcher, flight speed, acceleration, and timers.</summary>
    private readonly RoomEnemySlot _slot;
    /// <summary>Per-slot speed thresholds selected by Bull's population parameter.</summary>
    private readonly ushort[] _maxSpeeds;
    /// <summary>Latest measured angles toward Samus, used to decide when a flight has gone off target.</summary>
    private readonly ushort[] _anglesToSamus;
    /// <summary>Captured flight angles used to project speed into X and Y movement.</summary>
    private readonly ushort[] _angles;
    /// <summary>Per-slot flags suppressing repeated immune-shot kicks during cooldown.</summary>
    private readonly ushort[] _shotReactionDisableFlags;
    /// <summary>Configured acceleration countdown reload values.</summary>
    private readonly ushort[] _accelerationTimerResets;
    /// <summary>Configured deceleration countdown reload values.</summary>
    private readonly ushort[] _decelerationTimerResets;
    /// <summary>Remaining immune-shot cooldown ticks for each slot.</summary>
    private readonly ushort[] _shotReactionDisableTimers;
    /// <summary>Health captured before projectile contact to distinguish a no-damage hit.</summary>
    private readonly ushort[] _previousHealth;

    /// <summary>Exposes one Bull slot's common words and shared parallel extension arrays without copying state.</summary>
    internal BullEnemyState(RoomEnemySlot slot, BullSharedTables tables)
    {
        _slot = slot;
        _maxSpeeds = tables.MaxSpeeds;
        _anglesToSamus = tables.AnglesToSamus;
        _angles = tables.Angles;
        _shotReactionDisableFlags = tables.ShotReactionDisableFlags;
        _accelerationTimerResets = tables.AccelerationTimerResets;
        _decelerationTimerResets = tables.DecelerationTimerResets;
        _shotReactionDisableTimers = tables.ShotReactionDisableTimers;
        _previousHealth = tables.PreviousHealth;
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
    public NativeCountdown ActivationTimer
    {
        get => NativeCountdown.FromRaw(_slot.VariableE);
        internal set => _slot.VariableE = value.RawValue;
    }

    /// <summary>Native variable F: shared acceleration/deceleration interval countdown, decremented on eligible speed-update calls and reloaded from the current phase's reset value on exact-zero expiry.</summary>
    public NativeCountdown AccelerationIntervalTimer
    {
        get => NativeCountdown.FromRaw(_slot.VariableF);
        internal set => _slot.VariableF = value.RawValue;
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
    public NativeCountdown ShotReactionDisableTimer
    {
        get => NativeCountdown.FromRaw(_shotReactionDisableTimers[_slot.SlotIndex]);
        internal set => _shotReactionDisableTimers[_slot.SlotIndex] = value.RawValue;
    }

    /// <summary>Native parallel $7E:8800 health snapshot taken before projectile damage, retained so unchanged health can select the actor's immune-shot kick instead of treating it as ordinary damage.</summary>
    public ushort PreviousHealth
    {
        get => _previousHealth[_slot.SlotIndex];
        internal set => _previousHealth[_slot.SlotIndex] = value;
    }
}
