using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Fx;
namespace SuperMetroid.Core.Enemies.Skultera;

/// <summary>
/// Typed view of Skultera's six common-slot words and five per-slot words in extra WRAM.
/// The upstream symbols spell the structure <c>Skulltera</c>, but the enemy header and the
/// game's visible name use <c>Skultera</c>; the public decompilation API follows the latter.
/// </summary>
public sealed class SkulteraEnemyState
{
    /// <summary>Physical enemy record that owns the six common variable words.</summary>
    private readonly RoomEnemySlot _slot;
    /// <summary>Per-slot extra-WRAM vertical sine radius words.</summary>
    private readonly ushort[] _radii;
    /// <summary>Per-slot turn-completion latches set by the animation callback.</summary>
    private readonly ushort[] _turnFinishedFlags;
    /// <summary>Per-slot signed angle increments negated at each turn.</summary>
    private readonly ushort[] _angleDeltas;
    /// <summary>Prior successful movement's sine offsets, retained for delta calculation.</summary>
    private readonly ushort[] _previousYOffsets;
    /// <summary>Current sine offsets used by vertical movement and collision.</summary>
    private readonly ushort[] _currentYOffsets;

    /// <summary>Projects the common slot and shared extra-WRAM arrays at the slot's native index.</summary>
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

    /// <summary>Where the typed animation (#641) resumes, in place of the native instruction pointer.</summary>
    internal AnimationProgramCursor<SkulteraProgram> Program { get; set; }

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
