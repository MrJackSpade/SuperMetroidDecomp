namespace SuperMetroid.Core.Enemies.Atomic;

/// <summary>
/// Atomic's vertical movement routine. The original symbol calls this the X movement function even
/// though both targets update Y; the name describes the behaviour instead of perpetuating that label error.
/// Its values are arbitrary.
/// </summary>
public enum AtomicVerticalMovement
{
    /// <summary>Add the stored negative whole/fraction speed to Y without terrain collision, selected when wrapped Samus-minus-enemy Y is signed-negative.</summary>
    MoveUp = 1,
    /// <summary>Add the positive whole/fraction speed to Y without terrain collision, selected for signed-nonnegative Samus-minus-enemy Y, including equality.</summary>
    MoveDown,
}
