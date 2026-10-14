namespace SuperMetroid.Core.Game;

/// <summary>
/// Typed debugger view of the five private words overlaid on a Skree's common enemy slot.
/// The wrapper names the native A-E ownership without copying state away from the slot.
/// </summary>
public sealed class SkreeEnemyState
{
    /// <summary>Native enemy slot containing Skree's overlaid variables A through E.</summary>
    private readonly RoomEnemySlot _slot;

    /// <summary>Binds the typed Skree view to its existing enemy slot.</summary>
    internal SkreeEnemyState(RoomEnemySlot slot) => _slot = slot;

    /// <summary>Gets or sets variable A, reloaded to 21 while diving and counted down while burrowing.</summary>
    public NativeCountdown BurrowTimer
    {
        get => NativeCountdown.FromRaw(_slot.VariableA);
        internal set => _slot.VariableA = value.RawValue;
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
