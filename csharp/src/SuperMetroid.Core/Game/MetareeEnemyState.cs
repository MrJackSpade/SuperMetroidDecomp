using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>The literal bank-$A3 function pointer stored in a Metaree's variable B.</summary>
public enum MetareeEnemyFunction : ushort
{
    /// <summary>$A3:8987, Function_Metaree_Idling: activates within 72 horizontal pixels, without checking that Samus is below, and fixes the unsigned downward speed for the attack.</summary>
    Idling = 0x8987,
    /// <summary>$A3:89D4, Function_Metaree_PrepareToLaunchAttack: waits for the animation's attack-ready flag, then installs dive art and requests library-2 sound $5B.</summary>
    PreparingAttack = 0x89d4,
    /// <summary>$A3:89F3, Function_Metaree_LaunchedAttack: reloads the 21-update burrow timer, probes solid blocks below, and otherwise dives at fixed whole-pixel Y speed while steering two pixels toward Samus.</summary>
    LaunchedAttack = 0x89f3,
    /// <summary>$A3:8A5C, Function_Metaree_Burrowing: decrements the timer, emits four metal particles when it reaches eight, sinks one pixel per surviving update, and deletes the actor at zero.</summary>
    Burrowing = 0x8a5c,
}

/// <summary>
/// Typed view of Metaree's six private words at <c>$0FA8-$0FB2</c>. The wrapper deliberately
/// aliases the live enemy slot: debugger edits and native-width wrapping remain observable,
/// while the runtime no longer spreads anonymous <c>VariableA-F</c> accesses through its AI.
/// </summary>
public sealed class MetareeEnemyState
{
    /// <summary>Stores slot.</summary>
    private readonly RoomEnemySlot _slot;

    /// <summary>Initializes a new metaree enemy state instance.</summary>
    internal MetareeEnemyState(RoomEnemySlot slot) => _slot = slot;

    /// <summary>Reloaded to 21 during flight, then counted down while underground.</summary>
    public ushort BurrowTimer
    {
        get => _slot.VariableA;
        internal set => _slot.VariableA = value;
    }

    /// <summary>Direct bank-$A3 state-function pointer dispatched by main AI.</summary>
    public MetareeEnemyFunction Function
    {
        get => (MetareeEnemyFunction)_slot.VariableB;
        internal set => _slot.VariableB = (ushort)value;
    }

    /// <summary>Instruction-list index requested by AI.</summary>
    public SkreeMetareeAnimationPhase RequestedInstructionListIndex
    {
        get => (SkreeMetareeAnimationPhase)_slot.VariableC;
        internal set => _slot.VariableC = (ushort)value;
    }

    /// <summary>Instruction-list index currently installed in the common slot.</summary>
    public SkreeMetareeAnimationPhase InstalledInstructionListIndex
    {
        get => (SkreeMetareeAnimationPhase)_slot.VariableD;
        internal set => _slot.VariableD = (ushort)value;
    }

    /// <summary>Sets by instruction <c>$A3:8956</c> after the launch crouch completes.</summary>
    public bool AttackReady
    {
        get => _slot.VariableE != 0;
        internal set => _slot.VariableE = value ? (ushort)1 : (ushort)0;
    }

    /// <summary>
    /// Unsigned whole-pixel downward speed calculated once from Samus's vertical position.
    /// The unsigned type is important: the original game's above-Samus bug relies on wrap.
    /// </summary>
    public ushort YVelocity
    {
        get => _slot.VariableF;
        internal set => _slot.VariableF = value;
    }
}
