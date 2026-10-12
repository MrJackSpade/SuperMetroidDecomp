using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>The literal bank-$A3 function pointer stored in a Skree's variable B.</summary>
public enum SkreeEnemyFunction : ushort
{
    /// <summary><c>$A3:C6D5</c>: waits until Samus is within 48 horizontal pixels.</summary>
    Idling = 0xc6d5,

    /// <summary><c>$A3:C6F7</c>: waits for the wind-up instruction to authorize the dive.</summary>
    PreparingAttack = 0xc6f7,

    /// <summary><c>$A3:C716</c>: descends six pixels and steers one pixel toward Samus until floor collision.</summary>
    Diving = 0xc716,

    /// <summary><c>$A3:C77F</c>: sinks for the retained 21-update lifetime, emits debris, then deletes the enemy.</summary>
    Burrowing = 0xc77f,
}

/// <summary>
/// Typed debugger view of the five private words overlaid on a Skree's common enemy slot.
/// The wrapper names the native A-E ownership without copying state away from the slot.
/// </summary>
public sealed class SkreeEnemyState
{
    /// <summary>Stores slot.</summary>
    private readonly RoomEnemySlot _slot;

    /// <summary>Initializes a new skree enemy state instance.</summary>
    internal SkreeEnemyState(RoomEnemySlot slot) => _slot = slot;

    /// <summary>Gets or sets variable A, reloaded to 21 while diving and counted down while burrowing.</summary>
    public ushort BurrowTimer
    {
        get => _slot.VariableA;
        internal set => _slot.VariableA = value;
    }

    /// <summary>Gets or sets variable B, the literal bank-$A3 phase-function pointer.</summary>
    public SkreeEnemyFunction Function
    {
        get => (SkreeEnemyFunction)_slot.VariableB;
        internal set => _slot.VariableB = (ushort)value;
    }

    /// <summary>Gets or sets variable C, the animation phase requested by the current AI transition.</summary>
    public SkreeMetareeAnimationPhase RequestedInstructionIndex
    {
        get => (SkreeMetareeAnimationPhase)_slot.VariableC;
        internal set => _slot.VariableC = (ushort)value;
    }

    /// <summary>Gets or sets variable D, the animation phase whose instruction list is currently installed.</summary>
    public SkreeMetareeAnimationPhase InstalledInstructionIndex
    {
        get => (SkreeMetareeAnimationPhase)_slot.VariableD;
        internal set => _slot.VariableD = (ushort)value;
    }

    /// <summary>Gets or sets variable E, the wind-up instruction's one-shot authorization to begin diving.</summary>
    public bool AttackReady
    {
        get => _slot.VariableE != 0;
        internal set => _slot.VariableE = value ? (ushort)1 : (ushort)0;
    }
}
