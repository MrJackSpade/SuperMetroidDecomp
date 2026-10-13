namespace SuperMetroid.Core.Game;

/// <summary>
/// The two indirect main-AI destinations used by Kago definition <c>$E7FF</c>. Keeping the
/// ROM pointers as enum values makes a debugger watch directly comparable with variable A.
/// </summary>
public enum KagoEnemyFunction : ushort
{
    /// <summary>Function_Kago_Nothing, $A8:AB7B: the first main-AI call only replaces variable A with its $AB81 return label; shell animation and shot-triggered effects remain independently owned.</summary>
    InstallNoOp = 0xab7b,
    /// <summary>Function_Kago_Nothing.return, $A8:AB81: persistent inert RTL target after the initial handoff; the instruction interpreter animates the stationary shell and its shot callback controls damage response and bug spawning.</summary>
    NoOp = 0xab81,
}

/// <summary>
/// Typed view of Kago's ordinary variables plus its bank-$7E:7808 hit counter. The latter
/// is genuinely outside the common 64-byte enemy record, so it remains actor-owned state
/// instead of being disguised as an unrelated generic slot word.
/// </summary>
public sealed class KagoEnemyState
{
    private readonly RoomEnemySlot _slot;

    internal KagoEnemyState(RoomEnemySlot slot) => _slot = slot;

    /// <summary>Common variable A ($7E:0FA8 plus physical slot offset), holding the native bank-$A8 indirect main-AI word; initialized to $AB7B and changed to $AB81 on the first AI update.</summary>
    public KagoEnemyFunction Function
    {
        get => (KagoEnemyFunction)_slot.VariableA;
        internal set => _slot.VariableA = (ushort)value;
    }

    /// <summary>Variable B: zero for the ten-frame loop, one for the three-frame loop.</summary>
    public bool UsesFastAnimation
    {
        get => _slot.VariableB != 0;
        internal set => _slot.VariableB = value ? (ushort)1 : (ushort)0;
    }

    /// <summary>
    /// Extra enemy word four. The ROM decrements first and dies only when the signed result
    /// is negative, so population value ten permits eleven accepted shots.
    /// </summary>
    public NativeCountdown HitCounter { get; internal set; }

    /// <summary>Variable F: set after Kago requests death animation variant four.</summary>
    public bool DeathAnimationStarted
    {
        get => _slot.VariableF != 0;
        internal set => _slot.VariableF = value ? (ushort)1 : (ushort)0;
    }

    /// <summary>Number of bank-$86 Kago bugs successfully allocated from this shell.</summary>
    public int SpawnedBugCount { get; internal set; }
}
