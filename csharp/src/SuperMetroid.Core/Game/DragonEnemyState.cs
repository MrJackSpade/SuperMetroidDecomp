namespace SuperMetroid.Core.Game;

/// <summary>
/// Native bank-$A2 function pointer stored in Dragon variable F. Exposing the cartridge
/// addresses keeps debugger state comparable with WRAM while giving callers readable states.
/// </summary>
public enum DragonEnemyFunction : ushort
{
    /// <summary><c>$A2:E654</c>: waits below the lava, then selects facing and begins the 48-update rise.</summary>
    WaitToRise = 0xe654,

    /// <summary><c>$A2:E6AD</c>: moves the body and cosmetic wing upward one pixel per update.</summary>
    Rising = 0xe6ad,

    /// <summary><c>$A2:E6F1</c>: waits for each attack animation and emits the three-fireball volley.</summary>
    Attacking = 0xe6f1,

    /// <summary><c>$A2:E734</c>: holds the exposed idle pose for 96 updates before sinking.</summary>
    WaitToSink = 0xe734,

    /// <summary><c>$A2:E749</c>: moves the body and wing downward before restoring the hidden wait.</summary>
    Sinking = 0xe749,

    /// <summary><c>$A2:E781</c>: no-op function used by the cosmetic wing's separate physical enemy record.</summary>
    WingNoOp = 0xe781,
}

/// <summary>
/// Typed view of Dragon's overloaded ordinary and extended enemy words. The original actor
/// uses variable D first as a motion timer and then as its three-shot counter; both names are
/// intentionally retained as aliases so a debugger explains the current function's meaning.
/// </summary>
public sealed class DragonEnemyState
{
    private readonly RoomEnemySlot _slot;
    private readonly ushort[] _requestedInstructionLists;
    private readonly ushort[] _installedInstructionLists;
    private readonly ushort[] _animationFinishedFlags;

    internal DragonEnemyState(
        RoomEnemySlot slot,
        ushort[] requestedInstructionLists,
        ushort[] installedInstructionLists,
        ushort[] animationFinishedFlags)
    {
        _slot = slot;
        _requestedInstructionLists = requestedInstructionLists;
        _installedInstructionLists = installedInstructionLists;
        _animationFinishedFlags = animationFinishedFlags;
    }

    /// <summary>Population parameter one: zero is a body; nonzero is its cosmetic wing.</summary>
    public bool IsWing => _slot.Parameter1 != 0;

    /// <summary>Variable A; bit 15 is the facing bit consumed by the fireball initializer.</summary>
    public ushort DirectionWord
    {
        get => _slot.VariableA;
        internal set => _slot.VariableA = value;
    }

    /// <summary>Variable D while waiting, rising, or sinking.</summary>
    public ushort FunctionTimer
    {
        get => _slot.VariableD;
        internal set => _slot.VariableD = value;
    }

    /// <summary>Variable D while attacking; initialized to three before the first volley.</summary>
    public ushort AttackCounter
    {
        get => _slot.VariableD;
        internal set => _slot.VariableD = value;
    }

    /// <summary>Variable F, represented by its literal bank-$A2 function pointer.</summary>
    public DragonEnemyFunction Function
    {
        get => (DragonEnemyFunction)_slot.VariableF;
        internal set => _slot.VariableF = (ushort)value;
    }

    /// <summary>Extended enemy word $7E:7800,x: index into the six-entry list table.</summary>
    public DragonAnimationSelector RequestedInstructionListIndex
    {
        get => (DragonAnimationSelector)_requestedInstructionLists[_slot.SlotIndex];
        internal set => _requestedInstructionLists[_slot.SlotIndex] = (ushort)value;
    }

    /// <summary>Extended enemy word $7E:7802,x: list index already installed in the slot.</summary>
    public DragonAnimationSelector InstalledInstructionListIndex
    {
        get => (DragonAnimationSelector)_installedInstructionLists[_slot.SlotIndex];
        internal set => _installedInstructionLists[_slot.SlotIndex] = (ushort)value;
    }

    /// <summary>Extended enemy word $7E:7804,x, set by instruction opcode $A2:E5FB.</summary>
    public bool AnimationFinished
    {
        get => _animationFinishedFlags[_slot.SlotIndex] != 0;
        internal set => _animationFinishedFlags[_slot.SlotIndex] = value ? (ushort)1 : (ushort)0;
    }
}
