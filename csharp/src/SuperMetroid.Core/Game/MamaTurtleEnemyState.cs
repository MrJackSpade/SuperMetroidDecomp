namespace SuperMetroid.Core.Game;

/// <summary>
/// Typed view of Mama Turtle's ordinary variables and bank-$7E extension. The six ordinary
/// words remain backed by the physical enemy slot; the extension has no ordinary-slot alias
/// in the host and therefore lives in this one-to-one state object.
/// </summary>
public sealed class MamaTurtleEnemyState
{
    /// <summary>Physical parent slot whose common variable words remain debugger-visible.</summary>
    private readonly RoomEnemySlot _slot;

    /// <summary>Attaches Mama Turtle's typed state to its native parent record.</summary>
    internal MamaTurtleEnemyState(RoomEnemySlot slot) => _slot = slot;

    /// <summary>Gets the current bank-$A2 Mama Turtle AI function pointer.</summary>
    public MamaTurtleAiFunction Function
    {
        get => (MamaTurtleAiFunction)_slot.VariableA;
        internal set => _slot.VariableA = (ushort)value;
    }

    /// <summary>
    /// Signed 16.16 velocity. The horizontal whole word is ordinary variable E; the horizontal
    /// fraction and the vertical axis are extension words.
    /// </summary>
    public FixedVector<RoomPixels> Velocity
    {
        get => new(SnesSignedSixteenSixteen.FromParts(unchecked((short)_slot.VariableE), _horizontalSubvelocity), _verticalVelocity);
        internal set
        {
            _slot.VariableE = value.X.WholeWord;
            _horizontalSubvelocity = value.X.Fraction;
            _verticalVelocity = value.Y;
        }
    }

    /// <summary>Extra-WRAM fractional half of the parent's horizontal 16.16 velocity.</summary>
    private ushort _horizontalSubvelocity;
    /// <summary>Extra-WRAM vertical 16.16 velocity, paired with common-slot horizontal motion.</summary>
    private SnesSignedSixteenSixteen _verticalVelocity;

    /// <summary>Wake counter at variable F; each Baby touch or shot decrements it.</summary>
    public NativeCountdown AsleepFlag
    {
        get => NativeCountdown.FromRaw(_slot.VariableF);
        internal set => _slot.VariableF = value.RawValue;
    }

    /// <summary>Gets the phase-specific countdown used by peak and transition states.</summary>
    public NativeCountdown FunctionTimer { get; internal set; }
    /// <summary>Gets the fractional word of signed horizontal 16.16 acceleration.</summary>
    public ushort XSubAcceleration { get; internal set; }
    /// <summary>Gets the whole word of signed horizontal 16.16 acceleration.</summary>
    public ushort XAcceleration { get; internal set; }

    /// <summary>
    /// Extended word six. Every Baby clears this through its main-AI prologue; a child in
    /// carrying state republishes its Y radius. This odd last-writer-wins behavior is native.
    /// </summary>
    public ushort CarryingChildHeight { get; internal set; }
}
