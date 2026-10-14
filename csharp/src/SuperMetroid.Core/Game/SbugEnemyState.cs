namespace SuperMetroid.Core.Game;

/// <summary>
/// Typed debugger view of Sbug's common-slot words and its parallel WRAM extension arrays.
/// The names follow the behavior established by $A3:A14D-$A67C rather than the anonymous
/// var00/var20 labels required by the raw decompilation.
/// </summary>
public sealed class SbugEnemyState
{
    /// <summary>Live enemy slot backing Sbug's four common variables and population parameters.</summary>
    private readonly RoomEnemySlot _slot;

    /// <summary>Projects Sbug's existing slot into named AI and motion state.</summary>
    internal SbugEnemyState(RoomEnemySlot slot) => _slot = slot;

    /// <summary>Variable A; the long lifetime used only by behavior four.</summary>
    public NativeCountdown StraightMovementTimer
    {
        get => NativeCountdown.FromRaw(_slot.VariableA);
        internal set => _slot.VariableA = value.RawValue;
    }

    /// <summary>Variable B; the current same-bank AI dispatch pointer.</summary>
    public SbugEnemyFunction Function
    {
        get => (SbugEnemyFunction)_slot.VariableB;
        internal set => _slot.VariableB = (ushort)value;
    }

    /// <summary>Variable C; a standard-mathematics byte angle retained in a word.</summary>
    public ushort CustomAngle
    {
        get => _slot.VariableC;
        internal set => _slot.VariableC = value;
    }

    /// <summary>Variable F; the 32-frame segment/wait countdown.</summary>
    public NativeCountdown MovementTimer
    {
        get => NativeCountdown.FromRaw(_slot.VariableF);
        internal set => _slot.VariableF = value.RawValue;
    }

    /// <summary>Parameter one low byte, multiplied by the ROM trigonometry tables.</summary>
    public byte Speed => unchecked((byte)_slot.Parameter1);

    /// <summary>
    /// Parameter one high byte. Zero points right, $40 up, $80 left, and $C0 down.
    /// </summary>
    public byte InitialAngle => SnesLittleEndian.HighByte(_slot.Parameter1);

    /// <summary>Parameter two low byte; both-axis proximity needed for activation.</summary>
    public byte ActivationRadius => unchecked((byte)_slot.Parameter2);

    /// <summary>Parameter two high byte; the activation-table selector.</summary>
    public SbugActivationBehavior ActivationBehavior =>
        ClosedNativeWords.Decode<SbugActivationBehavior>(SnesLittleEndian.HighByte(_slot.Parameter2), "Sbug activation behavior");

    /// <summary>Spawn-computed unsigned 16.16 horizontal magnitude in pixels per AI update; the spawn angle supplies its sign during forward movement.</summary>
    public SbugVelocityWords ForwardXVelocity { get; internal set; }
    /// <summary>Spawn-computed unsigned 16.16 vertical magnitude in pixels per AI update; the spawn angle supplies its sign during forward movement.</summary>
    public SbugVelocityWords ForwardYVelocity { get; internal set; }
    /// <summary>Spawn-computed signed 16.16 horizontal displacement for angle <c>InitialAngle - $20</c>, used by the native left zigzag leg.</summary>
    public SbugVelocityWords LeftXVelocity { get; internal set; }
    /// <summary>Spawn-computed signed 16.16 vertical displacement for angle <c>InitialAngle - $20</c>, used by the native left zigzag leg.</summary>
    public SbugVelocityWords LeftYVelocity { get; internal set; }
    /// <summary>Spawn-computed signed 16.16 horizontal displacement for angle <c>InitialAngle + $20</c>, used by the native right zigzag leg.</summary>
    public SbugVelocityWords RightXVelocity { get; internal set; }
    /// <summary>Spawn-computed signed 16.16 vertical displacement for angle <c>InitialAngle + $20</c>, used by the native right zigzag leg.</summary>
    public SbugVelocityWords RightYVelocity { get; internal set; }
    /// <summary>Current signed 16.16 horizontal displacement in pixels per AI update, computed from the custom angle and independently word-negated on reversal.</summary>
    public SbugVelocityWords CustomXVelocity { get; internal set; }
    /// <summary>Current signed 16.16 vertical displacement in pixels per AI update, computed from the custom angle and independently word-negated on reversal.</summary>
    public SbugVelocityWords CustomYVelocity { get; internal set; }

    /// <summary>Even byte offsets into the eight-entry instruction-pointer table.</summary>
    public ushort ForwardInstructionIndex { get; internal set; }
    /// <summary>Spawn-computed even byte offset $00-$0E into <c>$A3:A111</c> for angle <c>InitialAngle - $20</c>; used by the left zigzag leg.</summary>
    public ushort LeftInstructionIndex { get; internal set; }
    /// <summary>Spawn-computed even byte offset $00-$0E into <c>$A3:A111</c> for angle <c>InitialAngle + $20</c>; used by the right zigzag leg.</summary>
    public ushort RightInstructionIndex { get; internal set; }
    /// <summary>Custom-direction byte offset into <c>$A3:A111</c>; normally even $00-$0E, but reversal preserves the native <c>(index + 4) &amp; $0007</c> mask.</summary>
    public ushort CustomInstructionIndex { get; internal set; }

    /// <summary>The list most recently requested by direction-selection code.</summary>
    public ushort RequestedInstructionList { get; internal set; }

    /// <summary>The list already installed in the common enemy instruction interpreter.</summary>
    public ushort InstalledInstructionList { get; internal set; }
}
