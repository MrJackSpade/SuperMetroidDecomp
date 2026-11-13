namespace SuperMetroid.Core.Enemies.Atomic;

/// <summary>
/// Atomic's horizontal movement behaviour; both targets update X.
/// Its values are arbitrary.
/// </summary>
public enum AtomicHorizontalMovement
{
    /// <summary>Add the stored negative whole/fraction speed to X without terrain collision, selected when wrapped Samus-minus-enemy X is signed-negative.</summary>
    MoveLeft = 1,
    /// <summary>Add the positive whole/fraction speed to X without terrain collision, selected for signed-nonnegative Samus-minus-enemy X, including equality.</summary>
    MoveRight,
}
