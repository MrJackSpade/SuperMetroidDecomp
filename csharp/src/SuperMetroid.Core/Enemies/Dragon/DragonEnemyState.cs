using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Fx;
namespace SuperMetroid.Core.Enemies.Dragon;

/// <summary>
/// Typed view of Dragon's overloaded ordinary and extended enemy words. The original actor
/// uses variable D first as a motion timer and then as its three-shot counter; both names are
/// intentionally retained as aliases so a debugger explains the current function's meaning.
/// </summary>
public sealed class DragonEnemyState
{
    /// <summary>Enemy slot whose ordinary variables hold Dragon's current function and counters.</summary>
    private readonly RoomEnemySlot _slot;
    /// <summary>Extended per-slot animation list requests.</summary>
    private readonly ushort[] _requestedInstructionLists;
    /// <summary>Extended per-slot list indices already installed in the interpreter.</summary>
    private readonly ushort[] _installedInstructionLists;
    /// <summary>Extended per-slot completion flags written by Dragon animation instructions.</summary>
    private readonly ushort[] _animationFinishedFlags;

    /// <summary>Binds the ordinary enemy slot to its three extended animation-word arrays.</summary>
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

    /// <summary>Where the typed animation (#641) resumes, in place of the native instruction pointer.</summary>
    internal AnimationProgramCursor<DragonProgram> Program { get; set; }

    /// <summary>Population parameter one: zero is a body; nonzero is its cosmetic wing.</summary>
    public bool IsWing => _slot.Parameter1 != 0;

    /// <summary>Variable A; bit 15 is the facing bit consumed by the fireball initializer.</summary>
    public ushort DirectionWord
    {
        get => _slot.VariableA;
        internal set => _slot.VariableA = value;
    }

    /// <summary>Variable D while waiting, rising, or sinking.</summary>
    public NativeCountdown FunctionTimer
    {
        get => NativeCountdown.FromRaw(_slot.VariableD);
        internal set => _slot.VariableD = value.RawValue;
    }

    /// <summary>Variable D while attacking; initialized to three before the first volley.</summary>
    public NativeCountdown AttackCounter
    {
        get => NativeCountdown.FromRaw(_slot.VariableD);
        internal set => _slot.VariableD = value.RawValue;
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
