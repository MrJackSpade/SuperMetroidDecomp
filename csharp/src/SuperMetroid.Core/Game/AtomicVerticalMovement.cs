namespace SuperMetroid.Core.Game;

/// <summary>
/// Literal bank-$A8 vertical movement function stored in Atomic's common variable A.
/// The original symbol calls this the X movement function even though both targets update Y;
/// the debugger-facing name describes the behavior instead of perpetuating that label error.
/// </summary>
public enum AtomicVerticalMovement : ushort
{
    /// <summary>$A8:E405, Function_Atomic_MoveUp: add the stored negative whole/fraction speed to Y without terrain collision, selected when wrapped Samus-minus-enemy Y is signed-negative.</summary>
    MoveUp = 0xe405,
    /// <summary>$A8:E424, Function_Atomic_MoveDown: add the positive whole/fraction speed to Y without terrain collision, selected for signed-nonnegative Samus-minus-enemy Y, including equality.</summary>
    MoveDown = 0xe424,
}
