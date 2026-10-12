namespace SuperMetroid.Core.Game;

/// <summary>
/// Literal bank-$A6 function word stored in Hibashi variable A. The graphics part switches
/// between one inactive timer and one instruction-driven active eruption.
/// </summary>
public enum HibashiEnemyFunction : ushort
{
    /// <summary>$A6:902F, Function_Hibashi_Inactive: decrement the graphics owner's wait word and, on signed underflow, restart eruption art and enable the following invisible collision part.</summary>
    Inactive = 0x902f,
    /// <summary>$A6:9062, Function_Hibashi_Active: let instruction-driven art/hitbox commands run until the finish flag is set, then reload the population wait and return to inactive.</summary>
    Active = 0x9062,
}

/// <summary>
/// Debugger-facing projection of Hibashi's shared variables and population aliases. Every
/// shipped pillar uses two consecutive records of definition $E07F: part zero owns art and
/// timing, while nonzero part one is the invisible collision body manipulated by part zero.
/// </summary>
public sealed class HibashiEnemyState
{
    /// <summary>Physical graphics or collision-part slot carrying Hibashi's native variables.</summary>
    private readonly RoomEnemySlot _slot;

    /// <summary>Projects Hibashi's per-part variables from its occupied enemy slot.</summary>
    internal HibashiEnemyState(RoomEnemySlot slot) => _slot = slot;

    /// <summary>Graphics part's same-bank function word in common variable A, initialized to inactive; nonzero population parts do not dispatch this state machine.</summary>
    public HibashiEnemyFunction Function
    {
        get => (HibashiEnemyFunction)_slot.VariableA;
        internal set => _slot.VariableA = (ushort)value;
    }

    /// <summary>Raw countdown in common variable B, decremented once per inactive AI update; zero wraps to $FFFF and erupts immediately, while a nonnegative reset N waits N+1 updates.</summary>
    public ushort InactiveTimer
    {
        get => _slot.VariableB;
        internal set => _slot.VariableB = value;
    }

    /// <summary>Animation-to-AI handshake in common variable C: cleared on eruption start, set to one by $A6:8FD1 after disabling the hitbox, and tested as nonzero by active AI.</summary>
    public ushort FinishedActivityFlag
    {
        get => _slot.VariableC;
        internal set => _slot.VariableC = value;
    }

    /// <summary>Graphics owner's whole-pixel room Y captured at initialization; activity frames place the next part's hitbox relative to it and the finish instruction restores the art owner to it.</summary>
    public ushort SpawnYPosition
    {
        get => _slot.VariableD;
        internal set => _slot.VariableD = value;
    }

    /// <summary>Native extension word $0FB4 aliases population init0/parameter one.</summary>
    public ushort InactiveTimerResetValue => _slot.Parameter1;

    /// <summary>Native extension word $0FB6 aliases population init1/parameter two.</summary>
    public ushort Part => _slot.Parameter2;
}
