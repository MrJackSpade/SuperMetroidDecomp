using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Indirect function pointers stored in Mama Turtle's ordinary enemy variable A. Keeping the
/// literal bank-$A2 offsets makes debugger watches directly comparable with the cartridge.
/// </summary>
public enum MamaTurtleAiFunction : ushort
{
    /// <summary>$A2:8DD8, links the four following Baby Turtle slots to their parent.</summary>
    Initial = 0x8dd8,
    /// <summary>$A2:8E09, inert state used after parent/child setup.</summary>
    Idle = 0x8e09,
    /// <summary>$A2:8E0A, waits asleep until the shared wake counter is cleared.</summary>
    Asleep = 0x8e0a,
    /// <summary>$A2:8EE0, moves Samus along the shell while Mama Turtle emerges.</summary>
    LeavingShell = 0x8ee0,
    /// <summary>$A2:8F3F, returns Mama Turtle and a carried Samus into the shell.</summary>
    EnteringShell = 0x8f3f,
    /// <summary>$A2:8F8D, rises from the shell toward the hovering height.</summary>
    RisingToHover = 0x8f8d,
    /// <summary>$A2:8FEB, hovers while updating shell collision and rider displacement.</summary>
    Hovering = 0x8feb,
    /// <summary>$A2:9083, accelerates upward toward the authored peak position.</summary>
    RisingToPeak = 0x9083,
    /// <summary>$A2:90CC, pauses at the peak before falling.</summary>
    HoveringAtPeak = 0x90cc,
    /// <summary>$A2:90E1, falls back toward the shell with a capped velocity.</summary>
    Falling = 0x90e1,
}

/// <summary>Indirect function pointers stored in each Baby Turtle's variable A.</summary>
public enum BabyTurtleAiFunction : ushort
{
    /// <summary>$A2:9142, crawls along the shell without carrying Samus.</summary>
    CrawlingNotCarryingSamus = 0x9142,
    /// <summary>$A2:916E, hides while remaining attached to a carried Samus.</summary>
    HidingCarryingSamus = 0x916e,
    /// <summary>$A2:9198, hides without carrying Samus.</summary>
    HidingNotCarryingSamus = 0x9198,
    /// <summary>$A2:91F8, spins while Samus cannot stop the child.</summary>
    SpinningUnstoppable = 0x91f8,
    /// <summary>$A2:9239, spins in the state that Samus can interrupt.</summary>
    SpinningStoppable = 0x9239,
    /// <summary>$A2:925E, crawls while carrying Samus along the shell contour.</summary>
    CrawlingCarryingSamus = 0x925e,
}

/// <summary>
/// Typed view of Mama Turtle's ordinary variables and bank-$7E extension. The six ordinary
/// words remain backed by the physical enemy slot; the extension has no ordinary-slot alias
/// in the host and therefore lives in this one-to-one state object.
/// </summary>
public sealed class MamaTurtleEnemyState
{
    private readonly RoomEnemySlot _slot;

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

    // Extension words behind Velocity: the horizontal fraction and the vertical 16.16 axis.
    private ushort _horizontalSubvelocity;
    private SnesSignedSixteenSixteen _verticalVelocity;

    /// <summary>Wake counter at variable F; each Baby touch or shot decrements it.</summary>
    public ushort AsleepFlag
    {
        get => _slot.VariableF;
        internal set => _slot.VariableF = value;
    }

    /// <summary>Gets the phase-specific countdown used by peak and transition states.</summary>
    public ushort FunctionTimer { get; internal set; }
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

/// <summary>Typed view of a Baby Turtle's ordinary variables and required extension words.</summary>
public sealed class BabyTurtleEnemyState
{
    private readonly RoomEnemySlot _slot;

    internal BabyTurtleEnemyState(RoomEnemySlot slot) => _slot = slot;

    /// <summary>Gets the current bank-$A2 Baby Turtle AI function pointer.</summary>
    public BabyTurtleAiFunction Function
    {
        get => (BabyTurtleAiFunction)_slot.VariableA;
        internal set => _slot.VariableA = (ushort)value;
    }

    /// <summary>Gets the parent Mama Turtle's native enemy-record byte index.</summary>
    public ushort ParentNativeIndex
    {
        get => _slot.VariableB;
        internal set => _slot.VariableB = value;
    }

    /// <summary>Gets the horizontal spawn position used by the child's shell path.</summary>
    public ushort SpawnXPosition
    {
        get => _slot.VariableC;
        internal set => _slot.VariableC = value;
    }

    /// <summary>Gets the upper boundary of the child's spawn hitbox.</summary>
    public ushort SpawnTopBoundary
    {
        get => _slot.VariableD;
        internal set => _slot.VariableD = value;
    }

    /// <summary>
    /// Signed whole-pixel velocity per update. The horizontal word is ordinary variable E; the
    /// vertical word is an extension word.
    /// </summary>
    public Offset16<RoomPixels> Velocity
    {
        get => new(unchecked((short)_slot.VariableE), _verticalVelocity);
        internal set
        {
            _slot.VariableE = unchecked((ushort)value.X);
            _verticalVelocity = value.Y;
        }
    }

    // Extension word behind Velocity's vertical axis.
    private short _verticalVelocity;

    /// <summary>Gets the phase-specific Baby Turtle countdown.</summary>
    public ushort FunctionTimer { get; internal set; }
    /// <summary>Gets the reaction countdown used while the child is not carrying Samus.</summary>
    public ushort NotCarryingSamusReactionTimer { get; internal set; }
}
