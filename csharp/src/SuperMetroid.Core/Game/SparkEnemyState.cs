namespace SuperMetroid.Core.Game;

/// <summary>
/// Debugger-facing projection of Spark's three private words. The actor uses common enemy
/// variable B for its indirect function, variable E for the population-supplied base time,
/// and variable F for the live countdown.
/// </summary>
public sealed class SparkEnemyState
{
    /// <summary>Physical enemy slot carrying Spark's function and countdown words.</summary>
    private readonly RoomEnemySlot _slot;

    /// <summary>Projects Spark's three private words from its occupied enemy slot.</summary>
    internal SparkEnemyState(RoomEnemySlot slot) => _slot = slot;

    /// <summary>Variable B holds the literal bank-$A8 indirect main-function address.</summary>
    public SparkEnemyFunction Function
    {
        get => (SparkEnemyFunction)_slot.VariableB;
        internal set => _slot.VariableB = (ushort)value;
    }

    /// <summary>Variable E supplies a base delay or a negative request for a randomized delay.</summary>
    public ushort BaseFunctionTime
    {
        get => _slot.VariableE;
        internal set => _slot.VariableE = value;
    }

    /// <summary>Variable F is the wrapping countdown decremented with native <c>DEC; BEQ</c> semantics.</summary>
    public ushort FunctionTimer
    {
        get => _slot.VariableF;
        internal set => _slot.VariableF = value;
    }
}
