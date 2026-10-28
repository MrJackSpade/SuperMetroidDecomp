namespace SuperMetroid.Core.Enemies.Atomic;

/// <summary>
/// Atomic's horizontal movement routine. The original symbol calls this the Y movement function even
/// though both targets update X.
/// Its values are arbitrary; verification's native view names each member's cartridge routine (#1412).
/// </summary>
public enum AtomicHorizontalMovement
{
    /// <summary>$A8:E443, Function_Atomic_MoveLeft: add the stored negative whole/fraction speed to X without terrain collision, selected when wrapped Samus-minus-enemy X is signed-negative.</summary>
    MoveLeft = 1,
    /// <summary>$A8:E462, Function_Atomic_MoveRight: add the positive whole/fraction speed to X without terrain collision, selected for signed-nonnegative Samus-minus-enemy X, including equality.</summary>
    MoveRight,
}
