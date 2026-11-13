namespace SuperMetroid.Core.Enemies.Atomic;

/// <summary>
/// Atomic's vertical movement behaviour; both targets update Y.
/// Its values are arbitrary.
/// </summary>
public enum AtomicVerticalMovement
{
    /// <summary>Add the stored negative whole/fraction speed to Y without terrain collision, selected when wrapped Samus-minus-enemy Y is signed-negative.</summary>
    MoveUp = 1,
    /// <summary>Add the positive whole/fraction speed to Y without terrain collision, selected for signed-nonnegative Samus-minus-enemy Y, including equality.</summary>
    MoveDown,
}
