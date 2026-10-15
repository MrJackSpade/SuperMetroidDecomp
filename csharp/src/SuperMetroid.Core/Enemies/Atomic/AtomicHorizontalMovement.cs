namespace SuperMetroid.Core.Enemies.Atomic;

/// <summary>
/// Literal bank-$A8 horizontal movement function stored in Atomic's common variable B.
/// The original symbol calls this the Y movement function even though both targets update X.
/// </summary>
public enum AtomicHorizontalMovement : ushort
{
    /// <summary>$A8:E443, Function_Atomic_MoveLeft: add the stored negative whole/fraction speed to X without terrain collision, selected when wrapped Samus-minus-enemy X is signed-negative.</summary>
    MoveLeft = 0xe443,
    /// <summary>$A8:E462, Function_Atomic_MoveRight: add the positive whole/fraction speed to X without terrain collision, selected for signed-nonnegative Samus-minus-enemy X, including equality.</summary>
    MoveRight = 0xe462,
}
