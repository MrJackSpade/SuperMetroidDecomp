namespace SuperMetroid.Core.Game;

/// <summary>
/// Typed view of the six native Dachora variables. Their meanings intentionally change
/// between phases: A/B are horizontal speed while running and vertical speed after the
/// shinespark; D is vertical subacceleration on the body but echo lifetime on echo slots.
/// </summary>
public sealed class DachoraEnemyState
{
    /// <summary>Physical Dachora body or echo slot carrying its phase-dependent variables.</summary>
    private readonly RoomEnemySlot _slot;

    /// <summary>Projects Dachora's six native words from its occupied enemy slot.</summary>
    internal DachoraEnemyState(RoomEnemySlot slot) => _slot = slot;

    /// <summary>Variable A while running, falling or shinesparking: whole speed.</summary>
    public ushort Speed
    {
        get => _slot.VariableA;
        internal set => _slot.VariableA = value;
    }

    /// <summary>
    /// Variable A while blinking or charging: the native wrapping countdown to the next phase
    /// (<c>DEC</c>, <c>BNE</c>).
    /// </summary>
    public NativeCountdown PhaseTimer
    {
        get => NativeCountdown.FromRaw(_slot.VariableA);
        internal set => _slot.VariableA = value.RawValue;
    }

    /// <summary>Variable A on the first echo slot: frames until the echo positions next advance.</summary>
    public NativeCountdown EchoPositionTimer
    {
        get => NativeCountdown.FromRaw(_slot.VariableA);
        internal set => _slot.VariableA = value.RawValue;
    }

    /// <summary>Variable B: fractional speed.</summary>
    public ushort Subspeed
    {
        get => _slot.VariableB;
        internal set => _slot.VariableB = value;
    }

    /// <summary>Variable C: whole vertical acceleration during the shinespark.</summary>
    public ushort VerticalAcceleration
    {
        get => _slot.VariableC;
        internal set => _slot.VariableC = value;
    }

    /// <summary>Variable D: fractional vertical acceleration or echo visibility lifetime.</summary>
    public ushort VerticalSubaccelerationOrLifetime
    {
        get => _slot.VariableD;
        internal set => _slot.VariableD = value;
    }

    /// <summary>
    /// Variable E: packed palette-animation frame (high byte) and low-byte countdown. The cartridge
    /// decrements the whole word and tests only the low byte.
    /// </summary>
    public ushort PaletteAnimationTimer
    {
        get => _slot.VariableE;
        internal set => _slot.VariableE = value;
    }

    /// <summary>Variable F: bank-$A7 indirect main-AI address.</summary>
    public DachoraAiFunction Function
    {
        get => (DachoraAiFunction)_slot.VariableF;
        internal set => _slot.VariableF = (ushort)value;
    }
}
