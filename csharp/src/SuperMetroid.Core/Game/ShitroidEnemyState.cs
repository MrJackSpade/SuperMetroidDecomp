namespace SuperMetroid.Core.Game;

/// <summary>Typed projection of Shitroid's common and extended enemy WRAM.</summary>
public sealed class ShitroidEnemyState
{
    /// <summary>Target palette image staged across the encounter's palette transitions.</summary>
    internal readonly Bgr555[] _targetPalette = new Bgr555[256];

    /// <summary>Projects Shitroid's common variables through its owning physical enemy slot.</summary>
    internal ShitroidEnemyState(RoomEnemySlot slot) => Slot = slot;

    /// <summary>Gets the physical enemy slot that owns the projected common words.</summary>
    public RoomEnemySlot Slot { get; }

    /// <summary>Where the Shitroid's program resumes (#641).</summary>
    internal AnimationProgramCursor<ShitroidProgram> Cursor { get; set; }

    /// <summary>Gets the current bank-$A9 Shitroid AI function pointer.</summary>
    public ShitroidAiFunction Function
    {
        get => (ShitroidAiFunction)Slot.VariableA;
        internal set => Slot.VariableA = (ushort)value;
    }

    /// <summary>Signed 8.8 velocity: X in native enemy variable B, Y in variable C.</summary>
    public EightEightVector<RoomPixels> Velocity
    {
        get => new(new SnesSignedEightEight(Slot.VariableB), new SnesSignedEightEight(Slot.VariableC));
        internal set
        {
            Slot.VariableB = value.X.RawValue;
            Slot.VariableC = value.Y.RawValue;
        }
    }

    /// <summary>Low-byte palette phase and high-byte delay timer in variable D.</summary>
    public ushort PaletteTimerAndPhase
    {
        get => Slot.VariableD;
        internal set => Slot.VariableD = value;
    }

    /// <summary>Gets the phase reload controlling Shitroid's palette-cycle cadence.</summary>
    public ushort PaletteDelay
    {
        get => Slot.VariableE;
        internal set => Slot.VariableE = value;
    }

    /// <summary>Gets the current AI state's general-purpose countdown.</summary>
    public NativeCountdown StateTimer
    {
        get => NativeCountdown.FromRaw(Slot.VariableF);
        internal set => Slot.VariableF = value.RawValue;
    }

    /// <summary>Native extended word 01: randomized vertical-follow duration.</summary>
    public NativeCountdown HoverTimer { get; internal set; }

    /// <summary>Native extended word 02: consecutive horizontal-lock accumulator.</summary>
    public ushort HorizontalLockAccumulator { get; internal set; }

    /// <summary>Native extended word 04: enables the periodic Shitroid cry.</summary>
    public ushort CryEnabled { get; internal set; }

    /// <summary>Native extended word 05: counts every fourth palette-cycle cry.</summary>
    public ushort CryCounter { get; internal set; }

    /// <summary>
    /// Diagnostic mirror of native word 08 in physical slot one. Shitroid writes the actual
    /// dead-sidehopper palette stage through <c>ActivateDeadSidehopperVictim</c>; retaining
    /// this value on the boss state makes that cross-slot side effect directly auditable.
    /// </summary>
    public ushort VictimActivationFlag { get; internal set; }
    /// <summary>Mutable target palette storage used by encounter palette updates.</summary>
    internal Span<Bgr555> MutableTargetPalette => _targetPalette;
}
