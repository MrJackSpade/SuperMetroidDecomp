using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>The literal bank-$A3 function pointer stored in a Skultera's variable A.</summary>
public enum SkulteraEnemyFunction : ushort
{
    /// <summary>$A3:9132, Function_Skultera_SwimmingLeft: applies signed 16.16 leftward displacement and whole-pixel sine-offset changes; either axis collision starts a right turn.</summary>
    SwimmingLeft = 0x9132,
    /// <summary>$A3:91AB, Function_Skultera_SwimmingRight: mirrors leftward swimming with the rightward speed; unobstructed movement advances the low-byte angle, and either axis collision starts a left turn.</summary>
    SwimmingRight = 0x91ab,
    /// <summary>$A3:9224, Function_Skultera_TurningRight: waits without swimming until animation sets the turn-finished flag, then clears it, negates the angle delta, and installs rightward swimming.</summary>
    TurningRight = 0x9224,
    /// <summary>$A3:9256, Function_Skultera_TurningLeft: waits for the animation's turn-finished flag, then clears it, negates the angle delta, and installs leftward swimming.</summary>
    TurningLeft = 0x9256,
}

/// <summary>
/// Typed view of Skultera's six common-slot words and five per-slot words in extra WRAM.
/// The upstream symbols spell the structure <c>Skulltera</c>, but the enemy header and the
/// game's visible name use <c>Skultera</c>; the public decompilation API follows the latter.
/// </summary>
public sealed class SkulteraEnemyState
{
    /// <summary>Stores slot.</summary>
    private readonly RoomEnemySlot _slot;
    /// <summary>Stores radii.</summary>
    private readonly ushort[] _radii;
    /// <summary>Stores turn finished flags.</summary>
    private readonly ushort[] _turnFinishedFlags;
    /// <summary>Stores angle deltas.</summary>
    private readonly ushort[] _angleDeltas;
    /// <summary>Stores previous yoffsets.</summary>
    private readonly ushort[] _previousYOffsets;
    /// <summary>Stores current yoffsets.</summary>
    private readonly ushort[] _currentYOffsets;

    /// <summary>Initializes a new skultera enemy state instance.</summary>
    internal SkulteraEnemyState(
        RoomEnemySlot slot,
        ushort[] radii,
        ushort[] turnFinishedFlags,
        ushort[] angleDeltas,
        ushort[] previousYOffsets,
        ushort[] currentYOffsets)
    {
        _slot = slot;
        _radii = radii;
        _turnFinishedFlags = turnFinishedFlags;
        _angleDeltas = angleDeltas;
        _previousYOffsets = previousYOffsets;
        _currentYOffsets = currentYOffsets;
    }

    /// <summary>Direct bank-$A3 state-function pointer dispatched by main AI.</summary>
    public SkulteraEnemyFunction Function
    {
        get => (SkulteraEnemyFunction)_slot.VariableA;
        internal set => _slot.VariableA = (ushort)value;
    }

    /// <summary>Fractional word of the positive/right signed 16.16 speed.</summary>
    public ushort RightSubvelocity
    {
        get => _slot.VariableB;
        internal set => _slot.VariableB = value;
    }

    /// <summary>Whole word of the positive/right signed 16.16 speed.</summary>
    public short RightVelocity
    {
        get => unchecked((short)_slot.VariableC);
        internal set => _slot.VariableC = unchecked((ushort)value);
    }

    /// <summary>Fractional word of the negative/left signed 16.16 speed.</summary>
    public ushort LeftSubvelocity
    {
        get => _slot.VariableD;
        internal set => _slot.VariableD = value;
    }

    /// <summary>Whole word of the negative/left signed 16.16 speed.</summary>
    public short LeftVelocity
    {
        get => unchecked((short)_slot.VariableE);
        internal set => _slot.VariableE = unchecked((ushort)value);
    }

    /// <summary>Low-byte sine-table angle; high-byte garbage is discarded after movement.</summary>
    public ushort Angle
    {
        get => _slot.VariableF;
        internal set => _slot.VariableF = value;
    }

    /// <summary>Low byte of population parameter two: vertical oscillation radius.</summary>
    public ushort Radius
    {
        get => _radii[_slot.SlotIndex];
        internal set => _radii[_slot.SlotIndex] = value;
    }

    /// <summary>Published by animation command <c>$A3:90AA</c> after a complete turn.</summary>
    public bool TurnFinished
    {
        get => _turnFinishedFlags[_slot.SlotIndex] != 0;
        internal set => _turnFinishedFlags[_slot.SlotIndex] = value ? (ushort)1 : (ushort)0;
    }

    /// <summary>
    /// Signed low-byte angle increment. Native code stores a word and negates the complete
    /// word at every turn, so the debugger-facing value deliberately remains a <c>short</c>.
    /// </summary>
    public short AngleDelta
    {
        get => unchecked((short)_angleDeltas[_slot.SlotIndex]);
        internal set => _angleDeltas[_slot.SlotIndex] = unchecked((ushort)value);
    }

    /// <summary>Sine result retained from the preceding successful movement attempt.</summary>
    public short PreviousYOffset
    {
        get => unchecked((short)_previousYOffsets[_slot.SlotIndex]);
        internal set => _previousYOffsets[_slot.SlotIndex] = unchecked((ushort)value);
    }

    /// <summary>Current sine result at extra WRAM <c>$7E:7808,x</c>.</summary>
    public short CurrentYOffset
    {
        get => unchecked((short)_currentYOffsets[_slot.SlotIndex]);
        internal set => _currentYOffsets[_slot.SlotIndex] = unchecked((ushort)value);
    }

    /// <summary>Complete signed 16.16 rightward displacement selected at initialization.</summary>
    public int RightDisplacement => (SnesSixteenSixteen.FromWhole(RightVelocity)) | RightSubvelocity;

    /// <summary>Complete signed 16.16 leftward displacement selected at initialization.</summary>
    public int LeftDisplacement => (SnesSixteenSixteen.FromWhole(LeftVelocity)) | LeftSubvelocity;
}
