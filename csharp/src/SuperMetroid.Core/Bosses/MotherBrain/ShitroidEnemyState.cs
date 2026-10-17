using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.Bosses.MotherBrain;

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

    /// <summary>
    /// Membrane palette-cycle phase, 0 through 7. It shares variable D with <see cref="PaletteTimer"/>; each view
    /// rewrites only its own half so the slot word stays exact for code that reads the slot by offset.
    /// </summary>
    public int PalettePhase
    {
        get => SnesLittleEndian.LowByte(Slot.VariableD);
        internal set => Slot.VariableD = SnesLittleEndian.Word(checked((byte)value), SnesLittleEndian.HighByte(Slot.VariableD));
    }

    /// <summary>
    /// Updates left before the membrane palette advances to its next phase; each update decrements it until zero,
    /// and the advance reloads it from <see cref="PaletteDelay"/>. It shares variable D with <see cref="PalettePhase"/>.
    /// </summary>
    public int PaletteTimer
    {
        get => SnesLittleEndian.HighByte(Slot.VariableD);
        internal set => Slot.VariableD = SnesLittleEndian.Word(SnesLittleEndian.LowByte(Slot.VariableD), checked((byte)value));
    }

    /// <summary>Updates between palette-cycle phases: ten at the ordinary pace, one while draining quickly.</summary>
    public int PaletteDelay
    {
        get => Slot.VariableE;
        internal set => Slot.VariableE = checked((ushort)value);
    }

    /// <summary>Gets the current AI state's general-purpose countdown.</summary>
    public NativeCountdown StateTimer
    {
        get => NativeCountdown.FromRaw(Slot.VariableF);
        internal set => Slot.VariableF = value.RawValue;
    }

    /// <summary>Native extended word 01: randomized vertical-follow duration.</summary>
    public NativeCountdown HoverTimer { get; internal set; }

    /// <summary>
    /// Horizontal-lock accumulator (native extended word 02): gains two each update the Shitroid
    /// is horizontally aligned with its target and loses one, never below zero, otherwise.
    /// </summary>
    public int HorizontalLockAccumulator { get; internal set; }

    /// <summary>Whether the periodic Shitroid cry is enabled (native extended word 04).</summary>
    public bool CryEnabled { get; internal set; }

    /// <summary>Palette cycles toward the next periodic cry, 0 through 3 (native extended word 05).</summary>
    public int CryCounter { get; internal set; }

    /// <summary>
    /// Diagnostic mirror of native word 08 in physical slot one. Shitroid writes the actual
    /// dead-sidehopper palette stage through <c>ActivateDeadSidehopperVictim</c>; retaining
    /// this value on the boss state makes that cross-slot side effect directly auditable.
    /// </summary>
    public bool VictimActivationFlag { get; internal set; }
    /// <summary>Mutable target palette storage used by encounter palette updates.</summary>
    internal Span<Bgr555> MutableTargetPalette => _targetPalette;
}
