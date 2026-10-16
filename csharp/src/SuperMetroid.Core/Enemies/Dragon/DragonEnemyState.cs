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
    /// <summary>The slot's Dragon extension words, kept for the room across occupants.</summary>
    private readonly DragonExtensionWords _words;

    /// <summary>Binds the ordinary enemy slot to its three extended animation-word arrays.</summary>
    internal DragonEnemyState(
        RoomEnemySlot slot, DragonExtensionWords words)
    {
        _slot = slot;
        _words = words;
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
        get => (DragonAnimationSelector)_words.RequestedInstructionList;
        internal set => _words.RequestedInstructionList = (ushort)value;
    }

    /// <summary>Extended enemy word $7E:7802,x: list index already installed in the slot.</summary>
    public DragonAnimationSelector InstalledInstructionListIndex
    {
        get => (DragonAnimationSelector)_words.InstalledInstructionList;
        internal set => _words.InstalledInstructionList = (ushort)value;
    }

    /// <summary>Extended enemy word $7E:7804,x, set by instruction opcode $A2:E5FB.</summary>
    public bool AnimationFinished
    {
        get => _words.AnimationFinishedFlag != 0;
        internal set => _words.AnimationFinishedFlag = value ? (ushort)1 : (ushort)0;
    }
}
