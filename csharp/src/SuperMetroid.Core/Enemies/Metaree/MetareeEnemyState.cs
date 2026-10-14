using SuperMetroid.Core.Enemies.Skree;

namespace SuperMetroid.Core.Enemies.Metaree;

/// <summary>
/// Typed view of Metaree's six private words at <c>$0FA8-$0FB2</c>. The wrapper deliberately
/// aliases the live enemy slot: debugger edits and native-width wrapping remain observable,
/// while the runtime no longer spreads anonymous <c>VariableA-F</c> accesses through its AI.
/// </summary>
public sealed class MetareeEnemyState
{
    /// <summary>Physical enemy slot backing Metaree's timer, function, animation, and dive speed.</summary>
    private readonly RoomEnemySlot _slot;

    /// <summary>Projects Metaree's six private words from its occupied enemy slot.</summary>
    internal MetareeEnemyState(RoomEnemySlot slot) => _slot = slot;

    /// <summary>Reloaded to 21 during flight, then counted down while underground.</summary>
    public NativeCountdown BurrowTimer
    {
        get => NativeCountdown.FromRaw(_slot.VariableA);
        internal set => _slot.VariableA = value.RawValue;
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

    /// <summary>Set by instruction <c>$A3:8956</c> after the launch crouch completes.</summary>
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
